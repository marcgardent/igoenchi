using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IGoEnchi
{
    public class IGSClient
    {
        public readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(30);
        public readonly TimeSpan KeepAlivePeriod = TimeSpan.FromSeconds(20);
        public readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(10);
        private DateTime connectionTime;
        private DateTime keepAliveRequestTime = DateTime.MinValue;
        private DateTime lastMessageTime;
        private readonly byte[] lineBuffer;
        private readonly StringBuilder lineBuilder;

        private Thread listenerThread;
        private readonly IGSMessageHandler[] messageHandlers;
        private NetworkStream networkStream;
        private TcpClient tcpClient;

        public IGSClient()
        {
            messageHandlers = new IGSMessageHandler[100];

            IGSMessageHandler handler = ErrorHandler;
            AddHandler(IGSMessages.Error, handler);

            ToggleSettings = IGSToggleSettings.Default;

            lineBuffer = new byte[1024];
            lineBuilder = new StringBuilder();
        }

        public IGSClient(IGSAccount account) : this()
        {
            Connect(account);
        }

        public bool Connected { get; private set; }

        public IGSAccount CurrentAccount { get; private set; }

        public IGSToggleSettings ToggleSettings { get; private set; }

        private bool HasData
        {
            get { return networkStream.DataAvailable; }
        }

        public event EventHandler Disconnected;

        public void EnsureNetworkConnection()
        {
            try
            {
                var request = WebRequest.Create("http://www.google.com") as HttpWebRequest;
                request.Method = "HEAD";
                request.GetResponse().Close();
            }
            catch (WebException)
            {
            }
        }

        public bool Connect(IGSAccount account)
        {
            try
            {
                EnsureNetworkConnection();
                tcpClient = new TcpClient(account.Server, account.Port);
            }
            catch (SocketException)
            {
                MessageBox.Show("Couldn't connect to " + account.Server + ":" + account.Port,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                return false;
            }

            networkStream = tcpClient.GetStream();
            if (networkStream == null)
            {
                MessageBox.Show("Couldn't connect to " + account.Server + ":" + account.Port,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                return false;
            }
            Connected = true;
            var guest = false;
            connectionTime = DateTime.Now;

            ReadUntil("Login:");
            WriteLine(account.Name);

            if (ReadUntil("1 1", "Password:", "#>") != "#>")
            {
                WriteLine(account.Password);

                var line = ReadUntil("39 ", "1 5", "#>", "1 0", "Login:");
                if (line == "1 0" || line == "Login:")
                {
                    MessageBox.Show("Wrong password",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                    networkStream.Close();
                    Connected = false;

                    account.Password = "";
                    account.PasswordSpecified = false;

                    return false;
                }
            }
            else
            {
                MessageBox.Show("Logged as guest, some commands will be disabled",
                    "Note",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button1);
                guest = true;
            }
            if (!Connected)
            {
                MessageBox.Show("Couldn't connect to " + account.Server + ":" + account.Port,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                return false;
            }

            listenerThread = new Thread(Listen);
            listenerThread.Priority = ThreadPriority.BelowNormal;
            listenerThread.IsBackground = true;
            listenerThread.Start();

            WriteLine("toggle client on");
            WriteLine("toggle quiet on");
            WriteLine("toggle newundo on");
            WriteLine("toggle nmatch on");
            WriteLine("toggle seek on");
            WriteLine("toggle review on");
            if (!guest)
            {
                WriteLine("nmatchrange BWN 0-9 2-19 0-900 0-3600 0-25 0 0 0-0");
                WriteLine("friend start");
            }

            CurrentAccount = account;

            return true;
        }

        public void ApplyToggleSettings(IGSToggleSettings settings)
        {
            ToggleSettings = settings;

            if (Connected)
            {
                WriteLine("toggle open " + Convert.ToInt32(ToggleSettings.Open));
                WriteLine("toggle looking " + Convert.ToInt32(ToggleSettings.Looking));
                WriteLine("toggle kibitz " + Convert.ToInt32(ToggleSettings.Kibitz));
            }
        }

        private void Listen()
        {
            try
            {
                var lastMessageType = 0;
                var messageType = 0;
                var list = new List<string>();
                var infoList = new List<string>();
                var bytesRead = 0;
                var buffer = new byte[1024];
                var line = new StringBuilder(1024);
                lastMessageTime = DateTime.Now;

                while (Connected)
                {
                    bytesRead =
                        HasData ? networkStream.Read(buffer, 0, 1024) : 0;
                    if (bytesRead == 0)
                    {
                        var now = DateTime.Now;
                        if (keepAliveRequestTime != DateTime.MinValue)
                        {
                            if (now - keepAliveRequestTime > KeepAliveTimeout)
                            {
                                keepAliveRequestTime = DateTime.MinValue;
                                throw new IOException();
                            }
                        }
                        else
                        {
                            if (now - lastMessageTime > KeepAlivePeriod)
                            {
                                WriteLine("ayt");
                                keepAliveRequestTime = now;
                            }
                        }
                    }
                    else
                    {
                        keepAliveRequestTime = DateTime.MinValue;
                        lastMessageTime = DateTime.Now;
                        var cursor = 0;
                        while (cursor < bytesRead)
                        {
                            while ((cursor < bytesRead) &&
                                   ((char) buffer[cursor] != '\n'))
                            {
                                if ((char) buffer[cursor] != '\r')
                                {
                                    line.Append((char) buffer[cursor]);
                                }
                                cursor++;
                            }

                            if ((cursor < bytesRead) &&
                                (line.Length > 1))
                            {
                                var numberChar = line[0];
                                var numberLength = 0;
                                if (char.IsDigit(numberChar))
                                {
                                    numberLength = 1;
                                    messageType = numberChar - '0';
                                    numberChar = line[1];
                                    if (char.IsDigit(numberChar))
                                    {
                                        numberLength = 2;
                                        messageType = messageType*10 + numberChar - '0';
                                    }

                                    if ((messageType != lastMessageType) &&
                                        (messageType != IGSMessages.Info))
                                    {
                                        if (messageHandlers[lastMessageType] != null)
                                        {
                                            messageHandlers[lastMessageType](list);
                                        }
                                        list.Clear();
                                    }

                                    if (messageType != IGSMessages.Info)
                                    {
                                        list.Add(line.Remove(0, numberLength + 1).ToString());
                                        lastMessageType = messageType;

                                        if (infoList.Count > 0)
                                        {
                                            if (messageHandlers[IGSMessages.Info] != null)
                                            {
                                                messageHandlers[IGSMessages.Info](infoList);
                                            }
                                            infoList.Clear();
                                        }
                                    }
                                    else
                                    {
                                        messageType = lastMessageType;
                                        var message = line.Remove(0, numberLength + 1).ToString();
                                        if (message != "yes")
                                        {
                                            infoList.Add(message);
                                        }
                                    }
                                }
                                else
                                {
                                    list.Add(line.ToString());
                                }
                                line.Remove(0, line.Length);
                            }
                            cursor++;
                        }
                    }
                }
            }
            catch (IGSParseException exception)
            {
                MessageBox.Show("Error while parsing server data: " + exception.Message,
                    "Parse error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
            }
            catch (IOException)
            {
                MessageBox.Show("Disconnected from server",
                    "Note",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button1);
            }
            Connected = false;
            OnDisconnected(EventArgs.Empty);
            networkStream.Close();
            tcpClient.Close();
        }

        public void AddHandler(int messageType, IGSMessageHandler messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }
            if ((messageType < messageHandlers.Length) &&
                (messageType >= 0))
            {
                messageHandlers[messageType] += messageHandler;
            }
        }

        private void ErrorHandler(List<string> lines)
        {
            foreach (var line in lines)
            {
                Console.WriteLine("Error: " + line);
            }
        }

        private string ReadUntil(params string[] strings)
        {
            lineBuilder.Remove(0, lineBuilder.Length);
            while (true)
            {
                if (HasData)
                {
                    var bytes = networkStream.Read(lineBuffer, 0, lineBuffer.Length);
                    var cursor = 0;
                    var codeStep = 0;
                    while (cursor < bytes)
                    {
                        while (cursor < bytes && lineBuffer[cursor] != 10)
                        {
                            var next = lineBuffer[cursor];
                            switch (codeStep)
                            {
                                case 0:
                                    codeStep = next == 255 ? 1 : 0;
                                    break;
                                case 1:
                                    codeStep = 2;
                                    break;
                                case 2:
                                    codeStep = 3;
                                    break;
                                case 3:
                                    codeStep = next == 255 ? 1 : 0;
                                    break;
                            }
                            if (codeStep == 0 && next != 13)
                            {
                                lineBuilder.Append((char) next);
                            }
                            cursor += 1;
                        }

                        var line = lineBuilder.ToString();
                        for (var i = 0; i < strings.Length; i++)
                        {
                            if (line.StartsWith(strings[i]))
                            {
                                return strings[i];
                            }
                        }

                        if (cursor < bytes)
                        {
                            lineBuilder.Remove(0, lineBuilder.Length);
                            cursor += 1;
                        }
                    }
                }
                else
                {
                    if (DateTime.Now - connectionTime > ConnectionTimeout)
                    {
                        Disconnect();
                        return "";
                    }
                }
            }
        }

        public string WriteLine(string line)
        {
            var bytes = 0;
            var data = Network.ASCIIBytes(line, out bytes);
            try
            {
                networkStream.Write(data, 0, bytes);
            }
            catch (Exception)
            {
                MessageBox.Show("Not connected to server",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                return null;
            }
            return line;
        }

        public void Disconnect()
        {
            Connected = false;
            WriteLine("quit");
        }

        public void OnDisconnected(EventArgs args)
        {
            if (Disconnected != null)
            {
                Disconnected(this, args);
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
 
namespace IGoEnchi
{		
	public class IGSAccount
	{		
		[XmlElement("server")]
		public string Server {get; set;}		
		[XmlElement("port")]
		public Int32 Port {get; set;}
		[XmlElement("name")]
		public string Name {get; set;}		
		[XmlElement("password")]
		public string Password {get; set;}
		[XmlIgnore]
		public bool PasswordSpecified {get; set;}
		
		public IGSAccount(): this("igs.joyjoy.net", 6969, "guest", "", false){}
		
		public IGSAccount(string server, Int32 port, string name, string password, bool savePassword)
		{
			Server = server;
			Port = port;
			Name = name;
			Password = password;
			PasswordSpecified = savePassword;
		}
		
		public static IGSAccount DefaultAccount
		{
			get {return new IGSAccount();}
		}
	}
	
	public class IGSToggleSettings
	{
		public bool Open {get; private set;}
		public bool Looking {get; private set;}
		public bool Kibitz {get; private set;}
		
		public IGSToggleSettings(bool open, bool looking, bool kibitz)
		{
			Open = open;
			Looking = looking;
			Kibitz = kibitz;
		}
		
		public static IGSToggleSettings Default
		{
			get {return new IGSToggleSettings(true, false, true);}
		}
	}
	
	public class IGSParseException: Exception
	{
		public IGSParseException(): base() {}
		
		public IGSParseException(string message): base(message)	{}
	}
	
	public delegate void IGSMessageHandler(List<string> lines);

	public struct IGSMessages
	{
		public static readonly int Error = 5;
		public static readonly int System = 1;
		public static readonly int Notice = 21;
		public static readonly int PlayersList = 42;
		public static readonly int File = 8;
		public static readonly int Info = 9;
		public static readonly int GamesList = 7;
		public static readonly int GameMove = 15;
		public static readonly int Chat = 24;
		public static readonly int Kibitz = 11;
		public static readonly int Say = 19;
		public static readonly int SaySource = 51;
		public static readonly int Adjourn = 48;
		public static readonly int Score = 20;
		public static readonly int Undo = 28;
		public static readonly int GameStatus = 22;
		public static readonly int StoneRemoval = 49;
		public static readonly int StoredGames = 18;
		public static readonly int SeekInfo = 63;
		public static readonly int Friend = 57;
	}
	
	public class IGSClient	
	{
		public readonly TimeSpan KeepAlivePeriod = TimeSpan.FromSeconds(20);
		public readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(10);
		public readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(30);
		
		private Thread listenerThread;
		private bool listening;
		private IGSMessageHandler[] messageHandlers;
		private TcpClient tcpClient;
		private NetworkStream networkStream;
		private IGSAccount account;
		private IGSToggleSettings toggleSettings;
		private DateTime connectionTime;
		private DateTime keepAliveRequestTime = DateTime.MinValue;
		private DateTime lastMessageTime;
		private byte[] lineBuffer;
		private StringBuilder lineBuilder;
		
		public event EventHandler Disconnected;
		
		public bool Connected
		{
			get {return listening;}
		}
		
		public IGSClient()
		{			
			messageHandlers = new IGSMessageHandler[100];			
			
			IGSMessageHandler handler = new IGSMessageHandler(this.ErrorHandler);
			AddHandler(IGSMessages.Error, handler);						
			
			toggleSettings = IGSToggleSettings.Default;
			
			lineBuffer = new byte[1024];
			lineBuilder = new StringBuilder();
		}				
		
		public IGSClient(IGSAccount account): this()
		{
			Connect(account);
		}

		public IGSAccount CurrentAccount
		{
			get {return account;}
		}
		
		public IGSToggleSettings ToggleSettings
		{
			get {return toggleSettings;}
		}
		
		public void EnsureNetworkConnection()
		{
			try
			{
				var request = WebRequest.Create("http://www.google.com") as HttpWebRequest;
				request.Method = "HEAD";
				request.GetResponse().Close();				
			}
			catch (WebException) {} 
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
			listening = true;
			var guest = false;
			connectionTime = DateTime.Now;
			
			ReadUntil(new [] {"Login:"});
			WriteLine(account.Name);
			
			if (ReadUntil(new [] {"1 1", "Password:", "#>"}) != "#>")
			{
				WriteLine(account.Password);								
				
				var line = ReadUntil(new [] {"39 ", "1 5", "#>", "1 0", "Login:"});
				if (line == "1 0" || line == "Login:")
				{
					MessageBox.Show("Wrong password",
				                    "Error",
				                    MessageBoxButtons.OK,
				                    MessageBoxIcon.Exclamation,
				                    MessageBoxDefaultButton.Button1);
					networkStream.Close();
					listening = false;
					
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
			if (!listening)
			{
				MessageBox.Show("Couldn't connect to " + account.Server + ":" + account.Port,
				                "Error",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation,
				                MessageBoxDefaultButton.Button1);
				return false;
			}
			
			listenerThread = new Thread(new ThreadStart(Listen));			
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
			
			this.account = account;
			
			return true;
		}
		
		public void ApplyToggleSettings(IGSToggleSettings settings)
		{
			toggleSettings = settings;
			
			if (Connected)
			{
				WriteLine("toggle open " + Convert.ToInt32(toggleSettings.Open));				
				WriteLine("toggle looking " + Convert.ToInt32(toggleSettings.Looking));
				WriteLine("toggle kibitz " + Convert.ToInt32(toggleSettings.Kibitz));
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
				
				while (listening)
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
								if (Char.IsDigit(numberChar))
								{
									numberLength = 1;
									messageType = numberChar - '0';
									numberChar = line[1];
									if (Char.IsDigit(numberChar))
									{			
										numberLength = 2;
										messageType = messageType * 10 + numberChar - '0';									
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
			listening = false;			
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
								case 0: codeStep = next == 255 ? 1 : 0;
										break;
								case 1: codeStep = 2;
										break;
								case 2: codeStep = 3;
										break;
								case 3: codeStep = next == 255 ? 1 : 0;
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
		
		private bool HasData
		{
			get {return (networkStream.DataAvailable);}
		}
		
		public void Disconnect()
		{				
			listening = false;		
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






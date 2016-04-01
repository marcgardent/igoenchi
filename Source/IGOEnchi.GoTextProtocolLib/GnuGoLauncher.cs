using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace IGoEnchi
{
    public class GnuGoLauncher : Link
    {
        private static readonly int Port = 5000;

        private static readonly uint TH32CS_SNAPPROCESS = 0x00000002;
        private byte[] buffer;
        private readonly TcpClient client;
        private int dataSize;
        private int dataStart;

        private readonly Process gnugo;

        private readonly GTP gtp;
        private readonly NetworkStream stream;

        private GnuGoLauncher(GnuGoSettings settings, string sgfFile)
        {
            var path = WinCEUtils.PathTo("gnugoce.exe");

            if (!File.Exists(path))
            {
                throw new GnuGoException(GnuGoError.ExecutableNotFound);
            }

            StopGnuGo();

            var info = new ProcessStartInfo(path, "--mode gtp --quiet --port " + Port)
            {
                UseShellExecute = false,
                WorkingDirectory = WinCEUtils.GetCurrentDirectory()
            };

            gnugo = Process.Start(info);
            if (gnugo.HasExited)
            {
                throw new GnuGoException(GnuGoError.CouldNotStart);
            }

            client = new TcpClient("localhost", Port);
            if (!client.Client.Connected)
            {
                throw new GnuGoException(GnuGoError.CouldNotConnect);
            }

            stream = client.GetStream();
            buffer = new byte[4096];

            gtp = new GTP(this,
                settings.BoardSize,
                settings.Handicap,
                settings.Komi,
                settings.Level,
                sgfFile);
        }

        public static GTP Run(GnuGoSettings settings, string sgfFile)
        {
            var launcher = new GnuGoLauncher(settings, sgfFile);
            return launcher.gtp;
        }

        private void StopGnuGo()
        {
            try
            {
                var snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
                if (snapshot != IntPtr.Zero)
                {
                    try
                    {
                        var size = 564;
                        var data = new byte[size];
                        BitConverter.GetBytes(size).CopyTo(data, 0);

                        var result = Process32First(snapshot, data);
                        while (result == 1)
                        {
                            var id = BitConverter.ToInt32(data, 8);
                            var name = Encoding.Unicode.GetString(data, 36, 260);
                            name = name.Substring(0, name.IndexOf('\0')).Trim().ToLower();
                            if (name == "gnugoce.exe")
                            {
                                Process.GetProcessById(id).Kill();
                                break;
                            }
                            result = Process32Next(snapshot, data);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                CloseToolhelp32Snapshot(snapshot);
            }
            catch (Exception)
            {
            }
        }

        [DllImport("toolhelp.dll")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

        [DllImport("toolhelp.dll")]
        public static extern int CloseToolhelp32Snapshot(IntPtr snapshot);

        [DllImport("toolhelp.dll")]
        public static extern int Process32First(IntPtr snapshot, byte[] data);

        [DllImport("toolhelp.dll")]
        public static extern int Process32Next(IntPtr snapshot, byte[] data);

        public override void Close()
        {
            stream.Close();
        }

        public override void SendCommand(string command)
        {
            var bytes = 0;
            var data = Network.ASCIIBytes(command, out bytes);
            stream.Write(data, 0, bytes);
        }

        public override string TryReceiveCommand()
        {
            var bytes = dataStart + dataSize;
            if (bytes < buffer.Length && stream.DataAvailable)
            {
                dataSize += stream.Read(buffer, bytes, buffer.Length - bytes);
            }

            if (dataSize > 0)
            {
                var index = 0;
                while (index < dataSize &&
                       buffer[dataStart + index] != '\r' &&
                       buffer[dataStart + index] != '\n')
                {
                    index += 1;
                }

                if (index == dataSize)
                {
                    if (dataStart + index == buffer.Length)
                    {
                        var newBuffer = new byte[buffer.Length];
                        buffer.CopyTo(newBuffer, dataStart);
                        buffer = newBuffer;
                        dataStart = 0;
                    }
                    return "";
                }
                var line = Encoding.ASCII.GetString(buffer, dataStart, index);
                dataStart += line.Length + 1;
                dataSize -= line.Length + 1;
                return line;
            }
            return "";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class TcpServer
    {
        private const int BufferSize = 1024;
        private TcpListener _listener;
        private Thread _serverThread;
        private int _headerCode = 1;

        public int Port { get; set; } = 65432;

        public TcpServer(int headerCode)
        {
            _headerCode = headerCode;
        }

        public void Start()
        {
            _serverThread = new Thread(StartListening);
            _serverThread.Start();
        }
        public void Stop()
        {
            _listener?.Stop();
            _serverThread?.Join();
            _serverThread = null;
        }

        private void StartListening()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                Console.WriteLine($"Server started listening on port {Port}");

                while (true)
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
                    HandleClient(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Error {e.Message}");
            }
            finally
            {
                _listener?.Stop();
                Console.WriteLine("Server Stopped");
            }
        }


        private async void HandleClient(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    byte[] headerData = new byte[new Header().GetLength()];
                    int bytesRead = await stream.ReadAsync(headerData, 0, headerData.Length);
                    if (bytesRead != headerData.Length)
                    {
                        Console.WriteLine($"Client header invalid");
                        return;
                    }
                    Header header = new Header();
                    header.code = headerData[0];
                    header.length = BitConverter.ToInt32(headerData, 1);
                    byte[] dataBuffer = new byte[header.length];
                    bytesRead = await stream.ReadAsync(dataBuffer, 0, dataBuffer.Length);
                    if (bytesRead != header.length)
                    {
                        Console.WriteLine($"Client Message invalid");
                        return;
                    }
                    string receivedMessage = Encoding.ASCII.GetString(dataBuffer, 0, bytesRead);
                    Console.WriteLine($"Received message from client: {receivedMessage}");
                    // Send data back to the client
                    byte[] responseData = Encoding.ASCII.GetBytes("This is the message from server");
                    Header responseHeader = new Header
                    {
                        code = (byte)_headerCode,
                        length = responseData.Length
                    };
                    await stream.WriteAsync(responseHeader.GetBytes(), 0, responseHeader.GetLength());
                    await stream.WriteAsync(responseData, 0, responseData.Length);


                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error handling client: {e.Message}");
                }
                finally
                {
                    client.Close();
                    Console.WriteLine($"Client disconnected.");
                }
            }
        }
    }
}

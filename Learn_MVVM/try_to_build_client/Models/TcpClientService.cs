// Models/TcpClientService.cs
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using try_to_build_client.Helpers;

namespace try_to_build_client.Models
{
    public class TcpClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly Encoding _encoding = Encoding.UTF8;
        public event Action<string> OnDataReceived;
        public event Action<string> OnConnectionError;
        public async Task ConnectAsync(string ipAddress, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ipAddress, port);
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Connection error: {ex.Message}");
            }
        }
        public async Task<bool> SendDataAsync(ClientMessage message, int headerCode)
        {
            if (_client?.Connected != true || _stream == null)
            {
                OnConnectionError?.Invoke("Not connected to server!");
                return false;
            }
            try
            {
                string json = JsonConvert.SerializeObject(message);
                byte[] data = string.IsNullOrEmpty(json) ? Array.Empty<byte>() : Encoding.ASCII.GetBytes(json);
                Header header = new Header
                {
                    code = (byte)headerCode,
                    length = data.Length
                };

                await _stream.WriteAsync(header.GetBytes(), 0, header.GetLength());
                await _stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (IOException ex)
            {
                OnConnectionError?.Invoke($"IO error while sending data: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Error while sending data: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        public async Task<ReceiveResult> StartReceivingAsync()
        {
            if (_client?.Connected != true || _stream == null)
            {
                OnConnectionError?.Invoke("Not connected to server!");
                return null;
            }

            try
            {
                Header header = new Header();
                byte[] headerData = new byte[header.GetLength()];
                if (await _stream.ReadAsync(headerData, 0, headerData.Length) != headerData.Length)
                {
                    Console.Error.WriteLine("Header invalid");
                    return new ReceiveResult { HeaderCode = -1, ServerMessage = null };
                }

                header.code = headerData[0];
                int headerCode = header.code;
                header.length = BitConverter.ToInt32(headerData, 1);

                // if no message sent, just return the header code
                if (header.length == 0)
                    return new ReceiveResult {HeaderCode = headerCode, ServerMessage = null };
                // if there's message, pare message
                byte[] messageData = new byte[header.length];

                int count;
                count = await _stream.ReadAsync(messageData, 0, messageData.Length);
                // The Encoding.ASCII.GetBytes method expects a non-null string as input. If message is null ,
                // it throws an ArgumentNullException, that's why we check (header.length == 0) first
                string json = Encoding.ASCII.GetString(messageData, 0, count);
                ServerMessage serverMessage = JsonConvert.DeserializeObject<ServerMessage>(json);

                return new ReceiveResult { HeaderCode = headerCode, ServerMessage = serverMessage };
            }
            catch (IOException ex)
            {
                OnConnectionError?.Invoke($"IO Error while receiving data: {ex.Message}");
                Disconnect();
                return null;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Receiving data canceled: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Error while receiving data: {ex.Message}");
                Disconnect();
                return null;
            }
        }

        private void Disconnect()
        {
            if (_client?.Connected == true)
            {
                _stream?.Close();
                _client?.Close();
            }
        }
    }
}
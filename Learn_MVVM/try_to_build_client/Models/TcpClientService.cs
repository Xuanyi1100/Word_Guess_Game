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
        public event Action<string> OnConnectionError;
        public async Task ConnectAsync(string ipAddress, int port)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(5000);
            CancellationToken token = tokenSource.Token;
            try
            {
                _client = new TcpClient();
                _client.SendTimeout = 5000;
                await _client.ConnectAsync(ipAddress, port);
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Connection error: {ex.Message}");
                tokenSource?.Cancel();
            }
            finally
            {
                tokenSource?.Dispose();
            }
        }
        public async Task<bool> SendDataAsync(ClientMessage message, int headerCode)
        {

            if (_client?.Connected != true || _stream == null)
            {
                OnConnectionError?.Invoke("Not connected to server!");
                return false;
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource(5000);
            CancellationToken token = tokenSource.Token;
            try
            {
                string json = JsonConvert.SerializeObject(message);
                byte[] data = string.IsNullOrEmpty(json) ? Array.Empty<byte>() : Encoding.ASCII.GetBytes(json);
                Header header = new Header
                {
                    code = (byte)headerCode,
                    length = data.Length
                };

                await _stream.WriteAsync(header.GetBytes(), 0, header.GetLength(), token);
                await _stream.WriteAsync(data, 0, data.Length, token);
                return true;
            }
            catch (IOException ex)
            {
                OnConnectionError?.Invoke($"IO error while sending data: {ex.Message}");
                Disconnect();
                tokenSource?.Cancel();
                return false;
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Error while sending data: {ex.Message}");
                Disconnect();
                tokenSource?.Cancel();
                return false;
            }
            finally
            {
                tokenSource?.Dispose();
            }
        }

        // "stateless" communication: After receive the message,disconnect immediately.
        public async Task<ReceiveResult> StartReceivingAsync()
        {
            if (_client?.Connected != true || _stream == null)
            {
                OnConnectionError?.Invoke("Not connected to server!");
                return null;
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource(5000);
            CancellationToken token = tokenSource.Token;
            try
            {
                Header header = new Header();
                byte[] headerData = new byte[header.GetLength()];

                // using CancellationToken  ensures the operation stops if it takes longer than 5 seconds
                if (await _stream.ReadAsync(headerData, 0, headerData.Length, token) != headerData.Length)
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
                count = await _stream.ReadAsync(messageData, 0, messageData.Length, token);
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
                tokenSource?.Cancel();
                return null;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Receiving data canceled: {ex.Message}");
                tokenSource?.Cancel();
                return null;
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Error while receiving data: {ex.Message}");
                Disconnect();
                tokenSource?.Cancel();
                return null;
            }
            finally
            {
                tokenSource?.Dispose();
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
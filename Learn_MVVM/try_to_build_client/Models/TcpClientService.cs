// Models/TcpClientService.cs
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
        private SendAndReceive _sendAndReceive;
        public event Action<string> OnDataReceived;
        public event Action<string> OnConnectionError;
        public async Task ConnectAsync(string ipAddress, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ipAddress, port);
                _stream = _client.GetStream();
                _sendAndReceive = new SendAndReceive(_stream);
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Connection error: {ex.Message}");
            }
        }
        public async Task SendDataAsync(string message, int headerCode)
        {
            if (_client?.Connected != true || _stream == null)
            {
                OnConnectionError?.Invoke("Not connected to server!");
                return;
            }
            try
            {
                await _sendAndReceive.SendAsync(message, headerCode);
            }
            catch (IOException ex)
            {
                OnConnectionError?.Invoke($"IO error while sending data: {ex.Message}");
                Disconnect();
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Error while sending data: {ex.Message}");
                Disconnect();
            }
        }

        public async Task<ReceiveResult> StartReceivingAsync()
        {
            try
            {
                return await _sendAndReceive.ReceiveAsync();
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
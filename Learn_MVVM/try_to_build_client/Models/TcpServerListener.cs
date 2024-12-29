// Models/TcpServerListener.cs
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using try_to_build_client.Helpers;

public class TcpServerListener
{
    private TcpListener _listener;
    public event Action OnShutdownMessageReceived;
    public event Action<string> OnConnectionError;
    private CancellationTokenSource _tokenSource;

    public async Task SetUpListener(string ipAddress, int port)
    {
        _tokenSource = new CancellationTokenSource();
        try
        {
            IPAddress localAddr = IPAddress.Parse(ipAddress);
            _listener = new TcpListener(localAddr, port);
            _listener.Start();

            await StartListeningAsync(_tokenSource.Token);
        }
        catch (SocketException e)
        {
            OnConnectionError?.Invoke($"Socket Error: {e.Message}");
        }
        finally
        {
            _listener?.Stop();
            Console.WriteLine("Server Stopped");
        }
    }

    private async Task StartListeningAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient client = null;
            NetworkStream stream = null;
            try
            {
                client = await _listener.AcceptTcpClientAsync();
                stream = client.GetStream();

                // Handle each client in a separate task
                _ = HandleClientAsync(stream, token);
            }
            catch (SocketException e)
            {
                OnConnectionError?.Invoke($"Socket error: {e.Message}");
                _tokenSource?.Cancel();
            }
            finally
            {
                // Let HandleClientAsync handle the cleanup since it's running separately
                if (token.IsCancellationRequested)
                {
                    stream?.Close();
                    client?.Close();
                }
            }
        }
    }

    private async Task HandleClientAsync(NetworkStream stream, CancellationToken token)
    {
        try
        {
            using (stream)  // This will also close the TcpClient
            {
                Header header = new Header();
                byte[] headerData = new byte[header.GetLength()];

                if (await stream.ReadAsync(headerData, 0, headerData.Length, token) != headerData.Length)
                {
                    Console.Error.WriteLine("Header invalid");
                    return;
                }

                header.code = headerData[0];
                header.length = BitConverter.ToInt32(headerData, 1);

                if (header.code == 5 && header.length == 0)
                {
                    OnShutdownMessageReceived?.Invoke();
                    _tokenSource?.Cancel();  // Stop listening when shutdown message received
                }
            }
        }
        catch (IOException ex)
        {
            OnConnectionError?.Invoke($"IO Error while receiving data: {ex.Message}");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Normal cancellation, exit quietly
        }
        catch (Exception ex)
        {
            OnConnectionError?.Invoke($"Error while receiving data: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _tokenSource?.Cancel();
        _listener?.Stop();
    }
}
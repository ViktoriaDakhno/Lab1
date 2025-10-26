using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class UdpClientWrapper : IUdpClient
    {
        private readonly IPEndPoint _localEndPoint;
        private CancellationTokenSource? _cts;
        private UdpClient? _udpClient;

        public event EventHandler<byte[]>? MessageReceived;

        public UdpClientWrapper(int port)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public async Task StartListeningAsync()
        {
            if (_cts != null)
                throw new InvalidOperationException("Listener is already running.");
        
            _cts = new CancellationTokenSource();
            Console.WriteLine("Start listening for UDP messages...");
        
            try
            {
                _udpClient = new UdpClient(_localEndPoint);
        
                while (!_cts.Token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(_cts.Token);
                    MessageReceived?.Invoke(this, result.Buffer);
        
                    Console.WriteLine($"Received from {result.RemoteEndPoint}");
                }
            }
            catch (OperationCanceledException)
            {
                // Listener was stopped
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Listener stopped.");
                _cts?.Dispose();
                _cts = null;
            }
        }


        public void StopListening()
        {
            _cts?.Cancel();
            _udpClient?.Close();
            Console.WriteLine("Stopped listening for UDP messages.");
        }

       public void Exit()
       {
       try
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
    
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }
    
            Console.WriteLine("UDP client exited and resources released.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while exiting UDP client: {ex.Message}");
        }
    }


        public override int GetHashCode()
        {
            var payload = $"{nameof(UdpClientWrapper)}|{_localEndPoint.Address}|{_localEndPoint.Port}";

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return BitConverter.ToInt32(hash, 0);
        }
    }
}

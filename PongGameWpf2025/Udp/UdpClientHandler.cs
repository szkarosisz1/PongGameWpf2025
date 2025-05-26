using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PongGameWpf2025.Udp
{
    public class UdpClientHandler
    {
        readonly UdpClient _client;
        readonly IPEndPoint _serverEndPoint;
        public event EventHandler<string> MessageReceived;

        public string ServerIp { get; }
        public int ServerPort { get; }

        private bool listening;

        public UdpClientHandler(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            _client = new UdpClient();
        }

        public async Task SendMessageAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(data, data.Length, _serverEndPoint);
        }

        public async Task<string> ReceiveMessageAsync()
        {
            var result = await _client.ReceiveAsync();
            return Encoding.UTF8.GetString(result.Buffer);
        }

        public void StartListening()
        {
            listening = true;
            Task.Run(ListenLoop);
        }

        private async Task ListenLoop()
        {
            while (listening)
            {
                try
                {
                    var result = await _client.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);

                    // Kiváltjuk az eseményt, amit a játékablak feliratkozik
                    MessageReceived?.Invoke(this, msg);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Naplózhatod, ha szükséges
                    Console.WriteLine($"[UdpClientHandler] Hiba: {ex.Message}");
                }
            }
        }

        public void StopListening()
        {
            listening = false;
            _client?.Close();
        }

        public void Close()
        {
            StopListening();
        }
    }
}

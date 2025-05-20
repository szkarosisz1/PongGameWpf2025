using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PongGameWpf2025.Udp
{
    internal class UdpClientHandler
    {
        readonly UdpClient _client;
        readonly IPEndPoint _serverEndPoint;

        public string ServerIp { get; }
        public int ServerPort { get; }

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

        public void Close()
        {
            _client?.Close();
        }
    }
}

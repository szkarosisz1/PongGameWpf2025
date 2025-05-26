using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PongGameWpf2025.Udp
{
    public class UdpServer
    {
        UdpClient udpListener;
        int port;
        bool listening;

        Dictionary<IPEndPoint, string> connectedClients = new();

        public event EventHandler<string> ClientConnected;
        public event EventHandler AllClientsDisconnected;
        public event EventHandler<(string message, IPEndPoint senderEP)> MessageReceived;
        public event EventHandler<string> ClientMovementReceived;

        public int Port => port;

        public UdpServer(int port)
        {
            this.port = port;
        }

        public void StartListening()
        {
            udpListener = new UdpClient(port);
            listening = true;
            ListenLoop();
        }

        async void ListenLoop()
        {
            Debug.WriteLine($"[UdpServer] Hallgat a {port}-os porton...");
            while (listening)
            {
                try
                {
                    var result = await udpListener.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);
                    var senderEP = result.RemoteEndPoint;

                    Debug.WriteLine($"[UdpServer] Üzenet érkezett: {msg}");

                    if (msg.StartsWith("NAME: "))
                    {
                        string clientName = msg.Substring(6);

                        if (!connectedClients.ContainsKey(senderEP))
                        {
                            connectedClients[senderEP] = clientName;
                            ClientConnected?.Invoke(this, clientName);
                        }

                        // Válasz a regisztrációra
                        await udpListener.SendAsync(Encoding.UTF8.GetBytes("ACK"), senderEP);
                    }
                    else if (msg.StartsWith("LEFT|"))
                    {
                        string clientName = msg.Substring(5);

                        var clientToRemove = connectedClients.FirstOrDefault(pair => pair.Value == clientName).Key;

                        if (!clientToRemove.Equals(default(IPEndPoint)))
                        {
                            connectedClients.Remove(clientToRemove);
                            Debug.WriteLine($"[UdpServer] Kliens kilépett: {clientName} - {DateTime.Now:yyyy.MM.dd HH:mm:ss}");
                        }
                        else
                        {
                            Debug.WriteLine($"[UdpServer] Kilépési üzenet érkezett ismeretlen kliensről: {clientName}");
                        }

                        // Válasz a kilépésre
                        await udpListener.SendAsync(Encoding.UTF8.GetBytes("BYE"), senderEP);

                        if (connectedClients.Count == 0)
                        {
                            Debug.WriteLine("[UdpServer] Minden kliens kilépett, leállítom a szervert.");
                            listening = false;
                            AllClientsDisconnected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        ClientMovementReceived?.Invoke(this, msg);

                        // Forward the message to all other clients
                        foreach (var clientEP in connectedClients.Keys)
                        {
                            if (!clientEP.Equals(senderEP))
                            {
                                byte[] data = Encoding.UTF8.GetBytes(msg);
                                await udpListener.SendAsync(data, data.Length, clientEP);
                            }
                        }
                    }

                    MessageReceived?.Invoke(this, (msg, senderEP));
                }
                catch (ObjectDisposedException)
                {
                    Debug.WriteLine("[UdpServer] A socket le lett zárva, kilépünk a hallgatásból.");
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    Debug.WriteLine("[UdpServer] I/O művelet megszakítva, bezárás miatt kilépünk.");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[UdpServer] Hiba a fogadásnál: {ex.Message}");
                }
            }

            udpListener?.Close();
        }

        public void Stop()
        {
            listening = false;

            try
            {
                udpListener?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UdpServer] Hiba a socket bezárásakor: {ex.Message}");
            }
        }

    }
}

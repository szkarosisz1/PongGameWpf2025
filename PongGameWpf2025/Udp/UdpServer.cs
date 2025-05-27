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

                        await udpListener.SendAsync(Encoding.UTF8.GetBytes("ACK"), senderEP);
                    }
                    else if (msg.StartsWith("LEFT|"))
                    {
                        var parts = msg.Split('|');
                        if (parts.Length >= 3)
                        {
                            string clientName = parts[1];
                            string role = parts[2].ToUpperInvariant();

                            var clientPair = connectedClients.FirstOrDefault(pair => pair.Value == clientName);

                            if (!clientPair.Equals(default(KeyValuePair<IPEndPoint, string>)))
                            {
                                var clientToRemove = clientPair.Key;
                                connectedClients.Remove(clientToRemove);
                                Debug.WriteLine($"[UdpServer] {role} kilépett: {clientName} - {DateTime.Now:yyyy.MM.dd HH:mm:ss}");

                                await udpListener.SendAsync(Encoding.UTF8.GetBytes("BYE"), senderEP);

                                if (role == "HOST")
                                {
                                    Debug.WriteLine("[UdpServer] A Host kilépett, értesítem a vendégeket...");

                                    byte[] hostLeftMsg = Encoding.UTF8.GetBytes("HOST_LEFT");
                                    foreach (var ep in connectedClients.Keys)
                                    {
                                        await udpListener.SendAsync(hostLeftMsg, ep);
                                    }

                                    listening = false;
                                    AllClientsDisconnected?.Invoke(this, EventArgs.Empty);
                                    return;
                                }
                                else if (role == "GUEST")
                                {
                                    byte[] guestLeftMsg = Encoding.UTF8.GetBytes($"GUEST_LEFT|{clientName}");
                                    foreach (var ep in connectedClients.Keys)
                                    {
                                        await udpListener.SendAsync(guestLeftMsg, ep);
                                    }
                                }

                                if (connectedClients.Count == 0)
                                {
                                    Debug.WriteLine("[UdpServer] Minden kliens kilépett, leállítom a szervert.");
                                    listening = false;
                                    AllClientsDisconnected?.Invoke(this, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"[UdpServer] Kilépési üzenet érkezett ismeretlen kliensről: {clientName}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("[UdpServer] Helytelen LEFT üzenet formátum");
                        }
                    }
                    else
                    {
                        ClientMovementReceived?.Invoke(this, msg);

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

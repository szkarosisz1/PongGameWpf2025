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
    internal class UdpServer
    {
        UdpClient udpListener;
        int port;
        bool listening;

        private List<string> connectedClients = new();

        public event EventHandler<string> ClientConnected;
        public event EventHandler AllClientsDisconnected;
        public event EventHandler<(string message, IPEndPoint senderEP)> MessageReceived;

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

                    Debug.WriteLine($"[UdpServer] Üzenet érkezett: {msg}");

                    if (msg.StartsWith("NAME: "))
                    {
                        string clientName = msg.Substring(6);
                        if (!connectedClients.Contains(clientName))
                        {
                            connectedClients.Add(clientName);
                            Debug.WriteLine($"[UdpServer] Új kliens regisztrálva: {clientName}");
                            ClientConnected?.Invoke(this, clientName);
                        }
                    }
                    else if (msg.StartsWith("LEFT|"))
                    {
                        string clientName = msg.Substring(5);
                        if (connectedClients.Contains(clientName))
                        {
                            connectedClients.Remove(clientName);
                            Debug.WriteLine($"[UdpServer] Kliens kilépett: {clientName} - {DateTime.Now:yyyy.MM.dd HH:mm:ss}");
                        }
                        else
                        {
                            Debug.WriteLine($"[UdpServer] Kilépési üzenet érkezett ismeretlen kliensről: {clientName}");
                        }

                        if (connectedClients.Count == 0)
                        {
                            Debug.WriteLine("[UdpServer] Minden kliens kilépett, leállítom a szervert.");
                            listening = false;
                            AllClientsDisconnected?.Invoke(this, EventArgs.Empty);
                        }
                    }


                    MessageReceived?.Invoke(this, (msg, result.RemoteEndPoint));
                }
                catch (ObjectDisposedException)
                {
                    // socket bezárva, szépen kilépünk
                    Debug.WriteLine("[UdpServer] A socket le lett zárva, kilépünk a hallgatásból.");
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    // I/O megszakítva: ez akkor jön, ha bezártuk a socketet
                    Debug.WriteLine("[UdpServer] I/O művelet megszakítva, bezárás miatt kilépünk.");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[UdpServer] Hiba a fogadásnál: {ex.Message}");
                }
            }

            // A ciklus után zárjuk le a socketet (itt biztonságos)
            udpListener?.Close();
        }


        public void Stop()
        {
            listening = false;
            // Ne zárd itt a udpListener-t! Csak jelzed, hogy álljon le.
            // udpListener?.Close(); // ezt vedd ki innen, mert ezzel megszakítod a fogadást
        }

    }
}

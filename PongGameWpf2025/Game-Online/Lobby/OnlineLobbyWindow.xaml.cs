using PongGameWpf2025.Game_Online.Game_Window_Online;
using PongGameWpf2025.Game_Online.Name_Input_Online;
using PongGameWpf2025.Udp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;


namespace PongGameWpf2025
{
    public partial class OnlineLobbyWindow : Window
    {
        bool isServer;
        string serverIp;
        int port;
        UdpServer server;
        UdpClientHandler udpClientHandler;

        string playerName;
        string serverPlayerName = "Hoszt";
        ObservableCollection<string> players = new ObservableCollection<string>();
        Dictionary<IPEndPoint, string> connectedClients = new();

        public OnlineLobbyWindow(bool isServer, string serverIp, int port, UdpServer server)
        {
            InitializeComponent();

            this.isServer = isServer;
            this.serverIp = serverIp;
            this.port = port;
            this.server = server;

            PlayersListBox.ItemsSource = players;

            if (isServer)
            {
                StartGameButton.IsEnabled = true;

                playerName = PromptName();
                if (playerName == null)
                    return;

                AddPlayer(playerName);

                this.server.ClientConnected += Server_ClientConnected;
                this.server.MessageReceived += (s, e) => Server_MessageReceived(s, e.message, e.senderEP);

                Debug.WriteLine($"[OnlineLobbyWindow] Szerverként indultál, port: {port}, név: {playerName}");

                udpClientHandler = new UdpClientHandler(serverIp, port);
                _ = udpClientHandler.SendMessageAsync("NAME: " + playerName);
            }
            else
            {
                StartGameButton.IsEnabled = false;

                playerName = PromptName();
                if (playerName == null)
                    return;

                udpClientHandler = new UdpClientHandler(serverIp, port);

                _ = udpClientHandler.SendMessageAsync("NAME: " + playerName);

                AddPlayer(playerName);

                Thread receiveThread = new Thread(() => ListenToServer());
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
        }

        string PromptName()
        {
            InputNameWindow input = new InputNameWindow();
            if (input.ShowDialog() == true)
            {
                return input.PlayerName.Trim();
            }
            else
            {
                this.Close();
                return null;
            }
        }

        void Server_ClientConnected(object sender, string clientName)
        {
            Dispatcher.Invoke(() =>
            {
                if (!players.Contains(clientName))
                {
                    AddPlayer(clientName);
                    Debug.WriteLine($"[OnlineLobbyWindow] Új játékos csatlakozott: {clientName}");
                }
            });
        }

        void Server_MessageReceived(object sender, string message, IPEndPoint senderEP)
        {
            if (message.StartsWith("NAME: "))
            {
                string name = message.Substring(6);

                if (!connectedClients.ContainsKey(senderEP))
                {
                    connectedClients[senderEP] = name;
                    Dispatcher.Invoke(() => AddPlayer(name));

                    using (var client = new UdpClient())
                    {
                        byte[] serverNameMsg = Encoding.UTF8.GetBytes("NAME: " + playerName);
                        client.Send(serverNameMsg, serverNameMsg.Length, senderEP);
                    }

                    Debug.WriteLine($"[Server_MessageReceived] Név: {name}, IP: {senderEP}");
                }
            }
        }

        void AddPlayer(string name)
        {
            if (!players.Contains(name))
                players.Add(name);
        }

        void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (players.Count < 2)
            {
                MessageBox.Show("A játék elindításához legalább 2 játékos szükséges.");
                return;
            }

            Debug.WriteLine("[OnlineLobbyWindow] Játék indítása...");

            string opponentName = players.FirstOrDefault(p => p != playerName) ?? "Ellenfél";

            foreach (var kvp in connectedClients)
            {
                using (var client = new UdpClient())
                {
                    byte[] nameMsg = Encoding.UTF8.GetBytes("SERVERNAME: " + playerName);
                    client.Send(nameMsg, nameMsg.Length, kvp.Key);

                    byte[] startMsg = Encoding.UTF8.GetBytes("START");
                    client.Send(startMsg, startMsg.Length, kvp.Key);
                }
            }

            OnlineGameWindow hostGame = new OnlineGameWindow(playerName, opponentName, udpClientHandler, server, true);
            hostGame.Title = $"Online Pong Game - {playerName} (Host)";
            hostGame.Show();

            Close();
        }

        async void ListenToServer()
        {
            try
            {
                while (true)
                {
                    string message = await udpClientHandler.ReceiveMessageAsync();
                    Debug.WriteLine($"[KLIENS ÜZENET] {message}");

                    if (message.StartsWith("SERVERNAME: "))
                    {
                        serverPlayerName = message.Substring("SERVERNAME: ".Length);
                        Debug.WriteLine($"[KLIENS] Hoszt neve: {serverPlayerName}");
                    }
                    else if (message.StartsWith("START"))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Debug.WriteLine("Játék indítása (kliens oldalon)...");

                            OnlineGameWindow guestGame = new OnlineGameWindow(serverPlayerName, playerName, udpClientHandler, server, false);
                            guestGame.Title = $"Online Pong Game - {playerName} (Guest)";
                            guestGame.Show();
                            Close();
                        });
                    }
                    else if (message.StartsWith("NAME: "))
                    {
                        string name = message.Substring(6);
                        Dispatcher.Invoke(() => AddPlayer(name));
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"[KLIENS] Socket hiba: {ex.Message}");
            }
        }
    }
}

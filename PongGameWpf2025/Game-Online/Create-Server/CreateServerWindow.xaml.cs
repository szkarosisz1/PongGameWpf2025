using PongGameWpf2025.Udp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PongGameWpf2025.Game_Online.Create_Server
{
    public partial class CreateServerWindow : Window
    {
        UdpServer server;

        public CreateServerWindow()
        {
            InitializeComponent();
            StartServer();
        }

        async void StartServer()
        {
            try
            {
                server = new UdpServer(55555);
                server.StartListening();

                // Szerver példány tárolása későbbi leállításhoz
                Application.Current.Properties["ServerInstance"] = server;

                Debug.WriteLine("Szerver elindítva, port: 55555");
                StatusTextBlock.Text = "Szerver elindítva a 127.0.0.1:55555-ös címen.";

                // Kis késleltetés, hogy a szerver tényleg induljon
                await Task.Delay(1000);

                // Megnyitjuk az online lobby ablakot (saját IP, port)
                // A játékos neve kérése az Online ablakban lesz
                OnlineLobbyWindow onlineWindow = new OnlineLobbyWindow(true, "127.0.0.1", 55555, server);
                onlineWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Szerver indítás hiba: {ex.Message}");
                MessageBox.Show($"Hiba történt a szerver indításakor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

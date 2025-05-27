using System;
using System.Collections.Generic;
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

namespace PongGameWpf2025.Game_Online.Server_Input
{
    public partial class ServerInputWindow : Window
    {
        public ServerInputWindow()
        {
            InitializeComponent();
        }

        void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parts = ServerNameTextBox.Text.Split(':');
                string ip = parts[0];
                int port = int.Parse(parts[1]);

                OnlineLobbyWindow onlineWindow = new OnlineLobbyWindow(false, ip, port, null);
                onlineWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hibás IP-cím vagy port formátum.\n" + ex.Message);
            }
        }
    }
}

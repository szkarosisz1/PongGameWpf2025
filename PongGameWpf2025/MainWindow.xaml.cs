using PongGameWpf2025.Game_Online.Create_Server;
using PongGameWpf2025.Game_Online.Game_Window_Online;
using PongGameWpf2025.Game_Online.Server_Input;
using System.Windows;

namespace PongGameWpf2025
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void Btn1v1_Click(object sender, RoutedEventArgs e)
        {
            _1v1Window _1V1Window = new _1v1Window();
            _1V1Window.Show();
            Close();
        }

        void BtnOnline_Click(object sender, RoutedEventArgs e)
        {
            ServerInputWindow serverInputWindow = new ServerInputWindow();
            serverInputWindow.ShowDialog();
        }

        void BtnCreateServer_Click(object sender, RoutedEventArgs e)
        {
            CreateServerWindow createServerWindow = new CreateServerWindow();
            createServerWindow.Show();
        }

        void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
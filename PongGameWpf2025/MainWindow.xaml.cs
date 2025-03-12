using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PongGameWpf2025
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn1v1_Click(object sender, RoutedEventArgs e)
        {
            _1v1Window _1V1Window = new _1v1Window();
            _1V1Window.Show();
            Close();
        }

        private void Btn1v2_Click(object sender, RoutedEventArgs e)
        {
            _1v2Window _1V2Window = new _1v2Window();
            _1V2Window.Show();
            Close();
        }

        private void BtnOnline_Click(object sender, RoutedEventArgs e)
        {
            OnlineWindow onlineWindow = new OnlineWindow();
            onlineWindow.Show();
            Close();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
            Close();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
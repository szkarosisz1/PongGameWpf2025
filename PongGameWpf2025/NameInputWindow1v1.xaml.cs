using System.Windows;

namespace PongGameWpf2025
{
    public partial class NameInputWindow1v1 : Window
    {
        public string PlayerName { get; set; }

        public NameInputWindow1v1()
        {
            InitializeComponent();
        }

        void Ok1v1Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PlayerName1v1TextBox.Text))
            {
                PlayerName = PlayerName1v1TextBox.Text;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("A név megadása kötelező!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
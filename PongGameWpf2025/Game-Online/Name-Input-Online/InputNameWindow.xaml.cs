using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PongGameWpf2025.Game_Online.Name_Input_Online
{
    public partial class InputNameWindow : Window
    {
        public string PlayerName { get; private set; }

        public InputNameWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                PlayerName = NameTextBox.Text.Trim();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Kérlek, adj meg egy nevet!", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

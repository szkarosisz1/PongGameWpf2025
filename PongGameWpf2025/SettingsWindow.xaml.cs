using System.ComponentModel;
using System.Windows;
using System.Windows.Media;


namespace PongGameWpf2025
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        Color _ballColor;
        Color _padColor;
        Color _backgroundColor;
        int _ballSpeed = 8;
        int _padSpeed = 8;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color BallColor
        {
            get => _ballColor;
            set
            {
                _ballColor = value;
                OnPropertyChanged(nameof(BallColor));
            }
        }

        public Color PadColor
        {
            get => _padColor;
            set
            {
                _padColor = value;
                OnPropertyChanged(nameof(PadColor));
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public int BallSpeed
        {
            get => _ballSpeed;
            set
            {
                _ballSpeed = value;
                OnPropertyChanged(nameof(BallSpeed));
            }
        }

        public int PadSpeed
        {
            get => _padSpeed;
            set
            {
                _padSpeed = value;
                OnPropertyChanged(nameof(PadSpeed));
            }
        }

        public SettingsWindow(Color ballColor, Color padColor, Color backgroundColor, int ballSpeed, int padSpeed)
        {
            InitializeComponent();
            DataContext = this;

            BallColor = ballColor;
            PadColor = padColor;
            BackgroundColor = backgroundColor;
            BallSpeed = ballSpeed;
            PadSpeed = padSpeed;
        }

        void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

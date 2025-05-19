using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PongGameWpf2025
{
    public partial class _1v1Window : Window
    {
        bool _PlayerOneUp;
        bool _PlayerOneDown;
        bool _playerTwoUp;
        bool _playerTwoDown;
        bool _isPaused = false;
        bool _gameOver = false;

        Color _ballColor = Colors.WhiteSmoke;
        Color _padColor = Colors.WhiteSmoke;
        Color _backgroundColor = Colors.Black;

        DispatcherTimer _timer;

        double _angle = 45;
        int _speed = 8;
        int _padSpeed = 8;

        readonly Ball _ball = new Ball { X = 380, Y = 210, MovingRight = true };
        readonly Pad _leftPad = new Pad { YPosition = 150 };
        readonly Pad _rightPad = new Pad { YPosition = 150 };

        string _playerOneName;
        string _playerTwoName;

        public string PlayerOneName
        {
            get { return _playerOneName; }
            set
            {
                _playerOneName = value;
                PlayerOneNameLabel.Content = value;
            }
        }

        public string PlayerTwoName
        {
            get { return _playerTwoName; }
            set
            {
                _playerTwoName = value;
                PlayerTwoNameLabel.Content = value;
            }
        }

        public _1v1Window()
        {
            InitializeComponent();
            DataContext = _ball;
            RightPad.DataContext = _rightPad;
            LeftPad.DataContext = _leftPad;
            Ball.DataContext = _ball;

            while (string.IsNullOrWhiteSpace(PlayerOneName))
            {
                NameInputWindow1v1 nameInputWindow1v1 = new NameInputWindow1v1();
                if (nameInputWindow1v1.ShowDialog() == true)
                {
                    PlayerOneName = nameInputWindow1v1.PlayerName;
                }
            }

            while (string.IsNullOrWhiteSpace(PlayerTwoName))
            {
                NameInputWindow1v1 nameInputWindow1v1 = new NameInputWindow1v1();
                if (nameInputWindow1v1.ShowDialog() == true)
                {
                    PlayerTwoName = nameInputWindow1v1.PlayerName;
                }
            }

            MessageBox.Show($"Játékos 1: {PlayerOneName}\nJátékos 2: {PlayerTwoName}", "Játékosok", MessageBoxButton.OK, MessageBoxImage.Information);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Start();
            _timer.Tick += Timer_Tick;
        }

        void NewGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = true;
            PauseLabel.Visibility = Visibility.Hidden;
            StartNewGame();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                MovePads();
                if (_ball != null)
                {
                    if (_ball.Y <= 0) _angle = _angle + (180 - 2 * _angle);
                    if (_ball.Y >= MainCanvas.ActualHeight - 20) _angle = _angle + (180 - 2 * _angle);

                    if (CheckCollision())
                    {
                        ChangeAngle();
                        ChangeDirection();
                    }

                    double radians = (Math.PI / 180) * _angle;
                    Vector vector = new Vector { X = Math.Sin(radians), Y = -Math.Cos(radians) };
                    _ball.X += vector.X * _speed;
                    _ball.Y += vector.Y * _speed;

                    double windowWidth = MainCanvas.ActualWidth;

                    if (_ball.X >= windowWidth - 30)
                    {
                        _ball.LeftResult += 1;
                        GameReset();
                        CheckForWinner();
                    }
                    if (_ball.X <= 10)
                    {
                        _ball.RightResult += 1;
                        GameReset();
                        CheckForWinner();
                    }
                }
            }
        }

        void CheckForWinner()
        {
            if (_ball.LeftResult == 9)
            {
                MessageBox.Show($"{_playerOneName} nyerte a meccset!", "Játék vége", MessageBoxButton.OK, MessageBoxImage.Information);
                _isPaused = true;
                _gameOver = true;
            }
            else if (_ball.RightResult == 9)
            {
                MessageBox.Show($"{_playerTwoName} nyerte a meccset!", "Játék vége", MessageBoxButton.OK, MessageBoxImage.Information);
                _isPaused = true;
                _gameOver = true;
            }
        }

        void StartNewGame()
        {
            var result = MessageBox.Show("Legyen új játék?", "Új játék", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _ball.LeftResult = 0;
                _ball.RightResult = 0;
                GameReset();
                _isPaused = false;
            }
            else
            {
                _isPaused = true;
            }
        }

        void GameReset()
        {
            _ball.Y = 210;
            _ball.X = 380;
        }

        void ChangeAngle()
        {
            if (_ball.MovingRight == true)
            {
                _angle = 270 - ((_ball.Y + 10) - (_rightPad.YPosition + 40));
                if (_angle < 200)
                    _angle = 200;
                if (_angle > 340)
                    _angle = 340;
            }
            else if (_ball.MovingRight == false)
            {
                _angle = 90 + ((_ball.Y + 10) - (_leftPad.YPosition + 40));
                if (_angle > 160)
                    _angle = 160;
                if (_angle < 20)
                    _angle = 20;
            }
        }

        void ChangeDirection()
        {
            _ball.MovingRight = !_ball.MovingRight;
        }

        bool CheckCollision()
        {
            bool collisionResult = false;
            double padWidth = 20;
            double padHeight = 80;

            if (_ball.MovingRight == true)
            {
                collisionResult = _ball.X >= 800 - padWidth && (_ball.Y > _rightPad.YPosition && _ball.Y < _rightPad.YPosition + padHeight);
            }
            else if (_ball.MovingRight == false)
            {
                collisionResult = _ball.X <= 28 && (_ball.Y > _leftPad.YPosition && _ball.Y < _leftPad.YPosition + padHeight);
            }

            return collisionResult;
        }

        void _1v1Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W) _PlayerOneUp = false;
            if (e.Key == Key.S) _PlayerOneDown = false;
            if (e.Key == Key.Up) _playerTwoUp = false;
            if (e.Key == Key.Down) _playerTwoDown = false;
        }

        void _1v1Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W) _PlayerOneUp = true;
            if (e.Key == Key.S) _PlayerOneDown = true;
            if (e.Key == Key.Up) _playerTwoUp = true;
            if (e.Key == Key.Down) _playerTwoDown = true;

            if (e.Key == Key.P)
            {
                _isPaused = !_isPaused;
                PauseLabel.Visibility = _isPaused ? Visibility.Visible : Visibility.Hidden;
            }
        }

        void MovePads()
        {
            int padHeight = 80;

            if (_PlayerOneUp)
                _leftPad.YPosition = Math.Max(_leftPad.YPosition - _padSpeed, 0);

            if (_PlayerOneDown)
                _leftPad.YPosition = Math.Min(_leftPad.YPosition + _padSpeed, (int)MainCanvas.ActualHeight - padHeight);

            if (_playerTwoUp)
                _rightPad.YPosition = Math.Max(_rightPad.YPosition - _padSpeed, 0);

            if (_playerTwoDown)
                _rightPad.YPosition = Math.Min(_rightPad.YPosition + _padSpeed, (int)MainCanvas.ActualHeight - padHeight);
        }

        void ButtonReturnToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }

            MainWindow mainMenuWindow = new MainWindow();
            mainMenuWindow.Show();
            Close();
        }

        void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool wasPausedBefore = _isPaused;
            _isPaused = true;

            if (!_gameOver)
            {
                PauseLabel.Visibility = Visibility.Visible;
            }

            var settingsWindow = new SettingsWindow(_ballColor, _padColor, _backgroundColor, _speed, _padSpeed)
            {
                BallSpeed = _speed,
                PadSpeed = _padSpeed
            };

            if (settingsWindow.ShowDialog() == true)
            {
                _ballColor = settingsWindow.BallColor;
                _padColor = settingsWindow.PadColor;
                _backgroundColor = settingsWindow.BackgroundColor;
                _speed = settingsWindow.BallSpeed;
                _padSpeed = settingsWindow.PadSpeed;

                ApplyColors();
            }

            if (!_gameOver)
            {
                _isPaused = wasPausedBefore;
                PauseLabel.Visibility = _isPaused ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                PauseLabel.Visibility = Visibility.Hidden;
            }

        }

        void ApplyColors()
        {
            Ball.Fill = new SolidColorBrush(_ballColor);
            LeftPad.Fill = new SolidColorBrush(_padColor);
            RightPad.Fill = new SolidColorBrush(_padColor);
            MainCanvas.Background = new SolidColorBrush(_backgroundColor);
        }

    }
}

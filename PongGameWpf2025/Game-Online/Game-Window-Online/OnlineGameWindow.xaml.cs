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
using System.Windows.Threading;

namespace PongGameWpf2025.Game_Online.Game_Window_Online
{
    public partial class OnlineGameWindow : Window
    {
        bool _playerOneUp, _playerOneDown, _playerTwoUp, _playerTwoDown;
        bool _isHost;
        bool _isPaused = false;
        bool _gameOver = false;

        DispatcherTimer _timer;
        double _angle = 45;
        int _speed = 3;
        int _padSpeed = 2;

        readonly Ball _ball = new Ball { X = 380, Y = 210, MovingRight = true };
        readonly Pad _leftPad = new Pad { YPosition = 150 };
        readonly Pad _rightPad = new Pad { YPosition = 150 };
        readonly UdpClientHandler _udpClientHandler;

        string _playerOneName, _playerTwoName;

        public string PlayerOneName
        {
            get { return _playerOneName; }
            set
            {
                _playerOneName = value;
                PlayerOneNameLabel.Content = $"{value}";
                PlayerOneNameLabel.Foreground = new SolidColorBrush(Colors.LightBlue);
            }
        }

        public string PlayerTwoName
        {
            get { return _playerTwoName; }
            set
            {
                _playerTwoName = value;
                PlayerTwoNameLabel.Content = $"{value}";
                PlayerTwoNameLabel.Foreground = new SolidColorBrush(Colors.Orange);
            }
        }

        public OnlineGameWindow(string playerOneName, string playerTwoName, UdpClientHandler udpClientHandler, UdpServer udpServer, bool isHost)
        {
            InitializeComponent();
            DataContext = _ball;
            RightPad.DataContext = _rightPad;
            LeftPad.DataContext = _leftPad;
            Ball.DataContext = _ball;

            PlayerOneName = playerOneName;
            PlayerTwoName = playerTwoName;
            _udpClientHandler = udpClientHandler;
            _isHost = isHost;

            _udpClientHandler.MessageReceived += (s, msg) => HandleIncomingMessage(msg);
            _udpClientHandler.StartListening();

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);

            _timer.Tick += Timer_Tick;
            _timer.Start();

            if (udpServer is not null)
            {
                udpServer.ClientMovementReceived -= UdpServer_ClientMovementReceived;
                udpServer.ClientMovementReceived += UdpServer_ClientMovementReceived;
            }

            Title = isHost ? $"Online Pong Game - {playerOneName} (Host)" : $"Online Pong Game - {playerTwoName} (Guest)";
            StartReceivingMessages();
        }

        void UdpServer_ClientMovementReceived(object? sender, string msg)
        {
            HandleIncomingMessage(msg);
        }
        async void LeaveServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Biztosan elhagyja a szervert?", "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string playerNameToSend = !string.IsNullOrWhiteSpace(PlayerTwoName) ? PlayerTwoName : PlayerOneName;
                    bool isHost = string.IsNullOrWhiteSpace(PlayerTwoName);

                    if (!string.IsNullOrWhiteSpace(playerNameToSend))
                    {
                        string leaveMessage = isHost ? $"LEFT|{playerNameToSend}|HOST" : $"LEFT|{playerNameToSend}|GUEST";
                        await _udpClientHandler?.SendMessageAsync(leaveMessage);
                    }

                    _udpClientHandler?.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt a szerver elhagyásakor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (Application.Current.Properties["ServerInstance"] is UdpServer server)
                {
                    server.Stop();
                    Application.Current.Properties.Remove("ServerInstance");
                    Debug.WriteLine("[OnlineGameWindow] Szerver leállítva.");
                }

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                }

                Close();
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                if (_isHost)
                {
                    UpdateGameState(); // Csak a Host végzi el a játék logikát
                }

                RenderGame(); // Mindkét oldal frissíti a képernyőt
            }
        }

        void RenderGame()
        {
            MovePads();
        }

        void UpdateGameState()
        {
            MovePads();

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
                ResetGame();
                CheckForWinner();
            }
            else if (_ball.X <= 10)
            {
                _ball.RightResult += 1;
                ResetGame();
                CheckForWinner();
            }

            // Küldés csak a Hostnál
            string ballMsg = $"BALL|{_ball.X:F1};{_ball.Y:F1};{_angle:F1};{_ball.MovingRight}";
            string leftPadMsg = $"PAD|LEFT;{_leftPad.YPosition}";
            string rightPadMsg = $"PAD|RIGHT;{_rightPad.YPosition}";
            string scoreMsg = $"SCORE|{_ball.LeftResult};{_ball.RightResult}";
            _udpClientHandler?.SendMessageAsync(ballMsg);
            _udpClientHandler?.SendMessageAsync(leftPadMsg);
            _udpClientHandler?.SendMessageAsync(rightPadMsg);
            _udpClientHandler?.SendMessageAsync(scoreMsg);
        }

        void CheckForWinner()
        {
            if (_ball.LeftResult == 9 && !_gameOver)
            {
                _gameOver = true;
                _isPaused = true;
                MessageBox.Show($"{_playerOneName} nyerte a meccset!", "Játék vége", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_ball.RightResult == 9 && !_gameOver)
            {
                _gameOver = true;
                _isPaused = true;
                MessageBox.Show($"{_playerTwoName} nyerte a meccset!", "Játék vége", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void ResetGame()
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

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isHost)
            {
                if (e.Key == Key.W)
                {
                    _playerOneUp = true;
                    _udpClientHandler?.SendMessageAsync("MOVE|LEFTPAD:W");
                }
                else if (e.Key == Key.S)
                {
                    _playerOneDown = true;
                    _udpClientHandler?.SendMessageAsync("MOVE|LEFTPAD:S");
                }
            }
            else
            {
                if (e.Key == Key.Up)
                {
                    _playerTwoUp = true;
                    _udpClientHandler?.SendMessageAsync("MOVE|RIGHTPAD:UP");
                }
                else if (e.Key == Key.Down)
                {
                    _playerTwoDown = true;
                    _udpClientHandler?.SendMessageAsync("MOVE|RIGHTPAD:DOWN");
                }
            }

            if (e.Key == Key.P)
            {
                _isPaused = !_isPaused;
                PauseLabel.Visibility = _isPaused ? Visibility.Visible : Visibility.Hidden;
                _udpClientHandler?.SendMessageAsync("PAUSE|" + (_isPaused ? "ON" : "OFF"));
            }
        }

        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (_isHost)
            {
                if (e.Key == Key.W)
                {
                    _playerOneUp = false;
                    _udpClientHandler?.SendMessageAsync("STOP|LEFTPAD:W");
                }
                else if (e.Key == Key.S)
                {
                    _playerOneDown = false;
                    _udpClientHandler?.SendMessageAsync("STOP|LEFTPAD:S");
                }
            }
            else
            {
                if (e.Key == Key.Up)
                {
                    _playerTwoUp = false;
                    _udpClientHandler?.SendMessageAsync("STOP|RIGHTPAD:UP");
                }
                else if (e.Key == Key.Down)
                {
                    _playerTwoDown = false;
                    _udpClientHandler?.SendMessageAsync("STOP|RIGHTPAD:DOWN");
                }
            }
        }

        void MovePads()
        {
            int padHeight = 80;

            if (_isHost)
            {
                if (_playerOneUp)
                    _leftPad.YPosition = Math.Max(_leftPad.YPosition - _padSpeed, 0);

                if (_playerOneDown)
                    _leftPad.YPosition = Math.Min(_leftPad.YPosition + _padSpeed, (int)MainCanvas.ActualHeight - padHeight);

                if (_playerTwoUp)
                    _rightPad.YPosition = Math.Max(_rightPad.YPosition - _padSpeed, 0);

                if (_playerTwoDown)
                    _rightPad.YPosition = Math.Min(_rightPad.YPosition + _padSpeed, (int)MainCanvas.ActualHeight - padHeight);
            }
        }

        void HandleIncomingMessage(string message)
        {
            if (message.StartsWith("MOVE|"))
            {
                string[] parts = message.Substring(5).Split(':');
                string pad = parts[0];
                string direction = parts[1];

                if (_isHost)
                {
                    if (pad == "RIGHTPAD")
                    {
                        if (direction == "UP") _playerTwoUp = true;
                        else if (direction == "DOWN") _playerTwoDown = true;
                    }
                }
                else
                {
                    if (pad == "LEFTPAD")
                    {
                        if (direction == "W") _playerOneUp = true;
                        else if (direction == "S") _playerOneDown = true;
                    }
                }
            }
            else if (message.StartsWith("STOP|"))
            {
                string[] parts = message.Substring(5).Split(':');
                string pad = parts[0];
                string direction = parts[1];

                if (_isHost)
                {
                    if (pad == "RIGHTPAD")
                    {
                        if (direction == "UP") _playerTwoUp = false;
                        else if (direction == "DOWN") _playerTwoDown = false;
                    }
                }
                else
                {
                    if (pad == "LEFTPAD")
                    {
                        if (direction == "W") _playerOneUp = false;
                        else if (direction == "S") _playerOneDown = false;
                    }
                }
            }
            else if (message.StartsWith("PAD|"))
            {
                string[] parts = message.Substring(4).Split(';');
                if (parts.Length == 2)
                {
                    string pad = parts[0];
                    if (int.TryParse(parts[1], out int yPos))
                    {
                        if (pad == "LEFT")
                        {
                            _leftPad.YPosition = yPos;
                        }
                        else if (pad == "RIGHT")
                        {
                            _rightPad.YPosition = yPos;
                        }
                    }
                }
            }
            else if (message.StartsWith("BALL|") && !_isHost)
            {
                string[] parts = message.Substring(5).Split(';');
                if (parts.Length == 4)
                {
                    if (double.TryParse(parts[0], out double x) &&
                        double.TryParse(parts[1], out double y) &&
                        double.TryParse(parts[2], out double angle) &&
                        bool.TryParse(parts[3], out bool movingRight))
                    {
                        _ball.X = x;
                        _ball.Y = y;
                        _angle = angle;
                        _ball.MovingRight = movingRight;
                    }
                }
            }
            else if (message.StartsWith("SCORE|") && !_isHost)
            {
                string[] parts = message.Substring(6).Split(';');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int leftScore) &&
                    int.TryParse(parts[1], out int rightScore))
                {
                    _ball.LeftResult = leftScore;
                    _ball.RightResult = rightScore;
                }
            }
            else if (message.StartsWith("PAUSE|"))
            {
                bool pause = message.EndsWith("ON");
                _isPaused = pause;
                PauseLabel.Dispatcher.Invoke(() =>
                {
                    PauseLabel.Visibility = pause ? Visibility.Visible : Visibility.Hidden;
                });
            }
        }

        void StartReceivingMessages()
        {
            if (_udpClientHandler == null)
            {
                Debug.WriteLine("[OnlineGameWindow] _udpClientHandler null, nem lehet fogadni az üzeneteket.");
                return;
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        string message = await _udpClientHandler.ReceiveMessageAsync();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                HandleIncomingMessage(message);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[OnlineGameWindow] Hiba az üzenet fogadásakor: " + ex.Message);
                        break;
                    }
                }
            });
        }
    }
}

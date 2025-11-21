using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace SnakeWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary> Главное окно, используется для для общения между страницами
        public static MainWindow mainWindow;

        /// <summary> Модель данных для передачи IP адреса устройства, порта и никнейма
        public ViewModelUserSettings ViewModelUserSettings = new ViewModelUserSettings();

        /// <summary> Модель игрока в который передаются координаты змеи, яблок и точки
        public ViewModelGames ViewModelGames = null;

        /// <summary> Удалённый IP адрес для подключения к серверу
        public static IPAddress remoteIPAddress = IPAddress.Parse("127.0.0.1");

        /// <summary> Удалённый порт для подключения к серверу
        public static int remotePort = 5001;

        /// <summary> Основной поток для получения данных о игре
        public Thread tRec;

        /// <summary> UDP клиент для получения данных
        public UdpClient receivingUdpClient;

        /// <summary> Страница HOME
        public SnakeWPF.Pages.Home Home = new SnakeWPF.Pages.Home();

        
        public SnakeWPF.Pages.Game Game = new SnakeWPF.Pages.Game();
        /// <summary> Начинаем слушать ответы от сервера
        public void StartReceiver()
        {
            // Создаём поток для прослушивания канала
            tRec = new Thread(new ThreadStart(Receiver));
            // Запускаем поток
            tRec.Start();
        }
        /// <summary> Открываем страницу Home
        public void OpenPage(Page PageOpen)
        {
            // Создаём анимацию
            DoubleAnimation startAnimation = new DoubleAnimation();
            // Задаём начальное значение анимации
            startAnimation.From = 1;
            // Задаём конечное значение анимации
            startAnimation.To = 0;
            // Задаём время анимации
            startAnimation.Duration = TimeSpan.FromSeconds(0.6);
            // Подписываемся на выолнение анимации
            startAnimation.Completed += delegate
            {
                // Переключаем страницу
                frame.Navigate(PageOpen);
                // Создаём конечную анимацию
                DoubleAnimation endAnimation = new DoubleAnimation();
                // Задаём начальное значение анимации
                endAnimation.From = 0;
                // Задаём конечное значение анимации
                endAnimation.To = 1;
                // Задаём время анимации
                endAnimation.Duration = TimeSpan.FromSeconds(0.6);
                // Воспроизводим анимацию на frame, анимация прозрачности
                frame.BeginAnimation(OpacityProperty, endAnimation);
            };
            // Воспроизводим анимацию на frame, анимация прозрачности
            frame.BeginAnimation(OpacityProperty, startAnimation);
        }
        /// <summary> Прослушиваем канал
        public void Receiver()
        {
            // Создаём клиент для прослушивания
            receivingUdpClient = new UdpClient(int.Parse(ViewModelUserSettings.Port));
            // Конечная сетевая точка
            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                // Слушаем постоянно
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                        ref RemoteIpEndPoint);

                    // Преобразуем и отображаем данные
                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    // Если у нас не существует данных от сервера (значит мы тольконачали игру и нам необходимо сменить экран
                    if (ViewModelGames == null)
                    {
                        // Говорим что выполняем вне потока
                        Dispatcher.Invoke(() =>
                        {
                            // Открываем окно с игрой
                            OpenPage(Game);
                        });
                    }

                    // Конвертируем данные в модель
                    ViewModelGames = JsonConvert.DeserializeObject<ViewModelGames>(returnData.ToString());
                    // Если игрок проиграл
                    if (ViewModelGames.SnakesPlayers.GameOver)
                    {
                        // Выполняем вне потока
                        Dispatcher.Invoke(() =>
                        {
                            // Открываем окно с окончанием игры
                            OpenPage(new Pages.EndGame());
                        });
                    }
                    else
                    {
                        // Вызываем создание UI
                        Game.CreateUI();
                    }
                }
            }
            catch (Exception ex)
            {
                // Если что-то пошло не по плану, выводим ошибку в консоль проекта
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

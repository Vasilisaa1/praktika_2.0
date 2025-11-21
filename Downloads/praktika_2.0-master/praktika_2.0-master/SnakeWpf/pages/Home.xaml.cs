using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
using Newtonsoft.Json;
using SnakeWpf;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }
        /// <summary> Старт игры
        private void StartGame(object sender, RoutedEventArgs e)
        {
            // Если есть соединение
            if (MainWindow.mainWindow.receivingUdpClient != null)
            {
                // Закрываем
                MainWindow.mainWindow.receivingUdpClient.Close();
            }
            // Если есть поток
            if (MainWindow.mainWindow.tRec != null)
            {
                // Останавливаем
                MainWindow.mainWindow.tRec.Abort();
            }
            IPAddress UserIPAddress;
            // Если IP не преобразуется
            if (!IPAddress.TryParse(ip.Text, out UserIPAddress))
            {
                // Выводим ошибку
                MessageBox.Show("Please use the IP address in the format X.X.X.X.");
                return;
            }
            // Порт игрока
            int UserPort;
            // Если порт не преобразуется
            if (!int.TryParse(port.Text, out UserPort))
            {
                // Выводим ошибку
                MessageBox.Show("Please use the port as a number.");
                return;
            }

            // Запускаем поток на прослушку
            MainWindow.mainWindow.StartReceiver();
            // Запоминаем IP адрес игрока в модель
            MainWindow.mainWindow.ViewModelUserSettings.IPAddress = ip.Text;
            // Запоминаем порт игрока в модель
            MainWindow.mainWindow.ViewModelUserSettings.Port = port.Text;
            // Запоминаем никнейм игрока в модель
            MainWindow.mainWindow.ViewModelUserSettings.Name = name.Text;
            // Отправляем команду /start и сконвертированные данные в Json
            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.ViewModelUserSettings));
        }

    }
}

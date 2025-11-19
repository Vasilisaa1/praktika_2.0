using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;

namespace praktika_2
{
    internal class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress =new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int localPort= 5001;
        public static int MaxSpeed = 15;

        static void Main(string[] args)
        {
        }
        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress) {
                UdpClient sender= new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(User.IPAddress), int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdSnake == User.IdSnake)));
                    sender.Send(bytes, bytes.Length, endPoint);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение:" + ex.ToString() + "\n" + ex.Message);
                }
                finally
                {
                    sender.Close();


                }


            }

            }
        public static void Receiver() {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint RemoteIpEndPoint = null;
            try
            {
                Console.WriteLine("Команды сервера:");
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(
                        ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду:" + returnData.ToString());
                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.ToString().Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress}: {viewModelUserSettings.Port}");
                        remoteIPAddress.Add(viewModelUserSettings);
                        viewModelUserSettings.IdSnake = AddSnake();
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;

                    }
                    else
                    {
                        string[] dataMessage = returnData.ToString().Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        int IdPlayer = -1;
                        IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);

                        if (IdPlayer != -1)
                        {
                            if (dataMessage[0] == "Up" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                            else if (dataMessage[0] == "Down" && viewModelGames[IdPlayer].SnakesPlayers.direction!= Snakes.Direction.Up)
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                            else if (dataMessage[0] == "Left" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                            else if (dataMessage[0] == "Right" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }
        }
        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();

            // Указываем стартовые координаты змеи
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                // Точки змеи
                Points = new List<Snakes.Point>()
        {
            new Snakes.Point() { X = 30, Y = 10 },
            new Snakes.Point() { X = 20, Y = 10 },
            new Snakes.Point() { X = 10, Y = 10 },
        },
                // Направление змеи
                direction = Snakes.Direction.Start
            };

            // Создаём рандомную точку на карте
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));

            // Добавляем змей в список всех змей
            viewModelGames.Add(viewModelGamesPlayer);

            // Возвращаем ID змеи чтобы связать игрока и змею
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }
        /// <summary> Таймер с игрой (тут происходит перемещение змеи и обработка столкн ...
        public static void Timer()
        {
            while (true)
            {
                // останавливаем на 100 миллисекунд
                Thread.Sleep(100);

                // Получаем змей которых необходимо удалить
                List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);

                // Если змей больше чем 0
                if (RemoteSnakes.Count > 0)
                {
                    // Перебираем удаляемых змей
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        // Говорим что отклчён игрока
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.WriteLine($"Отключён пользовател: {remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAddress}" + $": {remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).Port}");

                        // Удаляем пользователя
                        remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);

                        // Удаляем змей которых необходимо удалить
                        viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
                    }
                }

                // Перебираем подключенных игроков
                foreach (ViewModelUserSettings User in remoteIPAddress)
                {
                    Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;

                    // Перемещаем точки змеи с конца в начало
                    for (int i = Snake.Points.Count - 1; i > 0; i--)
                    {
                        if (i != 0)
                        {
                            // Если к нам не пришла точка
                            // Перемещаем точку на месте предыдущей
                            Snake.Points[i] = Snake.Points[i - 1];
                        }
                        else 
                        {
                          // Получаем скорость змеи (Поскольку радиус точки 10, начальная скорость 10 пунктов)
                            int Speed = 10 + (int)Math.Round(Snake.Points.Count / 20f); 
                            // Если скорость змеи более максимальной скорости
                            if (Speed > MaxSpeed) Speed = MaxSpeed;
                            // Если направление змеи вправо
                            if (Snake.direction == Snakes.Direction.Right)
                            {
                                // Двигаем змеи вправо
                                Snake.Points[1] = new Snakes.Point() { X = Snake.Points[1].X + Speed, Y = Snake.Points[1].Y };
                            }
                            // Если направление змеи вниз
                            else if (Snake.direction == Snakes.Direction.Down)
                            {
                                // Двигаем вниз
                                Snake.Points[1] = new Snakes.Point() { X = Snake.Points[1].X, Y = Snake.Points[1].Y + Speed };
                            }

                            // Если направление змеи вправо
                            else if (Snake.direction == Snakes.Direction.Up)
                            {
                                // Двигаем вверх
                                Snake.Points[1] = new Snakes.Point() { X = Snake.Points[1].X, Y = Snake.Points[1].Y - Speed };
                            }
                            // Если направление змеи влево
                            else if (Snake.direction == Snakes.Direction.Left)
                            {
                                Snake.Points[1] = new Snakes.Point() { X = Snake.Points[1].X - Speed, Y = Snake.Points[1].Y };
                            }
                        }
                    }

                    // проверяем выход столкновение с препятствием
                    // если первая точка змеи вышла за координаты экрана по горизонтали
                    if (Snake.Points[0].X <= 0 || Snake.Points[0].X > 793)
                    {
                        // Говорим что игра окончена
                        Snake.GameOver = true;
                    }
                    // если первая точка змеи вышла за координаты экрана по вертикали
                    else if (Snake.Points[0].Y <= 0 || Snake.Points[0].Y > 420)
                    {
                        // Говорим что игра окончена
                        Snake.GameOver = true;
                    }

                    // проверяем что мы не столкнулись сами с собой
                    if (Snake.direction != Snakes.Direction.Start)
                    {
                        // Прогоняем все точки кроме первой
                        for (int i = 1; i < Snake.Points.Count; i++)
                        {
                            // Если первая точка находится в координатах последующей по горизонтали
                            if (Snake.Points[0].X == Snake.Points[i].X-1 && Snake.Points[0].X == Snake.Points[i].X + 1)
                            {
                               // Если первая точка находится в координатах по вертикали
                                    if (Snake.Points[0].Y == Snake.Points[i].Y-1 && Snake.Points[0].Y == Snake.Points[i].Y + 1)
                                    {
                                        // Говорим что игра окончена
                                        Snake.GameOver = true;
                                        break;
                                    }
                            }
                        }
                    }

                    // Проверяем что если первая точка змеи игрока находится в координатах яблока по горизонтали
                    if (Snake.Points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                        Snake.Points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                    {
                        // Проверяем что если первая точка змеи игрока находится в координатах яблока по вертикали
                        if (Snake.Points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                            Snake.Points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                        {
                            // Если собрали яблоко
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(
                                new Random().Next(10, 783), 
                                new Random().Next(10, 410));

                            // Добавляем новую точку на координаты последней
                            Snake.Points.Add(new Snakes.Point()
                            {
                                X = Snake.Points[Snake.Points.Count - 1].X,
                                Y = Snake.Points[Snake.Points.Count - 1].Y
                            });

                            // загружаем таблицу
                            LoadLeaders();
                            // добавляем нас в таблицу
                            Leaders.Add(new Leaders()
                            {
                                Name = User.Name,
                                Points = Snake.Points.Count - 3
                            });

                            // сортируем таблицу по двум значениям сначала по кол-ву точек затем по наименованию
                            Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                            // Ищем в списке и записываем в модели змеи
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top =
                                Leaders.FindIndex(x => x.Points == Snake.Points.Count - 3 && x.Name == User.Name) + 1;
                        }
                    }

                    // Если игра для змеи закончена
                    if (Snake.GameOver)
                    {
                        // Загружаем таблицу
                        LoadLeaders();
                        // добавляем нас в таблицу
                        Leaders.Add(new Leaders()
                        {
                            // Указываем имя
                            Name = User.Name,
                            // Указываем кол-во яблок которое собрал пользователь
                            Points = Snake.Points.Count - 3
                        });

                        // Сохраняем результаты
                        SaveLeaders();
                    }
                    // Рассылаем пользователям ответ
                    Send();
                }
            }
        }
        /// <summary> Сохранение результата
        public static void SaveLeaders()
        {
            // Преобразуем данные игроков в JSON
            string json = JsonConvert.SerializeObject(Leaders);
            // Записываем в файл
            StreamWriter SW = new StreamWriter("./leaders.txt");
            // Пишем строку
            SW.WriteLine(json);
            // Закрываем файл
            SW.Close();
        }
        /// <summary> Загрузка результата
        public static void LoadLeaders()
        {
            // Проверяем что есть файл
            if (File.Exists("./leaders.txt"))
            {
                // Открываем файл
                StreamReader SR = new StreamReader("./leaders.txt");
                // читаем первую строку
                string json = SR.ReadLine();
                // Закрываем файл
                SR.Close();
                // Если есть что читать
                if (!string.IsNullOrEmpty(json))
                {
                    // Преобразуем строку в объект
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
                else
                {
                    // Возвращаем пустой результат
                    Leaders = new List<Leaders>();
                }
            }
            else
            {
                // Возвращаем пустой результат
                Leaders = new List<Leaders>();
            }
        }


    }
}

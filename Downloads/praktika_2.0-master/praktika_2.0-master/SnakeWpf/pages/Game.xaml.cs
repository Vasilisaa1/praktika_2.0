using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using SnakeWpf;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public int StepCad = 0;    
        public Game()
        {
            InitializeComponent();
        }
        public void CreateUI()
        {
            // Выполняем вне потока
            Dispatcher.Invoke(() =>
            {
                // Если кадр то кадр 1
                if (StepCad == 0) StepCad = 1;
                // Если кадр 0
                else StepCad = 0;
                // Чистим интерфейс
                canvas.Children.Clear();
                // Перебираем точки змеи
                for (int iPoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 1; iPoint >= 0; iPoint--)
                {
                    // Получаем точку
                    Snakes.Point SnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint];

                    // Смещение точек змеи
                    if (iPoint != 0)
                    {
                        // Получаем следующую точку змеи
                        Snakes.Point NextSnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint - 1];
                        // Если точка находится по горизонтали
                        if (SnakePoint.X > NextSnakePoint.X || SnakePoint.X < NextSnakePoint.X)
                        {
                            // если точка чётная
                            if (iPoint % 2 == 0)
                            {
                                // если кадр чётный
                                if (StepCad % 2 == 0)
                                {
                                    // смещаем в одну сторону
                                    SnakePoint.Y -= 1;
                                }
                                else
                                {
                                    // смещаем в другую сторону
                                    SnakePoint.Y += 1;
                                }
                            }
                            else
                            {
                                // если кадр не чётный
                                if (StepCad % 2 == 0)
                                {
                                    // смещаем в одну сторону
                                    SnakePoint.Y += 1;
                                }
                                else
                                {
                                    // смещаем в другую сторону
                                    SnakePoint.Y -= 1;
                                }
                            }
                        }
                        // Если точка находится по вертикали
                        else if (SnakePoint.Y > NextSnakePoint.Y || SnakePoint.Y < NextSnakePoint.Y)
                        {
                            // если точка чётная
                            if (iPoint % 2 == 0)
                            {
                                // если кадр чётный
                                if (StepCad % 2 == 0)
                                {
                                    SnakePoint.X -= 1;
                                }
                                else
                                {
                                    SnakePoint.X += 1;
                                }
                            }
                            else
                            {
                                // если кадр не чётный
                                if (StepCad % 2 == 0)
                                {
                                    SnakePoint.X += 1;
                                }
                                else
                                {
                                    SnakePoint.X -= 1;
                                }
                            }
                        }
                    }
                    // Цвет для точки
                    Brush Color;
                    // Если первая точка
                    if (iPoint == 0)
                        // Тёмно зелёный
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 127, 14));
                    else
                        // Светло зелёный
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 198, 19));

                    // Рисуем точку
                    Ellipse ellipse = new Ellipse()
                    {
                        // Ширина
                        Width = 20,
                        // Высота
                        Height = 20,
                        // Отступ
                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        // Цвет точки
                        Fill = Color,
                        // Ободка
                        Stroke = Brushes.Black
                    };
                    // Добавляем на canvas
                    canvas.Children.Add(ellipse);
                }

                // Отрисока яблока
                // Изображение яблока
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Image/Apple.png"));
                // яблока на UI
                Ellipse points = new Ellipse()
                {
                    // Ширина
                    Width = 40,
                    // Высота
                    Height = 40,
                    // Отступ (-20 центрирование яблока)
                    Margin = new Thickness(
                        MainWindow.mainWindow.ViewModelGames.Points.X - 20,
                        MainWindow.mainWindow.ViewModelGames.Points.Y - 20, 0, 0),
                    // Заливка картинкой
                    Fill = myBrush
                };
                // Добавляем на сцену
                canvas.Children.Add(points);
            });
        }

    }
}

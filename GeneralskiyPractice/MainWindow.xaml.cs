using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace GeneralskiyPractice
{
    public partial class MainWindow : Window
    {
        private List<DataPoint> dataPoints;
        private Random random = new Random();
        private DispatcherTimer timer;
        private int canvasWidth = 500;
        private int canvasHeight = 300;
        private double maxY = 800;
        private double minY = 700;
        private double interval = 10;
        private bool isRunning = true;
        private double pressureRange = 30;  // Максимальный разброс давления
        private double initialPressure = 740;  // Начальное давление

        private Line averageLine;

        public MainWindow()
        {
            InitializeComponent();

            dataPoints = new List<DataPoint>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private double Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                if (timer != null)  // Если таймер уже создан, измените его интервал
                {
                    timer.Interval = TimeSpan.FromMilliseconds(interval);
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isRunning)
                return;

            double previousPressure = dataPoints.Any() ? dataPoints.Last().CurrentPressure : minY;
            double currentPressure = GetNextCurrentPressure(previousPressure);
            double averagePressure = CalculateAveragePressure();

            dataPoints.Add(new DataPoint { CurrentPressure = currentPressure, AveragePressure = averagePressure });

            if (dataPoints.Count > canvasWidth)
                dataPoints.RemoveAt(0);

            DrawChart();

            UpdateTextBlocks();
        }


        private double GetNextCurrentPressure(double previousPressure)
        {
            double pressureChange = (random.NextDouble() * 2 - 1) * pressureRange; // Изменение в пределах [-pressureRange, pressureRange]
            double currentPressure = previousPressure + pressureChange;

            // Убедимся, что давление остается в пределах minY и maxY
            currentPressure = Math.Max(minY, Math.Min(maxY, currentPressure));

            return currentPressure;
        }



        private double CalculateAveragePressure()
        {
            if (!dataPoints.Any())
                return 0;

            return dataPoints.Select(p => p.CurrentPressure).Average();
        }

        private void DrawChart()
        {
            pressureCanvas.Children.Clear();

            double scaleX = canvasWidth / (double)dataPoints.Count;
            double scaleY = canvasHeight / (maxY - minY);

            // Ось Y
            DrawLine(0, 0, 0, canvasHeight, Brushes.Black, pressureCanvas);

            // Ось X
            DrawLine(0, canvasHeight, canvasWidth, canvasHeight, Brushes.Black, pressureCanvas);

            // Вертикальная линия измерения
            double measurementLineX = 100 * scaleX;
            DrawLine(measurementLineX, 0, measurementLineX, canvasHeight, Brushes.Black, pressureCanvas);

            for (int i = 1; i < dataPoints.Count; i++)
            {
                double x1 = (i - 1) * scaleX;
                double y1 = canvasHeight - (dataPoints[i - 1].CurrentPressure - minY) * scaleY;
                double x2 = i * scaleX;
                double y2 = canvasHeight - (dataPoints[i].CurrentPressure - minY) * scaleY;

                DrawLine(x1, y1, x2, y2, Brushes.Blue, pressureCanvas);
            }

            // Обновление вертикальной линии измерения
            Canvas.SetLeft(measurementLine, measurementLineX);
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Brush color, Canvas canvas)
        {
            Line line = new Line();
            line.Stroke = color;
            line.StrokeThickness = 2;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            canvas.Children.Add(line);
        }

        private void UpdateTextBlocks()
        {
            if (dataPoints.Any())
            {
                double currentPressure = dataPoints.Last().CurrentPressure;
                double averagePressure = dataPoints.Last().AveragePressure;

                currentPressureText.Text = $"Текущее значение: {currentPressure:F2}";
                averagePressureText.Text = isRunning ? string.Empty : $"{averagePressure:F2}";
            }
        }

        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            isRunning = !isRunning;
            startStopButton.Content = isRunning ? "Остановить" : "Продолжить";
        }

        private void CalculateAverageButtonClick(object sender, RoutedEventArgs e)
        {
            double averagePressure = CalculateAveragePressure();
            MessageBox.Show($"Среднее значение мм.рт.ст. в Туле - {averagePressure:F2}", "Среднее значение");
        }
    }

    public class DataPoint
    {
        public double CurrentPressure { get; set; }
        public double AveragePressure { get; set; }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractalLab.Midpoint
{
    // Задание 2: визуализация горного массива алгоритмом midpoint displacement.
    // Кнопки Шаг + и Шаг - показывают результаты последовательных шагов
    // алгоритма. Все параметры (высоты краев, разброс, затухание) можно менять
    // прямо во время работы, ломаная сразу перестраивается.
    public partial class MidpointPage : UserControl
    {
        private const int MaxSteps = 13; // 2^13 + 1 = 8193 точки, этого хватает

        private MidpointDisplacement _algo;
        private int _steps = 3; // показываем сразу несколько шагов, чтобы было что смотреть
        private int _seed;

        public MidpointPage()
        {
            InitializeComponent();
            _seed = Environment.TickCount; // при запуске случайное зерно
            Rebuild();
        }

        // Пересобирает ломаную с текущими параметрами и текущим числом шагов.
        // Одно и то же зерно дает одну и ту же гору, поэтому кнопка Шаг - честно
        // возвращает предыдущее состояние, а не рисует новое случайное.
        private void Rebuild()
        {
            double left = TextParser.ReadDouble(LeftHeightBox, 80, -500, 500);
            double right = TextParser.ReadDouble(RightHeightBox, 30, -500, 500);
            double spread = TextParser.ReadDouble(SpreadBox, 140, 0, 500);
            double decay = TextParser.ReadDouble(FactorBox, 0.55, 0.1, 0.95);

            _algo = new MidpointDisplacement(left, right, spread, decay, _seed);
            for (int i = 0; i < _steps; i++)
                _algo.Step();

            StatusText.Text = "Шаг: " + _steps + " из " + MaxSteps +
                              ", точек в ломаной: " + _algo.Points.Count;
            Redraw();
        }

        private void StepPlusClick(object sender, RoutedEventArgs e)
        {
            if (_steps >= MaxSteps)
            {
                StatusText.Text = "Достигнут максимум шагов (" + MaxSteps + ").";
                return;
            }
            _steps++;
            Rebuild();
        }

        private void StepMinusClick(object sender, RoutedEventArgs e)
        {
            if (_steps <= 0) return;
            _steps--;
            Rebuild();
        }

        private void RestartClick(object sender, RoutedEventArgs e)
        {
            _seed = new Random().Next();
            Rebuild();
        }

        private void Param_TextChanged(object sender, TextChangedEventArgs e)
        {
            // срабатывает при изменении любого числового поля
            if (_algo != null) Rebuild();
        }

        private void Redraw()
        {
            DrawCanvas.Children.Clear();
            if (_algo == null || DrawCanvas.ActualWidth < 10) return;

            double w = DrawCanvas.ActualWidth;
            double h = DrawCanvas.ActualHeight;
            var toScreen = WorldToScreen.Fit(_algo.Points, w, h, 20);

            // гора рисуется замкнутой фигурой: ломаная сверху и линия по низу окна
            var mountain = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromRgb(96, 96, 112)),
                Stroke = Brushes.Black,
                StrokeThickness = 1.5,
                StrokeLineJoin = PenLineJoin.Round
            };
            foreach (var p in _algo.Points)
                mountain.Points.Add(toScreen.ToScreen(p.X, p.Y));
            mountain.Points.Add(new Point(w, h));
            mountain.Points.Add(new Point(0, h));
            DrawCanvas.Children.Add(mountain);
        }

        private void DrawCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractalLab.LSystems
{
    // Задание 1.б: фрактальное дерево на базе L-системы из 1.a.
    // Дерево строится той же черепашкой, разница только в отрисовке:
    // толщина линии и цвет зависят от глубины ветки. Чем глубже ветка
    // (больше вложенных скобок), тем она тоньше и зеленее.
    public partial class TreePage : UserControl
    {
        private List<Segment> _segments;

        public TreePage()
        {
            InitializeComponent();
            foreach (var preset in LSystemPresets.Trees)
                TreePresetCombo.Items.Add(preset);
            TreePresetCombo.SelectedIndex = 0;
        }

        private void TreePreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TreePresetCombo.SelectedItem is LSystemPresets.Preset preset)
            {
                AngleBox.Text = preset.Angle.ToString();
                IterationsBox.Text = preset.Iterations.ToString();
            }
        }

        private void DrawClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var preset = (LSystemPresets.Preset)TreePresetCombo.SelectedItem;
                if (preset == null)
                {
                    ShowStatus("Выберите вид дерева.");
                    return;
                }

                var system = LSystem.Parse(preset.Text);
                // угол берем из поля на панели, чтобы его можно было менять
                system.Angle = TextParser.ReadDouble(AngleBox, system.Angle, 5, 60);
                int iterations = TextParser.ReadInt(IterationsBox, 5, 1, 6);
                int randomness = TextParser.ReadInt(RandomBox, 15, 0, 100);

                var rnd = new Random();
                string commands = system.Expand(iterations, rnd);
                if (commands.Length > 2000000)
                {
                    ShowStatus("Слишком много итераций, уменьшите число.");
                    return;
                }

                _segments = Turtle.Interpret(commands, system, randomness, rnd);
                if (_segments.Count == 0)
                {
                    ShowStatus("Не получилось ни одной ветки.");
                    return;
                }

                Redraw();
                ShowStatus("Нарисовано ветвей: " + _segments.Count + ".");
            }
            catch (FormatException ex)
            {
                ShowStatus("Ошибка: " + ex.Message);
            }
        }

        private void Redraw()
        {
            DrawCanvas.Children.Clear();
            if (_segments == null || _segments.Count == 0 || DrawCanvas.ActualWidth < 10) return;

            var all = new List<Point>(_segments.Count * 2);
            foreach (var s in _segments)
            {
                all.Add(new Point(s.X1, s.Y1));
                all.Add(new Point(s.X2, s.Y2));
            }
            var toScreen = WorldToScreen.Fit(all, DrawCanvas.ActualWidth, DrawCanvas.ActualHeight, 15);

            int maxDepth = 0;
            foreach (var s in _segments)
                if (s.Depth > maxDepth) maxDepth = s.Depth;

            // кисть и толщину для каждой глубины готовим заранее, чтобы не
            // создавать тысячи одинаковых кистей
            var brushes = new SolidColorBrush[maxDepth + 1];
            var widths = new double[maxDepth + 1];
            for (int d = 0; d <= maxDepth; d++)
            {
                // k = 0 у ствола, k = 1 у самых тонких веток
                double k = maxDepth == 0 ? 0 : (double)d / maxDepth;
                byte r = (byte)(110 + (40 - 110) * k);  // коричневый -> зеленый
                byte g = (byte)(70 + (150 - 70) * k);
                brushes[d] = new SolidColorBrush(Color.FromRgb(r, g, 40));
                brushes[d].Freeze();
                // каждая следующая глубина в 0.7 раза тоньше, но не меньше 0.7 px
                widths[d] = Math.Max(0.7, 9.0 * Math.Pow(0.7, d));
            }

            foreach (var s in _segments)
            {
                Point a = toScreen.ToScreen(s.X1, s.Y1);
                Point b = toScreen.ToScreen(s.X2, s.Y2);
                DrawCanvas.Children.Add(new Line
                {
                    X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y,
                    Stroke = brushes[s.Depth],
                    StrokeThickness = widths[s.Depth],
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                });
            }
        }

        private void DrawCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void ShowStatus(string text)
        {
            StatusText.Text = text;
        }
    }
}

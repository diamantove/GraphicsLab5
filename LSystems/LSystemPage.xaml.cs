using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
// в этом файле Path это фигура для рисования, а не System.IO.Path
using Path = System.Windows.Shapes.Path;

namespace FractalLab.LSystems
{
    // Задание 1.a: построение фрактальных узоров по описанию L-системы.
    // Описание берётся из встроенного примера, из txt файла или вводится руками
    // в текстовое поле. Сама математика сидит в LSystem и Turtle, здесь только
    // интерфейс и отрисовка.
    public partial class LSystemPage : UserControl
    {
        private List<Segment> _segments;

        public LSystemPage()
        {
            InitializeComponent();
            foreach (var preset in LSystemPresets.All)
                PresetCombo.Items.Add(preset);
            PresetCombo.SelectedIndex = 0; // вызовет загрузку описания в текстовое поле
        }

        private void PresetCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetCombo.SelectedItem is LSystemPresets.Preset preset)
            {
                DescriptionBox.Text = preset.Text;
                IterationsBox.Text = preset.Iterations.ToString();
                RandomBox.Text = "0";
            }
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Title = "Открыть описание L-системы"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DescriptionBox.Text = File.ReadAllText(dialog.FileName);
                }
                catch (Exception)
                {
                    ShowStatus("Не удалось прочитать файл.");
                }
            }
        }

        private void BuildClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var system = LSystem.Parse(DescriptionBox.Text);
                int iterations = TextParser.ReadInt(IterationsBox, 4, 0, 8);
                int randomness = TextParser.ReadInt(RandomBox, 0, 0, 100);

                var rnd = new Random();
                string commands = system.Expand(iterations, rnd);
                if (commands.Length > 2000000)
                {
                    ShowStatus("Строка получилась слишком большой, уменьшите число итераций.");
                    return;
                }

                _segments = Turtle.Interpret(commands, system, randomness, rnd);
                if (_segments.Count == 0)
                {
                    DrawCanvas.Children.Clear();
                    ShowStatus("Не нарисовано ни одного отрезка. Проверьте рисующие символы в первой строке.");
                    return;
                }

                ShowStatus("Построено отрезков: " + _segments.Count +
                           ", длина итоговой строки: " + commands.Length + ".");
                Redraw();
            }
            catch (FormatException ex)
            {
                ShowStatus("Ошибка в описании: " + ex.Message);
            }
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            _segments = null;
            DrawCanvas.Children.Clear();
            ShowStatus("");
        }

        private void Redraw()
        {
            DrawCanvas.Children.Clear();
            if (_segments == null || _segments.Count == 0 || DrawCanvas.ActualWidth < 10) return;

            // собираем все точки, чтобы узнать реальные размеры картинки
            var all = new List<Point>(_segments.Count * 2);
            foreach (var s in _segments)
            {
                all.Add(new Point(s.X1, s.Y1));
                all.Add(new Point(s.X2, s.Y2));
            }
            var toScreen = WorldToScreen.Fit(all, DrawCanvas.ActualWidth, DrawCanvas.ActualHeight, 15);

            // все отрезки складываем в одну геометрию: так WPF рисует даже
            // десятки тысяч отрезков без заметных тормозов
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                foreach (var s in _segments)
                {
                    context.BeginFigure(toScreen.ToScreen(s.X1, s.Y1), false, false);
                    context.LineTo(toScreen.ToScreen(s.X2, s.Y2), true, false);
                }
            }
            geometry.Freeze();
            DrawCanvas.Children.Add(new Path
            {
                Data = geometry,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractalLab.Bezier
{
    // Задание 3: визуализация составной кубической кривой Безье.
    // Опорные точки добавляются кликом по пустому месту, перемещаются мышью,
    // удаляются правым кликом. Каждый сегмент кривой строится по четырем
    // последовательным точкам, соседние сегменты имеют общую точку.
    // Касательные не редактируются, только сами точки.
    public partial class BezierPage : UserControl
    {
        private const double HitRadius = 10;

        private readonly List<Point> _points = new List<Point>();
        private int _dragIndex = -1;

        public BezierPage()
        {
            InitializeComponent();
            Loaded += BezierPage_Loaded;
        }

        private void BezierPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_points.Count == 0) LoadSample();
            Redraw();
        }

        // стартовый набор из семи точек (два сегмента), чтобы сразу было что посмотреть
        private void LoadSample()
        {
            double w = Math.Max(DrawCanvas.ActualWidth, 300);
            double h = Math.Max(DrawCanvas.ActualHeight, 200);
            _points.Clear();
            _points.Add(new Point(w * 0.12, h * 0.70));
            _points.Add(new Point(w * 0.30, h * 0.05));
            _points.Add(new Point(w * 0.32, h * 0.75));
            _points.Add(new Point(w * 0.50, h * 0.50));
            _points.Add(new Point(w * 0.68, h * 0.25));
            _points.Add(new Point(w * 0.70, h * 0.95));
            _points.Add(new Point(w * 0.88, h * 0.30));
        }

        // ищем опорную точку под курсором (в радиусе HitRadius пикселей)
        private int HitPoint(Point p)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                double dx = _points[i].X - p.X;
                double dy = _points[i].Y - p.Y;
                if (dx * dx + dy * dy <= HitRadius * HitRadius) return i;
            }
            return -1;
        }

        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DrawCanvas);
            int index = HitPoint(p);
            if (index >= 0)
            {
                _dragIndex = index;          // схватили точку, будем перетаскивать
                DrawCanvas.CaptureMouse();
            }
            else
            {
                _points.Add(p);              // клик по пустому месту добавляет точку в конец
                Redraw();
            }
            e.Handled = true;
        }

        private void CanvasMove(object sender, MouseEventArgs e)
        {
            if (_dragIndex < 0) return;
            _points[_dragIndex] = e.GetPosition(DrawCanvas);
            Redraw();
        }

        private void CanvasUp(object sender, MouseButtonEventArgs e)
        {
            _dragIndex = -1;
            DrawCanvas.ReleaseMouseCapture();
        }

        private void CanvasRightDown(object sender, MouseButtonEventArgs e)
        {
            int index = HitPoint(e.GetPosition(DrawCanvas));
            if (index >= 0)
            {
                _points.RemoveAt(index);
                Redraw();
            }
            e.Handled = true;
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            _points.Clear();
            Redraw();
        }

        private void SampleClick(object sender, RoutedEventArgs e)
        {
            LoadSample();
            Redraw();
        }

        private void Redraw()
        {
            DrawCanvas.Children.Clear();

            // опорный многоугольник пунктиром: видно, из каких точек строится кривая
            if (_points.Count >= 2)
            {
                var polygon = new Polyline
                {
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                foreach (var p in _points) polygon.Points.Add(p);
                DrawCanvas.Children.Add(polygon);
            }

            // сегменты кривой: точки 0-3, потом 3-6, потом 6-9 и так далее
            int segmentCount = 0;
            for (int i = 0; i + 3 < _points.Count; i += 3)
            {
                var curve = new Polyline { Stroke = Brushes.Firebrick, StrokeThickness = 2 };
                foreach (var p in BezierSpline.SampleSegment(
                    _points[i], _points[i + 1], _points[i + 2], _points[i + 3], 60))
                {
                    curve.Points.Add(p);
                }
                DrawCanvas.Children.Add(curve);
                segmentCount++;
            }

            // точки: синие уже входят в готовый сегмент, оранжевые ждут следующих точек
            int lastUsed = 3 * segmentCount;
            for (int i = 0; i < _points.Count; i++)
            {
                bool used = i <= lastUsed;
                var dot = new Ellipse { Width = 9, Height = 9 };
                dot.Fill = used ? Brushes.SteelBlue : Brushes.Orange;
                dot.Stroke = Brushes.White;
                Canvas.SetLeft(dot, _points[i].X - 4.5);
                Canvas.SetTop(dot, _points[i].Y - 4.5);
                DrawCanvas.Children.Add(dot);

                var label = new TextBlock { Text = i.ToString(), FontSize = 10, Foreground = Brushes.Gray };
                Canvas.SetLeft(label, _points[i].X + 7);
                Canvas.SetTop(label, _points[i].Y - 8);
                DrawCanvas.Children.Add(label);
            }

            StatusText.Text = "Точек: " + _points.Count +
                              ", сегментов кривой: " + segmentCount +
                              ". До нового сегмента нужно " + NeededPointsText(segmentCount) + ".";
        }

        // подсказка, сколько точек не хватает до следующего полного сегмента
        private string NeededPointsText(int segmentCount)
        {
            int need = 3 * (segmentCount + 1) + 1 - _points.Count;
            if (need <= 0) return "добавить 3 новые точки (или более)";
            return "еще " + need + " клик(ов)";
        }

        private void DrawCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded) Redraw();
        }
    }
}

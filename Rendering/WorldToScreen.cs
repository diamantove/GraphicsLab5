using System;
using System.Collections.Generic;
using System.Windows;

namespace FractalLab
{
    // Переводит мировые координаты (ось Y вверх, как в математике) в экранные
    // координаты Canvas (ось Y вниз) и заодно подбирает масштаб так, чтобы вся
    // картинка целиком поместилась в окно. Это нужно потому, что фракталы могут
    // быть и крошечными, и огромными, а шаг черепашки всегда равен единице.
    public class WorldToScreen
    {
        private readonly double _scale, _shiftX, _shiftY, _height;

        private WorldToScreen(double scale, double shiftX, double shiftY, double height)
        {
            _scale = scale;
            _shiftX = shiftX;
            _shiftY = shiftY;
            _height = height;
        }

        // Подбирает преобразование по списку всех точек картинки.
        // margin это отступ от краёв окна в пикселях.
        public static WorldToScreen Fit(IReadOnlyList<Point> points, double width, double height, double margin)
        {
            double minX = 0, maxX = 1, minY = 0, maxY = 1;
            if (points != null && points.Count > 0)
            {
                minX = maxX = points[0].X;
                minY = maxY = points[0].Y;
                foreach (var p in points)
                {
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }

            // если все точки одинаковые, раздвигаем диапазон, чтобы не делить на ноль
            if (maxX - minX < 1e-9) { minX -= 1; maxX += 1; }
            if (maxY - minY < 1e-9) { minY -= 1; maxY += 1; }

            double freeW = Math.Max(width - 2 * margin, 1);
            double freeH = Math.Max(height - 2 * margin, 1);
            double scale = Math.Min(freeW / (maxX - minX), freeH / (maxY - minY));

            // сдвиги подобраны так, чтобы картинка стояла по центру окна
            double shiftX = (width - (maxX - minX) * scale) / 2 - minX * scale;
            double shiftY = (height - (maxY - minY) * scale) / 2 - minY * scale;
            return new WorldToScreen(scale, shiftX, shiftY, height);
        }

        public Point ToScreen(double x, double y)
        {
            // по X оси направлены одинаково, а у экрана Y растёт вниз,
            // поэтому вычитаем из высоты окна
            return new Point(x * _scale + _shiftX, _height - (y * _scale + _shiftY));
        }
    }
}

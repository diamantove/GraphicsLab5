using System;
using System.Collections.Generic;
using System.Windows;

namespace FractalLab.Bezier
{
    // Составная кубическая кривая Безье.
    // Один сегмент кривой задается четырьмя опорными точками. Соседние сегменты
    // имеют одну общую точку:
    //   P0 P1 P2 P3 | P3 P4 P5 P6 | P6 P7 P8 P9 ...
    // Кривая не проходит через промежуточные точки, они только притягивают ее.
    // Управление идет только опорными точками, касательных в программе нет.
    public static class BezierSpline
    {
        // Точка сегмента при значении параметра t от 0 до 1.
        // Это многочлен Бернштейна:
        //   B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
        // при t = 0 получаем P0, при t = 1 получаем P3, поэтому соседние
        // сегменты точно стыкуются в общих точках.
        public static Point PointAt(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double u = 1 - t;
            double w0 = u * u * u;      // вес точки P0
            double w1 = 3 * u * u * t;  // вес точки P1
            double w2 = 3 * u * t * t;  // вес точки P2
            double w3 = t * t * t;      // вес точки P3
            return new Point(
                p0.X * w0 + p1.X * w1 + p2.X * w2 + p3.X * w3,
                p0.Y * w0 + p1.Y * w1 + p2.Y * w2 + p3.Y * w3);
        }

        // Точки ломаной для отрисовки одного сегмента (просто идем по t от 0 до 1).
        public static List<Point> SampleSegment(Point p0, Point p1, Point p2, Point p3, int steps)
        {
            var points = new List<Point>(steps + 1);
            for (int i = 0; i <= steps; i++)
                points.Add(PointAt(p0, p1, p2, p3, (double)i / steps));
            return points;
        }

        // Сколько полных сегментов можно построить из списка точек:
        // каждый следующий сегмент использует три новые точки и одну общую.
        public static int SegmentCount(int pointCount)
        {
            if (pointCount < 4) return 0;
            return (pointCount - 1) / 3;
        }
    }
}

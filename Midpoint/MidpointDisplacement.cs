using System;
using System.Collections.Generic;
using System.Windows;

namespace FractalLab.Midpoint
{
    // Алгоритм midpoint displacement для горного массива.
    // Идея: начинаем с ломаной из двух точек (левый и правый край). На каждом
    // шаге между каждой парой соседних точек вставляется середина, сдвинутая
    // по вертикали на случайную величину. Амплитуда сдвига с каждым шагом
    // уменьшается в decay раз, поэтому сначала появляются крупные черты
    // рельефа, а на следующих шагах добавляются всё более мелкие детали.
    public class MidpointDisplacement
    {
        public const double WorldWidth = 1000.0; // ширина массива в условных единицах

        private readonly double _amplitude; // амплитуда сдвига на первом шаге
        private readonly double _decay;     // во сколько раз амплитуда падает за шаг
        private readonly Random _rnd;

        public List<Point> Points { get; private set; }
        public int StepCount { get; private set; }

        public MidpointDisplacement(double leftHeight, double rightHeight,
                                    double amplitude, double decay, int seed)
        {
            _amplitude = amplitude;
            _decay = decay;
            _rnd = new Random(seed); // фиксированное зерно = повторяемый результат

            Points = new List<Point>
            {
                new Point(0, leftHeight),
                new Point(WorldWidth, rightHeight)
            };
        }

        // Один шаг: во каждое звено ломаной вставляем смещенную середину.
        public void Step()
        {
            // на шаге StepCount работаем с амплитудой amplitude * decay^StepCount
            double amp = _amplitude * Math.Pow(_decay, StepCount);

            var next = new List<Point>(Points.Count * 2);
            for (int i = 0; i < Points.Count - 1; i++)
            {
                Point a = Points[i];
                Point b = Points[i + 1];
                next.Add(a);

                // случайное число от -amp до +amp
                double shift = (_rnd.NextDouble() * 2 - 1) * amp;
                next.Add(new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2 + shift));
            }
            next.Add(Points[Points.Count - 1]);

            Points = next;
            StepCount++;
        }
    }
}

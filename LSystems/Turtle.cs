using System;
using System.Collections.Generic;

namespace FractalLab.LSystems
{
    // Один отрезок, который нарисовала черепашка.
    // Depth это глубина вложенности скобок: 0 у основного ствола, и чем она
    // больше, тем тоньше ветка. Используется при отрисовке дерева в задании 1.б.
    public class Segment
    {
        public double X1, Y1, X2, Y2;
        public int Depth;
    }

    // Черепашья графика: превращает развёрнутую строку L-системы в отрезки.
    // Команды:
    //   рисующий символ (обычно F) = шаг вперёд, оставляя отрезок
    //   + = повернуть на угол системы против часовой стрелки
    //   - = повернуть на угол системы по часовой стрелке
    //   [ = запомнить текущее состояние, ] = вернуться к сохранённому
    // Скобки и дают ветвление: после ] черепашка возвращается в развилку.
    //
    // Координаты считаются как в математике: ось Y вверх, угол отсчитывается
    // от оси X против часовой стрелки. Перевод в экранные координаты делает
    // отдельный класс WorldToScreen.
    public static class Turtle
    {
        public static List<Segment> Interpret(string commands, LSystem system, double randomnessPercent, Random rnd)
        {
            var segments = new List<Segment>();
            double x = 0, y = 0;
            double angle = system.StartAngle * Math.PI / 180.0;
            double delta = system.Angle * Math.PI / 180.0;
            int depth = 0;
            var stack = new Stack<double[]>(); // сохранённое состояние: x, y, угол, глубина

            foreach (char c in commands)
            {
                if (c == '+')
                {
                    angle += delta + RandomOffset(delta, randomnessPercent, rnd);
                }
                else if (c == '-')
                {
                    angle -= delta + RandomOffset(delta, randomnessPercent, rnd);
                }
                else if (c == '[')
                {
                    stack.Push(new[] { x, y, angle, depth });
                    depth++; 
                }
                else if (c == ']')
                {
                    if (stack.Count > 0)
                    {
                        var saved = stack.Pop();
                        x = saved[0];
                        y = saved[1];
                        angle = saved[2];
                        depth = (int)saved[3];
                    }
                }
                else if (system.DrawSymbols != null && system.DrawSymbols.IndexOf(c) >= 0)
                {
                    // шаг вперёд длиной 1 в направлении angle
                    double nx = x + Math.Cos(angle);
                    double ny = y + Math.Sin(angle);
                    segments.Add(new Segment { X1 = x, Y1 = y, X2 = nx, Y2 = ny, Depth = depth });
                    x = nx;
                    y = ny;
                }
                // остальные символы ни на что не влияют: это просто переменные правил
            }
            return segments;
        }

        // Случайная добавка к углу поворота, не больше заданного процента от угла.
        // При randomness = 0 получается обычная детерминированная L-система.
        private static double RandomOffset(double delta, double randomnessPercent, Random rnd)
        {
            if (randomnessPercent <= 0) return 0;
            return (rnd.NextDouble() * 2 - 1) * delta * randomnessPercent / 100.0;
        }
    }
}

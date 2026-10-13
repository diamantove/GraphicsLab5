using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FractalLab.LSystems
{
    // Описание L-системы: аксиома (стартовое слово), угол поворота черепашки,
    // начальное направление и правила подстановки.
    //
    // Формат описания (первая строка и далее правила):
    //   <атом> <угол поворота> <начальное направление> [рисующие символы]
    //   <символ> <во что заменяется>
    //   ...
    // Например, снежинка Коха:
    //   F--F--F 60 0 F
    //   F F+F--F+F
    //
    // Рисующие символы это те, на которые черепашка реагирует шагом вперёд
    // (по умолчанию только F). Они указываются четвёртым словом в первой
    // строке, потому что у некоторых фракталов рисуют A и B.
    //
    // Если для одного символа задано несколько правил, при развёртывании
    // выбирается случайное. Это один из способов добавить случайность.
    public class LSystem
    {
        public string Axiom;
        public double Angle;        // угол поворота в градусах
        public double StartAngle;   // начальное направление в градусах: 0 вправо, 90 вверх
        public string DrawSymbols = "F";
        public Dictionary<char, List<string>> Rules = new Dictionary<char, List<string>>();

        // Разбор текстового описания. Если формат нарушен, бросает FormatException
        // с понятным сообщением, которое потом просто выводится в статус.
        public static LSystem Parse(string text)
        {
            var rows = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(line => line.Trim())
                           .Where(line => line.Length > 0 && line[0] != '#')
                           .ToList();
            if (rows.Count == 0)
                throw new FormatException("описание пустое");

            var header = rows[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (header.Length < 3)
                throw new FormatException("в первой строке должно быть: <атом> <угол> <начальное направление>");

            var system = new LSystem
            {
                Axiom = header[0],
                Angle = ParseDouble(header[1]),
                StartAngle = ParseDouble(header[2])
            };
            if (header.Length >= 4) system.DrawSymbols = header[3];

            for (int i = 1; i < rows.Count; i++)
            {
                var parts = rows[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || parts[0].Length != 1)
                    throw new FormatException("строка " + (i + 1) + ": правило должно быть вида <символ> <замена>: " + rows[i]);

                char symbol = parts[0][0];
                List<string> options;
                if (!system.Rules.TryGetValue(symbol, out options))
                {
                    options = new List<string>();
                    system.Rules[symbol] = options;
                }
                options.Add(parts[1]);
            }
            return system;
        }

        // Разворачивает аксиому: iterations раз заменяет каждый символ по правилам.
        public string Expand(int iterations, Random rnd)
        {
            string current = Axiom;
            for (int step = 0; step < iterations; step++)
            {
                var sb = new StringBuilder();
                foreach (char c in current)
                {
                    List<string> options;
                    if (Rules.TryGetValue(c, out options))
                        sb.Append(options.Count == 1 ? options[0] : options[rnd.Next(options.Count)]);
                    else
                        sb.Append(c); // у символа нет правила, оставляем его как есть
                }
                current = sb.ToString();
            }
            return current;
        }

        // Числа в файле могут быть записаны и с точкой, и с запятой.
        private static double ParseDouble(string text)
        {
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                !double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                throw new FormatException("не удалось прочитать число: " + text);
            return value;
        }
    }
}

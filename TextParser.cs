using System;
using System.Globalization;
using System.Windows.Controls;

namespace FractalLab
{
    // Небольшая помощь, чтобы не повторять один и тот же разбор чисел из
    // текстовых полей в каждой вкладке. Понимает и точку, и запятую в дробях.
    // Если разобрать не удалось, возвращает запасное значение, а не падает.
    public static class TextParser
    {
        public static bool TryReadDouble(string text, out double value)
        {
            text = (text ?? "").Trim();
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        public static double ReadDouble(TextBox box, double fallback, double min, double max)
        {
            double value;
            if (!TryReadDouble(box.Text, out value)) value = fallback;
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        public static int ReadInt(TextBox box, int fallback, int min, int max)
        {
            double value;
            if (!TryReadDouble(box.Text, out value)) value = fallback;
            int result = (int)Math.Round(value);
            if (result < min) result = min;
            if (result > max) result = max;
            return result;
        }
    }
}

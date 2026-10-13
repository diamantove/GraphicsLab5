namespace FractalLab.LSystems
{
    // Готовые описания классических L-систем для быстрых тестов.
    // Точно такие же описания лежат в папке Examples в виде txt файлов,
    // их можно открывать кнопкой "Открыть файл" на вкладке 1.a.
    public static class LSystemPresets
    {
        public class Preset
        {
            public string Name;
            public string Text;
            public int Iterations;   // разумное число итераций по умолчанию
            public double Angle;     // используется вкладкой дерева
        }

        public const string KochSnowflake =
            "# снежинка Коха: три стороны, каждая итерация ломает каждую сторону\r\n" +
            "F--F--F 60 0 F\r\n" +
            "F F+F--F+F";

        public const string KochIsland =
            "# квадратичный остров Коха (квадрат вместо треугольника)\r\n" +
            "F+F+F+F 90 0 F\r\n" +
            "F F+F-F-F+F";

        public const string Sierpinski =
            "# треугольник Серпинского (кривая-стрелка), рисуют символы A и B\r\n" +
            "A 60 0 AB\r\n" +
            "A B-A-B\r\n" +
            "B A+B+A";

        public const string Dragon =
            "# кривая дракона, X и Y это переменные, рисует только F\r\n" +
            "FX 90 0 F\r\n" +
            "X X+YF+\r\n" +
            "Y -FX-Y";

        public const string Hilbert =
            "# кривая Гильберта заполняет квадрат\r\n" +
            "A 90 0 F\r\n" +
            "A -BF+AFA+FB-\r\n" +
            "B +AF-BFB-FA+";

        public const string Gosper =
            "# кривая Госсера (flowsnake)\r\n" +
            "A 60 0 AB\r\n" +
            "A A-B--B+A++AA+B-\r\n" +
            "B +A-BB--B-A++A+B";

        public const string Plant =
            "# растение с ветвлением, скобки дают ветки\r\n" +
            "X 25 90 F\r\n" +
            "X F+[[X]-X]-F[-FX]+X\r\n" +
            "F FF";

        public const string Bush =
            "# куст, направление 90 = растём вверх\r\n" +
            "F 22.5 90 F\r\n" +
            "F FF-[-F+F+F]+[+F-F-F]";

        public const string RandomBush =
            "# случайный куст: у F два правила, на каждом шаге выбирается случайное.\r\n" +
            "# Ещё можно задать случайность угла на панели сверху.\r\n" +
            "F 25 90 F\r\n" +
            "F FF-[-F+F]+[+F-F]\r\n" +
            "F FF";

        public static readonly Preset[] All =
        {
            new Preset { Name = "Снежинка Коха", Iterations = 4, Angle = 60, Text = KochSnowflake },
            new Preset { Name = "Остров Коха", Iterations = 4, Angle = 90, Text = KochIsland },
            new Preset { Name = "Треугольник Серпинского", Iterations = 6, Angle = 60, Text = Sierpinski },
            new Preset { Name = "Кривая дракона", Iterations = 11, Angle = 90, Text = Dragon },
            new Preset { Name = "Кривая Гильберта", Iterations = 5, Angle = 90, Text = Hilbert },
            new Preset { Name = "Кривая Госсера", Iterations = 4, Angle = 60, Text = Gosper },
            new Preset { Name = "Растение (ветвление)", Iterations = 5, Angle = 25, Text = Plant },
            new Preset { Name = "Куст (ветвление)", Iterations = 4, Angle = 22.5, Text = Bush },
            new Preset { Name = "Случайный куст", Iterations = 5, Angle = 25, Text = RandomBush },
        };

        // Пресеты для вкладки дерева (задание 1.б)
        public static readonly Preset[] Trees =
        {
            new Preset { Name = "Куст", Iterations = 5, Angle = 22.5, Text = Bush },
            new Preset { Name = "Растение", Iterations = 5, Angle = 25, Text = Plant },
        };
    }
}

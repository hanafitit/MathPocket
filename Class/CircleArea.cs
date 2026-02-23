using System;
using System.Collections.Generic;

namespace MathPocket
{
    internal class CircleArea : FunctionBase
    {
        public override string   Name       => "Площадь круга";
        public override string   Formula    => "S = π · r²";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "круг", "площадь круга", "радиус" };
        public override double   Calculate(double[] inputs) => Math.PI * inputs[0] * inputs[0];

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Площадь круга\n\n" +
                    "Формула: S = π · r², где r — радиус круга.\n\n" +
                    "π ≈ 3.14159... — это постоянная, она всегда одна и та же.\n\n" +
                    "Что такое радиус: это расстояние от центра круга до его края.\n" +
                    "Если в задаче дан диаметр d — радиус вдвое меньше: r = d ÷ 2.\n\n" +
                    "Пример: найти площадь круга с радиусом 5 см\n" +
                    "  · S = π · 5² = π · 25 ≈ 3.14159 · 25 ≈ 78.54 см²\n\n" +
                    "✏️ Введи радиус r:",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double r))
                        return $"«{s}» — не число. Введи одно число, например: 5 или 3.5";
                    if (r < 0)
                        return "Радиус не может быть отрицательным — это длина, она всегда ≥ 0.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double r = double.Parse(answers[0].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);
            double r2     = r * r;
            double result = Math.PI * r2;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ S = π · {Fr(r)}² = {Fr(result, 6)} ≈ {result:F2}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Возводим радиус в квадрат: {Fr(r)}² = {Fr(r2)}");
            sb.AppendLine($"  Шаг 2. Умножаем на π ≈ 3.14159:");
            sb.AppendLine($"    π · {Fr(r2)} = {result:F6}");
            sb.AppendLine();
            sb.AppendLine($"📌 S ≈ {result:F2} (квадратных единиц)");
            return sb.ToString().TrimEnd();
        }

        private static string Fr(double v, int decimals = 10)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e15) return ((long)v).ToString();
            return v.ToString($"G{decimals}", System.Globalization.CultureInfo.InvariantCulture)
                    .TrimEnd('0').TrimEnd('.');
        }
    }
}

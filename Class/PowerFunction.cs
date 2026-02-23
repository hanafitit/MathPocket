using System;
using System.Collections.Generic;

namespace MathPocket
{
    // a^n
    internal class PowerFunction : FunctionBase
    {
        public override string   Name       => "Степень числа";
        public override string   Formula    => "aⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "степень", "возвести", "вычислить", "вычисли" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0], inputs[1]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое степень числа?\n\n" +
                    "Степень — это сокращённая запись повторного умножения.\n" +
                    "aⁿ означает: умножить a само на себя n раз.\n\n" +
                    "Разберём на примере: 2³\n" +
                    "  · основание a = 2 — число, которое умножаем\n" +
                    "  · показатель n = 3 — сколько раз умножаем\n" +
                    "  · 2³ = 2 · 2 · 2 = 8\n\n" +
                    "Ещё примеры для понимания:\n" +
                    "  · 3² = 3 · 3 = 9\n" +
                    "  · 5¹ = 5 (любое число в степени 1 равно себе)\n" +
                    "  · 7⁰ = 1 (любое ненулевое число в степени 0 равно 1)\n" +
                    "  · (-2)² = (-2)·(-2) = 4 (минус на минус = плюс)\n" +
                    "  · (-2)³ = (-2)·(-2)·(-2) = -8 (трёхкратное — остаётся минус)\n\n" +
                    "✏️ Введи основание a (число, которое возводим в степень):",
                Validate = s =>
                {
                    if (double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out _))
                        return null;
                    return $"«{s}» — не число 🤔\nВведи одно число, например: 2 или -3 или 0.5";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи показатель степени n:\n\n" +
                    "Это число сверху маленьким шрифтом — сколько раз перемножаем основание.\n" +
                    "Например, в записи 2³ показатель равен 3.",
                Validate = s =>
                {
                    if (double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out _))
                        return null;
                    return $"«{s}» — не число 🤔\nВведи целое или дробное число, например: 3 или -1 или 0.5";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = double.Parse(answers[0].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);
            double n = double.Parse(answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);
            double result = Math.Pow(a, n);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {FmtA(a)}^{FmtA(n)} = {FmtR(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");

            if (n == 0)
            {
                sb.AppendLine($"  Любое ненулевое число в нулевой степени = 1");
                sb.AppendLine($"  {FmtA(a)}⁰ = 1");
            }
            else if (n == 1)
            {
                sb.AppendLine($"  Любое число в первой степени равно себе");
                sb.AppendLine($"  {FmtA(a)}¹ = {FmtA(a)}");
            }
            else if (n == (int)n && n > 0 && n <= 6)
            {
                int ni = (int)n;
                string chain = string.Join(" · ", System.Linq.Enumerable.Repeat(FmtA(a), ni));
                sb.AppendLine($"  Раскрываем: {chain} = {FmtR(result)}");
                if (a < 0 && ni % 2 == 0)
                    sb.AppendLine("  Чётная степень отрицательного числа → результат положительный");
                else if (a < 0 && ni % 2 != 0)
                    sb.AppendLine("  Нечётная степень отрицательного числа → результат отрицательный");
            }
            else
            {
                sb.AppendLine($"  Вычисляем: {FmtA(a)}^{FmtA(n)} = {FmtR(result)}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string FmtA(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15
                ? ((long)v).ToString()
                : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);

        private static string FmtR(double v)
        {
            if (double.IsInfinity(v) || double.IsNaN(v)) return v.ToString();
            return v == Math.Floor(v) && Math.Abs(v) < 1e15
                ? ((long)v).ToString()
                : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}

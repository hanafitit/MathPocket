using System;
using System.Collections.Generic;

namespace MathPocket
{
    // Сравнивает a^m и b^n
    internal class ComparePowersFunction : FunctionBase
    {
        public override string   Name       => "Сравнение степеней";
        public override string   Formula    => "aᵐ  vs  bⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "сравнить", "сравнение", "больше", "меньше", "равно" };
        public override double   Calculate(double[] inputs) =>
            Math.Sign(Math.Pow(inputs[0], inputs[1]) - Math.Pow(inputs[2], inputs[3]));

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как сравнивать степени?\n\n" +
                    "Чтобы сравнить два выражения вида aᵐ и bⁿ, " +
                    "нужно вычислить каждое и поставить знак < = >\n\n" +
                    "Пример: сравнить 2⁵ и 4³\n" +
                    "  · 2⁵ = 2·2·2·2·2 = 32\n" +
                    "  · 4³ = 4·4·4 = 64\n" +
                    "  · 32 < 64, поэтому 2⁵ < 4³\n\n" +
                    "Иногда удобно привести к одному основанию:\n" +
                    "  4³ = (2²)³ = 2⁶ = 64, а 2⁵ = 32, значит 2⁵ < 2⁶ ✓\n\n" +
                    "Введи данные первого выражения aᵐ:\n\n" +
                    "✏️ Основание a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Показатель m первого выражения:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question =
                    "Теперь второе выражение bⁿ:\n\n" +
                    "✏️ Основание b:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Показатель n второго выражения:",
                Validate = ParseDouble
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), m = D(answers[1]);
            double b = D(answers[2]), n = D(answers[3]);
            double left = Math.Pow(a, m), right = Math.Pow(b, n);
            string sign = left < right ? "<" : left > right ? ">" : "=";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {Fa(a)}^{Fa(m)}  {sign}  {Fa(b)}^{Fa(n)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Вычисляем первое выражение: {Fa(a)}^{Fa(m)} = {Fr(left)}");
            sb.AppendLine($"  Шаг 2. Вычисляем второе выражение: {Fa(b)}^{Fa(n)} = {Fr(right)}");
            sb.AppendLine($"  Шаг 3. Сравниваем числа: {Fr(left)} {sign} {Fr(right)}");
            sb.AppendLine();

            string conclusion = sign switch
            {
                "<" => $"  📌 {Fa(a)}^{Fa(m)} меньше {Fa(b)}^{Fa(n)}",
                ">" => $"  📌 {Fa(a)}^{Fa(m)} больше {Fa(b)}^{Fa(n)}",
                _   => $"  📌 {Fa(a)}^{Fa(m)} равно {Fa(b)}^{Fa(n)}"
            };
            sb.AppendLine(conclusion);
            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 2 или 4";
        }
        private static double D(string s) => double.Parse(s.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture);
        private static string Fa(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15 ? ((long)v).ToString()
            : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);
        private static string Fr(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15 ? ((long)v).ToString()
            : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
    }
}

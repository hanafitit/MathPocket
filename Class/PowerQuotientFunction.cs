using System;
using System.Collections.Generic;

namespace MathPocket
{
    // a^m / a^n = a^(m-n)
    internal class PowerQuotientFunction : FunctionBase
    {
        public override string   Name       => "Частное степеней";
        public override string   Formula    => "aᵐ ÷ aⁿ = aᵐ⁻ⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "частное степеней", "деление степеней", "одинаковые основания" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0], inputs[1] - inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Частное степеней с одинаковым основанием\n\n" +
                    "Правило: при делении степеней с одним основанием " +
                    "основание оставляем, а из верхнего показателя вычитаем нижний.\n\n" +
                    "Почему это работает? Разберём на примере: 2⁵ ÷ 2²\n" +
                    "  · 2⁵ = 2 · 2 · 2 · 2 · 2\n" +
                    "  · 2² = 2 · 2\n" +
                    "  · При делении две двойки сверху и снизу сокращаются:\n" +
                    "    (2·2·2·2·2) / (2·2) = 2·2·2 = 2³\n" +
                    "  · Показатель: 5 − 2 = 3. Итого: 2³ = 8\n\n" +
                    "Частный случай: если показатели равны, получаем a⁰ = 1.\n" +
                    "Например: 5³ ÷ 5³ = 5⁰ = 1 — число делится само на себя.\n\n" +
                    "✏️ Введи основание a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи показатель числителя m (верхняя степень):",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи показатель знаменателя n (нижняя степень):",
                Validate = ParseDouble
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), m = D(answers[1]), n = D(answers[2]);
            double result = Math.Pow(a, m - n);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {Fa(a)}^{Fa(m)} ÷ {Fa(a)}^{Fa(n)} = {Fa(a)}^({Fa(m)} − {Fa(n)}) = {Fa(a)}^{Fa(m - n)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Основания одинаковые ({Fa(a)}) — применяем правило.");
            sb.AppendLine($"  Шаг 2. Вычитаем показатели: {Fa(m)} − {Fa(n)} = {Fa(m - n)}");
            sb.AppendLine($"  Шаг 3. Записываем: {Fa(a)}^{Fa(m - n)} = {Fr(result)}");

            if (m == n)
                sb.AppendLine($"\n  Показатели равны → {Fa(a)}⁰ = 1 (число делится само на себя)");
            else if (m - n < 0)
                sb.AppendLine($"\n  Отрицательный показатель {Fa(m - n)} — это дробное выражение (1/{Fa(a)}^{Fa(n - m)})");

            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 5 или -2";
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

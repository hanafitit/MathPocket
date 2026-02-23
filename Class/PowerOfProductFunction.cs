using System;
using System.Collections.Generic;

namespace MathPocket
{
    // (a*b)^n = a^n * b^n
    internal class PowerOfProductFunction : FunctionBase
    {
        public override string   Name       => "Степень произведения";
        public override string   Formula    => "(a · b)ⁿ = aⁿ · bⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "степень произведения", "раскрыть скобки", "представить" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0] * inputs[1], inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Степень произведения\n\n" +
                    "Правило: степень произведения равна произведению степеней — " +
                    "показатель «раздаётся» каждому множителю.\n\n" +
                    "Почему это работает? Разберём на примере: (2 · 3)²\n" +
                    "  · (2 · 3)² = (2 · 3) · (2 · 3)\n" +
                    "  · Переставим множители: 2 · 2 · 3 · 3 = 2² · 3²\n" +
                    "  · Итого: (2 · 3)² = 4 · 9 = 36 — то же что 6² = 36 ✓\n\n" +
                    "Это удобно, когда нужно разложить выражение по множителям.\n" +
                    "Например: (5ab)³ = 5³ · a³ · b³ = 125a³b³\n\n" +
                    "✏️ Введи первый множитель a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи второй множитель b:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи показатель степени n:",
                Validate = ParseDouble
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), b = D(answers[1]), n = D(answers[2]);
            double an = Math.Pow(a, n), bn = Math.Pow(b, n), result = an * bn;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ ({Fa(a)} · {Fa(b)})^{Fa(n)} = {Fa(a)}^{Fa(n)} · {Fa(b)}^{Fa(n)} = {Fr(an)} · {Fr(bn)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Показатель {Fa(n)} «раздаётся» каждому множителю.");
            sb.AppendLine($"  Шаг 2. {Fa(a)}^{Fa(n)} = {Fr(an)}");
            sb.AppendLine($"  Шаг 3. {Fa(b)}^{Fa(n)} = {Fr(bn)}");
            sb.AppendLine($"  Шаг 4. Перемножаем: {Fr(an)} · {Fr(bn)} = {Fr(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 3 или -2";
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

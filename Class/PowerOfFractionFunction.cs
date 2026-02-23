using System;
using System.Collections.Generic;

namespace MathPocket
{
    // (a/b)^n = a^n / b^n
    internal class PowerOfFractionFunction : FunctionBase
    {
        public override string   Name       => "Степень дроби";
        public override string   Formula    => "(a/b)ⁿ = aⁿ / bⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "степень дроби", "дробное основание", "упростить" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0] / inputs[1], inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Степень дроби\n\n" +
                    "Правило: чтобы возвести дробь в степень, " +
                    "нужно возвести числитель в эту степень и знаменатель в эту же степень отдельно.\n\n" +
                    "Почему это работает? Разберём на примере: (2/3)²\n" +
                    "  · (2/3)² = (2/3) · (2/3)\n" +
                    "  · Перемножаем дроби: (2·2) / (3·3) = 4/9\n" +
                    "  · То же самое: 2² / 3² = 4/9 ✓\n\n" +
                    "Ещё пример: (3/4)³ = 3³/4³ = 27/64\n\n" +
                    "Важно: знаменатель не может быть равен нулю — на ноль делить нельзя.\n\n" +
                    "✏️ Введи числитель a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи знаменатель b:\n(не ноль — на ноль делить нельзя)",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double v))
                        return $"«{s}» — не число. Введи одно число, например: 3 или 4";
                    if (v == 0)
                        return "Знаменатель не может быть равен нулю — деление на ноль не определено.\n" +
                               "Введи любое ненулевое число.";
                    return null;
                }
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
            double an = Math.Pow(a, n), bn = Math.Pow(b, n), result = an / bn;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ ({Fa(a)}/{Fa(b)})^{Fa(n)} = {Fa(a)}^{Fa(n)} / {Fa(b)}^{Fa(n)} = {Fr(an)} / {Fr(bn)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Показатель {Fa(n)} применяем к числителю и знаменателю отдельно.");
            sb.AppendLine($"  Шаг 2. Числитель: {Fa(a)}^{Fa(n)} = {Fr(an)}");
            sb.AppendLine($"  Шаг 3. Знаменатель: {Fa(b)}^{Fa(n)} = {Fr(bn)}");
            sb.AppendLine($"  Шаг 4. Дробь: {Fr(an)} / {Fr(bn)} = {Fr(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 2 или 3";
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

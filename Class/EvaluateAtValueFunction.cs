using System;
using System.Collections.Generic;

namespace MathPocket
{
    // a^m / a^n при заданном a
    internal class EvaluateAtValueFunction : FunctionBase
    {
        public override string   Name       => "Значение выражения при заданном a";
        public override string   Formula    => "aᵐ ÷ aⁿ при заданном a";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "значение выражения", "при a", "подставить", "вычислить при" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0], inputs[1] - inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как найти значение выражения aᵐ ÷ aⁿ?\n\n" +
                    "Задача: в выражении вместо буквы a подставить число и вычислить.\n\n" +
                    "Удобный способ — сначала упростить по правилу деления степеней:\n" +
                    "  aᵐ ÷ aⁿ = aᵐ⁻ⁿ\n" +
                    "Тогда нужно вычислить только одну степень, а не две.\n\n" +
                    "Пример: вычислить a⁵ ÷ a² при a = 3\n" +
                    "  Шаг 1. Упрощаем: a⁵ ÷ a² = a^(5−2) = a³\n" +
                    "  Шаг 2. Подставляем a = 3: 3³ = 3·3·3 = 27\n\n" +
                    "Введи данные:\n\n" +
                    "✏️ Введи значение a (то число, которое подставляем):",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out _))
                        return $"«{s}» — не число. Введи одно число, например: 3 или -2";
                    return null;
                }
            },
            new InputStep
            {
                Question = "✏️ Введи показатель числителя m:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи показатель знаменателя n:",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double n))
                        return $"«{s}» — не число. Введи целое число, например: 2 или 0";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), m = D(answers[1]), n = D(answers[2]);
            double diff   = m - n;
            double result = Math.Pow(a, diff);

            if (a == 0 && diff < 0)
                return "⚠️ При a = 0 и отрицательном (m − n) возникает деление на ноль — результат не определён.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {Fa(a)}^{Fa(m)} ÷ {Fa(a)}^{Fa(n)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Упрощаем по правилу деления степеней:");
            sb.AppendLine($"    a^{Fa(m)} ÷ a^{Fa(n)} = a^({Fa(m)} − {Fa(n)}) = a^{Fa(diff)}");
            sb.AppendLine($"  Шаг 2. Подставляем a = {Fa(a)}:");
            sb.AppendLine($"    {Fa(a)}^{Fa(diff)} = {Fr(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 5 или 2";
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

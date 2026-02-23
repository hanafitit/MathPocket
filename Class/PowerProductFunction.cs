using System;
using System.Collections.Generic;

namespace MathPocket
{
    // a^m * a^n = a^(m+n)
    internal class PowerProductFunction : FunctionBase
    {
        public override string   Name       => "Произведение степеней";
        public override string   Formula    => "aᵐ · aⁿ = aᵐ⁺ⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "произведение степеней", "умножение степеней", "одинаковые основания" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0], inputs[1] + inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Произведение степеней с одинаковым основанием\n\n" +
                    "Правило: когда умножаем степени с одним и тем же основанием, " +
                    "основание оставляем, а показатели складываем.\n\n" +
                    "Почему это работает? Давай разберём на примере: 2³ · 2⁴\n" +
                    "  · 2³ = 2 · 2 · 2  (три двойки)\n" +
                    "  · 2⁴ = 2 · 2 · 2 · 2  (четыре двойки)\n" +
                    "  · всего двоек: 3 + 4 = 7\n" +
                    "  · значит 2³ · 2⁴ = 2⁷ = 128\n\n" +
                    "Важно: правило работает только когда основание одно и то же.\n" +
                    "2³ · 3⁴ нельзя упростить этим способом — основания разные.\n\n" +
                    "✏️ Введи основание a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи первый показатель m:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи второй показатель n:",
                Validate = ParseDouble
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), m = D(answers[1]), n = D(answers[2]);
            double result = Math.Pow(a, m + n);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {Fa(a)}^{Fa(m)} · {Fa(a)}^{Fa(n)} = {Fa(a)}^({Fa(m)} + {Fa(n)}) = {Fa(a)}^{Fa(m + n)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Основания одинаковые ({Fa(a)}) — применяем правило.");
            sb.AppendLine($"  Шаг 2. Складываем показатели: {Fa(m)} + {Fa(n)} = {Fa(m + n)}");
            sb.AppendLine($"  Шаг 3. Записываем результат: {Fa(a)}^{Fa(m + n)} = {Fr(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string? ParseDouble(string s)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи одно число, например: 2 или -3";
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

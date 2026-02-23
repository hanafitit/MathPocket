using System;
using System.Collections.Generic;

namespace MathPocket
{
    // (a^m)^n = a^(m*n)
    internal class PowerOfPowerFunction : FunctionBase
    {
        public override string   Name       => "Степень степени";
        public override string   Formula    => "(aᵐ)ⁿ = aᵐ·ⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "степень степени", "скобка степень", "упростить" };
        public override double   Calculate(double[] inputs) => Math.Pow(inputs[0], inputs[1] * inputs[2]);

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Степень степени\n\n" +
                    "Правило: когда степень снова возводят в степень, " +
                    "основание оставляем, а показатели перемножаем.\n\n" +
                    "Почему это работает? Разберём на примере: (2³)⁴\n" +
                    "  · (2³)⁴ означает: взять 2³ и перемножить его 4 раза\n" +
                    "  · 2³ · 2³ · 2³ · 2³ — умножаем четыре одинаковых множителя\n" +
                    "  · По правилу умножения степеней складываем показатели:\n" +
                    "    3 + 3 + 3 + 3 = 3 · 4 = 12\n" +
                    "  · Итого: (2³)⁴ = 2¹² = 4096\n\n" +
                    "Запомни: показатели перемножаются, а не складываются.\n" +
                    "Частая ошибка: (2³)⁴ = 2⁷ — это неверно. Верно: 2¹².\n\n" +
                    "✏️ Введи основание a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи внутренний показатель m (степень внутри скобки):",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи внешний показатель n (степень снаружи скобки):",
                Validate = ParseDouble
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = D(answers[0]), m = D(answers[1]), n = D(answers[2]);
            double result = Math.Pow(a, m * n);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ ({Fa(a)}^{Fa(m)})^{Fa(n)} = {Fa(a)}^({Fa(m)} · {Fa(n)}) = {Fa(a)}^{Fa(m * n)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Перемножаем показатели: {Fa(m)} · {Fa(n)} = {Fa(m * n)}");
            sb.AppendLine($"  Шаг 2. Вычисляем: {Fa(a)}^{Fa(m * n)} = {Fr(result)}");
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

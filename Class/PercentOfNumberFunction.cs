using System;
using System.Collections.Generic;

namespace MathPocket
{
    // Найти p% от числа x
    internal class PercentOfNumberFunction : FunctionBase
    {
        public override string   Name       => "Процент от числа";
        public override string   Formula    => "x · p / 100";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "процент", "% от числа", "найти процент" };
        public override double   Calculate(double[] inputs) => inputs[0] * inputs[1] / 100.0;

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как найти процент от числа?\n\n" +
                    "Процент — это сотая часть числа. Слово «процент» буквально " +
                    "значит «из ста» (от латинского per centum).\n\n" +
                    "Формула: p% от числа x = x · p ÷ 100\n\n" +
                    "Пример: найти 20% от 150\n" +
                    "  · Записываем: 150 · 20 ÷ 100\n" +
                    "  · 150 · 20 = 3000\n" +
                    "  · 3000 ÷ 100 = 30\n" +
                    "  · Ответ: 20% от 150 = 30\n\n" +
                    "Проверка смыслом: 10% от 150 — это 15 (просто сдвинуть запятую). " +
                    "20% — вдвое больше, то есть 30. Сходится!\n\n" +
                    "✏️ Введи число x (от которого ищем процент):",
                Validate = s =>
                {
                    if (double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
                    return $"«{s}» — не число. Введи одно число, например: 150 или 200";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи процент p:\n\n" +
                    "Это сколько процентов нужно найти.\n" +
                    "Просто число без знака %: например 20, а не 20%",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double p))
                        return $"«{s}» — не число. Введи одно число, например: 20 или 7.5";
                    if (p < 0)
                        return "Процент обычно неотрицательный. Введи число ≥ 0, например 15";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double x = D(answers[0]), p = D(answers[1]);
            double result = x * p / 100.0;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {Fr(p)}% от {Fr(x)} = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Записываем по формуле: {Fr(x)} · {Fr(p)} ÷ 100");
            sb.AppendLine($"  Шаг 2. {Fr(x)} · {Fr(p)} = {Fr(x * p)}");
            sb.AppendLine($"  Шаг 3. {Fr(x * p)} ÷ 100 = {Fr(result)}");
            sb.AppendLine();
            sb.AppendLine($"📌 {Fr(p)}% от {Fr(x)} = {Fr(result)}");
            return sb.ToString().TrimEnd();
        }

        private static double D(string s) => double.Parse(s.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture);
        private static string Fr(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15 ? ((long)v).ToString()
            : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    }
}

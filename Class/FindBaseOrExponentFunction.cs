using System;
using System.Collections.Generic;

namespace MathPocket
{
    // Найти неизвестный показатель в трёх видах равенств
    internal class FindBaseOrExponentFunction : FunctionBase
    {
        public override string   Name       => "Найти основание или показатель";
        public override string   Formula    => "aᵐ · a? = aⁿ  /  (aᵐ)? = aⁿ  /  a? ÷ aᵐ = aⁿ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "вместо звёздочки", "найти показатель", "верное равенство" };
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как найти неизвестный показатель?\n\n" +
                    "В задачах часто нужно найти число вместо «?» так, чтобы равенство было верным.\n\n" +
                    "Есть три вида таких задач. Выбери нужный — введи цифру:\n\n" +
                    "1️⃣  aᵐ · a? = aⁿ\n" +
                    "    При умножении степени складываются: m + ? = n, значит ? = n − m\n" +
                    "    Например: 2³ · 2? = 2⁷ → ? = 7 − 3 = 4\n\n" +
                    "2️⃣  (aᵐ)? = aⁿ\n" +
                    "    При степени степени показатели множатся: m · ? = n, значит ? = n ÷ m\n" +
                    "    Например: (2³)? = 2¹² → ? = 12 ÷ 3 = 4\n\n" +
                    "3️⃣  a? ÷ aᵐ = aⁿ\n" +
                    "    При делении степени вычитаются: ? − m = n, значит ? = n + m\n" +
                    "    Например: 2? ÷ 2³ = 2⁴ → ? = 4 + 3 = 7\n\n" +
                    "✏️ Введи номер вида задачи (1, 2 или 3):",
                Validate = s =>
                {
                    if (s == "1" || s == "2" || s == "3") return null;
                    return "Введи цифру 1, 2 или 3 — номер нужного вида задачи.";
                }
            },
            new InputStep
            {
                Question = "✏️ Введи основание a:",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи известный показатель m (тот, что уже задан):",
                Validate = ParseDouble
            },
            new InputStep
            {
                Question = "✏️ Введи показатель результата n (правая часть равенства):",
                Validate = s =>
                {
                    if (!double.TryParse(s.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out _))
                        return $"«{s}» — не число. Введи одно число, например: 7 или 12";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            int    mode = int.Parse(answers[0]);
            double a    = D(answers[1]);
            double m    = D(answers[2]);
            double n    = D(answers[3]);

            double unknown;
            string formula, explanation;

            switch (mode)
            {
                case 1:
                    unknown = n - m;
                    formula = $"{Fa(a)}^{Fa(m)} · {Fa(a)}^? = {Fa(a)}^{Fa(n)}";
                    explanation =
                        $"  Используем правило умножения: показатели складываются.\n" +
                        $"  m + ? = n → ? = n − m = {Fa(n)} − {Fa(m)} = {Fa(unknown)}";
                    break;
                case 2:
                    if (m == 0)
                        return "⚠️ Внутренний показатель m не может быть 0 — на него делим, а на ноль нельзя.";
                    unknown = n / m;
                    formula = $"({Fa(a)}^{Fa(m)})^? = {Fa(a)}^{Fa(n)}";
                    explanation =
                        $"  Используем правило степени степени: показатели множатся.\n" +
                        $"  m · ? = n → ? = n ÷ m = {Fa(n)} ÷ {Fa(m)} = {Fa(unknown)}";
                    break;
                default:
                    unknown = n + m;
                    formula = $"{Fa(a)}^? ÷ {Fa(a)}^{Fa(m)} = {Fa(a)}^{Fa(n)}";
                    explanation =
                        $"  Используем правило деления: показатели вычитаются.\n" +
                        $"  ? − m = n → ? = n + m = {Fa(n)} + {Fa(m)} = {Fa(unknown)}";
                    break;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ {formula}   →   ? = {Fa(unknown)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine(explanation);
            sb.AppendLine();

            // Проверка
            double check = mode switch
            {
                1 => Math.Pow(a, m) * Math.Pow(a, unknown),
                2 => Math.Pow(Math.Pow(a, m), unknown),
                _ => Math.Pow(a, unknown) / Math.Pow(a, m)
            };
            double expected = Math.Pow(a, n);
            sb.AppendLine($"  Проверка: подставляем ? = {Fa(unknown)}");
            sb.AppendLine($"  Левая часть = {Fr(check)}, правая часть = {Fr(expected)}");
            sb.AppendLine(Math.Abs(check - expected) < 1e-9
                ? "  ✓ Равенство верно!"
                : "  ⚠️ Что-то не сошлось — проверь исходные данные.");
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

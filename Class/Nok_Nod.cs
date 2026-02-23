using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  НОД — Наибольший общий делитель
    // ═══════════════════════════════════════════════════════════════
    internal class Nod : FunctionBase
    {
        public override string   Name       => "НОД (наибольший общий делитель)";
        public override string   Formula    => "НОД(a, b, ...)";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "НОД", "делитель", "общий делитель" };
        public override double   Calculate(double[] inputs)
        {
            int result = (int)inputs[0];
            for (int i = 1; i < inputs.Length; i++)
                result = MathUtils.Gcd(result, (int)inputs[i]);
            return result;
        }

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое НОД и зачем он нужен?\n\n" +
                    "НОД — это наибольшее число, на которое делятся все заданные числа без остатка.\n\n" +
                    "Зачем: НОД нужен чтобы сокращать дроби до несократимого вида.\n" +
                    "Например: дробь 12/18 — оба числа делятся на 6, значит НОД(12,18)=6,\n" +
                    "и дробь сокращается до 2/3.\n\n" +
                    "Как найти НОД — разложить на простые множители:\n" +
                    "  НОД(12, 18):\n" +
                    "  · 12 = 2 · 2 · 3\n" +
                    "  · 18 = 2 · 3 · 3\n" +
                    "  · Общие множители: 2 и 3\n" +
                    "  · НОД = 2 · 3 = 6\n\n" +
                    "✏️ Введи числа через пробел (например: 12 18 или 24 36 48):",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return "Введи хотя бы два числа через пробел, например: 12 18";
                    var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        return "Нужно хотя бы два числа через пробел, например: 12 18";
                    foreach (var p in parts)
                        if (!int.TryParse(p, out int v) || v <= 0)
                            return $"«{p}» — не подходит. Нужны натуральные числа (целые, больше 0).";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var nums = answers[0]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"✅ НОД({string.Join(", ", nums)})");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine();

            // Разложение каждого числа
            foreach (int num in nums)
            {
                string factors = PrimeFactors(num);
                sb.AppendLine($"  {num} = {factors}");
            }

            sb.AppendLine();
            sb.AppendLine("  Находим общие множители:");

            int result = nums[0];
            for (int i = 1; i < nums.Count; i++)
            {
                int prev = result;
                result = MathUtils.Gcd(result, nums[i]);
                sb.AppendLine($"  НОД({prev}, {nums[i]}) = {result}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 НОД({string.Join(", ", nums)}) = {result}");

            if (result == 1)
                sb.AppendLine("  Числа взаимно просты — общих делителей кроме 1 нет.");

            return sb.ToString().TrimEnd();
        }

        private static string PrimeFactors(int n)
        {
            if (n == 1) return "1";
            var factors = new List<int>();
            int d = 2;
            while (d * d <= n) { while (n % d == 0) { factors.Add(d); n /= d; } d++; }
            if (n > 1) factors.Add(n);
            return string.Join(" · ", factors);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  НОК — Наименьшее общее кратное
    // ═══════════════════════════════════════════════════════════════
    internal class Nok : FunctionBase
    {
        public override string   Name       => "НОК (наименьшее общее кратное)";
        public override string   Formula    => "НОК(a, b, ...)";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "НОК", "кратное", "общее кратное" };
        public override double   Calculate(double[] inputs)
        {
            int result = (int)inputs[0];
            for (int i = 1; i < inputs.Length; i++)
                result = MathUtils.Lcm(result, (int)inputs[i]);
            return result;
        }

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое НОК и зачем он нужен?\n\n" +
                    "НОК — это наименьшее число, которое делится на все заданные числа.\n\n" +
                    "Зачем: НОК нужен чтобы привести дроби к общему знаменателю.\n" +
                    "Например: 1/4 + 1/6 — нужен общий знаменатель. НОК(4,6)=12,\n" +
                    "поэтому 1/4 = 3/12 и 1/6 = 2/12, и складывать уже легко.\n\n" +
                    "Как найти НОК — разложить на простые множители и взять каждый в наибольшей степени:\n" +
                    "  НОК(4, 6):\n" +
                    "  · 4 = 2²\n" +
                    "  · 6 = 2 · 3\n" +
                    "  · Берём 2² и 3¹ (наибольшие степени)\n" +
                    "  · НОК = 4 · 3 = 12\n\n" +
                    "✏️ Введи числа через пробел (например: 4 6 или 6 8 12):",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return "Введи хотя бы два числа через пробел, например: 4 6";
                    var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        return "Нужно хотя бы два числа через пробел, например: 4 6";
                    foreach (var p in parts)
                        if (!int.TryParse(p, out int v) || v <= 0)
                            return $"«{p}» — не подходит. Нужны натуральные числа (целые, больше 0).";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var nums = answers[0]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"✅ НОК({string.Join(", ", nums)})");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine();

            foreach (int num in nums)
            {
                string factors = PrimeFactors(num);
                sb.AppendLine($"  {num} = {factors}");
            }

            sb.AppendLine();

            int result = nums[0];
            for (int i = 1; i < nums.Count; i++)
            {
                int prev = result;
                result = MathUtils.Lcm(result, nums[i]);
                sb.AppendLine($"  НОК({prev}, {nums[i]}) = {result}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 НОК({string.Join(", ", nums)}) = {result}");
            sb.AppendLine($"  Наименьший общий знаменатель для этих чисел: {result}");

            return sb.ToString().TrimEnd();
        }

        private static string PrimeFactors(int n)
        {
            if (n == 1) return "1";
            var factors = new List<int>();
            int d = 2;
            while (d * d <= n) { while (n % d == 0) { factors.Add(d); n /= d; } d++; }
            if (n > 1) factors.Add(n);
            return string.Join(" · ", factors);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Вспомогательные методы (без изменений)
    // ═══════════════════════════════════════════════════════════════
    public static class MathUtils
    {
        public static int Gcd(int a, int b)
        {
            while (b != 0) { int t = b; b = a % b; a = t; }
            return Math.Abs(a);
        }
        public static int Lcm(int a, int b) => a / Gcd(a, b) * b;
    }
}

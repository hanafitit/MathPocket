using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Вспомогательные утилиты для линейных функций
    // ═══════════════════════════════════════════════════════════════

    internal static class LinearHelper
    {
        /// <summary>
        /// Разбирает строку вида "kx+b", "kx", "b", "-x+3" и возвращает (k, b).
        /// Возвращает null если не удалось разобрать.
        /// </summary>
        public static (double k, double b)? ParseLinear(string raw)
        {
            raw = raw.Trim()
                     .Replace(" ", "")
                     .Replace("−", "-")
                     .Replace(",", ".");

            // Нормализуем: заменяем -x → -1x, +x → +1x, ^x в начале → 1x
            raw = System.Text.RegularExpressions.Regex.Replace(raw, @"(?<![0-9])x", "1x");
            raw = System.Text.RegularExpressions.Regex.Replace(raw, @"(?<!\d)-1x", "-1x"); // уже корректно

            // Ищем коэффициент при x
            var match = System.Text.RegularExpressions.Regex.Match(
                raw, @"^([+-]?[0-9]*\.?[0-9]*)x([+-][0-9]*\.?[0-9]+)?$");

            if (!match.Success) return null;

            string kStr = match.Groups[1].Value;
            string bStr = match.Groups[2].Value;

            double k = string.IsNullOrEmpty(kStr) || kStr == "+" ? 1
                     : kStr == "-" ? -1
                     : double.Parse(kStr, CultureInfo.InvariantCulture);

            double b = string.IsNullOrEmpty(bStr) ? 0
                     : double.Parse(bStr, CultureInfo.InvariantCulture);

            return (k, b);
        }

        /// <summary>Форматирует число: целое без .0, дробное через /</summary>
        public static string Fmt(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e12)
                return ((long)v).ToString();
            // Попытка красивой дроби
            for (int den = 2; den <= 20; den++)
            {
                double num = v * den;
                if (Math.Abs(num - Math.Round(num)) < 1e-9)
                {
                    long n = (long)Math.Round(num);
                    long g = Gcd(Math.Abs(n), den);
                    long nd = n / g, dd = den / (int)g;
                    if (dd < 0) { nd = -nd; dd = -dd; }
                    return dd == 1 ? nd.ToString() : $"{nd}/{dd}";
                }
            }
            return v.ToString("G6", CultureInfo.InvariantCulture);
        }

        private static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

        /// <summary>Форматирует линейную функцию y = kx + b в красивый вид</summary>
        public static string FormatLinear(double k, double b)
        {
            string kStr = k == 1 ? "" : k == -1 ? "-" : Fmt(k);
            string part1 = $"{kStr}x";

            if (b == 0) return $"y = {part1}";
            if (b > 0)  return $"y = {part1} + {Fmt(b)}";
            return             $"y = {part1} − {Fmt(-b)}";
        }

        public static string? ValidateLinear(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 3x+2  или  -x-5  или  0.5x";
            if (ParseLinear(s) is null)
                return $"Не могу разобрать «{s.Trim()}» как линейную функцию.\n" +
                       "Пример: 3x+2  или  -x-5  или  0.5x";
            return null;
        }

        public static string? ValidateNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Введи число.";
            if (!double.TryParse(s.Trim().Replace(',', '.').Replace("−", "-"),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return $"«{s.Trim()}» — не число.";
            return null;
        }

        public static double ParseNumber(string s) =>
            double.Parse(s.Trim().Replace(',', '.').Replace("−", "-"),
                NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.1 Является ли линейной функцией?
    // ═══════════════════════════════════════════════════════════════

    public class IsLinearFunction : FunctionBase
    {
        public override string   Name       => "Является ли линейной функцией";
        public override string   Formula    => "y = kx + b";
        public override string[] Keywords   => new[] { "линейная", "является", "функция" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Линейная функция — y = kx + b\n\n" +
                    "Условия:\n" +
                    "  · x стоит в первой степени\n" +
                    "  · нет x², x³, дробей с x, корней из x\n" +
                    "  · k и b — любые числа\n\n" +
                    "Примеры линейных:\n" +
                    "  y = 3x + 2  ✅\n" +
                    "  y = -x      ✅  (k=-1, b=0)\n" +
                    "  y = 5       ✅  (k=0, b=5)\n\n" +
                    "Примеры НЕ линейных:\n" +
                    "  y = x² - 5  ❌  (степень 2)\n" +
                    "  y = 1/x     ❌  (x в знаменателе)\n\n" +
                    "✏️ Введи правую часть функции (после y =):\n" +
                    "  Пример: 3x+2  или  x^2-5  или  1/x",
                Validate = s => string.IsNullOrWhiteSpace(s)
                    ? "Ты ничего не ввёл. Пример: 3x+2"
                    : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string raw = answers[0].Trim().Replace(" ", "").Replace("−", "-").Replace(",", ".");
            var sb = new StringBuilder();
            sb.AppendLine($"Проверяем: y = {answers[0].Trim()}");
            sb.AppendLine();

            // Признаки нелинейности
            bool hasSquare   = raw.Contains("x^2") || raw.Contains("x²") || raw.Contains("x*x");
            bool hasCube     = raw.Contains("x^3") || raw.Contains("x³");
            bool hasHighPow  = System.Text.RegularExpressions.Regex.IsMatch(raw, @"x\^[4-9]");
            bool hasFracX    = raw.Contains("/x") || raw.Contains("/(x");
            bool hasSqrtX    = raw.Contains("√x") || raw.Contains("sqrt(x");

            if (hasSquare || hasCube || hasHighPow || hasFracX || hasSqrtX)
            {
                if (hasSquare)  sb.AppendLine("  ❌ Есть x² — степень не первая");
                if (hasCube)    sb.AppendLine("  ❌ Есть x³ — степень не первая");
                if (hasHighPow) sb.AppendLine("  ❌ Есть x в степени ≥ 4");
                if (hasFracX)   sb.AppendLine("  ❌ x стоит в знаменателе дроби");
                if (hasSqrtX)   sb.AppendLine("  ❌ x стоит под корнем");
                sb.AppendLine();
                sb.AppendLine("📌 Это НЕ линейная функция.");
                return sb.ToString().TrimEnd();
            }

            var parsed = LinearHelper.ParseLinear(raw);
            if (parsed is null)
            {
                // Может быть просто константа вида b
                if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double bOnly))
                {
                    sb.AppendLine($"  ✅ x не присутствует → k = 0,  b = {LinearHelper.Fmt(bOnly)}");
                    sb.AppendLine($"  Это частный случай y = kx + b при k = 0.");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Это ЛИНЕЙНАЯ функция ✅");
                    sb.AppendLine($"   {LinearHelper.FormatLinear(0, bOnly)}");
                    return sb.ToString().TrimEnd();
                }

                sb.AppendLine("  ⚠️ Не удалось определить вид функции автоматически.");
                sb.AppendLine("  Проверь, нет ли x², дробей с x или корней.");
                return sb.ToString().TrimEnd();
            }

            var (k, b) = parsed.Value;
            sb.AppendLine($"  ✅ x стоит в первой степени");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)},  b = {LinearHelper.Fmt(b)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Это ЛИНЕЙНАЯ функция ✅");
            sb.AppendLine($"   {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.2/22.3 Найти y по x или x по y
    // ═══════════════════════════════════════════════════════════════

    public class LinearEvalFunction : FunctionBase
    {
        public override string   Name       => "Найти y по x или x по y";
        public override string   Formula    => "y = kx + b";
        public override string[] Keywords   => new[] { "линейная", "найти", "значение", "подставить" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти значение линейной функции\n\n" +
                    "Задача 1 — найти y: подставляем x в формулу.\n" +
                    "  y = 4x − 3,  x = 2  →  y = 4·2 − 3 = 5\n\n" +
                    "Задача 2 — найти x: решаем уравнение kx + b = y.\n" +
                    "  y = 4x − 3,  y = 9  →  4x = 12  →  x = 3\n\n" +
                    "✏️ Введи формулу (правую часть после y =):\n" +
                    "  Пример: 4x-3  или  -2x+1  или  0.5x",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question =
                    "✏️ Что ищем?\n" +
                    "  Введи:  y  — если знаешь x и ищешь y\n" +
                    "          x  — если знаешь y и ищешь x",
                Validate = s =>
                {
                    string t = s.Trim().ToLower();
                    return t == "y" || t == "x" ? null : "Введи: y  или  x";
                }
            },
            new InputStep
            {
                Question = "✏️ Введи известное значение (число):",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            string mode = answers[1].Trim().ToLower();
            double val  = LinearHelper.ParseNumber(answers[2]);
            var sb = new StringBuilder();

            sb.AppendLine($"Функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine();

            if (mode == "y")
            {
                double y = k * val + b;
                sb.AppendLine($"Подставляем x = {LinearHelper.Fmt(val)}:");
                sb.AppendLine($"  y = {LinearHelper.Fmt(k)}·{LinearHelper.Fmt(val)} + ({LinearHelper.Fmt(b)})");
                sb.AppendLine($"  y = {LinearHelper.Fmt(k * val)} + {LinearHelper.Fmt(b)}");
                sb.AppendLine();
                sb.AppendLine($"📌 y = {LinearHelper.Fmt(y)}");
            }
            else
            {
                // kx + b = val  →  x = (val - b) / k
                if (Math.Abs(k) < 1e-12)
                {
                    if (Math.Abs(b - val) < 1e-9)
                        sb.AppendLine("📌 k = 0, функция постоянная — x может быть любым числом.");
                    else
                        sb.AppendLine($"📌 k = 0, функция постоянная y = {LinearHelper.Fmt(b)} ≠ {LinearHelper.Fmt(val)} — решений нет.");
                    return sb.ToString().TrimEnd();
                }
                double x = (val - b) / k;
                sb.AppendLine($"Решаем уравнение: {LinearHelper.Fmt(k)}x + {LinearHelper.Fmt(b)} = {LinearHelper.Fmt(val)}");
                sb.AppendLine($"  {LinearHelper.Fmt(k)}x = {LinearHelper.Fmt(val)} − {LinearHelper.Fmt(b)}");
                sb.AppendLine($"  {LinearHelper.Fmt(k)}x = {LinearHelper.Fmt(val - b)}");
                sb.AppendLine($"  x = {LinearHelper.Fmt(val - b)} / {LinearHelper.Fmt(k)}");
                sb.AppendLine();
                sb.AppendLine($"📌 x = {LinearHelper.Fmt(x)}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.4/22.5 Построить таблицу и найти точки пересечения с осями
    // ═══════════════════════════════════════════════════════════════

    public class LinearTableAndAxesFunction : FunctionBase
    {
        public override string   Name       => "Таблица и пересечение с осями";
        public override string   Formula    => "y = kx + b → таблица, ось Ox, ось Oy";
        public override string[] Keywords   => new[] { "линейная", "таблица", "пересечение", "оси", "график" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Таблица значений и пересечение с осями\n\n" +
                    "Для построения графика y = kx + b:\n" +
                    "  1. Составим таблицу из нескольких точек\n" +
                    "  2. Найдём пересечение с осью Ox (y = 0)\n" +
                    "  3. Найдём пересечение с осью Oy (x = 0)\n\n" +
                    "✏️ Введи формулу (правую часть после y =):\n" +
                    "  Пример: 3x-6  или  -2x+4",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question =
                    "✏️ Введи значения x для таблицы через запятую:\n" +
                    "  Пример: -2, -1, 0, 1, 2",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Введи хотя бы одно значение.";
                    foreach (var p in s.Split(','))
                        if (LinearHelper.ValidateNumber(p) is not null)
                            return $"«{p.Trim()}» — не число.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            var xs = answers[1].Split(',')
                .Select(p => LinearHelper.ParseNumber(p))
                .ToList();
            var sb = new StringBuilder();

            sb.AppendLine($"Функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine();

            // Таблица
            sb.AppendLine("📋 Таблица значений:");
            var xStrs = xs.Select(x => LinearHelper.Fmt(x)).ToList();
            var yStrs = xs.Select(x => LinearHelper.Fmt(k * x + b)).ToList();
            sb.AppendLine($"  x:  {string.Join("   ", xStrs)}");
            sb.AppendLine($"  y:  {string.Join("   ", yStrs)}");
            sb.AppendLine();

            // Пересечение с Oy (x = 0)
            sb.AppendLine("📍 Пересечение с осью Oy (x = 0):");
            double yAxis = b;
            sb.AppendLine($"  y = {LinearHelper.Fmt(k)}·0 + {LinearHelper.Fmt(b)} = {LinearHelper.Fmt(yAxis)}");
            sb.AppendLine($"  Точка: A(0; {LinearHelper.Fmt(yAxis)})");
            sb.AppendLine();

            // Пересечение с Ox (y = 0)
            sb.AppendLine("📍 Пересечение с осью Ox (y = 0):");
            if (Math.Abs(k) < 1e-12)
            {
                if (Math.Abs(b) < 1e-9)
                    sb.AppendLine("  Функция y = 0 — совпадает с осью Ox целиком.");
                else
                    sb.AppendLine($"  k = 0, функция y = {LinearHelper.Fmt(b)} — не пересекает ось Ox.");
            }
            else
            {
                double xAxis = -b / k;
                sb.AppendLine($"  {LinearHelper.Fmt(k)}x + {LinearHelper.Fmt(b)} = 0");
                sb.AppendLine($"  {LinearHelper.Fmt(k)}x = {LinearHelper.Fmt(-b)}");
                sb.AppendLine($"  x = {LinearHelper.Fmt(xAxis)}");
                sb.AppendLine($"  Точка: B({LinearHelper.Fmt(xAxis)}; 0)");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.7 Принадлежит ли точка графику?
    // ═══════════════════════════════════════════════════════════════

    public class LinearPointBelongsFunction : FunctionBase
    {
        public override string   Name       => "Принадлежит ли точка графику";
        public override string   Formula    => "Подставить x, проверить y";
        public override string[] Keywords   => new[] { "линейная", "точка", "принадлежит", "график" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Принадлежит ли точка графику?\n\n" +
                    "Точка M(x₀; y₀) принадлежит графику y = kx + b,\n" +
                    "если при подстановке x₀ получается y₀.\n\n" +
                    "Пример: y = 1.5x + 1,  точка A(1; 29/14)?\n" +
                    "  y = 1.5·1 + 1 = 2.5 ≠ 29/14 ≈ 2.07 → не принадлежит\n\n" +
                    "✏️ Введи формулу (правую часть после y =):\n" +
                    "  Пример: 1.5x+1  или  -x+3",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question =
                    "✏️ Введи координату x точки:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи координату y точки:",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            double px = LinearHelper.ParseNumber(answers[1]);
            double py = LinearHelper.ParseNumber(answers[2]);
            var sb = new StringBuilder();

            sb.AppendLine($"Функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine($"Точка:   M({LinearHelper.Fmt(px)}; {LinearHelper.Fmt(py)})");
            sb.AppendLine();

            double yCalc = k * px + b;
            sb.AppendLine($"Подставляем x = {LinearHelper.Fmt(px)}:");
            sb.AppendLine($"  y = {LinearHelper.Fmt(k)}·{LinearHelper.Fmt(px)} + ({LinearHelper.Fmt(b)})");
            sb.AppendLine($"  y = {LinearHelper.Fmt(yCalc)}");
            sb.AppendLine();

            bool belongs = Math.Abs(yCalc - py) < 1e-9;
            if (belongs)
            {
                sb.AppendLine($"  {LinearHelper.Fmt(yCalc)} = {LinearHelper.Fmt(py)} ✅");
                sb.AppendLine();
                sb.AppendLine("📌 Точка ПРИНАДЛЕЖИТ графику функции.");
            }
            else
            {
                sb.AppendLine($"  {LinearHelper.Fmt(yCalc)} ≠ {LinearHelper.Fmt(py)} ❌");
                sb.AppendLine();
                sb.AppendLine("📌 Точка НЕ принадлежит графику функции.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.10–22.11 Найти b, зная k и точку
    // ═══════════════════════════════════════════════════════════════

    public class LinearFindBFunction : FunctionBase
    {
        public override string   Name       => "Найти b по точке и k";
        public override string   Formula    => "b = y − kx";
        public override string[] Keywords   => new[] { "линейная", "найти b", "свободный член" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти b — свободный член\n\n" +
                    "Если известны k и точка (x₀; y₀) на графике:\n" +
                    "  y₀ = k·x₀ + b  →  b = y₀ − k·x₀\n\n" +
                    "Пример: y = -1.2x + b, точка A(0; 2.4)\n" +
                    "  b = 2.4 − (-1.2)·0 = 2.4\n\n" +
                    "✏️ Введи k (угловой коэффициент):",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи x точки:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи y точки:",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k  = LinearHelper.ParseNumber(answers[0]);
            double px = LinearHelper.ParseNumber(answers[1]);
            double py = LinearHelper.ParseNumber(answers[2]);
            var sb = new StringBuilder();

            double b = py - k * px;

            sb.AppendLine($"k = {LinearHelper.Fmt(k)},  точка ({LinearHelper.Fmt(px)}; {LinearHelper.Fmt(py)})");
            sb.AppendLine();
            sb.AppendLine("Подставляем в y = kx + b:");
            sb.AppendLine($"  {LinearHelper.Fmt(py)} = {LinearHelper.Fmt(k)}·{LinearHelper.Fmt(px)} + b");
            sb.AppendLine($"  {LinearHelper.Fmt(py)} = {LinearHelper.Fmt(k * px)} + b");
            sb.AppendLine($"  b = {LinearHelper.Fmt(py)} − {LinearHelper.Fmt(k * px)}");
            sb.AppendLine();
            sb.AppendLine($"📌 b = {LinearHelper.Fmt(b)}");
            sb.AppendLine($"   Функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.12–22.13 Найти k, зная b и точку
    // ═══════════════════════════════════════════════════════════════

    public class LinearFindKFunction : FunctionBase
    {
        public override string   Name       => "Найти k по точке и b";
        public override string   Formula    => "k = (y − b) / x";
        public override string[] Keywords   => new[] { "линейная", "найти k", "угловой коэффициент" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти k — угловой коэффициент\n\n" +
                    "Если известны b и точка (x₀; y₀) на графике:\n" +
                    "  y₀ = k·x₀ + b  →  k = (y₀ − b) / x₀\n\n" +
                    "Пример: y = kx + 1/3, точка N(1; 4)\n" +
                    "  k = (4 − 1/3) / 1 = 11/3\n\n" +
                    "✏️ Введи b (свободный член):",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи x точки:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи y точки:",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double b  = LinearHelper.ParseNumber(answers[0]);
            double px = LinearHelper.ParseNumber(answers[1]);
            double py = LinearHelper.ParseNumber(answers[2]);
            var sb = new StringBuilder();

            sb.AppendLine($"b = {LinearHelper.Fmt(b)},  точка ({LinearHelper.Fmt(px)}; {LinearHelper.Fmt(py)})");
            sb.AppendLine();

            if (Math.Abs(px) < 1e-12)
            {
                sb.AppendLine("⚠️ x точки равен 0 — нельзя найти k через деление.");
                sb.AppendLine($"  Из y = k·0 + b следует b = {LinearHelper.Fmt(py)}.");
                sb.AppendLine("  Это подтверждает b, а k может быть любым числом.");
                return sb.ToString().TrimEnd();
            }

            double k = (py - b) / px;

            sb.AppendLine("Подставляем в y = kx + b:");
            sb.AppendLine($"  {LinearHelper.Fmt(py)} = k·{LinearHelper.Fmt(px)} + {LinearHelper.Fmt(b)}");
            sb.AppendLine($"  k·{LinearHelper.Fmt(px)} = {LinearHelper.Fmt(py)} − {LinearHelper.Fmt(b)}");
            sb.AppendLine($"  k·{LinearHelper.Fmt(px)} = {LinearHelper.Fmt(py - b)}");
            sb.AppendLine($"  k = {LinearHelper.Fmt(py - b)} / {LinearHelper.Fmt(px)}");
            sb.AppendLine();
            sb.AppendLine($"📌 k = {LinearHelper.Fmt(k)}");
            sb.AppendLine($"   Функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  22.14 Знак функции: при каких x значение y > 0 или y < 0
    // ═══════════════════════════════════════════════════════════════

    public class LinearSignFunction : FunctionBase
    {
        public override string   Name       => "Знак функции (y > 0 и y < 0)";
        public override string   Formula    => "y = kx + b: при каких x положительна/отрицательна";
        public override string[] Keywords   => new[] { "линейная", "знак", "положительная", "отрицательная" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Знак линейной функции\n\n" +
                    "Нужно найти при каких x:\n" +
                    "  · y > 0 (функция положительна)\n" +
                    "  · y < 0 (функция отрицательна)\n\n" +
                    "Метод: находим нуль функции x₀ = −b/k,\n" +
                    "затем смотрим на знак k:\n" +
                    "  · k > 0: y < 0 при x < x₀,  y > 0 при x > x₀\n" +
                    "  · k < 0: y > 0 при x < x₀,  y < 0 при x > x₀\n\n" +
                    "✏️ Введи формулу (правую часть после y =):\n" +
                    "  Пример: 3x-6  или  -2x+4",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine();

            if (Math.Abs(k) < 1e-12)
            {
                // Постоянная функция
                sb.AppendLine($"k = 0 — функция постоянная: y = {LinearHelper.Fmt(b)}");
                sb.AppendLine();
                if (b > 1e-9)
                    sb.AppendLine($"📌 y > 0 при любом x\n   y < 0: никогда");
                else if (b < -1e-9)
                    sb.AppendLine($"📌 y > 0: никогда\n   y < 0 при любом x");
                else
                    sb.AppendLine("📌 y = 0 всегда — функция совпадает с осью Ox.");
                return sb.ToString().TrimEnd();
            }

            double x0 = -b / k;
            sb.AppendLine($"Шаг 1. Находим нуль функции (y = 0):");
            sb.AppendLine($"  {LinearHelper.Fmt(k)}x + {LinearHelper.Fmt(b)} = 0");
            sb.AppendLine($"  x = {LinearHelper.Fmt(x0)}");
            sb.AppendLine();

            sb.AppendLine($"Шаг 2. Знак k = {LinearHelper.Fmt(k)} ({(k > 0 ? "положительный" : "отрицательный")}):");
            sb.AppendLine();

            if (k > 0)
            {
                sb.AppendLine($"  При x < {LinearHelper.Fmt(x0)}:  y < 0  (функция отрицательна)");
                sb.AppendLine($"  При x = {LinearHelper.Fmt(x0)}:  y = 0  (нуль функции)");
                sb.AppendLine($"  При x > {LinearHelper.Fmt(x0)}:  y > 0  (функция положительна)");
                sb.AppendLine();
                sb.AppendLine($"📌 y > 0  при  x ∈ ({LinearHelper.Fmt(x0)}; +∞)");
                sb.AppendLine($"   y < 0  при  x ∈ (−∞; {LinearHelper.Fmt(x0)})");
            }
            else
            {
                sb.AppendLine($"  При x < {LinearHelper.Fmt(x0)}:  y > 0  (функция положительна)");
                sb.AppendLine($"  При x = {LinearHelper.Fmt(x0)}:  y = 0  (нуль функции)");
                sb.AppendLine($"  При x > {LinearHelper.Fmt(x0)}:  y < 0  (функция отрицательна)");
                sb.AppendLine();
                sb.AppendLine($"📌 y > 0  при  x ∈ (−∞; {LinearHelper.Fmt(x0)})");
                sb.AppendLine($"   y < 0  при  x ∈ ({LinearHelper.Fmt(x0)}; +∞)");
            }

            return sb.ToString().TrimEnd();
        }
    }
}

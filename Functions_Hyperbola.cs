using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ─── Вспомогательный класс ───────────────────────────────────

    internal static class HyperbolaHelper
    {
        public static string Fmt(double v)
        {
            if (Math.Abs(v - Math.Round(v)) < 1e-9)
                return ((long)Math.Round(v)).ToString();
            // Дробь
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

        public static string FormatHyperbola(double k)
        {
            if (Math.Abs(k - 1) < 1e-9)  return "y = 1/x";
            if (Math.Abs(k + 1) < 1e-9)  return "y = −1/x";
            return k > 0 ? $"y = {Fmt(k)}/x" : $"y = −{Fmt(-k)}/x";
        }

        public static double? ParseNumber(string s)
        {
            s = s.Trim().Replace(",", ".").Replace("−", "-");
            if (s.Contains('/'))
            {
                var p = s.Split('/');
                if (p.Length == 2 &&
                    double.TryParse(p[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double n) &&
                    double.TryParse(p[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double d) &&
                    Math.Abs(d) > 1e-12)
                    return n / d;
                return null;
            }
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                return v;
            return null;
        }

        public static string? ValidateNumber(string s)
            => ParseNumber(s) == null ? "Введите число (например: 3  или  -2  или  1/3)" : null;

        public static string? ValidateNonZeroNumber(string s)
        {
            var v = ParseNumber(s);
            if (v == null) return "Введите число";
            if (Math.Abs(v.Value) < 1e-12) return "k не может быть равен 0";
            return null;
        }

        /// <summary>Свойства функции y = k/x.</summary>
        public static string GetProperties(double k)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"📊 Свойства функции {FormatHyperbola(k)}:");
            sb.AppendLine();
            sb.AppendLine("1️⃣ Область определения:");
            sb.AppendLine("   D(y) = (−∞; 0) ∪ (0; +∞)");
            sb.AppendLine("   Функция определена при любом x ≠ 0.");
            sb.AppendLine();
            sb.AppendLine("2️⃣ Область значений:");
            sb.AppendLine("   E(y) = (−∞; 0) ∪ (0; +∞)");
            sb.AppendLine("   y принимает любое значение, кроме 0.");
            sb.AppendLine();
            sb.AppendLine("3️⃣ Нули функции:");
            sb.AppendLine("   Нулей нет — y ≠ 0 при любом x из области определения.");
            sb.AppendLine("   График не пересекает ось Ox.");
            sb.AppendLine();
            sb.AppendLine("4️⃣ Знак функции:");
            if (k > 0)
            {
                sb.AppendLine("   k > 0:");
                sb.AppendLine("   y > 0 при x ∈ (0; +∞)  — I четверть");
                sb.AppendLine("   y < 0 при x ∈ (−∞; 0)  — III четверть");
            }
            else
            {
                sb.AppendLine("   k < 0:");
                sb.AppendLine("   y > 0 при x ∈ (−∞; 0)  — II четверть");
                sb.AppendLine("   y < 0 при x ∈ (0; +∞)  — IV четверть");
            }
            sb.AppendLine();
            sb.AppendLine("5️⃣ Возрастание и убывание:");
            if (k < 0)
            {
                sb.AppendLine("   k < 0: функция ВОЗРАСТАЮЩАЯ на каждой ветви:");
                sb.AppendLine("   Возрастает на (−∞; 0)");
                sb.AppendLine("   Возрастает на (0; +∞)");
            }
            else
            {
                sb.AppendLine("   k > 0: функция УБЫВАЮЩАЯ на каждой ветви:");
                sb.AppendLine("   Убывает на (−∞; 0)");
                sb.AppendLine("   Убывает на (0; +∞)");
            }
            sb.AppendLine();
            sb.AppendLine("6️⃣ Симметрия:");
            sb.AppendLine("   Гипербола симметрична относительно начала координат O(0; 0).");
            sb.AppendLine("   Центр симметрии: (0; 0).");
            sb.AppendLine();
            if (k > 0)
                sb.AppendLine($"📌 График — гипербола, ветви в I и III четвертях (k = {Fmt(k)} > 0)");
            else
                sb.AppendLine($"📌 График — гипербола, ветви во II и IV четвертях (k = {Fmt(k)} < 0)");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.1  Принадлежит ли точка графику y = k/x ─────────────

    public class HyperbolaPointBelongsFunction : FunctionBase
    {
        public override string   Name     => "Лежит ли точка на гиперболе?";
        public override string   Formula  => "Проверить: принадлежит ли (x₀; y₀) графику y = k/x";
        public override string[] Keywords => new[] { "гипербола", "принадлежит", "точка", "k/x" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Принадлежит ли точка графику y = k/x?\n\n" +
                    "Метод: подставить x₀ в формулу и сравнить с y₀.\n" +
                    "Если k/x₀ = y₀ — точка принадлежит.\n\n" +
                    "✏️ Введи k (коэффициент):\n" +
                    "  Пример: 1  или  -3  или  0.5",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи координату x₀ точки:\n" +
                           "  (x₀ ≠ 0)",
                Validate = s =>
                {
                    var v = HyperbolaHelper.ParseNumber(s);
                    if (v == null) return "Введите число";
                    if (Math.Abs(v.Value) < 1e-12) return "x не может быть равен 0";
                    return null;
                }
            },
            new InputStep
            {
                Question = "✏️ Введи координату y₀ точки:",
                Validate = HyperbolaHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k  = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            double x0 = HyperbolaHelper.ParseNumber(answers[1])!.Value;
            double y0 = HyperbolaHelper.ParseNumber(answers[2])!.Value;

            double computed = k / x0;
            bool belongs    = Math.Abs(computed - y0) < 1e-9;

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {HyperbolaHelper.FormatHyperbola(k)}");
            sb.AppendLine($"Точка:   ({HyperbolaHelper.Fmt(x0)}; {HyperbolaHelper.Fmt(y0)})");
            sb.AppendLine();
            sb.AppendLine($"Подставляем x₀ = {HyperbolaHelper.Fmt(x0)}:");
            sb.AppendLine($"  y = {HyperbolaHelper.Fmt(k)} / {HyperbolaHelper.Fmt(x0)}");
            sb.AppendLine($"  y = {HyperbolaHelper.Fmt(computed)}");
            sb.AppendLine();
            sb.AppendLine(belongs
                ? $"✅ {HyperbolaHelper.Fmt(computed)} = {HyperbolaHelper.Fmt(y0)}  — точка ПРИНАДЛЕЖИТ графику"
                : $"❌ {HyperbolaHelper.Fmt(computed)} ≠ {HyperbolaHelper.Fmt(y0)}  — точка НЕ ПРИНАДЛЕЖИТ графику");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.2  Построить y = k/x: все свойства + график ─────────

    public class HyperbolaPlotFunction : FunctionBase
    {
        public override string   Name     => "Построить гиперболу y = k/x";
        public override string   Formula  => "y = k/x — гипербола: свойства и график";
        public override string[] Keywords => new[] { "гипербола", "k/x", "построить", "свойства" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Построить график y = k/x\n\n" +
                    "Бот построит гиперболу и выведет все свойства:\n" +
                    "  · область определения и значений\n" +
                    "  · нули функции\n" +
                    "  · знак функции\n" +
                    "  · возрастание / убывание\n" +
                    "  · симметрия\n\n" +
                    "✏️ Введи k (коэффициент, k ≠ 0):\n" +
                    "  Пример: 1  или  -3  или  4",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return PlotHelper.HyperbolaPlot(k);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return HyperbolaHelper.GetProperties(k);
        }
    }

    // ─── 27.3  Два графика в одной системе координат ─────────────

    public class HyperbolaTwoGraphsFunction : FunctionBase
    {
        public override string   Name     => "Сравнить две гиперболы";
        public override string   Formula  => "Построить и сравнить две гиперболы";
        public override string[] Keywords => new[] { "гипербола", "два графика", "сравнить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Два графика y = k₁/x и y = k₂/x в одной системе\n\n" +
                    "Бот построит обе гиперболы и сравнит их.\n\n" +
                    "✏️ Введи k₁ (первый коэффициент):\n" +
                    "  Пример: 2  или  -1",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи k₂ (второй коэффициент):\n" +
                           "  Пример: 1  или  0.5",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k1 = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            double k2 = HyperbolaHelper.ParseNumber(answers[1])!.Value;
            return PlotHelper.TwoHyperbolaPlot(k1, k2);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k1 = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            double k2 = HyperbolaHelper.ParseNumber(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Функции: {HyperbolaHelper.FormatHyperbola(k1)}  и  {HyperbolaHelper.FormatHyperbola(k2)}");
            sb.AppendLine();
            sb.AppendLine("📐 Сравнение:");

            bool sameSign = (k1 > 0 && k2 > 0) || (k1 < 0 && k2 < 0);
            sb.AppendLine(sameSign
                ? "  k₁ и k₂ одного знака → ветви в одних и тех же четвертях"
                : "  k₁ и k₂ разных знаков → ветви в разных четвертях");

            sb.AppendLine();
            double abs1 = Math.Abs(k1), abs2 = Math.Abs(k2);
            if (Math.Abs(abs1 - abs2) < 1e-9)
                sb.AppendLine("  |k₁| = |k₂| — ветви одинаковой «ширины»");
            else if (abs1 > abs2)
                sb.AppendLine($"  |k₁| = {HyperbolaHelper.Fmt(abs1)} > |k₂| = {HyperbolaHelper.Fmt(abs2)} → первая гипербола дальше от начала координат");
            else
                sb.AppendLine($"  |k₂| = {HyperbolaHelper.Fmt(abs2)} > |k₁| = {HyperbolaHelper.Fmt(abs1)} → вторая гипербола дальше от начала координат");

            sb.AppendLine();
            sb.AppendLine("📍 Обе гиперболы имеют центр симметрии O(0; 0)");
            sb.AppendLine("   Обе не пересекают оси координат");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.4  Заполнить таблицу по формуле y = k/x ─────────────

    public class HyperbolaTableFunction : FunctionBase
    {
        public override string   Name     => "Составить таблицу для y = k/x";
        public override string   Formula  => "y = k/x → подставить x, найти y";
        public override string[] Keywords => new[] { "гипербола", "таблица", "заполнить", "k/x" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Заполнить таблицу для y = k/x\n\n" +
                    "Подставляем каждое x в формулу и находим y.\n\n" +
                    "Пример: y = 1/x\n" +
                    "  x = −2 → y = 1/(−2) = −1/2\n" +
                    "  x = 1  → y = 1/1   = 1\n\n" +
                    "✏️ Введи k (коэффициент):\n" +
                    "  Пример: 1  или  3  или  -5",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи значения x через запятую:\n" +
                    "  (x ≠ 0 для каждого)\n" +
                    "  Пример: -2, -1, 1, 2",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Введи значения x через запятую";
                    var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1) return "Введи хотя бы одно значение";
                    foreach (var p in parts)
                    {
                        var v = HyperbolaHelper.ParseNumber(p.Trim());
                        if (v == null) return $"«{p.Trim()}» — не число";
                        if (Math.Abs(v.Value) < 1e-12) return "x не может быть равен 0";
                    }
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k  = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            var xStrs = answers[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(p => p.Trim()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {HyperbolaHelper.FormatHyperbola(k)}");
            sb.AppendLine();
            sb.AppendLine("Подставляем каждое x:");

            var yStrs = new List<string>();
            foreach (var xStr in xStrs)
            {
                double x = HyperbolaHelper.ParseNumber(xStr)!.Value;
                double y = k / x;
                string yStr = HyperbolaHelper.Fmt(y);
                sb.AppendLine($"  x = {xStr} → y = {HyperbolaHelper.Fmt(k)} / {xStr} = {yStr}");
                yStrs.Add(yStr);
            }

            sb.AppendLine();
            sb.AppendLine("📌 Таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xStrs)}");
            sb.AppendLine($"  y: {string.Join("  ", yStrs)}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.5  Имеет ли уравнение корни (графически) ────────────
    //  Проверяет: пересекает ли y = k/x данную функцию
    //  Поддержка: y = kx+b, y = ax², y = c (константа)

    public class HyperbolaRootsFunction : FunctionBase
    {
        public override string   Name     => "Есть ли решение у уравнения?";
        public override string   Formula  => "k/x = f(x): найти точки пересечения";
        public override string[] Keywords => new[] { "гипербола", "корни", "уравнение", "пересечение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Корни уравнения k/x = f(x) (графически)\n\n" +
                    "Метод: строим y = k/x и y = f(x).\n" +
                    "Точки пересечения — корни уравнения.\n\n" +
                    "✏️ Введи k (коэффициент гиперболы):\n" +
                    "  Пример: 4  или  -2",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения f(x):\n\n" +
                    "  Поддерживаемые форматы:\n" +
                    "  · число (например: 3)\n" +
                    "  · линейная: 3x+2  или  -x+1\n" +
                    "  · квадратичная: 2x^2  или  -x^2\n" +
                    "  · гипербола: 1/x  или  -2/x\n\n" +
                    "  Примеры:\n" +
                    "  4/x = 3x+2  → введи: 3x+2",
                Validate = s => string.IsNullOrWhiteSpace(s)
                    ? "Ты ничего не ввёл"
                    : null
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            string fRaw = answers[1].Trim();
            return PlotHelper.HyperbolaWithFunction(k, fRaw);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k   = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            string raw = answers[1].Trim().Replace(" ", "").Replace("−", "-").Replace(",", ".");

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {HyperbolaHelper.Fmt(k)}/x = {answers[1].Trim()}");
            sb.AppendLine();

            // Попытаться распознать правую часть
            // 1. Число
            if (HyperbolaHelper.ParseNumber(raw) is double c)
            {
                // k/x = c  →  x = k/c
                sb.AppendLine($"Правая часть — константа: y = {HyperbolaHelper.Fmt(c)}");
                sb.AppendLine($"Решаем: {HyperbolaHelper.Fmt(k)}/x = {HyperbolaHelper.Fmt(c)}");
                if (Math.Abs(c) < 1e-12)
                {
                    sb.AppendLine("c = 0: k/x = 0 — решений нет (y = k/x ≠ 0 никогда)");
                    sb.AppendLine("\n📌 Корней нет.");
                }
                else
                {
                    double x0 = k / c;
                    sb.AppendLine($"  x = {HyperbolaHelper.Fmt(k)} / {HyperbolaHelper.Fmt(c)} = {HyperbolaHelper.Fmt(x0)}");
                    sb.AppendLine($"\n📌 Один корень: x = {HyperbolaHelper.Fmt(x0)}");
                }
                return sb.ToString().TrimEnd();
            }

            // 2. Линейная: kx+b
            var lin = LinearHelper.ParseLinear(raw);
            if (lin != null)
            {
                var (kL, b) = lin.Value;
                // k/x = kL*x + b  →  k = kL*x² + b*x  →  kL*x² + b*x - k = 0
                sb.AppendLine($"Правая часть — линейная: {LinearHelper.FormatLinear(kL, b)}");
                sb.AppendLine();
                if (Math.Abs(kL) < 1e-12)
                {
                    // k/x = b  →  x = k/b
                    sb.AppendLine($"Коэффициент при x равен 0 → {HyperbolaHelper.Fmt(k)}/x = {HyperbolaHelper.Fmt(b)}");
                    if (Math.Abs(b) < 1e-12)
                    {
                        sb.AppendLine("\n📌 Корней нет.");
                    }
                    else
                    {
                        double x0 = k / b;
                        sb.AppendLine($"  x = {HyperbolaHelper.Fmt(k)} / {HyperbolaHelper.Fmt(b)} = {HyperbolaHelper.Fmt(x0)}");
                        sb.AppendLine($"\n📌 Один корень: x = {HyperbolaHelper.Fmt(x0)}");
                    }
                    return sb.ToString().TrimEnd();
                }

                double A = kL, B = b, C = -k;
                double D = B * B - 4 * A * C;
                sb.AppendLine($"Умножаем обе части на x (x ≠ 0):");
                sb.AppendLine($"  {HyperbolaHelper.Fmt(k)} = {HyperbolaHelper.Fmt(kL)}x² + {HyperbolaHelper.Fmt(b)}x");
                sb.AppendLine($"  {HyperbolaHelper.Fmt(kL)}x² + {HyperbolaHelper.Fmt(b)}x − {HyperbolaHelper.Fmt(k)} = 0");
                sb.AppendLine();
                sb.AppendLine($"D = {HyperbolaHelper.Fmt(b)}² − 4·{HyperbolaHelper.Fmt(kL)}·({HyperbolaHelper.Fmt(C)}) = {HyperbolaHelper.Fmt(D)}");
                sb.AppendLine();

                if (D < -1e-9)
                {
                    sb.AppendLine("D < 0");
                    sb.AppendLine("\n📌 Уравнение не имеет корней.");
                }
                else if (Math.Abs(D) < 1e-9)
                {
                    double x0 = -B / (2 * A);
                    sb.AppendLine($"D = 0 → один корень:");
                    sb.AppendLine($"  x = {HyperbolaHelper.Fmt(x0)}");
                    sb.AppendLine($"\n📌 Один корень: x = {HyperbolaHelper.Fmt(x0)}");
                }
                else
                {
                    double sqD = Math.Sqrt(D);
                    double x1 = (-B - sqD) / (2 * A);
                    double x2 = (-B + sqD) / (2 * A);
                    if (x1 > x2) (x1, x2) = (x2, x1);
                    sb.AppendLine($"D > 0 → два корня:");
                    sb.AppendLine($"  x₁ = {HyperbolaHelper.Fmt(x1)}");
                    sb.AppendLine($"  x₂ = {HyperbolaHelper.Fmt(x2)}");
                    sb.AppendLine($"\n📌 Два корня: x₁ = {HyperbolaHelper.Fmt(x1)},  x₂ = {HyperbolaHelper.Fmt(x2)}");
                }
                return sb.ToString().TrimEnd();
            }

            // 3. Квадратичная: ax^2
            var qParsed = QuadraticHelper.ParseQuadratic(raw);
            if (qParsed != null)
            {
                double a = qParsed.Value;
                // k/x = ax²  →  k = ax³  →  x³ = k/a
                sb.AppendLine($"Правая часть — квадратичная: y = {QuadraticHelper.Fmt(a)}x²");
                sb.AppendLine();
                sb.AppendLine("Умножаем обе части на x (x ≠ 0):");
                sb.AppendLine($"  {HyperbolaHelper.Fmt(k)} = {HyperbolaHelper.Fmt(a)}x³");
                sb.AppendLine($"  x³ = {HyperbolaHelper.Fmt(k)} / {HyperbolaHelper.Fmt(a)} = {HyperbolaHelper.Fmt(k / a)}");
                double x0 = Math.Cbrt(k / a);
                sb.AppendLine($"  x = ∛{HyperbolaHelper.Fmt(k / a)} = {HyperbolaHelper.Fmt(x0)}");
                sb.AppendLine($"\n📌 Один корень: x = {HyperbolaHelper.Fmt(x0)}");
                return sb.ToString().TrimEnd();
            }

            // 4. Другая гипербола: k2/x
            // Формат: "число/x" или "-число/x"
            var m = System.Text.RegularExpressions.Regex.Match(raw, @"^([+-]?[0-9]*\.?[0-9]+)/x$");
            if (m.Success && double.TryParse(m.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double k2))
            {
                // k/x = k2/x  →  k = k2
                sb.AppendLine($"Правая часть — тоже гипербола: y = {HyperbolaHelper.Fmt(k2)}/x");
                sb.AppendLine();
                if (Math.Abs(k - k2) < 1e-9)
                {
                    sb.AppendLine("Гиперболы совпадают — бесконечно много решений.");
                    sb.AppendLine("\n📌 Любой x ≠ 0 является решением.");
                }
                else
                {
                    sb.AppendLine("Гиперболы не пересекаются — они параллельны.");
                    sb.AppendLine("\n📌 Корней нет.");
                }
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("⚠️ Не удалось автоматически определить тип функции.");
            sb.AppendLine("Поддерживаются: число, kx+b, ax^2, k/x");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.6  Решить уравнение k/x = f(x) графически ───────────
    //  (то же что RootsFunction, но с явным акцентом на графике)

    public class HyperbolaSolveGraphicallyFunction : FunctionBase
    {
        public override string   Name     => "Решить уравнение по графику";
        public override string   Formula  => "k/x = f(x) → строим оба графика";
        public override string[] Keywords => new[] { "гипербола", "решить", "графически", "уравнение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решить уравнение графически\n\n" +
                    "Строим y = k/x и вторую функцию.\n" +
                    "Абсциссы точек пересечения — решения уравнения.\n\n" +
                    "✏️ Введи k (коэффициент гиперболы):\n" +
                    "  Пример: 4  или  -2",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть: число, kx+b или ax^2\n" +
                    "  Пример: 3x+2  или  -x  или  5  или  x^2",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Введи функцию" : null
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return PlotHelper.HyperbolaWithFunction(k, answers[1].Trim());
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            // Делегируем к HyperbolaRootsFunction
            var roots = new HyperbolaRootsFunction();
            return roots.CalculateFromAnswers(answers);
        }
    }

    // ─── 27.7  Пересекается ли y = k/x с y = f(x) ───────────────

    public class HyperbolaIntersectFunction : FunctionBase
    {
        public override string   Name     => "Пересекается ли гипербола с прямой?";
        public override string   Formula  => "y = k/x и y = f(x): есть ли общие точки?";
        public override string[] Keywords => new[] { "гипербола", "пересекается", "функция" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Пересекается ли гипербола с другой функцией?\n\n" +
                    "Проверим есть ли точки пересечения.\n\n" +
                    "✏️ Введи k (коэффициент гиперболы):\n" +
                    "  Пример: 4  или  -5",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи формулу второй функции:\n" +
                    "  Число, kx+b, ax^2, k/x\n" +
                    "  Пример: -x  или  2x+1  или  x^2",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Введи функцию" : null
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return PlotHelper.HyperbolaWithFunction(k, answers[1].Trim());
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var roots = new HyperbolaRootsFunction();
            string result = roots.CalculateFromAnswers(answers);

            // Обогащаем ответ
            var sb = new StringBuilder();
            sb.AppendLine(result);
            sb.AppendLine();
            if (result.Contains("Корней нет") || result.Contains("не имеет корней"))
                sb.AppendLine("📌 Вывод: функции НЕ пересекаются.");
            else if (result.Contains("бесконечно много"))
                sb.AppendLine("📌 Вывод: функции совпадают (пересекаются во всех точках).");
            else
                sb.AppendLine("📌 Вывод: функции ПЕРЕСЕКАЮТСЯ.");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.8  Могут ли y = k/x и y = ax+b пересечься в N точках ─

    public class HyperbolaCanIntersectFunction : FunctionBase
    {
        public override string   Name     => "Сколько общих точек у гиперболы и прямой?";
        public override string   Formula  => "y = k/x и y = ax+b: 0, 1 или 2 точки?";
        public override string[] Keywords => new[] { "гипербола", "прямая", "сколько точек", "пересечение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сколько точек пересечения у y = k/x и y = ax + b?\n\n" +
                    "Метод:\n" +
                    "  k/x = ax + b  →  ax² + bx − k = 0\n" +
                    "  D = b² + 4ak\n\n" +
                    "  D > 0 → 2 точки\n" +
                    "  D = 0 → 1 точка\n" +
                    "  D < 0 → 0 точек\n\n" +
                    "✏️ Введи k (коэффициент гиперболы):\n" +
                    "  Пример: 4  или  -1",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи формулу прямой (после y =):\n" +
                    "  Пример: 3x+2  или  -x+1  или  2x",
                Validate = s => LinearHelper.ParseLinear(s) == null
                    ? "Введи линейную функцию, например: 3x+2"
                    : null
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return PlotHelper.HyperbolaWithFunction(k, answers[1].Trim());
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k       = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            var (a, b) = LinearHelper.ParseLinear(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Гипербола: {HyperbolaHelper.FormatHyperbola(k)}");
            sb.AppendLine($"Прямая:    {LinearHelper.FormatLinear(a, b)}");
            sb.AppendLine();

            if (Math.Abs(a) < 1e-12)
            {
                // Горизонтальная прямая y = b
                sb.AppendLine($"Прямая горизонтальная: y = {HyperbolaHelper.Fmt(b)}");
                if (Math.Abs(b) < 1e-12)
                {
                    sb.AppendLine("📌 Нет точек пересечения (y = k/x ≠ 0)");
                }
                else
                {
                    double x0 = k / b;
                    sb.AppendLine($"  k/x = {HyperbolaHelper.Fmt(b)}  →  x = {HyperbolaHelper.Fmt(x0)}");
                    sb.AppendLine($"📌 Одна точка: ({HyperbolaHelper.Fmt(x0)}; {HyperbolaHelper.Fmt(b)})");
                }
                return sb.ToString().TrimEnd();
            }

            // k/x = ax + b  →  ax² + bx - k = 0
            sb.AppendLine("Составляем уравнение:");
            sb.AppendLine($"  {HyperbolaHelper.Fmt(k)}/x = {LinearHelper.FormatLinear(a, b).Replace("y = ", "")}");
            sb.AppendLine($"  Умножаем на x: {HyperbolaHelper.Fmt(k)} = {HyperbolaHelper.Fmt(a)}x² + {HyperbolaHelper.Fmt(b)}x");
            sb.AppendLine($"  {HyperbolaHelper.Fmt(a)}x² + {HyperbolaHelper.Fmt(b)}x − {HyperbolaHelper.Fmt(k)} = 0");
            sb.AppendLine();

            double D = b * b + 4 * a * k;
            sb.AppendLine($"D = b² + 4ak = ({HyperbolaHelper.Fmt(b)})² + 4·{HyperbolaHelper.Fmt(a)}·{HyperbolaHelper.Fmt(k)} = {HyperbolaHelper.Fmt(D)}");
            sb.AppendLine();

            if (D < -1e-9)
            {
                sb.AppendLine("D < 0");
                sb.AppendLine("📌 Прямая и гипербола НЕ пересекаются (0 точек).");
            }
            else if (Math.Abs(D) < 1e-9)
            {
                double x0 = -b / (2 * a);
                double y0 = k / x0;
                sb.AppendLine("D = 0 — одна точка касания.");
                sb.AppendLine($"📌 Одна точка: ({HyperbolaHelper.Fmt(x0)}; {HyperbolaHelper.Fmt(y0)})");
            }
            else
            {
                double sqD = Math.Sqrt(D);
                double x1  = (-b - sqD) / (2 * a);
                double x2  = (-b + sqD) / (2 * a);
                if (x1 > x2) (x1, x2) = (x2, x1);
                double y1 = k / x1, y2 = k / x2;
                sb.AppendLine("D > 0 — две точки пересечения.");
                sb.AppendLine($"  x₁ = {HyperbolaHelper.Fmt(x1)},  x₂ = {HyperbolaHelper.Fmt(x2)}");
                sb.AppendLine($"📌 Две точки:");
                sb.AppendLine($"   ({HyperbolaHelper.Fmt(x1)}; {HyperbolaHelper.Fmt(y1)})");
                sb.AppendLine($"   ({HyperbolaHelper.Fmt(x2)}; {HyperbolaHelper.Fmt(y2)})");
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.9  Построить y = k/|x| или y = 1/|x| ────────────────

    public class HyperbolaAbsFunction : FunctionBase
    {
        public override string   Name     => "Построить y = k/|x|";
        public override string   Formula  => "y = k/|x| — обе ветви в одном знаке";
        public override string[] Keywords => new[] { "гипербола", "модуль", "k/|x|", "abs" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 График y = k/|x|\n\n" +
                    "Отличие от y = k/x:\n" +
                    "  |x| всегда ≥ 0 → знак y определяется только знаком k.\n\n" +
                    "  k > 0: обе ветви в I и II четвертях (y > 0)\n" +
                    "  k < 0: обе ветви в III и IV четвертях (y < 0)\n\n" +
                    "✏️ Введи k:\n" +
                    "  Пример: 2  или  -1  или  0.2",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            return PlotHelper.HyperbolaAbsPlot(k);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k = HyperbolaHelper.ParseNumber(answers[0])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: y = {HyperbolaHelper.Fmt(k)}/|x|");
            sb.AppendLine();
            sb.AppendLine("📊 Свойства:");
            sb.AppendLine();
            sb.AppendLine("1️⃣ Область определения:");
            sb.AppendLine("   D(y) = (−∞; 0) ∪ (0; +∞)  (x ≠ 0)");
            sb.AppendLine();
            sb.AppendLine("2️⃣ Знак функции:");
            if (k > 0)
            {
                sb.AppendLine("   k > 0 → y > 0 при всех x из области определения");
                sb.AppendLine("   Обе ветви расположены в I и II четвертях.");
            }
            else
            {
                sb.AppendLine("   k < 0 → y < 0 при всех x из области определения");
                sb.AppendLine("   Обе ветви расположены в III и IV четвертях.");
            }
            sb.AppendLine();
            sb.AppendLine("3️⃣ Симметрия:");
            sb.AppendLine("   График симметричен относительно оси Oy.");
            sb.AppendLine("   (в отличие от y = k/x, где симметрия относительно O)");
            sb.AppendLine();
            sb.AppendLine("4️⃣ Нули функции:");
            sb.AppendLine("   Нулей нет — y ≠ 0 при всех x ≠ 0.");
            sb.AppendLine();
            sb.AppendLine($"📌 y = {HyperbolaHelper.Fmt(k)}/|x|: обе ветви {(k > 0 ? "над" : "под")} осью Ox");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.10  Наибольшее/наименьшее на промежутке ──────────────

    public class HyperbolaMinMaxFunction : FunctionBase
    {
        public override string   Name     => "Наибольшее и наименьшее y = k/|x|";
        public override string   Formula  => "max и min функции y = k/|x| на промежутке";
        public override string[] Keywords => new[] { "гипербола", "наибольшее", "наименьшее", "модуль", "промежуток" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Наибольшее и наименьшее значение y = k/|x| на промежутке\n\n" +
                    "Метод: y = k/|x|, |x| растёт → |y| убывает.\n" +
                    "Наибольшее |y| — при наименьшем |x|.\n\n" +
                    "✏️ Введи k:\n" +
                    "  Пример: 2  или  -3",
                Validate = HyperbolaHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи левую границу промежутка (x > 0):\n" +
                           "  Пример: 2  или  0.5",
                Validate = s =>
                {
                    var v = HyperbolaHelper.ParseNumber(s);
                    if (v == null) return "Введите число";
                    if (v.Value <= 1e-12) return "Граница должна быть > 0";
                    return null;
                }
            },
            new InputStep
            {
                Question = "✏️ Введи правую границу промежутка:",
                Validate = s =>
                {
                    var v = HyperbolaHelper.ParseNumber(s);
                    if (v == null) return "Введите число";
                    if (v.Value <= 1e-12) return "Граница должна быть > 0";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k  = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            double x1 = HyperbolaHelper.ParseNumber(answers[1])!.Value;
            double x2 = HyperbolaHelper.ParseNumber(answers[2])!.Value;
            if (x1 > x2) (x1, x2) = (x2, x1);

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: y = {HyperbolaHelper.Fmt(k)}/|x|");
            sb.AppendLine($"Промежуток: [{HyperbolaHelper.Fmt(x1)}; {HyperbolaHelper.Fmt(x2)}]  (x > 0)");
            sb.AppendLine();

            double y1 = k / x1;
            double y2 = k / x2;

            sb.AppendLine($"y({HyperbolaHelper.Fmt(x1)}) = {HyperbolaHelper.Fmt(k)}/{HyperbolaHelper.Fmt(x1)} = {HyperbolaHelper.Fmt(y1)}");
            sb.AppendLine($"y({HyperbolaHelper.Fmt(x2)}) = {HyperbolaHelper.Fmt(k)}/{HyperbolaHelper.Fmt(x2)} = {HyperbolaHelper.Fmt(y2)}");
            sb.AppendLine();

            sb.AppendLine("Пояснение: при x > 0 функция y = k/x строго монотонна.");
            sb.AppendLine(k > 0
                ? "  k > 0: убывает → max при x = x₁ (левый конец), min при x = x₂"
                : "  k < 0: возрастает → min при x = x₁, max при x = x₂");

            sb.AppendLine();
            double yMax = Math.Max(y1, y2);
            double yMin = Math.Min(y1, y2);
            double xMax = Math.Abs(y1 - yMax) < 1e-9 ? x1 : x2;
            double xMin = Math.Abs(y1 - yMin) < 1e-9 ? x1 : x2;

            sb.AppendLine($"📌 Наибольшее значение: y = {HyperbolaHelper.Fmt(yMax)}  при x = {HyperbolaHelper.Fmt(xMax)}");
            sb.AppendLine($"📌 Наименьшее значение: y = {HyperbolaHelper.Fmt(yMin)}  при x = {HyperbolaHelper.Fmt(xMin)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ─── 27.11  Найти формулу y = k/x по точке ───────────────────

    public class HyperbolaFindKFunction : FunctionBase
    {
        public override string   Name     => "Найти k для гиперболы по точке";
        public override string   Formula  => "k = x₀ · y₀";
        public override string[] Keywords => new[] { "гипербола", "найти k", "по точке", "k/x" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти k — коэффициент гиперболы\n\n" +
                    "Если известна точка M(x₀; y₀) на графике y = k/x:\n" +
                    "  y₀ = k/x₀  →  k = x₀ · y₀\n\n" +
                    "Пример: M(−3; 4,2)  →  k = (−3)·4,2 = −12,6\n\n" +
                    "✏️ Введи координату x₀ точки:\n" +
                    "  (x₀ ≠ 0)",
                Validate = s =>
                {
                    var v = HyperbolaHelper.ParseNumber(s);
                    if (v == null) return "Введите число";
                    if (Math.Abs(v.Value) < 1e-12) return "x не может быть равен 0";
                    return null;
                }
            },
            new InputStep
            {
                Question = "✏️ Введи координату y₀ точки:",
                Validate = HyperbolaHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double x0 = HyperbolaHelper.ParseNumber(answers[0])!.Value;
            double y0 = HyperbolaHelper.ParseNumber(answers[1])!.Value;
            double k  = x0 * y0;

            var sb = new StringBuilder();
            sb.AppendLine($"Точка M({HyperbolaHelper.Fmt(x0)}; {HyperbolaHelper.Fmt(y0)}) лежит на y = k/x");
            sb.AppendLine();
            sb.AppendLine("Подставляем в формулу:");
            sb.AppendLine($"  y₀ = k / x₀");
            sb.AppendLine($"  {HyperbolaHelper.Fmt(y0)} = k / {HyperbolaHelper.Fmt(x0)}");
            sb.AppendLine($"  k = {HyperbolaHelper.Fmt(y0)} · {HyperbolaHelper.Fmt(x0)}");
            sb.AppendLine($"  k = {HyperbolaHelper.Fmt(k)}");
            sb.AppendLine();
            sb.AppendLine($"📌 k = {HyperbolaHelper.Fmt(k)}");
            sb.AppendLine($"   Функция: {HyperbolaHelper.FormatHyperbola(k)}");

            return sb.ToString().TrimEnd();
        }
    }
}

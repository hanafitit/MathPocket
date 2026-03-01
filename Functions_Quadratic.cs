using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Вспомогательные утилиты для функции y = ax²
    // ═══════════════════════════════════════════════════════════════

    internal static class QuadraticHelper
    {
        public static string Fmt(double v)
        {
            if (Math.Abs(v - Math.Round(v)) < 1e-9)
                return ((long)Math.Round(v)).ToString();
            return v.ToString("G6", CultureInfo.InvariantCulture);
        }

        public static string FormatQuadratic(double a)
        {
            if (Math.Abs(a - 1) < 1e-9)  return "y = x²";
            if (Math.Abs(a + 1) < 1e-9)  return "y = −x²";
            return $"y = {Fmt(a)}x²";
        }

        /// <summary>Разбирает "3x^2", "-x^2", "0.5x²", "x²" → коэффициент a.</summary>
        public static double? ParseQuadratic(string raw)
        {
            raw = raw.Trim()
                     .Replace(" ", "")
                     .Replace(",", ".")
                     .Replace("−", "-")
                     .Replace("²", "^2")
                     .ToLower();

            var m = System.Text.RegularExpressions.Regex.Match(raw,
                @"^([+-]?[0-9]*\.?[0-9]*)\*?x\^2$");
            if (m.Success)
            {
                string aStr = m.Groups[1].Value;
                if (aStr == "" || aStr == "+") return 1.0;
                if (aStr == "-") return -1.0;
                if (double.TryParse(aStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return Math.Abs(a) > 1e-12 ? a : (double?)null;
            }
            return null;
        }

        public static string? ValidateQuadratic(string s)
            => ParseQuadratic(s) == null
                ? "Не удалось распознать. Введите в виде: 3x^2  или  -0.5x^2  или  x^2"
                : null;

        /// <summary>Разбирает обычное число или дробь "1/3".</summary>
        public static double? ParseNumber(string s)
        {
            s = s.Trim().Replace(",", ".").Replace("−", "-");
            if (s.Contains('/'))
            {
                var p = s.Split('/');
                if (p.Length == 2 &&
                    double.TryParse(p[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double num) &&
                    double.TryParse(p[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double den) &&
                    Math.Abs(den) > 1e-12)
                    return num / den;
                return null;
            }
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                return v;
            return null;
        }

        public static string? ValidateNumber(string s)
            => ParseNumber(s) == null ? "Введите число (например: 3  или  -1.5  или  1/3)" : null;

        public static string? ValidateNonZeroNumber(string s)
        {
            var v = ParseNumber(s);
            if (v == null) return "Введите число";
            if (Math.Abs(v.Value) < 1e-12) return "Коэффициент не может быть равен 0";
            return null;
        }

        /// <summary>Полные свойства функции y = ax².</summary>
        public static string GetProperties(double a)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"📊 Свойства функции {FormatQuadratic(a)}:");
            sb.AppendLine();
            sb.AppendLine("1️⃣ Область определения:");
            sb.AppendLine("   D(y) = (−∞; +∞)  — любые x");
            sb.AppendLine();
            sb.AppendLine("2️⃣ Область значений:");
            sb.AppendLine(a > 0 ? "   E(y) = [0; +∞)" : "   E(y) = (−∞; 0]");
            sb.AppendLine();
            sb.AppendLine("3️⃣ Нули функции:");
            sb.AppendLine("   y = 0  при  x = 0  (единственный нуль)");
            sb.AppendLine();
            sb.AppendLine("4️⃣ Возрастание и убывание:");
            if (a > 0)
            {
                sb.AppendLine("   Убывает при  x ∈ (−∞; 0]");
                sb.AppendLine("   Возрастает при  x ∈ [0; +∞)");
            }
            else
            {
                sb.AppendLine("   Возрастает при  x ∈ (−∞; 0]");
                sb.AppendLine("   Убывает при  x ∈ [0; +∞)");
            }
            sb.AppendLine();
            sb.AppendLine("5️⃣ Промежутки знакопостоянства:");
            if (a > 0)
            {
                sb.AppendLine("   y > 0  при  x ∈ (−∞; 0) ∪ (0; +∞)");
                sb.AppendLine("   y = 0  при  x = 0");
            }
            else
            {
                sb.AppendLine("   y < 0  при  x ∈ (−∞; 0) ∪ (0; +∞)");
                sb.AppendLine("   y = 0  при  x = 0");
            }
            sb.AppendLine();
            sb.AppendLine(a > 0
                ? $"📌 Парабола, ветви вверх ↑  (a = {Fmt(a)} > 0)"
                : $"📌 Парабола, ветви вниз ↓  (a = {Fmt(a)} < 0)");
            sb.AppendLine("   Ось симметрии: x = 0 (ось Oy)");
            sb.AppendLine("   Вершина: (0; 0)");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.1  Принадлежит ли точка графику y = ax²
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticPointBelongsFunction : FunctionBase
    {
        public override string   Name     => "Точка на графике y = ax²?";
        public override string   Formula  => "Проверить: принадлежит ли (x₀; y₀) графику y = ax²";
        public override string[] Keywords => new[] { "парабола", "принадлежит", "точка", "ax²" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Принадлежит ли точка графику y = ax²?\n\n" +
                    "Метод: подставить x₀ в формулу и сравнить с y₀.\n" +
                    "Если ax₀² = y₀ — точка принадлежит.\n\n" +
                    "✏️ Введи коэффициент a:\n" +
                    "  Пример: 3  или  -1  или  1/3",
                Validate = QuadraticHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи координату x₀ точки:",
                Validate = QuadraticHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи координату y₀ точки:",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a  = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double x0 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double y0 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            double computed = a * x0 * x0;
            bool belongs    = Math.Abs(computed - y0) < 1e-9;

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"Точка: ({QuadraticHelper.Fmt(x0)}; {QuadraticHelper.Fmt(y0)})");
            sb.AppendLine();
            sb.AppendLine($"Подставляем x₀ = {QuadraticHelper.Fmt(x0)}:");
            sb.AppendLine($"  y = {QuadraticHelper.Fmt(a)} · ({QuadraticHelper.Fmt(x0)})²");
            sb.AppendLine($"  y = {QuadraticHelper.Fmt(a)} · {QuadraticHelper.Fmt(x0 * x0)}");
            sb.AppendLine($"  y = {QuadraticHelper.Fmt(computed)}");
            sb.AppendLine();
            sb.AppendLine(belongs
                ? $"✅ {QuadraticHelper.Fmt(computed)} = {QuadraticHelper.Fmt(y0)}  — точка ПРИНАДЛЕЖИТ графику"
                : $"❌ {QuadraticHelper.Fmt(computed)} ≠ {QuadraticHelper.Fmt(y0)}  — точка НЕ ПРИНАДЛЕЖИТ графику");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.2  Построить y = ax², все свойства
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticPlotFunction : FunctionBase
    {
        public override string   Name     => "Построить y = ax²";
        public override string   Formula  => "y = ax² — парабола: свойства и график";
        public override string[] Keywords => new[] { "парабола", "ax²", "построить", "квадратичная", "возрастание" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Построить график y = ax²\n\n" +
                    "Бот построит параболу и выведет все свойства:\n" +
                    "  · область определения и значений\n" +
                    "  · нули функции\n" +
                    "  · промежутки возрастания и убывания\n" +
                    "  · промежутки знакопостоянства\n\n" +
                    "✏️ Введи правую часть (после y =):\n" +
                    "  Пример: 3x^2  или  -x^2  или  0.4x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            return PlotHelper.QuadraticPlot(a);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            return QuadraticHelper.GetProperties(a);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.3  Два графика в одной системе координат
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticTwoGraphsFunction : FunctionBase
    {
        public override string   Name     => "Два графика y = a₁x² и y = a₂x²";
        public override string   Formula  => "Построить и сравнить две параболы в одной системе";
        public override string[] Keywords => new[] { "парабола", "два графика", "сравнить", "растяжение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Два графика y = a₁x² и y = a₂x² в одной системе\n\n" +
                    "Бот построит обе параболы и сравнит их.\n\n" +
                    "✏️ Введи правую часть первой функции:\n" +
                    "  Пример: 4x^2  или  -x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            },
            new InputStep
            {
                Question = "✏️ Введи правую часть второй функции:\n" +
                           "  Пример: x^2  или  0.25x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a1 = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double a2 = QuadraticHelper.ParseQuadratic(answers[1])!.Value;
            return PlotHelper.TwoQuadraticPlot(a1, a2);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a1 = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double a2 = QuadraticHelper.ParseQuadratic(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Функции: {QuadraticHelper.FormatQuadratic(a1)}  и  {QuadraticHelper.FormatQuadratic(a2)}");
            sb.AppendLine();
            sb.AppendLine("📐 Сравнение:");
            sb.AppendLine($"  {QuadraticHelper.FormatQuadratic(a1)}: ветви {(a1 > 0 ? "вверх ↑" : "вниз ↓")}");
            sb.AppendLine($"  {QuadraticHelper.FormatQuadratic(a2)}: ветви {(a2 > 0 ? "вверх ↑" : "вниз ↓")}");
            sb.AppendLine();

            double abs1 = Math.Abs(a1), abs2 = Math.Abs(a2);
            if (Math.Abs(abs1 - abs2) < 1e-9)
                sb.AppendLine("  |a₁| = |a₂| — одинаковой ширины");
            else if (abs1 > abs2)
                sb.AppendLine("  |a₁| > |a₂| → первый график уже (круче)");
            else
                sb.AppendLine("  |a₁| < |a₂| → второй график уже (круче)");

            sb.AppendLine();
            if (Math.Abs(a2) > 1e-12)
            {
                double ratio = Math.Abs(a1 / a2);
                if (Math.Abs(ratio - 1) > 1e-9)
                {
                    if (ratio > 1)
                        sb.AppendLine($"📌 Первый — растяжение второго вдоль Oy в {QuadraticHelper.Fmt(ratio)} раз");
                    else
                        sb.AppendLine($"📌 Первый — сжатие второго вдоль Oy в {QuadraticHelper.Fmt(1.0 / ratio)} раз");
                }
            }

            sb.AppendLine();
            sb.AppendLine("📍 Оба графика проходят через (0; 0)");
            sb.AppendLine("   Ось симметрии обоих: x = 0 (ось Oy)");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.4  Сравнить значения ax₁² и ax₂²
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticCompareValuesFunction : FunctionBase
    {
        public override string   Name     => "Сравнить значения y = ax² при двух x";
        public override string   Formula  => "Сравнить ax₁² и ax₂² используя свойства параболы";
        public override string[] Keywords => new[] { "парабола", "сравнить", "значения", "ax²" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сравнение значений y = ax² при двух x\n\n" +
                    "Метод: чем больше |x|, тем больше |ax²|.\n\n" +
                    "✏️ Введи коэффициент a:\n" +
                    "  Пример: 0.4  или  -3",
                Validate = QuadraticHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи первое значение x₁:",
                Validate = QuadraticHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи второе значение x₂:",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a  = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double x1 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double x2 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            double y1 = a * x1 * x1;
            double y2 = a * x2 * x2;

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine();
            sb.AppendLine($"y({QuadraticHelper.Fmt(x1)}) = {QuadraticHelper.Fmt(a)}·({QuadraticHelper.Fmt(x1)})² = {QuadraticHelper.Fmt(y1)}");
            sb.AppendLine($"y({QuadraticHelper.Fmt(x2)}) = {QuadraticHelper.Fmt(a)}·({QuadraticHelper.Fmt(x2)})² = {QuadraticHelper.Fmt(y2)}");
            sb.AppendLine();

            double abs1 = Math.Abs(x1), abs2 = Math.Abs(x2);
            sb.AppendLine("📐 Объяснение через свойства:");
            if (Math.Abs(abs1 - abs2) < 1e-9)
                sb.AppendLine("  |x₁| = |x₂| → y₁ = y₂ (парабола симметрична)");
            else if (abs1 > abs2)
            {
                sb.AppendLine($"  |x₁| = {QuadraticHelper.Fmt(abs1)} > |x₂| = {QuadraticHelper.Fmt(abs2)}");
                sb.AppendLine(a > 0 ? "  a > 0: парабола выше при большем |x| → y₁ > y₂"
                                    : "  a < 0: парабола ниже при большем |x| → y₁ < y₂");
            }
            else
            {
                sb.AppendLine($"  |x₂| = {QuadraticHelper.Fmt(abs2)} > |x₁| = {QuadraticHelper.Fmt(abs1)}");
                sb.AppendLine(a > 0 ? "  a > 0: парабола выше при большем |x| → y₂ > y₁"
                                    : "  a < 0: парабола ниже при большем |x| → y₂ < y₁");
            }

            sb.AppendLine();
            if (y1 > y2)      sb.AppendLine($"📌 y(x₁) > y(x₂):  {QuadraticHelper.Fmt(y1)} > {QuadraticHelper.Fmt(y2)}");
            else if (y1 < y2) sb.AppendLine($"📌 y(x₁) < y(x₂):  {QuadraticHelper.Fmt(y1)} < {QuadraticHelper.Fmt(y2)}");
            else               sb.AppendLine($"📌 y(x₁) = y(x₂):  {QuadraticHelper.Fmt(y1)} = {QuadraticHelper.Fmt(y2)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.5  Число корней ax² = c  (графически)
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticRootsCountFunction : FunctionBase
    {
        public override string   Name     => "Число корней ax² = c (графически)";
        public override string   Formula  => "Пересечение параболы y = ax² с прямой y = c";
        public override string[] Keywords => new[] { "парабола", "корни", "число корней", "ax²=c" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Число корней уравнения ax² = c (графически)\n\n" +
                    "Метод: пересечение параболы y = ax²\n" +
                    "с горизонтальной прямой y = c.\n\n" +
                    "✏️ Введи коэффициент a (при x²):",
                Validate = QuadraticHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи c (правая часть уравнения ax² = c):",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[1])!.Value;
            return PlotHelper.QuadraticWithLine(a, c);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {QuadraticHelper.Fmt(a)}x² = {QuadraticHelper.Fmt(c)}");
            sb.AppendLine($"Графически: {QuadraticHelper.FormatQuadratic(a)}  и  y = {QuadraticHelper.Fmt(c)}");
            sb.AppendLine();
            sb.AppendLine($"x² = {QuadraticHelper.Fmt(c)} / {QuadraticHelper.Fmt(a)} = {QuadraticHelper.Fmt(c / a)}");
            sb.AppendLine();

            double ratio = c / a;
            if (ratio < -1e-9)
            {
                sb.AppendLine("x² < 0 — невозможно.");
                sb.AppendLine("📌 Корней нет. Прямая y = c не пересекает параболу.");
            }
            else if (Math.Abs(ratio) < 1e-9)
            {
                sb.AppendLine("x² = 0  →  x = 0");
                sb.AppendLine("📌 Один корень: x = 0");
                sb.AppendLine("   Прямая y = c касается вершины параболы.");
            }
            else
            {
                double x1 = Math.Sqrt(ratio);
                sb.AppendLine($"x = ±√{QuadraticHelper.Fmt(ratio)} = ±{QuadraticHelper.Fmt(x1)}");
                sb.AppendLine($"📌 Два корня: x₁ = {QuadraticHelper.Fmt(-x1)},  x₂ = {QuadraticHelper.Fmt(x1)}");
                sb.AppendLine("   Прямая y = c пересекает параболу в двух точках.");
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.6  Пересекаются ли y = ax² и y = kx + b
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticIntersectsLinearFunction : FunctionBase
    {
        public override string   Name     => "Пересечение y = ax² и y = kx + b";
        public override string   Formula  => "Найти точки пересечения параболы и прямой";
        public override string[] Keywords => new[] { "парабола", "прямая", "пересечение", "ax²" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Пересечение y = ax² и y = kx + b\n\n" +
                    "Метод: ax² = kx + b  →  ax² − kx − b = 0\n" +
                    "Анализируем дискриминант.\n\n" +
                    "✏️ Введи правую часть параболы:\n" +
                    "  Пример: 3x^2  или  -x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            },
            new InputStep
            {
                Question = "✏️ Введи правую часть прямой (после y =):\n" +
                           "  Пример: 5-2x  или  3x+1  или  -x+4",
                Validate = s => LinearHelper.ParseLinear(s) == null
                    ? "Не удалось распознать. Введите: 5-2x  или  3x+1"
                    : null
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a       = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            var (k, b) = LinearHelper.ParseLinear(answers[1])!.Value;
            return PlotHelper.QuadraticWithLinearLine(a, k, b);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a       = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            var (k, b) = LinearHelper.ParseLinear(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Парабола: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"Прямая:   {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine();

            double A = a, B = -k, C = -b;
            sb.AppendLine($"Уравнение: {QuadraticHelper.Fmt(A)}x² {(B >= 0 ? "+" : "")}{QuadraticHelper.Fmt(B)}x {(C >= 0 ? "+" : "")}{QuadraticHelper.Fmt(C)} = 0");
            sb.AppendLine();

            double D = B * B - 4 * A * C;
            sb.AppendLine($"D = ({QuadraticHelper.Fmt(B)})² − 4·{QuadraticHelper.Fmt(A)}·({QuadraticHelper.Fmt(C)}) = {QuadraticHelper.Fmt(D)}");
            sb.AppendLine();

            if (D < -1e-9)
            {
                sb.AppendLine("D < 0");
                sb.AppendLine("📌 Парабола и прямая НЕ пересекаются.");
            }
            else if (Math.Abs(D) < 1e-9)
            {
                double x0 = -B / (2 * A);
                double y0 = a * x0 * x0;
                sb.AppendLine("D = 0 — одна точка касания:");
                sb.AppendLine($"📌 Касание в точке ({QuadraticHelper.Fmt(x0)}; {QuadraticHelper.Fmt(y0)})");
            }
            else
            {
                double sqrtD = Math.Sqrt(D);
                double x1 = (-B - sqrtD) / (2 * A);
                double x2 = (-B + sqrtD) / (2 * A);
                if (x1 > x2) (x1, x2) = (x2, x1);
                double y1 = a * x1 * x1, y2 = a * x2 * x2;
                sb.AppendLine($"D > 0 → две точки пересечения:");
                sb.AppendLine($"  x₁ = {QuadraticHelper.Fmt(x1)},  x₂ = {QuadraticHelper.Fmt(x2)}");
                sb.AppendLine();
                sb.AppendLine($"📌 ({QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(y1)})  и  ({QuadraticHelper.Fmt(x2)}; {QuadraticHelper.Fmt(y2)})");
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.7  Найти корни уравнения ax² + bx + c = 0 графически
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticFindRootsFunction : FunctionBase
    {
        public override string   Name     => "Корни ax² + bx + c = 0 (графически)";
        public override string   Formula  => "Пересечение y = ax² с y = −bx − c";
        public override string[] Keywords => new[] { "парабола", "корни", "уравнение", "графически", "квадратное" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Корни уравнения ax² + bx + c = 0 (графически)\n\n" +
                    "Метод: ax² = −bx − c\n" +
                    "Строим параболу y = ax² и прямую y = −bx − c.\n" +
                    "Абсциссы точек пересечения — корни.\n\n" +
                    "✏️ Введи коэффициент a (при x²):\n" +
                    "  Пример: 2  или  -1",
                Validate = QuadraticHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи коэффициент b (при x):\n" +
                           "  Пример: -3  или  0",
                Validate = QuadraticHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи свободный член c:\n" +
                           "  Пример: 1  или  -4  или  0",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double b = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[2])!.Value;
            return PlotHelper.QuadraticWithLinearLine(a, -b, -c);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double b = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[2])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {QuadraticHelper.Fmt(a)}x² {(b >= 0 ? "+" : "")}{QuadraticHelper.Fmt(b)}x {(c >= 0 ? "+" : "")}{QuadraticHelper.Fmt(c)} = 0");
            sb.AppendLine();
            sb.AppendLine("Графический метод:");
            sb.AppendLine($"  {QuadraticHelper.Fmt(a)}x² = {LinearHelper.FormatLinear(-b, -c)}");
            sb.AppendLine($"  Парабола: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"  Прямая:   {LinearHelper.FormatLinear(-b, -c)}");
            sb.AppendLine();

            double D = b * b - 4 * a * c;
            sb.AppendLine($"D = {QuadraticHelper.Fmt(b)}² − 4·{QuadraticHelper.Fmt(a)}·{QuadraticHelper.Fmt(c)} = {QuadraticHelper.Fmt(D)}");
            sb.AppendLine();

            if (D < -1e-9)
            {
                sb.AppendLine("D < 0 — прямая не пересекает параболу.");
                sb.AppendLine("📌 Корней нет.");
            }
            else if (Math.Abs(D) < 1e-9)
            {
                double x0 = -b / (2 * a);
                sb.AppendLine("D = 0 — прямая касается параболы.");
                sb.AppendLine($"📌 Один корень: x = {QuadraticHelper.Fmt(x0)}");
            }
            else
            {
                double sqrtD = Math.Sqrt(D);
                double x1 = (-b - sqrtD) / (2 * a);
                double x2 = (-b + sqrtD) / (2 * a);
                if (x1 > x2) (x1, x2) = (x2, x1);
                sb.AppendLine($"D > 0 → два корня.");
                sb.AppendLine($"  √D ≈ {QuadraticHelper.Fmt(sqrtD)}");
                sb.AppendLine($"  x₁ = {QuadraticHelper.Fmt(x1)}");
                sb.AppendLine($"  x₂ = {QuadraticHelper.Fmt(x2)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Два корня: x₁ = {QuadraticHelper.Fmt(x1)},  x₂ = {QuadraticHelper.Fmt(x2)}");
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.8  Является ли y = ax² возрастающей/убывающей на промежутке
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticMonotonicFunction : FunctionBase
    {
        public override string   Name     => "Монотонность y = ax² на промежутке";
        public override string   Formula  => "Возрастающая или убывающая на [a; b]?";
        public override string[] Keywords => new[] { "парабола", "возрастающая", "убывающая", "промежуток", "ax²" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Монотонность y = ax² на промежутке\n\n" +
                    "Если a > 0: убывает на (−∞; 0], возрастает на [0; +∞)\n" +
                    "Если a < 0: возрастает на (−∞; 0], убывает на [0; +∞)\n\n" +
                    "✏️ Введи правую часть (после y =):\n" +
                    "  Пример: 3x^2  или  -x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            },
            new InputStep
            {
                Question = "✏️ Введи левую границу промежутка:\n" +
                           "  Пример: -3  или  0",
                Validate = QuadraticHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи правую границу промежутка:",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a  = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double x1 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double x2 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            return PlotHelper.QuadraticOnInterval(a, x1, x2);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a  = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double x1 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double x2 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            if (x1 > x2) (x1, x2) = (x2, x1);

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"Промежуток: [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]");
            sb.AppendLine();
            sb.AppendLine($"a = {QuadraticHelper.Fmt(a)} {(a > 0 ? "> 0 → ветви вверх" : "< 0 → ветви вниз")}");
            sb.AppendLine("Ось симметрии: x = 0");
            sb.AppendLine();

            bool bothRight = x1 >= -1e-9;
            bool bothLeft  = x2 <= 1e-9;

            if (bothRight)
            {
                sb.AppendLine($"Промежуток [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}] — правее нуля.");
                sb.AppendLine(a > 0
                    ? $"📌 Функция ВОЗРАСТАЮЩАЯ на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]"
                    : $"📌 Функция УБЫВАЮЩАЯ на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]");
            }
            else if (bothLeft)
            {
                sb.AppendLine($"Промежуток [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}] — левее нуля.");
                sb.AppendLine(a > 0
                    ? $"📌 Функция УБЫВАЮЩАЯ на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]"
                    : $"📌 Функция ВОЗРАСТАЮЩАЯ на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]");
            }
            else
            {
                sb.AppendLine($"Промежуток [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}] содержит x = 0.");
                sb.AppendLine("На нём парабола сначала убывает, потом возрастает (или наоборот).");
                sb.AppendLine();
                if (a > 0)
                {
                    sb.AppendLine($"📌 НЕ монотонная на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]:");
                    sb.AppendLine($"   Убывает на [{QuadraticHelper.Fmt(x1)}; 0]");
                    sb.AppendLine($"   Возрастает на [0; {QuadraticHelper.Fmt(x2)}]");
                }
                else
                {
                    sb.AppendLine($"📌 НЕ монотонная на [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]:");
                    sb.AppendLine($"   Возрастает на [{QuadraticHelper.Fmt(x1)}; 0]");
                    sb.AppendLine($"   Убывает на [0; {QuadraticHelper.Fmt(x2)}]");
                }
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.9  Наибольшее и наименьшее значение на промежутке
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticMinMaxFunction : FunctionBase
    {
        public override string   Name     => "Наибольшее и наименьшее значение y = ax²";
        public override string   Formula  => "max и min функции y = ax² на промежутке [a; b]";
        public override string[] Keywords => new[] { "парабола", "наибольшее", "наименьшее", "максимум", "минимум" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Наибольшее и наименьшее значение y = ax²\n\n" +
                    "Метод: вычислить y на концах промежутка и в вершине (x=0),\n" +
                    "если вершина входит в промежуток.\n\n" +
                    "✏️ Введи правую часть (после y =):\n" +
                    "  Пример: 2x^2  или  -x^2",
                Validate = QuadraticHelper.ValidateQuadratic
            },
            new InputStep
            {
                Question = "✏️ Введи левую границу промежутка:",
                Validate = QuadraticHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи правую границу промежутка:",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a  = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double x1 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double x2 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            return PlotHelper.QuadraticOnInterval(a, x1, x2);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a  = QuadraticHelper.ParseQuadratic(answers[0])!.Value;
            double x1 = QuadraticHelper.ParseNumber(answers[1])!.Value;
            double x2 = QuadraticHelper.ParseNumber(answers[2])!.Value;
            if (x1 > x2) (x1, x2) = (x2, x1);

            var sb = new StringBuilder();
            sb.AppendLine($"Функция: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"Промежуток: [{QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(x2)}]");
            sb.AppendLine();

            double y1 = a * x1 * x1;
            double y2 = a * x2 * x2;
            sb.AppendLine($"y({QuadraticHelper.Fmt(x1)}) = {QuadraticHelper.Fmt(y1)}");
            sb.AppendLine($"y({QuadraticHelper.Fmt(x2)}) = {QuadraticHelper.Fmt(y2)}");

            var candidates = new List<(double x, double y)> { (x1, y1), (x2, y2) };

            bool vertexIn = x1 <= 0 && 0 <= x2;
            if (vertexIn)
            {
                sb.AppendLine("y(0) = 0  (вершина параболы входит в промежуток)");
                candidates.Add((0, 0));
            }

            sb.AppendLine();
            double yMax = double.MinValue, yMin = double.MaxValue;
            double xMax = 0, xMin = 0;
            foreach (var (x, y) in candidates)
            {
                if (y > yMax) { yMax = y; xMax = x; }
                if (y < yMin) { yMin = y; xMin = x; }
            }

            sb.AppendLine($"📌 Наибольшее значение: y = {QuadraticHelper.Fmt(yMax)}  при  x = {QuadraticHelper.Fmt(xMax)}");
            sb.AppendLine($"📌 Наименьшее значение: y = {QuadraticHelper.Fmt(yMin)}  при  x = {QuadraticHelper.Fmt(xMin)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  25.10  Пересечение y = ax² и y = ax − c
    // ═══════════════════════════════════════════════════════════════

    public class QuadraticCanIntersectFunction : FunctionBase
    {
        public override string   Name     => "Пересечение y = ax² и y = ax − c";
        public override string   Formula  => "Могут ли пересечься y = ax² и y = ax − c?";
        public override string[] Keywords => new[] { "парабола", "пересечение", "ax²", "ax−c" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Пересечение y = ax² и y = ax − c\n\n" +
                    "Метод: ax² = ax − c  →  ax² − ax + c = 0\n" +
                    "D = a² − 4ac\n\n" +
                    "✏️ Введи коэффициент a (одинаковый в обеих):\n" +
                    "  Пример: 1  или  2  или  -3",
                Validate = QuadraticHelper.ValidateNonZeroNumber
            },
            new InputStep
            {
                Question = "✏️ Введи c (в прямой y = ax − c):\n" +
                           "  Пример: 5  или  -2  или  0",
                Validate = QuadraticHelper.ValidateNumber
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[1])!.Value;
            return PlotHelper.QuadraticWithLinearLine(a, a, -c);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double a = QuadraticHelper.ParseNumber(answers[0])!.Value;
            double c = QuadraticHelper.ParseNumber(answers[1])!.Value;

            var sb = new StringBuilder();
            sb.AppendLine($"Парабола: {QuadraticHelper.FormatQuadratic(a)}");
            sb.AppendLine($"Прямая:   y = {QuadraticHelper.Fmt(a)}x − {QuadraticHelper.Fmt(c)}");
            sb.AppendLine();
            sb.AppendLine("Уравнение пересечения:");
            sb.AppendLine($"  {QuadraticHelper.Fmt(a)}x² − {QuadraticHelper.Fmt(a)}x + {QuadraticHelper.Fmt(c)} = 0");
            sb.AppendLine();

            double D = a * a - 4 * a * c;
            sb.AppendLine($"D = a² − 4ac = {QuadraticHelper.Fmt(a * a)} − {QuadraticHelper.Fmt(4 * a * c)} = {QuadraticHelper.Fmt(D)}");
            sb.AppendLine();

            if (D < -1e-9)
            {
                sb.AppendLine("D < 0");
                sb.AppendLine("📌 Графики НЕ пересекаются.");
            }
            else if (Math.Abs(D) < 1e-9)
            {
                double x0 = 0.5; // a/(2a) = 1/2
                double y0 = a * x0 * x0;
                sb.AppendLine("D = 0 — одна точка касания:");
                sb.AppendLine($"📌 Касание в точке ({QuadraticHelper.Fmt(x0)}; {QuadraticHelper.Fmt(y0)})");
            }
            else
            {
                double sqrtD = Math.Sqrt(D);
                double x1 = (a - sqrtD) / (2 * a);
                double x2 = (a + sqrtD) / (2 * a);
                if (x1 > x2) (x1, x2) = (x2, x1);
                double y1 = a * x1 * x1, y2 = a * x2 * x2;
                sb.AppendLine("D > 0 — два пересечения:");
                sb.AppendLine($"📌 ({QuadraticHelper.Fmt(x1)}; {QuadraticHelper.Fmt(y1)})  и  ({QuadraticHelper.Fmt(x2)}; {QuadraticHelper.Fmt(y2)})");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    //  § 24. Решение систем двух линейных уравнений с двумя
    //        переменными графическим способом

    //  Вспомогательный парсер уравнений вида ax + by = c

    internal static class LinearEquationParser
    {
        /// <summary>
        /// Разбирает уравнение вида ax + by = c или ax + by + c = 0
        /// Возвращает (a, b, c) где уравнение: ax + by = c
        /// </summary>
        public static (double a, double b, double c)? Parse(string raw)
        {
            raw = raw.Trim()
                     .Replace(" ", "")
                     .Replace("−", "-")
                     .Replace(",", ".");

            // Нормализуем: переводим в вид ax + by = c
            // Поддерживаем: ax+by=c  и  ax+by+c=0
            string left, right;
            int eq = raw.IndexOf('=');
            if (eq < 0) return null;
            left  = raw.Substring(0, eq);
            right = raw.Substring(eq + 1);

            // Если правая часть 0 — переносим константу влево
            double rhs;
            if (!double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out rhs))
                return null;

            // Парсим левую часть: ищем коэффициенты при x, y и константу
            double a = 0, b = 0, c_left = 0;

            // Нормализуем знаки: вставляем + перед числами где нет знака
            string expr = left;
            if (!expr.StartsWith("-") && !expr.StartsWith("+"))
                expr = "+" + expr;

            // Разбиваем по + и -
            var tokens = new List<string>();
            int start = 0;
            for (int i = 1; i < expr.Length; i++)
            {
                if ((expr[i] == '+' || expr[i] == '-') && i != start)
                {
                    tokens.Add(expr.Substring(start, i - start));
                    start = i;
                }
            }
            tokens.Add(expr.Substring(start));

            foreach (var tok in tokens)
            {
                if (string.IsNullOrEmpty(tok)) continue;
                string t = tok;

                if (t.Contains('x'))
                {
                    string coef = t.Replace("x", "");
                    a = coef == "+" || coef == "" ? 1
                      : coef == "-" ? -1
                      : double.Parse(coef, CultureInfo.InvariantCulture);
                }
                else if (t.Contains('y'))
                {
                    string coef = t.Replace("y", "");
                    b = coef == "+" || coef == "" ? 1
                      : coef == "-" ? -1
                      : double.Parse(coef, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out double cv))
                        c_left += cv;
                }
            }

            // ax + by + c_left = rhs  →  ax + by = rhs - c_left
            double c = rhs - c_left;
            return (a, b, c);
        }

        /// <summary>
        /// Выразить y через x: ax + by = c  →  y = (-a/b)x + c/b
        /// Возвращает (k, bVal) для y = kx + bVal, или null если b=0 (вертикальная прямая)
        /// </summary>
        public static (double k, double bVal)? ToYForm(double a, double b, double c)
        {
            if (Math.Abs(b) < 1e-12) return null; // вертикальная прямая
            double k    = -a / b;
            double bVal =  c / b;
            return (k, bVal);
        }

        public static string? Validate(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 2x+3y=6  или  x-y=1";
            if (Parse(s) is null)
                return $"Не могу разобрать «{s.Trim()}».\nПример: 2x+3y=6  или  -x+y=3";
            return null;
        }

        /// <summary>Форматировать уравнение ax + by = c</summary>
        public static string Format(double a, double b, double c)
        {
            var sb = new StringBuilder();
            if (Math.Abs(a) > 1e-12)
            {
                string aStr = a == 1 ? "" : a == -1 ? "-" : LinearHelper.Fmt(a);
                sb.Append($"{aStr}x");
            }
            if (Math.Abs(b) > 1e-12)
            {
                if (b > 0 && sb.Length > 0) sb.Append("+");
                string bStr = b == 1 ? "" : b == -1 ? "-" : LinearHelper.Fmt(b);
                sb.Append($"{bStr}y");
            }
            if (Math.Abs(c) < 1e-12 && sb.Length == 0) sb.Append("0");
            sb.Append($" = {LinearHelper.Fmt(c)}");
            return sb.ToString();
        }
    }

    //  24.1 Найти координаты точек пересечения с осью Ox

    public class SystemGraphOxFunction : FunctionBase
    {
        public override string   Name       => "Пересечение графика уравнения с осью Ox";
        public override string   Formula    => "ax + by = c при y = 0";
        public override string[] Keywords   => new[] { "пересечение", "ось ox", "уравнение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Пересечение графика уравнения с осью Ox\n\n" +
                    "Для нахождения точки пересечения с Ox\n" +
                    "подставляем y = 0 в уравнение и находим x.\n\n" +
                    "Пример: 5x − y = 7  →  при y=0:  5x = 7  →  x = 7/5\n" +
                    "Точка: (7/5; 0)\n\n" +
                    "Как записывать уравнение:\n" +
                    "  · 2x-3y=6  или  x+y=1  или  5x-y=7\n\n" +
                    "✏️ Введи уравнение:",
                Validate = LinearEquationParser.Validate
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a, b, c) = LinearEquationParser.Parse(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Уравнение: {LinearEquationParser.Format(a, b, c)}");
            sb.AppendLine();
            sb.AppendLine("Пересечение с осью Ox: подставляем y = 0");
            sb.AppendLine();

            // ax + b·0 = c  →  ax = c
            sb.AppendLine($"  {LinearHelper.Fmt(a)}x {LinearHelper.FmtTerm(b)}·0 = {LinearHelper.Fmt(c)}");
            sb.AppendLine($"  {LinearHelper.Fmt(a)}x = {LinearHelper.Fmt(c)}");

            if (Math.Abs(a) < 1e-12)
            {
                if (Math.Abs(c) < 1e-12)
                    sb.AppendLine("\n📌 Уравнение обращается в 0 = 0 — прямая совпадает с осью Ox.");
                else
                    sb.AppendLine("\n📌 Решений нет — прямая параллельна оси Ox, не пересекает её.");
            }
            else
            {
                double x = c / a;
                sb.AppendLine($"  x = {LinearHelper.Fmt(c)} / {LinearHelper.Fmt(a)} = {LinearHelper.Fmt(x)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Точка пересечения с осью Ox: ({LinearHelper.Fmt(x)}; 0)");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  24.2 Найти координаты точек пересечения с осью Oy

    public class SystemGraphOyFunction : FunctionBase
    {
        public override string   Name       => "Пересечение графика уравнения с осью Oy";
        public override string   Formula    => "ax + by = c при x = 0";
        public override string[] Keywords   => new[] { "пересечение", "ось oy", "уравнение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Пересечение графика уравнения с осью Oy\n\n" +
                    "Подставляем x = 0 в уравнение и находим y.\n\n" +
                    "Пример: 3x + 8y = 11  →  при x=0:  8y = 11  →  y = 11/8\n" +
                    "Точка: (0; 11/8)\n\n" +
                    "✏️ Введи уравнение:\n" +
                    "  Пример: 3x+8y=11  или  x-2y=4",
                Validate = LinearEquationParser.Validate
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a, b, c) = LinearEquationParser.Parse(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Уравнение: {LinearEquationParser.Format(a, b, c)}");
            sb.AppendLine();
            sb.AppendLine("Пересечение с осью Oy: подставляем x = 0");
            sb.AppendLine();

            sb.AppendLine($"  {LinearHelper.Fmt(a)}·0 {LinearHelper.FmtTerm(b)}y = {LinearHelper.Fmt(c)}");
            sb.AppendLine($"  {LinearHelper.Fmt(b)}y = {LinearHelper.Fmt(c)}");

            if (Math.Abs(b) < 1e-12)
            {
                if (Math.Abs(c) < 1e-12)
                    sb.AppendLine("\n📌 Уравнение обращается в 0 = 0 — прямая совпадает с осью Oy.");
                else
                    sb.AppendLine("\n📌 Решений нет — прямая параллельна оси Oy, не пересекает её.");
            }
            else
            {
                double y = c / b;
                sb.AppendLine($"  y = {LinearHelper.Fmt(c)} / {LinearHelper.Fmt(b)} = {LinearHelper.Fmt(y)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Точка пересечения с осью Oy: (0; {LinearHelper.Fmt(y)})");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  24.3 Построить график уравнения ax + by = c

    public class SystemGraphPlotFunction : FunctionBase
    {
        public override string   Name       => "Построить график уравнения";
        public override string   Formula    => "ax + by = c → прямая";
        public override string[] Keywords   => new[] { "построить", "график", "уравнение" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Построить график уравнения ax + by = c\n\n" +
                    "Выражаем y через x: y = (c − ax) / b\n" +
                    "Находим две точки (достаточно для прямой):\n" +
                    "  · при x = 0  →  y = c/b  (точка на Oy)\n" +
                    "  · при y = 0  →  x = c/a  (точка на Ox)\n\n" +
                    "Пример: x + 5 = 0  →  x = −5 (вертикальная прямая)\n\n" +
                    "✏️ Введи уравнение:\n" +
                    "  Пример: x+y=5  или  3y-18=0  или  x+2y=1",
                Validate = LinearEquationParser.Validate
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            var (a, b, c) = LinearEquationParser.Parse(answers[0])!.Value;
            var yForm = LinearEquationParser.ToYForm(a, b, c);
            if (yForm is null) return null; // вертикальная прямая — не рисуем
            return PlotHelper.LinearFunction(yForm.Value.k, yForm.Value.bVal);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a, b, c) = LinearEquationParser.Parse(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Уравнение: {LinearEquationParser.Format(a, b, c)}");
            sb.AppendLine();

            var yForm = LinearEquationParser.ToYForm(a, b, c);
            if (yForm is null)
            {
                // Вертикальная прямая: ax = c
                double x = c / a;
                sb.AppendLine("b = 0 — прямая вертикальная.");
                sb.AppendLine($"  x = {LinearHelper.Fmt(x)}");
                sb.AppendLine();
                sb.AppendLine($"📌 График — вертикальная прямая x = {LinearHelper.Fmt(x)}");
                return sb.ToString().TrimEnd();
            }

            var (k, bVal) = yForm.Value;
            sb.AppendLine("Выражаем y через x:");
            sb.AppendLine($"  {LinearHelper.Fmt(b)}y = {LinearHelper.Fmt(c)} − {LinearHelper.Fmt(a)}x");
            sb.AppendLine($"  y = {LinearHelper.Fmt(k)}x {LinearHelper.FmtTerm(bVal)}");
            sb.AppendLine();

            // Таблица двух точек
            double x0 = Math.Abs(a) > 1e-12 ? c / a : 0;
            double y0 = 0;
            double x1 = 0;
            double y1 = bVal;

            sb.AppendLine("Строим по двум точкам:");
            sb.AppendLine($"  при x = {LinearHelper.Fmt(x0)}: y = {LinearHelper.Fmt(y0)}  → ({LinearHelper.Fmt(x0)}; {LinearHelper.Fmt(y0)})");
            sb.AppendLine($"  при x = {LinearHelper.Fmt(x1)}: y = {LinearHelper.Fmt(y1)}  → ({LinearHelper.Fmt(x1)}; {LinearHelper.Fmt(y1)})");
            sb.AppendLine();
            sb.AppendLine($"📌 График — прямая {LinearHelper.FormatLinear(k, bVal)}");

            return sb.ToString().TrimEnd();
        }
    }

    //  24.4 / 24.5 Решить систему графически

    public class SystemSolveGraphicallyFunction : FunctionBase
    {
        public override string   Name       => "Решить систему уравнений графически";
        public override string   Formula    => "Построить два графика, найти точку пересечения";
        public override string[] Keywords   => new[] { "система", "решить графически", "два уравнения" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение системы двух линейных уравнений графически\n\n" +
                    "Алгоритм:\n" +
                    "  1. Выразить y через x из каждого уравнения\n" +
                    "  2. Построить оба графика в одной системе координат\n" +
                    "  3. Найти точку пересечения — это решение системы\n\n" +
                    "Варианты:\n" +
                    "  · Прямые пересекаются → одно решение\n" +
                    "  · Прямые параллельны  → решений нет\n" +
                    "  · Прямые совпадают    → бесконечно много решений\n\n" +
                    "✏️ Введи первое уравнение:\n" +
                    "  Пример: y-2x=0  или  2x+y=4  или  3x-y=1",
                Validate = LinearEquationParser.Validate
            },
            new InputStep
            {
                Question = "✏️ Введи второе уравнение:",
                Validate = LinearEquationParser.Validate
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            var eq1 = LinearEquationParser.Parse(answers[0])!.Value;
            var eq2 = LinearEquationParser.Parse(answers[1])!.Value;
            var yf1 = LinearEquationParser.ToYForm(eq1.a, eq1.b, eq1.c);
            var yf2 = LinearEquationParser.ToYForm(eq2.a, eq2.b, eq2.c);
            if (yf1 is null || yf2 is null) return null;
            return PlotHelper.TwoLinearFunctions(yf1.Value.k, yf1.Value.bVal, yf2.Value.k, yf2.Value.bVal);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a1, b1, c1) = LinearEquationParser.Parse(answers[0])!.Value;
            var (a2, b2, c2) = LinearEquationParser.Parse(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Уравнение 1: {LinearEquationParser.Format(a1, b1, c1)}");
            sb.AppendLine($"Уравнение 2: {LinearEquationParser.Format(a2, b2, c2)}");
            sb.AppendLine();

            // Приводим к y = kx + b
            var yf1 = LinearEquationParser.ToYForm(a1, b1, c1);
            var yf2 = LinearEquationParser.ToYForm(a2, b2, c2);

            if (yf1 is null || yf2 is null)
            {
                sb.AppendLine("⚠️ Одно из уравнений задаёт вертикальную прямую.");
                sb.AppendLine("Аналитическое решение:");
                // Вертикальная прямая: x = const
                if (yf1 is null && yf2 is not null)
                {
                    double xv = c1 / a1;
                    double yv = yf2.Value.k * xv + yf2.Value.bVal;
                    sb.AppendLine($"  x = {LinearHelper.Fmt(xv)}");
                    sb.AppendLine($"  y = {LinearHelper.Fmt(yv)}");
                    sb.AppendLine($"\n📌 Решение: ({LinearHelper.Fmt(xv)}; {LinearHelper.Fmt(yv)})");
                }
                else if (yf2 is null && yf1 is not null)
                {
                    double xv = c2 / a2;
                    double yv = yf1.Value.k * xv + yf1.Value.bVal;
                    sb.AppendLine($"  x = {LinearHelper.Fmt(xv)}");
                    sb.AppendLine($"  y = {LinearHelper.Fmt(yv)}");
                    sb.AppendLine($"\n📌 Решение: ({LinearHelper.Fmt(xv)}; {LinearHelper.Fmt(yv)})");
                }
                else
                {
                    sb.AppendLine("Оба уравнения задают вертикальные прямые.");
                    if (Math.Abs(c1 / a1 - c2 / a2) < 1e-9)
                        sb.AppendLine("📌 Прямые совпадают — бесконечно много решений.");
                    else
                        sb.AppendLine("📌 Прямые параллельны — решений нет.");
                }
                return sb.ToString().TrimEnd();
            }

            var (k1, bv1) = yf1.Value;
            var (k2, bv2) = yf2.Value;

            sb.AppendLine("Шаг 1. Выражаем y через x:");
            sb.AppendLine($"  1) {LinearHelper.FormatLinear(k1, bv1)}");
            sb.AppendLine($"  2) {LinearHelper.FormatLinear(k2, bv2)}");
            sb.AppendLine();

            bool kEq = Math.Abs(k1 - k2) < 1e-9;
            bool bEq = Math.Abs(bv1 - bv2) < 1e-9;

            if (kEq && bEq)
            {
                sb.AppendLine("Шаг 2. k₁ = k₂ и b₁ = b₂ — прямые совпадают.");
                sb.AppendLine();
                sb.AppendLine("📌 Система имеет бесконечно много решений.");
                sb.AppendLine("   Ответ: бесконечно много.");
            }
            else if (kEq)
            {
                sb.AppendLine("Шаг 2. k₁ = k₂, b₁ ≠ b₂ — прямые параллельны.");
                sb.AppendLine();
                sb.AppendLine("📌 Система не имеет решений.");
                sb.AppendLine("   Ответ: ∅");
            }
            else
            {
                double x = (bv2 - bv1) / (k1 - k2);
                double y = k1 * x + bv1;

                sb.AppendLine("Шаг 2. Приравниваем правые части:");
                sb.AppendLine($"  {LinearHelper.Fmt(k1)}x {LinearHelper.FmtTerm(bv1)} = {LinearHelper.Fmt(k2)}x {LinearHelper.FmtTerm(bv2)}");
                sb.AppendLine($"  {LinearHelper.Fmt(k1 - k2)}x = {LinearHelper.Fmt(bv2 - bv1)}");
                sb.AppendLine($"  x = {LinearHelper.Fmt(x)}");
                sb.AppendLine();
                sb.AppendLine("Шаг 3. Находим y:");
                sb.AppendLine($"  y = {LinearHelper.Fmt(k1)}·{LinearHelper.Fmt(x)} {LinearHelper.FmtTerm(bv1)} = {LinearHelper.Fmt(y)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Решение системы: ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)})");
                sb.AppendLine($"   Ответ: {{{LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)}}}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  24.8 Сколько решений имеет система?

    public class SystemCountSolutionsFunction : FunctionBase
    {
        public override string   Name       => "Сколько решений имеет система";
        public override string   Formula    => "0, 1 или бесконечно много";
        public override string[] Keywords   => new[] { "сколько решений", "система", "определить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сколько решений имеет система?\n\n" +
                    "Приводим оба уравнения к виду y = kx + b и сравниваем:\n\n" +
                    "  · k₁ ≠ k₂ → одно решение (прямые пересекаются)\n" +
                    "  · k₁ = k₂, b₁ ≠ b₂ → нет решений (параллельны)\n" +
                    "  · k₁ = k₂, b₁ = b₂ → бесконечно много (совпадают)\n\n" +
                    "✏️ Введи первое уравнение:",
                Validate = LinearEquationParser.Validate
            },
            new InputStep
            {
                Question = "✏️ Введи второе уравнение:",
                Validate = LinearEquationParser.Validate
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a1, b1, c1) = LinearEquationParser.Parse(answers[0])!.Value;
            var (a2, b2, c2) = LinearEquationParser.Parse(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Уравнение 1: {LinearEquationParser.Format(a1, b1, c1)}");
            sb.AppendLine($"Уравнение 2: {LinearEquationParser.Format(a2, b2, c2)}");
            sb.AppendLine();

            var yf1 = LinearEquationParser.ToYForm(a1, b1, c1);
            var yf2 = LinearEquationParser.ToYForm(a2, b2, c2);

            if (yf1 is null || yf2 is null)
            {
                sb.AppendLine("Одно из уравнений — вертикальная прямая.");
                if (yf1 is null && yf2 is null)
                {
                    double xv1 = c1 / a1, xv2 = c2 / a2;
                    if (Math.Abs(xv1 - xv2) < 1e-9)
                        sb.AppendLine("📌 Бесконечно много решений (прямые совпадают).");
                    else
                        sb.AppendLine("📌 Нет решений (прямые параллельны).");
                }
                else
                    sb.AppendLine("📌 Одно решение (вертикальная прямая пересекает наклонную).");
                return sb.ToString().TrimEnd();
            }

            var (k1, bv1) = yf1.Value;
            var (k2, bv2) = yf2.Value;

            sb.AppendLine("Приводим к виду y = kx + b:");
            sb.AppendLine($"  1) {LinearHelper.FormatLinear(k1, bv1)}  →  k₁ = {LinearHelper.Fmt(k1)}, b₁ = {LinearHelper.Fmt(bv1)}");
            sb.AppendLine($"  2) {LinearHelper.FormatLinear(k2, bv2)}  →  k₂ = {LinearHelper.Fmt(k2)}, b₂ = {LinearHelper.Fmt(bv2)}");
            sb.AppendLine();

            bool kEq = Math.Abs(k1 - k2) < 1e-9;
            bool bEq = Math.Abs(bv1 - bv2) < 1e-9;

            if (!kEq)
            {
                sb.AppendLine($"k₁ ≠ k₂ ({LinearHelper.Fmt(k1)} ≠ {LinearHelper.Fmt(k2)}) — прямые пересекаются.");
                sb.AppendLine("\n📌 Система имеет ОДНО решение.");
            }
            else if (!bEq)
            {
                sb.AppendLine($"k₁ = k₂ = {LinearHelper.Fmt(k1)}, но b₁ ≠ b₂ — прямые параллельны.");
                sb.AppendLine("\n📌 Система НЕ ИМЕЕТ решений.  Ответ: ∅");
            }
            else
            {
                sb.AppendLine($"k₁ = k₂ = {LinearHelper.Fmt(k1)} и b₁ = b₂ = {LinearHelper.Fmt(bv1)} — прямые совпадают.");
                sb.AppendLine("\n📌 Система имеет БЕСКОНЕЧНО МНОГО решений.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  24.9 Найти значение выражения 7x₀ + 3y₀, если (x₀; y₀)
    //  является решением системы

    public class SystemEvalExpressionFunction : FunctionBase
    {
        public override string   Name       => "Найти значение выражения по решению системы";
        public override string   Formula    => "Решить систему → подставить в выражение";
        public override string[] Keywords   => new[] { "значение выражения", "решение системы", "подставить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти значение выражения\n\n" +
                    "Дано: (x₀; y₀) — решение системы уравнений.\n" +
                    "Нужно найти значение выражения вида ax₀ + by₀.\n\n" +
                    "Метод:\n" +
                    "  1. Решить систему — найти x₀ и y₀\n" +
                    "  2. Подставить в выражение\n\n" +
                    "✏️ Введи первое уравнение системы:",
                Validate = LinearEquationParser.Validate
            },
            new InputStep
            {
                Question = "✏️ Введи второе уравнение системы:",
                Validate = LinearEquationParser.Validate
            },
            new InputStep
            {
                Question =
                    "✏️ Введи коэффициент при x в выражении:\n" +
                    "  (в выражении ax₀ + by₀  →  введи a)\n" +
                    "  Пример: 7",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи коэффициент при y в выражении:\n" +
                           "  Пример: 3",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (a1, b1, c1) = LinearEquationParser.Parse(answers[0])!.Value;
            var (a2, b2, c2) = LinearEquationParser.Parse(answers[1])!.Value;
            double ea = LinearHelper.ParseNumber(answers[2]);
            double eb = LinearHelper.ParseNumber(answers[3]);
            var sb = new StringBuilder();

            sb.AppendLine($"Система:");
            sb.AppendLine($"  {LinearEquationParser.Format(a1, b1, c1)}");
            sb.AppendLine($"  {LinearEquationParser.Format(a2, b2, c2)}");
            sb.AppendLine($"Выражение: {LinearHelper.Fmt(ea)}x₀ + {LinearHelper.Fmt(eb)}y₀");
            sb.AppendLine();

            // Решаем систему методом Крамера
            double det = a1 * b2 - a2 * b1;
            if (Math.Abs(det) < 1e-12)
            {
                sb.AppendLine("⚠️ Определитель = 0 — система либо не имеет решений, либо имеет бесконечно много.");
                return sb.ToString().TrimEnd();
            }

            double x = (c1 * b2 - c2 * b1) / det;
            double y = (a1 * c2 - a2 * c1) / det;

            sb.AppendLine("Шаг 1. Решаем систему (метод Крамера):");
            sb.AppendLine($"  D  = {LinearHelper.Fmt(a1)}·{LinearHelper.Fmt(b2)} − {LinearHelper.Fmt(a2)}·{LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(det)}");
            sb.AppendLine($"  Dx = {LinearHelper.Fmt(c1)}·{LinearHelper.Fmt(b2)} − {LinearHelper.Fmt(c2)}·{LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(c1 * b2 - c2 * b1)}");
            sb.AppendLine($"  Dy = {LinearHelper.Fmt(a1)}·{LinearHelper.Fmt(c2)} − {LinearHelper.Fmt(a2)}·{LinearHelper.Fmt(c1)} = {LinearHelper.Fmt(a1 * c2 - a2 * c1)}");
            sb.AppendLine($"  x₀ = {LinearHelper.Fmt(x)},  y₀ = {LinearHelper.Fmt(y)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Подставляем в выражение:");
            sb.AppendLine($"  {LinearHelper.Fmt(ea)}·{LinearHelper.Fmt(x)} + {LinearHelper.Fmt(eb)}·{LinearHelper.Fmt(y)}");
            sb.AppendLine($"  = {LinearHelper.Fmt(ea * x)} + {LinearHelper.Fmt(eb * y)}");

            double result = ea * x + eb * y;
            sb.AppendLine();
            sb.AppendLine($"📌 Значение выражения = {LinearHelper.Fmt(result)}");

            return sb.ToString().TrimEnd();
        }
    }
}

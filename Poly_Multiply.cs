using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Вспомогательный класс: умножение многочленов
    // ═══════════════════════════════════════════════════════════════

    internal static class PolyMultiplyHelper
    {
        /// <summary>Перемножить два списка слагаемых (раскрыть скобки).</summary>
        public static List<PolyTerm> MultiplyPolys(List<PolyTerm> a, List<PolyTerm> b) => Multiply(a, b);

        public static List<PolyTerm> Multiply(List<PolyTerm> a, List<PolyTerm> b)
        {
            var result = new List<PolyTerm>();
            foreach (var ta in a)
                foreach (var tb in b)
                    result.Add(new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree));
            return result;
        }

        /// <summary>Разобрать одночлен вида "3x^2", "-x", "5", "-2x" → (коэф, степень).</summary>
        public static (long coeff, int degree) ParseMonomial(string s)
        {
            s = s.Trim().Replace(" ", "").Replace("−", "-").Replace("–", "-");
            if (string.IsNullOrEmpty(s)) throw new FormatException("Пустая строка.");

            int sign = 1;
            if (s.StartsWith("-")) { sign = -1; s = s.Substring(1); }
            else if (s.StartsWith("+")) { s = s.Substring(1); }

            int xPos = s.IndexOf('x');
            if (xPos < 0)
            {
                if (!long.TryParse(s, out long c))
                    throw new FormatException($"Не могу прочитать «{s}» как одночлен.");
                return (sign * c, 0);
            }

            string coeffPart = s.Substring(0, xPos);
            string afterX    = s.Substring(xPos + 1);

            long coeff;
            if (string.IsNullOrEmpty(coeffPart))      coeff = 1;
            else if (!long.TryParse(coeffPart, out coeff))
                throw new FormatException($"Не могу прочитать коэффициент «{coeffPart}».");

            int degree;
            if (string.IsNullOrEmpty(afterX))
            {
                degree = 1;
            }
            else if (afterX.StartsWith("^"))
            {
                string degStr = afterX.Substring(1);
                if (!int.TryParse(degStr, out degree) || degree < 0)
                    throw new FormatException($"Не могу прочитать степень «{degStr}».");
            }
            else
            {
                throw new FormatException($"После x ожидался ^N, а получилось «{afterX}».");
            }

            return (sign * coeff, degree);
        }

        /// <summary>Форматировать одночлен (коэф, степень) в строку.</summary>
        public static string FormatMonomial(long coeff, int degree)
        {
            if (coeff == 0) return "0";
            string sign = coeff < 0 ? "-" : "";
            long abs    = Math.Abs(coeff);
            if (degree == 0) return $"{coeff}";
            string xPart = degree == 1 ? "x" : "x" + PolyTerm.Sup(degree);
            return abs == 1 ? $"{sign}{xPart}" : $"{coeff}{xPart}";
        }

        /// <summary>Валидатор одночлена для InputStep.</summary>
        public static string? ValidateMonomial(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Введи одночлен, например: 3x^2 или -2x или 5";
            try { ParseMonomial(s); return null; }
            catch (FormatException ex)
                { return $"Не могу разобрать: {ex.Message}\nПример: 3x^2 или -x или 7"; }
        }

        /// <summary>Построить детальный вывод раскрытия скобок (A)×(B).</summary>
        public static string BuildMultiplyDetail(
            List<PolyTerm> a, List<PolyTerm> b,
            string aStr, string bStr, string prefix = "")
        {
            var raw     = Multiply(a, b);
            var reduced = PolyParser.Reduce(raw);
            var sb      = new StringBuilder();

            sb.AppendLine($"{prefix}({aStr}) · ({bStr})");
            sb.AppendLine();
            sb.AppendLine("Раскрываем скобки (каждый член первого × каждый член второго):");

            // Строчка за строчкой: aᵢ × весь b
            foreach (var ta in a)
            {
                var row = b.Select(tb => new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree)).ToList();
                string rowStr = PolyParser.Format(row);
                sb.AppendLine($"  {ta.ToStringFirst()} × ({bStr}) = {rowStr}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем все слагаемые:");
            sb.AppendLine($"  {PolyParser.Format(raw)}");

            var groups = raw.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string label = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {label}: {chain} = {g.Sum(t => t.Coeff)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 1: Многочлен × одночлен
    // ═══════════════════════════════════════════════════════════════

    public class PolyTimesMonomial : FunctionBase
    {
        public override string   Name       => "Многочлен × одночлен";
        public override string   Formula    => "(aₙxⁿ + … + a₀) · kxᵐ";
        public override string[] Keywords   => new[] { "умножение", "многочлен", "одночлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Умножение многочлена на одночлен\n\n" +
                    "Правило: умножаем каждый член многочлена на одночлен.\n\n" +
                    "Пример: (3a − 7b) · 0,4a\n" +
                    "  = 3a · 0,4a + (−7b) · 0,4a\n" +
                    "  = 1,2a² − 2,8ab\n\n" +
                    "Как записывать: x² → x^2, слагаемые через + или −\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи одночлен (например: 3x^2 или -2x или 5):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly              = PolyParser.Parse(answers[0]);
            var (monoK, monoDeg) = PolyMultiplyHelper.ParseMonomial(answers[1]);
            string monoStr        = PolyMultiplyHelper.FormatMonomial(monoK, monoDeg);

            var raw     = poly.Select(t => new PolyTerm(t.Coeff * monoK, t.Degree + monoDeg)).ToList();
            var reduced = PolyParser.Reduce(raw);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(poly)}) · ({monoStr})");
            sb.AppendLine();
            sb.AppendLine("Умножаем каждый член многочлена на одночлен:");
            foreach (var t in poly)
            {
                var res = new PolyTerm(t.Coeff * monoK, t.Degree + monoDeg);
                sb.AppendLine($"  {t.ToStringFirst()} · ({monoStr}) = {res.ToStringFirst()}");
            }
            sb.AppendLine();
            sb.AppendLine($"Собираем: {PolyParser.Format(raw)}");

            var groups = raw.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string lbl   = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {lbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 2: Многочлен × многочлен
    // ═══════════════════════════════════════════════════════════════

    public class PolyTimesPolyFunction : FunctionBase
    {
        public override string   Name       => "Многочлен × многочлен";
        public override string   Formula    => "(A) · (B) — каждый член на каждый";
        public override string[] Keywords   => new[] { "умножение", "многочлен", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Умножение многочлена на многочлен\n\n" +
                    "Правило: каждый член первого умножаем на каждый член второго, " +
                    "затем приводим подобные.\n\n" +
                    "Пример: (a + c)(a² − c² + 5)\n" +
                    "  a · a² + a · (−c²) + a · 5\n" +
                    "  + c · a² + c · (−c²) + c · 5\n" +
                    "  = a³ − ac² + 5a + a²c − c³ + 5c\n\n" +
                    "✏️ Введи первый многочлен:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второй многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a = PolyParser.Parse(answers[0]);
            var b = PolyParser.Parse(answers[1]);

            string detail = PolyMultiplyHelper.BuildMultiplyDetail(
                a, b, PolyParser.Format(a), PolyParser.Format(b), "✅ ");
            return detail;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 3: Упростить выражение (сумма произведений)
    // ═══════════════════════════════════════════════════════════════

    public class PolySimplifyExpression : FunctionBase
    {
        public override string   Name       => "Упростить выражение";
        public override string   Formula    => "A·B ± C·D → раскрыть и привести подобные";
        public override string[] Keywords   => new[] { "упростить", "выражение", "произведение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражения со скобками\n\n" +
                    "Пример: 8(3a − 2m) − 5(2a − m)\n" +
                    "  = 24a − 16m − 10a + 5m\n" +
                    "  = 14a − 11m\n\n" +
                    "Введи первый множитель (одночлен или многочлен).\n\n" +
                    "✏️ Первый множитель 1-го произведения:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Второй множитель 1-го произведения:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "Есть второе произведение?\n\n" +
                    "Напиши знак и множители через * (пример: -5 * 2x-m)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    var parts = s.Split('*');
                    if (parts.Length != 2)
                        return "Напиши два множителя через * (пример: -5 * 2x-m) или 0";
                    try
                    {
                        PolyParser.Parse(parts[0].TrimStart('+').Trim());
                        PolyParser.Parse(parts[1].Trim());
                        return null;
                    }
                    catch (FormatException ex) { return ex.Message; }
                }
            },
            new InputStep
            {
                Question =
                    "Есть третье произведение?\n\n" +
                    "Напиши знак и множители через * (пример: +3 * x^2-1)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    var parts = s.Split('*');
                    if (parts.Length != 2)
                        return "Напиши два множителя через * или 0";
                    try
                    {
                        PolyParser.Parse(parts[0].TrimStart('+').Trim());
                        PolyParser.Parse(parts[1].Trim());
                        return null;
                    }
                    catch (FormatException ex) { return ex.Message; }
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var sb = new StringBuilder();

            // 1-е произведение
            var a1 = PolyParser.Parse(answers[0]);
            var b1 = PolyParser.Parse(answers[1]);
            var p1 = PolyMultiplyHelper.Multiply(a1, b1);

            sb.AppendLine("✅ Упрощаем выражение:");
            sb.AppendLine();
            sb.AppendLine($"Произведение 1: ({PolyParser.Format(a1)}) · ({PolyParser.Format(b1)})");
            foreach (var ta in a1)
            {
                var row = b1.Select(tb => new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree)).ToList();
                sb.AppendLine($"  {ta.ToStringFirst()} · ({PolyParser.Format(b1)}) = {PolyParser.Format(row)}");
            }
            sb.AppendLine($"  = {PolyParser.Format(p1)}");

            var allTerms = new List<PolyTerm>(p1);

            // 2-е произведение
            if (answers[2].Trim() != "0")
            {
                var parts2 = answers[2].Split('*');
                int sign2  = parts2[0].TrimStart().StartsWith("-") ? -1 : 1;
                var c1     = PolyParser.Parse(parts2[0].TrimStart('+').Trim());
                var d1     = PolyParser.Parse(parts2[1].Trim());
                var p2raw  = PolyMultiplyHelper.Multiply(c1, d1);
                var p2     = p2raw.Select(t => new PolyTerm(sign2 * t.Coeff, t.Degree)).ToList();

                sb.AppendLine();
                sb.AppendLine($"Произведение 2: ({PolyParser.Format(c1)}) · ({PolyParser.Format(d1)})");
                foreach (var tc in c1)
                {
                    var row = d1.Select(td => new PolyTerm(sign2 * tc.Coeff * td.Coeff, tc.Degree + td.Degree)).ToList();
                    sb.AppendLine($"  {tc.ToStringFirst()} · ({PolyParser.Format(d1)}) = {PolyParser.Format(row)}");
                }
                sb.AppendLine($"  = {PolyParser.Format(p2)}");
                allTerms.AddRange(p2);
            }

            // 3-е произведение
            if (answers[3].Trim() != "0")
            {
                var parts3 = answers[3].Split('*');
                int sign3  = parts3[0].TrimStart().StartsWith("-") ? -1 : 1;
                var e1     = PolyParser.Parse(parts3[0].TrimStart('+').Trim());
                var f1     = PolyParser.Parse(parts3[1].Trim());
                var p3raw  = PolyMultiplyHelper.Multiply(e1, f1);
                var p3     = p3raw.Select(t => new PolyTerm(sign3 * t.Coeff, t.Degree)).ToList();

                sb.AppendLine();
                sb.AppendLine($"Произведение 3: ({PolyParser.Format(e1)}) · ({PolyParser.Format(f1)})");
                foreach (var te in e1)
                {
                    var row = f1.Select(tf => new PolyTerm(sign3 * te.Coeff * tf.Coeff, te.Degree + tf.Degree)).ToList();
                    sb.AppendLine($"  {te.ToStringFirst()} · ({PolyParser.Format(f1)}) = {PolyParser.Format(row)}");
                }
                sb.AppendLine($"  = {PolyParser.Format(p3)}");
                allTerms.AddRange(p3);
            }

            var reduced = PolyParser.Reduce(allTerms);

            sb.AppendLine();
            sb.AppendLine("Собираем все слагаемые вместе:");
            sb.AppendLine($"  {PolyParser.Format(allTerms)}");

            var likeGroups = allTerms.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (likeGroups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Приводим подобные:");
                foreach (var g in likeGroups.OrderByDescending(g => g.Key))
                {
                    string lbl   = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {lbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 4: Вычислить значение произведения при x = …
    // ═══════════════════════════════════════════════════════════════

    public class PolyEvalProduct : FunctionBase
    {
        public override string   Name       => "Вычислить произведение при x";
        public override string   Formula    => "(A)·(B) при x = n";
        public override string[] Keywords   => new[] { "вычислить", "произведение", "значение", "при x" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вычисление значения произведения\n\n" +
                    "Способ: сначала раскрыть и упростить, потом подставить x.\n\n" +
                    "Пример: 8a²·(4ab) + 15ac²·(5c²) при a = 3\n" +
                    "  Шаг 1. Упрощаем каждое слагаемое\n" +
                    "  Шаг 2. Подставляем значение\n\n" +
                    "✏️ Введи первый многочлен:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второй многочлен:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи значение x (число):",
                Validate = PolyValidate.CheckNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a       = PolyParser.Parse(answers[0]);
            var b       = PolyParser.Parse(answers[1]);
            var raw     = PolyMultiplyHelper.Multiply(a, b);
            var reduced = PolyParser.Reduce(raw);

            double xVal = double.Parse(answers[2].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);

            double result = reduced.Sum(t => t.Coeff * Math.Pow(xVal, t.Degree));

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(a)}) · ({PolyParser.Format(b)}) при x = {answers[2]}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем скобки:");
            foreach (var ta in a)
            {
                var row = b.Select(tb => new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree)).ToList();
                sb.AppendLine($"  {ta.ToStringFirst()} · ({PolyParser.Format(b)}) = {PolyParser.Format(row)}");
            }
            sb.AppendLine();
            sb.AppendLine($"  Всё вместе: {PolyParser.Format(raw)}");

            var groups = raw.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Шаг 2. Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string lbl   = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {lbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
                sb.AppendLine($"  Упрощённый многочлен: {PolyParser.Format(reduced)}");
            }

            sb.AppendLine();
            sb.AppendLine($"Шаг 3. Подставляем x = {answers[2]}:");
            var parts = reduced.OrderByDescending(t => t.Degree).ToList();
            sb.AppendLine($"  {string.Join(" + ", parts.Select(t => $"({t.Coeff})·{answers[2]}{PolyTerm.Sup(t.Degree)}")).Replace("+ (", "+ (").Replace("+-", "-")}");
            sb.AppendLine();
            sb.AppendLine($"📌 Результат: {result:G10}".TrimEnd('0').TrimEnd('.'));
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 5: Уравнение с многочленами
    // ═══════════════════════════════════════════════════════════════

    public class PolyEquation : FunctionBase
    {
        public override string   Name       => "Уравнение с многочленами";
        public override string   Formula    => "(A)·(B) = C → раскрыть → решить";
        public override string[] Keywords   => new[] { "уравнение", "многочлен", "решить" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Уравнение с многочленами\n\n" +
                    "Пример: 3x(x² − 8) − 3x³ = 12\n" +
                    "  1. Раскрываем: 3x³ − 24x − 3x³ = 12\n" +
                    "  2. Приводим подобные: −24x = 12\n" +
                    "  3. Решаем: x = −1/2\n\n" +
                    "Введи левую часть уравнения как два множителя:\n\n" +
                    "✏️ Первый множитель левой части:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Второй множитель левой части (или 1, если скобок нет):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Правая часть уравнения (многочлен или число):",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a   = PolyParser.Parse(answers[0]);
            var b   = PolyParser.Parse(answers[1]);
            var rhs = PolyParser.Parse(answers[2]);

            var left    = PolyParser.Reduce(PolyMultiplyHelper.Multiply(a, b));
            var rhsNeg  = rhs.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();
            var diff    = PolyParser.Reduce(left.Concat(rhsNeg).ToList());

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(a)}) · ({PolyParser.Format(b)}) = {PolyParser.Format(rhs)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем скобки:");
            foreach (var ta in a)
            {
                var row = b.Select(tb => new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree)).ToList();
                sb.AppendLine($"  {ta.ToStringFirst()} · ({PolyParser.Format(b)}) = {PolyParser.Format(row)}");
            }
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Приводим подобные слева:");
            sb.AppendLine($"  {PolyParser.Format(left)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 3. Переносим всё влево (меняем знаки правой части):");
            sb.AppendLine($"  {PolyParser.Format(diff)} = 0");
            sb.AppendLine();

            int deg = PolyParser.PolynomialDegree(diff);

            if (deg == 0)
            {
                long free = diff.FirstOrDefault(t => t.Degree == 0).Coeff;
                sb.AppendLine(free == 0
                    ? "📌 Уравнение верно при любом x."
                    : "📌 Уравнение не имеет решений.");
            }
            else if (deg == 1)
            {
                long a1 = diff.FirstOrDefault(t => t.Degree == 1).Coeff;
                long a0 = diff.FirstOrDefault(t => t.Degree == 0).Coeff;
                sb.AppendLine("Линейное уравнение:");
                sb.AppendLine($"  {a1}x = {-a0}");
                string xVal = (a0 % a1 == 0) ? $"{-a0 / a1}" : $"{-a0}/{a1}";
                sb.AppendLine();
                sb.AppendLine($"📌 x = {xVal}");
            }
            else if (deg == 2)
            {
                long a2 = diff.FirstOrDefault(t => t.Degree == 2).Coeff;
                long a1 = diff.FirstOrDefault(t => t.Degree == 1).Coeff;
                long a0 = diff.FirstOrDefault(t => t.Degree == 0).Coeff;
                double D = a1 * a1 - 4.0 * a2 * a0;
                sb.AppendLine($"Квадратное уравнение: {a2}x² + {a1}x + {a0} = 0");
                sb.AppendLine($"  D = {a1}² − 4·{a2}·{a0} = {D}");
                if (D < 0)       sb.AppendLine("📌 Корней нет (D < 0).");
                else if (D == 0) sb.AppendLine($"📌 x = {-a1 / (2.0 * a2):G10}".TrimEnd('0').TrimEnd('.'));
                else
                {
                    double x1 = (-a1 + Math.Sqrt(D)) / (2.0 * a2);
                    double x2 = (-a1 - Math.Sqrt(D)) / (2.0 * a2);
                    sb.AppendLine($"📌 x₁ = {x1:G10}, x₂ = {x2:G10}".TrimEnd('0').TrimEnd('.'));
                }
            }
            else
            {
                sb.AppendLine($"Уравнение степени {deg} — воспользуйся разделом «Разложение на множители».");
                sb.AppendLine($"📌 {PolyParser.Format(diff)} = 0");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 6: Доказать тождество
    // ═══════════════════════════════════════════════════════════════

    public class PolyProveIdentity : FunctionBase
    {
        public override string   Name       => "Доказать тождество";
        public override string   Formula    => "A·B = C — раскрыть левую, сравнить";
        public override string[] Keywords   => new[] { "тождество", "доказать", "равенство" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождества\n\n" +
                    "Идея: раскрываем левую часть и проверяем, совпадает ли с правой.\n\n" +
                    "Пример: (7x − 3)(4 − 8x) + 2x(28x − 26) = −12\n" +
                    "  Левая: 28x − 56x² − 12 + 24x + 56x² − 52x = −12 ✓\n\n" +
                    "✏️ Первый множитель левой части:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Второй множитель левой части:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "Есть ли ещё слагаемое (произведение) в левой части?\n\n" +
                    "Напиши знак и оба множителя через * (пример: +2x * 28x-26)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    var parts = s.Split('*');
                    if (parts.Length != 2)
                        return "Напиши два множителя через * (пример: +2x * 28x-26) или 0";
                    try
                    {
                        PolyParser.Parse(parts[0].TrimStart('+').Trim());
                        PolyParser.Parse(parts[1].Trim());
                        return null;
                    }
                    catch (FormatException ex) { return ex.Message; }
                }
            },
            new InputStep
            {
                Question = "✏️ Правая часть тождества:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a   = PolyParser.Parse(answers[0]);
            var b   = PolyParser.Parse(answers[1]);
            var rhs = PolyParser.Parse(answers[3]);

            var allLeft = new List<PolyTerm>(PolyMultiplyHelper.Multiply(a, b));

            var sb = new StringBuilder();
            sb.AppendLine("✅ Доказательство тождества");
            sb.AppendLine();
            sb.AppendLine($"Левая часть: ({PolyParser.Format(a)}) · ({PolyParser.Format(b)}){(answers[2].Trim() != "0" ? " + …" : "")}");
            sb.AppendLine();
            sb.AppendLine($"Раскрываем ({PolyParser.Format(a)}) · ({PolyParser.Format(b)}):");
            foreach (var ta in a)
            {
                var row = b.Select(tb => new PolyTerm(ta.Coeff * tb.Coeff, ta.Degree + tb.Degree)).ToList();
                sb.AppendLine($"  {ta.ToStringFirst()} · ({PolyParser.Format(b)}) = {PolyParser.Format(row)}");
            }

            if (answers[2].Trim() != "0")
            {
                var parts = answers[2].Split('*');
                int sign  = parts[0].TrimStart().StartsWith("-") ? -1 : 1;
                var c     = PolyParser.Parse(parts[0].TrimStart('+').Trim());
                var d     = PolyParser.Parse(parts[1].Trim());
                var p2raw = PolyMultiplyHelper.Multiply(c, d);
                var p2    = p2raw.Select(t => new PolyTerm(sign * t.Coeff, t.Degree)).ToList();

                sb.AppendLine();
                sb.AppendLine($"Раскрываем ({PolyParser.Format(c)}) · ({PolyParser.Format(d)}):");
                foreach (var tc in c)
                {
                    var row = d.Select(td => new PolyTerm(sign * tc.Coeff * td.Coeff, tc.Degree + td.Degree)).ToList();
                    sb.AppendLine($"  {tc.ToStringFirst()} · ({PolyParser.Format(d)}) = {PolyParser.Format(row)}");
                }
                allLeft.AddRange(p2);
            }

            var reduced = PolyParser.Reduce(allLeft);

            var likeGroups = allLeft.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (likeGroups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Приводим подобные слагаемые:");
                foreach (var g in likeGroups.OrderByDescending(g => g.Key))
                {
                    string lbl   = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {lbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Левая часть после упрощения: {PolyParser.Format(reduced)}");
            sb.AppendLine($"Правая часть:                {PolyParser.Format(rhs)}");
            sb.AppendLine();

            bool equal = PolyParser.Format(reduced) == PolyParser.Format(PolyParser.Reduce(rhs.ToList()));
            if (equal)
                sb.AppendLine("✅ Левая часть = Правая часть — тождество доказано!");
            else
                sb.AppendLine("⚠️ Части не совпали — проверь, правильно ли введено выражение.");

            return sb.ToString().TrimEnd();
        }
    }
}

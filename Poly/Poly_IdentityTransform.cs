using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Тождественные преобразования выражений
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Упрощение суммы нескольких произведений:
    /// A·B + C·D + ... → раскрыть все скобки → привести подобные
    /// </summary>
    internal class IdentitySimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить (несколько произведений)";
        public override string   Formula    => "A·B ± C·D ± ... → раскрыть и привести подобные";
        public override string[] Keywords   => new[] { "упростить", "тождество", "преобразование", "несколько" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражения — несколько произведений\n\n" +
                    "Пример:\n" +
                    "  (5x − 2)(3x² + 2x − 9) − 7(4/7·x² − 7x) − 16x³\n\n" +
                    "Шаги:\n" +
                    "  1. Раскрыть каждые скобки\n" +
                    "  2. Привести подобные члены\n\n" +
                    "Введи первое произведение (два множителя отдельно).\n\n" +
                    "✏️ Первый множитель (1-го произведения):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Второй множитель (1-го произведения):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "Есть ли второе произведение?\n\n" +
                    "Напиши знак и оба множителя через * (например: -7 * x^2-7x)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    var parts = s.Split('*');
                    if (parts.Length != 2)
                        return "Напиши два множителя через *  (например: -7 * x^2-7x), или 0";
                    string first = parts[0].Trim();
                    // Первый может начинаться с + или - (знак)
                    string testFirst = first.TrimStart('+');
                    try { PolyParser.Parse(testFirst.Length > 0 ? testFirst : "0"); }
                    catch (FormatException ex) { return $"Первый множитель: {ex.Message}"; }
                    try { PolyParser.Parse(parts[1].Trim()); }
                    catch (FormatException ex) { return $"Второй множитель: {ex.Message}"; }
                    return null;
                }
            },
            new InputStep
            {
                Question =
                    "Есть ли третье слагаемое (одночлен или многочлен без умножения)?\n\n" +
                    "Напиши его со знаком (например: -16x^3 или +5x или -4)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    if (!s.StartsWith("+") && !s.StartsWith("-"))
                        return "Начни с + или −, либо напиши 0";
                    try { PolyParser.Parse(s); return null; }
                    catch (FormatException ex) { return ex.Message; }
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a1 = PolyParser.Parse(answers[0]);
            var b1 = PolyParser.Parse(answers[1]);
            var prod1 = PolyMultiplyHelper.MultiplyPolys(a1, b1);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Упрощаем выражение");
            sb.AppendLine();
            sb.AppendLine($"Шаг 1. ({PolyParser.Format(a1)}) · ({PolyParser.Format(b1)}):");
            sb.AppendLine($"  = {PolyParser.Format(prod1)}");

            var all = new List<PolyTerm>(prod1);

            // Второе произведение
            string second = answers[2].Trim();
            if (second != "0")
            {
                var parts = second.Split('*');
                string signedFirst = parts[0].Trim();
                // Знак выносим отдельно
                bool negative = signedFirst.StartsWith("-");
                string cleanFirst = signedFirst.TrimStart('+', '-');
                if (string.IsNullOrEmpty(cleanFirst)) cleanFirst = "1";

                var a2 = PolyParser.Parse(cleanFirst);
                var b2 = PolyParser.Parse(parts[1].Trim());
                var prod2 = PolyMultiplyHelper.MultiplyPolys(a2, b2);

                if (negative)
                    prod2 = prod2.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();

                string sign = negative ? "−" : "+";
                sb.AppendLine();
                sb.AppendLine($"Шаг 2. {sign} ({PolyParser.Format(a2)}) · ({PolyParser.Format(b2)}):");
                sb.AppendLine($"  = {(negative ? "−" : "")}{PolyParser.Format(prod2.Select(t => negative ? new PolyTerm(-t.Coeff, t.Degree) : t).ToList())}");
                all.AddRange(prod2);
            }

            // Третье слагаемое
            string extra = answers[3].Trim();
            if (extra != "0")
            {
                var extraTerms = PolyParser.Parse(extra);
                sb.AppendLine();
                sb.AppendLine($"Шаг 3. Добавляем: {PolyParser.Format(extraTerms)}");
                all.AddRange(extraTerms);
            }

            // Приводим подобные
            var reduced = PolyParser.Reduce(all);
            sb.AppendLine();
            sb.AppendLine("Раскрыли все скобки, собираем:");
            sb.AppendLine($"  {PolyParser.Format(all)}");
            sb.AppendLine();

            var groups = all.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine("Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string xLbl = g.Key == 0 ? "свободные" : $"x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {xLbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Представить алгебраическую сумму в виде произведения —
    /// вынести общий множитель + группировка (уже есть отдельно,
    /// этот класс объединяет подсказку и направляет к нужному разделу)
    /// </summary>
    public class IdentitySumAsProductFunction : FunctionBase
    {
        public override string   Name       => "Сумму представить произведением";
        public override string   Formula    => "вынести общий множитель → получить произведение";
        public override string[] Keywords   => new[] { "произведение", "сумма", "вынести", "представить" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Представить сумму в виде произведения\n\n" +
                    "Пример: 7a²b − 6ab − 21ab² + 18b²\n" +
                    "  Шаг 1. Выносим общий множитель b:\n" +
                    "    b(7a² − 6a − 21ab + 18b)\n" +
                    "  Шаг 2. Группируем по два:\n" +
                    "    b[(7a² − 21ab) + (−6a + 18b)]\n" +
                    "    = b[7a(a − 3b) + (−6)(a − 3b)]\n" +
                    "    = b(7a − 6)(a − 3b)\n\n" +
                    "Введи многочлен — покажу шаги разложения через вынесение и группировку.\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Многочлен: {PolyParser.Format(reduced)}");
            sb.AppendLine();

            // Найдём НОД всех коэффициентов
            long gcd = reduced.Select(t => Math.Abs(t.Coeff)).Aggregate(GCD);
            int  minDeg = reduced.Min(t => t.Degree);

            bool hasCommon = gcd > 1 || minDeg > 0;

            if (hasCommon)
            {
                string commonMono = PolyMultiplyHelper.FormatMonomial(gcd, minDeg);
                var inner = reduced.Select(t => new PolyTerm(t.Coeff / gcd, t.Degree - minDeg)).ToList();

                sb.AppendLine($"Шаг 1. Находим общий множитель: {commonMono}");
                sb.AppendLine($"  Выносим за скобки:");
                sb.AppendLine($"  {commonMono}·({PolyParser.Format(inner)})");
                sb.AppendLine();

                if (inner.Count >= 4)
                {
                    sb.AppendLine("Шаг 2. Пробуем сгруппировать скобочный многочлен по два:");
                    int half = inner.Count / 2;
                    var g1 = inner.Take(half).ToList();
                    var g2 = inner.Skip(half).ToList();
                    sb.AppendLine($"  Группа 1: {PolyParser.Format(g1)}");
                    sb.AppendLine($"  Группа 2: {PolyParser.Format(g2)}");
                    sb.AppendLine();
                    sb.AppendLine("Для полного разложения используй «Группировка: 4 члена (2+2)».");
                }
                else
                {
                    sb.AppendLine($"📌 Результат: {commonMono}·({PolyParser.Format(inner)})");
                }
            }
            else
            {
                sb.AppendLine("Общего числового множителя нет.");
                if (reduced.Count >= 4)
                {
                    sb.AppendLine("Попробуем группировку по два:");
                    int half = reduced.Count / 2;
                    var g1 = reduced.Take(half).ToList();
                    var g2 = reduced.Skip(half).ToList();
                    sb.AppendLine($"  Группа 1: {PolyParser.Format(g1)}");
                    sb.AppendLine($"  Группа 2: {PolyParser.Format(g2)}");
                    sb.AppendLine();
                    sb.AppendLine("Для полного разложения используй «Группировка: 4 члена (2+2)».");
                }
                else
                {
                    sb.AppendLine("Многочлен не раскладывается стандартными методами этого раздела.");
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    }

    /// <summary>
    /// Найти наименьшее/наибольшее целое при котором верно неравенство
    /// </summary>
    public class IdentityInequalityIntegerFunction : FunctionBase
    {
        public override string   Name       => "Целое число для неравенства";
        public override string   Formula    => "A(x) op B(x) → найти наименьшее/наибольшее целое x";
        public override string[] Keywords   => new[] { "неравенство", "целое", "наименьшее", "наибольшее" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти наибольшее/наименьшее целое число для неравенства\n\n" +
                    "Пример: (4x − 1)(5 + 6x) − 3x·m(2 + 3x)(8x − 1)\n" +
                    "  Раскрываем, приводим подобные → линейное неравенство → решаем.\n\n" +
                    "Введи первый множитель левой части:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второй множитель левой части (или 1 если нет):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи правую часть неравенства:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "Знак неравенства:\n" +
                    "  1 — меньше (<)\n" +
                    "  2 — больше (>)\n" +
                    "  3 — меньше или равно (≤)\n" +
                    "  4 — больше или равно (≥)\n\n" +
                    "✏️ Введи цифру:",
                Validate = s => new[] { "1","2","3","4" }.Contains(s.Trim())
                    ? null : "Введи 1, 2, 3 или 4"
            },
            new InputStep
            {
                Question = "Что ищем?\n  1 — наименьшее целое\n  2 — наибольшее целое\n\n✏️ Введи 1 или 2:",
                Validate = s => s.Trim() == "1" || s.Trim() == "2" ? null : "Введи 1 или 2"
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a   = PolyParser.Parse(answers[0]);
            var b   = PolyParser.Parse(answers[1]);
            var rhs = PolyParser.Parse(answers[2]);
            string sign = answers[3].Trim();
            bool findMin = answers[4].Trim() == "1";

            var left    = PolyMultiplyHelper.MultiplyPolys(a, b);
            var rhsNeg  = rhs.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();
            var combined = PolyParser.Reduce(left.Concat(rhsNeg).ToList());

            var sb = new StringBuilder();
            string signStr = sign switch { "1" => "<", "2" => ">", "3" => "≤", "4" => "≥", _ => "?" };
            sb.AppendLine($"✅ ({PolyParser.Format(a)})·({PolyParser.Format(b)}) {signStr} {PolyParser.Format(rhs)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем левую часть:");
            sb.AppendLine($"  {PolyParser.Format(left)} {signStr} {PolyParser.Format(rhs)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Переносим всё влево:");
            sb.AppendLine($"  {PolyParser.Format(combined)} {signStr} 0");
            sb.AppendLine();

            int deg = PolyParser.PolynomialDegree(combined);

            if (deg == 1)
            {
                long a1 = combined.FirstOrDefault(t => t.Degree == 1).Coeff;
                long a0 = combined.FirstOrDefault(t => t.Degree == 0).Coeff;

                // a1·x + a0 </>/<=/=> 0  →  x </>/<=/=> -a0/a1
                // Если a1 < 0, знак меняется
                double bound = -(double)a0 / a1;
                bool flipped = a1 < 0;

                string effectiveSign = flipped
                    ? sign switch { "1" => ">", "2" => "<", "3" => "≥", "4" => "≤", _ => "?" }
                    : signStr;

                sb.AppendLine($"Линейное неравенство: {a1}x {signStr} {-a0}");
                if (flipped) sb.AppendLine("  (делим на отрицательное — знак меняется)");
                sb.AppendLine($"  x {effectiveSign} {bound:G6}");
                sb.AppendLine();

                long answer;
                if (findMin)
                {
                    answer = effectiveSign is ">" or "≥"
                        ? (long)Math.Ceiling(effectiveSign == ">" ? bound + 1e-9 : bound)
                        : (long)Math.Floor(bound);
                    // Для < x должно быть меньше bound — нет наименьшего, но наибольшее есть
                    bool noMin = effectiveSign is "<" or "≤";
                    if (noMin)
                    {
                        sb.AppendLine("Наименьшего целого нет (решение не ограничено снизу).");
                        sb.AppendLine($"📌 Наибольшее целое: {(long)(effectiveSign == "<" ? Math.Ceiling(bound) - 1 : Math.Floor(bound))}");
                    }
                    else
                    {
                        sb.AppendLine($"📌 Наименьшее целое x = {answer}");
                    }
                }
                else
                {
                    bool noMax = effectiveSign is ">" or "≥";
                    if (noMax)
                    {
                        sb.AppendLine("Наибольшего целого нет (решение не ограничено сверху).");
                        sb.AppendLine($"📌 Наименьшее целое: {(long)(effectiveSign == ">" ? Math.Floor(bound) + 1 : Math.Ceiling(bound))}");
                    }
                    else
                    {
                        answer = effectiveSign == "<"
                            ? (long)Math.Ceiling(bound) - 1
                            : (long)Math.Floor(bound);
                        sb.AppendLine($"📌 Наибольшее целое x = {answer}");
                    }
                }
            }
            else if (deg == 0)
            {
                sb.AppendLine("После упрощения переменная пропала.");
                sb.AppendLine("Неравенство либо верно для всех x, либо не верно ни для какого.");
            }
            else
            {
                sb.AppendLine($"Получили многочлен степени {deg} — решение квадратных и выше");
                sb.AppendLine("неравенств в разделе «Неравенства».");
            }

            return sb.ToString().TrimEnd();
        }
    }
}

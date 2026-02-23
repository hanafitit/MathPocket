using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Одно слагаемое вида  coeff · x^degree
    //  Степень пишется сразу после x: x2 = x², x3 = x³
    // ═══════════════════════════════════════════════════════════════
    internal readonly struct PolyTerm
    {
        public readonly long Coeff;
        public readonly int  Degree;

        public PolyTerm(long coeff, int degree) { Coeff = coeff; Degree = degree; }

        public static string Sup(int n)
        {
            const string d = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            var sb = new StringBuilder();
            foreach (char c in n.ToString())
                sb.Append(c >= '0' && c <= '9' ? d[c - '0'] : c);
            return sb.ToString();
        }

        /// <summary>Абсолютное значение слагаемого без знака: «5x²»</summary>
        public string ToAbs()
        {
            long absC = Math.Abs(Coeff);
            if (Degree == 0) return absC.ToString();
            string xPart = Degree == 1 ? "x" : "x" + Sup(Degree);
            return absC == 1 ? xPart : absC + xPart;
        }

        /// <summary>Первое слагаемое со знаком: «-3x²»</summary>
        public string ToStringFirst()
        {
            if (Coeff == 0) return "0";
            string sign = Coeff < 0 ? "-" : "";
            if (Degree == 0) return sign + Math.Abs(Coeff);
            string xPart = Degree == 1 ? "x" : "x" + Sup(Degree);
            return Math.Abs(Coeff) == 1 ? sign + xPart : sign + Math.Abs(Coeff) + xPart;
        }

        /// <summary>Следующие слагаемые: « + 3x²» или « - 5x»</summary>
        public string ToStringNext()
        {
            if (Coeff == 0) return "";
            return (Coeff < 0 ? " - " : " + ") + ToAbs();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Парсер строки многочлена
    //
    //  Принимает: "3x^2 - 5x + 2", "-x^3 + 4x", "7", "0"
    //  Правила записи:
    //    x² → x^2    x³ → x^3    x → x (степень 1)
    //    Знаки + и − разделяют слагаемые, пробелы игнорируются.
    // ═══════════════════════════════════════════════════════════════
    internal static class PolyParser
    {
        public static List<PolyTerm> Parse(string input)
        {
            string s = input.Trim()
                            .Replace(" ", "")
                            .Replace("−", "-")
                            .Replace("–", "-");

            if (s == "0") return new List<PolyTerm> { new PolyTerm(0, 0) };
            if (string.IsNullOrEmpty(s))
                throw new FormatException("Пустая строка — многочлен не введён.");

            // Разбиваем на токены: знак входит в следующий токен
            var tokens = new List<string>();
            int start = 0;
            for (int i = 1; i < s.Length; i++)
            {
                if (s[i] == '+' || s[i] == '-')
                {
                    tokens.Add(s.Substring(start, i - start));
                    start = i;
                }
            }
            tokens.Add(s.Substring(start));

            var result = new List<PolyTerm>();
            foreach (var tok in tokens)
            {
                if (string.IsNullOrEmpty(tok)) continue;
                result.Add(ParseTerm(tok));
            }
            if (result.Count == 0)
                throw new FormatException("Не удалось найти ни одного слагаемого.");
            return result;
        }

        private static PolyTerm ParseTerm(string tok)
        {
            int sign = 1;
            string t = tok;
            if (t.StartsWith("-")) { sign = -1; t = t.Substring(1); }
            else if (t.StartsWith("+")) { t = t.Substring(1); }

            if (string.IsNullOrEmpty(t))
                throw new FormatException(
                    $"Слагаемое «{tok}» — только знак без числа или x.");

            int xPos = t.IndexOf('x');
            if (xPos < 0)
            {
                if (!long.TryParse(t, out long c))
                    throw new FormatException(
                        $"«{tok}» не понятно. Число пиши цифрами, " +
                        "переменную буквой x, степень через ^: например x^2.");
                return new PolyTerm(sign * c, 0);
            }

            string coeffStr = t.Substring(0, xPos);
            string afterX   = t.Substring(xPos + 1); // всё после 'x'

            // Степень: ожидаем "^N" или пусто (степень 1)
            string degStr;
            if (afterX.StartsWith("^"))
            {
                degStr = afterX.Substring(1); // убираем '^'
            }
            else if (string.IsNullOrEmpty(afterX))
            {
                degStr = ""; // просто x, степень 1
            }
            else
            {
                throw new FormatException(
                    $"После x в «{tok}» должен быть ^ и показатель. " +
                    $"Напиши x^{afterX} вместо x{afterX}.");
            }

            long coeff;
            if (string.IsNullOrEmpty(coeffStr))      coeff = 1;
            else if (!long.TryParse(coeffStr, out coeff))
                throw new FormatException(
                    $"«{coeffStr}» — не могу прочитать коэффициент перед x в «{tok}».");

            int degree;
            if (string.IsNullOrEmpty(degStr))          degree = 1;
            else if (!int.TryParse(degStr, out degree) || degree < 0)
                throw new FormatException(
                    $"«{degStr}» — не могу прочитать степень в «{tok}». " +
                    "Степень — целое число ≥ 0, пиши через ^: x^3.");

            return new PolyTerm(sign * coeff, degree);
        }

        /// <summary>Список слагаемых → строка многочлена.</summary>
        public static string Format(IEnumerable<PolyTerm> terms)
        {
            var list = terms.Where(t => t.Coeff != 0)
                            .OrderByDescending(t => t.Degree)
                            .ToList();
            if (list.Count == 0) return "0";
            var sb = new StringBuilder(list[0].ToStringFirst());
            for (int i = 1; i < list.Count; i++)
                sb.Append(list[i].ToStringNext());
            return sb.ToString();
        }

        /// <summary>Суммировать подобные (одинаковые степени).</summary>
        public static List<PolyTerm> Reduce(List<PolyTerm> terms)
        {
            return terms
                .GroupBy(t => t.Degree)
                .Select(g => new PolyTerm(g.Sum(t => t.Coeff), g.Key))
                .Where(t => t.Coeff != 0)
                .OrderByDescending(t => t.Degree)
                .ToList();
        }

        /// <summary>Степень многочлена — максимальный показатель с ненулевым коэффициентом.</summary>
        public static int PolynomialDegree(List<PolyTerm> terms)
        {
            var nz = terms.Where(t => t.Coeff != 0).ToList();
            return nz.Any() ? nz.Max(t => t.Degree) : 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Валидаторы для шагов ввода
    // ═══════════════════════════════════════════════════════════════
    internal static class PolyValidate
    {
        public static string? CheckPoly(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\n" +
                       "Напиши многочлен, например: 3x^2 - 5x + 2";
            try
            {
                PolyParser.Parse(s);
                return null;
            }
            catch (FormatException ex)
            {
                return $"Не получается разобрать: {ex.Message}\n\n" +
                       "Правила записи:\n" +
                       "  · x² пиши как x^2, x³ как x^3\n" +
                       "  · коэффициент пиши перед x: -3x^2\n" +
                       "  · слагаемые разделяй + или -\n" +
                       "  · пробелы не важны";
            }
        }

        public static string? CheckNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл. Введи число, например: 2 или -3";
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                return null;
            return $"«{s}» — это не число 🤔\n" +
                   "Введи одно число, например: 2 или -1 или 0.5";
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 1: Степень многочлена  (§11.7–11.9)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialDegreeFunction : FunctionBase
    {
        public override string   Name       => "Степень многочлена";
        public override string   Formula    => "степень = наибольший показатель x после приведения подобных";
        public override string[] Keywords   => new[] { "степень", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое степень многочлена?\n\n" +
                    "Степень многочлена — это наибольший показатель степени x " +
                    "среди всех слагаемых.\n\n" +
                    "Важный момент: сначала нужно привести подобные слагаемые — " +
                    "вдруг некоторые сократятся. Например, если у тебя 3x³ и -3x³, " +
                    "они дадут 0, и степень окажется ниже.\n\n" +
                    "Разберём на примере: 4x^3 − 2x + 7\n" +
                    "  · слагаемое 4x^3 — степень 3\n" +
                    "  · слагаемое -2x — степень 1\n" +
                    "  · слагаемое 7 — степень 0 (свободный член)\n" +
                    "  Наибольшая степень = 3. Это многочлен 3-й степени.\n\n" +
                    "Как записывать:\n" +
                    "  · x² пиши как x^2, x³ пиши как x^3\n" +
                    "  · коэффициент перед x: 4x^3 = 4x³\n" +
                    "  · слагаемые разделяй + или -\n\n" +
                    "✏️ Введи свой многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);
            int deg     = PolyParser.PolynomialDegree(reduced);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();

            // Проверяем, были ли подобные
            bool hadLike = terms.GroupBy(t => t.Degree).Any(g => g.Count() > 1);

            if (hadLike)
            {
                sb.AppendLine("Шаг 1. Приводим подобные слагаемые:");
                foreach (var g in terms.GroupBy(t => t.Degree).OrderByDescending(g => g.Key))
                {
                    if (g.Count() <= 1) continue;
                    long sum     = g.Sum(t => t.Coeff);
                    string xLbl  = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "слагаемые с x"
                                 : $"слагаемые с x{PolyTerm.Sup(g.Key)}";
                    string parts = string.Join(" + ",
                        g.Select(t => t.Coeff.ToString()))
                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {xLbl}: {parts} = {sum}");
                }
                sb.AppendLine($"  После приведения: {PolyParser.Format(reduced)}");
                sb.AppendLine();
                sb.AppendLine("Шаг 2. Смотрим на показатели степени x:");
            }
            else
            {
                sb.AppendLine("Подобных слагаемых нет — переходим сразу к анализу:");
            }

            foreach (var t in reduced.OrderByDescending(t => t.Degree))
            {
                if (t.Coeff == 0) continue;
                string label  = t.Degree == 0 ? "свободный член, степень 0"
                              : $"степень {t.Degree}";
                string marker = t.Degree == deg ? "  ◀ наибольшая" : "";
                sb.AppendLine($"  {t.ToStringFirst()}  →  {label}{marker}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Степень многочлена = {deg}");
            string name = deg switch
            {
                1 => "(линейный многочлен)",
                2 => "(квадратный трёхчлен)",
                3 => "(многочлен третьей степени)",
                _ => $"(многочлен {deg}-й степени)"
            };
            if (deg >= 1) sb.AppendLine(name);

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 2: Привести подобные слагаемые  (§11.3–11.6)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialLikeTermsFunction : FunctionBase
    {
        public override string   Name       => "Привести подобные слагаемые";
        public override string   Formula    => "aₙxⁿ + bₙxⁿ = (aₙ + bₙ)·xⁿ";
        public override string[] Keywords   => new[] { "подобные", "слагаемые", "привести" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое подобные слагаемые и как их приводить?\n\n" +
                    "Подобные слагаемые — это слагаемые с одинаковой степенью x.\n" +
                    "При сложении таких слагаемых складываются только коэффициенты, " +
                    "а x со степенью остаётся прежним.\n\n" +
                    "Почему это работает: 5x² + 3x² — это как 5 яблок и 3 яблока. " +
                    "Яблоки одинаковые, значит можно просто сложить: 5 + 3 = 8. " +
                    "Получаем 8x².\n\n" +
                    "Пример: 5x^2 − 3x^2 + 2x + x − 7\n" +
                    "  · 5x^2 и -3x^2 — одна степень (x²): 5 + (-3) = 2, итог 2x²\n" +
                    "  · 2x и x — одна степень (x): 2 + 1 = 3, итог 3x\n" +
                    "  · -7 — свободный член, один, остаётся -7\n" +
                    "  Результат: 2x^2 + 3x − 7\n\n" +
                    "Как записывать:\n" +
                    "  · x² пиши как x^2, x³ пиши как x^3\n" +
                    "  · слагаемые разделяй + или -\n\n" +
                    "✏️ Введи свой многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var groups  = terms.GroupBy(t => t.Degree)
                               .OrderByDescending(g => g.Key)
                               .ToList();
            bool anyLike = groups.Any(g => g.Count() > 1);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Исходный многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();

            if (!anyLike)
            {
                sb.AppendLine("Подобных слагаемых нет — многочлен уже приведён.");
                sb.AppendLine($"Результат: {PolyParser.Format(PolyParser.Reduce(terms))}");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("Разбираем по шагам:\n");

            var result  = new List<PolyTerm>();
            int stepNum = 1;

            foreach (var g in groups)
            {
                var   gList  = g.ToList();
                long  sum    = gList.Sum(t => t.Coeff);
                string xLabel = g.Key == 0 ? "свободный член"
                              : g.Key == 1 ? "слагаемые с x"
                              : $"слагаемые с x{PolyTerm.Sup(g.Key)}";

                if (gList.Count > 1)
                {
                    // Показываем цепочку коэффициентов
                    string coeffChain = string.Join(" + ",
                        gList.Select(t => t.Coeff.ToString()))
                        .Replace("+ -", "− ");

                    string xPart = g.Key == 0 ? ""
                                 : g.Key == 1 ? "x"
                                 : "x" + PolyTerm.Sup(g.Key);
                    string sumTerm = sum == 0 ? "0"
                                   : (Math.Abs(sum) == 1 && g.Key != 0)
                                       ? (sum < 0 ? "-" : "") + xPart
                                       : sum + xPart;

                    sb.AppendLine($"Шаг {stepNum}. {xLabel}:");
                    sb.AppendLine($"  Складываем коэффициенты: {coeffChain} = {sum}");
                    sb.AppendLine($"  Результат слагаемого: {sumTerm}");

                    if (sum == 0)
                        sb.AppendLine("  (ноль — слагаемое исчезает из многочлена)");

                    sb.AppendLine();
                    if (sum != 0) result.Add(new PolyTerm(sum, g.Key));
                }
                else
                {
                    sb.AppendLine($"Шаг {stepNum}. {xLabel}:");
                    sb.AppendLine($"  Одно слагаемое — остаётся: {gList[0].ToStringFirst()}");
                    sb.AppendLine();
                    result.Add(gList[0]);
                }

                stepNum++;
            }

            sb.AppendLine($"📌 Результат: {PolyParser.Format(result)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 3: Значение многочлена при x = ...  (§11.10–11.12)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialValueFunction : FunctionBase
    {
        public override string   Name       => "Значение многочлена при x = ...";
        public override string   Formula    => "P(a): подставить x = a и вычислить";
        public override string[] Keywords   => new[] { "значение", "многочлен", "подстановка" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как найти значение многочлена?\n\n" +
                    "Нужно подставить конкретное число вместо x и посчитать по порядку действий: " +
                    "сначала степени, потом умножения, потом сложения и вычитания.\n\n" +
                    "Пример: P(x) = 2x^2 − 3x + 1, найти P(2), то есть при x = 2\n" +
                    "  Подставляем x = 2:\n" +
                    "  · 2x^2 = 2·(2²) = 2·4 = 8\n" +
                    "  · -3x = -3·2 = -6\n" +
                    "  · свободный член: +1\n" +
                    "  Складываем: 8 + (-6) + 1 = 3\n" +
                    "  Ответ: P(2) = 3\n\n" +
                    "Как записывать многочлен:\n" +
                    "  · x² пиши как x^2, x³ пиши как x^3\n" +
                    "  · коэффициент пиши перед x: 2x^2 = 2x²\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "✏️ При каком значении x считаем?\n\n" +
                    "Введи одно число — целое или дробное.\n" +
                    "Дробь пиши через точку или запятую, отрицательное — со знаком минус.\n" +
                    "Например: 2 или -3 или 0.5",
                Validate = PolyValidate.CheckNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);

            double x = double.Parse(
                answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);

            string xStr    = FmtNum(x);
            string polyStr = PolyParser.Format(reduced);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ P(x) = {polyStr}");
            sb.AppendLine($"Найти P({xStr}) — подставляем x = {xStr}");
            sb.AppendLine();

            double total  = 0;
            int    stepN  = 1;
            var    parts  = new List<string>();

            foreach (var t in reduced.OrderByDescending(t => t.Degree))
            {
                if (t.Coeff == 0) continue;

                double termVal;

                if (t.Degree == 0)
                {
                    termVal = t.Coeff;
                    sb.AppendLine($"Шаг {stepN}. Свободный член:");
                    sb.AppendLine($"  {t.Coeff} → {FmtNum(termVal)}");
                }
                else if (t.Degree == 1)
                {
                    termVal = t.Coeff * x;
                    sb.AppendLine($"Шаг {stepN}. Слагаемое {t.ToStringFirst()}:");
                    sb.AppendLine($"  {t.Coeff} · {xStr} = {FmtNum(termVal)}");
                }
                else
                {
                    double xPow = Math.Pow(x, t.Degree);
                    termVal = t.Coeff * xPow;
                    sb.AppendLine($"Шаг {stepN}. Слагаемое {t.ToStringFirst()}:");
                    sb.AppendLine($"  Сначала степень: {xStr}{PolyTerm.Sup(t.Degree)} = {FmtNum(xPow)}");
                    sb.AppendLine($"  Затем умножение: {t.Coeff} · {FmtNum(xPow)} = {FmtNum(termVal)}");
                }

                sb.AppendLine();
                total += termVal;
                parts.Add(FmtNum(termVal));
                stepN++;
            }

            // Итоговое сложение (если больше одного слагаемого)
            if (parts.Count > 1)
            {
                string sumLine = string.Join(" + ", parts).Replace("+ -", "− ");
                sb.AppendLine($"Складываем все слагаемые:");
                sb.AppendLine($"  {sumLine} = {FmtNum(total)}");
                sb.AppendLine();
            }

            sb.AppendLine($"📌 P({xStr}) = {FmtNum(total)}");
            return sb.ToString().TrimEnd();
        }

        private static string FmtNum(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e15)
                return ((long)v).ToString();
            return v.ToString("G10",
                System.Globalization.CultureInfo.InvariantCulture)
                .TrimEnd('0').TrimEnd('.');
        }
    }
}

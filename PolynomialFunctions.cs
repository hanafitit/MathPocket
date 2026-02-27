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

    // ═══════════════════════════════════════════════════════════════
    //  §11.1–11.3: Составление многочлена из одночленов
    //  Пользователь вводит одночлены через запятую,
    //  бот записывает многочлен и называет его члены.
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialFromMonomialsFunction : FunctionBase
    {
        public override string   Name       => "Составить многочлен из одночленов";
        public override string   Formula    => "a₁ + a₂ + ... + aₙ";
        public override string[] Keywords   => new[] { "составить", "многочлен", "одночлены" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Что такое многочлен?\n\n" +
                    "Многочлен — это сумма одночленов. Каждый одночлен называется членом многочлена.\n\n" +
                    "Примеры многочленов:\n" +
                    "  · 3x² + 5x − 7   (три члена — трёхчлен)\n" +
                    "  · a² + b²         (два члена — двучлен)\n" +
                    "  · -4xy + 2x − y + 1  (четыре члена)\n\n" +
                    "Как записывать одночлены:\n" +
                    "  · Только с переменной x: 3x^2, -5x, 7\n" +
                    "  · Числовой одночлен (без x): просто число\n\n" +
                    "✏️ Введи одночлены через запятую.\n" +
                    "Например: 3x^2, -5x, 7",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return "Ты ничего не ввёл. Введи одночлены через запятую, например: 3x^2, -5x, 7";
                    var parts = s.Split(',');
                    if (parts.Length < 2)
                        return "Нужно минимум 2 одночлена, разделённых запятой.\nНапример: 3x^2, -5x, 7";
                    foreach (var p in parts)
                    {
                        try { PolyParser.Parse(p.Trim()); }
                        catch (FormatException ex)
                        {
                            return $"Не могу разобрать «{p.Trim()}»: {ex.Message}\n\n" +
                                   "Записывай степени через ^: x² → x^2, x³ → x^3";
                        }
                    }
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var parts   = answers[0].Split(',');
            var allTerms = new List<PolyTerm>();
            var monomials = new List<string>();

            foreach (var p in parts)
            {
                var trimmed = p.Trim();
                monomials.Add(trimmed);
                allTerms.AddRange(PolyParser.Parse(trimmed));
            }

            var sb = new StringBuilder();
            sb.AppendLine("✅ Многочлен составлен:");
            sb.AppendLine();

            // Собираем без приведения — просто запись суммы
            string poly = BuildSum(allTerms);
            sb.AppendLine($"  {poly}");
            sb.AppendLine();
            sb.AppendLine($"Количество членов: {parts.Length}");
            sb.AppendLine();
            sb.AppendLine("Члены многочлена:");
            for (int i = 0; i < monomials.Count; i++)
                sb.AppendLine($"  {i + 1}) {monomials[i]}");

            // Степень
            var reduced = PolyParser.Reduce(allTerms);
            int deg = PolyParser.PolynomialDegree(reduced);
            sb.AppendLine();
            sb.AppendLine($"📌 Степень многочлена = {deg}");

            return sb.ToString().TrimEnd();
        }

        private static string BuildSum(List<PolyTerm> terms)
        {
            if (!terms.Any()) return "0";
            var sb = new StringBuilder(terms[0].ToStringFirst());
            for (int i = 1; i < terms.Count; i++)
            {
                var t = terms[i];
                if (t.Coeff == 0) continue;
                sb.Append(t.Coeff < 0 ? " - " : " + ");
                sb.Append(new PolyTerm(Math.Abs(t.Coeff), t.Degree).ToStringFirst());
            }
            return sb.ToString();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.2: Привести каждый член многочлена к стандартному виду
    //  (многочлен уже есть, пользователь вводит его целиком)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialStandardMembersFunction : FunctionBase
    {
        public override string   Name       => "Стандартный вид каждого члена";
        public override string   Formula    => "Каждый член → стандартный вид, затем степень многочлена";
        public override string[] Keywords   => new[] { "стандартный", "вид", "член", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Стандартный вид члена многочлена\n\n" +
                    "Каждый член многочлена — это одночлен. Его стандартный вид:\n" +
                    "  число (коэффициент) · переменные в степенях\n\n" +
                    "Примеры:\n" +
                    "  · 3x² — уже в стандартном виде\n" +
                    "  · -5x³ — уже в стандартном виде\n" +
                    "  · 7 — числовой член, степень 0\n\n" +
                    "После приведения каждого члена к стандартному виду\n" +
                    "определяем степень многочлена — наибольший показатель.\n\n" +
                    "Как записывать:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · члены разделяй знаками + или −\n\n" +
                    "✏️ Введи многочлен:",
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
            sb.AppendLine("Члены в стандартном виде:");

            int stepN = 1;
            foreach (var t in terms)
            {
                if (t.Coeff == 0) continue;
                string xPart  = t.Degree == 0 ? "" : t.Degree == 1 ? "x" : "x" + PolyTerm.Sup(t.Degree);
                string coeff  = t.Degree > 0 && Math.Abs(t.Coeff) == 1 ? "" : Math.Abs(t.Coeff).ToString();
                string sign   = t.Coeff < 0 ? "-" : "";
                string std    = t.Degree == 0 ? t.Coeff.ToString() : $"{sign}{coeff}{xPart}";
                sb.AppendLine($"  {stepN}) {t.ToStringFirst()}  →  степень {t.Degree}");
                stepN++;
            }

            sb.AppendLine();
            sb.AppendLine($"Наибольшая степень среди всех членов = {deg}");
            sb.AppendLine();
            sb.AppendLine($"📌 Степень многочлена = {deg}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.3: Назвать члены многочлена (свободный, линейный и т.д.)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialNameMembersFunction : FunctionBase
    {
        public override string   Name       => "Назвать члены многочлена";
        public override string   Formula    => "Определить тип и степень каждого члена";
        public override string[] Keywords   => new[] { "назвать", "члены", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как называть члены многочлена?\n\n" +
                    "Каждый член многочлена — одночлен. По степени они называются:\n" +
                    "  · степень 0 → свободный член (число без x)\n" +
                    "  · степень 1 → линейный член (содержит x)\n" +
                    "  · степень 2 → квадратный член (содержит x²)\n" +
                    "  · степень 3 → кубический член (содержит x³)\n" +
                    "  · степень n → член n-й степени\n\n" +
                    "Пример: 5x³ − 6x² + 0,8y³\n" +
                    "  · 5x³  → член 3-й степени (кубический)\n" +
                    "  · -6x² → член 2-й степени (квадратный)\n" +
                    "  · 0,8y³ → член 3-й степени\n\n" +
                    "Как записывать:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · члены разделяй + или −\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms = PolyParser.Parse(answers[0]);
            var sb    = new StringBuilder();

            sb.AppendLine($"✅ Многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();
            sb.AppendLine("Разбираем каждый член:");

            int n = 1;
            foreach (var t in terms)
            {
                if (t.Coeff == 0) continue;
                string typeName = t.Degree switch
                {
                    0 => "свободный член",
                    1 => "линейный член (степень 1)",
                    2 => "квадратный член (степень 2)",
                    3 => "кубический член (степень 3)",
                    _ => $"член {t.Degree}-й степени"
                };
                sb.AppendLine($"  {n}) {t.ToStringFirst()}  →  {typeName}");
                n++;
            }

            var reduced = PolyParser.Reduce(terms);
            int deg = PolyParser.PolynomialDegree(reduced);
            sb.AppendLine();
            sb.AppendLine($"📌 Степень многочлена = {deg}");

            string polyName = (terms.Where(t => t.Coeff != 0).Count()) switch
            {
                1 => "одночлен",
                2 => "двучлен",
                3 => "трёхчлен",
                _ => $"многочлен ({terms.Where(t => t.Coeff != 0).Count()} членов)"
            };
            sb.AppendLine($"    Многочлен называется: {polyName}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.4–11.6: Сложение многочленов
    //  (приведение подобных при сложении двух многочленов)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialAddFunction : FunctionBase
    {
        public override string   Name       => "Сложение многочленов";
        public override string   Formula    => "(A) + (B) = приведение подобных";
        public override string[] Keywords   => new[] { "сложение", "многочлен", "сумма" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сложение многочленов\n\n" +
                    "Чтобы сложить многочлены, нужно раскрыть скобки и привести подобные члены.\n\n" +
                    "Пример: (3a + 2b − 1) + (−a + 5b + 4)\n" +
                    "  · Раскрываем скобки: 3a + 2b − 1 − a + 5b + 4\n" +
                    "  · Приводим подобные:\n" +
                    "      a: 3a − a = 2a\n" +
                    "      b: 2b + 5b = 7b\n" +
                    "      числа: −1 + 4 = 3\n" +
                    "  · Результат: 2a + 7b + 3\n\n" +
                    "Как записывать:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · члены через + или −\n\n" +
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
            var t1 = PolyParser.Parse(answers[0]);
            var t2 = PolyParser.Parse(answers[1]);

            var all     = t1.Concat(t2).ToList();
            var reduced = PolyParser.Reduce(all);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(t1)}) + ({PolyParser.Format(t2)})");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем скобки — знаки не меняются:");
            sb.AppendLine($"  {PolyParser.Format(all)}");
            sb.AppendLine();

            var groups = all.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine("Шаг 2. Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string label = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {label}: {chain} = {g.Sum(t => t.Coeff)}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.4–11.6: Вычитание многочленов
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialSubtractFunction : FunctionBase
    {
        public override string   Name       => "Вычитание многочленов";
        public override string   Formula    => "(A) − (B) = знаки B меняются";
        public override string[] Keywords   => new[] { "вычитание", "многочлен", "разность" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вычитание многочленов\n\n" +
                    "При вычитании многочлена меняем знаки всех его членов на противоположные.\n\n" +
                    "Пример: (5x² − 3x + 1) − (2x² + x − 4)\n" +
                    "  · Меняем знаки второго: 5x² − 3x + 1 − 2x² − x + 4\n" +
                    "  · Приводим подобные:\n" +
                    "      x²: 5x² − 2x² = 3x²\n" +
                    "      x:  −3x − x   = −4x\n" +
                    "      числа: 1 + 4  = 5\n" +
                    "  · Результат: 3x² − 4x + 5\n\n" +
                    "✏️ Введи первый многочлен (уменьшаемое):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второй многочлен (вычитаемое):",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var t1 = PolyParser.Parse(answers[0]);
            var t2 = PolyParser.Parse(answers[1]);

            // Меняем знаки второго
            var t2neg   = t2.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();
            var all     = t1.Concat(t2neg).ToList();
            var reduced = PolyParser.Reduce(all);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(t1)}) − ({PolyParser.Format(t2)})");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем скобки — меняем знаки второго многочлена:");
            sb.AppendLine($"  {PolyParser.Format(all)}");
            sb.AppendLine();

            var groups = all.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
            if (groups.Any())
            {
                sb.AppendLine("Шаг 2. Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string label = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {label}: {chain} = {g.Sum(t => t.Coeff)}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.7–11.9: Представить в стандартном виде и назвать степень
    //  (многочлен может иметь подобные — нужно привести и назвать)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialStandardFormFunction : FunctionBase
    {
        public override string   Name       => "Стандартный вид многочлена";
        public override string   Formula    => "Привести подобные → записать по убыванию степеней";
        public override string[] Keywords   => new[] { "стандартный", "вид", "многочлен", "степень" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Стандартный вид многочлена\n\n" +
                    "Многочлен в стандартном виде записывается так:\n" +
                    "  1. Подобные члены приведены (сложены)\n" +
                    "  2. Члены расставлены по убыванию степеней x\n\n" +
                    "Пример: 3 − 5x² + 2x + x²\n" +
                    "  · Приводим подобные: x²: −5x² + x² = −4x²\n" +
                    "  · Расставляем по убыванию: −4x² + 2x + 3\n" +
                    "  · Степень = 2\n\n" +
                    "Как записывать:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · члены через + или −\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);
            int deg     = PolyParser.PolynomialDegree(reduced);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Исходный многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();

            bool hadLike = terms.GroupBy(t => t.Degree).Any(g => g.Count() > 1);
            if (hadLike)
            {
                sb.AppendLine("Шаг 1. Приводим подобные члены:");
                foreach (var g in terms.GroupBy(t => t.Degree)
                                       .Where(g => g.Count() > 1)
                                       .OrderByDescending(g => g.Key))
                {
                    string label = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {label}: {chain} = {g.Sum(t => t.Coeff)}");
                }
                sb.AppendLine();
                sb.AppendLine("Шаг 2. Расставляем по убыванию степеней:");
            }
            else
            {
                sb.AppendLine("Подобных нет. Расставляем по убыванию степеней:");
            }

            sb.AppendLine($"  {PolyParser.Format(reduced)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Стандартный вид: {PolyParser.Format(reduced)}");
            sb.AppendLine($"   Степень многочлена = {deg}");

            string name = deg switch
            {
                1 => "(линейный многочлен)",
                2 => "(квадратный трёхчлен)",
                3 => "(многочлен третьей степени)",
                _ => $"(многочлен {deg}-й степени)"
            };
            if (deg >= 1) sb.AppendLine($"   {name}");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §11.10–11.12: Значение многочлена при двух значениях переменных
    //  (расширение уже существующей PolynomialValueFunction)
    //  Многочлен от двух переменных: подстановка a и b
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialValueTwoVarsFunction : FunctionBase
    {
        public override string   Name       => "Значение многочлена (два числа)";
        public override string   Formula    => "Подставить a и b, вычислить";
        public override string[] Keywords   => new[] { "значение", "многочлен", "два", "числа" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Значение многочлена при заданных значениях переменных\n\n" +
                    "Нужно подставить числа вместо переменных и вычислить.\n\n" +
                    "Пример: многочлен 0,7ab − 49 + a − 1,2ab + 47\n" +
                    "  При a = 2/3, b = 9/16:\n" +
                    "  · Сначала приводим подобные: 0,7ab − 1,2ab = −0,5ab\n" +
                    "  · Получаем: −0,5ab + a − 2\n" +
                    "  · Подставляем a и b, вычисляем\n\n" +
                    "✏️ Введи многочлен от одной переменной x:\n" +
                    "(многочлены от нескольких переменных — в следующей версии)\n\n" +
                    "Например: 5x^3 - 8x^2 + 44 - 10x^2 + 7x^3",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "✏️ Введи первое значение x:\n\n" +
                    "Например: -2 или 3 или 0.5",
                Validate = PolyValidate.CheckNumber
            },
            new InputStep
            {
                Question =
                    "✏️ Введи второе значение x\n" +
                    "(бот посчитает для обоих значений сразу):\n\n" +
                    "Например: 3 или -1",
                Validate = PolyValidate.CheckNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);

            double x1 = double.Parse(answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);
            double x2 = double.Parse(answers[2].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ P(x) = {PolyParser.Format(reduced)}");
            sb.AppendLine();

            // Покажем приведение если нужно
            bool hadLike = terms.GroupBy(t => t.Degree).Any(g => g.Count() > 1);
            if (hadLike)
            {
                sb.AppendLine("Шаг 1. Приводим подобные:");
                sb.AppendLine($"  {PolyParser.Format(reduced)}");
                sb.AppendLine();
            }

            foreach (double xVal in new[] { x1, x2 })
            {
                string xStr  = FmtNum(xVal);
                double total = reduced.Sum(t => t.Coeff * Math.Pow(xVal, t.Degree));

                sb.AppendLine($"При x = {xStr}:");
                foreach (var t in reduced.OrderByDescending(t => t.Degree))
                {
                    if (t.Coeff == 0) continue;
                    double tVal = t.Coeff * Math.Pow(xVal, t.Degree);
                    string tStr = t.Degree == 0 ? $"{t.Coeff}"
                                : t.Degree == 1 ? $"{t.Coeff} · {xStr} = {FmtNum(tVal)}"
                                : $"{t.Coeff} · {xStr}{PolyTerm.Sup(t.Degree)} = {FmtNum(tVal)}";
                    sb.AppendLine($"  {t.ToStringFirst()}: {tStr}");
                }
                sb.AppendLine($"  📌 P({xStr}) = {FmtNum(total)}");
                sb.AppendLine();
            }

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


// ═══════════════════════════════════════════════════════════════
//  §15.1–15.7  Вынесение общего множителя за скобки
// ═══════════════════════════════════════════════════════════════
    public class FactorOutGcfFunction : FunctionBase
    {
        public override string   Name       => "Вынести общий множитель";
        public override string   Formula    => "ax + ay = a(x + y)";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "вынести", "общий множитель", "скобки", "разложить" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вынесение общего множителя за скобки\n\n" +
                    "Нужно найти НОД коэффициентов и наименьшие степени переменных.\n\n" +
                    "Пример: 15ab − 8ac + 7/2·ad\n" +
                    "  · Общий множитель по буквам: a (есть в каждом члене)\n" +
                    "  · НОД коэффициентов 15, 8, 7/2 → можно вынести a\n" +
                    "  · Ответ: a(15b − 8c + 7/2·d)\n\n" +
                    "Введи многочлен (используй x^2 для степеней):\n" +
                    "✏️ Например: 15x^1 - 8x^1 + 7x^1 или 14x^3 - 49x^2 - 35x^2",
                Validate = s => {
                    try { PolyParser.Parse(s); return null; }
                    catch { return $"Не могу разобрать «{s}». Пример: 6x^2 - 9x^1 + 3"; }
                }
            },
            new InputStep
            {
                Question = "✏️ Введи общий множитель (коэффициент и/или степень x), например: 3x^1 или 7x^2 или 5:",
                Validate = s => {
                    try { PolyParser.Parse(s); return null; }
                    catch { return $"Не могу разобрать «{s}». Введи одночлен, например: 3x^1 или 5"; }
                }
            }
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 1) return $"🔍 Многочлен: {answers[0]}";
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly   = PolyParser.Parse(answers[0]);
            var factor = PolyParser.Parse(answers[1]);
            var f = factor[0]; // общий множитель — один член

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📌 Разложить: {answers[0]}");
            sb.AppendLine($"   Общий множитель: {answers[1]}");
            sb.AppendLine();

            // Делим каждый член на множитель
            var quotient = new List<PolyTerm>();
            bool ok = true;
            foreach (var t in poly)
            {
                if (f.Coeff == 0) { ok = false; break; }
                long q = t.Coeff / f.Coeff;
                int  d = t.Degree - f.Degree;
                if (t.Coeff % f.Coeff != 0 || d < 0) { ok = false; break; }
                quotient.Add(new PolyTerm(q, d));
            }

            if (!ok)
            {
                sb.AppendLine("⚠️ Указанный множитель не делит все члены нацело.");
                sb.AppendLine("Проверь правильность общего множителя.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("Делим каждый член на общий множитель:");
            foreach (var (t, q) in poly.Zip(quotient, (a,b) => (a,b)))
                sb.AppendLine($"  {t.ToStringFirst()} ÷ {f.ToStringFirst()} = {q.ToStringFirst()}");

            sb.AppendLine();
            sb.AppendLine($"✅ Ответ: {answers[1]}·({PolyParser.Format(quotient)})");

            // Проверка
            var check = new List<PolyTerm>();
            foreach (var q in quotient)
                check.Add(new PolyTerm(f.Coeff * q.Coeff, f.Degree + q.Degree));
            var checkReduced = PolyParser.Reduce(check);
            var original = PolyParser.Reduce(poly);
            if (PolyParser.Format(checkReduced) == PolyParser.Format(original))
                sb.AppendLine("✓ Проверка: раскрываем скобки — совпадает с исходным.");
            else
                sb.AppendLine($"⚠️ Проверка: получилось {PolyParser.Format(checkReduced)} — не совпадает. Проверь множитель.");

            return sb.ToString().TrimEnd();
        }
    }

// ═══════════════════════════════════════════════════════════════
//  §15.5–15.6  Вынесение многочлена как общего множителя
// ═══════════════════════════════════════════════════════════════
    public class FactorOutPolyFunction : FunctionBase
    {
        public override string   Name       => "Общий множитель — многочлен";
        public override string   Formula    => "a(m+n) + b(m+n) = (m+n)(a+b)";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "общий множитель многочлен", "вынести многочлен" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Общий множитель — многочлен\n\n" +
                    "Иногда общим множителем является не одночлен, а многочлен.\n\n" +
                    "Пример: 12(a+b) − 3(a+b) → общий множитель (a+b)\n" +
                    "  = (a+b)(12 − 3) = (a+b)·9\n\n" +
                    "Пример: 11(x−1) − x(x−1) + y(1−x)\n" +
                    "  Замена: (1−x) = −(x−1)\n" +
                    "  = (x−1)(11 − x − y)\n\n" +
                    "✏️ Введи первый коэффициент (число перед скобкой), например: 12 или -1:",
                Validate = s => {
                    if (double.TryParse(s.Replace(',','.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
                    return $"«{s}» — не число";
                }
            },
            new InputStep
            {
                Question = "✏️ Введи второй коэффициент:",
                Validate = s => {
                    if (double.TryParse(s.Replace(',','.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
                    return $"«{s}» — не число";
                }
            },
            new InputStep
            {
                Question = "Ещё один коэффициент? Введи число или «нет»:",
                Validate = s => {
                    if (s.ToLower() == "нет" || s.ToLower() == "no") return null;
                    if (double.TryParse(s.Replace(',','.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
                    return "Введи число или «нет»";
                }
            }
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 1) return $"🔍 Первый коэффициент: {answers[0]}";
            if (answers.Count == 2) return $"🔍 Коэффициенты: {answers[0]}, {answers[1]}";
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var coeffs = new List<double>();
            foreach (var a in answers)
            {
                if (a.ToLower() == "нет" || a.ToLower() == "no") break;
                coeffs.Add(double.Parse(a.Replace(',','.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture));
            }

            double sum = coeffs.Sum();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📌 Вынесение многочлена-множителя за скобки");
            sb.AppendLine($"   Коэффициенты: {string.Join(", ", coeffs.Select(c => FmtD(c)))}");
            sb.AppendLine();
            sb.AppendLine("Обозначим общий множитель как M. Тогда:");
            string terms = string.Join(" + ", coeffs.Select(c => $"{FmtD(c)}·M")).Replace("+ -", "- ");
            sb.AppendLine($"  {terms}");
            sb.AppendLine($"= M · ({string.Join(" + ", coeffs.Select(c => FmtD(c))).Replace("+ -", "- ")})");
            sb.AppendLine($"= M · {FmtD(sum)}");
            sb.AppendLine();
            sb.AppendLine($"✅ Ответ: M·({FmtD(sum)}), где M — общий многочлен-множитель");
            sb.AppendLine("\n💡 Подставь свой многочлен вместо M в ответ.");
            return sb.ToString().TrimEnd();
        }

        private static string FmtD(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15
                ? ((long)v).ToString()
                : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    }

// ═══════════════════════════════════════════════════════════════
//  §15.8–15.9  Решить уравнение через вынесение за скобки
// ═══════════════════════════════════════════════════════════════
    public class FactorEquationFunction : FunctionBase
    {
        public override string   Name       => "Уравнение через разложение";
        public override string   Formula    => "ax + bx² = 0 → x(a + bx) = 0";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "уравнение вынести", "разложить уравнение" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнения через вынесение общего множителя\n\n" +
                    "Пример: x² + 6x = 0\n" +
                    "  · Выносим общий множитель x: x(x + 6) = 0\n" +
                    "  · Произведение = 0 когда один из множителей = 0\n" +
                    "  · x = 0 или x + 6 = 0 → x = −6\n" +
                    "  · Ответ: {0; −6}\n\n" +
                    "✏️ Введи левую часть уравнения (правая = 0):\n" +
                    "Например: x^2 + 6x^1 или 7x^1 - 5x^2",
                Validate = s => {
                    try { PolyParser.Parse(s); return null; }
                    catch { return $"Не могу разобрать «{s}». Пример: x^2 + 6x^1"; }
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(poly);
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"📌 Уравнение: {answers[0]} = 0");
            sb.AppendLine();

            // Найдём наименьшую степень (общий множитель x^k)
            int minDeg = reduced.Where(t => t.Coeff != 0).Min(t => t.Degree);
            long gcd = reduced.Where(t => t.Coeff != 0)
                               .Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));

            var quotient = reduced
                .Select(t => new PolyTerm(t.Coeff / gcd, t.Degree - minDeg))
                .ToList();

            string factorStr = minDeg == 0
                ? gcd.ToString()
                : gcd == 1
                    ? (minDeg == 1 ? "x" : $"x{PolyTerm.Sup(minDeg)}")
                    : (minDeg == 1 ? $"{gcd}x" : $"{gcd}x{PolyTerm.Sup(minDeg)}");

            sb.AppendLine($"Шаг 1. Общий множитель: {factorStr}");
            sb.AppendLine($"Шаг 2. Выносим: {factorStr}·({PolyParser.Format(quotient)}) = 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3. Произведение = 0 когда один из множителей = 0:");

            var roots = new List<string>();

            // Корень от x^k = 0
            if (minDeg > 0)
            {
                roots.Add("0");
                sb.AppendLine($"  {factorStr} = 0 → x = 0");
            }

            // Корень от скобки (если линейная)
            var nonConst = quotient.Where(t => t.Degree > 0).ToList();
            var constTerm = quotient.FirstOrDefault(t => t.Degree == 0);

            if (nonConst.Count == 1 && nonConst[0].Degree == 1)
            {
                long a = nonConst[0].Coeff;
                long b = constTerm.Coeff;
                // ax + b = 0 → x = -b/a
                double root = -(double)b / a;
                roots.Add(FmtD(root));
                sb.AppendLine($"  {PolyParser.Format(quotient)} = 0 → {a}x = {-b} → x = {FmtD(root)}");
            }
            else if (nonConst.Count == 0)
            {
                // Только константа в скобке
                if (constTerm.Coeff != 0)
                    sb.AppendLine($"  ({PolyParser.Format(quotient)}) ≠ 0 — дополнительных корней нет");
            }
            else
            {
                sb.AppendLine($"  ({PolyParser.Format(quotient)}) = 0 — решается методами старших классов");
            }

            sb.AppendLine();
            sb.AppendLine($"✅ Ответ: {{{string.Join("; ", roots)}}}");
            return sb.ToString().TrimEnd();
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
        private static string FmtD(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15
                ? ((long)v).ToString()
                : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    }

    // ═══════════════════════════════════════════════════════════════
    //  §16.1–16.6  Группировка 4 члена (2+2)
    //
    //  Пример: x + xy + a + ay
    //    Группа 1: x + xy  = x(1 + y)
    //    Группа 2: a + ay  = a(1 + y)
    //    Итог: (1 + y)(x + a)
    // ═══════════════════════════════════════════════════════════════
    public class GroupingFourTermsFunction : FunctionBase
    {
        public override string   Name       => "Группировка: 4 члена (2+2)";
        public override string   Formula    => "ac + ad + bc + bd = a(c+d) + b(c+d) = (a+b)(c+d)";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "группировка", "разложение", "4 члена" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 §16. Разложение способом группировки\n\n" +
                    "Если не все члены имеют общий множитель — группируем их!\n\n" +
                    "Алгоритм:\n" +
                    "  1. Разбиваем члены на группы\n" +
                    "  2. В каждой группе выносим свой общий множитель\n" +
                    "  3. Если в скобках получилось одинаковое выражение — выносим его\n\n" +
                    "Пример: 20a − 4ab + 5c − bc\n" +
                    "  Группа 1: 20a − 4ab = 4a(5 − b)\n" +
                    "  Группа 2: 5c − bc   = c(5 − b)\n" +
                    "  Итог: (5 − b)(4a + c)\n\n" +
                    "✏️ Введи первый член многочлена (например: x или 20a или -4ab):\n" +
                    "Используй x^2 для степеней: 3x^2",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второй член:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи третий член:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи четвёртый член:",
                Validate = PolyValidate.CheckPoly
            },
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 2)
                return $"🔍 Группа 1: {answers[0]} + {answers[1]}";
            if (answers.Count == 4)
                return $"🔍 Группа 2: {answers[2]} + {answers[3]}";
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            // Парсим 4 члена
            var t1 = PolyParser.Parse(answers[0]);
            var t2 = PolyParser.Parse(answers[1]);
            var t3 = PolyParser.Parse(answers[2]);
            var t4 = PolyParser.Parse(answers[3]);

            string m1 = answers[0].Trim();
            string m2 = answers[1].Trim();
            string m3 = answers[2].Trim();
            string m4 = answers[3].Trim();

            // Знак для отображения
            string Sign(string s) => s.StartsWith("-") ? s : "+ " + s;

            var sb = new StringBuilder();
            sb.AppendLine($"📌 Многочлен: {m1} {Sign(m2)} {Sign(m3)} {Sign(m4)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Разбиваем на 2 группы:");
            sb.AppendLine($"  ({m1} {Sign(m2)}) + ({m3} {Sign(m4)})");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. В каждой группе ищем общий множитель:");
            sb.AppendLine($"  Группа 1: {m1} {Sign(m2)}");

            // НОД коэффициентов группы 1
            var g1 = t1.Concat(t2).ToList();
            var g2 = t3.Concat(t4).ToList();

            long gcd1 = g1.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));
            long gcd2 = g2.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));

            // Минимальная степень в группе
            int minDeg1 = g1.Where(t => t.Coeff != 0).Min(t => t.Degree);
            int minDeg2 = g2.Where(t => t.Coeff != 0).Min(t => t.Degree);

            if (gcd1 < 1) gcd1 = 1;
            if (gcd2 < 1) gcd2 = 1;

            // Частное для каждой группы
            var q1 = g1.Select(t => new PolyTerm(t.Coeff / gcd1, t.Degree - minDeg1)).ToList();
            var q2 = g2.Select(t => new PolyTerm(t.Coeff / gcd2, t.Degree - minDeg2)).ToList();

            string factor1 = FormatFactor(gcd1, minDeg1);
            string factor2 = FormatFactor(gcd2, minDeg2);
            string bracket1 = PolyParser.Format(q1);
            string bracket2 = PolyParser.Format(q2);

            sb.AppendLine($"    = {factor1}({bracket1})");
            sb.AppendLine($"  Группа 2: {m3} {Sign(m4)}");
            sb.AppendLine($"    = {factor2}({bracket2})");
            sb.AppendLine();

            // Проверяем совпадение скобок
            if (bracket1 == bracket2)
            {
                sb.AppendLine("Шаг 3. Скобки совпадают! Выносим общий множитель:");
                sb.AppendLine($"  = ({bracket1}) · ({factor1} + {factor2})");
                sb.AppendLine();

                // Проверка знаков — второй фактор
                string outer = FormatOuterSum(factor1, factor2);
                sb.AppendLine($"✅ Ответ: ({bracket1})({outer})");
            }
            else
            {
                sb.AppendLine("⚠️ Скобки не совпали — попробуй другую группировку.");
                sb.AppendLine($"  Группа 1 дала: ({bracket1})");
                sb.AppendLine($"  Группа 2 дала: ({bracket2})");
                sb.AppendLine();
                sb.AppendLine("💡 Подсказка: попробуй поменять порядок членов —");
                sb.AppendLine("  например (1,3) + (2,4) вместо (1,2) + (3,4).");
            }

            return sb.ToString().TrimEnd();
        }

        private static string FormatFactor(long coeff, int degree)
        {
            if (degree == 0) return coeff == 1 ? "1" : coeff.ToString();
            string x = degree == 1 ? "x" : "x" + PolyTerm.Sup(degree);
            return coeff == 1 ? x : coeff + x;
        }

        private static string FormatOuterSum(string a, string b)
        {
            // Просто соединяем два множителя знаком +
            return $"{a} + {b}";
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    }

    // ═══════════════════════════════════════════════════════════════
    //  §16.7  Группировка 6 членов (3+3)
    //
    //  Пример: mt − 4t + 5n − 5m − mn + 20
    //    Группа 1: mt + 5n − mn = m(t − n) + 5(n − m)  ...
    //    Разные варианты группировки.
    // ═══════════════════════════════════════════════════════════════
    public class GroupingSixTermsFunction : FunctionBase
    {
        public override string   Name       => "Группировка: 6 членов (3+3)";
        public override string   Formula    => "6 членов → две группы по 3 → выносим множители";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "группировка", "6 членов", "три группы" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 §16. Группировка шести членов (3+3)\n\n" +
                    "Когда многочлен из 6 членов — группируем по три.\n\n" +
                    "Пример: mt − 4t + 5n − 5m − mn + 20\n" +
                    "  Группируем по 3:\n" +
                    "  (mt − mn − 4t) + (5n − 5m + 20)\n" +
                    "  = m(t − n) − 4(t) + 5(n − m + 4)\n\n" +
                    "Или по-другому (2+2+2):\n" +
                    "  (mt − 4t) + (5n − mn) + (20 − 5m)\n" +
                    "  = t(m − 4) + n(5 − m) + 5(4 − m)\n" +
                    "  = t(m − 4) − n(m − 5) + 5(4 − m)\n\n" +
                    "✏️ Введи все 6 членов по одному.\n\nЧлен 1:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep { Question = "✏️ Член 2:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Член 3:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Член 4:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Член 5:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Член 6:", Validate = PolyValidate.CheckPoly },
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 3)
                return $"🔍 Группа 1: {string.Join(", ", answers)}";
            if (answers.Count == 6)
                return $"🔍 Группа 2: {string.Join(", ", answers.Skip(3))}";
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            string Sign(string s) => s.Trim().StartsWith("-") ? s.Trim() : "+ " + s.Trim();

            string all = $"{answers[0]} {Sign(answers[1])} {Sign(answers[2])} " +
                         $"{Sign(answers[3])} {Sign(answers[4])} {Sign(answers[5])}";

            var sb = new StringBuilder();
            sb.AppendLine($"📌 Многочлен: {all}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Группируем по 3 члена (3+3):");
            sb.AppendLine($"  ({answers[0]} {Sign(answers[1])} {Sign(answers[2])})");
            sb.AppendLine($"  + ({answers[3]} {Sign(answers[4])} {Sign(answers[5])})");
            sb.AppendLine();

            // Группа 1: первые 3
            var g1 = answers.Take(3).SelectMany(a => PolyParser.Parse(a)).ToList();
            long gcd1 = g1.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));
            int minD1 = g1.Where(t => t.Coeff != 0).Min(t => t.Degree);
            if (gcd1 < 1) gcd1 = 1;
            var q1 = g1.Select(t => new PolyTerm(t.Coeff / gcd1, t.Degree - minD1)).ToList();
            string f1 = FormatFactor(gcd1, minD1);
            string b1 = PolyParser.Format(q1);

            // Группа 2: последние 3
            var g2 = answers.Skip(3).SelectMany(a => PolyParser.Parse(a)).ToList();
            long gcd2 = g2.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));
            int minD2 = g2.Where(t => t.Coeff != 0).Min(t => t.Degree);
            if (gcd2 < 1) gcd2 = 1;
            var q2 = g2.Select(t => new PolyTerm(t.Coeff / gcd2, t.Degree - minD2)).ToList();
            string f2 = FormatFactor(gcd2, minD2);
            string b2 = PolyParser.Format(q2);

            sb.AppendLine("Шаг 2. Выносим общий множитель в каждой группе:");
            sb.AppendLine($"  Группа 1 → {f1}({b1})");
            sb.AppendLine($"  Группа 2 → {f2}({b2})");
            sb.AppendLine();

            if (b1 == b2)
            {
                sb.AppendLine("Шаг 3. Скобки совпадают — выносим общий множитель:");
                sb.AppendLine($"✅ Ответ: ({b1})({f1} + {f2})");
            }
            else
            {
                sb.AppendLine("⚠️ Скобки не совпали при группировке (3+3).");
                sb.AppendLine("💡 Подсказка: попробуй другой порядок членов или группировку (2+2+2).");
            }

            return sb.ToString().TrimEnd();
        }

        private static string FormatFactor(long coeff, int degree)
        {
            if (degree == 0) return coeff == 1 ? "1" : coeff.ToString();
            string x = degree == 1 ? "x" : "x" + PolyTerm.Sup(degree);
            return coeff == 1 ? x : coeff + x;
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    }

    // ═══════════════════════════════════════════════════════════════
    //  §16.8–16.9  Уравнение через группировку
    //
    //  Пользователь вводит многочлен (левая часть = 0).
    //  Бот пытается разложить через группировку и найти корни.
    //
    //  Пример: x² + 2x − 15 = 0
    //    Подбираем: x² + 5x − 3x − 15 = x(x+5) − 3(x+5) = (x+5)(x−3) = 0
    //    Корни: x = −5, x = 3
    // ═══════════════════════════════════════════════════════════════
    public class GroupingEquationFunction : FunctionBase
    {
        public override string   Name       => "Уравнение через группировку";
        public override string   Formula    => "многочлен = 0 → группировка → произведение = 0";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "уравнение", "группировка", "корни" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 §16. Решение уравнения через группировку\n\n" +
                    "Алгоритм:\n" +
                    "  1. Разложить левую часть на множители (группировкой)\n" +
                    "  2. Произведение = 0, если хотя бы один множитель = 0\n" +
                    "  3. Решить каждое уравнение отдельно\n\n" +
                    "Пример: y² + 2y − 63 = 0\n" +
                    "  y² + 2y − 63 = y² − 7y + 9y − 63\n" +
                    "  = y(y − 7) + 9(y − 7)\n" +
                    "  = (y − 7)(y + 9) = 0\n" +
                    "  y = 7  или  y = −9\n\n" +
                    "✏️ Введи левую часть уравнения (правая = 0):\n" +
                    "Например: x^2 + 5x + 6  или  x^2 - 5x + 4",
                Validate = s =>
                {
                    try
                    {
                        var t = PolyParser.Parse(s);
                        if (PolyParser.PolynomialDegree(PolyParser.Reduce(t)) < 2)
                            return "Введи многочлен степени ≥ 2. Например: x^2 + 5x + 6";
                        return null;
                    }
                    catch { return $"Не могу разобрать «{s}». Пример: x^2 - 5x + 6"; }
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Как ты хочешь разбить средний член?\n\n" +
                    "Для группировки квадратного трёхчлена ax²+bx+c нужно\n" +
                    "представить средний коэффициент b как сумму двух чисел p+q,\n" +
                    "так чтобы p·q = a·c\n\n" +
                    "Например: x²+5x+6 → b=5, a·c=6\n" +
                    "  Ищем p+q=5, p·q=6 → p=2, q=3 ✓\n" +
                    "  x²+2x+3x+6 = x(x+2)+3(x+2) = (x+2)(x+3)\n\n" +
                    "Введи первое слагаемое для разбиения (например: 2x^1 или -7x^1):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второе слагаемое для разбиения:",
                Validate = PolyValidate.CheckPoly
            },
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 1)
                return $"🔍 Уравнение: {answers[0]} = 0";
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly    = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(poly);
            var sb      = new StringBuilder();

            sb.AppendLine($"📌 Уравнение: {PolyParser.Format(reduced)} = 0");
            sb.AppendLine();

            // Получаем два члена разбиения от пользователя
            var split1 = PolyParser.Parse(answers[1]);
            var split2 = PolyParser.Parse(answers[2]);

            // Квадратный трёхчлен: первый, [разбитый1, разбитый2], свободный
            var deg2terms = reduced.Where(t => t.Degree == 2).ToList();
            var deg0terms = reduced.Where(t => t.Degree == 0).ToList();

            string s1 = PolyParser.Format(split1);
            string s2 = PolyParser.Format(split2);

            sb.AppendLine("Шаг 1. Разбиваем средний член:");
            sb.AppendLine($"  {PolyParser.Format(reduced)}");

            // Отображаем разбитый вид
            string leadStr  = deg2terms.Any() ? PolyParser.Format(deg2terms) : "";
            string freeStr  = deg0terms.Any() ? PolyParser.Format(deg0terms) : "";
            string splitStr = $"{(leadStr != "" ? leadStr + " + " : "")}{s1} + {s2}{(freeStr != "" ? " + " + freeStr : "")}";
            sb.AppendLine($"  = {splitStr}");
            sb.AppendLine();

            // Группа 1: первый + split1
            var g1terms = deg2terms.Concat(split1).ToList();
            var g2terms = split2.Concat(deg0terms).ToList();

            long gcd1 = g1terms.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));
            int  md1  = g1terms.Where(t => t.Coeff != 0).Min(t => t.Degree);
            if (gcd1 < 1) gcd1 = 1;
            var q1    = g1terms.Select(t => new PolyTerm(t.Coeff / gcd1, t.Degree - md1)).ToList();
            string f1 = FormatFactor(gcd1, md1);
            string b1 = PolyParser.Format(q1);

            long gcd2 = g2terms.Aggregate(0L, (acc, t) => GCD(Math.Abs(acc), Math.Abs(t.Coeff)));
            int  md2  = g2terms.Where(t => t.Coeff != 0).Min(t => t.Degree);
            if (gcd2 < 1) gcd2 = 1;
            // Если gcd2 отрицательный (ведущий знак) — корректируем
            if (g2terms.FirstOrDefault().Coeff < 0) gcd2 = -gcd2;
            var q2    = g2terms.Select(t => new PolyTerm(t.Coeff / gcd2, t.Degree - md2)).ToList();
            string f2 = FormatFactor(gcd2, md2);
            string b2 = PolyParser.Format(q2);

            sb.AppendLine("Шаг 2. Группируем и выносим общий множитель:");
            sb.AppendLine($"  ({PolyParser.Format(g1terms)}) + ({PolyParser.Format(g2terms)})");
            sb.AppendLine($"  = {f1}({b1}) + {f2}({b2})");
            sb.AppendLine();

            if (b1 == b2)
            {
                sb.AppendLine("Шаг 3. Скобки совпали — выносим:");
                sb.AppendLine($"  ({b1})({f1} + {f2}) = 0");
                sb.AppendLine();
                sb.AppendLine("Шаг 4. Произведение = 0 когда один множитель = 0:");

                // Пробуем найти корни из обоих множителей
                var roots = new List<string>();

                TryLinearRoot(PolyParser.Parse(b1), roots, sb);
                TryLinearRoot(PolyParser.Parse($"{f1}+{f2}".Replace("+ -","+-")), roots, sb);

                if (roots.Any())
                    sb.AppendLine($"\n✅ Ответ: {{{string.Join("; ", roots)}}}");
                else
                    sb.AppendLine($"\n✅ Разложение: ({b1})({f1} + {f2}) = 0\nКорни ищи самостоятельно из каждого множителя.");
            }
            else
            {
                sb.AppendLine("⚠️ Скобки не совпали. Проверь разбиение среднего члена:");
                sb.AppendLine($"  Группа 1 дала скобку: ({b1})");
                sb.AppendLine($"  Группа 2 дала скобку: ({b2})");
                sb.AppendLine();
                sb.AppendLine("💡 Правило: p + q = b,  p · q = a · c");
            }

            return sb.ToString().TrimEnd();
        }

        private static void TryLinearRoot(List<PolyTerm> terms, List<string> roots, StringBuilder sb)
        {
            var red = PolyParser.Reduce(terms);
            var lin = red.FirstOrDefault(t => t.Degree == 1);
            var con = red.FirstOrDefault(t => t.Degree == 0);
            if (lin.Coeff != 0 && red.Count <= 2 && red.All(t => t.Degree <= 1))
            {
                double root = -(double)con.Coeff / lin.Coeff;
                string rootStr = FmtD(root);
                roots.Add(rootStr);
                sb.AppendLine($"  {PolyParser.Format(red)} = 0  →  x = {rootStr}");
            }
        }

        private static string FormatFactor(long coeff, int degree)
        {
            if (degree == 0) return coeff == 1 ? "1" : coeff.ToString();
            string x = degree == 1 ? "x" : "x" + PolyTerm.Sup(degree);
            if (coeff ==  1) return x;
            if (coeff == -1) return "-" + x;
            return coeff + x;
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);

        private static string FmtD(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e15
                ? ((long)v).ToString()
                : v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    }

    // ═══════════════════════════════════════════════════════════════
    //  Заглушки — раздел «Умножение многочленов» (в разработке)
    // ═══════════════════════════════════════════════════════════════

    internal abstract class StubFunction : FunctionBase
    {
        protected abstract string SectionName { get; }
        public override string[] Parameters => [];
        public override double Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question  = $"⚙️ Раздел «{SectionName}» пока в разработке.\n\nНажми «◀️ Назад».",
                Validate  = _ => "Эта функция ещё не реализована. Нажми «◀️ Назад»."
            }
        ];

        public override string CalculateFromAnswers(List<string> answers) =>
            $"⚙️ «{Name}» ещё не реализована.";
    }

    
}

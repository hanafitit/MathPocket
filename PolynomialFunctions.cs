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

        /// <summary>
        /// Проверяет, есть ли подобные члены.
        /// </summary>
        public static bool HasLikeTerms(List<PolyTerm> terms) =>
            terms.GroupBy(t => t.Degree).Any(g => g.Count() > 1);

        /// <summary>
        /// Генерирует пошаговый текст приведения подобных членов.
        /// Возвращает null если подобных нет.
        /// </summary>
        public static string? ShowReduction(List<PolyTerm> terms)
        {
            var groups = terms.GroupBy(t => t.Degree)
                              .Where(g => g.Count() > 1)
                              .OrderByDescending(g => g.Key)
                              .ToList();
            if (!groups.Any()) return null;

            var sb = new System.Text.StringBuilder();
            foreach (var g in groups)
            {
                string label = g.Key == 0 ? "свободные члены"
                             : g.Key == 1 ? "члены с x"
                             : $"члены с x{PolyTerm.Sup(g.Key)}";
                string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                     .Replace("+ -", "− ");
                sb.AppendLine($"  {label}: {chain} = {g.Sum(t => t.Coeff)}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Форматирует список слагаемых без сортировки (для вывода "как ввёл").
        /// </summary>
        public static string FormatRaw(IEnumerable<PolyTerm> terms)
        {
            var list = terms.Where(t => t.Coeff != 0).ToList();
            if (!list.Any()) return "0";
            var sb = new System.Text.StringBuilder(list[0].ToStringFirst());
            foreach (var t in list.Skip(1))
                sb.Append(t.ToStringNext());
            return sb.ToString();
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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Parse(answers[0]);
                return $"\U0001f50d Многочлен: {PolyParser.Format(t)}";
            } catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);
            int deg     = PolyParser.PolynomialDegree(reduced);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();

            var reductionText = PolyParser.ShowReduction(terms);
            if (reductionText != null)
            {
                sb.AppendLine("Шаг 1. Приводим подобные слагаемые:");
                sb.AppendLine(reductionText);
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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Parse(answers[0]);
                return $"\U0001f50d Многочлен: {PolyParser.Format(t)}";
            } catch { return null; }
        }

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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Reduce(PolyParser.Parse(answers[0]));
                if (answers.Count == 1)
                    return $"\U0001f50d P(x) = {PolyParser.Format(t)}";
                return $"\U0001f50d P(x) = {PolyParser.Format(t)}, x = {answers[1]}";
            } catch { return null; }
        }

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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var parts = answers[0].Split(',');
                var sb = new System.Text.StringBuilder("\U0001f50d Члены: ");
                sb.Append(string.Join(", ", parts.Select(p => p.Trim())));
                return sb.ToString();
            } catch { return null; }
        }

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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Parse(answers[0]);
                return $"\U0001f50d Многочлен: {PolyParser.Format(t)}";
            } catch { return null; }
        }

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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Parse(answers[0]);
                return $"\U0001f50d Многочлен: {PolyParser.Format(t)}";
            } catch { return null; }
        }

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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t1 = PolyParser.Parse(answers[0]);
                if (answers.Count == 1)
                    return $"\U0001f50d Первый: {PolyParser.Format(t1)}";
                var t2 = PolyParser.Parse(answers[1]);
                var res = PolyParser.Reduce(t1.Concat(t2).ToList());
                return $"\U0001f50d ({PolyParser.Format(t1)}) + ({PolyParser.Format(t2)}) = {PolyParser.Format(res)}";
            } catch { return null; }
        }

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

            var reductionText2 = PolyParser.ShowReduction(all);
            if (reductionText2 != null)
            {
                sb.AppendLine("Шаг 2. Приводим подобные:");
                sb.AppendLine(reductionText2);
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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t1 = PolyParser.Parse(answers[0]);
                if (answers.Count == 1)
                    return $"\U0001f50d Первый: {PolyParser.Format(t1)}";
                var t2 = PolyParser.Parse(answers[1]);
                var t2neg = t2.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();
                var res = PolyParser.Reduce(t1.Concat(t2neg).ToList());
                return $"\U0001f50d ({PolyParser.Format(t1)}) − ({PolyParser.Format(t2)}) = {PolyParser.Format(res)}";
            } catch { return null; }
        }

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

            var reductionText2 = PolyParser.ShowReduction(all);
            if (reductionText2 != null)
            {
                sb.AppendLine("Шаг 2. Приводим подобные:");
                sb.AppendLine(reductionText2);
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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Parse(answers[0]);
                var r = PolyParser.Reduce(t);
                return $"\U0001f50d Многочлен: {PolyParser.Format(t)}\n    Стандартный вид: {PolyParser.Format(r)}";
            } catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);
            int deg     = PolyParser.PolynomialDegree(reduced);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Исходный многочлен: {PolyParser.Format(terms)}");
            sb.AppendLine();

            var reductionText = PolyParser.ShowReduction(terms);
            if (reductionText != null)
            {
                sb.AppendLine("Шаг 1. Приводим подобные члены:");
                sb.AppendLine(reductionText);
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


        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try {
                var t = PolyParser.Reduce(PolyParser.Parse(answers[0]));
                if (answers.Count == 1)
                    return $"\U0001f50d P(x) = {PolyParser.Format(t)}";
                if (answers.Count == 2)
                    return $"\U0001f50d P(x) = {PolyParser.Format(t)}, первое x = {answers[1]}";
                return $"\U0001f50d P(x) = {PolyParser.Format(t)}, x₁ = {answers[1]}, x₂ = {answers[2]}";
            } catch { return null; }
        }

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
    //  §12.1–12.4: Сумма нескольких многочленов (3+)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialSumManyFunction : FunctionBase
    {
        public override string   Name       => "Сумма нескольких многочленов";
        public override string   Formula    => "A + B + C + ...";
        public override string[] Keywords   => new[] { "сумма", "несколько", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        // Динамические шаги: сначала спросим сколько многочленов, потом каждый
        public override InputStep[] Steps => _steps;

        private static readonly InputStep[] _steps = new InputStep[]
        {
            new InputStep
            {
                Question =
                    "📘 Сумма нескольких многочленов\n\n" +
                    "Чтобы сложить несколько многочленов:\n" +
                    "  1. Раскрываем все скобки (знаки не меняются)\n" +
                    "  2. Приводим подобные члены\n\n" +
                    "Пример: (x² + 5) и (x² − 4) и (2x + 3)\n" +
                    "  = x² + 5 + x² − 4 + 2x + 3\n" +
                    "  = 2x² + 2x + 4\n\n" +
                    "Сколько многочленов хочешь сложить?\n" +
                    "Напиши число от 2 до 5.",
                Validate = s =>
                {
                    if (int.TryParse(s, out int n) && n >= 2 && n <= 5) return null;
                    return "Напиши число от 2 до 5.";
                }
            },
            new InputStep { Question = "✏️ Введи многочлен №1:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №2:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №3:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №4:", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №5:", Validate = PolyValidate.CheckPoly },
        };

        // Количество активных шагов: 1 (вопрос о кол-ве) + N многочленов
        public int ActiveStepCount(List<string> answers)
        {
            if (answers.Count == 0) return 6;
            if (int.TryParse(answers[0], out int n)) return 1 + n;
            return 6;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            if (answers.Count == 1)
            {
                int.TryParse(answers[0], out int n);
                return $"\U0001f50d Складываем {n} многочлена(ов)";
            }
            try
            {
                int.TryParse(answers[0], out int total);
                var entered = answers.Skip(1).ToList();
                var allTerms = new List<PolyTerm>();
                var parts = new List<string>();
                foreach (var a in entered)
                {
                    var t = PolyParser.Parse(a);
                    allTerms.AddRange(t);
                    parts.Add(PolyParser.Format(t));
                }
                var reduced = PolyParser.Reduce(allTerms);
                string joined = string.Join(" + ", parts.Select(p => $"({p})"));
                if (entered.Count < total)
                    return $"\U0001f50d Пока: {joined}\n    = {PolyParser.Format(reduced)}";
                return $"\U0001f50d ({string.Join(") + (", parts)})\n    = {PolyParser.Format(reduced)}";
            }
            catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            int.TryParse(answers[0], out int count);
            var polys = answers.Skip(1).Take(count).ToList();

            var allTerms = new List<PolyTerm>();
            var parsed   = new List<List<PolyTerm>>();
            foreach (var p in polys)
            {
                var t = PolyParser.Parse(p);
                parsed.Add(t);
                allTerms.AddRange(t);
            }
            var reduced = PolyParser.Reduce(allTerms);

            var sb = new StringBuilder();
            string expr = string.Join(" + ", parsed.Select(t => $"({PolyParser.Format(t)})"));
            sb.AppendLine($"✅ {expr}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем все скобки:");
            sb.AppendLine($"  {PolyParser.Format(allTerms)}");
            sb.AppendLine();

            var groups = allTerms.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
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
    //  §12.2–12.4: Разность нескольких многочленов (3+)
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialDiffManyFunction : FunctionBase
    {
        public override string   Name       => "Разность нескольких многочленов";
        public override string   Formula    => "A − B − C − ...";
        public override string[] Keywords   => new[] { "разность", "несколько", "многочлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => _steps;

        private static readonly InputStep[] _steps = new InputStep[]
        {
            new InputStep
            {
                Question =
                    "📘 Разность нескольких многочленов\n\n" +
                    "При вычитании меняем знаки всех членов вычитаемого.\n\n" +
                    "Пример: (5x² + 3x) − (2x² − x + 1) − (x² + 2)\n" +
                    "  = 5x² + 3x − 2x² + x − 1 − x² − 2\n" +
                    "  = 2x² + 4x − 3\n\n" +
                    "Сколько многочленов? (от 2 до 5)\n" +
                    "Первый — уменьшаемое, остальные вычитаются.",
                Validate = s =>
                {
                    if (int.TryParse(s, out int n) && n >= 2 && n <= 5) return null;
                    return "Напиши число от 2 до 5.";
                }
            },
            new InputStep { Question = "✏️ Введи многочлен №1 (уменьшаемое):", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №2 (вычитаемый):", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №3 (вычитаемый):", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №4 (вычитаемый):", Validate = PolyValidate.CheckPoly },
            new InputStep { Question = "✏️ Введи многочлен №5 (вычитаемый):", Validate = PolyValidate.CheckPoly },
        };

        public int ActiveStepCount(List<string> answers)
        {
            if (answers.Count == 0) return 6;
            if (int.TryParse(answers[0], out int n)) return 1 + n;
            return 6;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            if (answers.Count == 1) {
                int.TryParse(answers[0], out int n);
                return $"\U0001f50d Вычитаем {n-1} из первого";
            }
            try
            {
                int.TryParse(answers[0], out int total);
                var entered = answers.Skip(1).ToList();
                var allTerms = new List<PolyTerm>();
                for (int i = 0; i < entered.Count; i++)
                {
                    var t = PolyParser.Parse(entered[i]);
                    if (i == 0) allTerms.AddRange(t);
                    else allTerms.AddRange(t.Select(tt => new PolyTerm(-tt.Coeff, tt.Degree)));
                }
                var reduced = PolyParser.Reduce(allTerms);
                return $"\U0001f50d Пока: {PolyParser.Format(reduced)}";
            }
            catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            int.TryParse(answers[0], out int count);
            var polys = answers.Skip(1).Take(count).ToList();

            var parsed = new List<List<PolyTerm>>();
            foreach (var p in polys)
                parsed.Add(PolyParser.Parse(p));

            // Собираем с учётом знаков
            var allTerms = new List<PolyTerm>();
            for (int i = 0; i < parsed.Count; i++)
            {
                if (i == 0) allTerms.AddRange(parsed[i]);
                else allTerms.AddRange(parsed[i].Select(t => new PolyTerm(-t.Coeff, t.Degree)));
            }
            var reduced = PolyParser.Reduce(allTerms);

            var sb = new StringBuilder();
            string expr = PolyParser.Format(parsed[0]) + " − " +
                          string.Join(" − ", parsed.Skip(1).Select(t => $"({PolyParser.Format(t)})"));
            sb.AppendLine($"✅ ({expr})");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Раскрываем скобки (меняем знаки вычитаемых):");
            sb.AppendLine($"  {PolyParser.Format(allTerms)}");
            sb.AppendLine();

            var groups = allTerms.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
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
    //  §12.7: Проверить тождество многочленов
    //  Пользователь вводит левую и правую части, бот проверяет
    //  равенство подстановкой нескольких значений x
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialIdentityCheckFunction : FunctionBase
    {
        public override string   Name       => "Проверить тождество";
        public override string   Formula    => "Верно ли A = B?";
        public override string[] Keywords   => new[] { "тождество", "проверить", "верно", "равенство" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как проверить тождество?\n\n" +
                    "Тождество — это равенство, верное при любых значениях переменных.\n\n" +
                    "Способ проверки:\n" +
                    "  1. Упростим левую и правую части (приведём подобные)\n" +
                    "  2. Если стандартные виды совпадают — тождество верно\n\n" +
                    "Пример: (18,9 − x²) − (5x² − 21) + (7x³ − 39,9) = x²?\n" +
                    "  Левая часть: 18,9 − x² − 5x² + 21 + 7x³ − 39,9\n" +
                    "             = 7x³ − 6x² + 0 = 7x³ − 6x²\n" +
                    "  Правая: x²\n" +
                    "  Не совпадают → тождество неверно\n\n" +
                    "✏️ Введи левую часть равенства:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи правую часть равенства:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try
            {
                var left = PolyParser.Reduce(PolyParser.Parse(answers[0]));
                if (answers.Count == 1)
                    return $"\U0001f50d Левая часть: {PolyParser.Format(left)}";
                var right = PolyParser.Reduce(PolyParser.Parse(answers[1]));
                return $"\U0001f50d {PolyParser.Format(left)} = {PolyParser.Format(right)}?";
            }
            catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var leftRaw  = PolyParser.Parse(answers[0]);
            var rightRaw = PolyParser.Parse(answers[1]);
            var left     = PolyParser.Reduce(leftRaw);
            var right    = PolyParser.Reduce(rightRaw);

            string leftStr  = PolyParser.Format(left);
            string rightStr = PolyParser.Format(right);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Проверяем: {PolyParser.Format(leftRaw)} = {PolyParser.Format(rightRaw)}");
            sb.AppendLine();

            // Проверяем приведение левой
            bool leftHadLike = leftRaw.GroupBy(t => t.Degree).Any(g => g.Count() > 1);
            if (leftHadLike)
            {
                sb.AppendLine("Шаг 1. Упрощаем левую часть:");
                sb.AppendLine($"  {leftStr}");
            }
            else sb.AppendLine($"Левая часть: {leftStr}");

            bool rightHadLike = rightRaw.GroupBy(t => t.Degree).Any(g => g.Count() > 1);
            if (rightHadLike)
            {
                sb.AppendLine("Шаг 2. Упрощаем правую часть:");
                sb.AppendLine($"  {rightStr}");
            }
            else sb.AppendLine($"Правая часть: {rightStr}");

            sb.AppendLine();

            // Сравниваем по коэффициентам
            bool equal = AreEqual(left, right);
            if (equal)
            {
                sb.AppendLine($"Левая = {leftStr}");
                sb.AppendLine($"Правая = {rightStr}");
                sb.AppendLine();
                sb.AppendLine("✅ Стандартные виды совпадают.");
                sb.AppendLine("📌 Тождество ВЕРНО ✓");
            }
            else
            {
                sb.AppendLine($"Левая  = {leftStr}");
                sb.AppendLine($"Правая = {rightStr}");
                sb.AppendLine();
                sb.AppendLine("❌ Стандартные виды не совпадают.");
                sb.AppendLine("📌 Тождество НЕВЕРНО ✗");
            }

            return sb.ToString().TrimEnd();
        }

        private static bool AreEqual(List<PolyTerm> a, List<PolyTerm> b)
        {
            var da = a.Where(t => t.Coeff != 0).ToDictionary(t => t.Degree, t => t.Coeff);
            var db = b.Where(t => t.Coeff != 0).ToDictionary(t => t.Degree, t => t.Coeff);
            if (da.Count != db.Count) return false;
            foreach (var kv in da)
                if (!db.TryGetValue(kv.Key, out long v) || v != kv.Value) return false;
            return true;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §12.8: Упростить выражение и найти его значение
    // ═══════════════════════════════════════════════════════════════
    public class PolynomialSimplifyAndEvalFunction : FunctionBase
    {
        public override string   Name       => "Упростить и найти значение";
        public override string   Formula    => "Упростить → подставить x";
        public override string[] Keywords   => new[] { "упростить", "значение", "выражение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упростить выражение и найти значение\n\n" +
                    "Порядок действий:\n" +
                    "  1. Раскрыть скобки\n" +
                    "  2. Привести подобные\n" +
                    "  3. Подставить заданное значение x\n\n" +
                    "Пример: (20a² + 7a²) − (57 − 20a²) при a = 2\n" +
                    "  = 20a² + 7a² − 57 + 20a²\n" +
                    "  = 47a² − 57\n" +
                    "  При a=2: 47·4 − 57 = 188 − 57 = 131\n\n" +
                    "Как записывать выражение:\n" +
                    "  · Введи уже раскрытое выражение одной строкой\n" +
                    "  · x² → x^2,  члены через + или −\n\n" +
                    "✏️ Введи выражение (многочлен):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question =
                    "✏️ При каком значении x найти значение?\n\n" +
                    "Введи одно число, например: 2 или -5 или 0.5",
                Validate = PolyValidate.CheckNumber
            }
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            try
            {
                var t = PolyParser.Reduce(PolyParser.Parse(answers[0]));
                if (answers.Count == 1)
                    return $"\U0001f50d Упрощено: {PolyParser.Format(t)}";
                double x = double.Parse(answers[1].Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture);
                double val = t.Sum(tt => tt.Coeff * Math.Pow(x, tt.Degree));
                return $"\U0001f50d {PolyParser.Format(t)} при x={answers[1]} = {FmtNum(val)}";
            }
            catch { return null; }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var raw     = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(raw);

            double x = double.Parse(answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);
            string xStr = FmtNum(x);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Выражение: {PolyParser.Format(raw)}");
            sb.AppendLine();

            var reductionText = PolyParser.ShowReduction(raw);
            if (reductionText != null)
            {
                sb.AppendLine("Шаг 1. Приводим подобные члены:");
                sb.AppendLine(reductionText);
                sb.AppendLine($"  Упрощённый вид: {PolyParser.Format(reduced)}");
                sb.AppendLine();
            }

            sb.AppendLine($"Шаг 2. Подставляем x = {xStr}:");
            double total = 0;
            var parts = new List<string>();
            foreach (var t in reduced.OrderByDescending(t => t.Degree))
            {
                if (t.Coeff == 0) continue;
                double tVal = t.Coeff * Math.Pow(x, t.Degree);
                string detail = t.Degree == 0 ? $"{t.Coeff}"
                              : t.Degree == 1 ? $"{t.Coeff}·{xStr} = {FmtNum(tVal)}"
                              : $"{t.Coeff}·{xStr}{PolyTerm.Sup(t.Degree)} = {FmtNum(tVal)}";
                sb.AppendLine($"  {t.ToStringFirst()}: {detail}");
                total += tVal;
                parts.Add(FmtNum(tVal));
            }

            if (parts.Count > 1)
            {
                string sumLine = string.Join(" + ", parts).Replace("+ -", "− ");
                sb.AppendLine();
                sb.AppendLine($"  Складываем: {sumLine} = {FmtNum(total)}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Значение при x = {xStr}: {FmtNum(total)}");
            return sb.ToString().TrimEnd();
        }

        private static string FmtNum(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e15) return ((long)v).ToString();
            return v.ToString("G10", System.Globalization.CultureInfo.InvariantCulture)
                    .TrimEnd('0').TrimEnd('.');
        }
    }
}

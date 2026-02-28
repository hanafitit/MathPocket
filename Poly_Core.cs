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
    //  ФУНКЦИЯ 1: Степень многочлена
}

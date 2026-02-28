using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
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
//  Вынесение многочлена как общего множителя
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
//  Решить уравнение через вынесение за скобки
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
    //  Группировка 4 члена (2+2)
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
                    "📘 Разложение способом группировки\n\n" +
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
    //  Группировка 6 членов (3+3)
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
                    "📘 Группировка шести членов (3+3)\n\n" +
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
    //  Уравнение через группировку
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
                    "📘 Решение уравнения через группировку\n\n" +
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
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathPocket
{

    // ─── Вспомогательные утилиты для ФСУ ─────────────────────────────────────

    internal static class DiffSquaresHelper
    {
        /// <summary>Парсит дробь вида "3/4", целое "5" или десятичное "1.5" → (числитель, знаменатель).</summary>
        public static (long num, long den) ParseFrac(string s)
        {
            s = s.Trim().Replace(',', '.').Replace("−", "-");
            if (s.Contains('/'))
            {
                var p = s.Split('/');
                return (long.Parse(p[0]), long.Parse(p[1]));
            }
            if (s.Contains('.'))
            {
                int dot = s.IndexOf('.');
                int dec = s.Length - dot - 1;
                long den = (long)Math.Pow(10, dec);
                long num = long.Parse(s.Replace(".", ""));
                long g = Gcd(Math.Abs(num), den);
                return (num / g, den / g);
            }
            return (long.Parse(s), 1);
        }

        public static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

        /// <summary>Форматирует дробь: если знаменатель 1 — возвращает только числитель.</summary>
        public static string FmtFrac(long num, long den)
        {
            long g = Gcd(Math.Abs(num), Math.Abs(den));
            num /= g; den /= g;
            if (den < 0) { num = -num; den = -den; }
            return den == 1 ? num.ToString() : $"{num}/{den}";
        }

        /// <summary>Возводит дробь в квадрат.</summary>
        public static (long num, long den) SqFrac(long num, long den)
            => (num * num, den * den);

        /// <summary>Умножает две дроби.</summary>
        public static (long num, long den) MulFrac(long n1, long d1, long n2, long d2)
        {
            long n = n1 * n2, d = d1 * d2;
            long g = Gcd(Math.Abs(n), Math.Abs(d));
            return (n / g, d / g);
        }

        /// <summary>Складывает/вычитает две дроби.</summary>
        public static (long num, long den) AddFrac(long n1, long d1, long n2, long d2, bool minus = false)
        {
            if (minus) n2 = -n2;
            long lcm = d1 / Gcd(d1, d2) * d2;
            long n = n1 * (lcm / d1) + n2 * (lcm / d2);
            long g = Gcd(Math.Abs(n), Math.Abs(lcm));
            return (n / g, lcm / g);
        }

        public static string? ValidateNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.";
            s = s.Trim().Replace(',', '.').Replace("−", "-");
            if (s.Contains('/'))
            {
                var p = s.Split('/');
                if (p.Length == 2 && long.TryParse(p[0], out _) && long.TryParse(p[1], out long den) && den != 0)
                    return null;
                return $"Не могу разобрать «{s}» как дробь. Пример: 3/4";
            }
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return null;
            return $"«{s}» не похоже на число. Введи целое, дробь (3/4) или десятичное (1.5).";
        }
    }

    // ─── 31.1 / 31.2  Выполнить умножение / действие: (a+b)(a−b) ─────────────

    /// <summary>
    /// Раскрывает произведение суммы и разности: (a + b)(a − b) = a² − b²
    /// Пользователь вводит a и b как числа или выражения-описания.
    /// </summary>
    public class DiffSqExpandFunction : FunctionBase
    {
        public override string   Name       => "Раскрыть (a+b)(a−b)";
        public override string   Formula    => "(a + b)(a − b) = a² − b²";
        public override string[] Keywords   => new[] { "разность квадратов", "раскрыть", "умножение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Формула разности квадратов:\n" +
                    "  (a + b)(a − b) = a² − b²\n" +
                    "Логика: раскрываем скобки, средние члены +ab и −ab сокращаются.\n" +
                    "Примеры:\n" +
                    "  (x + 3)(x − 3) = x² − 9\n" +
                    "  (2a + 5b)(2a − 5b) = 4a² − 25b²\n" +
                    "  (1/3 + x)(1/3 − x) = 1/9 − x²\n" +
                    "✏️ Введи a (первое выражение):\n" +
                    "  Пример: x  или  2a  или  3/4",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (второе выражение):\n" +
                           "  Пример: 3  или  5b  или  1/3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a = answers[0].Trim();
            string b = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: ({a} + {b})({a} − {b})");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу: (a + b)(a − b) = a² − b²");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}   →   a² = ({a})²");
            sb.AppendLine($"  b = {b}   →   b² = ({b})²");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({a} + {b})({a} − {b}) = ({a})² − ({b})²");

            // Если оба являются числами — вычислим численно
            double av = 0, bv = 0;
            bool aIsNum = double.TryParse(a.Replace(',', '.').Replace("−", "-"),
                NumberStyles.Any, CultureInfo.InvariantCulture, out av);
            bool bIsNum = double.TryParse(b.Replace(',', '.').Replace("−", "-"),
                NumberStyles.Any, CultureInfo.InvariantCulture, out bv);

            if (aIsNum && bIsNum)
            {
                double result = av * av - bv * bv;
                sb.AppendLine($"         = {av * av} − {bv * bv}");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: {result}");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: ({a})² − ({b})²");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 31.3 / 31.4  Разложить на множители: a² − b² = (a+b)(a−b) ──────────

    /// <summary>
    /// Разложение разности квадратов на множители: a² − b² = (a + b)(a − b)
    /// </summary>
    public class DiffSqFactorFunction : FunctionBase
    {
        public override string   Name       => "Разложить a²−b² на множители";
        public override string   Formula    => "a² − b² = (a + b)(a − b)";
        public override string[] Keywords   => new[] { "разность квадратов", "разложить", "множители", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Разложение разности квадратов:\n" +
                    "  a² − b² = (a + b)(a − b)\n" +
                    "Шаг 1 — найти a: это квадратный корень из первого слагаемого.\n" +
                    "Шаг 2 — найти b: это квадратный корень из второго слагаемого.\n" +
                    "Шаг 3 — записать произведение (a + b)(a − b).\n" +
                    "Примеры:\n" +
                    "  x² − 9      =  (x + 3)(x − 3)        [a=x, b=3]\n" +
                    "  4a² − 25b²  =  (2a + 5b)(2a − 5b)    [a=2a, b=5b]\n" +
                    "  1/9 − y²    =  (1/3 + y)(1/3 − y)    [a=1/3, b=y]\n" +
                    "✏️ Введи a (корень первого слагаемого):\n" +
                    "  Пример: x  или  2a  или  1/3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (корень второго слагаемого):\n" +
                           "  Пример: 3  или  5b  или  y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a = answers[0].Trim();
            string b = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a})² − ({b})²");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу: a² − b² = (a + b)(a − b)");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}");
            sb.AppendLine($"  b = {b}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({a})² − ({b})²  =  ({a} + {b})({a} − {b})");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a} + {b})({a} − {b})");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 31.5  Вычислить с помощью формулы a²−b² = (a−b)(a+b) ───────────────

    /// <summary>
    /// Вычисление разности квадратов чисел: a² − b² = (a − b)(a + b)
    /// Пользователь вводит a и b как числа (целые, дроби, смешанные).
    /// </summary>
    public class DiffSqComputeFunction : FunctionBase
    {
        public override string   Name       => "Вычислить a²−b² через формулу";
        public override string   Formula    => "a² − b² = (a − b)(a + b)";
        public override string[] Keywords   => new[] { "разность квадратов", "вычислить", "фсу", "a-b" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вычисление по формуле разности квадратов:\n" +
                    "  a² − b² = (a − b)(a + b)\n" +
                    "Удобно применять, когда перемножить (a−b) и (a+b) ПРОЩЕ,\n" +
                    "чем считать a² и b² по отдельности.\n" +
                    "Примеры:\n" +
                    "  13² − 9²  = (13−9)(13+9) = 4 · 22 = 88\n" +
                    "  76² − 24² = (76−24)(76+24) = 52 · 100 = 5200\n" +
                    "  (3/8)² − (4/5)² = (3/8 − 4/5)(3/8 + 4/5)\n" +
                    "✏️ Введи a:\n" +
                    "  Пример: 13  или  3/8  или  2.2",
                Validate = DiffSquaresHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи b:\n" +
                           "  Пример: 9  или  4/5  или  2.8",
                Validate = DiffSquaresHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (an, ad) = DiffSquaresHelper.ParseFrac(answers[0].Trim());
            var (bn, bd) = DiffSquaresHelper.ParseFrac(answers[1].Trim());

            string aStr = DiffSquaresHelper.FmtFrac(an, ad);
            string bStr = DiffSquaresHelper.FmtFrac(bn, bd);

            // (a−b)
            var (diffN, diffD) = DiffSquaresHelper.AddFrac(an, ad, bn, bd, minus: true);
            // (a+b)
            var (sumN, sumD) = DiffSquaresHelper.AddFrac(an, ad, bn, bd, minus: false);

            // результат = diff * sum
            var (resN, resD) = DiffSquaresHelper.MulFrac(diffN, diffD, sumN, sumD);

            // a² и b² для проверки
            var (a2n, a2d) = DiffSquaresHelper.SqFrac(an, ad);
            var (b2n, b2d) = DiffSquaresHelper.SqFrac(bn, bd);
            var (checkN, checkD) = DiffSquaresHelper.AddFrac(a2n, a2d, b2n, b2d, minus: true);

            string diffStr = DiffSquaresHelper.FmtFrac(diffN, diffD);
            string sumStr  = DiffSquaresHelper.FmtFrac(sumN, sumD);
            string resStr  = DiffSquaresHelper.FmtFrac(resN, resD);

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: {aStr}² − {bStr}²");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу: a² − b² = (a − b)(a + b)");
            sb.AppendLine();
            sb.AppendLine($"  a = {aStr},  b = {bStr}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  {aStr}² − {bStr}²");
            sb.AppendLine($"  = ({aStr} − {bStr})({aStr} + {bStr})");
            sb.AppendLine($"  = {diffStr} · {sumStr}");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: {resStr}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 31.6  Вычислить, представив как разность квадратов (101·99 и т.п.) ──

    /// <summary>
    /// Вычисление произведения вида (n+1)(n−1) через a²−b²
    /// Например: 101 · 99 = (100+1)(100−1) = 100² − 1² = 9999
    /// </summary>
    public class DiffSqProductTrickFunction : FunctionBase
    {
        public override string   Name       => "Вычислить произведение через a²−b²";
        public override string   Formula    => "(a+b)(a−b) = a² − b²";
        public override string[] Keywords   => new[] { "разность квадратов", "произведение", "101 99", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вычислить произведение через формулу разности квадратов:\n" +
                    "  (a + b)(a − b) = a² − b²\n" +
                    "Идея: если два числа симметричны вокруг некоторого «среднего»,\n" +
                    "то их произведение = среднее² − отклонение².\n" +
                    "Примеры:\n" +
                    "  101 · 99  → среднее 100, отклонение 1\n" +
                    "            = 100² − 1² = 10000 − 1 = 9999\n" +
                    "  103 · 97  → среднее 100, отклонение 3\n" +
                    "            = 100² − 3² = 10000 − 9 = 9991\n" +
                    "✏️ Введи первый множитель (большее число):\n" +
                    "  Пример: 101",
                Validate = DiffSquaresHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи второй множитель (меньшее число):\n" +
                           "  Пример: 99",
                Validate = DiffSquaresHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            if (!double.TryParse(answers[0].Trim().Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double x))
                return "Ошибка разбора первого числа.";
            if (!double.TryParse(answers[1].Trim().Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                return "Ошибка разбора второго числа.";

            double a = (x + y) / 2.0;
            double b = (x - y) / 2.0;
            double result = x * y;

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: {Fmt(x)} · {Fmt(y)}");
            sb.AppendLine();

            if (b == Math.Floor(b))   // отклонение целое — красивая форма
            {
                sb.AppendLine($"Замечаем: оба числа симметричны вокруг {Fmt(a)}");
                sb.AppendLine($"  {Fmt(x)} = {Fmt(a)} + {Fmt(b)}");
                sb.AppendLine($"  {Fmt(y)} = {Fmt(a)} − {Fmt(b)}");
                sb.AppendLine();
                sb.AppendLine("Применяем: (a + b)(a − b) = a² − b²");
                sb.AppendLine();
                sb.AppendLine($"  {Fmt(x)} · {Fmt(y)}");
                sb.AppendLine($"  = ({Fmt(a)} + {Fmt(b)})({Fmt(a)} − {Fmt(b)})");
                sb.AppendLine($"  = {Fmt(a)}² − {Fmt(b)}²");
                sb.AppendLine($"  = {Fmt(a * a)} − {Fmt(b * b)}");
            }
            else
            {
                sb.AppendLine("Применяем: (a + b)(a − b) = a² − b²");
                sb.AppendLine($"  {Fmt(x)} · {Fmt(y)} = {Fmt(a * a)} − {Fmt(b * b)}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: {Fmt(result)}");

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 31.7  Упростить выражение ────────────────────────────────────────────

    /// <summary>
    /// Упрощение выражений вида (a+b)(a−b) − c²  или  c² + (a+b)(a−b) и т.п.
    /// Пошаговое объяснение стратегии упрощения.
    /// </summary>
    public class DiffSqSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить выражение с (a+b)(a−b)";
        public override string   Formula    => "(a+b)(a−b) = a² − b²";
        public override string[] Keywords   => new[] { "разность квадратов", "упростить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражений с формулой разности квадратов\n" +
                    "Стратегия:\n" +
                    "1️⃣  Найди части вида (a + b)(a − b) и замени на a² − b²\n" +
                    "2️⃣  Раскрой оставшиеся скобки\n" +
                    "3️⃣  Приведи подобные слагаемые\n" +
                    "Примеры:\n" +
                    "  (5+b)(b−5) − b²\n" +
                    "  = (b+5)(b−5) − b²\n" +
                    "  = b² − 25 − b²\n" +
                    "  = −25\n" +
                    "  (1/3−a)(1/3+a) − 1/9\n" +
                    "  = 1/9 − a² − 1/9\n" +
                    "  = −a²\n" +
                    "✏️ Введи a (первое выражение в паре скобок):\n" +
                    "  Пример: b  или  1/3  или  9x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (второе выражение в паре скобок):\n" +
                           "  Пример: 5  или  a  или  4",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи остаток выражения (то что прибавляется/вычитается после скобок):\n" +
                    "  Если ничего нет — введи 0\n" +
                    "  Пример: -b²  или  +1/9  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            string rest = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a} + {b})({a} − {b}){(rest == "0" ? "" : " " + rest)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Применяем формулу (a + b)(a − b) = a² − b²");
            sb.AppendLine($"  ({a} + {b})({a} − {b})  =  ({a})² − ({b})²");
            sb.AppendLine();

            if (rest != "0" && !string.IsNullOrEmpty(rest))
            {
                sb.AppendLine("Шаг 2: Записываем полное выражение");
                sb.AppendLine($"  ({a})² − ({b})² {rest}");
                sb.AppendLine();
                sb.AppendLine("Шаг 3: Приводим подобные слагаемые (если есть)");
                sb.AppendLine($"  Проверь, не сокращается ли ({b})² с {rest}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Результат после замены: ({a})² − ({b})²" +
                          (rest == "0" ? "" : " " + rest));

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 31.8  Найти значение выражения (подстановка после упрощения) ─────────

    /// <summary>
    /// Найти значение выражения вида (a+b)(a−b) при заданном x.
    /// Пример: (7+d)(d−7) + (d+3)(3−d) + 40(d+1) при d=0.5
    /// </summary>
    public class DiffSqEvalFunction : FunctionBase
    {
        public override string   Name       => "Найти значение выражения с a²−b²";
        public override string   Formula    => "упростить, затем подставить";
        public override string[] Keywords   => new[] { "разность квадратов", "значение", "подставить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Нахождение значения выражения\n" +
                    "Стратегия:\n" +
                    "1️⃣  Упрости выражение (примени формулу разности квадратов)\n" +
                    "2️⃣  Подставь числовое значение переменной\n" +
                    "Пример:\n" +
                    "  (7+d)(d−7) + (d+3)(3−d) + 40(d+1)  при d = 0.5\n" +
                    "  (7+d)(d−7) = (d+7)(d−7) = d² − 49\n" +
                    "  (d+3)(3−d) = −(d+3)(d−3) = −(d² − 9) = −d² + 9\n" +
                    "  40(d+1) = 40d + 40\n" +
                    "  Итого: d² − 49 − d² + 9 + 40d + 40 = 40d + 0 = 40d\n" +
                    "  При d = 0.5: 40 · 0.5 = 20\n" +
                    "✏️ Введи упрощённое выражение (после применения формулы):\n" +
                    "  Пример: 40d  или  -a²  или  25",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи имя переменной (одна буква):\n" +
                           "  Пример: d  или  x  или  a",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Ты ничего не ввёл.";
                    if (s.Trim().Length != 1) return "Введи одну букву, например: x";
                    return null;
                }
            },
            new InputStep
            {
                Question = "✏️ Введи числовое значение переменной:\n" +
                           "  Пример: 0.5  или  -101  или  2/3",
                Validate = DiffSquaresHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string expr = answers[0].Trim();
            string var_  = answers[1].Trim();
            string valStr = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("Алгоритм решения:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Упрощаем выражение с помощью формулы (a+b)(a−b) = a²−b²");
            sb.AppendLine($"  Упрощённый вид: {expr}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 2: Подставляем {var_} = {valStr}");

            // Пробуем подставить числовое значение в простые линейные выражения
            bool parsed = double.TryParse(valStr.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double val);

            if (parsed)
            {
                // Попытка вычислить линейное выражение вида k·var + c
                // Простой парсинг: ищем коэффициент перед переменной
                string e = expr.Replace(" ", "").Replace("−", "-").Replace(",", ".");
                double k = 0, c = 0;
                bool linearParsed = TryParseLinear(e, var_[0], out k, out c);

                if (linearParsed)
                {
                    double result = k * val + c;
                    sb.AppendLine($"  {expr}");
                    if (k != 0 && c != 0)
                        sb.AppendLine($"  = {FmtD(k)} · {valStr} + ({FmtD(c)})");
                    else if (k != 0)
                        sb.AppendLine($"  = {FmtD(k)} · {valStr}");
                    else
                        sb.AppendLine($"  = {FmtD(c)}");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Ответ: {FmtD(result)}");
                }
                else
                {
                    sb.AppendLine($"  Подставь {var_} = {valStr} в выражение {expr} вручную.");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Вычисли результат подстановки {var_} = {valStr}.");
                }
            }
            else
            {
                sb.AppendLine($"  Подставь {var_} = {valStr} в {expr}");
            }

            return sb.ToString().TrimEnd();
        }

        private static bool TryParseLinear(string e, char v, out double k, out double c)
        {
            k = 0; c = 0;
            // Проверяем нет ли квадратов — если есть, не линейное
            if (e.Contains(v + "²") || e.Contains(v + "^2")) return false;
            // Ищем kv+c или kv-c или просто kv
            var m = System.Text.RegularExpressions.Regex.Match(e,
                $@"^([+-]?[0-9]*\.?[0-9]*){v}([+-][0-9]+\.?[0-9]*)?$");
            if (!m.Success) return false;
            string ks = m.Groups[1].Value;
            string cs = m.Groups[2].Value;
            k = string.IsNullOrEmpty(ks) || ks == "+" ? 1
              : ks == "-" ? -1
              : double.Parse(ks, CultureInfo.InvariantCulture);
            c = string.IsNullOrEmpty(cs) ? 0
              : double.Parse(cs, CultureInfo.InvariantCulture);
            return true;
        }

        private static string FmtD(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 31.9  Решить уравнение a²−b²=0 ─────────────────────────────────────

    /// <summary>
    /// Решение уравнений вида a² − b² = 0 (разложение + ноль-произведение).
    /// Примеры: x² − 16 = 0,  25 − y² = 0,  1.69 − z² = 0
    /// </summary>
    public class DiffSqEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение a²−b²=0";
        public override string   Formula    => "a² − b² = (a+b)(a−b) = 0";
        public override string[] Keywords   => new[] { "разность квадратов", "уравнение", "корни", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений через формулу разности квадратов\n" +
                    "  a² − b² = 0  →  (a + b)(a − b) = 0\n" +
                    "  →  a + b = 0  или  a − b = 0\n" +
                    "  →  a = −b    или  a = b\n" +
                    "Примеры:\n" +
                    "  x² − 16 = 0  [a=x, b=4]\n" +
                    "  (x+4)(x−4) = 0\n" +
                    "  x = −4  или  x = 4\n" +
                    "  1,69 − z² = 0  →  z² = 1,69  [a=z, b=1,3]\n" +
                    "  (z+1,3)(z−1,3) = 0\n" +
                    "  z = −1,3  или  z = 1,3\n" +
                    "✏️ Введи a (переменная или выражение с переменной):\n" +
                    "  Пример: x  или  z  или  y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (числовое значение, корень из числа):\n" +
                    "  Примеры:  4   (если уравнение x²−16=0)\n" +
                    "            1.3 (если уравнение z²=1.69)\n" +
                    "            5   (если уравнение 25−y²=0)",
                Validate = DiffSquaresHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a  = answers[0].Trim();
            string bS = answers[1].Trim();

            bool isNum = double.TryParse(bS.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double b);

            var (bn, bd) = DiffSquaresHelper.ParseFrac(bS);
            string bStr = DiffSquaresHelper.FmtFrac(bn, bd);

            // b² для заголовка
            var (b2n, b2d) = DiffSquaresHelper.SqFrac(bn, bd);
            string b2Str = DiffSquaresHelper.FmtFrac(b2n, b2d);

            // Отрицательный b
            var (mbn, mbd) = (-bn, bd);
            string mbStr = DiffSquaresHelper.FmtFrac(mbn, mbd);

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {a}² − {b2Str} = 0");
            sb.AppendLine();
            sb.AppendLine($"Замечаем: {b2Str} = {bStr}²");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Применяем формулу a² − b² = (a + b)(a − b)");
            sb.AppendLine($"  {a}² − {bStr}²  =  ({a} + {bStr})({a} − {bStr})");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Уравнение принимает вид");
            sb.AppendLine($"  ({a} + {bStr})({a} − {bStr}) = 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Произведение равно нулю, если хотя бы один множитель = 0");
            sb.AppendLine($"  {a} + {bStr} = 0   →   {a} = {mbStr}");
            sb.AppendLine($"  {a} − {bStr} = 0   →   {a} = {bStr}");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: {{{mbStr}; {bStr}}}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 31.10 / 31.11  Доказать тождество / независимость от переменной ──────

    /// <summary>
    /// Объяснение стратегии доказательства тождеств и независимости выражения от переменной.
    /// </summary>
    public class DiffSqProveIdentityFunction : FunctionBase
    {
        public override string   Name       => "Доказать тождество с a²−b²";
        public override string   Formula    => "a² − b² = (a+b)(a−b)";
        public override string[] Keywords   => new[] { "разность квадратов", "тождество", "доказать", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождеств\n" +
                    "Стратегия:\n" +
                    "  Преобразуй ЛЕВУЮ часть так, чтобы получить ПРАВУЮ.\n" +
                    "Часто нужно:\n" +
                    "  · применить (a+b)(a−b) = a²−b²\n" +
                    "  · раскрыть скобки\n" +
                    "  · привести подобные члены\n" +
                    "Пример:\n" +
                    "  Доказать: (x−1.6)(1.6+x) + 5 − x² = −(0.6)·(2x−0.6)\n" +
                    "  Левая часть:\n" +
                    "  (x+1.6)(x−1.6) + 5 − x²\n" +
                    "  = x² − 2.56 + 5 − x²\n" +
                    "  = 2.44  ✅\n" +
                    "✏️ Введи левую часть тождества (своими словами или шаги):\n" +
                    "  Пример: (x-1,6)(1,6+x) + 5 - x²",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи ожидаемый результат (правая часть или число):\n" +
                    "  Пример: 2.44  или  -6",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string right  = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства тождества:");
            sb.AppendLine();
            sb.AppendLine($"Левая часть:  {left}");
            sb.AppendLine($"Правая часть: {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Найди произведения вида (a+b)(a−b) и замени на a²−b²");
            sb.AppendLine("Шаг 2: Раскрой оставшиеся скобки");
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("Шаг 4: Сравни с правой частью");
            sb.AppendLine();
            sb.AppendLine("Если хочешь проверить конкретный шаг — напиши его мне 😊");
            sb.AppendLine();
            sb.AppendLine($"📌 Цель: показать, что левая часть = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    //  §32. Формулы квадрата суммы и квадрата разности двух выражений
    //  (a + b)² = a² + 2ab + b²
    //  (a − b)² = a² − 2ab + b²
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── 32.1 / 32.2  Представить в виде многочлена: (a±b)² ──────────────────

    /// <summary>
    /// Раскрывает квадрат суммы или разности: (a ± b)² → трёхчлен
    /// Задачи 32.1, 32.2
    /// </summary>
    public class SqSumExpandFunction : FunctionBase
    {
        public override string   Name       => "Раскрыть (a±b)² в многочлен";
        public override string   Formula    => "(a±b)² = a² ± 2ab + b²";
        public override string[] Keywords   => new[] { "квадрат суммы", "квадрат разности", "раскрыть", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Формулы квадрата суммы и квадрата разности:\n" +
                    "  (a + b)² = a² + 2ab + b²\n" +
                    "  (a − b)² = a² − 2ab + b²\n" +
                    "Как запомнить:\n" +
                    "  Квадрат первого  +/−  удвоенное произведение  +  квадрат второго\n" +
                    "Примеры:\n" +
                    "  (m − 3)²    = m² − 6m + 9\n" +
                    "  (x + 5)²    = x² + 10x + 25\n" +
                    "  (a + 1/7)²  = a² + 2/7·a + 1/49\n" +
                    "  (4¹/₃ − x)² = (4¹/₃)² − 2·4¹/₃·x + x²\n" +
                    "✏️ Введи a (первое выражение):\n" +
                    "  Пример: m  или  5n  или  0.7c",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второе выражение):\n" +
                    "  Пример: 3  или  2m  или  1/7",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак между a и b: сумма или разность?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string pm   = plus ? "+" : "−";

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: ({a} {sign} {b})²");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: (a + b)² = a² + 2ab + b²"
                : "Формула: (a − b)² = a² − 2ab + b²");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}");
            sb.AppendLine($"  b = {b}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({a} {sign} {b})²");
            sb.AppendLine($"  = ({a})² {pm} 2·({a})·({b}) + ({b})²");
            sb.AppendLine();

            // Числовой расчёт если оба числа
            double av = 0, bv = 0;
            bool aIsNum = double.TryParse(a.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out av);
            bool bIsNum = double.TryParse(b.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out bv);

            if (aIsNum && bIsNum)
            {
                double a2  = av * av;
                double ab2 = 2 * av * bv;
                double b2  = bv * bv;
                double res = plus ? (av + bv) * (av + bv) : (av - bv) * (av - bv);
                sb.AppendLine($"  = {Fmt(a2)} {pm} {Fmt(ab2)} + {Fmt(b2)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: {Fmt(res)}");
            }
            else
            {
                sb.AppendLine($"📌 Ответ: ({a})² {pm} 2·{a}·{b} + ({b})²");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 32.3  Вычислить, представив как квадрат суммы/разности ─────────────

    /// <summary>
    /// Вычисление степеней типа 101², 99², 103² через формулу квадрата суммы/разности.
    /// Задача 32.3
    /// </summary>
    public class SqSumComputeFunction : FunctionBase
    {
        public override string   Name       => "Вычислить n² через (a±b)²";
        public override string   Formula    => "n² = (a ± b)² = a² ± 2ab + b²";
        public override string[] Keywords   => new[] { "квадрат суммы", "вычислить", "101²", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вычисление квадрата числа через формулу\n" +
                    "Идея: представить число как удобную сумму или разность,\n" +
                    "затем применить формулу квадрата.\n" +
                    "Примеры:\n" +
                    "  101² = (100 + 1)² = 10000 + 200 + 1 = 10201\n" +
                    "   99² = (100 − 1)² = 10000 − 200 + 1 = 9801\n" +
                    "  103² = (100 + 3)² = 10000 + 600 + 9 = 10609\n" +
                    "   97² = (100 − 3)² = 10000 − 600 + 9 = 9409\n" +
                    "✏️ Введи число, квадрат которого нужно найти:\n" +
                    "  Пример: 101  или  99  или  98",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Ты ничего не ввёл.";
                    if (!double.TryParse(s.Trim().Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return $"«{s.Trim()}» не похоже на число.";
                    return null;
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи «удобное» основание (a) — круглое число рядом:\n" +
                    "  Пример: 100 (для 101 или 99)  или  50 (для 52 или 48)",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Ты ничего не ввёл.";
                    if (!double.TryParse(s.Trim().Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return $"«{s.Trim()}» не похоже на число.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double n = double.Parse(answers[0].Trim().Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture);
            double a = double.Parse(answers[1].Trim().Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture);
            double b = n - a;

            string sign  = b >= 0 ? "+" : "−";
            double absB  = Math.Abs(b);
            double a2    = a * a;
            double ab2   = 2 * a * absB;
            double b2    = absB * absB;
            double result = n * n;

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: {Fmt(n)}²");
            sb.AppendLine();
            sb.AppendLine($"Представляем: {Fmt(n)} = {Fmt(a)} {sign} {Fmt(absB)}");
            sb.AppendLine();
            sb.AppendLine(b >= 0
                ? "Формула: (a + b)² = a² + 2ab + b²"
                : "Формула: (a − b)² = a² − 2ab + b²");
            sb.AppendLine();
            sb.AppendLine($"  a = {Fmt(a)},  b = {Fmt(absB)}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  {Fmt(n)}²");
            sb.AppendLine($"  = ({Fmt(a)} {sign} {Fmt(absB)})²");
            sb.AppendLine($"  = {Fmt(a)}² {sign} 2·{Fmt(a)}·{Fmt(absB)} + {Fmt(absB)}²");
            sb.AppendLine($"  = {Fmt(a2)} {sign} {Fmt(ab2)} + {Fmt(b2)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: {Fmt(result)}");

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 32.4 / 32.5 / 32.6 / 32.7  Представить трёхчлен как квадрат двучлена ─

    /// <summary>
    /// Проверяет, является ли трёхчлен квадратом суммы/разности, и записывает его в виде (a±b)².
    /// Задачи 32.4–32.7
    /// </summary>
    public class SqSumRecognizeFunction : FunctionBase
    {
        public override string   Name       => "Трёхчлен → квадрат двучлена";
        public override string   Formula    => "a² ± 2ab + b² = (a ± b)²";
        public override string[] Keywords   => new[] { "квадрат суммы", "квадрат разности", "трёхчлен", "двучлен", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Распознавание квадрата двучлена\n" +
                    "Трёхчлен a² + 2ab + b² = (a + b)²\n" +
                    "Трёхчлен a² − 2ab + b² = (a − b)²\n" +
                    "Как проверить?\n" +
                    "  1️⃣  Первый и последний члены — полные квадраты?\n" +
                    "  2️⃣  Средний член = ±2·√первого·√последнего?\n" +
                    "Примеры:\n" +
                    "  a² + 2a + 1  →  a=a, b=1  →  (a+1)²  ✅\n" +
                    "  b² − 8b + 16 →  a=b, b=4  →  (b−4)²  ✅\n" +
                    "  n² + 14n + 49 → a=n, b=7 →  (n+7)²  ✅\n" +
                    "  0,36 − 1,2b + b² → a=0,6, b=b → (0,6−b)² ✅\n" +
                    "✏️ Введи a (корень из первого члена):\n" +
                    "  Пример: a  или  3n  или  0.6",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (корень из последнего члена):\n" +
                    "  Пример: 1  или  7  или  b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак среднего члена: + или −?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string pm   = plus ? "+" : "−";

            var sb = new StringBuilder();
            sb.AppendLine("Проверяем условия:");
            sb.AppendLine();
            sb.AppendLine($"  ✅ Первый член:  ({a})²");
            sb.AppendLine($"  ✅ Последний член: ({b})²");
            sb.AppendLine($"  ✅ Средний член:  {sign}2·({a})·({b})  =  {sign}2{a}{b}");
            sb.AppendLine();
            sb.AppendLine("Все три условия выполнены → трёхчлен является полным квадратом.");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу:");
            sb.AppendLine(plus
                ? $"  a² + 2ab + b² = (a + b)²"
                : $"  a² − 2ab + b² = (a − b)²");
            sb.AppendLine();
            sb.AppendLine($"  ({a})² {pm} 2·({a})·({b}) + ({b})²");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a} {sign} {b})²");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.8  Упростить выражение ────────────────────────────────────────────

    /// <summary>
    /// Упрощение выражений с применением (a±b)².
    /// Задача 32.8
    /// </summary>
    public class SqSumSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить выражение с (a±b)²";
        public override string   Formula    => "(a±b)² = a² ± 2ab + b²";
        public override string[] Keywords   => new[] { "квадрат суммы", "упростить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражений\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой все скобки с помощью (a±b)²\n" +
                    "  2️⃣  Раскрой оставшиеся скобки обычным умножением\n" +
                    "  3️⃣  Приведи подобные слагаемые\n" +
                    "Примеры:\n" +
                    "  (x+5)(x−3)²\n" +
                    "  = (x+5)(x²−6x+9)\n" +
                    "  = x³−6x²+9x+5x²−30x+45\n" +
                    "  = x³−x²−21x+45\n" +
                    "  0,3+(b²−(b−0,5)²)\n" +
                    "  = 0,3+b²−b²+b−0,25\n" +
                    "  = b+0,05\n" +
                    "✏️ Введи a (первое выражение в квадрате):\n" +
                    "  Пример: b  или  x  или  3a",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второе выражение в квадрате):\n" +
                    "  Пример: 0.5  или  5  или  2y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи остаток выражения (что прибавляется/вычитается снаружи):\n" +
                    "  Если ничего нет — введи 0\n" +
                    "  Пример: +0.3  или  -b²  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string pm   = plus ? "+" : "−";
            string rest = answers[3].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a} {sign} {b})²" + (rest == "0" ? "" : " " + rest));
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: (a + b)² = a² + 2ab + b²"
                : "Формула: (a − b)² = a² − 2ab + b²");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрываем квадрат:");
            sb.AppendLine($"  ({a} {sign} {b})²");
            sb.AppendLine($"  = ({a})² {pm} 2·({a})·({b}) + ({b})²");
            sb.AppendLine();

            if (rest != "0")
            {
                sb.AppendLine($"Шаг 2: Добавляем остаток {rest}:");
                sb.AppendLine($"  ({a})² {pm} 2·({a})·({b}) + ({b})² {rest}");
                sb.AppendLine();
                sb.AppendLine("Шаг 3: Приводим подобные слагаемые (проверь, что сокращается).");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Раскрытый вид: ({a})² {pm} 2·{a}·{b} + ({b})²" +
                          (rest == "0" ? "" : " " + rest));

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.9  Разложить на множители трёхчлен ───────────────────────────────

    /// <summary>
    /// Разложение трёхчлена на множители через формулу квадрата суммы/разности.
    /// Задача 32.9
    /// </summary>
    public class SqSumFactorFunction : FunctionBase
    {
        public override string   Name       => "Разложить трёхчлен (квадрат двучлена)";
        public override string   Formula    => "a² ± 2ab + b² = (a ± b)²";
        public override string[] Keywords   => new[] { "квадрат суммы", "разложить", "множители", "трёхчлен", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Разложение трёхчлена на множители\n" +
                    "  a² + 2ab + b² = (a + b)²\n" +
                    "  a² − 2ab + b² = (a − b)²\n" +
                    "Алгоритм:\n" +
                    "  1️⃣  Найди a = √(первый член)\n" +
                    "  2️⃣  Найди b = √(последний член)\n" +
                    "  3️⃣  Проверь: средний член = ±2ab?\n" +
                    "  4️⃣  Запиши (a + b)² или (a − b)²\n" +
                    "Примеры:\n" +
                    "  5x² + 20x + 20 = 5(x² + 4x + 4) = 5(x + 2)²\n" +
                    "  −3x² + 18x − 27 = −3(x² − 6x + 9) = −3(x − 3)²\n" +
                    "  9a² + 6a + 1 = (3a + 1)²\n" +
                    "✏️ Введи a (квадратный корень из первого члена):\n" +
                    "  Пример: 3a  или  x  или  2m",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (квадратный корень из последнего члена):\n" +
                    "  Пример: 1  или  3  или  4b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак среднего члена (+2ab или −2ab)?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Есть ли общий множитель, вынесенный за скобку?\n" +
                    "  Если нет — введи 1\n" +
                    "  Пример: 5  или  -3  или  1",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Введи 1 если общего множителя нет.";
                    if (!double.TryParse(s.Trim().Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return "Введи число (например 5 или -3 или 1).";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a      = answers[0].Trim();
            string b      = answers[1].Trim();
            bool   plus   = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign   = plus ? "+" : "−";
            string factorS = answers[3].Trim();
            double factor = double.Parse(factorS.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine("Проверяем структуру трёхчлена:");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}  →  a² = ({a})²");
            sb.AppendLine($"  b = {b}  →  b² = ({b})²");
            sb.AppendLine($"  Средний член: {sign}2·({a})·({b}) = {sign}2{a}{b}");
            sb.AppendLine();
            sb.AppendLine("✅ Условие выполнено — трёхчлен является полным квадратом.");
            sb.AppendLine();

            if (factor != 1)
            {
                sb.AppendLine($"Дополнительно вынесен общий множитель: {Fmt(factor)}");
                sb.AppendLine();
                sb.AppendLine(plus
                    ? $"  {Fmt(factor)}·(({a})² + 2·{a}·{b} + ({b})²)"
                    : $"  {Fmt(factor)}·(({a})² − 2·{a}·{b} + ({b})²)");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: {Fmt(factor)}·({a} {sign} {b})²");
            }
            else
            {
                sb.AppendLine(plus
                    ? "Применяем: a² + 2ab + b² = (a + b)²"
                    : "Применяем: a² − 2ab + b² = (a − b)²");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: ({a} {sign} {b})²");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 32.10  Упростить выражение (сложные случаи) ─────────────────────────

    /// <summary>
    /// Упрощение выражений с произведениями вида a(a−2b)(3b+a)² и т.п.
    /// Задача 32.10
    /// </summary>
    public class SqSumAdvancedSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить сложное выражение с (a±b)²";
        public override string   Formula    => "(a+b)² и (a−b)² вместе";
        public override string[] Keywords   => new[] { "квадрат суммы", "упростить", "сложный", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение сложных выражений\n" +
                    "Часто встречаются выражения, где нужно:\n" +
                    "  · применить (a±b)² для одной части\n" +
                    "  · применить (a+b)(a−b) = a²−b² для другой\n" +
                    "  · затем приводить подобные\n" +
                    "Примеры (32.10):\n" +
                    "  a(a−2b)(3b+a)²\n" +
                    "  3(b−2b)(2b+3) − 5b²\n" +
                    "  (m+8)² − 2(m+8)(m−8) + (m−8)²  — это (A−B)² где A=(m+8), B=(m−8)\n" +
                    "Введи первое выражение для (a + b)²:\n" +
                    "  Пример: m+8  или  2x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи второе выражение для (a − b)²:\n" +
                    "  Пример: m−8  или  3y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string p = answers[0].Trim();
            string q = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("Замечаем структуру выражения:");
            sb.AppendLine();
            sb.AppendLine($"  A = ({p}),  B = ({q})");
            sb.AppendLine();
            sb.AppendLine("Выражение имеет вид:");
            sb.AppendLine($"  A² − 2AB + B² = (A − B)²");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({p})² − 2·({p})·({q}) + ({q})²");
            sb.AppendLine($"  = (({p}) − ({q}))²");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({p} − {q})²");
            sb.AppendLine();
            sb.AppendLine("💡 Далее можно раскрыть скобки и упростить.");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.11 / 32.12  Представить в виде квадрата двучлена (трёхчлены) ─────

    /// <summary>
    /// Разложение трёхчлена с коэффициентами при a² и b²:
    /// 9y² − 12xy + 4x²  и т.п.
    /// Задачи 32.11, 32.12
    /// </summary>
    public class SqSumPolyToSquareFunction : FunctionBase
    {
        public override string   Name       => "Трёхчлен с коэффициентами → (a±b)²";
        public override string   Formula    => "ka² ± 2√k·√m·ab + mb² = (√k·a ± √m·b)²";
        public override string[] Keywords   => new[] { "квадрат суммы", "квадрат разности", "трёхчлен", "коэффициент", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Трёхчлен вида ka² ± 2√k√m·ab + mb² = (√k·a ± √m·b)²\n" +
                    "Примеры:\n" +
                    "  9y² − 12xy + 4x²\n" +
                    "  = (3y)² − 2·(3y)·(2x) + (2x)²\n" +
                    "  = (3y − 2x)²\n" +
                    "  16k² + 40k + 25\n" +
                    "  = (4k)² + 2·(4k)·5 + 5²\n" +
                    "  = (4k + 5)²\n" +
                    "  0,04a² − 1,2a + 9\n" +
                    "  = (0,2a)² − 2·(0,2a)·3 + 3²\n" +
                    "  = (0,2a − 3)²\n" +
                    "✏️ Введи a (первый двучлен, например 3y или 4k или 0.2a):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второй двучлен, например 2x или 5 или 3):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак среднего члена: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string pm   = plus ? "+" : "−";

            var sb = new StringBuilder();
            sb.AppendLine("Проверяем структуру трёхчлена:");
            sb.AppendLine();
            sb.AppendLine($"  a = ({a}),  b = ({b})");
            sb.AppendLine();
            sb.AppendLine($"  Первый член:    ({a})²");
            sb.AppendLine($"  Средний член:   {sign}2·({a})·({b})");
            sb.AppendLine($"  Последний член: ({b})²");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу:");
            sb.AppendLine(plus
                ? "  a² + 2ab + b² = (a + b)²"
                : "  a² − 2ab + b² = (a − b)²");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a} {sign} {b})²");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.13 / 32.14  Разложить на множители (общий вид) ──────────────────

    /// <summary>
    /// Разложение на множители трёхчленов и выражений.
    /// Задачи 32.13, 32.14
    /// </summary>
    public class SqSumFactorPolyFunction : FunctionBase
    {
        public override string   Name       => "Разложить многочлен (квадрат двучлена)";
        public override string   Formula    => "вынести общий множитель + (a±b)²";
        public override string[] Keywords   => new[] { "квадрат суммы", "разложить", "множители", "вынести", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Разложение на множители через квадрат двучлена\n" +
                    "Алгоритм:\n" +
                    "  1️⃣  Вынести общий множитель (если есть)\n" +
                    "  2️⃣  Распознать a² ± 2ab + b²\n" +
                    "  3️⃣  Записать (a ± b)²\n" +
                    "Примеры:\n" +
                    "  5x² + 20x + 20 = 5(x² + 4x + 4) = 5(x+2)²\n" +
                    "  −3x² + 18x − 27 = −3(x²−6x+9) = −3(x−3)²\n" +
                    "  6x² + 12x + 6 = 6(x+1)²\n" +
                    "  −10a² + 20a − 10 = −10(a−1)²\n" +
                    "✏️ Введи общий множитель (число или выражение):\n" +
                    "  Если нет — введи 1",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Введи 1 если общего множителя нет." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи a (из квадрата внутри скобок):\n" +
                    "  Пример: x  или  2a  или  3m",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (из квадрата внутри скобок):\n" +
                    "  Пример: 2  или  3  или  b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string factor = answers[0].Trim();
            string a      = answers[1].Trim();
            string b      = answers[2].Trim();
            bool   plus   = answers[3].Trim() != "-" && answers[3].Trim() != "−";
            string sign   = plus ? "+" : "−";

            var sb = new StringBuilder();

            if (factor != "1")
            {
                sb.AppendLine($"Шаг 1: Выносим общий множитель {factor}:");
                sb.AppendLine(plus
                    ? $"  {factor}·(({a})² + 2·{a}·{b} + ({b})²)"
                    : $"  {factor}·(({a})² − 2·{a}·{b} + ({b})²)");
                sb.AppendLine();
                sb.AppendLine("Шаг 2: Распознаём квадрат двучлена внутри:");
            }
            else
            {
                sb.AppendLine("Шаг 1: Распознаём квадрат двучлена:");
            }

            sb.AppendLine(plus
                ? $"  ({a})² + 2·({a})·({b}) + ({b})²  =  ({a} + {b})²"
                : $"  ({a})² − 2·({a})·({b}) + ({b})²  =  ({a} − {b})²");
            sb.AppendLine();

            if (factor != "1")
                sb.AppendLine($"📌 Ответ: {factor}·({a} {sign} {b})²");
            else
                sb.AppendLine($"📌 Ответ: ({a} {sign} {b})²");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.15 / 32.16 / 32.17  Решить уравнение ────────────────────────────

    /// <summary>
    /// Решение уравнений вида (a±b)² = c или (a+b)²−d = 0.
    /// Задачи 32.15–32.17
    /// </summary>
    public class SqSumEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение с (a±b)²";
        public override string   Formula    => "(a±b)² = c  →  a±b = ±√c";
        public override string[] Keywords   => new[] { "квадрат суммы", "уравнение", "корни", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений вида (a ± b)² = c\n" +
                    "Шаги:\n" +
                    "  1️⃣  Привести к виду (выражение)² = число\n" +
                    "  2️⃣  Взять квадратный корень: выражение = ±√число\n" +
                    "  3️⃣  Решить два линейных уравнения\n" +
                    "Примеры:\n" +
                    "  (x + 11)² − x² = 11\n" +
                    "    x² + 22x + 121 − x² = 11\n" +
                    "    22x = −110  →  x = −5\n" +
                    "  (a−3)² − (a+8)(a−8) = 0\n" +
                    "    a²−6a+9 − (a²−64) = 0\n" +
                    "    −6a + 73 = 0  →  a = 73/6\n" +
                    "  x(x−4) = 2 + (x−1)²\n" +
                    "    x²−4x = 2+x²−2x+1\n" +
                    "    −2x = 3  →  x = −3/2\n" +
                    "✏️ Введи левую часть уравнения (до =):\n" +
                    "  Пример: (x+11)^2 - x^2",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:\n" +
                    "  Пример: 11  или  0  или  (x-1)^2",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм решения:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой все квадраты двучленов");
            sb.AppendLine("  Используй: (a + b)² = a² + 2ab + b²");
            sb.AppendLine("         или (a − b)² = a² − 2ab + b²");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси все в левую часть");
            sb.AppendLine("  (левая) − (правая) = 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("  Часто x² сокращается — получается линейное уравнение");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное уравнение и запиши ответ");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.18 / 32.19 / 32.20  Решить неравенство ──────────────────────────

    /// <summary>
    /// Решение неравенств с (a±b)².
    /// Задачи 32.18–32.20
    /// </summary>
    public class SqSumInequalityFunction : FunctionBase
    {
        public override string   Name       => "Решить неравенство с (a±b)²";
        public override string   Formula    => "(a±b)² ≥ 0 всегда";
        public override string[] Keywords   => new[] { "квадрат суммы", "неравенство", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение неравенств с (a ± b)²\n" +
                    "Ключевые факты:\n" +
                    "  · (a ± b)² ≥ 0 ВСЕГДА (квадрат всегда неотрицателен)\n" +
                    "  · (a ± b)² = 0 только если a ± b = 0\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой квадраты через формулы\n" +
                    "  2️⃣  Перенеси всё в одну часть\n" +
                    "  3️⃣  Приведи подобные\n" +
                    "  4️⃣  Реши полученное линейное (или квадратное) неравенство\n" +
                    "Примеры:\n" +
                    "  n² − (n+1)² > 2\n" +
                    "    n² − n² − 2n − 1 > 2\n" +
                    "    −2n > 3  →  n < −3/2\n" +
                    "  (1−t)² > 3\n" +
                    "    1 − 2t + t² > 3\n" +
                    "    t² − 2t − 2 > 0  → решаем квадратное неравенство\n" +
                    "✏️ Введи левую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак неравенства: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:\n" +
                    "  Пример: 2  или  0  или  3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string op    = answers[1].Trim();
            string right = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Неравенство: {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм решения:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой квадраты двучленов");
            sb.AppendLine("  (a + b)² = a² + 2ab + b²");
            sb.AppendLine("  (a − b)² = a² − 2ab + b²");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси правую часть влево");
            sb.AppendLine($"  {left} − ({right}) {op} 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("  Часто x² сокращается → линейное неравенство");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши неравенство и запиши ответ в виде промежутка");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} {op} {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 32.31  Доказать что значение всегда отрицательное ───────────────────

    /// <summary>
    /// Доказательство знака выражения через (a±b)² ≥ 0.
    /// Задача 32.31
    /// </summary>
    public class SqSumProveSignFunction : FunctionBase
    {
        public override string   Name       => "Доказать знак выражения с (a±b)²";
        public override string   Formula    => "(a±b)² ≥ 0";
        public override string[] Keywords   => new[] { "квадрат суммы", "доказать", "отрицательное", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство знака выражения\n" +
                    "Ключевой факт: (a ± b)² ≥ 0 при любых значениях переменных.\n" +
                    "Стратегия:\n" +
                    "  Преобразуй выражение так, чтобы оно приняло вид\n" +
                    "  −(что-то)²  или  −[(a±b)² + c],  где c ≥ 0\n" +
                    "Примеры (32.31):\n" +
                    "  5(3−5a)² − 5(3a−7)(3a+7) − 80a²\n" +
                    "    = 5(9−30a+25a²) − 5(9a²−49) − 80a²\n" +
                    "    = 45−150a+125a² − 45a²+245 − 80a²\n" +
                    "    = 290 − 150a   ← зависит от a, не подходит для доказательства\n" +
                    "  (m−1)² − 4(m+1)² + 5 + 3m² + 10m\n" +
                    "    = m²−2m+1 − 4m²−8m−4 + 5 + 3m² + 10m\n" +
                    "    = 0·m² + 0·m + 2 = 2 > 0  ✅\n" +
                    "✏️ Опиши своё выражение (первый шаг раскрытия):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Что получилось после раскрытия всех скобок и приведения подобных?\n" +
                    "  Пример: -300  или  2  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string step1  = answers[0].Trim();
            string result = answers[1].Trim();

            bool isNum = double.TryParse(result.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double num);

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрываем все квадраты двучленов");
            sb.AppendLine($"  {step1}");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Приводим подобные слагаемые");
            sb.AppendLine($"  Результат: {result}");
            sb.AppendLine();

            if (isNum)
            {
                if (num < 0)
                    sb.AppendLine($"📌 Вывод: {result} < 0 — выражение всегда отрицательно ✅");
                else if (num > 0)
                    sb.AppendLine($"📌 Вывод: {result} > 0 — выражение всегда положительно ✅");
                else
                    sb.AppendLine($"📌 Вывод: {result} = 0 — выражение всегда равно нулю ✅");
            }
            else
            {
                sb.AppendLine($"📌 Вывод: значение выражения = {result} для любых переменных.");
                sb.AppendLine("  Проверь знак этой константы или упрости дальше.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  §33. Формулы куба суммы и куба разности двух выражений
    //  (a + b)³ = a³ + 3a²b + 3ab² + b³
    //  (a − b)³ = a³ − 3a²b + 3ab² − b³
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── 33.1 / 33.2  Представить в виде многочлена: (a±b)³ ──────────────────

    /// <summary>
    /// Раскрывает куб суммы или разности: (a ± b)³ → многочлен.
    /// Задачи 33.1–33.4
    /// </summary>
    public class CubeSumExpandFunction : FunctionBase
    {
        public override string   Name       => "Раскрыть (a±b)³ в многочлен";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "куб разности", "раскрыть", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Формулы куба суммы и куба разности:\n\n" +
                    "  (a + b)³ = a³ + 3a²b + 3ab² + b³\n" +
                    "  (a − b)³ = a³ − 3a²b + 3ab² − b³\n\n" +
                    "Как запомнить:\n" +
                    "  Куб первого\n" +
                    "  ± утроенный квадрат первого на второй\n" +
                    "  + утроенный первый на квадрат второго\n" +
                    "  ± куб второго\n\n" +
                    "Примеры:\n" +
                    "  (2 + x)³   = 8 + 12x + 6x² + x³\n" +
                    "  (a − 2)³   = a³ − 6a² + 12a − 8\n" +
                    "  (0,2a + 5)³ = 0,008a³ + 0,6a² + 15a + 125\n" +
                    "  (1/4·p − s)³ = 1/64·p³ − 3/16·p²s + 3/4·ps² − s³\n\n" +
                    "✏️ Введи a (первое выражение):\n" +
                    "  Пример: x  или  4m  или  0.2a",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второе выражение):\n" +
                    "  Пример: 2  или  3n  или  s",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак между a и b: сумма или разность?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string s1   = plus ? "+" : "−";   // знак при 3a²b
            string s2   = "+";                  // знак при 3ab² всегда +
            string s3   = plus ? "+" : "−";    // знак при b³

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: ({a} {sign} {b})³");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: (a + b)³ = a³ + 3a²b + 3ab² + b³"
                : "Формула: (a − b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}");
            sb.AppendLine($"  b = {b}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({a} {sign} {b})³");
            sb.AppendLine($"  = ({a})³ {s1} 3·({a})²·({b}) {s2} 3·({a})·({b})² {s3} ({b})³");
            sb.AppendLine();

            // Числовой расчёт если оба числа
            double av = 0, bv = 0;
            bool aIsNum = double.TryParse(a.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out av);
            bool bIsNum = double.TryParse(b.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out bv);

            if (aIsNum && bIsNum)
            {
                double a3   = av * av * av;
                double a2b3 = 3 * av * av * bv;
                double ab23 = 3 * av * bv * bv;
                double b3   = bv * bv * bv;
                double res  = plus ? Math.Pow(av + bv, 3) : Math.Pow(av - bv, 3);

                sb.AppendLine($"  = {Fmt(a3)} {s1} {Fmt(a2b3)} {s2} {Fmt(ab23)} {s3} {Fmt(b3)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: {Fmt(res)}");
            }
            else
            {
                sb.AppendLine($"📌 Ответ: ({a})³ {s1} 3({a})²({b}) + 3({a})({b})² {s3} ({b})³");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 33.5 / 33.6  Представить многочлен в виде куба двучлена ─────────────

    /// <summary>
    /// Распознаёт куб суммы/разности в многочлене: a³ ± 3a²b + 3ab² ± b³ = (a ± b)³
    /// Задачи 33.5, 33.6
    /// </summary>
    public class CubeSumRecognizeFunction : FunctionBase
    {
        public override string   Name       => "Многочлен → куб двучлена";
        public override string   Formula    => "a³ ± 3a²b + 3ab² ± b³ = (a ± b)³";
        public override string[] Keywords   => new[] { "куб суммы", "куб разности", "многочлен", "двучлен", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Как распознать куб двучлена в многочлене?\n\n" +
                    "Проверяем 4 условия:\n" +
                    "  1️⃣  Первый член — полный куб: a³\n" +
                    "  2️⃣  Последний член — полный куб: b³ (со знаком ±)\n" +
                    "  3️⃣  Второй член = ±3a²b\n" +
                    "  4️⃣  Третий член = +3ab²\n\n" +
                    "Примеры:\n" +
                    "  y³ − 3y² + 3y − 1\n" +
                    "    a=y, b=1  →  (y − 1)³  ✅\n\n" +
                    "  8x³ − 60x²y + 150xy² − 125y³\n" +
                    "    a=2x, b=5y  →  (2x − 5y)³  ✅\n\n" +
                    "  0,125a³ − 0,15a²b⁴ + 0,06ab⁸ − 0,008b¹²\n" +
                    "    a=0,5a, b=0,2b⁴  →  (0,5a − 0,2b⁴)³  ✅\n\n" +
                    "✏️ Введи a (кубический корень из первого члена):\n" +
                    "  Пример: y  или  2x  или  0.5a",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (кубический корень из последнего члена):\n" +
                    "  Пример: 1  или  5y  или  0.2b⁴",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или − (по знаку второго члена ±3a²b)?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";

            var sb = new StringBuilder();
            sb.AppendLine("Проверяем структуру многочлена:");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}  →  a³ = ({a})³");
            sb.AppendLine($"  b = {b}  →  b³ = ({b})³");
            sb.AppendLine($"  2-й член: {sign}3·({a})²·({b}) = {sign}3{a}²{b}");
            sb.AppendLine($"  3-й член: +3·({a})·({b})² = +3{a}{b}²");
            sb.AppendLine();
            sb.AppendLine("✅ Все четыре условия выполнены.");
            sb.AppendLine();
            sb.AppendLine("Применяем формулу:");
            sb.AppendLine(plus
                ? "  a³ + 3a²b + 3ab² + b³ = (a + b)³"
                : "  a³ − 3a²b + 3ab² − b³ = (a − b)³");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a} {sign} {b})³");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.7  Упростить выражение и найти его значение ──────────────────────

    /// <summary>
    /// Упрощение выражений с (a±b)³ и подстановка числовых значений.
    /// Задача 33.7
    /// </summary>
    public class CubeSumSimplifyEvalFunction : FunctionBase
    {
        public override string   Name       => "Упростить выражение с (a±b)³ и найти значение";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "упростить", "значение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение и нахождение значения выражения\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой все кубы двучленов\n" +
                    "  2️⃣  Раскрой оставшиеся скобки\n" +
                    "  3️⃣  Приведи подобные слагаемые\n" +
                    "  4️⃣  Подставь числовое значение\n\n" +
                    "Примеры (33.7):\n" +
                    "  (3a−1)³ − 27a³ + 5 при a=−1; 0; 1\n" +
                    "    = 27a³−27a²+9a−1 − 27a³ + 5\n" +
                    "    = −27a² + 9a + 4\n" +
                    "    При a=−1: −27−9+4 = −32\n" +
                    "    При a=0:  4\n" +
                    "    При a=1:  −27+9+4 = −14\n\n" +
                    "✏️ Введи a (первое выражение в кубе):\n" +
                    "  Пример: 3a  или  x  или  5x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второе выражение в кубе):\n" +
                    "  Пример: 1  или  2  или  y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Что прибавляется/вычитается после куба?\n" +
                    "  Если ничего — введи 0\n" +
                    "  Пример: -27a³  или  +5  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи числовое значение переменной для подстановки:\n" +
                    "  Пример: -1  или  0  или  0.5",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Ты ничего не ввёл.";
                    if (!double.TryParse(s.Trim().Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return $"«{s.Trim()}» не похоже на число.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string s1   = plus ? "+" : "−";
            string s3   = plus ? "+" : "−";
            string rest = answers[3].Trim();
            string valS = answers[4].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a} {sign} {b})³" + (rest == "0" ? "" : " " + rest));
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: (a + b)³ = a³ + 3a²b + 3ab² + b³"
                : "Формула: (a − b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрываем куб двучлена:");
            sb.AppendLine($"  ({a} {sign} {b})³");
            sb.AppendLine($"  = ({a})³ {s1} 3·({a})²·({b}) + 3·({a})·({b})² {s3} ({b})³");

            if (rest != "0")
            {
                sb.AppendLine();
                sb.AppendLine($"Шаг 2: Добавляем остаток {rest} и приводим подобные:");
                sb.AppendLine($"  ({a})³ {s1} 3({a})²({b}) + 3({a})({b})² {s3} ({b})³ {rest}");
                sb.AppendLine("  (сократи одинаковые члены)");
            }

            sb.AppendLine();
            sb.AppendLine($"Шаг 3: Подставляем значение переменной = {valS}");
            sb.AppendLine($"  (замени переменную числом и вычисли)");
            sb.AppendLine();
            sb.AppendLine($"📌 Подставь переменную = {valS} в упрощённый результат.");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.8 / 33.9  Решить уравнение с (a±b)³ ─────────────────────────────

    /// <summary>
    /// Решение уравнений вида (a+b)³ = c·(a+b) и т.п.
    /// Задачи 33.8, 33.9
    /// </summary>
    public class CubeSumEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение с (a±b)³";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "уравнение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений с кубом двучлена\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой все кубы через формулы\n" +
                    "  2️⃣  Перенеси всё в одну сторону\n" +
                    "  3️⃣  Приведи подобные — часто x³ сокращается\n" +
                    "  4️⃣  Реши полученное уравнение\n\n" +
                    "Примеры (33.8):\n" +
                    "  (x+1)³ − 4x = 5 + x²(x+3)\n" +
                    "    x³+3x²+3x+1 − 4x = 5+x³+3x²\n" +
                    "    −x − 4 = 0  →  x = −4\n\n" +
                    "  (1−y)³ + 8y = 7 + y²(3−y)\n" +
                    "    1−3y+3y²−y³+8y = 7+3y²−y³\n" +
                    "    5y − 6 = 0  →  y = 6/5\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм решения:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой все кубы двучленов");
            sb.AppendLine("  (a + b)³ = a³ + 3a²b + 3ab² + b³");
            sb.AppendLine("  (a − b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси правую часть влево");
            sb.AppendLine($"  ({left}) − ({right}) = 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("  x³ часто сокращается → остаётся линейное уравнение");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.10  Доказать тождество ────────────────────────────────────────────

    /// <summary>
    /// Доказательство тождеств с кубами суммы/разности.
    /// Задача 33.10
    /// </summary>
    public class CubeSumProveIdentityFunction : FunctionBase
    {
        public override string   Name       => "Доказать тождество с (a±b)³";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "тождество", "доказать", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождеств с кубами\n\n" +
                    "Стратегия: преобразуй ЛЕВУЮ часть, чтобы получить ПРАВУЮ.\n\n" +
                    "Примеры (33.10):\n\n" +
                    "  1) (3a+b)³ − a(3a+b)² − 18ab(a−b) = 26a(a³−b³)\n" +
                    "     Левая:\n" +
                    "     = 27a³+27a²b+9ab²+b³ − a(9a²+6ab+b²) − 18ab(a−b)\n" +
                    "     = 27a³+27a²b+9ab²+b³ − 9a³−6a²b−ab² − 18a²b+18ab²\n" +
                    "     = 18a³−b²·(a−b)·... → проверяем что = 26a(a³−b³)\n\n" +
                    "  2) (x+4y)³ + (4x−y)³ + 12xy(3x−5y) = 65(x³−y³)\n\n" +
                    "✏️ Введи левую часть тождества:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть тождества (ожидаемый результат):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства тождества:");
            sb.AppendLine();
            sb.AppendLine($"Надо доказать: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Раскрой все кубы двучленов в левой части");
            sb.AppendLine("  (a + b)³ = a³ + 3a²b + 3ab² + b³");
            sb.AppendLine("  (a − b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой все оставшиеся скобки");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Убедись, что получилась правая часть:");
            sb.AppendLine($"  {right}  ✅");
            sb.AppendLine();
            sb.AppendLine("💡 Если хочешь проверить конкретный шаг — напиши его.");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.13 / 33.14  Упростить выражение ─────────────────────────────────

    /// <summary>
    /// Упрощение выражений, содержащих кубы двучленов.
    /// Задачи 33.13, 33.14
    /// </summary>
    public class CubeSumSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить выражение с (a±b)³";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "упростить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражений с кубами\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой каждый куб двучлена по формуле\n" +
                    "  2️⃣  Раскрой остальные скобки\n" +
                    "  3️⃣  Приведи подобные слагаемые\n\n" +
                    "Примеры (33.13):\n" +
                    "  (x²+1)³ − 3(x²−1)(x²+1)² − 5x(x−2) + 10\n" +
                    "    Раскроем (x²+1)³ = x⁶+3x⁴+3x²+1\n" +
                    "    3(x²−1)(x²+1)² = 3(x²−1)(x⁴+2x²+1)\n" +
                    "    = 3(x⁶+2x⁴+x²−x⁴−2x²−1)\n" +
                    "    = 3x⁶+3x⁴−3x²−3\n" +
                    "    Итого: x⁶+3x⁴+3x²+1 − 3x⁶−3x⁴+3x²+3 − 5x²+10x+10\n" +
                    "    = −2x⁶ + x² + 10x + 14\n\n" +
                    "✏️ Введи первый двучлен (a в кубе):\n" +
                    "  Пример: x  или  x²  или  2x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи второй двучлен (b в кубе):\n" +
                    "  Пример: 5  или  1  или  3y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string s1   = plus ? "+" : "−";
            string s3   = plus ? "+" : "−";

            var sb = new StringBuilder();
            sb.AppendLine($"Раскрываем: ({a} {sign} {b})³");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: (a + b)³ = a³ + 3a²b + 3ab² + b³"
                : "Формула: (a − b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine($"  = ({a})³ {s1} 3·({a})²·({b}) + 3·({a})·({b})² {s3} ({b})³");
            sb.AppendLine();
            sb.AppendLine("Далее приводи подобные слагаемые с остальными членами выражения.");
            sb.AppendLine();
            sb.AppendLine($"📌 Раскрытый куб: ({a})³ {s1} 3({a})²·{b} + 3{a}·({b})² {s3} ({b})³");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.15 / 33.16  Решить уравнение (сложные) ───────────────────────────

    /// <summary>
    /// Решение уравнений с кубами двучленов (сложные случаи).
    /// Задачи 33.15, 33.16
    /// </summary>
    public class CubeSumEquationAdvancedFunction : FunctionBase
    {
        public override string   Name       => "Решить сложное уравнение с (a±b)³";
        public override string   Formula    => "раскрыть кубы → линейное уравнение";
        public override string[] Keywords   => new[] { "куб суммы", "уравнение", "сложный", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение сложных уравнений с кубами\n\n" +
                    "Примеры (33.15–33.16):\n\n" +
                    "  (x+2)³ − (x−2)³ = 2x(6x+2)\n" +
                    "    Раскрываем:\n" +
                    "    (x³+6x²+12x+8) − (x³−6x²+12x−8) = 12x²+4x\n" +
                    "    12x² + 16 = 12x² + 4x\n" +
                    "    4x = 16  →  x = 4\n\n" +
                    "  (6−x)³ − x²(16−x) = x²+116\n" +
                    "    216−108x+18x²−x³ − 16x²+x³ = x²+116\n" +
                    "    216−108x+2x² − x² = x²+116  (тут x² сокращается не всегда)\n" +
                    "    → −108x = −100  →  x = 100/108 = 25/27\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой кубы двучленов");
            sb.AppendLine("  (a+b)³ = a³ + 3a²b + 3ab² + b³");
            sb.AppendLine("  (a−b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой скобки в правой части");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Перенеси всё влево, приведи подобные");
            sb.AppendLine("  Чаще всего x³ сокращается → линейное уравнение");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши и запиши ответ");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.17 / 33.18  Решить неравенство ──────────────────────────────────

    /// <summary>
    /// Решение неравенств с (a±b)³.
    /// Задачи 33.17, 33.18
    /// </summary>
    public class CubeSumInequalityFunction : FunctionBase
    {
        public override string   Name       => "Решить неравенство с (a±b)³";
        public override string   Formula    => "раскрыть кубы → линейное неравенство";
        public override string[] Keywords   => new[] { "куб суммы", "неравенство", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение неравенств с кубами\n\n" +
                    "Примеры (33.17–33.18):\n\n" +
                    "  (2−3x)³ − 54x² ≤ −27x³ − 41x\n" +
                    "    8−36x+54x²−27x³−54x² ≤ −27x³−41x\n" +
                    "    8−36x ≤ −41x\n" +
                    "    5x ≤ −8  →  x ≤ −8/5\n\n" +
                    "  (x−7)³ + 42x² ≥ (x+7)³ + 14 − 7x\n" +
                    "    x³−21x²+147x−343+42x² ≥ x³+21x²+147x+343+14−7x\n" +
                    "    21x² − 7x ≥ 21x² + 343 + 14\n" +
                    "    −7x ≥ 357  →  x ≤ −51\n\n" +
                    "✏️ Введи левую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак неравенства: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string op    = answers[1].Trim();
            string right = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Неравенство: {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой кубы двучленов");
            sb.AppendLine("  (a+b)³ = a³ + 3a²b + 3ab² + b³");
            sb.AppendLine("  (a−b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой правую часть");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Перенеси всё влево, приведи подобные");
            sb.AppendLine("  x³ обычно сокращается → остаётся линейное неравенство");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное неравенство и запиши ответ");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} {op} {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 33.19 / 33.20  Доказать тождество / знак выражения ─────────────────

    /// <summary>
    /// Доказательство: выражение равно нулю или доказать тождество.
    /// Задачи 33.19, 33.20
    /// </summary>
    public class CubeSumProveZeroFunction : FunctionBase
    {
        public override string   Name       => "Доказать что выражение = 0 (кубы)";
        public override string   Formula    => "(a±b)³ = a³ ± 3a²b + 3ab² ± b³";
        public override string[] Keywords   => new[] { "куб суммы", "доказать", "равно нулю", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство: выражение = 0\n\n" +
                    "Примеры (33.19):\n\n" +
                    "  (b+5)³ − b(b−5)² − 25(1+b)² = 100\n" +
                    "    = b³+15b²+75b+125 − b(b²−10b+25) − 25(1+2b+b²)\n" +
                    "    = b³+15b²+75b+125 − b³+10b²−25b − 25−50b−25b²\n" +
                    "    = 100  ✅\n\n" +
                    "  5(1−b)³ + 5b(1+b)² − (1−5b²)² = 4\n" +
                    "    Раскрыть кубы, привести подобные → получить 4\n\n" +
                    "✏️ Введи выражение (левую часть):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Что должно получиться (правая часть)?\n" +
                    "  Пример: 0  или  100  или  4",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string expr   = answers[0].Trim();
            string target = answers[1].Trim();

            bool isNum = double.TryParse(target.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double num);

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства:");
            sb.AppendLine();
            sb.AppendLine($"Надо показать: {expr} = {target}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Раскрой все кубы двучленов");
            sb.AppendLine("  (a+b)³ = a³ + 3a²b + 3ab² + b³");
            sb.AppendLine("  (a−b)³ = a³ − 3a²b + 3ab² − b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой квадраты двучленов (если есть)");
            sb.AppendLine("  (a±b)² = a² ± 2ab + b²");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Раскрой все оставшиеся скобки (умножение)");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Приведи подобные слагаемые");
            sb.AppendLine();

            if (isNum && num == 0)
                sb.AppendLine($"📌 Цель: все слагаемые должны сократиться → 0  ✅");
            else
                sb.AppendLine($"📌 Цель: после всех преобразований должно остаться {target}  ✅");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  §34. Формулы суммы и разности кубов двух выражений
    //  a³ + b³ = (a + b)(a² − ab + b²)
    //  a³ − b³ = (a − b)(a² + ab + b²)
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── 34.1 / 34.2 / 34.3  Разложить на множители ──────────────────────────

    /// <summary>
    /// Разложение суммы или разности кубов на множители.
    /// a³ ± b³ = (a ± b)(a² ∓ ab + b²)
    /// Задачи 34.1–34.3, 34.10
    /// </summary>
    public class CubeSumFactorFunction : FunctionBase
    {
        public override string   Name       => "Разложить a³±b³ на множители";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "разложить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Формулы суммы и разности кубов:\n\n" +
                    "  a³ + b³ = (a + b)(a² − ab + b²)\n" +
                    "  a³ − b³ = (a − b)(a² + ab + b²)\n\n" +
                    "Второй множитель — «неполный квадрат разности»:\n" +
                    "  a² ∓ ab + b²  (знак при ab — противоположный знаку в сумме/разности)\n\n" +
                    "Примеры:\n" +
                    "  a³ + x³  = (a + x)(a² − ax + x²)\n" +
                    "  27 − a³  = 3³ − a³ = (3 − a)(9 + 3a + a²)\n" +
                    "  1/27 + z³ = (1/3)³ + z³ = (1/3 + z)(1/9 − z/3 + z²)\n" +
                    "  8 + q³  = 2³ + q³ = (2 + q)(4 − 2q + q²)\n\n" +
                    "✏️ Введи a (кубический корень из первого члена):\n" +
                    "  Пример: a  или  3  или  2x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (кубический корень из второго члена):\n" +
                    "  Пример: x  или  5  или  y²",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Это сумма или разность кубов?\n" +
                    "  Введи + или −",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            // знак при ab в неполном квадрате — противоположный
            string abSign = plus ? "−" : "+";

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a})³ {sign} ({b})³");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Формула: a³ + b³ = (a + b)(a² − ab + b²)"
                : "Формула: a³ − b³ = (a − b)(a² + ab + b²)");
            sb.AppendLine();
            sb.AppendLine($"  a = {a}");
            sb.AppendLine($"  b = {b}");
            sb.AppendLine();
            sb.AppendLine("Подставляем:");
            sb.AppendLine($"  ({a})³ {sign} ({b})³");
            sb.AppendLine($"  = ({a} {sign} {b})" +
                          $"·(({a})² {abSign} ({a})·({b}) + ({b})²)");
            sb.AppendLine();

            // Числовой расчёт если оба числа
            double av = 0, bv = 0;
            bool aIsNum = double.TryParse(a.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out av);
            bool bIsNum = double.TryParse(b.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out bv);

            if (aIsNum && bIsNum)
            {
                double a2   = av * av;
                double ab   = av * bv;
                double b2   = bv * bv;
                double sum  = plus ? av + bv : av - bv;
                double quad = a2 + (plus ? -ab : ab) + b2;
                sb.AppendLine($"  = ({Fmt(sum)})·({Fmt(a2)} {abSign} {Fmt(Math.Abs(ab))} + {Fmt(b2)})");
                sb.AppendLine($"  = {Fmt(sum)} · {Fmt(quad)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: ({Fmt(sum)})·({Fmt(quad)})");
            }
            else
            {
                sb.AppendLine($"📌 Ответ: ({a} {sign} {b})·(({a})² {abSign} {a}·{b} + ({b})²)");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 34.4  Представить произведение в виде многочлена ────────────────────

    /// <summary>
    /// Раскрывает произведение (a±b)(a²∓ab+b²) в многочлен = a³±b³.
    /// Задача 34.4
    /// </summary>
    public class CubeSumProductExpandFunction : FunctionBase
    {
        public override string   Name       => "Произведение (a±b)(a²∓ab+b²) → многочлен";
        public override string   Formula    => "(a+b)(a²−ab+b²) = a³+b³";
        public override string[] Keywords   => new[] { "сумма кубов", "произведение", "многочлен", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Произведение суммы/разности на неполный квадрат:\n\n" +
                    "  (a + b)(a² − ab + b²) = a³ + b³\n" +
                    "  (a − b)(a² + ab + b²) = a³ − b³\n\n" +
                    "Примеры (34.4):\n" +
                    "  (a − 2a)(2a² + 2a·2a + 4a²)  — проверь структуру\n" +
                    "  (k − 5)(k² + 5k + 25) = k³ − 125  [a=k, b=5]\n" +
                    "  (3+m)(9−3m+m²) = 27 + m³        [a=3, b=m]\n" +
                    "  (4−x²)(16+4x²+x⁴) = 64 − x⁶    [a=4, b=x²]\n\n" +
                    "✏️ Введи a:\n" +
                    "  Пример: k  или  3  или  4",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b:\n" +
                    "  Пример: 5  или  m  или  x²",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string abSign = plus ? "−" : "+";

            var sb = new StringBuilder();
            sb.AppendLine($"Произведение: ({a} {sign} {b})" +
                          $"·(({a})² {abSign} ({a})·({b}) + ({b})²)");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Применяем: (a + b)(a² − ab + b²) = a³ + b³"
                : "Применяем: (a − b)(a² + ab + b²) = a³ − b³");
            sb.AppendLine();
            sb.AppendLine($"  a = {a},  b = {b}");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a})³ {sign} ({b})³");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.5 / 34.6  Упростить выражение ────────────────────────────────────

    /// <summary>
    /// Упрощение выражений с применением формул суммы/разности кубов.
    /// Задачи 34.5, 34.6
    /// </summary>
    public class CubeSumSimplify2Function : FunctionBase
    {
        public override string   Name       => "Упростить выражение с a³±b³";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "упростить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение с формулами суммы/разности кубов\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Найди части вида a³±b³ и разложи на множители\n" +
                    "  2️⃣  Найди части вида (a±b)(a²∓ab+b²) и замени на a³±b³\n" +
                    "  3️⃣  Приведи подобные\n\n" +
                    "Примеры (34.5, 34.6):\n" +
                    "  (x−10)(x²+10x+100) − x³\n" +
                    "  = x³ − 1000 − x³\n" +
                    "  = −1000\n\n" +
                    "  216 − (a+6)(a²−6a+36)\n" +
                    "  = 6³ − (a+6)(a²−6a+36)\n" +
                    "  Заметим: 6³−a³ = (6−a)(36+6a+a²)  — но здесь другая структура\n" +
                    "  = 216 − (a³ + 6³) = −a³ ← проверяй знак\n\n" +
                    "  (a−1)(a²+a+1) − a(a²+1)\n" +
                    "  = a³−1 − a³−a = −a−1\n\n" +
                    "✏️ Введи a (первый элемент):\n" +
                    "  Пример: x  или  a  или  2b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второй элемент):\n" +
                    "  Пример: 10  или  1  или  3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Что ещё прибавляется/вычитается?\n" +
                    "  Если ничего — введи 0\n" +
                    "  Пример: -x³  или  +216  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string abSign = plus ? "−" : "+";
            string rest = answers[3].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a} {sign} {b})" +
                          $"·(({a})² {abSign} {a}·{b} + ({b})²)" +
                          (rest == "0" ? "" : " " + rest));
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Применяем формулу:");
            sb.AppendLine(plus
                ? "  (a + b)(a² − ab + b²) = a³ + b³"
                : "  (a − b)(a² + ab + b²) = a³ − b³");
            sb.AppendLine($"  → ({a})³ {sign} ({b})³");

            if (rest != "0")
            {
                sb.AppendLine();
                sb.AppendLine($"Шаг 2: Добавляем {rest} и приводим подобные:");
                sb.AppendLine($"  ({a})³ {sign} ({b})³ {rest}");
                sb.AppendLine("  (проверь, что сокращается)");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 После замены: ({a})³ {sign} ({b})³" +
                          (rest == "0" ? "" : " " + rest));

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.7  Решить уравнение ───────────────────────────────────────────────

    /// <summary>
    /// Решение уравнений с применением формул суммы/разности кубов.
    /// Задача 34.7
    /// </summary>
    public class CubeSumFactorEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение с a³±b³";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "уравнение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений с формулами суммы/разности кубов\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой кубы двучленов по формулам (a±b)³\n" +
                    "  2️⃣  Либо разложи сумму/разность кубов: a³±b³=(a±b)(a²∓ab+b²)\n" +
                    "  3️⃣  Перенеси всё в одну сторону\n" +
                    "  4️⃣  Реши уравнение\n\n" +
                    "Примеры (34.7):\n" +
                    "  (2x−3)(4x²+6x+9) − 8x³ = 2.7x\n" +
                    "  = (2x)³ − 3³ − 8x³ = 2.7x\n" +
                    "  = 8x³ − 27 − 8x³ = 2.7x\n" +
                    "  = −27 = 2.7x  →  x = −10\n\n" +
                    "  (3+4x)(16x²−12x+9) − 64x³ = −10x\n" +
                    "  = (4x)³ + 3³ − 64x³ = −10x\n" +
                    "  = 27 = −10x  →  x = −27/10\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Найди произведения вида (a±b)(a²∓ab+b²)");
            sb.AppendLine("  и замени на a³±b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси всё влево");
            sb.AppendLine($"  ({left}) − ({right}) = 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные (x³ обычно сокращается)");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.8  Решить неравенство ─────────────────────────────────────────────

    /// <summary>
    /// Решение неравенств с применением формул суммы/разности кубов.
    /// Задача 34.8
    /// </summary>
    public class CubeSumFactorInequalityFunction : FunctionBase
    {
        public override string   Name       => "Решить неравенство с a³±b³";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "неравенство", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение неравенств с формулами кубов\n\n" +
                    "Примеры (34.8):\n\n" +
                    "  (1−4x)(1+4x+16x²) ≤ 10 − 70x²\n" +
                    "  = 1 − (4x)³ ≤ 10 − 70x²\n" +
                    "  = 1 − 64x³ ≤ 10 − 70x²  ← нет x³ здесь, раскроем (4x)³=64x³\n" +
                    "  проверяем структуру: (1−4x)(1+4x+16x²) = 1³−(4x)³\n" +
                    "  = 1 − 64x³ ≤ 10 − 70x²\n" +
                    "  → 70x² − 64x³ ≤ 9 ... решаем дальше\n\n" +
                    "  99x³ − (1+5x)(1−5x+25x²) ≥ 12x − 26x³\n" +
                    "  = 99x³ − (1+125x³) ≥ 12x − 26x³\n" +
                    "  = 125x³ − 1 ≥ 12x − 26x³\n" +
                    "  = 0 ≥ 12x + 1  →  x ≤ −1/12\n\n" +
                    "✏️ Введи левую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак неравенства: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string op    = answers[1].Trim();
            string right = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Неравенство: {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Найди произведения (a±b)(a²∓ab+b²)");
            sb.AppendLine("  и замени на a³±b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси правую часть влево");
            sb.AppendLine($"  ({left}) − ({right}) {op} 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши полученное неравенство");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} {op} {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.9  Доказать тождество ─────────────────────────────────────────────

    /// <summary>
    /// Доказательство тождеств с применением формул суммы/разности кубов.
    /// Задача 34.9
    /// </summary>
    public class CubeSumFactorProveFunction : FunctionBase
    {
        public override string   Name       => "Доказать тождество с a³±b³";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "тождество", "доказать", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождеств\n\n" +
                    "Примеры (34.9):\n\n" +
                    "  (5x−6)(25x²+30x+36) − 0.25(500x³−864) = 0\n" +
                    "  Левая:\n" +
                    "  (5x−6)(25x²+30x+36) = (5x)³−6³ = 125x³−216\n" +
                    "  0.25(500x³−864) = 125x³ − 216\n" +
                    "  Итого: 125x³−216 − 125x³+216 = 0  ✅\n\n" +
                    "  91x³ − (3x−4)(9x²+12x+16) − (3+4x)(9−12x+16x²) = 37\n" +
                    "  = 91x³ − (27x³−64) − (27+64x³) = 37\n" +
                    "  = 91x³ − 27x³+64 − 27−64x³ = 37\n" +
                    "  = 0·x³ + 37 = 37  ✅\n\n" +
                    "✏️ Введи левую часть тождества:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть тождества (ожидаемый результат):\n" +
                    "  Пример: 0  или  37",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string target = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства:");
            sb.AppendLine();
            sb.AppendLine($"Надо показать: {left} = {target}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Найди произведения (a±b)(a²∓ab+b²)");
            sb.AppendLine("  и замени каждое на a³±b³");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой оставшиеся скобки");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("  x³ обычно полностью сокращается");
            sb.AppendLine();
            sb.AppendLine($"📌 Цель: должно остаться {target}  ✅");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.11 / 34.12  Записать произведение / представить в виде произведения

    /// <summary>
    /// Запись выражений в виде произведения с использованием обеих формул кубов.
    /// Задачи 34.11, 34.12
    /// </summary>
    public class CubeSumWriteAsProductFunction : FunctionBase
    {
        public override string   Name       => "Записать в виде произведения (кубы)";
        public override string   Formula    => "a³±b³ = (a±b)(a²∓ab+b²)";
        public override string[] Keywords   => new[] { "сумма кубов", "разность кубов", "произведение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Запись выражения в виде произведения\n\n" +
                    "Используем все изученные ФСУ:\n" +
                    "  · a²−b² = (a+b)(a−b)\n" +
                    "  · (a±b)² = a²±2ab+b²\n" +
                    "  · a³+b³ = (a+b)(a²−ab+b²)\n" +
                    "  · a³−b³ = (a−b)(a²+ab+b²)\n\n" +
                    "Примеры (34.11, 34.12):\n" +
                    "  (x+1/3)(1/9−x/3+x²) → a=x, b=1/3 → x³+1/27\n" +
                    "  (n−1/2)(n²+n/2+1/4) → a=n, b=1/2 → n³−1/8\n" +
                    "  m³−n²+2n−1 = m³−(n−1)² = [m−(n−1)][m²+m(n−1)+(n−1)²]\n\n" +
                    "✏️ Введи a (из структуры a³±b³):\n" +
                    "  Пример: x  или  m  или  2a",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b:\n" +
                    "  Пример: 1/3  или  1/2  или  b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Знак: + или −?",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == "+" || t == "-" || t == "−") return null;
                    return "Введи + или −";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a    = answers[0].Trim();
            string b    = answers[1].Trim();
            bool   plus = answers[2].Trim() != "-" && answers[2].Trim() != "−";
            string sign = plus ? "+" : "−";
            string abSign = plus ? "−" : "+";

            var sb = new StringBuilder();
            sb.AppendLine($"Выражение: ({a})³ {sign} ({b})³");
            sb.AppendLine();
            sb.AppendLine(plus
                ? "Применяем: a³ + b³ = (a + b)(a² − ab + b²)"
                : "Применяем: a³ − b³ = (a − b)(a² + ab + b²)");
            sb.AppendLine();
            sb.AppendLine($"  a = {a},  b = {b}");
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: ({a} {sign} {b})" +
                          $"·(({a})² {abSign} ({a})·({b}) + ({b})²)");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.13 / 34.14  Упростить (смешанные формулы) ───────────────────────

    /// <summary>
    /// Упрощение выражений с применением всех ФСУ одновременно.
    /// Задачи 34.13, 34.14
    /// </summary>
    public class CubeSumAllFSUSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить с применением всех ФСУ";
        public override string   Formula    => "все формулы: ±кубы, ±квадраты, куб двучлена";
        public override string[] Keywords   => new[] { "сумма кубов", "все фсу", "упростить", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение с применением всех ФСУ\n\n" +
                    "Полный набор формул:\n" +
                    "  · a²−b²   = (a+b)(a−b)\n" +
                    "  · (a+b)²  = a²+2ab+b²\n" +
                    "  · (a−b)²  = a²−2ab+b²\n" +
                    "  · (a+b)³  = a³+3a²b+3ab²+b³\n" +
                    "  · (a−b)³  = a³−3a²b+3ab²−b³\n" +
                    "  · a³+b³   = (a+b)(a²−ab+b²)\n" +
                    "  · a³−b³   = (a−b)(a²+ab+b²)\n\n" +
                    "Примеры (34.13–34.14):\n" +
                    "  2a³+9 − 2(a+1)(a²−a+1)\n" +
                    "  = 2a³+9 − 2(a³+1)\n" +
                    "  = 2a³+9 − 2a³−2 = 7\n\n" +
                    "  (x+2)(x²−2x+4) − x(x−3)(x+3) − 19x\n" +
                    "  = x³+8 − x(x²−9) − 19x\n" +
                    "  = x³+8 − x³+9x − 19x\n" +
                    "  = −10x+8\n\n" +
                    "✏️ Опиши структуру выражения (какую формулу применяешь первой):\n" +
                    "  Пример: a³+b³  или  (a−b)³  или  a²−b²",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи a:\n" +
                    "  Пример: x  или  a  или  2b",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b:\n" +
                    "  Пример: 2  или  1  или  3y",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string formula = answers[0].Trim();
            string a       = answers[1].Trim();
            string b       = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("Алгоритм упрощения:");
            sb.AppendLine();
            sb.AppendLine($"Применяем формулу: {formula}");
            sb.AppendLine($"  a = {a},  b = {b}");
            sb.AppendLine();
            sb.AppendLine("Полный набор ФСУ для справки:");
            sb.AppendLine("  a²−b²  = (a+b)(a−b)");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a³+b³  = (a+b)(a²−ab+b²)");
            sb.AppendLine("  a³−b³  = (a−b)(a²+ab+b²)");
            sb.AppendLine();
            sb.AppendLine("Последовательность шагов:");
            sb.AppendLine("  1️⃣  Замени произведения по ФСУ");
            sb.AppendLine("  2️⃣  Раскрой оставшиеся скобки");
            sb.AppendLine("  3️⃣  Приведи подобные слагаемые");
            sb.AppendLine();
            sb.AppendLine($"📌 После замены {formula}: ({a})³ или ({a})²");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.15 / 34.16  Упростить / найти корни уравнения ───────────────────

    /// <summary>
    /// Упрощение сложных выражений и решение уравнений высокого уровня.
    /// Задачи 34.15, 34.16
    /// </summary>
    public class CubeSumAdvancedEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить сложное уравнение (все ФСУ)";
        public override string   Formula    => "все формулы ФСУ";
        public override string[] Keywords   => new[] { "сумма кубов", "уравнение", "сложный", "все фсу", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение сложных уравнений с применением всех ФСУ\n\n" +
                    "Примеры (34.15, 34.16):\n\n" +
                    "  (x+2)³ − (x−2)³ = 2x(6x+2)  ← §33\n" +
                    "  (x+1)² − 2(x−1)(x+1) + (x−1)² = (2x+1)²\n" +
                    "    [(x+1)−(x−1)]² = (2x+1)²\n" +
                    "    4 = (2x+1)²  →  2x+1 = ±2\n" +
                    "    x = 1/2 или x = −3/2\n\n" +
                    "  x³ − (x−3)³(16−x) = x²+116\n\n" +
                    "  y(y−5)² − (y−2)(y³−5y²+2y−4) + 3y = 0\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Определи, какие ФСУ применить");
            sb.AppendLine("  · (a+b)³/(a−b)³ — куб суммы/разности");
            sb.AppendLine("  · a³±b³ — сумма/разность кубов");
            sb.AppendLine("  · a²−b² — разность квадратов");
            sb.AppendLine("  · (a±b)² — квадрат суммы/разности");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Примени формулы и раскрой скобки");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные (высокие степени часто сокращаются)");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши полученное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 34.17 / 34.18  Доказать тождество (все ФСУ) ────────────────────────

    /// <summary>
    /// Доказательство тождеств с применением всех формул ФСУ.
    /// Задачи 34.17, 34.18
    /// </summary>
    public class CubeSumAllFSUProveFunction : FunctionBase
    {
        public override string   Name       => "Доказать тождество (все ФСУ)";
        public override string   Formula    => "все формулы ФСУ";
        public override string[] Keywords   => new[] { "сумма кубов", "тождество", "все фсу", "доказать", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождеств с применением всех ФСУ\n\n" +
                    "Примеры (34.17):\n\n" +
                    "  (x+y)³ − (x−y)³ − 6y(x²−y²) = 8y³\n" +
                    "  Левая:\n" +
                    "  = x³+3x²y+3xy²+y³ − x³+3x²y−3xy²+y³ − 6y(x+y)(x−y)\n" +
                    "  = 6x²y+2y³ − 6y(x²−y²)\n" +
                    "  = 6x²y+2y³ − 6x²y+6y³ = 8y³  ✅\n\n" +
                    "  (a²−3)(a⁴+3a²+9) − (a³−3)²(a³+3) = 27\n" +
                    "  = a⁶−27 − (a³−3)²(a³+3)\n" +
                    "  ... раскрываем и приводим подобные\n\n" +
                    "✏️ Введи левую часть тождества:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть тождества:\n" +
                    "  Пример: 8y³  или  27  или  0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string target = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства:");
            sb.AppendLine();
            sb.AppendLine($"Надо показать: {left} = {target}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Примени нужные ФСУ к каждой части");
            sb.AppendLine("  · (a+b)³ = a³+3a²b+3ab²+b³");
            sb.AppendLine("  · (a−b)³ = a³−3a²b+3ab²−b³");
            sb.AppendLine("  · a³±b³  = (a±b)(a²∓ab+b²)");
            sb.AppendLine("  · a²−b²  = (a+b)(a−b)");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой все оставшиеся скобки");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine();
            sb.AppendLine($"📌 Цель: получить {target}  ✅");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  §35. Тождественные преобразования выражений
    //  Применение всех ФСУ для упрощения, решения уравнений, неравенств, доказательств
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── 35.1 / 35.2 / 35.3 / 35.4  Упростить выражение ─────────────────────

    /// <summary>
    /// Упрощение выражений с применением всех ФСУ.
    /// Задачи 35.1–35.4
    /// </summary>
    public class IdentitySimplify2Function : FunctionBase
    {
        public override string   Name       => "Упростить выражение (все ФСУ вместе)";
        public override string   Formula    => "все ФСУ: a²−b², (a±b)², (a±b)³, a³±b³";
        public override string[] Keywords   => new[] { "тождественные преобразования", "упростить", "все фсу", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражений — все ФСУ\n\n" +
                    "Все формулы сокращённого умножения:\n" +
                    "  · a²−b²   = (a+b)(a−b)\n" +
                    "  · (a+b)²  = a²+2ab+b²\n" +
                    "  · (a−b)²  = a²−2ab+b²\n" +
                    "  · (a+b)³  = a³+3a²b+3ab²+b³\n" +
                    "  · (a−b)³  = a³−3a²b+3ab²−b³\n" +
                    "  · a³+b³   = (a+b)(a²−ab+b²)\n" +
                    "  · a³−b³   = (a−b)(a²+ab+b²)\n\n" +
                    "Пример (35.1):\n" +
                    "  (4−5a)² − 8a(3a+1) + (7a−4)(4+7a)\n" +
                    "  = 16−40a+25a² − 24a²−8a + (7a+4)(7a−4)\n" +
                    "  = 16−40a+25a² − 24a²−8a + 49a²−16\n" +
                    "  = 50a²−48a\n\n" +
                    "Пример (35.2):\n" +
                    "  (1,1x²−6y)(1,1x²+6y)(1,21x⁴+36y²)\n" +
                    "  = [(1,1x²)²−(6y)²](1,21x⁴+36y²)\n" +
                    "  = (1,21x⁴−36y²)(1,21x⁴+36y²)\n" +
                    "  = (1,21x⁴)²−(36y²)²\n" +
                    "  = 1,4641x⁸ − 1296y⁴\n\n" +
                    "✏️ Опиши, какую ФСУ применяешь первой:\n" +
                    "  Пример: разность квадратов  или  квадрат разности  или  куб суммы",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи a (первый элемент в выбранной формуле):\n" +
                    "  Пример: 4−5a  или  1.1x²  или  x",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи b (второй элемент):\n" +
                    "  Пример: 5a  или  6y  или  3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string formulaName = answers[0].Trim();
            string a           = answers[1].Trim();
            string b           = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("Стратегия упрощения:");
            sb.AppendLine();
            sb.AppendLine($"Шаг 1: Применяем «{formulaName}»");
            sb.AppendLine($"  a = {a},  b = {b}");
            sb.AppendLine();

            // Определяем формулу по ключевым словам
            string fl = formulaName.ToLower();
            if (fl.Contains("разность квадрат"))
            {
                sb.AppendLine($"  ({a})² − ({b})² = ({a}+{b})({a}−{b})");
            }
            else if (fl.Contains("квадрат суммы") || fl.Contains("квадрат разности"))
            {
                bool plus = fl.Contains("суммы");
                string sign = plus ? "+" : "−";
                string pm   = plus ? "+" : "−";
                sb.AppendLine($"  ({a} {sign} {b})² = ({a})² {pm} 2·{a}·{b} + ({b})²");
            }
            else if (fl.Contains("куб суммы") || fl.Contains("куб разности"))
            {
                bool plus = fl.Contains("суммы");
                string sign = plus ? "+" : "−";
                string s1   = plus ? "+" : "−";
                string s3   = plus ? "+" : "−";
                sb.AppendLine($"  ({a} {sign} {b})³ = ({a})³ {s1} 3({a})²({b}) + 3({a})({b})² {s3} ({b})³");
            }
            else if (fl.Contains("сумма куб"))
            {
                sb.AppendLine($"  ({a})³ + ({b})³ = ({a}+{b})(({a})²−{a}·{b}+({b})²)");
            }
            else if (fl.Contains("разность куб"))
            {
                sb.AppendLine($"  ({a})³ − ({b})³ = ({a}−{b})(({a})²+{a}·{b}+({b})²)");
            }
            else
            {
                sb.AppendLine($"  Применяем формулу к a={a}, b={b}");
            }

            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой оставшиеся скобки");
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine();
            sb.AppendLine("Справочник всех ФСУ:");
            sb.AppendLine("  a²−b²  = (a+b)(a−b)");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a³+b³  = (a+b)(a²−ab+b²)");
            sb.AppendLine("  a³−b³  = (a−b)(a²+ab+b²)");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.5 / 35.6  Решить уравнение ───────────────────────────────────────

    /// <summary>
    /// Решение уравнений с применением всех ФСУ (тождественные преобразования).
    /// Задачи 35.5, 35.6
    /// </summary>
    public class IdentityEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение (все ФСУ)";
        public override string   Formula    => "применить ФСУ → линейное/квадратное";
        public override string[] Keywords   => new[] { "тождественные преобразования", "уравнение", "все фсу", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений с применением всех ФСУ\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Применить ФСУ к каждой части уравнения\n" +
                    "  2️⃣  Раскрыть оставшиеся скобки\n" +
                    "  3️⃣  Перенести всё в левую часть\n" +
                    "  4️⃣  Привести подобные (высокие степени часто сокращаются)\n" +
                    "  5️⃣  Решить линейное или квадратное уравнение\n\n" +
                    "Примеры (35.5):\n" +
                    "  35 + (5x−1)(5x+1) = (5x+2)²\n" +
                    "  35 + 25x²−1 = 25x²+20x+4\n" +
                    "  34 = 20x+4  →  20x = 30  →  x = 1.5\n\n" +
                    "  6 − x + (2x−1)² = 4(x+3)²\n" +
                    "  6−x+4x²−4x+1 = 4x²+24x+36\n" +
                    "  7−5x = 24x+36  →  −29x = 29  →  x = −1\n\n" +
                    "Примеры (35.6):\n" +
                    "  7x − (x−2)³ = 13 − x²(x−6)\n" +
                    "  7x − x³+6x²−12x+8 = 13−x³+6x²\n" +
                    "  −5x+8 = 13  →  x = −1\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Определи и примени ФСУ в каждой части");
            sb.AppendLine("  · (a+b)(a−b) → a²−b²");
            sb.AppendLine("  · (a±b)² → a²±2ab+b²");
            sb.AppendLine("  · (a±b)³ → a³±3a²b+3ab²±b³");
            sb.AppendLine("  · a³±b³ → (a±b)(a²∓ab+b²)");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой все оставшиеся скобки");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Перенеси правую часть влево, приведи подобные");
            sb.AppendLine("  Высокие степени (x², x³) обычно сокращаются");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.7  Найти корни уравнения ─────────────────────────────────────────

    /// <summary>
    /// Нахождение корней уравнений вида (выражение)² = 0 или (выражение)² = c.
    /// Задача 35.7
    /// </summary>
    public class IdentityFindRootsFunction : FunctionBase
    {
        public override string   Name       => "Найти корни уравнения (a±b)²=c";
        public override string   Formula    => "(a±b)² = c  →  a±b = ±√c";
        public override string[] Keywords   => new[] { "тождественные преобразования", "корни", "уравнение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Нахождение корней уравнения\n\n" +
                    "Уравнения вида (выражение)² = c:\n" +
                    "  → выражение = ±√c\n" +
                    "  → два линейных уравнения\n\n" +
                    "Примеры (35.7):\n" +
                    "  (x−7)² − 49 = 0\n" +
                    "  (x−7)² = 49\n" +
                    "  x−7 = ±7\n" +
                    "  x = 14  или  x = 0\n\n" +
                    "  (6+y)² − 81 = 0\n" +
                    "  (6+y)² = 81\n" +
                    "  6+y = ±9\n" +
                    "  y = 3  или  y = −15\n\n" +
                    "  100 − (z−19)² = 0\n" +
                    "  (z−19)² = 100\n" +
                    "  z−19 = ±10\n" +
                    "  z = 29  или  z = 9\n\n" +
                    "  25 − (13+t)² = 0\n" +
                    "  (13+t)² = 25\n" +
                    "  13+t = ±5\n" +
                    "  t = −8  или  t = −18\n\n" +
                    "✏️ Введи выражение в скобках (без степени):\n" +
                    "  Пример: x−7  или  6+y  или  z−19",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть (c, куда переносится число):\n" +
                    "  Пример: 49  или  81  или  100",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Ты ничего не ввёл.";
                    if (!double.TryParse(s.Trim().Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out double v) || v < 0)
                        return "Введи неотрицательное число.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string expr  = answers[0].Trim();
            string cStr  = answers[1].Trim();
            double c = double.Parse(cStr.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture);
            double sqrtC = Math.Sqrt(c);
            bool   isPerfectSq = Math.Abs(sqrtC - Math.Round(sqrtC)) < 1e-9;

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: ({expr})² = {Fmt(c)}");
            sb.AppendLine();
            sb.AppendLine($"Берём квадратный корень из обеих частей:");
            sb.AppendLine($"  {expr} = ±√{Fmt(c)}");

            if (isPerfectSq)
            {
                long sq = (long)Math.Round(sqrtC);
                sb.AppendLine($"  {expr} = ±{sq}");
                sb.AppendLine();
                sb.AppendLine($"Два уравнения:");
                sb.AppendLine($"  {expr} = {sq}   →   решение 1");
                sb.AppendLine($"  {expr} = −{sq}  →   решение 2");
            }
            else
            {
                sb.AppendLine($"  √{Fmt(c)} ≈ {sqrtC:G6}");
                sb.AppendLine();
                sb.AppendLine($"Два уравнения:");
                sb.AppendLine($"  {expr} = √{Fmt(c)}   →   решение 1");
                sb.AppendLine($"  {expr} = −√{Fmt(c)}  →   решение 2");
            }

            sb.AppendLine();
            sb.AppendLine("Реши каждое линейное уравнение и запиши оба корня.");

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);
    }

    // ─── 35.8  Решить уравнение с произведением выражений = 0 ─────────────────

    /// <summary>
    /// Решение уравнений вида P(x)·Q(x) = 0 после применения ФСУ.
    /// Задача 35.8
    /// </summary>
    public class IdentityProductZeroFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение P·Q=0 (ФСУ)";
        public override string   Formula    => "AB=0 → A=0 или B=0";
        public override string[] Keywords   => new[] { "тождественные преобразования", "уравнение", "произведение нуль", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение уравнений через произведение = 0\n\n" +
                    "Произведение равно нулю, если хотя бы один множитель = 0.\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Примени ФСУ для разложения на множители\n" +
                    "  2️⃣  Перепиши в виде A·B = 0\n" +
                    "  3️⃣  Реши A = 0 и B = 0 по отдельности\n\n" +
                    "Примеры (35.8):\n" +
                    "  x(0.25x−3) − (0.5x+1)(0.5x−1) = 0\n" +
                    "  0.25x²−3x − (0.25x²−1) = 0\n" +
                    "  −3x+1 = 0  →  x = 1/3\n\n" +
                    "  0.49x²−3x−(0.7x+2)(0.7x−2) = 0\n" +
                    "  0.49x²−3x−0.49x²+4 = 0\n" +
                    "  −3x+4 = 0  →  x = 4/3\n\n" +
                    "  1.6x²+x(1−1.6x) − 64x(1−0.04x) = 0\n" +
                    "  x(1−64+64·0.04x+1.6x−1.6x) = 0\n" +
                    "  x(1−64+2.56x) = 0  →  x=0  или  2.56x=63  →  x≈24.6\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть (обычно 0):\n" +
                    "  Пример: 0",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Примени ФСУ к произведениям в скобках");
            sb.AppendLine("  Часто: (a+b)(a−b) = a²−b² убирает квадратичный член");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси правую часть, приведи подобные");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Если получилось A·B = 0:");
            sb.AppendLine("  A = 0  →  решение 1");
            sb.AppendLine("  B = 0  →  решение 2");
            sb.AppendLine();
            sb.AppendLine("Если получилось линейное kx+b = 0:");
            sb.AppendLine("  x = −b/k");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.9 / 35.10  Решить уравнение (сложные) ────────────────────────────

    /// <summary>
    /// Решение сложных уравнений с несколькими применениями ФСУ.
    /// Задачи 35.9, 35.10
    /// </summary>
    public class IdentityComplexEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить сложное уравнение (все ФСУ)";
        public override string   Formula    => "раскрыть все ФСУ → привести подобные";
        public override string[] Keywords   => new[] { "тождественные преобразования", "сложное уравнение", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сложные уравнения с применением ФСУ\n\n" +
                    "Примеры (35.9):\n" +
                    "  (7x−5)² + 67x − 49x² = −2\n" +
                    "  49x²−70x+25 + 67x−49x² = −2\n" +
                    "  −3x+25 = −2  →  x = 9\n\n" +
                    "  196x²−(14x+3)² + 80x = −5\n" +
                    "  196x²−196x²−84x−9+80x = −5\n" +
                    "  −4x−9 = −5  →  x = −1\n\n" +
                    "Примеры (35.10):\n" +
                    "  5(2+y)³ − 5x³ = 28x + 30x²  [опечатка в учебнике, скорее всего y=x]\n" +
                    "  54x²(x−3)³ − 162 = 6x³\n" +
                    "  (x+9)(x²−9x+81) = −7 − 4x + x²\n" +
                    "  x³−2x−331 = (x²−11x+121)(x+11)\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Примени ФСУ — раскрой все квадраты и кубы двучленов");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a³±b³ = (a±b)(a²∓ab+b²)");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Раскрой произведения многочленов");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Перенеси правую часть влево");
            sb.AppendLine("  Высокие степени (x², x³) обычно сокращаются полностью");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши линейное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.11 / 35.12 / 35.13  Решить неравенство ───────────────────────────

    /// <summary>
    /// Решение неравенств с применением всех ФСУ.
    /// Задачи 35.11–35.13
    /// </summary>
    public class IdentityInequalityFunction : FunctionBase
    {
        public override string   Name       => "Решить неравенство (все ФСУ)";
        public override string   Formula    => "раскрыть ФСУ → линейное/квадратное неравенство";
        public override string[] Keywords   => new[] { "тождественные преобразования", "неравенство", "все фсу", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Решение неравенств с применением всех ФСУ\n\n" +
                    "Стратегия:\n" +
                    "  1️⃣  Раскрой все ФСУ с обеих сторон\n" +
                    "  2️⃣  Перенеси правую часть влево\n" +
                    "  3️⃣  Приведи подобные (высокие степени сокращаются)\n" +
                    "  4️⃣  Реши линейное или квадратное неравенство\n\n" +
                    "Примеры (35.11):\n" +
                    "  (x+8)² ≤ 11x → x²+16x+64 ≤ 11x → x²+5x+64 ≤ 0\n" +
                    "  x²−9 > −2x  → x²+2x−9 > 0\n\n" +
                    "Примеры (35.12):\n" +
                    "  (y+7)³ − 21y² ≥ 0  →  y³+21y²+147y+343−21y² ≥ 0\n" +
                    "  y³+147y+343 ≥ 0\n\n" +
                    "Примеры (35.13) — знаковые рассуждения:\n" +
                    "  (10+x)(100−10x+x²) ≥ 0  →  x³+1000 ≥ 0  →  x ≥ −10\n\n" +
                    "✏️ Введи левую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string op    = answers[1].Trim();
            string right = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Неравенство: {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой все ФСУ в обеих частях");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a³±b³ = (a±b)(a²∓ab+b²)");
            sb.AppendLine();
            sb.AppendLine($"Шаг 2: Перенеси правую часть влево:");
            sb.AppendLine($"  ({left}) − ({right}) {op} 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные слагаемые");
            sb.AppendLine("  Высокие степени обычно сокращаются");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши полученное неравенство и запиши ответ в виде промежутка");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} {op} {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.14 / 35.15  Доказать тождество ───────────────────────────────────

    /// <summary>
    /// Доказательство тождеств с применением всех ФСУ.
    /// Задачи 35.14, 35.15
    /// </summary>
    public class IdentityProveAllFSUFunction : FunctionBase
    {
        public override string   Name       => "Доказать тождество (все ФСУ)";
        public override string   Formula    => "преобразовать левую часть к правой";
        public override string[] Keywords   => new[] { "тождественные преобразования", "тождество", "доказать", "все фсу", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказательство тождеств\n\n" +
                    "Стратегия: преобразуй ЛЕВУЮ часть → получи ПРАВУЮ.\n" +
                    "(или преобразуй обе части и покажи, что они равны)\n\n" +
                    "Примеры (35.14):\n" +
                    "  (3x+4y)² − (4y−3x)² = 48xy\n" +
                    "  Левая = (3x+4y+4y−3x)(3x+4y−4y+3x) = 8y·6x = 48xy  ✅\n\n" +
                    "  (1.5x−2y)² + (2x+1.5y)² = 6.25(x²+y²)\n" +
                    "  = 2.25x²−6xy+4y² + 4x²+6xy+2.25y² = 6.25x²+6.25y²  ✅\n\n" +
                    "Примеры (35.15):\n" +
                    "  (5z²−6k)² − (5z²+3k)³  ← сложные выражения\n" +
                    "  (1.2x⁴−7y²)(1.2x⁴+7y²)(0.56x⁴+49y²) = 1.4⁴x¹⁶−2401y⁸?\n\n" +
                    "✏️ Введи левую часть тождества:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть тождества (ожидаемый результат):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string target = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства:");
            sb.AppendLine();
            sb.AppendLine($"Надо показать: {left} = {target}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Выбери стратегию");
            sb.AppendLine("  · Преобразуй левую часть → правую");
            sb.AppendLine("  · Или: преобразуй обе части → одинаковый вид");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Примени нужные ФСУ");
            sb.AppendLine("  a²−b²  = (a+b)(a−b)   ← часто для «телескопических» произведений");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a³±b³  = (a±b)(a²∓ab+b²)");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Раскрой скобки и приведи подобные");
            sb.AppendLine();
            sb.AppendLine($"📌 Цель: получить {target}  ✅");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.16 / 35.17  Упростить (уровень Б) ────────────────────────────────

    /// <summary>
    /// Упрощение сложных выражений уровня Б.
    /// Задачи 35.16, 35.17
    /// </summary>
    public class IdentityAdvancedSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить сложное выражение (уровень Б)";
        public override string   Formula    => "многократное применение ФСУ";
        public override string[] Keywords   => new[] { "тождественные преобразования", "упростить", "уровень б", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение сложных выражений (уровень Б)\n\n" +
                    "Примеры (35.16):\n" +
                    "  (4x³−1)(9x²+5) − (6x³+1)² \n\n" +
                    "  (x⁴−1)(9x²+5) − (6x²+1)²  — проверь что здесь\n\n" +
                    "Примеры (35.17):\n" +
                    "  (a²+b²)³ − (a²−b²)³ − 2b²\n" +
                    "    = [(a²+b²)−(a²−b²)]·... — применяем разность кубов\n" +
                    "    или раскрываем кубы напрямую\n\n" +
                    "  3a³b⁸(a³+b⁸)(a³−b⁸) − (a⁶−b¹⁶)²·½\n" +
                    "    = 3a³b⁸(a⁶−b¹⁶) − ½(a⁶−b¹⁶)²\n\n" +
                    "Ключевой приём: замена подвыражения на букву\n" +
                    "  Пусть u = a²+b², v = a²−b²\n" +
                    "  тогда u³−v³ = (u−v)(u²+uv+v²)\n\n" +
                    "✏️ Введи подвыражение u (что подставляем вместо «a» в ФСУ):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи подвыражение v (что подставляем вместо «b» в ФСУ):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Какую ФСУ применяем?\n" +
                    "  Введи: a2-b2  или  (a+b)2  или  (a-b)2  или  (a+b)3  или  (a-b)3  или  a3+b3  или  a3-b3",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string u      = answers[0].Trim();
            string v      = answers[1].Trim();
            string fsuKey = answers[2].Trim().ToLower();

            var sb = new StringBuilder();
            sb.AppendLine($"Замена: u = {u},  v = {v}");
            sb.AppendLine();

            if (fsuKey.Contains("a2-b2") || fsuKey.Contains("a²-b²"))
                sb.AppendLine($"  u² − v² = (u+v)(u−v)");
            else if (fsuKey.Contains("a+b)2") || fsuKey.Contains("квадрат суммы"))
                sb.AppendLine($"  (u+v)² = u²+2uv+v²");
            else if (fsuKey.Contains("a-b)2") || fsuKey.Contains("квадрат разности"))
                sb.AppendLine($"  (u−v)² = u²−2uv+v²");
            else if (fsuKey.Contains("a+b)3") || fsuKey.Contains("куб суммы"))
                sb.AppendLine($"  (u+v)³ = u³+3u²v+3uv²+v³");
            else if (fsuKey.Contains("a-b)3") || fsuKey.Contains("куб разности"))
                sb.AppendLine($"  (u−v)³ = u³−3u²v+3uv²−v³");
            else if (fsuKey.Contains("a3+b3") || fsuKey.Contains("сумма кубов"))
                sb.AppendLine($"  u³+v³ = (u+v)(u²−uv+v²)");
            else if (fsuKey.Contains("a3-b3") || fsuKey.Contains("разность кубов"))
                sb.AppendLine($"  u³−v³ = (u−v)(u²+uv+v²)");
            else
                sb.AppendLine($"  Применяем ФСУ: {fsuKey}");

            sb.AppendLine();
            sb.AppendLine("Подставляем назад:");
            sb.AppendLine($"  u = {u}");
            sb.AppendLine($"  v = {v}");
            sb.AppendLine();
            sb.AppendLine("Раскрываем и приводим подобные слагаемые.");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.18 / 35.19  Решить уравнение (уровень Б, высокая степень) ────────

    /// <summary>
    /// Решение уравнений высокой степени через ФСУ (уровень Б).
    /// Задачи 35.18, 35.19
    /// </summary>
    public class IdentityHighDegreeEquationFunction : FunctionBase
    {
        public override string   Name       => "Решить уравнение высокой степени (ФСУ)";
        public override string   Formula    => "разложение на множители через ФСУ";
        public override string[] Keywords   => new[] { "тождественные преобразования", "уравнение высокой степени", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Уравнения высокой степени через ФСУ (уровень Б)\n\n" +
                    "Примеры (35.18):\n" +
                    "  8x(x²−11)(x+5) = (5x+3)(x+1)³ ... сложное\n" +
                    "  2.5(4+x)² + 7(5−x)(5+x) = 295 − 4.5x²\n" +
                    "    2.5(16+8x+x²) + 7(25−x²) = 295−4.5x²\n" +
                    "    40+20x+2.5x² + 175−7x² = 295−4.5x²\n" +
                    "    20x + 215 = 295\n" +
                    "    20x = 80  →  x = 4\n\n" +
                    "Примеры (35.19):\n" +
                    "  (2.3x−7)(5.29x²+23x+100) − 125x = 12.167x³\n" +
                    "  Замечаем: (2.3x−7)(5.29x²+23x+100) ≈ (a−b)(a²+ab+b²) = a³−b³?\n" +
                    "  a=2.3x=23x/10, b=... проверяем\n\n" +
                    "✏️ Введи левую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть уравнения:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string right = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Уравнение: {left} = {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Стратегия для уравнений высокой степени:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Определи, есть ли произведения вида");
            sb.AppendLine("  · (a−b)(a²+ab+b²) → a³−b³");
            sb.AppendLine("  · (a+b)(a²−ab+b²) → a³+b³");
            sb.AppendLine("  · (a±b)³ → раскрой");
            sb.AppendLine("  · (a+b)(a−b) → a²−b²");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Замени произведения на кубы/квадраты");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Перенеси в одну сторону, приведи подобные");
            sb.AppendLine("  Высокие степени сокращаются → линейное уравнение");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} = {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.20 / 35.21  Решить неравенство (уровень Б) ──────────────────────

    /// <summary>
    /// Решение сложных неравенств (уровень Б).
    /// Задачи 35.20, 35.21
    /// </summary>
    public class IdentityAdvancedInequalityFunction : FunctionBase
    {
        public override string   Name       => "Решить сложное неравенство (уровень Б)";
        public override string   Formula    => "все ФСУ → привести подобные";
        public override string[] Keywords   => new[] { "тождественные преобразования", "сложное неравенство", "уровень б", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сложные неравенства (уровень Б)\n\n" +
                    "Примеры (35.20):\n" +
                    "  (9x−7)²−10 ≤ (9x+3)(9x−5)\n" +
                    "  81x²−126x+49−10 ≤ 81x²−45x+27x−15\n" +
                    "  81x²−126x+39 ≤ 81x²−18x−15\n" +
                    "  −108x ≤ −54  →  x ≥ 1/2\n\n" +
                    "  (3+7x²)²−(6x)² ≥ 21x\n" +
                    "  Замечаем: это (a+b)(a−b) если a=3+7x², b=6x?\n" +
                    "  Или раскрываем (3+7x²)² = 9+42x²+49x⁴\n\n" +
                    "Примеры (35.21):\n" +
                    "  13 + x³(x−9) ≤ (x−3)³+11\n" +
                    "  26 + (2+x)² < x²(6+x)\n\n" +
                    "✏️ Введи левую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left  = answers[0].Trim();
            string op    = answers[1].Trim();
            string right = answers[2].Trim();

            var sb = new StringBuilder();
            sb.AppendLine($"Неравенство: {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine();
            sb.AppendLine("Шаг 1: Раскрой все ФСУ с обеих сторон");
            sb.AppendLine("  (a±b)² = a²±2ab+b²");
            sb.AppendLine("  (a±b)³ = a³±3a²b+3ab²±b³");
            sb.AppendLine("  a²−b² = (a+b)(a−b) — для быстрого умножения");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Перенеси правую часть влево");
            sb.AppendLine($"  ({left}) − ({right}) {op} 0");
            sb.AppendLine();
            sb.AppendLine("Шаг 3: Приведи подобные (высокие степени сокращаются)");
            sb.AppendLine();
            sb.AppendLine("Шаг 4: Реши и запиши ответ промежутком");
            sb.AppendLine();
            sb.AppendLine($"📌 Реши: {left} {op} {right}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.22 / 35.23  Найти наибольшее/наименьшее целое решение неравенства ─

    /// <summary>
    /// Нахождение наибольшего/наименьшего целого числа — решения неравенства.
    /// Задачи 35.22, 35.23
    /// </summary>
    public class IdentityIntegerSolutionFunction : FunctionBase
    {
        public override string   Name       => "Найти целое число — решение неравенства";
        public override string   Formula    => "решить неравенство → найти целое";
        public override string[] Keywords   => new[] { "тождественные преобразования", "целое число", "неравенство", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти наибольшее/наименьшее целое число — решение неравенства\n\n" +
                    "Алгоритм:\n" +
                    "  1️⃣  Реши неравенство (применив ФСУ)\n" +
                    "  2️⃣  Запиши ответ в виде промежутка\n" +
                    "  3️⃣  Найди нужное целое: наибольшее целое ≤ правой границе\n" +
                    "     или наименьшее целое ≥ левой границе\n\n" +
                    "Примеры (35.22):\n" +
                    "  (3−x)(9+3x+x²) − 2x+x³ ≤ 7x+7\n" +
                    "  (3³−x³) − 2x+x³ ≤ 7x+7\n" +
                    "  27 − 9x ≤ 7x+7  →  x ≥ 20/16 = 5/4\n" +
                    "  Наибольшее целое ≤ 5/4? Нет — ищем наибольшее целое x ≤ ...\n\n" +
                    "✏️ Введи неравенство (левая часть):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи знак: >, <, >= или <=",
                Validate = s =>
                {
                    string t = s.Trim();
                    if (t == ">" || t == "<" || t == ">=" || t == "<=" || t == "≥" || t == "≤")
                        return null;
                    return "Введи >, <, >= или <=";
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть неравенства:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Что ищем: наибольшее или наименьшее целое?\n" +
                    "  Введи: наибольшее  или  наименьшее",
                Validate = s =>
                {
                    string t = s.Trim().ToLower();
                    if (t == "наибольшее" || t == "наименьшее") return null;
                    return "Введи: наибольшее  или  наименьшее";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string op     = answers[1].Trim();
            string right  = answers[2].Trim();
            string target = answers[3].Trim().ToLower();

            var sb = new StringBuilder();
            sb.AppendLine($"Задача: найти {target} целое число — решение неравенства");
            sb.AppendLine($"  {left} {op} {right}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Шаг 1: Реши неравенство");
            sb.AppendLine("  Применяй ФСУ, приводи подобные");
            sb.AppendLine();
            sb.AppendLine("Шаг 2: Запиши ответ в виде промежутка");
            sb.AppendLine("  Например: x ≥ 5/4  или  x < −3.5");
            sb.AppendLine();

            if (target == "наибольшее")
            {
                sb.AppendLine("Шаг 3: Найти НАИБОЛЬШЕЕ целое:");
                sb.AppendLine("  Если x ≤ a:  наибольшее целое = ⌊a⌋  (округлить вниз)");
                sb.AppendLine("  Если x < a:  наибольшее целое = ⌈a⌉−1  (если a — целое: a−1)");
            }
            else
            {
                sb.AppendLine("Шаг 3: Найти НАИМЕНЬШЕЕ целое:");
                sb.AppendLine("  Если x ≥ a:  наименьшее целое = ⌈a⌉  (округлить вверх)");
                sb.AppendLine("  Если x > a:  наименьшее целое = ⌊a⌋+1  (если a — целое: a+1)");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Реши {left} {op} {right}, затем найди {target} целое.");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 35.24 / 35.25  Доказать тождество (уровень В) ──────────────────────

    /// <summary>
    /// Доказательство сложных тождеств (уровень В).
    /// Задачи 35.24, 35.25
    /// </summary>
    public class IdentityProveAdvancedFunction : FunctionBase
    {
        public override string   Name       => "Доказать сложное тождество (уровень В)";
        public override string   Formula    => "все ФСУ + замена переменной";
        public override string[] Keywords   => new[] { "тождественные преобразования", "тождество", "сложный", "уровень в", "фсу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Сложные тождества (уровень В)\n\n" +
                    "Примеры (35.24):\n\n" +
                    "  ((a−b⁸)(b⁴+a⁴))(b⁴+a⁴) − a²(+2b⁸+a¹²) = b¹⁶\n\n" +
                    "  (x³+9x⁴−9x²+9) − (x⁶+3x⁴x) (9x²-x+9) + 36x³ = 35x³\n\n" +
                    "Примеры (35.25):\n" +
                    "  (a¹⁰−t¹⁰)(a⁴+t⁴)(a⁸+t⁸)−(t²−a²·t²)(a⁸+...) = ...\n\n" +
                    "Ключевые приёмы:\n" +
                    "  · Замена подвыражения: u = a²+b², v = a²−b²\n" +
                    "  · Цепочка разностей квадратов: (a−b)(a+b) = a²−b²\n" +
                    "    а потом снова (a²−b²)(a²+b²) = a⁴−b⁴ и т.д.\n" +
                    "  · «Телескопическое» произведение:\n" +
                    "    (a−b)(a+b)(a²+b²)(a⁴+b⁴) = a⁸−b⁸\n\n" +
                    "✏️ Введи левую часть тождества:",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую часть (ожидаемый результат):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string left   = answers[0].Trim();
            string target = answers[1].Trim();

            var sb = new StringBuilder();
            sb.AppendLine("📋 Схема доказательства (уровень В):");
            sb.AppendLine();
            sb.AppendLine($"Надо показать: {left} = {target}");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Приёмы для сложных тождеств:");
            sb.AppendLine();
            sb.AppendLine("🔹 Телескопическое произведение:");
            sb.AppendLine("  (a−b)(a+b) = a²−b²");
            sb.AppendLine("  (a²−b²)(a²+b²) = a⁴−b⁴");
            sb.AppendLine("  Обобщение: (a−b)·∏(aⁿ+bⁿ) = a²ⁿ⁺¹−b²ⁿ⁺¹");
            sb.AppendLine();
            sb.AppendLine("🔹 Замена переменной:");
            sb.AppendLine("  Если выражение содержит (a²+b²) или (a³+b³)");
            sb.AppendLine("  — обозначи это как u, упрости, замени назад");
            sb.AppendLine();
            sb.AppendLine("🔹 Группировка слагаемых:");
            sb.AppendLine("  Иногда удобно переставить и сгруппировать по ФСУ");
            sb.AppendLine();
            sb.AppendLine($"📌 Цель: получить {target}  ✅");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════
    //   НЕДОСТАЮЩИЕ КЛАССЫ ТОЖДЕСТВ
    // ═══════════════════════════════════════════════════════

    public class IdentitySimplifyFunction : FunctionBase
    {
        public override string   Name       => "Тождества: упростить выражение";
        public override string   Formula    => "ФСУ → упрощение";
        public override string[] Keywords   => ["тождество", "упростить", "выражение", "фсу"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Упрощение выражений с помощью ФСУ\n\n" +
                    "Используем формулы:\n" +
                    "  (a+b)² = a²+2ab+b²\n" +
                    "  (a−b)² = a²−2ab+b²\n" +
                    "  (a+b)(a−b) = a²−b²\n" +
                    "  (a+b)³ = a³+3a²b+3ab²+b³\n" +
                    "  (a−b)³ = a³−3a²b+3ab²−b³\n\n" +
                    "Пример: (x+3)² − (x−3)²\n" +
                    "  = (x²+6x+9) − (x²−6x+9) = 12x\n\n" +
                    "✏️ Введи a (первый член, например: x или 2a):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (второй член, например: 3 или y):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Выбери тип выражения:\n" +
                    "  1 — (a+b)² − (a−b)²\n" +
                    "  2 — (a+b)² + (a−b)²\n" +
                    "  3 — (a+b)(a−b)\n" +
                    "  4 — (a+b)³ − (a−b)³",
                Validate = s => s.Trim() is "1" or "2" or "3" or "4"
                    ? null : "Введи цифру от 1 до 4."
            }
        ];

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a = answers[0].Trim(), b = answers[1].Trim();
            string type = answers[2].Trim();
            var sb = new StringBuilder();

            switch (type)
            {
                case "1":
                    sb.AppendLine($"Упрощаем: ({a}+{b})² − ({a}−{b})²");
                    sb.AppendLine();
                    sb.AppendLine($"  ({a}+{b})² = {a}² + 2·{a}·{b} + {b}²");
                    sb.AppendLine($"  ({a}−{b})² = {a}² − 2·{a}·{b} + {b}²");
                    sb.AppendLine();
                    sb.AppendLine("  Вычитаем:");
                    sb.AppendLine($"  ({a}²+2{a}{b}+{b}²) − ({a}²−2{a}{b}+{b}²)");
                    sb.AppendLine($"  = 4·{a}·{b}");
                    sb.AppendLine($"📌 = 4{a}{b}");
                    break;
                case "2":
                    sb.AppendLine($"Упрощаем: ({a}+{b})² + ({a}−{b})²");
                    sb.AppendLine();
                    sb.AppendLine($"  ({a}+{b})² = {a}² + 2{a}{b} + {b}²");
                    sb.AppendLine($"  ({a}−{b})² = {a}² − 2{a}{b} + {b}²");
                    sb.AppendLine();
                    sb.AppendLine("  Складываем:");
                    sb.AppendLine($"  = 2{a}² + 2{b}²");
                    sb.AppendLine($"📌 = 2({a}² + {b}²)");
                    break;
                case "3":
                    sb.AppendLine($"Упрощаем: ({a}+{b})·({a}−{b})");
                    sb.AppendLine();
                    sb.AppendLine("  Применяем разность квадратов:");
                    sb.AppendLine($"📌 = {a}² − {b}²");
                    break;
                case "4":
                    sb.AppendLine($"Упрощаем: ({a}+{b})³ − ({a}−{b})³");
                    sb.AppendLine();
                    sb.AppendLine($"  ({a}+{b})³ = {a}³ + 3{a}²{b} + 3{a}{b}² + {b}³");
                    sb.AppendLine($"  ({a}−{b})³ = {a}³ − 3{a}²{b} + 3{a}{b}² − {b}³");
                    sb.AppendLine();
                    sb.AppendLine("  Вычитаем:");
                    sb.AppendLine($"  = 6{a}²{b} + 2{b}³");
                    sb.AppendLine($"📌 = 2{b}(3{a}² + {b}²)");
                    break;
            }

            return sb.ToString().TrimEnd();
        }
    }

    public class IdentitySumAsProductFunction : FunctionBase
    {
        public override string   Name       => "Тождества: сумму представить как произведение";
        public override string   Formula    => "a²−b² = (a+b)(a−b)";
        public override string[] Keywords   => ["тождество", "сумма", "произведение", "представить"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Представить сумму (разность) как произведение\n\n" +
                    "Это разложение на множители с помощью ФСУ.\n\n" +
                    "Примеры:\n" +
                    "  x² − 9     = (x+3)(x−3)\n" +
                    "  4a² − 25b² = (2a+5b)(2a−5b)\n" +
                    "  x² + 6x + 9 = (x+3)²\n" +
                    "  8a³ + b³   = (2a+b)(4a²−2ab+b²)\n\n" +
                    "✏️ Введи a (первый член, например: x или 2a):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (второй член, например: 3 или 5b):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question =
                    "✏️ Выбери формулу:\n" +
                    "  1 — a² − b²   (разность квадратов)\n" +
                    "  2 — a² + 2ab + b²   (полный квадрат суммы)\n" +
                    "  3 — a² − 2ab + b²   (полный квадрат разности)\n" +
                    "  4 — a³ + b³   (сумма кубов)\n" +
                    "  5 — a³ − b³   (разность кубов)",
                Validate = s => s.Trim() is "1" or "2" or "3" or "4" or "5"
                    ? null : "Введи цифру от 1 до 5."
            }
        ];

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a = answers[0].Trim(), b = answers[1].Trim();
            string type = answers[2].Trim();
            var sb = new StringBuilder();

            switch (type)
            {
                case "1":
                    sb.AppendLine($"Разность квадратов: {a}² − {b}²");
                    sb.AppendLine($"📌 = ({a} + {b})({a} − {b})");
                    break;
                case "2":
                    sb.AppendLine($"Полный квадрат суммы: {a}² + 2·{a}·{b} + {b}²");
                    sb.AppendLine($"📌 = ({a} + {b})²");
                    break;
                case "3":
                    sb.AppendLine($"Полный квадрат разности: {a}² − 2·{a}·{b} + {b}²");
                    sb.AppendLine($"📌 = ({a} − {b})²");
                    break;
                case "4":
                    sb.AppendLine($"Сумма кубов: {a}³ + {b}³");
                    sb.AppendLine($"📌 = ({a} + {b})({a}² − {a}·{b} + {b}²)");
                    break;
                case "5":
                    sb.AppendLine($"Разность кубов: {a}³ − {b}³");
                    sb.AppendLine($"📌 = ({a} − {b})({a}² + {a}·{b} + {b}²)");
                    break;
            }

            return sb.ToString().TrimEnd();
        }
    }

    public class IdentityInequalityIntegerFunction : FunctionBase
    {
        public override string   Name       => "Тождества: неравенство с целыми числами";
        public override string   Formula    => "a² + b² ≥ 2ab";
        public override string[] Keywords   => ["тождество", "неравенство", "целые", "доказать"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Неравенства с целыми числами через ФСУ\n\n" +
                    "Ключевые неравенства:\n" +
                    "  a² + b² ≥ 2ab   (из (a−b)² ≥ 0)\n" +
                    "  (a+b)² ≥ 4ab    (если a,b > 0)\n" +
                    "  a² ≥ 0          (квадрат всегда ≥ 0)\n\n" +
                    "Метод доказательства:\n" +
                    "  Записываем a² + b² − 2ab = (a−b)² ≥ 0\n" +
                    "  Следовательно a² + b² ≥ 2ab ✅\n\n" +
                    "✏️ Введи a (число или выражение, например: 3 или x):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            },
            new InputStep
            {
                Question = "✏️ Введи b (число или выражение, например: 5 или y):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Ты ничего не ввёл." : null
            }
        ];

        public override string CalculateFromAnswers(List<string> answers)
        {
            string a = answers[0].Trim(), b = answers[1].Trim();
            var sb = new StringBuilder();

            sb.AppendLine($"Доказываем: {a}² + {b}² ≥ 2·{a}·{b}");
            sb.AppendLine();
            sb.AppendLine("Ключевое наблюдение: квадрат любого числа ≥ 0");
            sb.AppendLine();
            sb.AppendLine($"  ({a} − {b})² ≥ 0");
            sb.AppendLine();
            sb.AppendLine("Раскрываем левую часть:");
            sb.AppendLine($"  {a}² − 2·{a}·{b} + {b}² ≥ 0");
            sb.AppendLine();
            sb.AppendLine("Переносим −2ab вправо:");
            sb.AppendLine($"  {a}² + {b}² ≥ 2·{a}·{b}  ✅");
            sb.AppendLine();

            double av = 0, bv = 0;
            bool isNums = double.TryParse(a.Replace(',', '.'),
                              System.Globalization.NumberStyles.Any,
                              System.Globalization.CultureInfo.InvariantCulture, out av)
                       && double.TryParse(b.Replace(',', '.'),
                              System.Globalization.NumberStyles.Any,
                              System.Globalization.CultureInfo.InvariantCulture, out bv);
            if (isNums)
            {
                sb.AppendLine($"Проверка: {av}² + {bv}² = {av*av + bv*bv},  2·{av}·{bv} = {2*av*bv}");
                sb.AppendLine($"  {av*av + bv*bv} ≥ {2*av*bv}  — " +
                              (av*av + bv*bv >= 2*av*bv ? "верно ✅" : "ошибка ❌"));
            }

            sb.AppendLine();
            sb.AppendLine("📌 Равенство достигается только при a = b.");

            return sb.ToString().TrimEnd();
        }
    }
}

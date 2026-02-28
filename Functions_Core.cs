using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Общие шаги для табличных функций
    // ═══════════════════════════════════════════════════════════════

    internal static class TableSteps
    {
        public static string? ValidateRow(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 1, 2, 3, 4";
            if (s.Split(',', StringSplitOptions.RemoveEmptyEntries).Length < 2)
                return "Введи минимум 2 значения через запятую.\nПример: 1, 2, 3, 4";
            return null;
        }

        public static List<string> ParseRow(string s) =>
            s.Split(',', StringSplitOptions.RemoveEmptyEntries)
             .Select(p => p.Trim())
             .ToList();

        public static bool TryParseDoubles(List<string> row, out List<double> result)
        {
            result = new List<double>();
            foreach (var s in row)
            {
                if (!double.TryParse(s.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                    return false;
                result.Add(v);
            }
            return true;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Является ли зависимость функцией?
    // ═══════════════════════════════════════════════════════════════

    public class IsFunctionFunction : FunctionBase
    {
        public override string   Name       => "Является ли функцией";
        public override string   Formula    => "каждому x — ровно одно y";
        public override string[] Keywords   => new[] { "функция", "является", "таблица" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Является ли зависимость функцией?\n\n" +
                    "Функция — зависимость, при которой каждому x\n" +
                    "соответствует РОВНО ОДНО y.\n\n" +
                    "Пример — функция:\n" +
                    "  x:  1   2   3   4\n" +
                    "  y:  5   8   5   2\n" +
                    "  ✅ Каждому x — одно y (y может повторяться — это не нарушение)\n\n" +
                    "Пример — НЕ функция:\n" +
                    "  x:  1   2   2   3\n" +
                    "  y:  5   8   3   2\n" +
                    "  ❌ x=2 даёт y=8 и y=3 — нарушение!\n\n" +
                    "✏️ Введи строку x через запятую:\n" +
                    "  Пример: 1, 2, 3, 4",
                Validate = TableSteps.ValidateRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:\n" +
                           "  Пример: 5, 8, 5, 2",
                Validate = TableSteps.ValidateRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);
            var sb = new StringBuilder();

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            var dict = new Dictionary<string, List<string>>();
            for (int i = 0; i < xs.Count; i++)
            {
                if (!dict.ContainsKey(xs[i])) dict[xs[i]] = new List<string>();
                dict[xs[i]].Add(ys[i]);
            }

            sb.AppendLine("Проверяем каждый x:");
            bool isFunc = true;
            foreach (var kv in dict)
            {
                // Сравниваем числово если возможно, иначе строково
                var distinct = kv.Value
                    .Select(v => double.TryParse(v.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double d) ? d.ToString("G15") : v)
                    .Distinct()
                    .ToList();
                bool bad = distinct.Count > 1;
                if (bad) isFunc = false;
                sb.AppendLine(bad
                    ? $"  ❌ x = {kv.Key} → y = {string.Join(", ", distinct)}  (два значения!)"
                    : $"  ✅ x = {kv.Key} → y = {kv.Value[0]}");
            }

            sb.AppendLine();

            if (!isFunc)
            {
                sb.AppendLine("📌 Это НЕ функция.");
                sb.AppendLine("Причина: одному x соответствует больше одного y.");
            }
            else
            {
                sb.AppendLine("📌 Это ФУНКЦИЯ ✅");
                var repY = ys.GroupBy(y => y).Where(g => g.Count() > 1).ToList();
                if (repY.Any())
                    sb.AppendLine("Заметка: некоторые y повторяются — это не нарушение.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Возрастающая или убывающая функция?
    // ═══════════════════════════════════════════════════════════════

    public class IsIncreasingDecreasingFunction : FunctionBase
    {
        public override string   Name       => "Возрастающая или убывающая";
        public override string   Formula    => "x₁ < x₂ → f(x₁) < f(x₂)";
        public override string[] Keywords   => new[] { "возрастающая", "убывающая", "функция" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Возрастающая и убывающая функция\n\n" +
                    "Функция возрастает — при увеличении x значение y тоже растёт.\n" +
                    "Функция убывает  — при увеличении x значение y уменьшается.\n\n" +
                    "Пример — возрастающая:\n" +
                    "  x:  1   2   3   4\n" +
                    "  y:  3   6   9   12\n\n" +
                    "Пример — убывающая:\n" +
                    "  x:  10  20  30  40\n" +
                    "  y:  120 60  40  30\n\n" +
                    "✏️ Введи строку x через запятую (по возрастанию):",
                Validate = ValidateNumericRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:",
                Validate = ValidateNumericRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);
            var sb = new StringBuilder();

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            if (!TableSteps.TryParseDoubles(xs, out var xd) ||
                !TableSteps.TryParseDoubles(ys, out var yd))
                return "⚠️ Все значения должны быть числами.";

            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            sb.AppendLine("Анализируем пары соседних значений:");
            var changes = new List<string>();
            for (int i = 0; i < xd.Count - 1; i++)
            {
                double dy    = yd[i + 1] - yd[i];
                string arrow = dy > 0 ? "↑" : dy < 0 ? "↓" : "→";
                string word  = dy > 0 ? "растёт" : dy < 0 ? "убывает" : "не меняется";
                changes.Add(arrow);
                sb.AppendLine($"  x: {xs[i]} → {xs[i+1]}  |  y: {ys[i]} → {ys[i+1]}  ({word} {arrow})");
            }

            sb.AppendLine();

            bool allUp   = changes.All(c => c == "↑");
            bool allDown = changes.All(c => c == "↓");
            bool allSame = changes.All(c => c == "→");

            if      (allUp)   sb.AppendLine("📌 Функция ВОЗРАСТАЮЩАЯ на всей области.");
            else if (allDown) sb.AppendLine("📌 Функция УБЫВАЮЩАЯ на всей области.");
            else if (allSame) sb.AppendLine("📌 Функция ПОСТОЯННАЯ.");
            else
            {
                sb.AppendLine("📌 Функция не является ни возрастающей, ни убывающей.");
                var inc = new List<string>();
                var dec = new List<string>();
                for (int i = 0; i < changes.Count; i++)
                {
                    if (changes[i] == "↑") inc.Add($"[{xs[i]}; {xs[i+1]}]");
                    if (changes[i] == "↓") dec.Add($"[{xs[i]}; {xs[i+1]}]");
                }
                if (inc.Any()) sb.AppendLine($"Возрастает на: {string.Join(", ", inc)}");
                if (dec.Any()) sb.AppendLine($"Убывает на:    {string.Join(", ", dec)}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string? ValidateNumericRow(string s)
        {
            var err = TableSteps.ValidateRow(s);
            if (err is not null) return err;
            foreach (var p in TableSteps.ParseRow(s))
                if (!double.TryParse(p.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    return $"«{p}» — не число.";
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 1: Анализ функции по таблице
    //  — является ли функцией
    //  — область определения и множество значений
    //  — возрастающая / убывающая / смешанная
    // ═══════════════════════════════════════════════════════════════

    public class TableAnalysisFunction : FunctionBase
    {
        public override string   Name       => "Анализ функции по таблице";
        public override string   Formula    => "D(f), E(f), возрастание/убывание";
        public override string[] Keywords   => new[] { "таблица", "функция", "анализ", "возрастающая", "убывающая", "область" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Анализ функции по таблице\n\n" +
                    "По таблице можно узнать:\n" +
                    "  · является ли зависимость функцией\n" +
                    "  · область определения D(f) — все значения x\n" +
                    "  · множество значений E(f) — все значения y\n" +
                    "  · возрастает или убывает функция\n\n" +
                    "Пример:\n" +
                    "  x:  1   2   3   4   5\n" +
                    "  y:  3   6   9   12  15\n\n" +
                    "✏️ Введи строку x через запятую:\n" +
                    "  Пример: 1, 2, 3, 4, 5",
                Validate = TableSteps.ValidateRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:\n" +
                           "  Пример: 3, 6, 9, 12, 15",
                Validate = TableSteps.ValidateRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);
            var sb = new StringBuilder();

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            // ── Таблица ───────────────────────────────────────────
            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            // ── 1. Является ли функцией ───────────────────────────
            sb.AppendLine("1️⃣ Является ли функцией?");

            var dict = new Dictionary<string, List<string>>();
            for (int i = 0; i < xs.Count; i++)
            {
                if (!dict.ContainsKey(xs[i])) dict[xs[i]] = new List<string>();
                dict[xs[i]].Add(ys[i]);
            }

            var violations = dict.Where(kv => kv.Value.Distinct().Count() > 1).ToList();
            bool isFunc    = !violations.Any();

            if (isFunc)
            {
                sb.AppendLine("   Каждому x соответствует ровно одно y ✅");
                sb.AppendLine("   → Это ФУНКЦИЯ");
            }
            else
            {
                foreach (var kv in violations)
                    sb.AppendLine($"   ❌ x = {kv.Key} даёт y = {string.Join(", ", kv.Value.Distinct())}");
                sb.AppendLine("   → Это НЕ функция");
            }

            sb.AppendLine();

            // ── 2. Область определения и множество значений ───────
            sb.AppendLine("2️⃣ Область определения и множество значений:");
            sb.AppendLine($"   D(f) = {{{string.Join("; ", xs.Distinct())}}}");
            sb.AppendLine($"   E(f) = {{{string.Join("; ", ys.Distinct())}}}");
            sb.AppendLine();

            // ── 3. Возрастание / убывание (только если числа) ─────
            if (isFunc && TableSteps.TryParseDoubles(xs, out var xd) &&
                          TableSteps.TryParseDoubles(ys, out var yd))
            {
                sb.AppendLine("3️⃣ Возрастание / убывание:");

                var changes = new List<string>();
                for (int i = 0; i < xd.Count - 1; i++)
                {
                    double dy    = yd[i + 1] - yd[i];
                    string arrow = dy > 0 ? "↑" : dy < 0 ? "↓" : "→";
                    changes.Add(arrow);
                    string word = dy > 0 ? "растёт" : dy < 0 ? "убывает" : "не меняется";
                    sb.AppendLine($"   x: {xs[i]} → {xs[i+1]}  |  y: {ys[i]} → {ys[i+1]}  ({word} {arrow})");
                }

                sb.AppendLine();

                bool allUp   = changes.All(c => c == "↑");
                bool allDown = changes.All(c => c == "↓");
                bool allSame = changes.All(c => c == "→");

                if      (allUp)   sb.AppendLine("   📌 Функция ВОЗРАСТАЮЩАЯ на всей области определения.");
                else if (allDown) sb.AppendLine("   📌 Функция УБЫВАЮЩАЯ на всей области определения.");
                else if (allSame) sb.AppendLine("   📌 Функция ПОСТОЯННАЯ.");
                else
                {
                    sb.AppendLine("   📌 Функция не является ни возрастающей, ни убывающей.");
                    var inc = new List<string>();
                    var dec = new List<string>();
                    for (int i = 0; i < changes.Count; i++)
                    {
                        if (changes[i] == "↑") inc.Add($"[{xs[i]}; {xs[i+1]}]");
                        if (changes[i] == "↓") dec.Add($"[{xs[i]}; {xs[i+1]}]");
                    }
                    if (inc.Any()) sb.AppendLine($"   Возрастает на: {string.Join(", ", inc)}");
                    if (dec.Any()) sb.AppendLine($"   Убывает на:    {string.Join(", ", dec)}");
                }

                sb.AppendLine();

                // ── 4. Нули функции ───────────────────────────────
                sb.AppendLine("4️⃣ Нули функции (y = 0):");
                var zeros = new List<string>();
                for (int i = 0; i < xd.Count; i++)
                    if (Math.Abs(yd[i]) < 1e-9) zeros.Add(xs[i]);

                // Линейная интерполяция: нуль между соседними точками
                for (int i = 0; i < xd.Count - 1; i++)
                {
                    if (yd[i] * yd[i + 1] < 0)
                    {
                        double zeroX = xd[i] - yd[i] * (xd[i + 1] - xd[i]) / (yd[i + 1] - yd[i]);
                        zeros.Add($"≈{zeroX:G6} (между {xs[i]} и {xs[i+1]})");
                    }
                }

                if (zeros.Count == 0)
                    sb.AppendLine("   Нулей нет (y не обращается в 0 ни в одной точке таблицы).");
                else
                    foreach (var z in zeros)
                        sb.AppendLine($"   x = {z}");

                sb.AppendLine();

                // ── 5. Знак функции ───────────────────────────────
                sb.AppendLine("5️⃣ Знак функции:");
                var pos = new List<string>();
                var neg = new List<string>();
                for (int i = 0; i < xd.Count; i++)
                {
                    if (yd[i] > 1e-9)  pos.Add(xs[i]);
                    if (yd[i] < -1e-9) neg.Add(xs[i]);
                }
                sb.AppendLine(pos.Count > 0
                    ? $"   y > 0 при x = {string.Join(", ", pos)}"
                    : "   y > 0: нет таких точек в таблице");
                sb.AppendLine(neg.Count > 0
                    ? $"   y < 0 при x = {string.Join(", ", neg)}"
                    : "   y < 0: нет таких точек в таблице");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 2: Область определения функции
    //
    //  Пользователь вводит правую часть: 3x-5  или  1/(x^2-5x+6)
    //  Бот сам находит знаменатель, решает уравнение знаменатель=0
    //  и выводит область определения.
    //
    //  Случаи:
    //    · нет дроби          → D = (−∞; +∞)
    //    · deg знаменателя=0  → константа → D = (−∞; +∞) или функция не существует
    //    · deg = 1            → одно исключение
    //    · deg = 2            → дискриминант, 0/1/2 исключения
    //    · deg > 2            → числовой поиск корней
    // ═══════════════════════════════════════════════════════════════

    public class DomainFunction : FunctionBase
    {
        public override string   Name       => "Область определения";
        public override string   Formula    => "D(f): при каких x функция существует";
        public override string[] Keywords   => new[] { "область определения", "D(f)", "функция" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Область определения функции\n\n" +
                    "Область определения — все x, при которых функция имеет смысл.\n\n" +
                    "Главное правило: на ноль делить нельзя.\n" +
                    "Поэтому если в знаменателе стоит x —\n" +
                    "нужно найти при каких x знаменатель = 0 и исключить их.\n\n" +
                    "Примеры:\n" +
                    "  3x − 5           → любой x,  D = (−∞; +∞)\n" +
                    "  1/(2x-6)         → x ≠ 3\n" +
                    "  x/(x^2-5x+6)     → x ≠ 2 и x ≠ 3\n" +
                    "  1/(x^3-x)        → x ≠ −1, 0, 1\n\n" +
                    "Как записывать:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · дробь: числитель/(знаменатель)\n\n" +
                    "✏️ Введи правую часть формулы (после y =):",
                Validate = ValidateInput
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string raw    = answers[0].Trim().Replace(" ", "").Replace("−", "-");
            string? denom = ExtractDenominator(raw);
            var sb        = new StringBuilder();

            sb.AppendLine($"✅ y = {answers[0].Trim()}");
            sb.AppendLine();

            // ── Нет знаменателя ───────────────────────────────────
            if (denom is null)
            {
                sb.AppendLine("Знаменателя нет — это многочлен.");
                sb.AppendLine("Ограничений на x не существует.");
                sb.AppendLine();
                sb.AppendLine("📌 Область определения: (−∞; +∞)");
                sb.AppendLine("   Функция определена при любом x.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine($"Знаменатель: {denom}");
            sb.AppendLine("Условие: знаменатель ≠ 0");
            sb.AppendLine();

            List<PolyTerm> denomTerms;
            try { denomTerms = PolyParser.Reduce(PolyParser.Parse(denom)); }
            catch
            {
                sb.AppendLine("⚠️ Не удалось разобрать знаменатель.");
                sb.AppendLine("Попробуй записать иначе, например: x^2-5x+6");
                return sb.ToString().TrimEnd();
            }

            int deg = PolyParser.PolynomialDegree(denomTerms);

            // ── Константа ─────────────────────────────────────────
            if (deg == 0)
            {
                long c = denomTerms.FirstOrDefault(t => t.Degree == 0).Coeff;
                if (c == 0)
                    sb.AppendLine("⚠️ Знаменатель равен нулю — функция не существует ни при каком x.");
                else
                {
                    sb.AppendLine($"Знаменатель = {c} — константа, никогда не равна нулю.");
                    sb.AppendLine();
                    sb.AppendLine("📌 Область определения: (−∞; +∞)");
                }
                return sb.ToString().TrimEnd();
            }

            // ── Линейный знаменатель: kx + b ─────────────────────
            if (deg == 1)
            {
                long k = denomTerms.FirstOrDefault(t => t.Degree == 1).Coeff;
                long b = denomTerms.FirstOrDefault(t => t.Degree == 0).Coeff;

                sb.AppendLine($"Шаг 1. Решаем: {PolyParser.Format(denomTerms)} = 0");
                if (b != 0)
                {
                    sb.AppendLine($"  {k}x = {-b}");
                    sb.AppendLine($"  x = {FmtFrac(-b, k)}");
                }
                else
                    sb.AppendLine($"  x = 0");

                string badX = FmtFrac(-b, k);
                sb.AppendLine();
                sb.AppendLine($"Шаг 2. Исключаем x = {badX}.");
                sb.AppendLine();
                sb.AppendLine($"📌 Область определения: (−∞; {badX}) ∪ ({badX}; +∞)");
                sb.AppendLine($"   Все числа, кроме x = {badX}");
                return sb.ToString().TrimEnd();
            }

            // ── Квадратный знаменатель: ax² + bx + c ─────────────
            if (deg == 2)
            {
                long a2 = denomTerms.FirstOrDefault(t => t.Degree == 2).Coeff;
                long a1 = denomTerms.FirstOrDefault(t => t.Degree == 1).Coeff;
                long a0 = denomTerms.FirstOrDefault(t => t.Degree == 0).Coeff;

                sb.AppendLine($"Шаг 1. Решаем: {PolyParser.Format(denomTerms)} = 0");
                sb.AppendLine($"  a = {a2},  b = {a1},  c = {a0}");

                long disc = a1 * a1 - 4 * a2 * a0;
                sb.AppendLine($"  D = {a1}² − 4·{a2}·{a0} = {disc}");
                sb.AppendLine();

                if (disc < 0)
                {
                    sb.AppendLine("  D < 0 — корней нет.");
                    sb.AppendLine("  Знаменатель никогда не равен нулю.");
                    sb.AppendLine();
                    sb.AppendLine("📌 Область определения: (−∞; +∞)");
                }
                else if (disc == 0)
                {
                    string x1 = FmtFrac(-a1, 2 * a2);
                    sb.AppendLine($"  D = 0 → один корень: x = {x1}");
                    sb.AppendLine();
                    sb.AppendLine($"Шаг 2. Исключаем x = {x1}.");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Область определения: (−∞; {x1}) ∪ ({x1}; +∞)");
                    sb.AppendLine($"   Все числа, кроме x = {x1}");
                }
                else
                {
                    double sqrtD   = Math.Sqrt((double)disc);
                    bool   exact   = Math.Abs(sqrtD - Math.Round(sqrtD)) < 1e-9;

                    if (exact)
                    {
                        long   sq  = (long)Math.Round(sqrtD);
                        double x1d = (double)(-a1 - sq) / (2 * a2);
                        double x2d = (double)(-a1 + sq) / (2 * a2);
                        if (x1d > x2d) (x1d, x2d) = (x2d, x1d);

                        // Строки уже соответствуют отсортированным x1d/x2d:
                        // x1d = (-a1 - sq) / 2a2 — меньший корень (если a2 > 0)
                        // Убеждаемся через числовой порядок
                        string x1 = x1d <= x2d ? FmtFrac(-a1 - sq, 2 * a2) : FmtFrac(-a1 + sq, 2 * a2);
                        string x2 = x1d <= x2d ? FmtFrac(-a1 + sq, 2 * a2) : FmtFrac(-a1 - sq, 2 * a2);

                        sb.AppendLine($"  √D = {sq}");
                        sb.AppendLine($"  x₁ = ({-a1} − {sq}) / (2·{a2}) = {x1}");
                        sb.AppendLine($"  x₂ = ({-a1} + {sq}) / (2·{a2}) = {x2}");
                        sb.AppendLine();
                        sb.AppendLine($"Шаг 2. Исключаем x = {x1} и x = {x2}.");
                        sb.AppendLine();
                        sb.AppendLine($"📌 Область определения:");
                        sb.AppendLine($"   (−∞; {x1}) ∪ ({x1}; {x2}) ∪ ({x2}; +∞)");
                        sb.AppendLine($"   Все числа, кроме x = {x1} и x = {x2}");
                    }
                    else
                    {
                        sb.AppendLine($"  √D = √{disc} ≈ {sqrtD:F4}");
                        sb.AppendLine($"  x₁ = ({-a1} − √{disc}) / (2·{a2})");
                        sb.AppendLine($"  x₂ = ({-a1} + √{disc}) / (2·{a2})");
                        sb.AppendLine();
                        sb.AppendLine("Шаг 2. Исключаем оба корня.");
                        sb.AppendLine();
                        sb.AppendLine("📌 Область определения: все x, кроме x₁ и x₂ (см. выше)");
                    }
                }
                return sb.ToString().TrimEnd();
            }

            // ── Степень > 2: числовой поиск корней ───────────────
            sb.AppendLine($"Знаменатель — многочлен степени {deg}.");
            sb.AppendLine("Ищем корни на интервале [−100; 100]:");
            sb.AppendLine();

            var roots = FindRoots(denomTerms, -100, 100, 10000);

            if (roots.Count == 0)
            {
                sb.AppendLine("Корней не найдено — знаменатель не обращается в ноль.");
                sb.AppendLine();
                sb.AppendLine("📌 Область определения: (−∞; +∞)");
            }
            else
            {
                sb.AppendLine($"Найдено корней: {roots.Count}");
                foreach (var r in roots)
                    sb.AppendLine($"  x ≈ {r:F4}");
                sb.AppendLine();

                // Строим запись области
                var parts  = new List<string>();
                double prev = double.NegativeInfinity;
                foreach (var r in roots)
                {
                    string lo  = prev == double.NegativeInfinity ? "−∞" : $"{prev:F4}";
                    string hi  = $"{r:F4}";
                    parts.Add($"({lo}; {hi})");
                    prev = r;
                }
                parts.Add($"({prev:F4}; +∞)");

                sb.AppendLine("📌 Область определения:");
                sb.AppendLine("   " + string.Join(" ∪ ", parts));
            }

            return sb.ToString().TrimEnd();
        }

        // ─── Вспомогательное ─────────────────────────────────────

        private static string? ExtractDenominator(string s)
        {
            int slash = s.IndexOf('/');
            if (slash < 0) return null;
            string after = s.Substring(slash + 1).Trim();
            if (after.StartsWith("(") && after.EndsWith(")"))
                after = after.Substring(1, after.Length - 2);
            return string.IsNullOrWhiteSpace(after) ? null : after;
        }

        private static string FmtFrac(long num, long den)
        {
            if (den == 0) return "∞";
            if (num == 0) return "0";
            long g = Gcd(Math.Abs(num), Math.Abs(den));
            long n = num / g, d = den / g;
            if (d < 0) { n = -n; d = -d; }
            return d == 1 ? n.ToString() : $"{n}/{d}";
        }

        private static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

        private static List<double> FindRoots(List<PolyTerm> terms, double lo, double hi, int steps)
        {
            var    roots = new List<double>();
            double step  = (hi - lo) / steps;
            double Eval(double x) => terms.Sum(t => t.Coeff * Math.Pow(x, t.Degree));

            for (double x = lo; x < hi; x += step)
            {
                double f1 = Eval(x), f2 = Eval(x + step);
                if (Math.Abs(f1) < 1e-9)
                {
                    double r = Math.Round(x, 4);
                    if (roots.All(e => Math.Abs(e - r) > 1e-6)) roots.Add(r);
                    continue;
                }
                if (f1 * f2 < 0)
                {
                    double a = x, b = x + step;
                    for (int i = 0; i < 60; i++)
                    {
                        double mid = (a + b) / 2;
                        if (Eval(a) * Eval(mid) <= 0) b = mid; else a = mid;
                    }
                    double root = Math.Round((a + b) / 2, 4);
                    if (roots.All(e => Math.Abs(e - root) > 1e-6)) roots.Add(root);
                }
            }
            roots.Sort();
            return roots;
        }

        private static string? ValidateInput(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 3x-5  или  1/(x^2-5x+6)";

            string raw    = s.Trim().Replace(" ", "").Replace("−", "-");
            string? denom = ExtractDenominator(raw);

            if (denom is null)
            {
                try   { PolyParser.Parse(raw); return null; }
                catch (FormatException ex)
                    { return $"Не могу разобрать: {ex.Message}\nПример: 3x-5  или  1/(x^2-5x+6)"; }
            }

            // Валидируем и числитель, и знаменатель
            int slash = raw.IndexOf('/');
            string numPart = slash >= 0 ? raw.Substring(0, slash) : string.Empty;
            if (!string.IsNullOrEmpty(numPart))
            {
                try   { PolyParser.Parse(numPart); }
                catch (FormatException ex)
                    { return $"Не могу разобрать числитель: {ex.Message}"; }
            }
            try   { PolyParser.Parse(denom); return null; }
            catch (FormatException ex)
                { return $"Не могу разобрать знаменатель: {ex.Message}"; }
        }
    }
    // ═══════════════════════════════════════════════════════════════
    //  Область определения и множество значений по таблице
    // ═══════════════════════════════════════════════════════════════

    public class DomainFromTableFunction : FunctionBase
    {
        public override string   Name       => "Область определения по таблице";
        public override string   Formula    => "D(f) = {x₁; x₂; ...},  E(f) = {y₁; y₂; ...}";
        public override string[] Keywords   => new[] { "область определения", "множество значений", "таблица" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Область определения и множество значений по таблице\n\n" +
                    "Область определения D(f) — все значения x из таблицы.\n" +
                    "Множество значений E(f)   — все значения y из таблицы.\n\n" +
                    "Пример:\n" +
                    "  x:  15  18  20  21  30  40\n" +
                    "  y:  30  36  40  42  60  120\n" +
                    "  D(f) = {15; 18; 20; 21; 30; 40}\n" +
                    "  E(f) = {30; 36; 40; 42; 60; 120}\n\n" +
                    "✏️ Введи строку x через запятую:",
                Validate = TableSteps.ValidateRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:",
                Validate = TableSteps.ValidateRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            var sb = new StringBuilder();
            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            var domain = xs.Distinct().ToList();
            var range  = ys.Distinct().ToList();

            sb.AppendLine($"📌 Область определения D(f) = {{{string.Join("; ", domain)}}}");
            sb.AppendLine($"📌 Множество значений  E(f) = {{{string.Join("; ", range)}}}");

            if (domain.Count < xs.Count)
            {
                sb.AppendLine();
                sb.AppendLine("Заметка: некоторые x повторяются —");
                sb.AppendLine("в D(f) каждое значение записывается один раз.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Определить закономерность по таблице
    //  Проверяет: y=kx+b, y=kx, y=kx², y=kx²+b, y=kx³, y=k/x, y=|x|
    // ═══════════════════════════════════════════════════════════════

    public class DetectFormulaFunction : FunctionBase
    {
        public override string   Name       => "Назвать функцию по таблице";
        public override string   Formula    => "y = ? (бот находит сам)";
        public override string[] Keywords   => new[] { "закономерность", "формула", "определить", "найти" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Определить закономерность по таблице\n\n" +
                    "Бот проверит все известные виды зависимостей:\n" +
                    "  y = kx + b   (линейная)\n" +
                    "  y = kx²      (квадратичная)\n" +
                    "  y = kx² + b  (квадратичная со сдвигом)\n" +
                    "  y = kx³      (кубическая)\n" +
                    "  y = k/x      (обратная пропорция)\n" +
                    "  y = k·|x|    (модуль)\n\n" +
                    "И выведет подходящую формулу.\n\n" +
                    "✏️ Введи строку x через запятую:\n" +
                    "  Пример: 1, 2, 3, 4, 5",
                Validate = ValidateNumericRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:",
                Validate = ValidateNumericRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            if (!TableSteps.TryParseDoubles(xs, out var xd) ||
                !TableSteps.TryParseDoubles(ys, out var yd))
                return "⚠️ Все значения должны быть числами.";

            var sb = new StringBuilder();
            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            var matches = new List<string>();

            // ── y = kx (прямая пропорция) ─────────────────────────
            if (xd.All(x => Math.Abs(x) > 1e-9))
            {
                var ratios = xd.Select((x, i) => yd[i] / x).ToList();
                if (ratios.All(r => Math.Abs(r - ratios[0]) < 1e-9))
                    matches.Add($"y = {Fmt(ratios[0])}x");
            }

            // ── y = kx + b (линейная) ─────────────────────────────
            {
                var diffs = Enumerable.Range(0, yd.Count - 1).Select(i => yd[i+1] - yd[i]).ToList();
                double xStep = xd.Count >= 2 ? xd[1] - xd[0] : 0;
                if (diffs.All(d => Math.Abs(d - diffs[0]) < 1e-9) && xd.Count >= 2
                    && Math.Abs(xStep) > 1e-9)
                {
                    double k = (yd[1] - yd[0]) / xStep;
                    double b = yd[0] - k * xd[0];
                    if (Fits(xd, yd, x => k * x + b))
                    {
                        // b == 0 означает прямую пропорцию — уже обрабатывается веткой y=kx
                        if (Math.Abs(b) > 1e-9)
                        {
                            string formula = b > 0
                                ? $"y = {Fmt(k)}x + {Fmt(b)}"
                                : $"y = {Fmt(k)}x − {Fmt(-b)}";
                            if (!matches.Contains(formula)) matches.Add(formula);
                        }
                    }
                }
            }

            // ── y = kx² (квадратичная без сдвига) ─────────────────
            if (xd.All(x => Math.Abs(x) > 1e-9))
            {
                var ratios = xd.Select((x, i) => yd[i] / (x * x)).ToList();
                if (ratios.All(r => Math.Abs(r - ratios[0]) < 1e-9))
                    matches.Add($"y = {Fmt(ratios[0])}x²");
            }

            // ── y = kx² + b ───────────────────────────────────────
            if (xd.Count >= 3 && xd.All(x => Math.Abs(x) > 1e-9))
            {
                // Берём три точки и решаем систему
                double x1 = xd[0], y1 = yd[0];
                double x2 = xd[1], y2 = yd[1];
                double denom = x1*x1 - x2*x2;
                if (Math.Abs(denom) > 1e-9)
                {
                    double k = (y1 - y2) / denom;
                    double b = y1 - k * x1 * x1;
                    if (b != 0 && Fits(xd, yd, x => k * x * x + b))
                    {
                        string formula = b > 0
                            ? $"y = {Fmt(k)}x² + {Fmt(b)}"
                            : $"y = {Fmt(k)}x² − {Fmt(-b)}";
                        matches.Add(formula);
                    }
                }
            }

            // ── y = kx³ (кубическая) ──────────────────────────────
            if (xd.All(x => Math.Abs(x) > 1e-9))
            {
                var ratios = xd.Select((x, i) => yd[i] / (x * x * x)).ToList();
                if (ratios.All(r => Math.Abs(r - ratios[0]) < 1e-9))
                    matches.Add($"y = {Fmt(ratios[0])}x³");
            }

            // ── y = k/x (обратная пропорция) ──────────────────────
            if (xd.All(x => Math.Abs(x) > 1e-9))
            {
                var products = xd.Select((x, i) => yd[i] * x).ToList();
                if (products.All(p => Math.Abs(p - products[0]) < 1e-9))
                    matches.Add($"y = {Fmt(products[0])}/x");
            }

            // ── y = k·|x| (модуль) ────────────────────────────────
            if (xd.All(x => Math.Abs(x) > 1e-9))
            {
                var ratios = xd.Select((x, i) => yd[i] / Math.Abs(x)).ToList();
                if (ratios.All(r => Math.Abs(r - ratios[0]) < 1e-9) && ratios[0] >= 0)
                {
                    string candidate = $"y = {Fmt(ratios[0])}·|x|";
                    if (!matches.Any(m => m == $"y = {Fmt(ratios[0])}x"))
                        matches.Add(candidate);
                }
            }

            // ── Вывод ─────────────────────────────────────────────
            if (matches.Count == 0)
            {
                sb.AppendLine("Ни одна из известных закономерностей не подошла.");
                sb.AppendLine("Проверь правильность введённых данных");
                sb.AppendLine("или добавь больше точек.");
            }
            else
            {
                sb.AppendLine($"Найдено подходящих формул: {matches.Count}");
                sb.AppendLine();
                foreach (var m in matches)
                    sb.AppendLine($"  📌 {m}");

                if (matches.Count > 1)
                {
                    sb.AppendLine();
                    sb.AppendLine("Несколько формул подошли — добавь больше точек");
                    sb.AppendLine("чтобы точнее определить зависимость.");
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static bool Fits(List<double> xs, List<double> ys, Func<double, double> f)
        {
            for (int i = 0; i < xs.Count; i++)
                if (Math.Abs(f(xs[i]) - ys[i]) > 1e-6) return false;
            return true;
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);

        private static string? ValidateNumericRow(string s)
        {
            var err = TableSteps.ValidateRow(s);
            if (err is not null) return err;
            foreach (var p in TableSteps.ParseRow(s))
                if (!double.TryParse(p.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    return $"«{p}» — не число.";
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Угадать формулу по таблице
    // ═══════════════════════════════════════════════════════════════

    public class FormulaFromTableFunction : FunctionBase
    {
        public override string   Name       => "Угадать формулу по таблице";
        public override string   Formula    => "y = kx + b";
        public override string[] Keywords   => new[] { "формула", "таблица", "угадать", "найти" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти формулу по таблице\n\n" +
                    "Смотрим на закономерность между x и y.\n\n" +
                    "Пример:\n" +
                    "  x:  1   2   3   4   5\n" +
                    "  y:  3   6   9   12  15\n" +
                    "  y = 3x (каждый y = x · 3)\n\n" +
                    "Пример с b:\n" +
                    "  x:  1   2   3   4\n" +
                    "  y:  5   7   9   11\n" +
                    "  Разница y: 2, 2, 2 → k = 2\n" +
                    "  При x=1: y=5 → b = 5 - 2·1 = 3\n" +
                    "  y = 2x + 3\n\n" +
                    "✏️ Введи строку x через запятую:",
                Validate = ValidateNumericRow
            },
            new InputStep
            {
                Question = "✏️ Введи строку y через запятую:",
                Validate = ValidateNumericRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = TableSteps.ParseRow(answers[0]);
            var ys = TableSteps.ParseRow(answers[1]);
            var sb = new StringBuilder();

            if (xs.Count != ys.Count)
                return $"⚠️ Количество x ({xs.Count}) не совпадает с количеством y ({ys.Count}).";

            if (!TableSteps.TryParseDoubles(xs, out var xd) ||
                !TableSteps.TryParseDoubles(ys, out var yd))
                return "⚠️ Все значения должны быть числами.";

            sb.AppendLine("Введённая таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");
            sb.AppendLine();

            // ── Шаг 1: разности y ────────────────────────────────
            var diffs = new List<double>();
            for (int i = 0; i < yd.Count - 1; i++)
                diffs.Add(yd[i + 1] - yd[i]);

            bool constDiff = diffs.All(d => Math.Abs(d - diffs[0]) < 1e-9);

            if (constDiff)
            {
                double k = diffs[0];
                double b = yd[0] - k * xd[0];

                sb.AppendLine("Шаг 1. Смотрим на разности y:");
                sb.AppendLine($"  {string.Join(", ", diffs.Select(d => d.ToString("G6")))}");
                sb.AppendLine($"  Разность постоянная → k = {Fmt(k)}");
                sb.AppendLine();
                sb.AppendLine($"Шаг 2. Находим b:");
                sb.AppendLine($"  b = y − k·x = {Fmt(yd[0])} − {Fmt(k)}·{Fmt(xd[0])} = {Fmt(b)}");
                sb.AppendLine();

                // Проверяем все точки
                sb.AppendLine("Шаг 3. Проверяем по всем точкам:");
                bool allOk = true;
                for (int i = 0; i < xd.Count; i++)
                {
                    double yCalc = k * xd[i] + b;
                    bool   ok    = Math.Abs(yCalc - yd[i]) < 1e-9;
                    if (!ok) allOk = false;
                    sb.AppendLine($"  x={Fmt(xd[i])}: {Fmt(k)}·{Fmt(xd[i])}+{Fmt(b)} = {Fmt(yCalc)} {(ok ? "✅" : $"≠ {Fmt(yd[i])} ❌")}");
                }

                sb.AppendLine();
                if (allOk)
                {
                    string formula = b == 0 ? $"y = {Fmt(k)}x"
                                   : b > 0  ? $"y = {Fmt(k)}x + {Fmt(b)}"
                                   :          $"y = {Fmt(k)}x − {Fmt(-b)}";
                    sb.AppendLine($"📌 Формула: {formula}");
                }
                else
                    sb.AppendLine("📌 Линейная формула не подходит — зависимость нелинейная.");
            }
            else
            {
                // Проверяем y = kx (пропорция)
                var ratios = new List<double>();
                bool canDiv = xd.All(x => Math.Abs(x) > 1e-9);
                if (canDiv)
                {
                    for (int i = 0; i < xd.Count; i++)
                        ratios.Add(yd[i] / xd[i]);

                    bool constRatio = ratios.All(r => Math.Abs(r - ratios[0]) < 1e-9);
                    if (constRatio)
                    {
                        sb.AppendLine("Шаг 1. Проверяем y/x:");
                        sb.AppendLine($"  {string.Join(", ", ratios.Select(r => Fmt(r)))}");
                        sb.AppendLine($"  Отношение постоянное → k = {Fmt(ratios[0])}");
                        sb.AppendLine();
                        sb.AppendLine($"📌 Формула: y = {Fmt(ratios[0])}x");
                        return sb.ToString().TrimEnd();
                    }
                }

                // Проверяем y = x²
                var sq = xd.Select((x, i) => new { ratio = yd[i] / (x * x), x }).Where(p => Math.Abs(p.x) > 1e-9).ToList();
                if (sq.All(p => Math.Abs(p.ratio - sq[0].ratio) < 1e-9))
                {
                    sb.AppendLine("Шаг 1. Проверяем y/x²:");
                    sb.AppendLine($"  Отношение постоянное → k = {Fmt(sq[0].ratio)}");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Формула: y = {(sq[0].ratio == 1 ? "" : Fmt(sq[0].ratio))}x²");
                    return sb.ToString().TrimEnd();
                }

                sb.AppendLine("Шаг 1. Разности y непостоянны:");
                sb.AppendLine($"  {string.Join(", ", diffs.Select(d => Fmt(d)))}");
                sb.AppendLine();
                sb.AppendLine("📌 По данной таблице линейная формула y = kx + b не подходит.");
                sb.AppendLine("Попробуй добавить больше точек или проверь ввод.");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);

        private static string? ValidateNumericRow(string s)
        {
            var err = TableSteps.ValidateRow(s);
            if (err is not null) return err;
            foreach (var p in TableSteps.ParseRow(s))
                if (!double.TryParse(p.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    return $"«{p}» — не число.";
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Заполнить таблицу по формуле
    // ═══════════════════════════════════════════════════════════════

    public class TableFromFormulaFunction : FunctionBase
    {
        public override string   Name       => "Заполнить таблицу по формуле";
        public override string   Formula    => "y = f(x) → подставляем x, находим y";
        public override string[] Keywords   => new[] { "таблица", "формула", "заполнить", "подставить" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Заполнить таблицу по формуле\n\n" +
                    "Подставляем каждое x в формулу и находим y.\n\n" +
                    "Пример: y = 2x + 1\n" +
                    "  x = 1 → y = 2·1 + 1 = 3\n" +
                    "  x = 2 → y = 2·2 + 1 = 5\n" +
                    "  x = 3 → y = 2·3 + 1 = 7\n\n" +
                    "Как вводить формулу:\n" +
                    "  · x² → x^2,  x³ → x^3\n" +
                    "  · Пример: 2x+1  или  x^2-3x  или  1/(x-2)\n\n" +
                    "✏️ Введи формулу (правую часть после y =):",
                Validate = ValidateFormula
            },
            new InputStep
            {
                Question = "✏️ Введи значения x через запятую:\n" +
                           "  Пример: 1, 2, 3, 4, 5",
                Validate = TableSteps.ValidateRow
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string formula = answers[0].Trim();
            var    xs      = TableSteps.ParseRow(answers[1]);
            var    sb      = new StringBuilder();

            sb.AppendLine($"✅ y = {formula}");
            sb.AppendLine();
            sb.AppendLine("Подставляем каждое x:");

            var ys = new List<string>();
            foreach (var xStr in xs)
            {
                if (!double.TryParse(xStr.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out double xVal))
                {
                    sb.AppendLine($"  x = {xStr} → ⚠️ не число");
                    ys.Add("?");
                    continue;
                }

                try
                {
                    double yVal = EvalFormula(formula, xVal);
                    string yStr = Fmt(yVal);
                    sb.AppendLine($"  x = {xStr} → y = {yStr}");
                    ys.Add(yStr);
                }
                catch
                {
                    sb.AppendLine($"  x = {xStr} → ⚠️ не определено (деление на 0?)");
                    ys.Add("—");
                }
            }

            sb.AppendLine();
            sb.AppendLine("📌 Таблица:");
            sb.AppendLine($"  x: {string.Join("  ", xs)}");
            sb.AppendLine($"  y: {string.Join("  ", ys)}");

            return sb.ToString().TrimEnd();
        }

        // ─── Вычислить формулу при заданном x ────────────────────
        // Использует PolyParser для числителя и знаменателя
        private static double EvalFormula(string formula, double x)
        {
            string raw = formula.Replace(" ", "").Replace("−", "-");
            int    slash = raw.IndexOf('/');

            if (slash < 0)
            {
                var terms = PolyParser.Parse(raw);
                return terms.Sum(t => t.Coeff * Math.Pow(x, t.Degree));
            }

            string numStr   = raw.Substring(0, slash);
            string denomStr = raw.Substring(slash + 1);
            if (denomStr.StartsWith("(") && denomStr.EndsWith(")"))
                denomStr = denomStr.Substring(1, denomStr.Length - 2);

            double num   = PolyParser.Parse(numStr).Sum(t => t.Coeff * Math.Pow(x, t.Degree));
            double denom = PolyParser.Parse(denomStr).Sum(t => t.Coeff * Math.Pow(x, t.Degree));

            if (Math.Abs(denom) < 1e-12) throw new DivideByZeroException();
            return num / denom;
        }

        private static string Fmt(double v) =>
            v == Math.Floor(v) && Math.Abs(v) < 1e12
                ? ((long)v).ToString()
                : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);

        private static string? ValidateFormula(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 2x+1  или  x^2-3x";
            string raw = s.Trim().Replace(" ", "").Replace("−", "-");
            int slash  = raw.IndexOf('/');
            if (slash < 0)
            {
                try   { PolyParser.Parse(raw); return null; }
                catch (FormatException ex) { return $"Не могу разобрать: {ex.Message}"; }
            }
            // Валидируем числитель и знаменатель раздельно
            string numPart   = raw.Substring(0, slash);
            string denomPart = raw.Substring(slash + 1).Trim('(', ')');
            if (!string.IsNullOrEmpty(numPart))
            {
                try   { PolyParser.Parse(numPart); }
                catch (FormatException ex) { return $"Не могу разобрать числитель: {ex.Message}"; }
            }
            try   { PolyParser.Parse(denomPart); return null; }
            catch (FormatException ex) { return $"Не могу разобрать знаменатель: {ex.Message}"; }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Записать область определения по концам отрезка/луча/прямой
    //  Пользователь снимает границы с графика и вводит числами.
    // ═══════════════════════════════════════════════════════════════

    public class GraphDomainFunction : FunctionBase
    {
        public override string   Name       => "Записать область определения по графику";
        public override string   Formula    => "D(f) = [a; b] / (a; b) / [a; +∞) / ...";
        public override string[] Keywords   => new[] { "область определения", "график", "отрезок", "луч" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Область определения по графику\n\n" +
                    "Смотри на график и найди:\n" +
                    "  · самую левую точку (или −∞ если линия уходит влево)\n" +
                    "  · самую правую точку (или +∞ если линия уходит вправо)\n\n" +
                    "Примеры:\n" +
                    "  Отрезок от −2 до 5     → D(f) = [−2; 5]\n" +
                    "  Луч вправо от 0        → D(f) = [0; +∞)\n" +
                    "  Вся числовая прямая    → D(f) = (−∞; +∞)\n\n" +
                    "✏️ Введи левую границу:\n" +
                    "  Число (например: -2) или слово: бесконечность",
                Validate = ValidateBound
            },
            new InputStep
            {
                Question =
                    "✏️ Левая граница включена в область?\n" +
                    "  Введи: да  или  нет\n\n" +
                    "  Закрашенная точка на графике → да (включена, скобка [)\n" +
                    "  Пустая точка                → нет (исключена, скобка ()\n" +
                    "  Если ввёл бесконечность      → всегда нет",
                Validate = ValidateYesNo
            },
            new InputStep
            {
                Question =
                    "✏️ Введи правую границу:\n" +
                    "  Число (например: 5) или слово: бесконечность",
                Validate = ValidateBound
            },
            new InputStep
            {
                Question =
                    "✏️ Правая граница включена в область?\n" +
                    "  Введи: да  или  нет",
                Validate = ValidateYesNo
            },
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string leftRaw  = answers[0].Trim().ToLower();
            bool   leftIn   = answers[1].Trim().ToLower() == "да";
            string rightRaw = answers[2].Trim().ToLower();
            bool   rightIn  = answers[3].Trim().ToLower() == "да";

            bool leftInf  = IsInfinity(leftRaw);
            bool rightInf = IsInfinity(rightRaw);

            // Бесконечности всегда со скобками (
            if (leftInf)  leftIn  = false;
            if (rightInf) rightIn = false;

            string leftStr  = leftInf  ? "−∞" : FormatBound(leftRaw);
            string rightStr = rightInf ? "+∞" : FormatBound(rightRaw);

            string leftBracket  = leftIn  ? "[" : "(";
            string rightBracket = rightIn ? "]" : ")";

            string domain = $"{leftBracket}{leftStr}; {rightStr}{rightBracket}";

            var sb = new StringBuilder();
            sb.AppendLine("Читаем границы:");
            sb.AppendLine($"  Левая:  {leftStr}  {(leftIn ? "(включена →  [)" : "(исключена → ()")}");
            sb.AppendLine($"  Правая: {rightStr}  {(rightIn ? "(включена →  ])" : "(исключена → )")}");
            sb.AppendLine();

            sb.AppendLine("Пояснение:");
            if (leftInf && rightInf)
                sb.AppendLine("  Функция определена при любом x.");
            else if (leftInf)
                sb.AppendLine($"  Функция определена при x {(rightIn ? "≤" : "<")} {rightStr}.");
            else if (rightInf)
                sb.AppendLine($"  Функция определена при x {(leftIn ? "≥" : ">")} {leftStr}.");
            else
                sb.AppendLine($"  Функция определена при {leftStr} {(leftIn ? "≤" : "<")} x {(rightIn ? "≤" : "<")} {rightStr}.");

            sb.AppendLine();
            sb.AppendLine($"📌 D(f) = {domain}");

            return sb.ToString().TrimEnd();
        }

        private static bool IsInfinity(string s) =>
            s == "бесконечность" || s == "inf" || s == "∞" || s == "-∞" || s == "+∞";

        private static string FormatBound(string s)
        {
            s = s.Replace(',', '.');
            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double v))
            {
                return v == Math.Floor(v) && Math.Abs(v) < 1e12
                    ? ((long)v).ToString()
                    : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);
            }
            return s;
        }

        private static string? ValidateBound(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл. Введи число или слово: бесконечность";
            string t = s.Trim().ToLower().Replace(',', '.');
            if (IsInfinity(t)) return null;
            if (double.TryParse(t, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"Не понимаю «{s}». Введи число (например: -3) или слово: бесконечность";
        }

        private static string? ValidateYesNo(string s)
        {
            string t = s.Trim().ToLower();
            if (t == "да" || t == "нет") return null;
            return "Введи: да  или  нет";
        }
    }

}

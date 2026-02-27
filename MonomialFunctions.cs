using System;
using System.Collections.Generic;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Одночлен вида  k · aᵖ · bq
    // ═══════════════════════════════════════════════════════════════
    internal readonly struct Monomial
    {
        public readonly double K;
        public readonly int    Pa;
        public readonly int    Pb;

        public Monomial(double k, int pa, int pb) { K = k; Pa = pa; Pb = pb; }

        public int Degree => Pa + Pb;

        public Monomial Pow(int n) =>
            new Monomial(Math.Pow(K, n), Pa * n, Pb * n);

        public static Monomial operator *(Monomial a, Monomial b) =>
            new Monomial(a.K * b.K, a.Pa + b.Pa, a.Pb + b.Pb);

        public override string ToString()
        {
            if (K == 0) return "0";
            var sb    = new StringBuilder();
            bool hasA = Pa > 0, hasB = Pb > 0, hasVar = hasA || hasB;

            if (!hasVar)        sb.Append(Fmt(K));
            else if (K ==  1)   { }
            else if (K == -1)   sb.Append('-');
            else                sb.Append(Fmt(K));

            if (hasA) { sb.Append('a'); if (Pa > 1) sb.Append(Sup(Pa)); }
            if (hasB) { sb.Append('b'); if (Pb > 1) sb.Append(Sup(Pb)); }
            return sb.ToString();
        }

        // ── Валидаторы ────────────────────────────────────────────
        public static string? ValidateCoeff(string s, out double result)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out result))
                return null;
            return $"«{s}» не похоже на коэффициент.\n" +
                   "Коэффициент — это одно число перед буквами: целое, дробное или отрицательное.\n" +
                   "Например: 3 или -5 или 0.5 или 1 (если числа нет — пиши 1)";
        }

        public static string? ValidateDegree(string s, string varName, out int result)
        {
            if (int.TryParse(s, out result) && result >= 0)
                return null;
            return $"«{s}» не подходит для степени {varName}.\n" +
                   $"Показатель степени — целое число начиная с 0.\n" +
                   $"0 означает что переменной {varName} нет, 1 — просто {varName}, 2 — {varName}², и так далее.";
        }

        // ── Вспомогательные ──────────────────────────────────────
        public static string Fmt(double v) =>
            (v == Math.Floor(v) && !double.IsInfinity(v))
                ? ((long)v).ToString()
                : v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);

        public static string Sup(int n)
        {
            const string s = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            var sb = new StringBuilder();
            foreach (char c in n.ToString())
                sb.Append(c >= '0' && c <= '9' ? s[c - '0'] : c);
            return sb.ToString();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 1: Стандартный вид одночлена
    // ═══════════════════════════════════════════════════════════════
    public class MonomialStandardFormFunction : FunctionBase
    {
        public override string   Name       => "Стандартный вид одночлена";
        public override string   Formula    => "k·aᵖ·bq,  степень = p + q";
        public override string[] Keywords   => new[] { "стандартный", "вид", "одночлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new InputStep[]
        {
            // Шаг 0 — сколько переменных
            new InputStep
            {
                Question =
                    "📘 Что такое одночлен и его стандартный вид?\n\n" +
                    "Одночлен — это произведение числа и переменных в натуральных степенях.\n" +
                    "Например: 3a²b, -5ab³, 7a⁴ — это одночлены.\n\n" +
                    "Стандартный вид: сначала коэффициент (число), потом переменные по алфавиту.\n" +
                    "Степень одночлена — это сумма всех показателей степеней переменных.\n\n" +
                    "Пример: -2a³b²\n" +
                    "  · коэффициент: -2\n" +
                    "  · степень a: 3\n" +
                    "  · степень b: 2\n" +
                    "  · степень одночлена: 3 + 2 = 5\n\n" +
                    "Выбери сколько переменных в твоём одночлене:\n\n" +
                    "Напиши 1 — если только a (например, 4a³)\n" +
                    "Напиши 2 — если a и b (например, 4a³b²)",
                Validate = s => s == "1" || s == "2" ? null
                    : "Напиши цифру 1 или 2.\n" +
                      "1 — если в одночлене одна буква (только a)\n" +
                      "2 — если две буквы (a и b)"
            },

            // Шаг 1 — коэффициент
            new InputStep
            {
                Question =
                    "✏️ Введи коэффициент — число перед буквами.\n\n" +
                    "Коэффициент — это числовой множитель одночлена.\n" +
                    "Если число не написано — значит коэффициент равен 1 (пиши 1).\n" +
                    "Если перед буквой стоит минус — коэффициент равен -1 (пиши -1).\n\n" +
                    "Например: в 3a² коэффициент = 3, в -a² коэффициент = -1, в a² = 1",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },

            // Шаг 2 — степень a
            new InputStep
            {
                Question =
                    "✏️ Введи показатель степени переменной a.\n\n" +
                    "Это маленькая цифра сверху после буквы a.\n" +
                    "Если a есть без цифры — степень равна 1 (пиши 1).\n" +
                    "Если буквы a вообще нет в одночлене — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "a", out _)
            },

            // Шаг 3 — степень b (только для 2 переменных)
            new InputStep
            {
                Question =
                    "✏️ Введи показатель степени переменной b.\n\n" +
                    "Это маленькая цифра сверху после буквы b.\n" +
                    "Если b есть без цифры — степень равна 1 (пиши 1).\n" +
                    "Если буквы b нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "b", out _)
            },
        };

        public int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 4 : 3;

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";
            double k = Monomial.ParseDouble(answers[1]);
            int    pa = Monomial.ParseInt(answers[2]);
            int    pb = twoVars && answers.Count > 3 ? Monomial.ParseInt(answers[3]) : 0;
            var    m  = new Monomial(k, pa, pb);
            var sb = new StringBuilder();

            sb.AppendLine($"✅ Стандартный вид: {m}");
            sb.AppendLine();

            if (m.Degree == 0)
            {
                sb.AppendLine("Это числовой одночлен — переменных нет.");
                sb.AppendLine("Степень числового одночлена = 0.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Коэффициент: {Monomial.Fmt(k)}");

            if (pa > 0)
                sb.AppendLine($"  Шаг 2. Переменная a в степени {pa} → a{Monomial.Sup(pa)}");
            else
                sb.AppendLine("  Шаг 2. Переменной a нет (показатель 0)");

            if (twoVars)
            {
                if (pb > 0)
                    sb.AppendLine($"  Шаг 3. Переменная b в степени {pb} → b{Monomial.Sup(pb)}");
                else
                    sb.AppendLine("  Шаг 3. Переменной b нет (показатель 0)");
            }

            sb.AppendLine();

            if (twoVars && pa > 0 && pb > 0)
            {
                sb.AppendLine($"Степень одночлена = сумма показателей:");
                sb.AppendLine($"  {pa} + {pb} = {m.Degree}");
            }
            else
            {
                sb.AppendLine($"Степень одночлена = {m.Degree}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 {m}  — одночлен {m.Degree}-й степени");
            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 2: Степень одночлена
    // ═══════════════════════════════════════════════════════════════
    public class MonomialPowerFunction : FunctionBase
    {
        public override string   Name       => "Степень одночлена";
        public override string   Formula    => "(k·aᵖ·bq)ⁿ = kⁿ·aᵖⁿ·bqⁿ";
        public override string[] Keywords   => new[] { "степень", "одночлен", "возведение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new InputStep[]
        {
            // Шаг 0 — количество переменных
            new InputStep
            {
                Question =
                    "📘 Как возводить одночлен в степень?\n\n" +
                    "Правило: каждый множитель одночлена возводится в эту степень отдельно.\n\n" +
                    "Почему это работает — разберём на примере: (2a³)²\n" +
                    "  · (2a³)² = (2a³) · (2a³)\n" +
                    "  · Числа: 2 · 2 = 2² = 4\n" +
                    "  · Буквы: a³ · a³ = a^(3+3) = a^(3·2) = a⁶\n" +
                    "  · Итого: (2a³)² = 4a⁶\n\n" +
                    "Формула: (k·aᵖ·bq)ⁿ = kⁿ·aᵖ·ⁿ·bq·ⁿ\n" +
                    "  · коэффициент возводится в степень n\n" +
                    "  · каждый показатель перемножается на n\n\n" +
                    "Выбери сколько переменных в твоём одночлене:\n\n" +
                    "Напиши 1 — если только a (например, 3a²)\n" +
                    "Напиши 2 — если a и b (например, 3a²b)",
                Validate = s => s == "1" || s == "2" ? null
                    : "Напиши цифру 1 или 2."
            },

            // Шаг 1 — коэффициент
            new InputStep
            {
                Question =
                    "✏️ Введи коэффициент одночлена — число перед буквами.\n\n" +
                    "Если числа нет — пиши 1.\n" +
                    "Если стоит минус без числа — пиши -1.",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },

            // Шаг 2 — степень a
            new InputStep
            {
                Question =
                    "✏️ Введи показатель степени переменной a в исходном одночлене.\n\n" +
                    "Если a есть без цифры — пиши 1.\n" +
                    "Если a нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "a", out _)
            },

            // Шаг 3 — степень b
            new InputStep
            {
                Question =
                    "✏️ Введи показатель степени переменной b в исходном одночлене.\n\n" +
                    "Если b есть без цифры — пиши 1.\n" +
                    "Если b нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "b", out _)
            },

            // Шаг 4 — внешний показатель n
            new InputStep
            {
                Question =
                    "✏️ В какую степень возводим весь одночлен?\n\n" +
                    "Это число n снаружи скобки: (...)ⁿ\n" +
                    "Именно на него умножаются все показатели внутри.\n\n" +
                    "Если скобок нет и одночлен не возводится — пиши 1.",
                Validate = s =>
                {
                    if (int.TryParse(s, out int n) && n >= 0) return null;
                    return $"«{s}» не подходит.\n" +
                           "Внешний показатель — целое число ≥ 0, например 2 или 3.";
                }
            },
        };

        public int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 5 : 4;

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            bool two = answers[0] == "2";
            if (answers.Count == 1)
                return $"\U0001f50d Переменных: {(two ? "a и b" : "только a")}";
            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k);
            int pa = answers.Count > 2 && int.TryParse(answers[2], out int pa2) ? pa2 : 0;
            int pb = answers.Count > 3 && two && int.TryParse(answers[3], out int pb2) ? pb2 : 0;
            // nIdx: если two — n на позиции 4, иначе на 3
            int nIdx = two ? 4 : 3;
            int n  = answers.Count > nIdx && int.TryParse(answers[nIdx], out int n2) ? n2 : 0;
            var m  = new Monomial(k, pa, pb);
            if (answers.Count == 2) return $"\U0001f50d Одночлен: {Monomial.Fmt(k)}";
            if (answers.Count >= 3 && n == 0) return $"\U0001f50d Одночлен: {m}";
            if (n > 0) return $"\U0001f50d Возводим: ({m}){Monomial.Sup(n)}";
            return $"\U0001f50d Одночлен: {m}";
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";
            double k    = Monomial.ParseDouble(answers[1]);
            int    pa   = Monomial.ParseInt(answers[2]);
            int    nIdx = twoVars ? 4 : 3;
            int    pb   = twoVars ? Monomial.ParseInt(answers[3]) : 0;
            int    n    = Monomial.ParseInt(answers[nIdx]);

            var m      = new Monomial(k, pa, pb);
            var result = m.Pow(n);
            double kn  = Math.Pow(k, n);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({m}){Monomial.Sup(n)} = {result}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Возводим коэффициент в степень {n}:");
            sb.AppendLine($"    {Monomial.Fmt(k)}^{n} = {Monomial.Fmt(kn)}");

            if (pa > 0)
            {
                sb.AppendLine($"  Шаг 2. Показатель a умножаем на {n}:");
                sb.AppendLine($"    {pa} · {n} = {pa * n}  →  a{Monomial.Sup(pa * n)}");
            }

            if (twoVars && pb > 0)
            {
                sb.AppendLine($"  Шаг 3. Показатель b умножаем на {n}:");
                sb.AppendLine($"    {pb} · {n} = {pb * n}  →  b{Monomial.Sup(pb * n)}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем вместе: {result}");
            sb.AppendLine($"Степень результата: {result.Degree}");

            if (k < 0 && n % 2 == 0)
                sb.AppendLine("Коэффициент отрицательный, степень чётная → результат положительный.");
            else if (k < 0 && n % 2 != 0)
                sb.AppendLine("Коэффициент отрицательный, степень нечётная → результат отрицательный.");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ФУНКЦИЯ 3: Умножение двух одночленов
    // ═══════════════════════════════════════════════════════════════
    public class MonomialMultiplyFunction : FunctionBase
    {
        public override string   Name       => "Умножение одночленов";
        public override string   Formula    => "(k₁·aᵖ·bq) · (k₂·aʳ·bˢ) = k₁k₂·aᵖ⁺ʳ·bq⁺ˢ";
        public override string[] Keywords   => new[] { "умножение", "одночлен", "произведение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps => new InputStep[]
        {
            // Шаг 0 — количество переменных
            new InputStep
            {
                Question =
                    "📘 Как умножать одночлены?\n\n" +
                    "Правило: перемножаем коэффициенты, а показатели одинаковых переменных складываем.\n\n" +
                    "Почему складываем показатели — потому что умножение степеней:\n" +
                    "  a² · a³ = (a·a) · (a·a·a) = a⁵ = a^(2+3)\n\n" +
                    "Разберём полный пример: (3a²b) · (2ab³)\n" +
                    "  · Коэффициенты: 3 · 2 = 6\n" +
                    "  · Степени a: 2 + 1 = 3  →  a³\n" +
                    "  · Степени b: 1 + 3 = 4  →  b⁴\n" +
                    "  · Итого: 6a³b⁴\n\n" +
                    "Выбери сколько переменных в твоих одночленах:\n\n" +
                    "Напиши 1 — если только a\n" +
                    "Напиши 2 — если a и b",
                Validate = s => s == "1" || s == "2" ? null
                    : "Напиши цифру 1 или 2."
            },

            // ── Первый одночлен ───────────────────────────────────
            new InputStep
            {
                Question =
                    "Вводим первый одночлен.\n\n" +
                    "✏️ Коэффициент первого одночлена:\n\n" +
                    "Если числа нет — пиши 1.\n" +
                    "Если минус без числа — пиши -1.",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },
            new InputStep
            {
                Question =
                    "✏️ Показатель степени a у первого одночлена:\n\n" +
                    "Если a есть без цифры — пиши 1.\n" +
                    "Если a нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "a", out _)
            },
            new InputStep
            {
                Question =
                    "✏️ Показатель степени b у первого одночлена:\n\n" +
                    "Если b есть без цифры — пиши 1.\n" +
                    "Если b нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "b", out _)
            },

            // ── Второй одночлен ───────────────────────────────────
            new InputStep
            {
                Question =
                    "Отлично! Теперь второй одночлен.\n\n" +
                    "✏️ Коэффициент второго одночлена:\n\n" +
                    "Если числа нет — пиши 1.\n" +
                    "Если минус без числа — пиши -1.",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },
            new InputStep
            {
                Question =
                    "✏️ Показатель степени a у второго одночлена:\n\n" +
                    "Если a есть без цифры — пиши 1.\n" +
                    "Если a нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "a", out _)
            },
            new InputStep
            {
                Question =
                    "✏️ Показатель степени b у второго одночлена:\n\n" +
                    "Если b есть без цифры — пиши 1.\n" +
                    "Если b нет — пиши 0.",
                Validate = s => Monomial.ValidateDegree(s, "b", out _)
            },
        };

        // Одна переменная: шаги 0,1,2, 4,5  (без шагов 3 и 6 — степень b)
        // Две переменные:  шаги 0,1,2,3, 4,5,6
        public int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 7 : 5;

        public int StepIndex(List<string> answers, int logicalStep)
        {
            bool two = answers.Count > 0 && answers[0] == "2";
            if (!two)
                return logicalStep < 3 ? logicalStep : logicalStep + 1;
            return logicalStep;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;
            bool two = answers[0] == "2";
            if (answers.Count == 1)
                return $"\U0001f50d Переменных: {(two ? "a и b" : "только a")}";

            // Парсим первый одночлен (позиции 1,2,[3])
            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k1);
            int pa1 = answers.Count > 2 && int.TryParse(answers[2], out int pa1v) ? pa1v : 0;
            int pb1 = answers.Count > 3 && two && int.TryParse(answers[3], out int pb1v) ? pb1v : 0;
            var m1 = new Monomial(k1, pa1, pb1);

            // Позиция начала второго одночлена
            int s2 = two ? 4 : 3;

            // Парсим второй одночлен если уже начали вводить
            double.TryParse(answers.Count > s2 ? answers[s2].Replace(',', '.') : "",
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k2);
            int pa2 = answers.Count > s2+1 && int.TryParse(answers[s2+1], out int pa2v) ? pa2v : 0;
            int pb2 = answers.Count > s2+2 && two && int.TryParse(answers[s2+2], out int pb2v) ? pb2v : 0;
            var m2 = new Monomial(k2, pa2, pb2);

            bool hasSecond = answers.Count > s2;

            if (!hasSecond)
                return $"\U0001f50d Первый одночлен: {m1}";
            else
            {
                var result = m1 * m2;
                return $"\U0001f50d ({m1}) · ({m2})\n    = {result}";
            }
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";

            int    i   = 1;
            double k1  = Monomial.ParseDouble(answers[i++]);
            int    pa1 = Monomial.ParseInt(answers[i++]);
            int    pb1 = twoVars ? Monomial.ParseInt(answers[i++]) : 0;
            double k2  = Monomial.ParseDouble(answers[i++]);
            int    pa2 = Monomial.ParseInt(answers[i++]);
            int    pb2 = twoVars ? Monomial.ParseInt(answers[i++]) : 0;

            var m1     = new Monomial(k1, pa1, pb1);
            var m2     = new Monomial(k2, pa2, pb2);
            var result = m1 * m2;

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({m1}) · ({m2}) = {result}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Перемножаем коэффициенты:");
            sb.AppendLine($"    {Monomial.Fmt(k1)} · {Monomial.Fmt(k2)} = {Monomial.Fmt(k1 * k2)}");

            sb.AppendLine($"  Шаг 2. Складываем показатели a:");
            sb.AppendLine($"    {pa1} + {pa2} = {pa1 + pa2}  →  a{(pa1 + pa2 == 1 ? "" : Monomial.Sup(pa1 + pa2))}");

            if (twoVars)
            {
                sb.AppendLine($"  Шаг 3. Складываем показатели b:");
                sb.AppendLine($"    {pb1} + {pb2} = {pb1 + pb2}  →  b{(pb1 + pb2 == 1 ? "" : Monomial.Sup(pb1 + pb2))}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем вместе: {result}");
            sb.AppendLine($"Степень результата: {result.Degree}");

            if (k1 < 0 && k2 < 0)
                sb.AppendLine("Оба коэффициента отрицательные → минус на минус = плюс.");
            else if ((k1 < 0) != (k2 < 0))
                sb.AppendLine("Один коэффициент отрицательный → результат отрицательный.");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §14.1–14.2  Деление одночлена на одночлен
    // ═══════════════════════════════════════════════════════════════
    public class MonomialDivideFunction : FunctionBase
    {
        public override string   Name       => "Деление одночленов";
        public override string   Formula    => "k₁aᵖbq ÷ k₂aʳbˢ";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "делить", "деление одночлен", "частное" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Деление одночленов\n\n" +
                    "Правило: делим коэффициенты, из показателей степени делимого\n" +
                    "вычитаем показатели степени делителя.\n\n" +
                    "Пример: 46a²b ÷ (2a)\n" +
                    "  · Коэффициенты: 46 ÷ 2 = 23\n" +
                    "  · Степень a: a² ÷ a¹ = a²⁻¹ = a¹\n" +
                    "  · b: b¹ ÷ b⁰ = b¹\n" +
                    "  · Ответ: 23ab\n\n" +
                    "Используешь одну переменную (a) или две (a и b)?\n" +
                    "✏️ Введи 1 или 2:",
                Validate = s => s == "1" || s == "2" ? null : "Введи 1 или 2"
            },
            new InputStep { Question = "✏️ Коэффициент делимого k₁:", Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делимом:", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Степень b в делимом (или 0):", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:", Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делителе:", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Степень b в делителе (или 0):", Validate = ValidateDeg },
        };

        public override int ActiveStepCount(List<string> answers)
        {
            if (answers.Count == 0) return 7;
            bool two = answers[0] == "2";
            return two ? 7 : 6; // без степени b если одна переменная
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count >= 3)
            {
                string k1 = answers[1], pa1 = answers[2];
                string pb1 = answers.Count > 3 ? answers[3] : "0";
                var m = FormatMono(k1, pa1, pb1, answers[0] == "2");
                return $"🔍 Делимое: {m}";
            }
            if (answers.Count >= 6)
            {
                string k2 = answers[4], pa2 = answers[5];
                string pb2 = answers.Count > 6 ? answers[6] : "0";
                var d = FormatMono(k2, pa2, pb2, answers[0] == "2");
                return $"🔍 Делитель: {d}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool two = answers[0] == "2";
            int i = 1;
            double k1 = Monomial.ParseDouble(answers[i++]);
            int pa1   = Monomial.ParseInt(answers[i++]);
            int pb1   = two ? Monomial.ParseInt(answers[i++]) : 0;
            double k2 = Monomial.ParseDouble(answers[i++]);
            int pa2   = Monomial.ParseInt(answers[i++]);
            int pb2   = two ? Monomial.ParseInt(answers[i++]) : 0;

            if (k2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double kR = k1 / k2;
            int paR = pa1 - pa2;
            int pbR = pb1 - pb2;

            var sb = new StringBuilder();
            string m1 = FormatMono(k1.ToString(), pa1.ToString(), pb1.ToString(), two);
            string m2 = FormatMono(k2.ToString(), pa2.ToString(), pb2.ToString(), two);
            sb.AppendLine($"📌 ({m1}) ÷ ({m2})");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Коэффициенты: {Monomial.Fmt(k1)} ÷ {Monomial.Fmt(k2)} = {Monomial.Fmt(kR)}");
            if (pa1 != 0 || pa2 != 0)
                sb.AppendLine($"  Шаг 2. Степень a: a{Monomial.Sup(pa1)} ÷ a{Monomial.Sup(pa2)} = a{Monomial.Sup(paR)}");
            if (two && (pb1 != 0 || pb2 != 0))
                sb.AppendLine($"  Шаг 3. Степень b: b{Monomial.Sup(pb1)} ÷ b{Monomial.Sup(pb2)} = b{Monomial.Sup(pbR)}");
            sb.AppendLine();
            sb.AppendLine($"✅ Ответ: {FormatMono(Monomial.Fmt(kR), paR.ToString(), pbR.ToString(), two)}");
            return sb.ToString().TrimEnd();
        }

        private static string FormatMono(string k, string pa, string pb, bool two)
        {
            double kd = double.Parse(k.Replace(",","."), System.Globalization.CultureInfo.InvariantCulture);
            int pai = int.Parse(pa), pbi = int.TryParse(pb, out int tmp) ? tmp : 0;
            var m = new Monomial(kd, pai, two ? pbi : 0);
            return m.ToString();
        }

        private static string? ValidateCoeff(string s)
        {
            if (double.TryParse(s.Replace(',','.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число. Введи коэффициент, например: 46 или -7";
        }
        private static string? ValidateDeg(string s)
        {
            if (int.TryParse(s, out int r) && r >= 0) return null;
            return $"«{s}» — введи целое число ≥ 0";
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  §14.5  Найти значение выражения вида (монном ÷ моном) при a,b
    // ═══════════════════════════════════════════════════════════════
    public class MonomialDivideEvalFunction : FunctionBase
    {
        public override string   Name       => "Значение частного при a, b";
        public override string   Formula    => "k₁aᵐbⁿ ÷ k₂aᵖbq при a=…, b=…";
        public override string[] Parameters => Array.Empty<string>();
        public override string[] Keywords   => new[] { "значение деления", "подставить частное" };
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Значение частного одночленов\n\n" +
                    "Сначала делим одночлены (упрощаем), потом подставляем значения.\n\n" +
                    "Пример: 100b⁴ : (4b² − 5b) при b = 0.2\n" +
                    "  Шаг 1. 100b⁴ ÷ 4b² = 25b²\n" +
                    "  Шаг 2. При b = 0.2: 25 · 0.04 = 1\n\n" +
                    "✏️ Коэффициент делимого k₁:",
                Validate = ValidateCoeff
            },
            new InputStep { Question = "✏️ Степень a в делимом (0 если нет):", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Степень b в делимом (0 если нет):", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:", Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делителе (0 если нет):", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Степень b в делителе (0 если нет):", Validate = ValidateDeg },
            new InputStep { Question = "✏️ Введи значение a:", Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Введи значение b:", Validate = ValidateCoeff },
        };

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 3)
            {
                string mono = FormatResult(Monomial.ParseDouble(answers[0]),
                    Monomial.ParseInt(answers[1]), Monomial.ParseInt(answers[2]));
                return $"🔍 Делимое: {mono}";
            }
            if (answers.Count == 6)
            {
                double kR = Monomial.ParseDouble(answers[0]) / Monomial.ParseDouble(answers[3]);
                int paR = Monomial.ParseInt(answers[1]) - Monomial.ParseInt(answers[4]);
                int pbR = Monomial.ParseInt(answers[2]) - Monomial.ParseInt(answers[5]);
                return $"🔍 Упрощённое: {FormatResult(kR, paR, pbR)}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k1 = Monomial.ParseDouble(answers[0]);
            int pa1 = Monomial.ParseInt(answers[1]), pb1 = Monomial.ParseInt(answers[2]);
            double k2 = Monomial.ParseDouble(answers[3]);
            int pa2 = Monomial.ParseInt(answers[4]), pb2 = Monomial.ParseInt(answers[5]);
            double a = Monomial.ParseDouble(answers[6]);
            double b = Monomial.ParseDouble(answers[7]);

            if (k2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double kR = k1 / k2;
            int paR = pa1 - pa2, pbR = pb1 - pb2;
            double result = kR * Math.Pow(a, paR) * Math.Pow(b, pbR);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {FormatResult(k1,pa1,pb1)} ÷ {FormatResult(k2,pa2,pb2)} при a={Monomial.Fmt(a)}, b={Monomial.Fmt(b)}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 1. Делим: {FormatResult(kR, paR, pbR)}");
            sb.AppendLine($"Шаг 2. Подставляем a={Monomial.Fmt(a)}, b={Monomial.Fmt(b)}:");
            sb.AppendLine($"  = {Monomial.Fmt(kR)} · {Monomial.Fmt(a)}{Monomial.Sup(paR)} · {Monomial.Fmt(b)}{Monomial.Sup(pbR)}");
            sb.AppendLine($"\n✅ Ответ: {Monomial.Fmt(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string FormatResult(double k, int pa, int pb)
            => new Monomial(k, pa, pb).ToString();

        private static string? ValidateCoeff(string s)
        {
            if (double.TryParse(s.Replace(',','.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _)) return null;
            return $"«{s}» — не число";
        }
        private static string? ValidateDeg(string s)
        {
            if (int.TryParse(s, out int r) && r >= 0) return null;
            return $"«{s}» — введи целое число ≥ 0";
        }
    }
}

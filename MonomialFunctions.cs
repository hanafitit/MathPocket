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
            double.TryParse(answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k);
            int.TryParse(answers[2], out int pa);
            int pb = 0;
            if (twoVars && answers.Count > 3) int.TryParse(answers[3], out pb);

            var m  = new Monomial(k, pa, pb);
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
            double.TryParse(answers[1].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k);
            int.TryParse(answers[2], out int pa);
            int pb = 0;
            int nIdx = 3;
            if (twoVars) { int.TryParse(answers[3], out pb); nIdx = 4; }
            int.TryParse(answers[nIdx], out int n);

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

            int i = 1;
            double.TryParse(answers[i++].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k1);
            int.TryParse(answers[i++], out int pa1);
            int pb1 = 0;
            if (twoVars) int.TryParse(answers[i++], out pb1);

            double.TryParse(answers[i++].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double k2);
            int.TryParse(answers[i++], out int pa2);
            int pb2 = 0;
            if (twoVars) int.TryParse(answers[i++], out pb2);

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

            double kRes = k1 * k2;
            if (k1 < 0 && k2 < 0)
                sb.AppendLine("Оба коэффициента отрицательные → минус на минус = плюс.");
            else if ((k1 < 0) != (k2 < 0))
                sb.AppendLine("Один коэффициент отрицательный → результат отрицательный.");

            return sb.ToString().TrimEnd();
        }
    }
}

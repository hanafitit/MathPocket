using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathPocket
{
    //  Одночлен вида  k · aᵖ · bq

    internal readonly struct Monomial
    {
        public readonly double K;
        public readonly int    Pa;
        public readonly int    Pb;

        public Monomial(double k, int pa, int pb) => (K, Pa, Pb) = (k, pa, pb);

        public int Degree => Pa + Pb;

        public Monomial Pow(int n) => new(Math.Pow(K, n), Pa * n, Pb * n);

        public static Monomial operator *(Monomial a, Monomial b) =>
            new(a.K * b.K, a.Pa + b.Pa, a.Pb + b.Pb);

        public override string ToString()
        {
            if (K == 0) return "0";

            var sb    = new StringBuilder();
            bool hasA = Pa > 0, hasB = Pb > 0, hasVar = hasA || hasB;

            if      (!hasVar) sb.Append(Fmt(K));
            else if (K ==  1) { /* пишем только переменные */ }
            else if (K == -1) sb.Append('-');
            else              sb.Append(Fmt(K));

            if (hasA) { sb.Append('a'); if (Pa > 1) sb.Append(Sup(Pa)); }
            if (hasB) { sb.Append('b'); if (Pb > 1) sb.Append(Sup(Pb)); }

            return sb.ToString();
        }

        // ─── Валидаторы ───────────────────────────────────────────

        /// <summary>Проверяет строку как коэффициент. Возвращает null если OK, иначе сообщение.</summary>
        public static string? ValidateCoeff(string s, out double result)
        {
            if (double.TryParse(s.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return null;

            return $"«{s}» не похоже на коэффициент.\n" +
                   "Коэффициент — это одно число перед буквами: целое, дробное или отрицательное.\n" +
                   "Например: 3 или -5 или 0.5 или 1 (если числа нет — пиши 1)";
        }

        /// <summary>Проверяет строку как показатель степени ≥ 0. Возвращает null если OK.</summary>
        public static string? ValidateDegree(string s, string varName, out int result)
        {
            if (int.TryParse(s, out result) && result >= 0)
                return null;

            return $"«{s}» не подходит для степени {varName}.\n" +
                   $"Показатель степени — целое число начиная с 0.\n" +
                   $"0 означает что переменной {varName} нет, 1 — просто {varName}, 2 — {varName}², и так далее.";
        }

        // ─── Форматирование ───────────────────────────────────────

        /// <summary>Красиво форматирует double: без дробной части если целое.</summary>
        public static string Fmt(double v) =>
            v == Math.Floor(v) && !double.IsInfinity(v)
                ? ((long)v).ToString()
                : v.ToString("G6", CultureInfo.InvariantCulture);

        /// <summary>Переводит цифры числа в надстрочные символы Юникод.</summary>
        public static string Sup(int n)
        {
            const string Superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            var sb = new StringBuilder();
            foreach (char c in n.ToString())
                sb.Append(c is >= '0' and <= '9' ? Superscripts[c - '0'] : c);
            return sb.ToString();
        }

        // ─── Парсинг ──────────────────────────────────────────────

        public static double ParseDouble(string s) =>
            double.Parse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);

        public static int ParseInt(string s) => int.Parse(s);
    }

    //  Общие шаги для функций одночлена (DRY)

    internal static class MonomialSteps
    {
        public static readonly InputStep VarCount = new()
        {
            Question  = string.Empty, // переопределяется в каждой функции
            Validate  = s => s is "1" or "2" ? null
                : "Напиши цифру 1 или 2.\n1 — если в одночлене одна буква (только a)\n2 — если две буквы (a и b)"
        };

        public static readonly InputStep Coeff = new()
        {
            Question = "✏️ Введи коэффициент — число перед буквами.\n\n" +
                       "Если числа нет — пиши 1.\nЕсли перед буквой стоит минус — пиши -1.",
            Validate  = s => Monomial.ValidateCoeff(s, out _)
        };

        public static readonly InputStep DegreeA = new()
        {
            Question = "✏️ Введи показатель степени переменной a.\n\n" +
                       "Если a есть без цифры — пиши 1.\nЕсли a нет — пиши 0.",
            Validate = s => Monomial.ValidateDegree(s, "a", out _)
        };

        public static readonly InputStep DegreeB = new()
        {
            Question = "✏️ Введи показатель степени переменной b.\n\n" +
                       "Если b есть без цифры — пиши 1.\nЕсли b нет — пиши 0.",
            Validate = s => Monomial.ValidateDegree(s, "b", out _)
        };
    }

    //  ФУНКЦИЯ 1: Стандартный вид одночлена

    public class MonomialStandardFormFunction : FunctionBase
    {
        public override string   Name       => "Привести одночлен к стандартному виду";
        public override string   Formula    => "k·aᵖ·bq,  степень = p + q";
        public override string[] Keywords   => ["стандартный", "вид", "одночлен"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
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
                Validate = s => s is "1" or "2" ? null
                    : "Напиши цифру 1 или 2.\n" +
                      "1 — если в одночлене одна буква (только a)\n" +
                      "2 — если две буквы (a и b)"
            },
            MonomialSteps.Coeff,
            MonomialSteps.DegreeA,
            MonomialSteps.DegreeB,
        ];

        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 4 : 3;

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";
            double k     = Monomial.ParseDouble(answers[1]);
            int    pa    = Monomial.ParseInt(answers[2]);
            int    pb    = twoVars && answers.Count > 3 ? Monomial.ParseInt(answers[3]) : 0;
            var    m     = new Monomial(k, pa, pb);
            var    sb    = new StringBuilder();

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

            sb.AppendLine(pa > 0
                ? $"  Шаг 2. Переменная a в степени {pa} → a{Monomial.Sup(pa)}"
                : "  Шаг 2. Переменной a нет (показатель 0)");

            if (twoVars)
                sb.AppendLine(pb > 0
                    ? $"  Шаг 3. Переменная b в степени {pb} → b{Monomial.Sup(pb)}"
                    : "  Шаг 3. Переменной b нет (показатель 0)");

            sb.AppendLine();

            if (twoVars && pa > 0 && pb > 0)
            {
                sb.AppendLine("Степень одночлена = сумма показателей:");
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

    //  ФУНКЦИЯ 2: Степень одночлена

    public class MonomialPowerFunction : FunctionBase
    {
        public override string   Name       => "Найти степень одночлена";
        public override string   Formula    => "(k·aᵖ·bq)ⁿ = kⁿ·aᵖⁿ·bqⁿ";
        public override string[] Keywords   => ["степень", "одночлен", "возведение"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Как возводить одночлен в степень?\n\n" +
                    "Правило: каждый множитель одночлена возводится в эту степень отдельно.\n\n" +
                    "Пример: (2a³)²\n" +
                    "  · (2a³)² = 2² · (a³)² = 4 · a⁶ = 4a⁶\n\n" +
                    "Формула: (k·aᵖ·bq)ⁿ = kⁿ·aᵖⁿ·bqⁿ\n\n" +
                    "Выбери сколько переменных:\n\n" +
                    "Напиши 1 — если только a\nНапиши 2 — если a и b",
                Validate = s => s is "1" or "2" ? null : "Напиши цифру 1 или 2."
            },
            MonomialSteps.Coeff,
            MonomialSteps.DegreeA,
            MonomialSteps.DegreeB,
            new InputStep
            {
                Question =
                    "✏️ В какую степень возводим весь одночлен?\n\n" +
                    "Это число n снаружи скобки: (...)ⁿ\n" +
                    "Если не возводится — пиши 1.",
                Validate = s => int.TryParse(s, out int n) && n >= 0 ? null
                    : $"«{s}» не подходит. Внешний показатель — целое число ≥ 0."
            },
        ];

        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 5 : 4;

        /// <summary>
        /// При !twoVars логический шаг 3 (degB) пропускается.
        /// Steps = [выбор, коэфф, degA, degB, степень] — при откате с шага 4 (степень)
        /// нужно вернуться на шаг 2 (degA), минуя скрытый шаг 3.
        /// </summary>
        public override void RollbackStep(StepInputSession session)
        {
            session.CurrentStep--;
            if (session.Answers.Count > 0)
                session.Answers.RemoveAt(session.Answers.Count - 1);

            bool twoVars = session.Answers.Count > 0 && session.Answers[0] == "2";
            if (!twoVars && session.CurrentStep == 3)
            {
                session.CurrentStep--;
                // Ответ для шага 3 не записывался — Answers не трогаем.
            }
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;

            bool two = answers[0] == "2";
            if (answers.Count == 1)
                return $"🔍 Переменных: {(two ? "a и b" : "только a")}";

            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double k);
            int pa  = answers.Count > 2 && int.TryParse(answers[2], out int pa2) ? pa2 : 0;
            int pb  = answers.Count > 3 && two && int.TryParse(answers[3], out int pb2) ? pb2 : 0;
            int nIdx = two ? 4 : 3;
            int n   = answers.Count > nIdx && int.TryParse(answers[nIdx], out int n2) ? n2 : 0;

            var m = new Monomial(k, pa, pb);

            if (answers.Count == 2)  return $"🔍 Одночлен: {Monomial.Fmt(k)}";
            if (n == 0)              return $"🔍 Одночлен: {m}";
            return $"🔍 Возводим: ({m}){Monomial.Sup(n)}";
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";
            double k     = Monomial.ParseDouble(answers[1]);
            int    pa    = Monomial.ParseInt(answers[2]);
            int    nIdx  = twoVars ? 4 : 3;
            int    pb    = twoVars ? Monomial.ParseInt(answers[3]) : 0;
            int    n     = Monomial.ParseInt(answers[nIdx]);

            var    m      = new Monomial(k, pa, pb);
            var    result = m.Pow(n);
            double kn     = Math.Pow(k, n);

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

            if      (k < 0 && n % 2 == 0) sb.AppendLine("Коэффициент отрицательный, степень чётная → результат положительный.");
            else if (k < 0 && n % 2 != 0) sb.AppendLine("Коэффициент отрицательный, степень нечётная → результат отрицательный.");

            return sb.ToString().TrimEnd();
        }
    }

    //  ФУНКЦИЯ 3: Умножение двух одночленов

    public class MonomialMultiplyFunction : FunctionBase
    {
        public override string   Name       => "Перемножить одночлены";
        public override string   Formula    => "(k₁·aᵖ·bq) · (k₂·aʳ·bˢ) = k₁k₂·aᵖ⁺ʳ·bq⁺ˢ";
        public override string[] Keywords   => ["умножение", "одночлен", "произведение"];
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Как умножать одночлены?\n\n" +
                    "Правило: перемножаем коэффициенты, показатели одинаковых переменных складываем.\n\n" +
                    "Пример: (3a²b) · (2ab³)\n" +
                    "  · Коэффициенты: 3 · 2 = 6\n" +
                    "  · Степени a: 2 + 1 = 3  →  a³\n" +
                    "  · Степени b: 1 + 3 = 4  →  b⁴\n" +
                    "  · Итого: 6a³b⁴\n\n" +
                    "Напиши 1 — если только a\nНапиши 2 — если a и b",
                Validate = s => s is "1" or "2" ? null : "Напиши цифру 1 или 2."
            },
            // ── Первый одночлен ───────────────────────────────────
            new InputStep
            {
                Question = "Вводим первый одночлен.\n\n" +
                           "✏️ Коэффициент первого одночлена:\n\nЕсли числа нет — пиши 1.\nЕсли минус без числа — пиши -1.",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },
            MonomialSteps.DegreeA with { Question = "✏️ Показатель степени a у первого одночлена:\n\nЕсли a есть без цифры — пиши 1.\nЕсли a нет — пиши 0." },
            MonomialSteps.DegreeB with { Question = "✏️ Показатель степени b у первого одночлена:\n\nЕсли b есть без цифры — пиши 1.\nЕсли b нет — пиши 0." },
            // ── Второй одночлен ───────────────────────────────────
            new InputStep
            {
                Question = "Отлично! Теперь второй одночлен.\n\n" +
                           "✏️ Коэффициент второго одночлена:\n\nЕсли числа нет — пиши 1.\nЕсли минус без числа — пиши -1.",
                Validate = s => Monomial.ValidateCoeff(s, out _)
            },
            MonomialSteps.DegreeA with { Question = "✏️ Показатель степени a у второго одночлена:\n\nЕсли a есть без цифры — пиши 1.\nЕсли a нет — пиши 0." },
            MonomialSteps.DegreeB with { Question = "✏️ Показатель степени b у второго одночлена:\n\nЕсли b есть без цифры — пиши 1.\nЕсли b нет — пиши 0." },
        ];

        // Одна переменная: 5 шагов (0,1,2, 4,5) — без шагов 3 и 6 (степень b)
        // Две переменные:  7 шагов (0..6)
        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 7 : 5;

        /// <summary>
        /// При !twoVars логический шаг 3 (degB первого одночлена) пропускается.
        /// Если откатываемся с шага 4 — нужно вернуться на 2, а не на 3 (который никогда не показывался).
        /// </summary>
        public override void RollbackStep(StepInputSession session)
        {
            session.CurrentStep--;
            if (session.Answers.Count > 0)
                session.Answers.RemoveAt(session.Answers.Count - 1);

            // После декремента проверяем: если попали на "скрытый" шаг 3 (!twoVars),
            // откатываемся ещё на один — пользователь его не видел и не отвечал на него.
            bool twoVars = session.Answers.Count > 0 && session.Answers[0] == "2";
            if (!twoVars && session.CurrentStep == 3)
            {
                session.CurrentStep--;
                // Ответ для шага 3 не записывался — Answers не трогаем.
            }
        }

        /// <summary>Сопоставляет логический шаг с реальным индексом в Steps[].</summary>
        public static int StepIndex(List<string> answers, int logicalStep)
        {
            bool two = answers.Count > 0 && answers[0] == "2";
            // При одной переменной логический шаг ≥ 3 смещается на 1 (пропускаем Steps[3] — степень b)
            return !two && logicalStep >= 3 ? logicalStep + 1 : logicalStep;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;

            bool two = answers[0] == "2";
            if (answers.Count == 1)
                return $"🔍 Переменных: {(two ? "a и b" : "только a")}";

            // Парсим первый одночлен
            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double k1);
            int pa1 = answers.Count > 2 && int.TryParse(answers[2], out int pa1v) ? pa1v : 0;
            int pb1 = answers.Count > 3 && two && int.TryParse(answers[3], out int pb1v) ? pb1v : 0;
            var m1  = new Monomial(k1, pa1, pb1);

            // Позиция начала второго одночлена
            int s2  = two ? 4 : 3;
            double.TryParse(answers.Count > s2 ? answers[s2].Replace(',', '.') : "",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double k2);
            int pa2 = answers.Count > s2 + 1 && int.TryParse(answers[s2 + 1], out int pa2v) ? pa2v : 0;
            int pb2 = answers.Count > s2 + 2 && two && int.TryParse(answers[s2 + 2], out int pb2v) ? pb2v : 0;
            var m2  = new Monomial(k2, pa2, pb2);

            if (answers.Count <= s2) return $"🔍 Первый одночлен: {m1}";

            var result = m1 * m2;
            return $"🔍 ({m1}) · ({m2})\n    = {result}";
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVars = answers[0] == "2";
            int  i       = 1;

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
            sb.AppendLine("  Шаг 1. Перемножаем коэффициенты:");
            sb.AppendLine($"    {Monomial.Fmt(k1)} · {Monomial.Fmt(k2)} = {Monomial.Fmt(k1 * k2)}");
            sb.AppendLine("  Шаг 2. Складываем показатели a:");
            sb.AppendLine($"    {pa1} + {pa2} = {pa1 + pa2}  →  a{(pa1 + pa2 == 1 ? "" : Monomial.Sup(pa1 + pa2))}");

            if (twoVars)
            {
                sb.AppendLine("  Шаг 3. Складываем показатели b:");
                sb.AppendLine($"    {pb1} + {pb2} = {pb1 + pb2}  →  b{(pb1 + pb2 == 1 ? "" : Monomial.Sup(pb1 + pb2))}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем вместе: {result}");
            sb.AppendLine($"Степень результата: {result.Degree}");

            if      (k1 < 0 && k2 < 0)        sb.AppendLine("Оба коэффициента отрицательные → минус на минус = плюс.");
            else if ((k1 < 0) != (k2 < 0))    sb.AppendLine("Один коэффициент отрицательный → результат отрицательный.");

            return sb.ToString().TrimEnd();
        }
    }

    //  ФУНКЦИЯ 4: Деление одночлена на одночлен

    public class MonomialDivideFunction : FunctionBase
    {
        public override string   Name       => "Разделить одночлены";
        public override string   Formula    => "k₁aᵖbq ÷ k₂aʳbˢ";
        public override string[] Parameters => [];
        public override string[] Keywords   => ["делить", "деление одночлен", "частное"];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Деление одночленов\n\n" +
                    "Правило: делим коэффициенты, из показателей делимого вычитаем показатели делителя.\n\n" +
                    "Пример: 46a²b ÷ 2a\n" +
                    "  · 46 ÷ 2 = 23\n" +
                    "  · a²⁻¹ = a¹\n" +
                    "  · b¹⁻⁰ = b¹\n" +
                    "  · Ответ: 23ab\n\n" +
                    "✏️ Введи 1 (только a) или 2 (a и b):",
                Validate = s => s is "1" or "2" ? null : "Введи 1 или 2"
            },
            new InputStep { Question = "✏️ Коэффициент делимого k₁:",         Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делимом:",              Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Степень b в делимом (или 0):",      Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:",          Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делителе:",             Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Степень b в делителе (или 0):",     Validate = ValidateDeg   },
        ];

        public override int ActiveStepCount(List<string> answers)
        {
            if (answers.Count == 0) return 7;
            return answers[0] == "2" ? 7 : 6;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count >= 3)
            {
                var m = FormatMono(answers[1], answers[2],
                    answers.Count > 3 ? answers[3] : "0", answers[0] == "2");
                return $"🔍 Делимое: {m}";
            }
            if (answers.Count >= 6)
            {
                var d = FormatMono(answers[4], answers[5],
                    answers.Count > 6 ? answers[6] : "0", answers[0] == "2");
                return $"🔍 Делитель: {d}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool two  = answers[0] == "2";
            int  i    = 1;
            double k1 = Monomial.ParseDouble(answers[i++]);
            int  pa1  = Monomial.ParseInt(answers[i++]);
            int  pb1  = two ? Monomial.ParseInt(answers[i++]) : 0;
            double k2 = Monomial.ParseDouble(answers[i++]);
            int  pa2  = Monomial.ParseInt(answers[i++]);
            int  pb2  = two ? Monomial.ParseInt(answers[i++]) : 0;

            if (k2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double kR = k1 / k2;
            int   paR = pa1 - pa2;
            int   pbR = pb1 - pb2;

            var sb = new StringBuilder();
            string m1s = FormatMono(Monomial.Fmt(k1), pa1.ToString(), pb1.ToString(), two);
            string m2s = FormatMono(Monomial.Fmt(k2), pa2.ToString(), pb2.ToString(), two);
            sb.AppendLine($"📌 ({m1s}) ÷ ({m2s})");
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
            double kd = double.Parse(k.Replace(",", "."), CultureInfo.InvariantCulture);
            int pai   = int.Parse(pa);
            int pbi   = int.TryParse(pb, out int tmp) ? tmp : 0;
            return new Monomial(kd, pai, two ? pbi : 0).ToString();
        }

        private static string? ValidateCoeff(string s) =>
            double.TryParse(s.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out _)
                ? null
                : $"«{s}» — не число. Введи коэффициент, например: 46 или -7";

        private static string? ValidateDeg(string s) =>
            int.TryParse(s, out int r) && r >= 0
                ? null
                : $"«{s}» — введи целое число ≥ 0";
    }

    //  ФУНКЦИЯ 5: Значение частного при a, b

    public class MonomialDivideEvalFunction : FunctionBase
    {
        public override string   Name       => "Вычислить частное при заданных a и b";
        public override string   Formula    => "k₁aᵐbⁿ ÷ k₂aᵖbq при a=…, b=…";
        public override string[] Parameters => [];
        public override string[] Keywords   => ["значение деления", "подставить частное"];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Значение частного одночленов\n\n" +
                    "Сначала делим одночлены (упрощаем), потом подставляем значения.\n\n" +
                    "Пример: 100b⁴ ÷ 4b² при b = 0.2\n" +
                    "  Шаг 1. 100b⁴ ÷ 4b² = 25b²\n" +
                    "  Шаг 2. При b = 0.2: 25 · 0.04 = 1\n\n" +
                    "✏️ Коэффициент делимого k₁:",
                Validate = ValidateCoeff
            },
            new InputStep { Question = "✏️ Степень a в делимом (0 если нет):",  Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Степень b в делимом (0 если нет):",  Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:",           Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Степень a в делителе (0 если нет):", Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Степень b в делителе (0 если нет):", Validate = ValidateDeg   },
            new InputStep { Question = "✏️ Введи значение a:",                  Validate = ValidateCoeff },
            new InputStep { Question = "✏️ Введи значение b:",                  Validate = ValidateCoeff },
        ];

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 3)
            {
                string mono = FormatResult(
                    Monomial.ParseDouble(answers[0]),
                    Monomial.ParseInt(answers[1]),
                    Monomial.ParseInt(answers[2]));
                return $"🔍 Делимое: {mono}";
            }
            if (answers.Count == 6)
            {
                double kR = Monomial.ParseDouble(answers[0]) / Monomial.ParseDouble(answers[3]);
                int   paR = Monomial.ParseInt(answers[1]) - Monomial.ParseInt(answers[4]);
                int   pbR = Monomial.ParseInt(answers[2]) - Monomial.ParseInt(answers[5]);
                return $"🔍 Упрощённое: {FormatResult(kR, paR, pbR)}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double k1  = Monomial.ParseDouble(answers[0]);
            int    pa1 = Monomial.ParseInt(answers[1]), pb1 = Monomial.ParseInt(answers[2]);
            double k2  = Monomial.ParseDouble(answers[3]);
            int    pa2 = Monomial.ParseInt(answers[4]), pb2 = Monomial.ParseInt(answers[5]);
            double a   = Monomial.ParseDouble(answers[6]);
            double b   = Monomial.ParseDouble(answers[7]);

            if (k2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double kR     = k1 / k2;
            int    paR    = pa1 - pa2, pbR = pb1 - pb2;
            double result = kR * Math.Pow(a, paR) * Math.Pow(b, pbR);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {FormatResult(k1, pa1, pb1)} ÷ {FormatResult(k2, pa2, pb2)} при a={Monomial.Fmt(a)}, b={Monomial.Fmt(b)}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 1. Делим: {FormatResult(kR, paR, pbR)}");
            sb.AppendLine($"Шаг 2. Подставляем a={Monomial.Fmt(a)}, b={Monomial.Fmt(b)}:");
            sb.AppendLine($"  = {Monomial.Fmt(kR)} · {Monomial.Fmt(a)}{Monomial.Sup(paR)} · {Monomial.Fmt(b)}{Monomial.Sup(pbR)}");
            sb.Append($"\n✅ Ответ: {Monomial.Fmt(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string FormatResult(double k, int pa, int pb) =>
            new Monomial(k, pa, pb).ToString();

        private static string? ValidateCoeff(string s) =>
            double.TryParse(s.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out _)
                ? null
                : $"«{s}» — не число";

        private static string? ValidateDeg(string s) =>
            int.TryParse(s, out int r) && r >= 0
                ? null
                : $"«{s}» — введи целое число ≥ 0";
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathPocket
{
    //  Одночлен вида  k · aᵖ · bq

    internal readonly record struct Monomial(double Coefficient, int DegreeA, int DegreeB)
    {
        public int TotalDegree => DegreeA + DegreeB;

        public Monomial Pow(int exponent) => new(Math.Pow(Coefficient, exponent), DegreeA * exponent, DegreeB * exponent);

        public static Monomial operator *(Monomial left, Monomial right) =>
            new(left.Coefficient * right.Coefficient, left.DegreeA + right.DegreeA, left.DegreeB + right.DegreeB);

        public override string ToString()
        {
            if (Coefficient == 0) return "0";

            var sb = new StringBuilder();
            bool hasA = DegreeA > 0, hasB = DegreeB > 0;
            bool hasVariables = hasA || hasB;

            if (!hasVariables)
            {
                sb.Append(Format(Coefficient));
            }
            else if (Coefficient == 1)
            {
                // Coefficient 1 is omitted when variables are present
            }
            else if (Coefficient == -1)
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(Format(Coefficient));
            }

            if (hasA)
            {
                sb.Append('a');
                if (DegreeA > 1) sb.Append(ToSuperscript(DegreeA));
            }

            if (hasB)
            {
                sb.Append('b');
                if (DegreeB > 1) sb.Append(ToSuperscript(DegreeB));
            }

            return sb.ToString();
        }

        // ─── Валидаторы ───────────────────────────────────────────

        /// <summary>Проверяет строку как коэффициент. Возвращает null если OK, иначе сообщение.</summary>
        public static string? ValidateCoefficient(string input, out double result)
        {
            if (double.TryParse(input.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return null;

            return $"«{input}» не похоже на коэффициент.\n" +
                   "Коэффициент — это одно число перед буквами: целое, дробное или отрицательное.\n" +
                   "Например: 3 или -5 или 0.5 или 1 (если числа нет — пиши 1)";
        }

        /// <summary>Проверяет строку как показатель степени ≥ 0. Возвращает null если OK.</summary>
        public static string? ValidateDegree(string input, string variableName, out int result)
        {
            if (int.TryParse(input, out result) && result >= 0)
                return null;

            return $"«{input}» не подходит для степени {variableName}.\n" +
                   $"Показатель степени — целое число начиная с 0.\n" +
                   $"0 означает что переменной {variableName} нет, 1 — просто {variableName}, 2 — {variableName}², и так далее.";
        }

        // ─── Форматирование ───────────────────────────────────────

        /// <summary>Красиво форматирует double: без дробной части если целое.</summary>
        public static string Format(double value) =>
            value == Math.Floor(value) && !double.IsInfinity(value)
                ? ((long)value).ToString()
                : value.ToString("G6", CultureInfo.InvariantCulture);

        /// <summary>Переводит цифры числа в надстрочные символы Юникод.</summary>
        public static string ToSuperscript(int number)
        {
            const string Superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            var sb = new StringBuilder();
            foreach (char c in number.ToString())
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
        public static readonly InputStep VariableCount = new()
        {
            Question = string.Empty, // Overridden in each function
            Validate = input => input is "1" or "2" ? null
                : "Напиши цифру 1 или 2.\n1 — если в одночлене одна буква (только a)\n2 — если две буквы (a и b)"
        };

        public static readonly InputStep Coefficient = new()
        {
            Question = "✏️ Введи коэффициент — число перед буквами.\n\n" +
                       "Если числа нет — пиши 1.\nЕсли перед буквой стоит минус — пиши -1.",
            Validate = input => Monomial.ValidateCoefficient(input, out _)
        };

        public static readonly InputStep DegreeA = new()
        {
            Question = "✏️ Введи показатель степени переменной a.\n\n" +
                       "Если a есть без цифры — пиши 1.\nЕсли a нет — пиши 0.",
            Validate = input => Monomial.ValidateDegree(input, "a", out _)
        };

        public static readonly InputStep DegreeB = new()
        {
            Question = "✏️ Введи показатель степени переменной b.\n\n" +
                       "Если b есть без цифры — пиши 1.\nЕсли b нет — пиши 0.",
            Validate = input => Monomial.ValidateDegree(input, "b", out _)
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
                Validate = input => input is "1" or "2" ? null
                    : "Напиши цифру 1 или 2.\n" +
                      "1 — если в одночлене одна буква (только a)\n" +
                      "2 — если две буквы (a и b)"
            },
            MonomialSteps.Coefficient,
            MonomialSteps.DegreeA,
            MonomialSteps.DegreeB,
        ];

        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 4 : 3;

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVariables = answers[0] == "2";
            double coefficient = Monomial.ParseDouble(answers[1]);
            int degreeA = Monomial.ParseInt(answers[2]);
            int degreeB = twoVariables && answers.Count > 3 ? Monomial.ParseInt(answers[3]) : 0;
            var monomial = new Monomial(coefficient, degreeA, degreeB);
            var sb = new StringBuilder();

            sb.AppendLine($"✅ Стандартный вид: {monomial}");
            sb.AppendLine();

            if (monomial.TotalDegree == 0)
            {
                sb.AppendLine("Это числовой одночлен — переменных нет.");
                sb.AppendLine("Степень числового одночлена = 0.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Коэффициент: {Monomial.Format(coefficient)}");

            sb.AppendLine(degreeA > 0
                ? $"  Шаг 2. Переменная a в степени {degreeA} → a{Monomial.ToSuperscript(degreeA)}"
                : "  Шаг 2. Переменной a нет (показатель 0)");

            if (twoVariables)
                sb.AppendLine(degreeB > 0
                    ? $"  Шаг 3. Переменная b в степени {degreeB} → b{Monomial.ToSuperscript(degreeB)}"
                    : "  Шаг 3. Переменной b нет (показатель 0)");

            sb.AppendLine();

            if (twoVariables && degreeA > 0 && degreeB > 0)
            {
                sb.AppendLine("Степень одночлена = сумма показателей:");
                sb.AppendLine($"  {degreeA} + {degreeB} = {monomial.TotalDegree}");
            }
            else
            {
                sb.AppendLine($"Степень одночлена = {monomial.TotalDegree}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 {monomial}  — одночлен {monomial.TotalDegree}-й степени");
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
                Validate = input => input is "1" or "2" ? null : "Напиши цифру 1 или 2."
            },
            MonomialSteps.Coefficient,
            MonomialSteps.DegreeA,
            MonomialSteps.DegreeB,
            new InputStep
            {
                Question =
                    "✏️ В какую степень возводим весь одночлен?\n\n" +
                    "Это число n снаружи скобки: (...)ⁿ\n" +
                    "Если не возводится — пиши 1.",
                Validate = input => int.TryParse(input, out int n) && n >= 0 ? null
                    : $"«{input}» не подходит. Внешний показатель — целое число ≥ 0."
            },
        ];

        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 5 : 4;

        /// <summary>
        /// При !twoVariables логический шаг 3 (degB) пропускается.
        /// Steps = [выбор, коэфф, degA, degB, степень] — при откате с шага 4 (степень)
        /// нужно вернуться на шаг 2 (degA), минуя скрытый шаг 3.
        /// </summary>
        public override void RollbackStep(StepInputSession session)
        {
            session.CurrentStep--;
            if (session.Answers.Count > 0)
                session.Answers.RemoveAt(session.Answers.Count - 1);

            bool twoVariables = session.Answers.Count > 0 && session.Answers[0] == "2";
            if (!twoVariables && session.CurrentStep == 3)
            {
                session.CurrentStep--;
                // Ответ для шага 3 не записывался — Answers не трогаем.
            }
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;

            bool twoVariables = answers[0] == "2";
            if (answers.Count == 1)
                return $"🔍 Переменных: {(twoVariables ? "a и b" : "только a")}";

            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double coefficient);
            int degreeA = answers.Count > 2 && int.TryParse(answers[2], out int pa2) ? pa2 : 0;
            int degreeB = answers.Count > 3 && twoVariables && int.TryParse(answers[3], out int pb2) ? pb2 : 0;
            int exponentIdx = twoVariables ? 4 : 3;
            int exponent = answers.Count > exponentIdx && int.TryParse(answers[exponentIdx], out int n2) ? n2 : 0;

            var monomial = new Monomial(coefficient, degreeA, degreeB);

            if (answers.Count == 2)  return $"🔍 Одночлен: {Monomial.Format(coefficient)}";
            if (exponent == 0)       return $"🔍 Одночлен: {monomial}";
            return $"🔍 Возводим: ({monomial}){Monomial.ToSuperscript(exponent)}";
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVariables = answers[0] == "2";
            double coefficient = Monomial.ParseDouble(answers[1]);
            int degreeA = Monomial.ParseInt(answers[2]);
            int exponentIdx = twoVariables ? 4 : 3;
            int degreeB = twoVariables ? Monomial.ParseInt(answers[3]) : 0;
            int exponent = Monomial.ParseInt(answers[exponentIdx]);

            var monomial = new Monomial(coefficient, degreeA, degreeB);
            var result = monomial.Pow(exponent);
            double poweredCoefficient = Math.Pow(coefficient, exponent);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({monomial}){Monomial.ToSuperscript(exponent)} = {result}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Возводим коэффициент в степень {exponent}:");
            sb.AppendLine($"    {Monomial.Format(coefficient)}^{exponent} = {Monomial.Format(poweredCoefficient)}");

            if (degreeA > 0)
            {
                sb.AppendLine($"  Шаг 2. Показатель a умножаем на {exponent}:");
                sb.AppendLine($"    {degreeA} · {exponent} = {degreeA * exponent}  →  a{Monomial.ToSuperscript(degreeA * exponent)}");
            }

            if (twoVariables && degreeB > 0)
            {
                sb.AppendLine($"  Шаг 3. Показатель b умножаем на {exponent}:");
                sb.AppendLine($"    {degreeB} · {exponent} = {degreeB * exponent}  →  b{Monomial.ToSuperscript(degreeB * exponent)}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем вместе: {result}");
            sb.AppendLine($"Степень результата: {result.TotalDegree}");

            if      (coefficient < 0 && exponent % 2 == 0) sb.AppendLine("Коэффициент отрицательный, степень чётная → результат положительный.");
            else if (coefficient < 0 && exponent % 2 != 0) sb.AppendLine("Коэффициент отрицательный, степень нечётная → результат отрицательный.");

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
                Validate = input => input is "1" or "2" ? null : "Напиши цифру 1 или 2."
            },
            // ── Первый одночлен ───────────────────────────────────
            new InputStep
            {
                Question = "Вводим первый одночлен.\n\n" +
                           "✏️ Коэффициент первого одночлена:\n\nЕсли числа нет — пиши 1.\nЕсли минус без числа — пиши -1.",
                Validate = input => Monomial.ValidateCoefficient(input, out _)
            },
            MonomialSteps.DegreeA with { Question = "✏️ Показатель степени a у первого одночлена:\n\nЕсли a есть без цифры — пиши 1.\nЕсли a нет — пиши 0." },
            MonomialSteps.DegreeB with { Question = "✏️ Показатель степени b у первого одночлена:\n\nЕсли b есть без цифры — пиши 1.\nЕсли b нет — пиши 0." },
            // ── Второй одночлен ───────────────────────────────────
            new InputStep
            {
                Question = "Отлично! Теперь второй одночлен.\n\n" +
                           "✏️ Коэффициент второго одночлена:\n\nЕсли числа нет — пиши 1.\nЕсли минус без числа — пиши -1.",
                Validate = input => Monomial.ValidateCoefficient(input, out _)
            },
            MonomialSteps.DegreeA with { Question = "✏️ Показатель степени a у второго одночлена:\n\nЕсли a есть без цифры — пиши 1.\nЕсли a нет — пиши 0." },
            MonomialSteps.DegreeB with { Question = "✏️ Показатель степени b у второго одночлена:\n\nЕсли b есть без цифры — пиши 1.\nЕсли b нет — пиши 0." },
        ];

        // Одна переменная: 5 шагов (0,1,2, 4,5) — без шагов 3 и 6 (степень b)
        // Две переменные:  7 шагов (0..6)
        public override int ActiveStepCount(List<string> answers) =>
            answers.Count > 0 && answers[0] == "2" ? 7 : 5;

        /// <summary>
        /// При !twoVariables логический шаг 3 (degB первого одночлена) пропускается.
        /// Если откатываемся с шага 4 — нужно вернуться на 2, а не на 3 (который никогда не показывался).
        /// </summary>
        public override void RollbackStep(StepInputSession session)
        {
            session.CurrentStep--;
            if (session.Answers.Count > 0)
                session.Answers.RemoveAt(session.Answers.Count - 1);

            // После декремента проверяем: если попали на "скрытый" шаг 3 (!twoVariables),
            // откатываемся ещё на один — пользователь его не видел и не отвечал на него.
            bool twoVariables = session.Answers.Count > 0 && session.Answers[0] == "2";
            if (!twoVariables && session.CurrentStep == 3)
            {
                session.CurrentStep--;
                // Ответ для шага 3 не записывался — Answers не трогаем.
            }
        }

        /// <summary>Сопоставляет логический шаг с реальным индексом в Steps[].</summary>
        public static int StepIndex(List<string> answers, int logicalStep)
        {
            bool twoVariables = answers.Count > 0 && answers[0] == "2";
            // При одной переменной логический шаг ≥ 3 смещается на 1 (пропускаем Steps[3] — степень b)
            return !twoVariables && logicalStep >= 3 ? logicalStep + 1 : logicalStep;
        }

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 0) return null;

            bool twoVariables = answers[0] == "2";
            if (answers.Count == 1)
                return $"🔍 Переменных: {(twoVariables ? "a и b" : "только a")}";

            // Парсим первый одночлен
            double.TryParse(answers.Count > 1 ? answers[1].Replace(',', '.') : "0",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double coefficient1);
            int degreeA1 = answers.Count > 2 && int.TryParse(answers[2], out int pa1v) ? pa1v : 0;
            int degreeB1 = answers.Count > 3 && twoVariables && int.TryParse(answers[3], out int pb1v) ? pb1v : 0;
            var monomial1  = new Monomial(coefficient1, degreeA1, degreeB1);

            // Позиция начала второго одночлена
            int startIdx2  = twoVariables ? 4 : 3;
            double.TryParse(answers.Count > startIdx2 ? answers[startIdx2].Replace(',', '.') : "",
                NumberStyles.Any, CultureInfo.InvariantCulture, out double coefficient2);
            int degreeA2 = answers.Count > startIdx2 + 1 && int.TryParse(answers[startIdx2 + 1], out int pa2v) ? pa2v : 0;
            int degreeB2 = answers.Count > startIdx2 + 2 && twoVariables && int.TryParse(answers[startIdx2 + 2], out int pb2v) ? pb2v : 0;
            var monomial2  = new Monomial(coefficient2, degreeA2, degreeB2);

            if (answers.Count <= startIdx2) return $"🔍 Первый одночлен: {monomial1}";

            var result = monomial1 * monomial2;
            return $"🔍 ({monomial1}) · ({monomial2})\n    = {result}";
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVariables = answers[0] == "2";
            int i = 1;

            double coefficient1 = Monomial.ParseDouble(answers[i++]);
            int degreeA1 = Monomial.ParseInt(answers[i++]);
            int degreeB1 = twoVariables ? Monomial.ParseInt(answers[i++]) : 0;
            double coefficient2 = Monomial.ParseDouble(answers[i++]);
            int degreeA2 = Monomial.ParseInt(answers[i++]);
            int degreeB2 = twoVariables ? Monomial.ParseInt(answers[i++]) : 0;

            var monomial1 = new Monomial(coefficient1, degreeA1, degreeB1);
            var monomial2 = new Monomial(coefficient2, degreeA2, degreeB2);
            var result = monomial1 * monomial2;

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({monomial1}) · ({monomial2}) = {result}");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine("  Шаг 1. Перемножаем коэффициенты:");
            sb.AppendLine($"    {Monomial.Format(coefficient1)} · {Monomial.Format(coefficient2)} = {Monomial.Format(coefficient1 * coefficient2)}");
            sb.AppendLine("  Шаг 2. Складываем показатели a:");
            sb.AppendLine($"    {degreeA1} + {degreeA2} = {degreeA1 + degreeA2}  →  a{(degreeA1 + degreeA2 == 1 ? "" : Monomial.ToSuperscript(degreeA1 + degreeA2))}");

            if (twoVariables)
            {
                sb.AppendLine("  Шаг 3. Складываем показатели b:");
                sb.AppendLine($"    {degreeB1} + {degreeB2} = {degreeB1 + degreeB2}  →  b{(degreeB1 + degreeB2 == 1 ? "" : Monomial.ToSuperscript(degreeB1 + degreeB2))}");
            }

            sb.AppendLine();
            sb.AppendLine($"Собираем вместе: {result}");
            sb.AppendLine($"Степень результата: {result.TotalDegree}");

            if      (coefficient1 < 0 && coefficient2 < 0)     sb.AppendLine("Оба коэффициента отрицательные → минус на минус = плюс.");
            else if ((coefficient1 < 0) != (coefficient2 < 0)) sb.AppendLine("Один коэффициент отрицательный → результат отрицательный.");

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
                Validate = input => input is "1" or "2" ? null : "Введи 1 или 2"
            },
            new InputStep { Question = "✏️ Коэффициент делимого k₁:",         Validate = ValidateCoefficient },
            new InputStep { Question = "✏️ Степень a в делимом:",              Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Степень b в делимом (или 0):",      Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:",          Validate = ValidateCoefficient },
            new InputStep { Question = "✏️ Степень a в делителе:",             Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Степень b в делителе (или 0):",     Validate = ValidateDegree   },
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
                var monomial = FormatMonomial(answers[1], answers[2],
                    answers.Count > 3 ? answers[3] : "0", answers[0] == "2");
                return $"🔍 Делимое: {monomial}";
            }
            if (answers.Count >= 6)
            {
                var divisor = FormatMonomial(answers[4], answers[5],
                    answers.Count > 6 ? answers[6] : "0", answers[0] == "2");
                return $"🔍 Делитель: {divisor}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            bool twoVariables  = answers[0] == "2";
            int  i    = 1;
            double coefficient1 = Monomial.ParseDouble(answers[i++]);
            int  degreeA1  = Monomial.ParseInt(answers[i++]);
            int  degreeB1  = twoVariables ? Monomial.ParseInt(answers[i++]) : 0;
            double coefficient2 = Monomial.ParseDouble(answers[i++]);
            int  degreeA2  = Monomial.ParseInt(answers[i++]);
            int  degreeB2  = twoVariables ? Monomial.ParseInt(answers[i++]) : 0;

            if (coefficient2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double resultCoefficient = coefficient1 / coefficient2;
            int   resultDegreeA = degreeA1 - degreeA2;
            int   resultDegreeB = degreeB1 - degreeB2;

            var sb = new StringBuilder();
            string m1s = FormatMonomial(Monomial.Format(coefficient1), degreeA1.ToString(), degreeB1.ToString(), twoVariables);
            string m2s = FormatMonomial(Monomial.Format(coefficient2), degreeA2.ToString(), degreeB2.ToString(), twoVariables);
            sb.AppendLine($"📌 ({m1s}) ÷ ({m2s})");
            sb.AppendLine();
            sb.AppendLine("Разбираем по шагам:");
            sb.AppendLine($"  Шаг 1. Коэффициенты: {Monomial.Format(coefficient1)} ÷ {Monomial.Format(coefficient2)} = {Monomial.Format(resultCoefficient)}");
            if (degreeA1 != 0 || degreeA2 != 0)
                sb.AppendLine($"  Шаг 2. Степень a: a{Monomial.ToSuperscript(degreeA1)} ÷ a{Monomial.ToSuperscript(degreeA2)} = a{Monomial.ToSuperscript(resultDegreeA)}");
            if (twoVariables && (degreeB1 != 0 || degreeB2 != 0))
                sb.AppendLine($"  Шаг 3. Степень b: b{Monomial.ToSuperscript(degreeB1)} ÷ b{Monomial.ToSuperscript(degreeB2)} = b{Monomial.ToSuperscript(resultDegreeB)}");
            sb.AppendLine();
            sb.AppendLine($"✅ Ответ: {FormatMonomial(Monomial.Format(resultCoefficient), resultDegreeA.ToString(), resultDegreeB.ToString(), twoVariables)}");
            return sb.ToString().TrimEnd();
        }

        private static string FormatMonomial(string coefficient, string degreeA, string degreeB, bool twoVariables)
        {
            double kd = double.Parse(coefficient.Replace(",", "."), CultureInfo.InvariantCulture);
            int pai   = int.Parse(degreeA);
            int pbi   = int.TryParse(degreeB, out int tmp) ? tmp : 0;
            return new Monomial(kd, pai, twoVariables ? pbi : 0).ToString();
        }

        private static string? ValidateCoefficient(string input) =>
            double.TryParse(input.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out _)
                ? null
                : $"«{input}» — не число. Введи коэффициент, например: 46 или -7";

        private static string? ValidateDegree(string input) =>
            int.TryParse(input, out int result) && result >= 0
                ? null
                : $"«{input}» — введи целое число ≥ 0";
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
                Validate = ValidateCoefficient
            },
            new InputStep { Question = "✏️ Степень a в делимом (0 если нет):",  Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Степень b в делимом (0 если нет):",  Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Коэффициент делителя k₂:",           Validate = ValidateCoefficient },
            new InputStep { Question = "✏️ Степень a в делителе (0 если нет):", Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Степень b в делителе (0 если нет):", Validate = ValidateDegree   },
            new InputStep { Question = "✏️ Введи значение a:",                  Validate = ValidateCoefficient },
            new InputStep { Question = "✏️ Введи значение b:",                  Validate = ValidateCoefficient },
        ];

        public override string? GetPreview(List<string> answers)
        {
            if (answers.Count == 3)
            {
                string monomial = FormatResult(
                    Monomial.ParseDouble(answers[0]),
                    Monomial.ParseInt(answers[1]),
                    Monomial.ParseInt(answers[2]));
                return $"🔍 Делимое: {monomial}";
            }
            if (answers.Count == 6)
            {
                double coefficientResult = Monomial.ParseDouble(answers[0]) / Monomial.ParseDouble(answers[3]);
                int   degreeAResult = Monomial.ParseInt(answers[1]) - Monomial.ParseInt(answers[4]);
                int   degreeBResult = Monomial.ParseInt(answers[2]) - Monomial.ParseInt(answers[5]);
                return $"🔍 Упрощённое: {FormatResult(coefficientResult, degreeAResult, degreeBResult)}";
            }
            return null;
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            double coefficient1 = Monomial.ParseDouble(answers[0]);
            int    degreeA1 = Monomial.ParseInt(answers[1]), degreeB1 = Monomial.ParseInt(answers[2]);
            double coefficient2 = Monomial.ParseDouble(answers[3]);
            int    degreeA2 = Monomial.ParseInt(answers[4]), degreeB2 = Monomial.ParseInt(answers[5]);
            double aValue = Monomial.ParseDouble(answers[6]);
            double bValue = Monomial.ParseDouble(answers[7]);

            if (coefficient2 == 0) return "⚠️ Делитель не может быть равен нулю.";

            double resultCoefficient = coefficient1 / coefficient2;
            int    resultDegreeA = degreeA1 - degreeA2;
            int    resultDegreeB = degreeB1 - degreeB2;
            double result = resultCoefficient * Math.Pow(aValue, resultDegreeA) * Math.Pow(bValue, resultDegreeB);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {FormatResult(coefficient1, degreeA1, degreeB1)} ÷ {FormatResult(coefficient2, degreeA2, degreeB2)} при a={Monomial.Format(aValue)}, b={Monomial.Format(bValue)}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 1. Делим: {FormatResult(resultCoefficient, resultDegreeA, resultDegreeB)}");
            sb.AppendLine($"Шаг 2. Подставляем a={Monomial.Format(aValue)}, b={Monomial.Format(bValue)}:");
            sb.AppendLine($"  = {Monomial.Format(resultCoefficient)} · {Monomial.Format(aValue)}{Monomial.ToSuperscript(resultDegreeA)} · {Monomial.Format(bValue)}{Monomial.ToSuperscript(resultDegreeB)}");
            sb.Append($"\n✅ Ответ: {Monomial.Format(result)}");
            return sb.ToString().TrimEnd();
        }

        private static string FormatResult(double coefficient, int degreeA, int degreeB) =>
            new Monomial(coefficient, degreeA, degreeB).ToString();

        private static string? ValidateCoefficient(string input) =>
            double.TryParse(input.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out _)
                ? null
                : $"«{input}» — не число";

        private static string? ValidateDegree(string input) =>
            int.TryParse(input, out int result) && result >= 0
                ? null
                : $"«{input}» — введи целое число ≥ 0";
    }
}

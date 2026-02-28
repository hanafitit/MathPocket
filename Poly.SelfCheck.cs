using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Проверь себя — смешанные задачи по многочленам
    //  Только функции, которых нет в других разделах
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Деление многочлена на одночлен:
    /// каждый член многочлена делим на одночлен
    /// </summary>
    public class PolyDivideByMonomialFunction : FunctionBase
    {
        public override string   Name       => "Многочлен ÷ одночлен";
        public override string   Formula    => "(aₙxⁿ + … + a₀) ÷ kxᵐ";
        public override string[] Keywords   => new[] { "деление", "многочлен", "одночлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Деление многочлена на одночлен\n\n" +
                    "Правило: каждый член многочлена делим на одночлен.\n\n" +
                    "Пример: (24m²n²) ÷ (12m²n²)\n" +
                    "  = 24m²n² ÷ 12m²n²\n" +
                    "  = 2\n\n" +
                    "Пример: (6a²b + 9ab² − 3ab) ÷ 3ab\n" +
                    "  = 6a²b/3ab + 9ab²/3ab − 3ab/3ab\n" +
                    "  = 2a + 3b − 1\n\n" +
                    "✏️ Введи многочлен (делимое):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи одночлен-делитель (например: 3x^2 или -2x или 5):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly = PolyParser.Parse(answers[0]);
            var (k, deg) = PolyMultiplyHelper.ParseMonomial(answers[1]);
            string divisorStr = PolyMultiplyHelper.FormatMonomial(k, deg);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(poly)}) ÷ ({divisorStr})");
            sb.AppendLine();
            sb.AppendLine("Делим каждый член на одночлен:");

            var quotients = new List<PolyTerm>();
            bool hasRemainder = false;

            foreach (var t in poly)
            {
                int newDeg = t.Degree - deg;
                if (newDeg < 0 || t.Coeff % k != 0)
                {
                    sb.AppendLine($"  ({t.ToStringFirst()}) ÷ ({divisorStr}) — не делится нацело");
                    hasRemainder = true;
                    continue;
                }
                var q = new PolyTerm(t.Coeff / k, newDeg);
                sb.AppendLine($"  ({t.ToStringFirst()}) ÷ ({divisorStr}) = {PolyMultiplyHelper.FormatMonomial(q.Coeff, q.Degree)}");
                quotients.Add(q);
            }

            sb.AppendLine();
            if (hasRemainder)
            {
                sb.AppendLine("⚠️ Некоторые члены не делятся нацело — проверь делитель.");
            }
            else
            {
                var reduced = PolyParser.Reduce(quotients);
                sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// При каком значении x два выражения равны (x² − 6x − 1 и 6 + x² + x)
    /// </summary>
    public class PolyFindEqualXFunction : FunctionBase
    {
        public override string   Name       => "При каком x выражения равны";
        public override string   Formula    => "A(x) = B(x) → найти x";
        public override string[] Keywords   => new[] { "равны", "значение x", "найти", "при каком" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 При каком значении x два выражения равны?\n\n" +
                    "Пример: x² − 6x − 1 и 6 + x² + x\n" +
                    "  x² − 6x − 1 = 6 + x² + x\n" +
                    "  −6x − x = 6 + 1\n" +
                    "  −7x = 7  →  x = −1\n\n" +
                    "✏️ Введи первое выражение:",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи второе выражение:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var a   = PolyParser.Parse(answers[0]);
            var b   = PolyParser.Parse(answers[1]);
            var bNeg = b.Select(t => new PolyTerm(-t.Coeff, t.Degree)).ToList();
            var diff = PolyParser.Reduce(a.Concat(bNeg).ToList());

            var sb = new StringBuilder();
            sb.AppendLine($"✅ {PolyParser.Format(a)} = {PolyParser.Format(b)}");
            sb.AppendLine();
            sb.AppendLine("Переносим всё влево:");
            sb.AppendLine($"  {PolyParser.Format(diff)} = 0");
            sb.AppendLine();

            int deg = PolyParser.PolynomialDegree(diff);

            if (deg == 0)
            {
                long free = diff.FirstOrDefault(t => t.Degree == 0).Coeff;
                sb.AppendLine(free == 0
                    ? "📌 Выражения равны при любом x."
                    : "📌 Выражения не равны ни при каком x.");
            }
            else if (deg == 1)
            {
                long a1 = diff.FirstOrDefault(t => t.Degree == 1).Coeff;
                long a0 = diff.FirstOrDefault(t => t.Degree == 0).Coeff;
                sb.AppendLine($"  {a1}x = {-a0}");
                string xVal = (a0 % a1 == 0) ? $"{-a0 / a1}" : $"{-a0}/{a1}";
                sb.AppendLine();
                sb.AppendLine($"📌 x = {xVal}");
            }
            else
            {
                sb.AppendLine($"Уравнение степени {deg}.");
                sb.AppendLine("Для решения используй раздел «Квадратные уравнения».");
                sb.AppendLine($"📌 {PolyParser.Format(diff)} = 0");
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Найти общий множитель выражения (для подготовки к группировке)
    /// </summary>
    public class PolyFindGcmExpressionFunction : FunctionBase
    {
        public override string   Name       => "Найти общий множитель выражения";
        public override string   Formula    => "8x·y³ − 8x³y = 8xy(y² − x²)";
        public override string[] Keywords   => new[] { "общий множитель", "вынести", "выражение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти общий множитель выражения\n\n" +
                    "Пример: 8xy³ − 8x³y\n" +
                    "  НОД коэффициентов: НОД(8, 8) = 8\n" +
                    "  Общая степень x: min(1, 3) = 1\n" +
                    "  Общий множитель: 8x\n" +
                    "  8x(y³ − x²y)\n\n" +
                    "✏️ Введи многочлен:",
                Validate = PolyValidate.CheckPoly
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var terms   = PolyParser.Parse(answers[0]);
            var reduced = PolyParser.Reduce(terms);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ Многочлен: {PolyParser.Format(reduced)}");
            sb.AppendLine();

            long gcd    = reduced.Select(t => Math.Abs(t.Coeff)).Aggregate(GCD);
            int  minDeg = reduced.Min(t => t.Degree);

            sb.AppendLine("Шаг 1. НОД коэффициентов:");
            sb.AppendLine($"  НОД({string.Join(", ", reduced.Select(t => Math.Abs(t.Coeff)))}) = {gcd}");
            sb.AppendLine();

            if (minDeg > 0)
            {
                sb.AppendLine($"Шаг 2. Минимальная степень x среди всех членов: {minDeg}");
                sb.AppendLine();
            }

            string commonMono = PolyMultiplyHelper.FormatMonomial(gcd, minDeg);
            sb.AppendLine($"Общий множитель: {commonMono}");
            sb.AppendLine();

            if (gcd == 1 && minDeg == 0)
            {
                sb.AppendLine("Общего числового или буквенного множителя нет.");
                sb.AppendLine("Попробуй «Группировка: 4 члена (2+2)» для разложения.");
            }
            else
            {
                var inner = reduced.Select(t => new PolyTerm(t.Coeff / gcd, t.Degree - minDeg)).ToList();
                sb.AppendLine($"Выносим за скобки:");
                sb.AppendLine($"  {commonMono}·({PolyParser.Format(inner)})");
                sb.AppendLine();
                sb.AppendLine($"📌 {PolyParser.Format(reduced)} = {commonMono}·({PolyParser.Format(inner)})");
            }

            return sb.ToString().TrimEnd();
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    }
}

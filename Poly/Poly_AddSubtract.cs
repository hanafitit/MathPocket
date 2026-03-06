using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
    internal class PolynomialAddFunction : FunctionBase
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

            var groups = all.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
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
    //  Вычитание многочленов
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

            var groups = all.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();
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
    //  Представить в стандартном виде и назвать степень
}

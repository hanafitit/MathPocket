using System;
using System.Collections.Generic;
using System.Text;

namespace MathPocket
{
    public class SquareRootSimplifyFunction : FunctionBase
    {
        public override string Name => "Упростить квадратный корень";
        public override string Formula => "√n = a√b";
        public override string[] Keywords => ["корень", "квадратный", "упростить", "извлечь"];
        public override string[] Parameters => [];

        public override double Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question =
                    "📘 Упрощение квадратного корня\n\n" +
                    "Чтобы упростить √n, нужно найти наибольший квадрат,\n" +
                    "на который делится число n.\n\n" +
                    "Пример: √72\n" +
                    "  72 делится на 36 (это 6²)\n" +
                    "  √72 = √(36 · 2) = √36 · √2 = 6√2\n\n" +
                    "✏️ Введи число под корнем (n):",
                Validate = input =>
                {
                    if (int.TryParse(input, out int n) && n >= 0) return null;
                    return "Введи целое неотрицательное число.";
                }
            }
        ];

        public override string CalculateFromAnswers(List<string> answers)
        {
            int n = int.Parse(answers[0]);
            if (n == 0) return "√0 = 0";
            if (n == 1) return "√1 = 1";

            int outside = 1;
            int inside = n;
            int d = 2;

            var sb = new StringBuilder();
            sb.AppendLine($"🔍 Упрощаем √{n}:");
            sb.AppendLine();

            while (d * d <= inside)
            {
                if (inside % (d * d) == 0)
                {
                    sb.AppendLine($"  {inside} делится на {d}² ({d*d})");
                    outside *= d;
                    inside /= (d * d);
                }
                else
                {
                    d++;
                }
            }

            if (outside == 1)
            {
                sb.AppendLine("Число не имеет квадратичных делителей.");
                sb.AppendLine($"\n✅ Ответ: √{n} (не упрощается)");
            }
            else
            {
                sb.AppendLine($"\nВыносим {outside} из-под корня.");
                string result = inside == 1 ? $"{outside}" : $"{outside}√{inside}";
                sb.AppendLine($"\n✅ Ответ: √{n} = {result}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}

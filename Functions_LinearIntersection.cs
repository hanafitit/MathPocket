using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    //  § 23. Взаимное расположение графиков линейных функций

    //  23.1 Определить взаимное расположение двух функций
    //  (пересекаются / параллельны / совпадают)

    public class LinearRelationFunction : FunctionBase
    {
        public override string   Name       => "Взаимное расположение двух функций";
        public override string   Formula    => "y = k₁x + b₁  и  y = k₂x + b₂";
        public override string[] Keywords   => new[] { "взаимное", "расположение", "параллельны", "пересекаются", "совпадают" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Взаимное расположение графиков линейных функций\n\n" +
                    "Два графика y = k₁x + b₁ и y = k₂x + b₂ могут:\n" +
                    "  · Пересекаться — если k₁ ≠ k₂\n" +
                    "  · Быть параллельными — если k₁ = k₂, но b₁ ≠ b₂\n" +
                    "  · Совпадать — если k₁ = k₂ и b₁ = b₂\n\n" +
                    "✏️ Введи первую функцию (после y =):\n" +
                    "  Пример: 3x+2  или  -x-5",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи вторую функцию (после y =):\n" +
                           "  Пример: 2x-5  или  3x+1",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция 1: {LinearHelper.FormatLinear(k1, b1)}");
            sb.AppendLine($"Функция 2: {LinearHelper.FormatLinear(k2, b2)}");
            sb.AppendLine();
            sb.AppendLine($"Коэффициенты:");
            sb.AppendLine($"  k₁ = {LinearHelper.Fmt(k1)},  b₁ = {LinearHelper.Fmt(b1)}");
            sb.AppendLine($"  k₂ = {LinearHelper.Fmt(k2)},  b₂ = {LinearHelper.Fmt(b2)}");
            sb.AppendLine();

            bool kEqual = Math.Abs(k1 - k2) < 1e-9;
            bool bEqual = Math.Abs(b1 - b2) < 1e-9;

            if (!kEqual)
            {
                double x = (b2 - b1) / (k1 - k2);
                double y = k1 * x + b1;
                sb.AppendLine("Шаг 1. k₁ ≠ k₂ — графики пересекаются.");
                sb.AppendLine();
                sb.AppendLine("Шаг 2. Находим точку пересечения:");
                sb.AppendLine($"  k₁x + b₁ = k₂x + b₂");
                sb.AppendLine($"  {LinearHelper.Fmt(k1)}x + ({LinearHelper.Fmt(b1)}) = {LinearHelper.Fmt(k2)}x + ({LinearHelper.Fmt(b2)})");
                sb.AppendLine($"  ({LinearHelper.Fmt(k1)} − {LinearHelper.Fmt(k2)})x = {LinearHelper.Fmt(b2)} − {LinearHelper.Fmt(b1)}");
                sb.AppendLine($"  {LinearHelper.Fmt(k1 - k2)}x = {LinearHelper.Fmt(b2 - b1)}");
                sb.AppendLine($"  x = {LinearHelper.Fmt(x)}");
                sb.AppendLine($"  y = {LinearHelper.Fmt(k1)}·{LinearHelper.Fmt(x)} + {LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(y)}");
                sb.AppendLine();
                sb.AppendLine($"📌 Графики ПЕРЕСЕКАЮТСЯ в точке ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)}).");
            }
            else if (!bEqual)
            {
                sb.AppendLine("Шаг 1. k₁ = k₂ — угловые коэффициенты равны.");
                sb.AppendLine("Шаг 2. b₁ ≠ b₂ — свободные члены различны.");
                sb.AppendLine();
                sb.AppendLine("📌 Графики ПАРАЛЛЕЛЬНЫ.");
                sb.AppendLine("  Общих точек нет.");
            }
            else
            {
                sb.AppendLine("Шаг 1. k₁ = k₂ — угловые коэффициенты равны.");
                sb.AppendLine("Шаг 2. b₁ = b₂ — свободные члены тоже равны.");
                sb.AppendLine();
                sb.AppendLine("📌 Графики СОВПАДАЮТ.");
                sb.AppendLine("  Это одна и та же прямая.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  23.2 / 23.3 Написать формулу параллельной / пересекающейся /
    //  совпадающей функции по условию

    public class LinearWriteRelatedFunction : FunctionBase
    {
        public override string   Name       => "Написать формулу параллельной / совпадающей функции";
        public override string   Formula    => "y = kx + b  →  параллельная / совпадающая";
        public override string[] Keywords   => new[] { "параллельная", "совпадающая", "написать формулу", "пересекает" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Написать формулу функции по условию\n\n" +
                    "Правила:\n" +
                    "  · Параллельная: тот же k, другой b\n" +
                    "    Пример: к y = 3x + 1 параллельна y = 3x − 5\n\n" +
                    "  · Совпадающая: тот же k, тот же b\n" +
                    "    Пример: к y = 3x + 1 совпадает y = 3x + 1\n\n" +
                    "  · Пересекающая: другой k\n" +
                    "    Пример: к y = 3x + 1 пересекает y = 2x + 1\n\n" +
                    "✏️ Введи данную функцию (после y =):\n" +
                    "  Пример: 3x+2",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question =
                    "✏️ Что нужно написать?\n" +
                    "  Введи:  1 — параллельную\n" +
                    "          2 — совпадающую\n" +
                    "          3 — пересекающую",
                Validate = s =>
                {
                    string t = s.Trim();
                    return t == "1" || t == "2" || t == "3" ? null : "Введи: 1, 2 или 3";
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            string mode = answers[1].Trim();
            var sb = new StringBuilder();

            sb.AppendLine($"Данная функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)},  b = {LinearHelper.Fmt(b)}");
            sb.AppendLine();

            switch (mode)
            {
                case "1":
                    double newB = b + 3;
                    sb.AppendLine("Параллельная функция: тот же k, но другой b.");
                    sb.AppendLine($"  Берём k = {LinearHelper.Fmt(k)}, b = {LinearHelper.Fmt(newB)} (любое ≠ {LinearHelper.Fmt(b)})");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Пример параллельной: {LinearHelper.FormatLinear(k, newB)}");
                    sb.AppendLine($"   (можно взять любой b ≠ {LinearHelper.Fmt(b)})");
                    break;

                case "2":
                    sb.AppendLine("Совпадающая функция: тот же k и тот же b.");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Совпадающая: {LinearHelper.FormatLinear(k, b)}");
                    sb.AppendLine("   (это та же самая прямая)");
                    break;

                case "3":
                    double newK = k + 1;
                    sb.AppendLine("Пересекающая функция: другой k (b — любой).");
                    sb.AppendLine($"  Берём k = {LinearHelper.Fmt(newK)} (любое ≠ {LinearHelper.Fmt(k)})");
                    sb.AppendLine();
                    sb.AppendLine($"📌 Пример пересекающей: {LinearHelper.FormatLinear(newK, b)}");
                    sb.AppendLine($"   (можно взять любой k ≠ {LinearHelper.Fmt(k)})");
                    break;
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  23.5 Найти координаты точки пересечения двух функций

    public class LinearIntersectionPointFunction : FunctionBase
    {
        public override string   Name       => "Найти точку пересечения двух функций";
        public override string   Formula    => "k₁x + b₁ = k₂x + b₂  →  (x; y)";
        public override string[] Keywords   => new[] { "точка пересечения", "координаты", "два графика" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти точку пересечения двух графиков\n\n" +
                    "Приравниваем правые части:\n" +
                    "  k₁x + b₁ = k₂x + b₂\n" +
                    "  Переносим x в одну сторону, числа в другую\n" +
                    "  Находим x, затем подставляем в любую функцию\n\n" +
                    "Пример: y = −6x + 1 и y = 5x + 9\n" +
                    "  −6x + 1 = 5x + 9  →  −11x = 8  →  x = −8/11\n\n" +
                    "✏️ Введи первую функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи вторую функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция 1: {LinearHelper.FormatLinear(k1, b1)}");
            sb.AppendLine($"Функция 2: {LinearHelper.FormatLinear(k2, b2)}");
            sb.AppendLine();

            if (Math.Abs(k1 - k2) < 1e-9)
            {
                if (Math.Abs(b1 - b2) < 1e-9)
                    sb.AppendLine("📌 Функции совпадают — пересекаются во всех точках.");
                else
                    sb.AppendLine("📌 Функции параллельны — точек пересечения нет.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine("Шаг 1. Приравниваем:");
            sb.AppendLine($"  {LinearHelper.Fmt(k1)}x + ({LinearHelper.Fmt(b1)}) = {LinearHelper.Fmt(k2)}x + ({LinearHelper.Fmt(b2)})");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Переносим x влево, числа вправо:");
            sb.AppendLine($"  {LinearHelper.Fmt(k1)}x − {LinearHelper.Fmt(k2)}x = {LinearHelper.Fmt(b2)} − {LinearHelper.Fmt(b1)}");
            sb.AppendLine($"  {LinearHelper.Fmt(k1 - k2)}x = {LinearHelper.Fmt(b2 - b1)}");
            sb.AppendLine();

            double x = (b2 - b1) / (k1 - k2);
            double y = k1 * x + b1;

            sb.AppendLine("Шаг 3. Находим x:");
            sb.AppendLine($"  x = {LinearHelper.Fmt(b2 - b1)} / {LinearHelper.Fmt(k1 - k2)} = {LinearHelper.Fmt(x)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 4. Находим y (подставляем в первую функцию):");
            sb.AppendLine($"  y = {LinearHelper.Fmt(k1)}·{LinearHelper.Fmt(x)} + {LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(y)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Точка пересечения: ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)})");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.6 Доказать что графики пересекаются

    public class LinearProveIntersectFunction : FunctionBase
    {
        public override string   Name       => "Доказать что графики пересекаются";
        public override string   Formula    => "k₁ ≠ k₂  →  пересекаются";
        public override string[] Keywords   => new[] { "доказать", "пересекаются", "доказательство" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Доказать что два графика пересекаются\n\n" +
                    "Достаточно показать что k₁ ≠ k₂.\n" +
                    "Тогда система имеет единственное решение —\n" +
                    "точку пересечения.\n\n" +
                    "✏️ Введи первую функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи вторую функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция 1: {LinearHelper.FormatLinear(k1, b1)}  →  k₁ = {LinearHelper.Fmt(k1)}");
            sb.AppendLine($"Функция 2: {LinearHelper.FormatLinear(k2, b2)}  →  k₂ = {LinearHelper.Fmt(k2)}");
            sb.AppendLine();

            if (Math.Abs(k1 - k2) < 1e-9)
            {
                sb.AppendLine($"k₁ = k₂ = {LinearHelper.Fmt(k1)} — угловые коэффициенты равны.");
                sb.AppendLine();
                if (Math.Abs(b1 - b2) < 1e-9)
                    sb.AppendLine("📌 Графики совпадают — пересечение доказать нельзя.");
                else
                    sb.AppendLine("📌 Графики параллельны — они НЕ пересекаются.");
                return sb.ToString().TrimEnd();
            }

            double x = (b2 - b1) / (k1 - k2);
            double y = k1 * x + b1;

            sb.AppendLine($"k₁ = {LinearHelper.Fmt(k1)} ≠ k₂ = {LinearHelper.Fmt(k2)}");
            sb.AppendLine();
            sb.AppendLine("Доказательство:");
            sb.AppendLine("  Приравниваем правые части:");
            sb.AppendLine($"  {LinearHelper.Fmt(k1)}x + {LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(k2)}x + {LinearHelper.Fmt(b2)}");
            sb.AppendLine($"  {LinearHelper.Fmt(k1 - k2)}x = {LinearHelper.Fmt(b2 - b1)}");
            sb.AppendLine($"  x = {LinearHelper.Fmt(x)}  — единственное решение");
            sb.AppendLine($"  y = {LinearHelper.Fmt(y)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Так как k₁ ≠ k₂, графики ПЕРЕСЕКАЮТСЯ.");
            sb.AppendLine($"   Точка пересечения: ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)})  ✓");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.8 Написать несколько формул параллельных функций

    public class LinearParallelExamplesFunction : FunctionBase
    {
        public override string   Name       => "Примеры параллельных функций";
        public override string   Formula    => "y = kx + b₁,  y = kx + b₂, ...";
        public override string[] Keywords   => new[] { "параллельные", "несколько", "примеры" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Несколько параллельных функций\n\n" +
                    "Все функции с одинаковым k параллельны друг другу.\n" +
                    "Меняем только b.\n\n" +
                    "Пример: к y = −4 параллельны y = −4 + 1 = y=1... нет,\n" +
                    "здесь k=0, параллельны все горизонтальные прямые.\n\n" +
                    "✏️ Введи функцию (после y =):\n" +
                    "  Пример: 3x+2  или  -4  или  0.5x",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Данная функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)}");
            sb.AppendLine();
            sb.AppendLine("Параллельные функции имеют тот же k, но другой b:");
            sb.AppendLine();

            double[] bValues = { b + 1, b + 2, b - 1, b - 2, b + 5 };
            int n = 1;
            foreach (double bv in bValues)
            {
                sb.AppendLine($"  {n++}. {LinearHelper.FormatLinear(k, bv)}");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Любая функция вида y = {LinearHelper.Fmt(k)}x + b при b ≠ {LinearHelper.Fmt(b)}");
            sb.AppendLine($"   параллельна данной.");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.9 Построить два графика в одной системе координат

    public class LinearTwoGraphsFunction : FunctionBase
    {
        public override string   Name       => "Построить два графика в одной системе";
        public override string   Formula    => "y = k₁x + b₁  и  y = k₂x + b₂";
        public override string[] Keywords   => new[] { "два графика", "одна система", "построить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Построить два графика в одной системе координат\n\n" +
                    "Бот построит оба графика и покажет их взаимное расположение.\n\n" +
                    "✏️ Введи первую функцию (после y =):\n" +
                    "  Пример: 0.5x+3",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи вторую функцию (после y =):\n" +
                           "  Пример: -0.5x+3",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override byte[]? GetPlotBytes(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            return PlotHelper.TwoLinearFunctions(k1, b1, k2, b2);
        }

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция 1: {LinearHelper.FormatLinear(k1, b1)}");
            sb.AppendLine($"Функция 2: {LinearHelper.FormatLinear(k2, b2)}");
            sb.AppendLine();

            bool kEqual = Math.Abs(k1 - k2) < 1e-9;
            bool bEqual = Math.Abs(b1 - b2) < 1e-9;

            if (!kEqual)
            {
                double x = (b2 - b1) / (k1 - k2);
                double y = k1 * x + b1;
                sb.AppendLine($"📌 Графики ПЕРЕСЕКАЮТСЯ в точке ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)}).");
            }
            else if (!bEqual)
            {
                sb.AppendLine("📌 Графики ПАРАЛЛЕЛЬНЫ (k₁ = k₂, b₁ ≠ b₂).");
            }
            else
            {
                sb.AppendLine("📌 Графики СОВПАДАЮТ (k₁ = k₂, b₁ = b₂).");
            }

            return sb.ToString().TrimEnd();
        }
    }

    //  23.10 Написать формулу функции, график которой пересекает
    //  ось ординат в точке (0; b) и параллелен данной функции

    public class LinearFindByPointOnOyFunction : FunctionBase
    {
        public override string   Name       => "Функция по точке на Oy и параллельности";
        public override string   Formula    => "График через (0; b) параллельный данной";
        public override string[] Keywords   => new[] { "пересекает ось ординат", "параллельна", "найти формулу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти функцию по точке на оси Oy и параллельности\n\n" +
                    "Если график параллелен y = kx + b₀ и\n" +
                    "пересекает ось Oy в точке (0; b) — то:\n" +
                    "  · k берём из данной функции\n" +
                    "  · b берём из условия точки\n\n" +
                    "Пример: параллельна y = 4x − 7, проходит через (0; −3,5)\n" +
                    "  k = 4,  b = −3,5  →  y = 4x − 3,5\n\n" +
                    "✏️ Введи данную функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи координату b точки на оси Oy\n" +
                           "  (точка вида (0; b)):",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b0) = LinearHelper.ParseLinear(answers[0])!.Value;
            double b   = LinearHelper.ParseNumber(answers[1]);
            var sb     = new StringBuilder();

            sb.AppendLine($"Данная функция: {LinearHelper.FormatLinear(k, b0)}");
            sb.AppendLine($"Точка на оси Oy: (0; {LinearHelper.Fmt(b)})");
            sb.AppendLine();
            sb.AppendLine("Параллельная функция имеет тот же k:");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)}  (берём из данной)");
            sb.AppendLine($"  b = {LinearHelper.Fmt(b)}  (берём из точки)");
            sb.AppendLine();
            sb.AppendLine($"📌 Искомая функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.11 Найти b, зная что два графика пересекаются
    //  в одной и той же точке с третьим графиком

    public class LinearFindBFromIntersectionFunction : FunctionBase
    {
        public override string   Name       => "Найти b из условия общей точки";
        public override string   Formula    => "Графики пересекаются в одной точке с y = kx + b";
        public override string[] Keywords   => new[] { "найти b", "общая точка", "пересекаются в одной точке" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти b — графики пересекаются в одной точке\n\n" +
                    "Есть три функции: y = f₁(x), y = f₂(x), y = kx + b.\n" +
                    "Известно, что y = kx + b пересекается с f₁ и f₂\n" +
                    "в одной и той же точке.\n\n" +
                    "Метод:\n" +
                    "  1. Найти точку пересечения f₁ и f₂\n" +
                    "  2. Подставить её в y = kx + b → найти b\n\n" +
                    "✏️ Введи первую известную функцию (после y =):\n" +
                    "  Пример: x+7,2",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи вторую известную функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи k искомой функции y = kx + b:",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, b1) = LinearHelper.ParseLinear(answers[0])!.Value;
            var (k2, b2) = LinearHelper.ParseLinear(answers[1])!.Value;
            double k     = LinearHelper.ParseNumber(answers[2]);
            var sb       = new StringBuilder();

            sb.AppendLine($"Функция 1: {LinearHelper.FormatLinear(k1, b1)}");
            sb.AppendLine($"Функция 2: {LinearHelper.FormatLinear(k2, b2)}");
            sb.AppendLine($"Искомая:   y = {LinearHelper.Fmt(k)}x + b");
            sb.AppendLine();

            if (Math.Abs(k1 - k2) < 1e-9)
            {
                sb.AppendLine("⚠️ Функции 1 и 2 параллельны или совпадают — общей точки нет.");
                return sb.ToString().TrimEnd();
            }

            double x = (b2 - b1) / (k1 - k2);
            double y = k1 * x + b1;

            sb.AppendLine("Шаг 1. Находим общую точку функций 1 и 2:");
            sb.AppendLine($"  {LinearHelper.Fmt(k1)}x + {LinearHelper.Fmt(b1)} = {LinearHelper.Fmt(k2)}x + {LinearHelper.Fmt(b2)}");
            sb.AppendLine($"  x = {LinearHelper.Fmt(x)},  y = {LinearHelper.Fmt(y)}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 2. Подставляем точку ({LinearHelper.Fmt(x)}; {LinearHelper.Fmt(y)}) в y = {LinearHelper.Fmt(k)}x + b:");
            sb.AppendLine($"  {LinearHelper.Fmt(y)} = {LinearHelper.Fmt(k)}·{LinearHelper.Fmt(x)} + b");
            sb.AppendLine($"  {LinearHelper.Fmt(y)} = {LinearHelper.Fmt(k * x)} + b");

            double b = y - k * x;
            sb.AppendLine($"  b = {LinearHelper.Fmt(y)} − {LinearHelper.Fmt(k * x)}");
            sb.AppendLine();
            sb.AppendLine($"📌 b = {LinearHelper.Fmt(b)}");
            sb.AppendLine($"   Искомая функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.12 В каких четвертях расположен график y = kx + b

    public class LinearQuadrantsFunction : FunctionBase
    {
        public override string   Name       => "В каких четвертях расположен график";
        public override string   Formula    => "Знак k и b определяют четверти";
        public override string[] Keywords   => new[] { "четверти", "координатные четверти", "расположен" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 В каких четвертях расположен график?\n\n" +
                    "Правила для y = kx + b:\n" +
                    "  · k > 0, b > 0 → I, II, III четверти\n" +
                    "  · k > 0, b < 0 → I, III, IV четверти\n" +
                    "  · k > 0, b = 0 → I и III (через начало координат)\n" +
                    "  · k < 0, b > 0 → I, II, IV четверти\n" +
                    "  · k < 0, b < 0 → II, III, IV четверти\n" +
                    "  · k < 0, b = 0 → II и IV (через начало координат)\n" +
                    "  · k = 0 → горизонтальная прямая\n\n" +
                    "✏️ Введи функцию (после y =):\n" +
                    "  Пример: 7x+5",
                Validate = LinearHelper.ValidateLinear
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b) = LinearHelper.ParseLinear(answers[0])!.Value;
            var sb = new StringBuilder();

            sb.AppendLine($"Функция: {LinearHelper.FormatLinear(k, b)}");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)},  b = {LinearHelper.Fmt(b)}");
            sb.AppendLine();

            if (Math.Abs(k) < 1e-9)
            {
                sb.AppendLine("k = 0 — горизонтальная прямая.");
                if (Math.Abs(b) < 1e-9)
                    sb.AppendLine("📌 График совпадает с осью Ox (не в четвертях).");
                else if (b > 0)
                    sb.AppendLine("📌 График проходит через I и II четверти (y > 0).");
                else
                    sb.AppendLine("📌 График проходит через III и IV четверти (y < 0).");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine($"Анализ знаков:");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)} → {(k > 0 ? "k > 0 (возрастающая)" : "k < 0 (убывающая)")}");
            sb.AppendLine($"  b = {LinearHelper.Fmt(b)} → {(Math.Abs(b) < 1e-9 ? "b = 0 (через начало)" : b > 0 ? "b > 0" : "b < 0")}");
            sb.AppendLine();

            List<string> quarters = new();
            if (Math.Abs(b) < 1e-9)
            {
                // Через начало координат
                if (k > 0) { quarters.Add("I"); quarters.Add("III"); }
                else       { quarters.Add("II"); quarters.Add("IV"); }
                sb.AppendLine("b = 0 → график проходит через начало координат.");
            }
            else if (k > 0 && b > 0)
            {
                quarters.AddRange(new[] { "I", "II", "III" });
                sb.AppendLine("k > 0, b > 0 → прямая пересекает Oy выше нуля, возрастает.");
            }
            else if (k > 0 && b < 0)
            {
                quarters.AddRange(new[] { "I", "III", "IV" });
                sb.AppendLine("k > 0, b < 0 → прямая пересекает Oy ниже нуля, возрастает.");
            }
            else if (k < 0 && b > 0)
            {
                quarters.AddRange(new[] { "I", "II", "IV" });
                sb.AppendLine("k < 0, b > 0 → прямая пересекает Oy выше нуля, убывает.");
            }
            else
            {
                quarters.AddRange(new[] { "II", "III", "IV" });
                sb.AppendLine("k < 0, b < 0 → прямая пересекает Oy ниже нуля, убывает.");
            }

            sb.AppendLine();
            sb.AppendLine($"📌 График проходит через {string.Join(", ", quarters)} четверти.");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.13 Написать формулу функции по точке и точке на оси Oy

    public class LinearFindFormulaByPointAndOyFunction : FunctionBase
    {
        public override string   Name       => "Формула функции по точке и пересечению с Oy";
        public override string   Formula    => "График через A(x₀; y₀) и (0; b)";
        public override string[] Keywords   => new[] { "формула", "точка", "пересекает ось ординат", "найти формулу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти формулу функции\n\n" +
                    "Дано: график проходит через точку A(x₀; y₀)\n" +
                    "и пересекает ось Oy в точке (0; b).\n\n" +
                    "Метод:\n" +
                    "  b берём сразу из точки на Oy\n" +
                    "  k = (y₀ − b) / x₀\n\n" +
                    "Пример: A(1; 3), пересекает Oy в (0; −5)\n" +
                    "  b = −5,  k = (3 − (−5)) / 1 = 8\n" +
                    "  y = 8x − 5\n\n" +
                    "✏️ Введи x₀ точки A:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи y₀ точки A:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи b — координату пересечения с осью Oy\n" +
                           "  (точка (0; b)):",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            double x0 = LinearHelper.ParseNumber(answers[0]);
            double y0 = LinearHelper.ParseNumber(answers[1]);
            double b  = LinearHelper.ParseNumber(answers[2]);
            var sb    = new StringBuilder();

            sb.AppendLine($"Точка A: ({LinearHelper.Fmt(x0)}; {LinearHelper.Fmt(y0)})");
            sb.AppendLine($"Пересечение с Oy: (0; {LinearHelper.Fmt(b)})  →  b = {LinearHelper.Fmt(b)}");
            sb.AppendLine();

            if (Math.Abs(x0) < 1e-12)
            {
                sb.AppendLine("⚠️ x₀ = 0 — точка A сама лежит на оси Oy.");
                sb.AppendLine("k не определяется — функция не единственна.");
                return sb.ToString().TrimEnd();
            }

            double k = (y0 - b) / x0;
            sb.AppendLine("Шаг 1. b известен из условия:");
            sb.AppendLine($"  b = {LinearHelper.Fmt(b)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Находим k через точку A:");
            sb.AppendLine($"  y₀ = k·x₀ + b");
            sb.AppendLine($"  {LinearHelper.Fmt(y0)} = k·{LinearHelper.Fmt(x0)} + {LinearHelper.Fmt(b)}");
            sb.AppendLine($"  k = ({LinearHelper.Fmt(y0)} − {LinearHelper.Fmt(b)}) / {LinearHelper.Fmt(x0)}");
            sb.AppendLine($"  k = {LinearHelper.Fmt(y0 - b)} / {LinearHelper.Fmt(x0)} = {LinearHelper.Fmt(k)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Искомая функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }

    //  23.14 Найти формулу функции параллельной данной
    //  и проходящей через заданную точку

    public class LinearParallelThroughPointFunction : FunctionBase
    {
        public override string   Name       => "Параллельная функция через заданную точку";
        public override string   Formula    => "y = kx + b: тот же k, через точку A";
        public override string[] Keywords   => new[] { "параллельная", "через точку", "найти формулу" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Параллельная функция через заданную точку\n\n" +
                    "Параллельная функция имеет тот же k.\n" +
                    "b находим, подставляя точку A в y = kx + b.\n\n" +
                    "Пример: параллельна y = 3x + 5, через A(−4; 1)\n" +
                    "  k = 3\n" +
                    "  1 = 3·(−4) + b  →  b = 13\n" +
                    "  y = 3x + 13\n\n" +
                    "✏️ Введи данную функцию (после y =):",
                Validate = LinearHelper.ValidateLinear
            },
            new InputStep
            {
                Question = "✏️ Введи x точки A:",
                Validate = LinearHelper.ValidateNumber
            },
            new InputStep
            {
                Question = "✏️ Введи y точки A:",
                Validate = LinearHelper.ValidateNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k, b0) = LinearHelper.ParseLinear(answers[0])!.Value;
            double px  = LinearHelper.ParseNumber(answers[1]);
            double py  = LinearHelper.ParseNumber(answers[2]);
            var sb     = new StringBuilder();

            sb.AppendLine($"Данная функция: {LinearHelper.FormatLinear(k, b0)}  →  k = {LinearHelper.Fmt(k)}");
            sb.AppendLine($"Точка A: ({LinearHelper.Fmt(px)}; {LinearHelper.Fmt(py)})");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Берём k из данной функции:");
            sb.AppendLine($"  k = {LinearHelper.Fmt(k)}");
            sb.AppendLine();
            sb.AppendLine("Шаг 2. Находим b, подставляя точку A:");
            sb.AppendLine($"  y = kx + b");
            sb.AppendLine($"  {LinearHelper.Fmt(py)} = {LinearHelper.Fmt(k)}·{LinearHelper.Fmt(px)} + b");
            sb.AppendLine($"  {LinearHelper.Fmt(py)} = {LinearHelper.Fmt(k * px)} + b");

            double b = py - k * px;
            sb.AppendLine($"  b = {LinearHelper.Fmt(py)} − {LinearHelper.Fmt(k * px)} = {LinearHelper.Fmt(b)}");
            sb.AppendLine();
            sb.AppendLine($"📌 Искомая функция: {LinearHelper.FormatLinear(k, b)}");

            return sb.ToString().TrimEnd();
        }
    }
}

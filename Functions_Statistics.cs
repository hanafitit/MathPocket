using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ─── Вспомогательный класс для статистики ────────────────────

    internal static class StatHelper
    {
        public static string Fmt(double v)
        {
            if (Math.Abs(v - Math.Round(v)) < 1e-9)
                return ((long)Math.Round(v)).ToString();
            return v.ToString("G6", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Парсит строку чисел через запятую или пробел.
        /// Возвращает null если ошибка.
        /// </summary>
        public static List<double>? ParseNumbers(string s)
        {
            s = s.Replace(";", ",").Replace("  ", " ").Trim();
            var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<double>();
            foreach (var p in parts)
            {
                string t = p.Trim().Replace("−", "-").Replace(",", ".");
                if (!double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                    return null;
                result.Add(v);
            }
            return result.Count > 0 ? result : null;
        }

        public static string? ValidateNumbers(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "Ты ничего не ввёл.\nПример: 3, 1, 4, 1, 5, 9, 2, 6";
            if (ParseNumbers(s) == null)
                return "Не удалось разобрать числа.\nВведи через запятую или пробел.\nПример: 3, 1, 4, 1, 5";
            if (ParseNumbers(s)!.Count < 2)
                return "Введи минимум 2 числа.";
            return null;
        }

        /// <summary>Сортирует список и возвращает вариационный ряд.</summary>
        public static List<double> ToVariationRow(List<double> data, bool ascending = true)
        {
            var sorted = data.OrderBy(x => x).ToList();
            return ascending ? sorted : sorted.AsEnumerable().Reverse().ToList();
        }

        /// <summary>Проверяет, является ли список вариационным рядом (упорядочен).</summary>
        public static (bool isVariation, bool ascending) CheckVariation(List<double> data)
        {
            bool asc  = true, desc = true;
            for (int i = 0; i < data.Count - 1; i++)
            {
                if (data[i] > data[i + 1]) asc  = false;
                if (data[i] < data[i + 1]) desc = false;
            }
            return (asc || desc, asc);
        }
    }

    // ─── 28.1  Является ли последовательность вариационным рядом ─

    public class IsVariationRowFunction : FunctionBase
    {
        public override string   Name     => "Ряд упорядочен или нет?";
        public override string   Formula  => "Проверить упорядоченность последовательности";
        public override string[] Keywords => new[] { "вариационный ряд", "упорядоченный", "является" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вариационный ряд\n\n" +
                    "Вариационный ряд — это последовательность, в которой\n" +
                    "каждый следующий элемент не меньше предыдущего\n" +
                    "(упорядочена по неубыванию) или не больше\n" +
                    "(упорядочена по невозрастанию).\n\n" +
                    "Примеры вариационных рядов:\n" +
                    "  1, 3, 5, 7, 9  ✅  (по возрастанию)\n" +
                    "  9, 7, 5, 3, 1  ✅  (по убыванию)\n" +
                    "  1, 1, 2, 2, 3  ✅  (неубывающая)\n\n" +
                    "НЕ вариационные:\n" +
                    "  3, 1, 4, 1, 5  ❌  (не упорядочена)\n\n" +
                    "✏️ Введи последовательность через запятую:\n" +
                    "  Пример: 1, 3, 5, 7  или  9, 7, 5, 3",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data = StatHelper.ParseNumbers(answers[0])!;
            var sb   = new StringBuilder();

            sb.AppendLine("Введённая последовательность:");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            // Проверяем попарно
            bool asc = true, desc = true;
            var violations = new List<string>();

            for (int i = 0; i < data.Count - 1; i++)
            {
                if (data[i] > data[i + 1])
                {
                    asc = false;
                    violations.Add($"  {StatHelper.Fmt(data[i])} > {StatHelper.Fmt(data[i + 1])}  (позиции {i + 1} и {i + 2}) ❌");
                }
                if (data[i] < data[i + 1])
                    desc = false;
            }

            sb.AppendLine("Проверяем порядок соседних элементов:");

            if (asc || desc)
            {
                sb.AppendLine("  Все пары упорядочены ✅");
                sb.AppendLine();
                if (asc && !desc)
                    sb.AppendLine("📌 Да — это ВАРИАЦИОННЫЙ РЯД (по невозрастанию → возрастанию).");
                else if (desc && !asc)
                    sb.AppendLine("📌 Да — это ВАРИАЦИОННЫЙ РЯД (по неубыванию → убыванию).");
                else
                    sb.AppendLine("📌 Да — это ВАРИАЦИОННЫЙ РЯД (все элементы равны).");
            }
            else
            {
                foreach (var v in violations)
                    sb.AppendLine(v);
                sb.AppendLine();
                sb.AppendLine("📌 Нет — это НЕ вариационный ряд.");
                sb.AppendLine("   Последовательность не упорядочена.");

                // Показываем правильный вариационный ряд
                var sorted = StatHelper.ToVariationRow(data);
                sb.AppendLine();
                sb.AppendLine($"💡 Вариационный ряд из этих чисел:");
                sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.2  Составить вариационный ряд ────────────────────────

    public class MakeVariationRowFunction : FunctionBase
    {
        public override string   Name     => "Упорядочить числа по возрастанию";
        public override string   Formula  => "Упорядочить данные по невозрастанию или убыванию";
        public override string[] Keywords => new[] { "вариационный ряд", "составить", "упорядочить", "сортировка" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Составить вариационный ряд\n\n" +
                    "Нужно записать данные в порядке возрастания\n" +
                    "(или убывания) — это и есть вариационный ряд.\n\n" +
                    "Пример:\n" +
                    "  Исходные: 5, 3, 8, 1, 3, 7\n" +
                    "  Вариационный ряд: 1, 3, 3, 5, 7, 8\n\n" +
                    "✏️ Введи исходные данные через запятую:\n" +
                    "  Пример: 5, 3, 8, 1, 3, 7",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            sb.AppendLine("Исходные данные:");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"  Количество вариант: n = {data.Count}");
            sb.AppendLine();

            sb.AppendLine("Шаг 1. Сортируем по возрастанию:");
            sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            // Варианты (уникальные значения)
            var variants = sorted.Distinct().OrderBy(x => x).ToList();
            sb.AppendLine("Шаг 2. Варианты (уникальные значения):");
            sb.AppendLine($"  {string.Join(", ", variants.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            // Наименьшая и наибольшая варианты
            sb.AppendLine($"📌 Вариационный ряд:");
            sb.AppendLine($"   {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();
            sb.AppendLine($"   Наименьшая варианта: {StatHelper.Fmt(sorted.First())}");
            sb.AppendLine($"   Наибольшая варианта: {StatHelper.Fmt(sorted.Last())}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.3  Найти наибольшую и наименьшую варианту ────────────

    public class MinMaxVariantFunction : FunctionBase
    {
        public override string   Name     => "Наибольшее и наименьшее значение";
        public override string   Formula  => "min и max вариационного ряда";
        public override string[] Keywords => new[] { "варианта", "наибольшая", "наименьшая", "вариационный ряд" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Наибольшая и наименьшая варианта\n\n" +
                    "Каждый элемент вариационного ряда называется вариантой.\n" +
                    "Наименьшая варианта — первый элемент ряда.\n" +
                    "Наибольшая варианта — последний элемент ряда.\n\n" +
                    "✏️ Введи последовательность через запятую:\n" +
                    "  Пример: 3, 5, 5, 7, 8, 10",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            sb.AppendLine("Введённые данные:");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            var (isVar, asc) = StatHelper.CheckVariation(data);
            if (!isVar)
            {
                sb.AppendLine("⚠️ Последовательность не является вариационным рядом.");
                sb.AppendLine("Составим вариационный ряд:");
                sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
                sb.AppendLine();
            }

            double min = data.Min();
            double max = data.Max();

            sb.AppendLine($"📌 Наименьшая варианта: {StatHelper.Fmt(min)}");
            sb.AppendLine($"📌 Наибольшая варианта: {StatHelper.Fmt(max)}");
            sb.AppendLine();

            // Повторяющаяся варианта
            var groups = data.GroupBy(x => x).OrderByDescending(g => g.Count()).ToList();
            var maxFreq = groups.First().Count();
            if (maxFreq > 1)
            {
                var mostFreq = groups.Where(g => g.Count() == maxFreq).Select(g => StatHelper.Fmt(g.Key)).ToList();
                sb.AppendLine($"   Чаще всего встречается: {string.Join(", ", mostFreq)}  ({maxFreq} раза)");
            }
            else
            {
                sb.AppendLine("   Каждая варианта встречается по одному разу.");
            }

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.4  Найти варианту, которая повторяется чаще/реже всего

    public class FrequencyVariantFunction : FunctionBase
    {
        public override string   Name     => "Какое значение встречается чаще всего?";
        public override string   Formula  => "Найти моду вариационного ряда";
        public override string[] Keywords => new[] { "варианта", "повторяется", "чаще", "реже", "мода" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Какая варианта повторяется чаще/реже?\n\n" +
                    "Бот подсчитает, сколько раз встречается каждый элемент,\n" +
                    "и найдёт наиболее и наименее частые.\n\n" +
                    "✏️ Введи последовательность через запятую:\n" +
                    "  Пример: 1, 3, 4, 4, 4, 5, 6, 6, 8",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data = StatHelper.ParseNumbers(answers[0])!;
            var sb   = new StringBuilder();

            sb.AppendLine("Введённые данные:");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            // Вариационный ряд
            var sorted = StatHelper.ToVariationRow(data);
            sb.AppendLine("Вариационный ряд:");
            sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            // Таблица частот
            var groups = data.GroupBy(x => x)
                             .Select(g => (value: g.Key, count: g.Count()))
                             .OrderBy(p => p.value)
                             .ToList();

            sb.AppendLine("Частота каждой варианты:");
            foreach (var (value, count) in groups)
                sb.AppendLine($"  {StatHelper.Fmt(value):10} — {count} раз(а)  {new string('█', Math.Min(count, 15))}");
            sb.AppendLine();

            int maxCount = groups.Max(g => g.count);
            int minCount = groups.Min(g => g.count);

            var mostFreq  = groups.Where(g => g.count == maxCount).Select(g => StatHelper.Fmt(g.value)).ToList();
            var leastFreq = groups.Where(g => g.count == minCount).Select(g => StatHelper.Fmt(g.value)).ToList();

            sb.AppendLine($"📌 Чаще всего ({maxCount} раз): {string.Join(", ", mostFreq)}");
            sb.AppendLine($"📌 Реже всего  ({minCount} раз): {string.Join(", ", leastFreq)}");
            sb.AppendLine();
            sb.AppendLine($"   Всего вариант: {data.Count}");
            sb.AppendLine($"   Наименьшая варианта: {StatHelper.Fmt(sorted.First())}");
            sb.AppendLine($"   Наибольшая варианта: {StatHelper.Fmt(sorted.Last())}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.5  Составить вариационный ряд из таблицы ─────────────

    public class VariationRowFromTableFunction : FunctionBase
    {
        public override string   Name     => "Упорядочить данные из таблицы";
        public override string   Formula  => "Упорядочить данные из таблицы";
        public override string[] Keywords => new[] { "вариационный ряд", "таблица", "составить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Вариационный ряд из таблицы\n\n" +
                    "Вводи значения из таблицы через запятую.\n\n" +
                    "Пример: данные о массе животных:\n" +
                    "  Медведь — 60 кг, Волк — 50 кг,\n" +
                    "  Лисица — 5 кг, Заяц — 3.5 кг,\n" +
                    "  Косуля — 25 кг, Лось — 45 кг, Кабан — 36 кг\n\n" +
                    "Ввод: 60, 50, 5, 3.5, 25, 45, 36\n\n" +
                    "✏️ Введи значения через запятую:",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            sb.AppendLine("Исходные данные:");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"  Объём выборки: n = {data.Count}");
            sb.AppendLine();

            sb.AppendLine("Сортируем по возрастанию:");
            sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            sb.AppendLine($"📌 Вариационный ряд:");
            sb.AppendLine($"   {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();
            sb.AppendLine($"   Наименьшая варианта: {StatHelper.Fmt(sorted.First())}");
            sb.AppendLine($"   Наибольшая варианта: {StatHelper.Fmt(sorted.Last())}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.6  Полный анализ вариационного ряда ──────────────────

    public class VariationRowAnalysisFunction : FunctionBase
    {
        public override string   Name     => "Полный анализ ряда данных";
        public override string   Formula  => "Ряд, варианты, наиб/наим значения, частоты";
        public override string[] Keywords => new[] { "анализ", "вариационный ряд", "варианта", "частота", "наибольшее", "наименьшее" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Полный анализ вариационного ряда\n\n" +
                    "Бот:\n" +
                    "  1. Составит вариационный ряд\n" +
                    "  2. Найдёт наибольшую и наименьшую варианты\n" +
                    "  3. Найдёт вариант, повторяющийся чаще/реже всего\n" +
                    "  4. Подсчитает частоту каждой варианты\n\n" +
                    "✏️ Введи данные через запятую:\n" +
                    "  Пример: 1, 3, 4, 4, 5, 5, 5, 6, 8, 8",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            // 1. Вариационный ряд
            sb.AppendLine("1️⃣ Вариационный ряд:");
            sb.AppendLine($"   {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine($"   Объём выборки: n = {data.Count}");
            sb.AppendLine();

            // 2. Наиб/наим
            sb.AppendLine("2️⃣ Наибольшая и наименьшая варианты:");
            sb.AppendLine($"   Наименьшая: {StatHelper.Fmt(sorted.First())}");
            sb.AppendLine($"   Наибольшая: {StatHelper.Fmt(sorted.Last())}");
            sb.AppendLine();

            // 3. Частоты
            var groups = data.GroupBy(x => x)
                             .Select(g => (value: g.Key, count: g.Count()))
                             .OrderBy(p => p.value)
                             .ToList();

            sb.AppendLine("3️⃣ Частота каждой варианты:");
            sb.AppendLine($"  {"Варианта",-12} {"Частота",-10} {"Визуализация"}");
            sb.AppendLine($"  {new string('─', 40)}");
            foreach (var (value, count) in groups)
                sb.AppendLine($"  {StatHelper.Fmt(value),-12} {count,-10} {new string('█', Math.Min(count, 15))}");
            sb.AppendLine();

            // 4. Мода
            int maxCount  = groups.Max(g => g.count);
            int minCount  = groups.Min(g => g.count);
            var mostFreq  = groups.Where(g => g.count == maxCount).Select(g => StatHelper.Fmt(g.value)).ToList();
            var leastFreq = groups.Where(g => g.count == minCount).Select(g => StatHelper.Fmt(g.value)).ToList();

            sb.AppendLine("4️⃣ Варианты по частоте:");
            sb.AppendLine($"   Чаще всего ({maxCount} раз): {string.Join(", ", mostFreq)}");
            sb.AppendLine($"   Реже всего  ({minCount} раз): {string.Join(", ", leastFreq)}");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.7  Понятия: генеральная совокупность и выборка ───────

    public class PopulationAndSampleFunction : FunctionBase
    {
        public override string   Name     => "Что такое выборка и совокупность?";
        public override string   Formula  => "Объяснение понятий с примерами";
        public override string[] Keywords => new[] { "генеральная совокупность", "выборка", "случайная", "понятие" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Генеральная совокупность и случайная выборка\n\n" +
                    "Введи тему исследования (для примера),\n" +
                    "или нажми ◀️ Назад, чтобы вернуться.\n\n" +
                    "Например:\n" +
                    "  рост учеников  или  температура воздуха  или  оценки\n\n" +
                    "✏️ Введи тему (или любое слово):",
                Validate = s => string.IsNullOrWhiteSpace(s) ? "Введи тему" : null
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            string topic = answers[0].Trim();
            var sb = new StringBuilder();

            sb.AppendLine("📚 Основные понятия статистики");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("🌍 ГЕНЕРАЛЬНАЯ СОВОКУПНОСТЬ");
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Множество ВСЕХ объектов, обладающих общей\n" +
                          "изучаемой характеристикой.");
            sb.AppendLine();
            sb.AppendLine($"Пример для темы «{topic}»:");
            sb.AppendLine($"  Все объекты, у которых измеряется «{topic}».");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("🎯 СЛУЧАЙНАЯ ВЫБОРКА");
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Часть генеральной совокупности, отобранная\n" +
                          "произвольным образом (случайно).");
            sb.AppendLine();
            sb.AppendLine("Условие: каждый элемент совокупности должен\n" +
                          "иметь равную вероятность попасть в выборку.");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("📊 ВАРИАЦИОННЫЙ РЯД");
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("Упорядоченная по неубыванию (или невозрастанию)\n" +
                          "последовательность элементов выборки.");
            sb.AppendLine();
            sb.AppendLine("Каждый элемент ряда называется ВАРИАНТОЙ.");
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine("📌 Запись:");
            sb.AppendLine("  D(y) = (−∞; 0) ∪ (0; +∞)  — символически");
            sb.AppendLine("  Генеральная совокупность ⊃ Выборка ⊃ Вариационный ряд");

            return sb.ToString().TrimEnd();
        }
    }

    // ─── 28.8  Заполнить таблицу значений для y = 2x-1, x², 2/x ─

    public class FillTableThreeFunctionsFunction : FunctionBase
    {
        public override string   Name     => "Заполнить таблицу частот";
        public override string   Formula  => "y = 2x−1, y = x², y = 2/x при заданных x";
        public override string[] Keywords => new[] { "таблица", "заполнить", "функции", "значения" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Заполнить таблицу для трёх функций\n\n" +
                    "Бот вычислит значения трёх функций:\n" +
                    "  y = 2x − 1\n" +
                    "  y = x²\n" +
                    "  y = 2/x\n\n" +
                    "при каждом заданном значении x.\n\n" +
                    "✏️ Введи значения x через запятую:\n" +
                    "  (x ≠ 0 для y = 2/x)\n" +
                    "  Пример: -1, 1, 4, 5",
                Validate = s =>
                {
                    var err = StatHelper.ValidateNumbers(s);
                    if (err != null) return err;
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var xs = StatHelper.ParseNumbers(answers[0])!;
            var sb = new StringBuilder();

            sb.AppendLine("Вычисляем значения функций:");
            sb.AppendLine();
            sb.AppendLine($"  {"x",-8} {"y=2x−1",-10} {"y=x²",-10} {"y=2/x",-10}");
            sb.AppendLine($"  {new string('─', 40)}");

            foreach (double x in xs)
            {
                double y1 = 2 * x - 1;
                double y2 = x * x;
                string y3 = Math.Abs(x) < 1e-12 ? "не сущ." : StatHelper.Fmt(2 / x);

                sb.AppendLine($"  {StatHelper.Fmt(x),-8} {StatHelper.Fmt(y1),-10} {StatHelper.Fmt(y2),-10} {y3,-10}");
            }

            sb.AppendLine();
            sb.AppendLine("📌 Таблица заполнена.");
            sb.AppendLine("   Заметка: y = 2/x не существует при x = 0.");

            return sb.ToString().TrimEnd();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // §29. АБСОЛЮТНАЯ И ОТНОСИТЕЛЬНАЯ ЧАСТОТА. ТАБЛИЦА ЧАСТОТ
    // ═══════════════════════════════════════════════════════════════

    // ─── 29.1  Найти абсолютную и относительную частоту ──────────

    public class AbsRelFrequencyFunction : FunctionBase
    {
        public override string   Name     => "Посчитать частоту значения";
        public override string   Formula  => "w = n_i / n  (относительная частота)";
        public override string[] Keywords => new[] { "частота", "абсолютная", "относительная", "варианта" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Абсолютная и относительная частота\n\n" +
                    "Абсолютная частота — сколько раз встречается варианта.\n" +
                    "Относительная частота:\n" +
                    "  w = n_i / n,  где n — общее число наблюдений\n\n" +
                    "Пример: 14 наблюдений, вариант −7° встретилась 2 раза\n" +
                    "  w = 2/14 = 1/7 ≈ 0.14\n\n" +
                    "✏️ Введи данные через запятую:\n" +
                    "  Пример: 20, 20, 30, 10, 20, 30, 20, 30, 20",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data = StatHelper.ParseNumbers(answers[0])!;
            int n    = data.Count;
            var sb   = new StringBuilder();

            sb.AppendLine($"Данные: {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"Общее число наблюдений: n = {n}");
            sb.AppendLine();

            var groups = data.GroupBy(x => x)
                             .Select(g => (value: g.Key, abs: g.Count()))
                             .OrderBy(p => p.value)
                             .ToList();

            sb.AppendLine("Таблица частот:");
            sb.AppendLine($"  {"Варианта",-12} {"Абс. n_i",-14} {"Отн. w_i",-14} {"В %"}");
            sb.AppendLine($"  {new string('─', 52)}");

            double totalRel = 0;
            foreach (var (value, abs) in groups)
            {
                double rel = (double)abs / n;
                totalRel  += rel;
                string frac = FormatFraction(abs, n);
                sb.AppendLine($"  {StatHelper.Fmt(value),-12} {abs,-14} {frac,-14} {rel * 100:F1}%");
            }

            sb.AppendLine($"  {new string('─', 52)}");
            sb.AppendLine($"  {"Σ",-12} {n,-14} {"≈1.00"}");
            sb.AppendLine();
            sb.AppendLine("📌 Свойства таблицы частот:");
            sb.AppendLine($"   Сумма абсолютных частот = {n}  (= n)");
            sb.AppendLine($"   Сумма относительных частот ≈ 1");

            return sb.ToString().TrimEnd();
        }

        private static string FormatFraction(int num, int den)
        {
            int g = Gcd(num, den);
            int n = num / g, d = den / g;
            double v = (double)num / den;
            string frac = d == 1 ? n.ToString() : $"{n}/{d}";
            return $"{frac} ≈ {v:F2}";
        }

        private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
    }

    // ─── 29.2  Составить таблицу частот ──────────────────────────

    public class FrequencyTableFunction : FunctionBase
    {
        public override string   Name     => "Составить таблицу частот";
        public override string   Formula  => "Варианты → абсолютные и относительные частоты";
        public override string[] Keywords => new[] { "таблица частот", "абсолютная", "относительная", "составить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Составить таблицу частот\n\n" +
                    "По данным бот построит таблицу:\n" +
                    "  · варианты (уникальные значения)\n" +
                    "  · абсолютная частота n_i\n" +
                    "  · относительная частота w_i = n_i / n\n\n" +
                    "✏️ Введи данные через запятую или пробел:\n" +
                    "  Пример: 2, 2, 3, 3, 3, 4, 2, 3, 5, 3",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            int n      = data.Count;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            sb.AppendLine($"Исходный ряд: {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"Вариационный ряд: {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine($"Общее число наблюдений: n = {n}");
            sb.AppendLine();

            var groups = data.GroupBy(x => x)
                             .Select(g => (value: g.Key, abs: g.Count()))
                             .OrderBy(p => p.value)
                             .ToList();

            var varRow  = "Варианты:   " + string.Join("  ", groups.Select(g => $"{StatHelper.Fmt(g.value),5}"));
            var absRow  = "Абс. n_i:   " + string.Join("  ", groups.Select(g => $"{g.abs,5}"));
            var relStrs = groups.Select(g => {
                int gcd = Gcd(g.abs, n);
                int num = g.abs / gcd, den = n / gcd;
                return den == 1 ? num.ToString() : $"{num}/{den}";
            }).ToList();
            var relRow  = "Отн. w_i:   " + string.Join("  ", relStrs.Select(s => $"{s,5}"));
            var pctRow  = "В %:        " + string.Join("  ", groups.Select(g =>
                $"{(double)g.abs / n * 100,4:F0}%"));

            sb.AppendLine("📊 Таблица частот:");
            sb.AppendLine();
            sb.AppendLine(varRow);
            sb.AppendLine(absRow);
            sb.AppendLine(relRow);
            sb.AppendLine(pctRow);
            sb.AppendLine();

            double sumRel = groups.Sum(g => (double)g.abs / n);
            sb.AppendLine($"📌 Сумма абсолютных частот = {n}  (= n)");
            sb.AppendLine($"   Сумма относительных частот = {sumRel:F4} ≈ 1");
            sb.AppendLine();

            int maxAbs  = groups.Max(g => g.abs);
            var popular = groups.Where(g => g.abs == maxAbs).Select(g => StatHelper.Fmt(g.value));
            sb.AppendLine($"   Наиболее популярная варианта: {string.Join(", ", popular)}  (n_i = {maxAbs})");

            return sb.ToString().TrimEnd();
        }

        private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
    }

    // ─── 29.3  Частота конкретной варианты ───────────────────────

    public class SpecificVariantFrequencyFunction : FunctionBase
    {
        public override string   Name     => "Найти частоту одного значения";
        public override string   Formula  => "Найти абс. и отн. частоту заданного значения";
        public override string[] Keywords => new[] { "частота", "варианта", "конкретная", "найти" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти частоту конкретной варианты\n\n" +
                    "✏️ Введи все данные через запятую:\n" +
                    "  Пример: 2, 3, 3, 4, 2, 3, 5, 3, 4, 3",
                Validate = StatHelper.ValidateNumbers
            },
            new InputStep
            {
                Question =
                    "✏️ Введи значение варианты, частоту которой нужно найти:\n" +
                    "  Пример: 3",
                Validate = s =>
                {
                    s = s.Trim().Replace(",", ".").Replace("−", "-");
                    if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return "Введи число.";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            double val = double.Parse(answers[1].Trim().Replace(",", ".").Replace("−", "-"),
                            NumberStyles.Any, CultureInfo.InvariantCulture);
            int n      = data.Count;
            int abs    = data.Count(x => Math.Abs(x - val) < 1e-9);
            var sb     = new StringBuilder();

            sb.AppendLine($"Данные: {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"Всего наблюдений: n = {n}");
            sb.AppendLine($"Ищем варианту: {StatHelper.Fmt(val)}");
            sb.AppendLine();

            if (abs == 0)
            {
                sb.AppendLine($"Варианта {StatHelper.Fmt(val)} в данных не встречается.");
                sb.AppendLine($"📌 Абсолютная частота = 0");
                sb.AppendLine($"   Относительная частота = 0");
            }
            else
            {
                double rel = (double)abs / n;
                int g = Gcd(abs, n);
                string frac = (n / g == 1) ? (abs / g).ToString() : $"{abs / g}/{n / g}";
                sb.AppendLine($"Шаг 1. Считаем сколько раз встречается {StatHelper.Fmt(val)}:");
                sb.AppendLine($"  n_i = {abs}");
                sb.AppendLine();
                sb.AppendLine($"Шаг 2. Вычисляем относительную частоту:");
                sb.AppendLine($"  w = n_i / n = {abs} / {n} = {frac} ≈ {rel:F4}");
                sb.AppendLine();
                sb.AppendLine($"📌 Абсолютная частота = {abs}");
                sb.AppendLine($"   Относительная частота = {frac} ≈ {rel:F2}  ({rel * 100:F1}%)");
            }

            return sb.ToString().TrimEnd();
        }

        private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
    }

    // ─── 29.4  Статистический ряд → таблица частот ───────────────

    public class StatRowToFreqTableFunction : FunctionBase
    {
        public override string   Name     => "Из ряда данных → таблица частот";
        public override string   Formula  => "Вариационный ряд + абс./отн. частоты по варианте";
        public override string[] Keywords => new[] { "статистический ряд", "вариационный ряд", "таблица", "частота" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Из статистического ряда — вариационный ряд и таблица частот\n\n" +
                    "Введи статистический ряд (данные в порядке наблюдений).\n\n" +
                    "Пример: 2 2 3 3 3 3 4 2 3 3 3 2 3 4 3 3 2 3 5 3\n\n" +
                    "✏️ Введи данные через запятую или пробел:",
                Validate = StatHelper.ValidateNumbers
            },
            new InputStep
            {
                Question =
                    "✏️ Для каких вариант найти частоту?\n" +
                    "  Введи через запятую или слово «все»:\n" +
                    "  Пример: 3, 4  или  все",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Введи значения или слово «все»";
                    if (s.Trim().ToLower() == "все") return null;
                    if (StatHelper.ParseNumbers(s) == null)
                        return "Введи числа через запятую или слово «все»";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            int n      = data.Count;
            var sorted = StatHelper.ToVariationRow(data);
            var sb     = new StringBuilder();

            sb.AppendLine($"Статистический ряд ({n} наблюдений):");
            sb.AppendLine($"  {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine();
            sb.AppendLine("Вариационный ряд:");
            sb.AppendLine($"  {string.Join(", ", sorted.Select(StatHelper.Fmt))}");
            sb.AppendLine();

            var groups = data.GroupBy(x => x)
                             .Select(g => (value: g.Key, abs: g.Count()))
                             .OrderBy(p => p.value)
                             .ToList();

            sb.AppendLine("📊 Полная таблица частот:");
            sb.AppendLine($"  {"Варианта",-12} {"n_i",-8} {"w_i",-12} {"%"}");
            sb.AppendLine($"  {new string('─', 40)}");
            foreach (var (value, abs) in groups)
            {
                double rel = (double)abs / n;
                int g = Gcd(abs, n);
                string frac = (n / g == 1) ? (abs / g).ToString() : $"{abs / g}/{n / g}";
                sb.AppendLine($"  {StatHelper.Fmt(value),-12} {abs,-8} {frac,-12} {rel * 100:F1}%");
            }
            sb.AppendLine($"  {new string('─', 40)}");
            sb.AppendLine($"  {"Σ",-12} {n,-8} {"≈1.00",-12} 100%");
            sb.AppendLine();

            string query = answers[1].Trim().ToLower();
            List<double> targets = query == "все"
                ? groups.Select(g => g.value).ToList()
                : StatHelper.ParseNumbers(answers[1])!;

            sb.AppendLine("📌 Запрошенные варианты:");
            foreach (double t in targets)
            {
                var match = groups.FirstOrDefault(g => Math.Abs(g.value - t) < 1e-9);
                if (match == default)
                {
                    sb.AppendLine($"   {StatHelper.Fmt(t)}: не встречается (n_i = 0, w = 0)");
                    continue;
                }
                int abs    = match.abs;
                double rel = (double)abs / n;
                int g      = Gcd(abs, n);
                string frac = (n / g == 1) ? (abs / g).ToString() : $"{abs / g}/{n / g}";
                sb.AppendLine($"   {StatHelper.Fmt(t)}: n_i = {abs},  w = {frac} ≈ {rel:F2}  ({rel * 100:F1}%)");
            }

            return sb.ToString().TrimEnd();
        }

        private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
    }

    // ─── 29.5  Найти w по таблице с n_i ──────────────────────────

    public class FindRelFreqFromTableFunction : FunctionBase
    {
        public override string   Name     => "Найти относительную частоту";
        public override string   Formula  => "w_i = n_i / n,  n = Σ n_i";
        public override string[] Keywords => new[] { "относительная частота", "таблица", "найти w", "дополнить" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Найти относительную частоту по таблице\n\n" +
                    "Дана таблица: варианты и их абсолютные частоты n_i.\n" +
                    "Нужно найти n и w_i для каждой варианты.\n\n" +
                    "✏️ Введи абсолютные частоты n_i через запятую:\n" +
                    "  (строка «Абсолютная частота» из таблицы)\n" +
                    "  Пример: 4, 4, 5, 6, 4, 4, 2, 1",
                Validate = s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return "Введи числа";
                    var nums = StatHelper.ParseNumbers(s);
                    if (nums == null) return "Введи числа через запятую";
                    if (nums.Any(x => x < 0)) return "Частоты не могут быть отрицательными";
                    return null;
                }
            },
            new InputStep
            {
                Question =
                    "✏️ Введи варианты через запятую:\n" +
                    "  (строка «Варианты» из таблицы)\n" +
                    "  Пример: 20, 21, 22, 23, 24, 25, 26, 27",
                Validate = StatHelper.ValidateNumbers
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var freqs    = StatHelper.ParseNumbers(answers[0])!;
            var variants = StatHelper.ParseNumbers(answers[1])!;
            var sb       = new StringBuilder();

            if (freqs.Count != variants.Count)
                return $"⚠️ Количество вариант ({variants.Count}) не совпадает с количеством частот ({freqs.Count}).";

            int n = (int)freqs.Sum();
            sb.AppendLine($"Общее число наблюдений: n = Σ n_i = {n}");
            sb.AppendLine();
            sb.AppendLine("Вычисляем w_i = n_i / n:");
            sb.AppendLine();
            sb.AppendLine($"  {"Варианта",-12} {"n_i",-8} {"w_i",-16} {"До 0.01"}");
            sb.AppendLine($"  {new string('─', 48)}");

            double sumRel = 0;
            for (int i = 0; i < variants.Count; i++)
            {
                int abs    = (int)freqs[i];
                double rel = (double)abs / n;
                sumRel    += rel;
                int g      = Gcd(abs, n);
                string frac = (n / g == 1) ? (abs / g).ToString() : $"{abs/g}/{n/g}";
                sb.AppendLine($"  {StatHelper.Fmt(variants[i]),-12} {abs,-8} {frac,-16} {rel:F2}");
            }

            sb.AppendLine($"  {new string('─', 48)}");
            sb.AppendLine($"  {"Σ",-12} {n,-8} {"≈1",-16} {sumRel:F2}");
            sb.AppendLine();

            int maxAbs  = (int)freqs.Max();
            var popular = variants.Zip(freqs, (v, f) => (v, f))
                                  .Where(p => (int)p.f == maxAbs)
                                  .Select(p => StatHelper.Fmt(p.v));
            sb.AppendLine($"📌 Самая популярная варианта: {string.Join(", ", popular)}  (n_i = {maxAbs})");
            sb.AppendLine($"   Проверка: Σ w_i = {sumRel:F4} ≈ 1  ✅");

            return sb.ToString().TrimEnd();
        }

        private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
    }

    // ─── 29.6  Абс. частота конкретного результата ───────────────

    public class HomeworkFrequencyFunction : FunctionBase
    {
        public override string   Name     => "Частота за несколько наблюдений";
        public override string   Formula  => "n_i — число результатов с заданным значением";
        public override string[] Keywords => new[] { "абсолютная частота", "четверть", "оценка", "задание" };
        public override string[] Parameters => [];
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Абсолютная частота конкретного результата\n\n" +
                    "Найдём, сколько раз в серии наблюдений получено\n" +
                    "определённое значение.\n\n" +
                    "✏️ Введи все результаты через запятую:\n" +
                    "  Пример: 5, 4, 5, 5, 4, 5, 5, 4",
                Validate = StatHelper.ValidateNumbers
            },
            new InputStep
            {
                Question =
                    "✏️ Какое значение ищем?\n" +
                    "  Пример: 5",
                Validate = s =>
                {
                    s = s.Trim().Replace(",", ".");
                    if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        return "Введи число";
                    return null;
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var data   = StatHelper.ParseNumbers(answers[0])!;
            double val = double.Parse(answers[1].Trim().Replace(",", "."),
                            NumberStyles.Any, CultureInfo.InvariantCulture);
            int n      = data.Count;
            int absEq  = data.Count(x => Math.Abs(x - val) < 1e-9);
            int absNeq = n - absEq;
            var sb     = new StringBuilder();

            sb.AppendLine($"Результаты: {string.Join(", ", data.Select(StatHelper.Fmt))}");
            sb.AppendLine($"Всего наблюдений: n = {n}");
            sb.AppendLine($"Ищем значение: {StatHelper.Fmt(val)}");
            sb.AppendLine();

            double rel = (double)absEq / n;
            sb.AppendLine($"📌 Абсолютная частота {StatHelper.Fmt(val)}: {absEq}");
            sb.AppendLine($"   Относительная частота: {absEq}/{n} ≈ {rel:F2}  ({rel * 100:F1}%)");
            sb.AppendLine();
            sb.AppendLine($"   Значение, отличное от {StatHelper.Fmt(val)}: {absNeq} раз");

            return sb.ToString().TrimEnd();
        }
    }
}

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
        public override string   Name     => "Является ли вариационным рядом";
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
        public override string   Name     => "Составить вариационный ряд";
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
        public override string   Name     => "Наибольшая и наименьшая варианта";
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
        public override string   Name     => "Варианта, повторяющаяся чаще/реже всего";
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
        public override string   Name     => "Вариационный ряд из таблицы данных";
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
        public override string   Name     => "Полный анализ вариационного ряда";
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
        public override string   Name     => "Генеральная совокупность и выборка";
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
        public override string   Name     => "Заполнить таблицу для трёх функций";
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
}

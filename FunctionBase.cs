using System;
using System.Collections.Generic;
using System.Linq;

namespace MathPocket
{
    /// <summary>
    /// Базовый класс для всех математических функций бота.
    /// Поддерживает два режима ввода:
    ///   • Пошаговый — если <see cref="Steps"/> не null.
    ///   • Однострочный — иначе (через <see cref="CalculateFromText"/>).
    /// </summary>
    public abstract partial class FunctionBase
    {
        /// <summary>Отображаемое название функции.</summary>
        public virtual string Name { get; } = string.Empty;

        /// <summary>Формула функции (для показа пользователю).</summary>
        public virtual string Formula => string.Empty;

        /// <summary>Параметры для однострочного режима.</summary>
        public virtual string[] Parameters => Array.Empty<string>();

        /// <summary>Ключевые слова для поиска.</summary>
        public virtual string[] Keywords => Array.Empty<string>();

        /// <summary>
        /// Вычислить результат по массиву чисел (однострочный режим).
        /// </summary>
        public abstract double Calculate(double[] inputs);

        /// <summary>
        /// Разобрать строку пользователя, вычислить и вернуть ответ (однострочный режим).
        /// </summary>
        public virtual string CalculateFromText(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != Parameters.Length)
                return $"Ожидалось {Parameters.Length} данных, получено {parts.Length}.";

            double[] numbers;
            try
            {
                numbers = parts
                    .Select(p => double.Parse(p.Replace(',', '.'),
                        System.Globalization.CultureInfo.InvariantCulture))
                    .ToArray();
            }
            catch (FormatException)
            {
                return "Ошибка: введите числа через пробел (например: 3 4.5).";
            }

            return Calculate(numbers).ToString();
        }

        // ─── Пошаговый ввод (опциональный) ───────────────────────

        /// <summary>
        /// Шаги пошагового ввода. Если <c>null</c> — используется <see cref="CalculateFromText"/>.
        /// </summary>
        public virtual InputStep[]? Steps => null;

        /// <summary>
        /// Вычислить результат по накопленным ответам пошагового ввода.
        /// Переопределяйте совместно со <see cref="Steps"/>.
        /// </summary>
        public virtual string CalculateFromAnswers(List<string> answers) =>
            throw new NotImplementedException(
                $"{GetType().Name} не реализует CalculateFromAnswers.");

        /// <summary>
        /// Реальное количество активных шагов (может зависеть от предыдущих ответов).
        /// По умолчанию — <c>Steps.Length</c>.
        /// </summary>
        public virtual int ActiveStepCount(List<string> answers) =>
            Steps?.Length ?? 0;

        /// <summary>
        /// Превью текущего состояния — показывается перед следующим вопросом.
        /// Возвращайте <c>null</c>, если превью не нужно.
        /// </summary>
        public virtual string? GetPreview(List<string> answers) => null;

        /// <summary>
        /// Если функция возвращает график — переопределите этот метод.
        /// Возвращает байты PNG, или <c>null</c> если результат текстовый.
        /// </summary>
        public virtual byte[]? GetPlotBytes(List<string> answers) => null;
    }
}

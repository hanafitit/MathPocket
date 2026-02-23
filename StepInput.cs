using System;
using System.Collections.Generic;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Пошаговый ввод данных
    //
    //  Как работает:
    //    1. Функция объявляет список шагов (Steps) вместо Parameters.
    //    2. BotHandler задаёт вопрос из текущего шага.
    //    3. Ответ пользователя валидируется и сохраняется в Answers.
    //    4. Когда все шаги пройдены — вызывается CalculateFromAnswers.
    //
    //  Для обычных функций (без пошагового ввода) Steps = null,
    //  бот ведёт себя по-старому.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Один шаг диалога ввода.</summary>
    public class InputStep
    {
        /// <summary>Вопрос, который бот задаёт пользователю.</summary>
        public string Question { get; init; } = string.Empty;

        /// <summary>
        /// Валидация ответа. Возвращает null если ок,
        /// или строку с объяснением ошибки если нет.
        /// </summary>
        public Func<string, string?> Validate { get; init; } = _ => null;
    }

    /// <summary>
    /// Хранит текущий прогресс пошагового ввода одного пользователя.
    /// </summary>
    public class StepInputSession
    {
        public int            CurrentStep { get; set; } = 0;
        public List<string>   Answers     { get; }      = new();
    }

    // ─────────────────────────────────────────────────────────────
    //  Расширение FunctionBase — добавляем пошаговый ввод
    // ─────────────────────────────────────────────────────────────

    public abstract partial class FunctionBase
    {
        /// <summary>
        /// Шаги пошагового ввода. Если null — используется старый
        /// однострочный режим (CalculateFromText).
        /// </summary>
        public virtual InputStep[]? Steps => null;

        /// <summary>
        /// Вычислить результат по накопленным ответам пошагового ввода.
        /// Переопределяйте вместе со Steps.
        /// </summary>
        public virtual string CalculateFromAnswers(List<string> answers) =>
            throw new NotImplementedException(
                $"{GetType().Name} не реализует CalculateFromAnswers.");
    }
}

using System;
using System.Collections.Generic;

namespace MathPocket
{
    // ─── Один шаг диалога ─────────────────────────────────────────

    /// <summary>Один вопрос пошагового ввода.</summary>
    public sealed record InputStep
    {
        /// <summary>Текст вопроса, который бот отправляет пользователю.</summary>
        public required string Question { get; init; }

        /// <summary>
        /// Валидация ответа.
        /// Возвращает <c>null</c>, если ответ корректен,
        /// или строку с объяснением ошибки — иначе.
        /// </summary>
        public Func<string, string?> Validate { get; init; } = _ => null;
    }

    // ─── Сессия пошагового ввода ──────────────────────────────────

    /// <summary>
    /// Хранит прогресс пошагового ввода одного пользователя:
    /// текущий логический шаг и накопленные ответы.
    /// </summary>
    public sealed class StepInputSession
    {
        /// <summary>Индекс текущего логического шага (0-based).</summary>
        public int CurrentStep { get; set; } = 0;

        /// <summary>Ответы пользователя, собранные на каждом пройденном шаге.</summary>
        public List<string> Answers { get; } = [];
    }
}

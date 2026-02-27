using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Модели каталога
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Группа функций (используется как прямой список или как подраздел категории).
    /// </summary>
    public sealed class MathSection
    {
        public string         Name      { get; init; } = string.Empty;
        public FunctionBase[] Functions { get; init; } = [];
    }

    /// <summary>
    /// Категория верхнего уровня.
    /// Содержит либо <see cref="SubSections"/> (вложенное меню),
    /// либо <see cref="Functions"/> напрямую — но не оба сразу.
    /// </summary>
    public sealed class MathCategory
    {
        public string       Name        { get; init; } = string.Empty;
        public MathSection[] SubSections { get; init; } = [];
        public FunctionBase[] Functions  { get; init; } = [];

        public bool HasSubSections => SubSections.Length > 0;
    }

    // ═══════════════════════════════════════════════════════════════
    //  BotHandler
    // ═══════════════════════════════════════════════════════════════

    internal sealed class BotHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly MathCategory[]     _categories;
        private readonly Material[]         _materials;

        // ── Состояние пользователей (per-chat) ────────────────────
        private readonly ConcurrentDictionary<long, string>           _userState       = new();
        private readonly ConcurrentDictionary<long, MathCategory>     _selectedCategory = new();
        private readonly ConcurrentDictionary<long, MathSection>      _selectedSection  = new();
        private readonly ConcurrentDictionary<long, FunctionBase>     _selectedFunction = new();
        private readonly ConcurrentDictionary<long, StepInputSession> _inputSession     = new();

        // ── Диспетчер состояний ───────────────────────────────────
        private readonly Dictionary<string, Func<Message, Task>> _stateHandlers;

        // ── Команды верхнего уровня ───────────────────────────────
        private const string BtnSections   = "📂 Разделы";
        private const string BtnCalculator = "🧮 Калькулятор";
        private const string BtnBack       = "◀️ Назад";

        public BotHandler(ITelegramBotClient bot)
        {
            _bot        = bot;
            _categories = FunctionCatalog.All;
            _materials  =
            [
                new Material
                {
                    Name     = "Пример",
                    Keywords = ["пример"],
                    Content  = "Это пример материала."
                }
            ];

            _stateHandlers = new Dictionary<string, Func<Message, Task>>
            {
                ["choose_category"]   = HandleChooseCategory,
                ["choose_subsection"] = HandleChooseSubSection,
                ["choose_function"]   = HandleChooseFunction,
                ["input_data"]        = HandleInputData,
                ["universal_calc"]    = HandleUniversalCalculator,
            };
        }

        // ═══════════════════════════════════════════════════════════
        //  Входная точка
        // ═══════════════════════════════════════════════════════════

        public async Task HandleUpdateAsync(
            ITelegramBotClient _,
            Update update,
            CancellationToken ct)
        {
            if (update.Message is { } msg)
                await OnMessage(msg);
        }

        public Task HandleErrorAsync(
            ITelegramBotClient _,
            Exception exception,
            Telegram.Bot.Polling.HandleErrorSource source,
            CancellationToken ct)
        {
            Console.WriteLine($"[ОШИБКА] {exception.Message} (источник: {source})");
            return Task.CompletedTask;
        }

        // ═══════════════════════════════════════════════════════════
        //  Логирование
        // ═══════════════════════════════════════════════════════════

        private static readonly object _logLock = new();

        private static string LogFilePath()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"steps_{DateTime.Now:yyyy-MM-dd}.log");
        }

        private static void WriteLog(string line)
        {
            Console.WriteLine(line);
            lock (_logLock)
            {
                try { File.AppendAllText(LogFilePath(), line + Environment.NewLine, Encoding.UTF8); }
                catch { /* игнорируем ошибки файловой системы */ }
            }
        }

        private static void LogStep(
            string tag, long chatId, FunctionBase func, StepInputSession session,
            string? userAnswer = null, string? extra = null)
        {
            var sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:HH:mm:ss}] [{tag}] user={chatId} | func=\"{func.Name}\"");

            int stepNum = session.CurrentStep + 1;
            int total   = func.Steps?.Length ?? 0;

            sb.Append(userAnswer is not null
                ? $" | шаг {stepNum}/{total} | ответ=\"{userAnswer}\""
                : $" | шаг {stepNum}/{total}");

            if (session.Answers.Count > 0)
            {
                var formatted = string.Join(", ",
                    session.Answers.Select((a, i) => $"[{i + 1}]=\"{a}\""));
                sb.Append($" | накоплено: [{formatted}]");
            }

            if (extra is not null)
                sb.Append($" | {extra}");

            WriteLog(sb.ToString());
        }

        private static void LogSessionEnd(
            string tag, long chatId, FunctionBase func, List<string> answers,
            string? result = null, string? error = null)
        {
            var sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:HH:mm:ss}] [{tag}] user={chatId} | func=\"{func.Name}\" | ЗАВЕРШЕНО");

            var formatted = string.Join(", ", answers.Select((a, i) => $"[{i + 1}]=\"{a}\""));
            sb.Append($" | ответы: [{formatted}]");

            if (result is not null) sb.Append($" | результат=\"{result}\"");
            if (error  is not null) sb.Append($" | ОШИБКА: {error}");

            WriteLog(sb.ToString());
        }

        // ═══════════════════════════════════════════════════════════
        //  Роутинг сообщений
        // ═══════════════════════════════════════════════════════════

        private async Task OnMessage(Message msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Text))
            {
                await _bot.SendMessage(msg.Chat.Id, "Введите текст.");
                return;
            }

            var text   = msg.Text.Trim();
            var chatId = msg.Chat.Id;

            WriteLog($"[{DateTime.Now:HH:mm:ss}] [СООБЩЕНИЕ] user={chatId} (@{msg.Chat.Username ?? "no_username"}) | текст=\"{text}\"");

            // ── Глобальные команды ────────────────────────────────
            if (text.StartsWith("/start"))
            {
                ClearState(chatId);
                await SendStartMessage(chatId);
                return;
            }

            if (text == BtnBack)
            {
                await HandleBack(chatId);
                return;
            }

            if (text == BtnSections)
            {
                ClearState(chatId);
                _userState[chatId] = "choose_category";
                await SendCategoryMenu(chatId);
                return;
            }

            if (text == BtnCalculator)
            {
                ClearState(chatId);
                _userState[chatId] = "universal_calc";
                await _bot.SendMessage(chatId,
                    "Введите выражение для вычисления:\n(например: 2 + 3 * (4 - 1))",
                    replyMarkup: BackKeyboard());
                return;
            }

            // ── Обработка по текущему состоянию ──────────────────
            if (_userState.TryGetValue(chatId, out var state) &&
                _stateHandlers.TryGetValue(state, out var handler))
            {
                await handler(msg);
                return;
            }

            await _bot.SendMessage(chatId, "Я вас не понял. Нажмите /start для перезапуска.");
        }

        // ═══════════════════════════════════════════════════════════
        //  Кнопка «Назад»
        //
        //  Дерево навигации:
        //    Главное меню
        //      └─ choose_category
        //           └─ choose_subsection (если есть подразделы)
        //                └─ choose_function
        //                     └─ input_data
        // ═══════════════════════════════════════════════════════════

        private async Task HandleBack(long chatId)
        {
            _userState.TryGetValue(chatId, out var state);

            switch (state)
            {
                case "input_data":
                    await HandleBackFromInput(chatId);
                    break;

                case "choose_function":
                    HandleBackFromFunction(chatId);
                    if (_selectedCategory.TryGetValue(chatId, out var cat) && cat.HasSubSections)
                        await SendSubSectionMenu(chatId, cat);
                    else
                        await SendCategoryMenu(chatId);
                    break;

                case "choose_subsection":
                    _selectedSection.TryRemove(chatId, out _);
                    _selectedCategory.TryRemove(chatId, out _);
                    _userState[chatId] = "choose_category";
                    await SendCategoryMenu(chatId);
                    break;

                default:
                    ClearState(chatId);
                    await SendStartMessage(chatId);
                    break;
            }
        }

        private async Task HandleBackFromInput(long chatId)
        {
            // Шаг назад внутри пошагового диалога
            if (_inputSession.TryGetValue(chatId, out var session) &&
                session.CurrentStep > 0 &&
                _selectedFunction.TryGetValue(chatId, out var func) &&
                func.Steps is not null)
            {
                session.CurrentStep--;
                if (session.Answers.Count > 0)
                    session.Answers.RemoveAt(session.Answers.Count - 1);

                WriteLog($"[{DateTime.Now:HH:mm:ss}] [ОТКАТ] user={chatId} | func=\"{func.Name}\" | возврат к шагу {session.CurrentStep + 1} | накоплено={session.Answers.Count}");
                await AskCurrentStep(chatId, func, session);
                return;
            }

            // Выход к списку функций
            if (_inputSession.TryGetValue(chatId, out var aborted) &&
                _selectedFunction.TryGetValue(chatId, out var abortedFunc))
            {
                WriteLog($"[{DateTime.Now:HH:mm:ss}] [ОТМЕНА] user={chatId} | func=\"{abortedFunc.Name}\" | прервано на шаге {aborted.CurrentStep + 1}");
            }

            _inputSession.TryRemove(chatId, out _);
            _selectedFunction.TryRemove(chatId, out _);
            _userState[chatId] = "choose_function";

            if (_selectedSection.TryGetValue(chatId, out var sec))
                await SendFunctionMenu(chatId, sec);
            else
            {
                ClearState(chatId);
                await SendStartMessage(chatId);
            }
        }

        private void HandleBackFromFunction(long chatId)
        {
            _selectedFunction.TryRemove(chatId, out _);
            _selectedSection.TryRemove(chatId, out _);

            if (_selectedCategory.TryGetValue(chatId, out var cat) && cat.HasSubSections)
                _userState[chatId] = "choose_subsection";
            else
            {
                _selectedCategory.TryRemove(chatId, out _);
                _userState[chatId] = "choose_category";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Обработчики состояний
        // ═══════════════════════════════════════════════════════════

        private async Task HandleChooseCategory(Message msg)
        {
            var category = _categories.FirstOrDefault(c =>
                c.Name.Equals(msg.Text, StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                await _bot.SendMessage(msg.Chat.Id, "Выберите раздел из предложенных кнопок.");
                return;
            }

            _selectedCategory[msg.Chat.Id] = category;

            if (category.HasSubSections)
            {
                _userState[msg.Chat.Id] = "choose_subsection";
                await SendSubSectionMenu(msg.Chat.Id, category);
                return;
            }

            if (category.Functions.Length == 0)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"📭 Раздел «{category.Name}» пока не содержит функций.\n" +
                    "Выберите другой раздел или нажмите «◀️ Назад».");
                return;
            }

            // Напрямую к функциям — оборачиваем в временный MathSection
            var section = new MathSection { Name = category.Name, Functions = category.Functions };
            _selectedSection[msg.Chat.Id] = section;
            _userState[msg.Chat.Id]       = "choose_function";
            await SendFunctionMenu(msg.Chat.Id, section);
        }

        private async Task HandleChooseSubSection(Message msg)
        {
            if (!_selectedCategory.TryGetValue(msg.Chat.Id, out var category))
            {
                await _bot.SendMessage(msg.Chat.Id, "Произошла ошибка. Попробуйте /start");
                return;
            }

            var section = category.SubSections.FirstOrDefault(s =>
                s.Name.Equals(msg.Text, StringComparison.OrdinalIgnoreCase));

            if (section is null)
            {
                await _bot.SendMessage(msg.Chat.Id, "Выберите подраздел из предложенных кнопок.");
                return;
            }

            if (section.Functions.Length == 0)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"📭 Подраздел «{section.Name}» пока не содержит функций.\n" +
                    "Выберите другой или нажмите «◀️ Назад».");
                return;
            }

            _selectedSection[msg.Chat.Id] = section;
            _userState[msg.Chat.Id]       = "choose_function";
            await SendFunctionMenu(msg.Chat.Id, section);
        }

        private async Task HandleChooseFunction(Message msg)
        {
            if (!_selectedSection.TryGetValue(msg.Chat.Id, out var section))
            {
                await _bot.SendMessage(msg.Chat.Id, "Произошла ошибка. Попробуйте /start");
                return;
            }

            var func = section.Functions.FirstOrDefault(f =>
                f.Name.Equals(msg.Text, StringComparison.OrdinalIgnoreCase));

            if (func is null)
            {
                await _bot.SendMessage(msg.Chat.Id, "Выберите функцию из предложенных кнопок.");
                return;
            }

            _selectedFunction[msg.Chat.Id] = func;
            _userState[msg.Chat.Id]        = "input_data";

            if (func.Steps is not null)
            {
                // ── Пошаговый ввод ────────────────────────────────
                var session = new StepInputSession();
                _inputSession[msg.Chat.Id] = session;

                WriteLog($"[{DateTime.Now:HH:mm:ss}] [СТАРТ] user={msg.Chat.Id} | func=\"{func.Name}\" | шагов={func.Steps.Length}");

                await _bot.SendMessage(msg.Chat.Id,
                    $"✅ {func.Name}\nФормула: {func.Formula}",
                    replyMarkup: BackKeyboard());

                await AskCurrentStep(msg.Chat.Id, func, session);
            }
            else
            {
                // ── Однострочный режим ────────────────────────────
                await _bot.SendMessage(msg.Chat.Id,
                    $"✅ {func.Name}\n" +
                    $"Формула: {func.Formula}\n\n" +
                    $"Введите через пробел: {string.Join(", ", func.Parameters)}",
                    replyMarkup: BackKeyboard());
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Обработчик ввода данных
        // ═══════════════════════════════════════════════════════════

        private async Task HandleInputData(Message msg)
        {
            if (!_selectedFunction.TryGetValue(msg.Chat.Id, out var func))
            {
                await _bot.SendMessage(msg.Chat.Id, "Произошла ошибка. Попробуйте /start");
                return;
            }

            if (func.Steps is not null &&
                _inputSession.TryGetValue(msg.Chat.Id, out var session))
            {
                await HandleStepInput(msg, func, session);
                return;
            }

            await HandleSingleLineInput(msg, func);
        }

        private async Task HandleStepInput(Message msg, FunctionBase func, StepInputSession session)
        {
            int stepIndex = GetStepIndex(func, session);
            var step      = func.Steps![stepIndex];
            var answer    = msg.Text!.Trim();

            var error = step.Validate(answer);
            if (error is not null)
            {
                LogStep("ОШИБКА_ВАЛИДАЦИИ", msg.Chat.Id, func, session,
                    userAnswer: answer, extra: $"причина=\"{error}\"");
                await _bot.SendMessage(msg.Chat.Id,
                    $"⚠️ {error}\n\nПопробуй ещё раз:",
                    replyMarkup: BackKeyboard());
                return;
            }

            LogStep("ОТВЕТ", msg.Chat.Id, func, session, userAnswer: answer);
            session.Answers.Add(answer);
            session.CurrentStep++;

            int total = GetTotalSteps(func, session);

            if (session.CurrentStep >= total)
            {
                await FinishStepSession(msg.Chat.Id, func, session);
                return;
            }

            LogStep("СЛЕДУЮЩИЙ_ШАГ", msg.Chat.Id, func, session);
            await AskCurrentStep(msg.Chat.Id, func, session);
        }

        private async Task FinishStepSession(long chatId, FunctionBase func, StepInputSession session)
        {
            string result;
            try
            {
                result = func.CalculateFromAnswers(session.Answers);
                LogSessionEnd("ИТОГ", chatId, func, session.Answers, result: result);
            }
            catch (Exception ex)
            {
                LogSessionEnd("ИТОГ", chatId, func, session.Answers, error: ex.Message);
                await _bot.SendMessage(chatId,
                    $"⚠️ Что-то пошло не так: {ex.Message}",
                    replyMarkup: BackKeyboard());
                return;
            }

            await _bot.SendMessage(chatId, result, replyMarkup: BackKeyboard());
            await _bot.SendMessage(chatId, "Хочешь посчитать ещё раз? Нажми «◀️ Назад» и выбери функцию снова.");
            _inputSession.TryRemove(chatId, out _);
        }

        private async Task HandleSingleLineInput(Message msg, FunctionBase func)
        {
            string resultText;
            try
            {
                resultText = func.CalculateFromText(msg.Text!);
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"⚠️ Ошибка: {ex.Message}\n\n" +
                    $"Введите данные заново: {string.Join(", ", func.Parameters)}",
                    replyMarkup: BackKeyboard());
                return;
            }

            await _bot.SendMessage(msg.Chat.Id,
                $"✔️ Результат: {resultText}", replyMarkup: BackKeyboard());
            await _bot.SendMessage(msg.Chat.Id,
                $"Можете ввести следующие значения для «{func.Name}» или нажмите «◀️ Назад».");
        }

        private async Task HandleUniversalCalculator(Message msg)
        {
            var input = msg.Text?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                await _bot.SendMessage(msg.Chat.Id,
                    "Введите выражение для вычисления:",
                    replyMarkup: BackKeyboard());
                return;
            }

            try
            {
                var (steps, result) = UnivCalc.Calculate(input);
                var sb = new StringBuilder("📋 Пошаговое решение:\n");
                foreach (var step in steps) sb.AppendLine(step);
                sb.Append($"\nИтог: {result}");
                await _bot.SendMessage(msg.Chat.Id, sb.ToString(), replyMarkup: BackKeyboard());
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"⚠️ Ошибка: {ex.Message}", replyMarkup: BackKeyboard());
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Пошаговый ввод: задать вопрос текущего шага
        // ═══════════════════════════════════════════════════════════

        private async Task AskCurrentStep(long chatId, FunctionBase func, StepInputSession session)
        {
            int stepIndex = GetStepIndex(func, session);
            var step      = func.Steps![stepIndex];
            int total     = GetTotalSteps(func, session);

            await _bot.SendMessage(chatId,
                $"Шаг {session.CurrentStep + 1} из {total}\n\n{step.Question}",
                replyMarkup: BackKeyboard());
        }

        // ─── Динамические шаги: сопоставление логического шага с индексом в Steps[] ──

        private static int GetStepIndex(FunctionBase func, StepInputSession session)
        {
            if (func is MonomialMultiplyFunction mf)
                return MonomialMultiplyFunction.StepIndex(session.Answers, session.CurrentStep);

            // MonomialStandardFormFunction и MonomialPowerFunction пропускают Steps[3] (степень b)
            // когда переменная одна: логические шаги 0,1,2 → Steps[0,1,2]; шаг 3 → Steps[4]
            if (func is MonomialStandardFormFunction or MonomialPowerFunction)
            {
                bool twoVars = session.Answers.Count > 0 && session.Answers[0] == "2";
                if (!twoVars && session.CurrentStep >= 3)
                    return session.CurrentStep + 1;
            }

            return session.CurrentStep;
        }

        private static int GetTotalSteps(FunctionBase func, StepInputSession session) =>
            func switch
            {
                MonomialStandardFormFunction sf => sf.ActiveStepCount(session.Answers),
                MonomialPowerFunction        pf => pf.ActiveStepCount(session.Answers),
                MonomialMultiplyFunction     mf => mf.ActiveStepCount(session.Answers),
                _                               => func.Steps?.Length ?? 0,
            };

        // ═══════════════════════════════════════════════════════════
        //  Меню
        // ═══════════════════════════════════════════════════════════

        public async Task SendStartMessage(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(
                new[] { new KeyboardButton[] { BtnSections, BtnCalculator } })
            { ResizeKeyboard = true };

            await _bot.SendMessage(chatId,
                "👋 Привет! Я математический бот.\nВыберите действие:",
                replyMarkup: keyboard);
        }

        private async Task SendCategoryMenu(long chatId)
        {
            var rows = _categories
                .Select(c => new KeyboardButton[] { c.Name })
                .Append([BtnBack])
                .ToArray();

            await _bot.SendMessage(chatId, "📂 Выберите раздел:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private async Task SendSubSectionMenu(long chatId, MathCategory category)
        {
            var rows = category.SubSections
                .Select(s => new KeyboardButton[] { s.Name })
                .Append([BtnBack])
                .ToArray();

            await _bot.SendMessage(chatId, $"📂 {category.Name} — выберите подраздел:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private async Task SendFunctionMenu(long chatId, MathSection section)
        {
            var rows = section.Functions
                .Select(f => new KeyboardButton[] { f.Name })
                .Append([BtnBack])
                .ToArray();

            await _bot.SendMessage(chatId, $"📂 {section.Name} — выберите функцию:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private static ReplyKeyboardMarkup BackKeyboard() =>
            new(new[] { new KeyboardButton[] { BtnBack } }) { ResizeKeyboard = true };

        // ═══════════════════════════════════════════════════════════
        //  Вспомогательное
        // ═══════════════════════════════════════════════════════════

        private void ClearState(long chatId)
        {
            _userState.TryRemove(chatId, out _);
            _selectedCategory.TryRemove(chatId, out _);
            _selectedSection.TryRemove(chatId, out _);
            _selectedFunction.TryRemove(chatId, out _);
            _inputSession.TryRemove(chatId, out _);
        }
    }
}

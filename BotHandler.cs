using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Структура каталога: Раздел → Функции
    // ═══════════════════════════════════════════════════════════════

    public class MathSection
    {
        public string Name { get; init; } = string.Empty;
        public FunctionBase[] Functions { get; init; } = Array.Empty<FunctionBase>();
    }

    /// <summary>
    /// Категория верхнего уровня. Может содержать подразделы (SubSections)
    /// или напрямую функции (Functions) — но не то и другое одновременно.
    /// </summary>
    public class MathCategory
    {
        public string Name { get; init; } = string.Empty;
        /// <summary>Подразделы (вложенное меню). Если не пусто — Functions игнорируются.</summary>
        public MathSection[] SubSections { get; init; } = Array.Empty<MathSection>();
        /// <summary>Функции напрямую (без подразделов).</summary>
        public FunctionBase[] Functions { get; init; } = Array.Empty<FunctionBase>();
        public bool HasSubSections => SubSections.Length > 0;
    }

    // ═══════════════════════════════════════════════════════════════
    //  BotHandler
    // ═══════════════════════════════════════════════════════════════

    class BotHandler
    {
        private readonly ITelegramBotClient _bot;

        // ── Каталог категорий ─────────────────────────────────────
        private readonly MathCategory[] Categories;

        // ── Материалы ─────────────────────────────────────────────
        private readonly Material[] Materials;

        // ── Состояние пользователей ───────────────────────────────
        private readonly ConcurrentDictionary<long, string>            UserState        = new();
        private readonly ConcurrentDictionary<long, MathCategory>     SelectedCategory   = new();
        private readonly ConcurrentDictionary<long, MathSection>       SelectedSection    = new();
        private readonly ConcurrentDictionary<long, FunctionBase>      SelectedFunction = new();

        // Пошаговый ввод: сессия хранит текущий шаг и накопленные ответы
        private readonly ConcurrentDictionary<long, StepInputSession>  InputSession     = new();

        private readonly Dictionary<string, Func<Message, Task>> StateHandlers;

        public BotHandler(ITelegramBotClient bot)
        {
            _bot = bot;

            // ══════════════════════════════════════════════════════
            //  КАТАЛОГ ФУНКЦИЙ
            //  Структура: MathSection → FunctionBase[]
            //
            //  Чтобы добавить функцию:
            //    1. Создайте класс, унаследованный от FunctionBase
            //    2. Вставьте new МойКласс() в нужный MathSection
            //
            //  Разделы с пустым Functions[] показывают «пока пуст».
            // ══════════════════════════════════════════════════════
            Categories = new MathCategory[]
            {
                // ── ⚡ Степень ────────────────────────────────────
                new MathCategory
                {
                    Name = "⚡ Степень",
                    Functions = new FunctionBase[]
                    {
                        new PowerFunction(),
                        new EvaluateAtValueFunction(),
                        new PowerProductFunction(),
                        new PowerQuotientFunction(),
                        new PowerOfPowerFunction(),
                        new PowerOfProductFunction(),
                        new PowerOfFractionFunction(),
                        new ComparePowersFunction(),
                        new FindBaseOrExponentFunction(),
                    }
                },

                // ── 🔢 Одночлены (без подразделов) ───────────────
                new MathCategory
                {
                    Name = "🔢 Одночлены",
                    Functions = new FunctionBase[]
                    {
                        new MonomialStandardFormFunction(),
                        new MonomialPowerFunction(),
                        new MonomialMultiplyFunction(),
                    }
                },

                // ── 🔣 Многочлены (с подразделами) ───────────────
                new MathCategory
                {
                    Name = "🔣 Многочлены",
                    SubSections = new MathSection[]
                    {
                        new MathSection
                        {
                            Name = "🔣 Основы многочленов",
                            Functions = new FunctionBase[]
                            {
                                new PolynomialFromMonomialsFunction(),
                                new PolynomialStandardMembersFunction(),
                                new PolynomialNameMembersFunction(),
                                new PolynomialDegreeFunction(),
                                new PolynomialStandardFormFunction(),
                                new PolynomialValueFunction(),
                                new PolynomialValueTwoVarsFunction(),
                            }
                        },
                        new MathSection
                        {
                            Name = "➕ Сложение и вычитание",
                            Functions = new FunctionBase[]
                            {
                                new PolynomialLikeTermsFunction(),
                                new PolynomialAddFunction(),
                                new PolynomialSubtractFunction(),
                            }
                        },
                        new MathSection
                        {
                            Name = "✖️ Умножение многочленов",
                            Functions = new FunctionBase[]
                            {
                                new PolyTimesMonomial(),
                                new PolyTimesPolyFunction(),
                                new PolySimplifyExpression(),
                                new PolyEvalProduct(),
                                new PolyEquation(),
                                new PolyProveIdentity(),
                            }
                        },
                    }
                },

                

                new MathCategory { Name = "✂️ Формулы сокращённого умножения" },
                new MathCategory { Name = "➗ Алгебраические дроби" },
                new MathCategory { Name = "√ Квадратные корни" },
                new MathCategory { Name = "🔲 Квадратные уравнения" },
                new MathCategory { Name = "⚖️ Неравенства" },
                new MathCategory { Name = "🔀 Системы уравнений и неравенств" },
                new MathCategory { Name = "🎲 Комбинаторика" },
                new MathCategory { Name = "🔢 Последовательности" },
                new MathCategory { Name = "📐 Тригонометрия" },
                new MathCategory
                {
                    Name = "🎯 Теория вероятностей",
                    Functions = new FunctionBase[] { new PercentOfNumberFunction() }
                },
                new MathCategory { Name = "📊 Элементы статистики" },
            };
            // ══════════════════════════════════════════════════════

            Materials = new Material[]
            {
                new Material
                {
                    Name     = "Пример",
                    Keywords = new[] { "пример" },
                    Content  = "Это пример материала."
                }
            };

            StateHandlers = new Dictionary<string, Func<Message, Task>>
            {
                ["choose_category"]   = HandleChooseCategory,
                ["choose_subsection"] = HandleChooseSubSection,
                ["choose_function"]   = HandleChooseFunction,
                ["input_data"]        = HandleInputData,
                ["universal_calc"]    = HandleUniversalCalculator,
            };
        }

        // ═══════════════════════════════════════════════════════════
        //  Входная точка обновлений
        // ═══════════════════════════════════════════════════════════

        public async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken ct)
        {
            if (update.Message is not { } msg) return;
            await OnMessage(msg);
        }

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            Telegram.Bot.Polling.HandleErrorSource source,
            CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {exception.Message} (Источник: {source})");
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────
        //  Логирование пошагового ввода → файл
        //
        //  Файл: logs/steps_YYYY-MM-DD.log  (рядом с exe)
        //  Каждый день создаётся новый файл автоматически.
        //  Запись thread-safe через lock.
        // ─────────────────────────────────────────────────────────

        private static readonly object _logLock = new();

        private static string LogFilePath()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(dir, $"steps_{date}.log");
        }

        private static void WriteLog(string line)
        {
            Console.WriteLine(line);
            lock (_logLock)
            {
                try
                {
                    File.AppendAllText(LogFilePath(), line + Environment.NewLine,
                        System.Text.Encoding.UTF8);
                }
                catch
                {
                    // игнорируем ошибки записи в файл
                }
            }
        }

        private static void LogStep(string tag, long chatId, FunctionBase func,
            StepInputSession session, string? userAnswer = null, string? extra = null)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var stepNum   = session.CurrentStep + 1;
            var total     = func.Steps?.Length ?? 0;

            var sb = new System.Text.StringBuilder();
            sb.Append($"[{timestamp}] [{tag}] user={chatId} | func=\"{func.Name}\"");

            if (userAnswer != null)
                sb.Append($" | шаг {stepNum}/{total} | ответ=\"{userAnswer}\"");
            else
                sb.Append($" | шаг {stepNum}/{total}");

            if (session.Answers.Count > 0)
            {
                var answersFormatted = string.Join(", ",
                    session.Answers.Select((a, i) => $"[{i + 1}]=\"{a}\""));
                sb.Append($" | накоплено: [{answersFormatted}]");
            }

            if (extra != null)
                sb.Append($" | {extra}");

            WriteLog(sb.ToString());
        }

        private static void LogSessionEnd(string tag, long chatId, FunctionBase func,
            List<string> answers, string? result = null, string? error = null)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var answersFormatted = string.Join(", ",
                answers.Select((a, i) => $"[{i + 1}]=\"{a}\""));

            var sb = new System.Text.StringBuilder();
            sb.Append($"[{timestamp}] [{tag}] user={chatId} | func=\"{func.Name}\"");
            sb.Append($" | ЗАВЕРШЕНО | ответы: [{answersFormatted}]");

            if (result != null)
                sb.Append($" | результат=\"{result}\"");
            if (error != null)
                sb.Append($" | ОШИБКА: {error}");

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

            // Логируем все входящие сообщения
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [СООБЩЕНИЕ] user={chatId} (@{msg.Chat.Username ?? "no_username"}) | текст=\"{text}\"");

            if (text.StartsWith("/start"))
            {
                ClearState(chatId);
                await SendStartMessage(chatId);
                return;
            }

            if (text == "◀️ Назад")
            {
                await HandleBack(chatId);
                return;
            }

            if (text == "📂 Разделы")
            {
                ClearState(chatId);
                UserState[chatId] = "choose_category";
                await SendCategoryMenu(chatId);
                return;
            }

            if (text == "🧮 Калькулятор")
            {
                ClearState(chatId);
                UserState[chatId] = "universal_calc";
                await _bot.SendMessage(chatId,
                    "Введите выражение для вычисления:\n(например: 2 + 3 * (4 - 1))",
                    replyMarkup: BackKeyboard());
                return;
            }

            if (UserState.TryGetValue(chatId, out var state) &&
                StateHandlers.TryGetValue(state, out var handler))
            {
                await handler(msg);
                return;
            }

            await _bot.SendMessage(chatId,
                "Я вас не понял. Нажмите /start для перезапуска.");
        }

        // ═══════════════════════════════════════════════════════════
        //  Логика кнопки «Назад»
        //
        //  Главное меню
        //    └─ choose_category
        //         └─ choose_subsection (только если есть подразделы)
        //              └─ choose_function
        //                   └─ input_data  (пошаговый или однострочный)
        // ═══════════════════════════════════════════════════════════

        private async Task HandleBack(long chatId)
        {
            UserState.TryGetValue(chatId, out var state);

            switch (state)
            {
                case "input_data":
                    // Если идёт пошаговый ввод и мы не на первом шаге —
                    // откатываем один шаг назад внутри диалога
                    if (InputSession.TryGetValue(chatId, out var session) &&
                        session.CurrentStep > 0 &&
                        SelectedFunction.TryGetValue(chatId, out var funcForBack) &&
                        funcForBack.Steps != null)
                    {
                        session.CurrentStep--;
                        if (session.Answers.Count > 0)
                            session.Answers.RemoveAt(session.Answers.Count - 1);

                        WriteLog($"[{DateTime.Now:HH:mm:ss}] [ОТКАТ] user={chatId} | func=\"{funcForBack.Name}\" | возврат к шагу {session.CurrentStep + 1} | накоплено ответов={session.Answers.Count}");

                        await AskCurrentStep(chatId, funcForBack, session);
                        return;
                    }

                    // Иначе — выходим к списку функций
                    if (InputSession.TryGetValue(chatId, out var abortedSession) &&
                        SelectedFunction.TryGetValue(chatId, out var abortedFunc))
                    {
                        WriteLog($"[{DateTime.Now:HH:mm:ss}] [ОТМЕНА] user={chatId} | func=\"{abortedFunc.Name}\" | прервано на шаге {abortedSession.CurrentStep + 1} | накоплено ответов={abortedSession.Answers.Count}");
                    }
                    InputSession.TryRemove(chatId, out _);
                    SelectedFunction.TryRemove(chatId, out _);
                    UserState[chatId] = "choose_function";
                    if (SelectedSection.TryGetValue(chatId, out var sec))
                        await SendFunctionMenu(chatId, sec);
                    else
                        goto default;
                    break;

                case "choose_function":
                    SelectedFunction.TryRemove(chatId, out _);
                    // Если у категории есть подразделы — назад к подразделам
                    if (SelectedCategory.TryGetValue(chatId, out var catForFunc) && catForFunc.HasSubSections)
                    {
                        SelectedSection.TryRemove(chatId, out _);
                        UserState[chatId] = "choose_subsection";
                        await SendSubSectionMenu(chatId, catForFunc);
                    }
                    else
                    {
                        SelectedSection.TryRemove(chatId, out _);
                        UserState[chatId] = "choose_category";
                        await SendCategoryMenu(chatId);
                    }
                    break;

                case "choose_subsection":
                    SelectedSection.TryRemove(chatId, out _);
                    SelectedCategory.TryRemove(chatId, out _);
                    UserState[chatId] = "choose_category";
                    await SendCategoryMenu(chatId);
                    break;

                default:
                    ClearState(chatId);
                    await SendStartMessage(chatId);
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Обработчики состояний
        // ═══════════════════════════════════════════════════════════

        private async Task HandleChooseCategory(Message msg)
        {
            var category = Categories.FirstOrDefault(c =>
                c.Name.Equals(msg.Text, StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                await _bot.SendMessage(msg.Chat.Id, "Выберите раздел из предложенных кнопок.");
                return;
            }

            SelectedCategory[msg.Chat.Id] = category;

            // Если есть подразделы — показываем подменю
            if (category.HasSubSections)
            {
                UserState[msg.Chat.Id] = "choose_subsection";
                await SendSubSectionMenu(msg.Chat.Id, category);
                return;
            }

            // Иначе — напрямую к функциям
            if (!category.Functions.Any())
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"📭 Раздел «{category.Name}» пока не содержит функций.\n" +
                    "Выберите другой раздел или нажмите «◀️ Назад».");
                return;
            }

            // Создаём временный MathSection из функций категории
            var section = new MathSection { Name = category.Name, Functions = category.Functions };
            SelectedSection[msg.Chat.Id] = section;
            UserState[msg.Chat.Id]       = "choose_function";
            await SendFunctionMenu(msg.Chat.Id, section);
        }

        private async Task HandleChooseSubSection(Message msg)
        {
            if (!SelectedCategory.TryGetValue(msg.Chat.Id, out var category))
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

            if (!section.Functions.Any())
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"📭 Подраздел «{section.Name}» пока не содержит функций.\n" +
                    "Выберите другой или нажмите «◀️ Назад».");
                return;
            }

            SelectedSection[msg.Chat.Id] = section;
            UserState[msg.Chat.Id]       = "choose_function";
            await SendFunctionMenu(msg.Chat.Id, section);
        }

        private async Task HandleChooseFunction(Message msg)
        {
            if (!SelectedSection.TryGetValue(msg.Chat.Id, out var section))
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

            SelectedFunction[msg.Chat.Id] = func;
            UserState[msg.Chat.Id]        = "input_data";

            // ── Пошаговый ввод ────────────────────────────────────
            if (func.Steps != null)
            {
                var session = new StepInputSession();
                InputSession[msg.Chat.Id] = session;

                WriteLog($"[{DateTime.Now:HH:mm:ss}] [СТАРТ] user={msg.Chat.Id} | func=\"{func.Name}\" | шагов={func.Steps.Length}");

                // Показываем формулу и сразу задаём первый вопрос
                await _bot.SendMessage(msg.Chat.Id,
                    $"✅ {func.Name}\nФормула: {func.Formula}",
                    replyMarkup: BackKeyboard());

                await AskCurrentStep(msg.Chat.Id, func, session);
                return;
            }

            // ── Старый однострочный режим ─────────────────────────
            var prompt =
                $"✅ {func.Name}\n" +
                $"Формула: {func.Formula}\n\n" +
                $"Введите через пробел: {string.Join(", ", func.Parameters)}";

            await _bot.SendMessage(msg.Chat.Id, prompt, replyMarkup: BackKeyboard());
        }

        // ─────────────────────────────────────────────────────────
        //  Пошаговый ввод: задать текущий вопрос
        // ─────────────────────────────────────────────────────────

        private async Task AskCurrentStep(long chatId, FunctionBase func, StepInputSession session)
        {
            // Определяем реальный индекс шага в Steps[]
            int stepIndex = GetStepIndex(func, session);
            var step      = func.Steps![stepIndex];

            int total   = GetTotalSteps(func, session);
            int current = session.CurrentStep + 1;

            await _bot.SendMessage(chatId,
                $"Шаг {current} из {total}\n\n{step.Question}",
                replyMarkup: BackKeyboard());
        }

        // ─────────────────────────────────────────────────────────
        //  Определить индекс шага в Steps[] с учётом динамики
        // ─────────────────────────────────────────────────────────

        private static int GetStepIndex(FunctionBase func, StepInputSession session)
        {
            // Функции с динамическим количеством шагов переопределяют StepIndex
            if (func is MonomialMultiplyFunction mf)
                return mf.StepIndex(session.Answers, session.CurrentStep);

            // Для стандартной формы и степени: если одна переменная,
            // пропускаем шаг про b (индекс 3) когда логический шаг == 3
            // (т.е. шаг 3 = "в какую степень" для MonomialPowerFunction)
            if (func is MonomialStandardFormFunction || func is MonomialPowerFunction)
            {
                bool two = session.Answers.Count > 0 && session.Answers[0] == "2";
                // Steps[] layout: [0=varCount, 1=k, 2=pa, 3=pb, 4=n]
                // 1 var: logical steps 0,1,2 -> Steps[0,1,2]; step 3 (Power: n) -> Steps[4]
                // 2 var: logical steps 0,1,2,3 -> Steps[0,1,2,3]; step 4 (Power: n) -> Steps[4]
                if (!two && session.CurrentStep >= 3)
                    return session.CurrentStep + 1; // skip Steps[3] (b-step)
            }

            return session.CurrentStep;
        }

        private static int GetTotalSteps(FunctionBase func, StepInputSession session)
        {
            if (func is MonomialStandardFormFunction sf) return sf.ActiveStepCount(session.Answers);
            if (func is MonomialPowerFunction pf)       return pf.ActiveStepCount(session.Answers);
            if (func is MonomialMultiplyFunction mf)    return mf.ActiveStepCount(session.Answers);
            return func.Steps?.Length ?? 0;
        }

        // ─────────────────────────────────────────────────────────
        //  Обработчик ввода данных (пошаговый + однострочный)
        // ─────────────────────────────────────────────────────────

        private async Task HandleInputData(Message msg)
        {
            if (!SelectedFunction.TryGetValue(msg.Chat.Id, out var func))
            {
                await _bot.SendMessage(msg.Chat.Id, "Произошла ошибка. Попробуйте /start");
                return;
            }

            // ── Пошаговый режим ───────────────────────────────────
            if (func.Steps != null &&
                InputSession.TryGetValue(msg.Chat.Id, out var session))
            {
                int stepIndex = GetStepIndex(func, session);
                var step      = func.Steps[stepIndex];

                // Валидация текущего ответа
                var error = step.Validate(msg.Text.Trim());
                if (error != null)
                {
                    LogStep("ОШИБКА_ВАЛИДАЦИИ", msg.Chat.Id, func, session,
                        userAnswer: msg.Text.Trim(), extra: $"причина=\"{error}\"");
                    await _bot.SendMessage(msg.Chat.Id,
                        $"⚠️ {error}\n\nПопробуй ещё раз:",
                        replyMarkup: BackKeyboard());
                    return;
                }

                // Сохраняем ответ и переходим к следующему шагу
                LogStep("ОТВЕТ", msg.Chat.Id, func, session, userAnswer: msg.Text.Trim());
                session.Answers.Add(msg.Text.Trim());
                session.CurrentStep++;

                int total = GetTotalSteps(func, session);

                // Все шаги пройдены — считаем результат
                if (session.CurrentStep >= total)
                {
                    string result;
                    try
                    {
                        result = func.CalculateFromAnswers(session.Answers);
                        LogSessionEnd("ИТОГ", msg.Chat.Id, func, session.Answers, result: result);
                    }
                    catch (Exception ex)
                    {
                        LogSessionEnd("ИТОГ", msg.Chat.Id, func, session.Answers, error: ex.Message);
                        await _bot.SendMessage(msg.Chat.Id,
                            $"⚠️ Что-то пошло не так: {ex.Message}",
                            replyMarkup: BackKeyboard());
                        return;
                    }

                    await _bot.SendMessage(msg.Chat.Id, result, replyMarkup: BackKeyboard());
                    await _bot.SendMessage(msg.Chat.Id,
                        "Хочешь посчитать ещё раз? Нажми «◀️ Назад» и выбери функцию снова.");

                    // Сбрасываем сессию, но остаёмся в input_data
                    InputSession.TryRemove(msg.Chat.Id, out _);
                    return;
                }

                // Задаём следующий вопрос
                LogStep("СЛЕДУЮЩИЙ_ШАГ", msg.Chat.Id, func, session);
                await AskCurrentStep(msg.Chat.Id, func, session);
                return;
            }

            // ── Однострочный режим (старые функции) ───────────────
            string resultText;
            try
            {
                resultText = func.CalculateFromText(msg.Text);
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
                var reply = "📋 Пошаговое решение:\n";
                foreach (var step in steps) reply += step + "\n";
                reply += $"\nИтог: {result}";
                await _bot.SendMessage(msg.Chat.Id, reply, replyMarkup: BackKeyboard());
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"⚠️ Ошибка: {ex.Message}", replyMarkup: BackKeyboard());
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Отправка меню
        // ═══════════════════════════════════════════════════════════

        public async Task SendStartMessage(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "📂 Разделы", "🧮 Калькулятор" },
            })
            { ResizeKeyboard = true };

            await _bot.SendMessage(chatId,
                "👋 Привет! Я математический бот.\nВыберите действие:",
                replyMarkup: keyboard);
        }

        private async Task SendCategoryMenu(long chatId)
        {
            var rows = Categories
                .Select(c => new KeyboardButton[] { c.Name })
                .Append(new KeyboardButton[] { "◀️ Назад" })
                .ToArray();

            await _bot.SendMessage(chatId, "📂 Выберите раздел:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private async Task SendSubSectionMenu(long chatId, MathCategory category)
        {
            var rows = category.SubSections
                .Select(s => new KeyboardButton[] { s.Name })
                .Append(new KeyboardButton[] { "◀️ Назад" })
                .ToArray();

            await _bot.SendMessage(chatId, $"📂 {category.Name} — выберите подраздел:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private async Task SendFunctionMenu(long chatId, MathSection section)
        {
            var rows = section.Functions
                .Select(f => new KeyboardButton[] { f.Name })
                .Concat(new[] { new KeyboardButton[] { "◀️ Назад" } })
                .ToArray();

            await _bot.SendMessage(chatId, $"📂 {section.Name} — выберите функцию:",
                replyMarkup: new ReplyKeyboardMarkup(rows) { ResizeKeyboard = true });
        }

        private static ReplyKeyboardMarkup BackKeyboard() =>
            new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "◀️ Назад" }
            })
            { ResizeKeyboard = true };

        // ═══════════════════════════════════════════════════════════
        //  Вспомогательное
        // ═══════════════════════════════════════════════════════════

        private void ClearState(long chatId)
        {
            UserState.TryRemove(chatId, out _);
            SelectedCategory.TryRemove(chatId, out _);
            SelectedSection.TryRemove(chatId, out _);
            SelectedFunction.TryRemove(chatId, out _);
            InputSession.TryRemove(chatId, out _);
        }
    }
}

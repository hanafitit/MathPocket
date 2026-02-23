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

    // ═══════════════════════════════════════════════════════════════
    //  BotHandler
    // ═══════════════════════════════════════════════════════════════

    class BotHandler
    {
        private readonly ITelegramBotClient _bot;

        // ── Каталог разделов ──────────────────────────────────────
        private readonly MathSection[] Sections;

        // ── Материалы ─────────────────────────────────────────────
        private readonly Material[] Materials;

        // ── Состояние пользователей ───────────────────────────────
        private readonly ConcurrentDictionary<long, string>            UserState        = new();
        private readonly ConcurrentDictionary<long, MathSection>       SelectedSection  = new();
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
            Sections = new MathSection[]
            {
                new MathSection
                {
                    Name = "⚡ Степень",
                    Functions = new FunctionBase[]
                    {
                        new PowerFunction(),
                        new PowerProductFunction(),
                        new PowerQuotientFunction(),
                        new PowerOfPowerFunction(),
                        new PowerOfProductFunction(),
                        new PowerOfFractionFunction(),
                        new ComparePowersFunction(),
                        new FindBaseOrExponentFunction(),
                    }
                },

                new MathSection
                {
                    Name = "🔢 Одночлены",
                    Functions = new FunctionBase[]
                    {
                        new MonomialStandardFormFunction(),
                        new MonomialPowerFunction(),
                        new MonomialMultiplyFunction(),
                    }
                },

                new MathSection
{
    Name = "🔣 Многочлены",
    Functions = new FunctionBase[]
    {
        new PolynomialDegreeFunction(),
        new PolynomialLikeTermsFunction(),
        new PolynomialValueFunction(),
    }
},

                

                new MathSection
                {
                    Name = "✂️ Формулы сокращённого умножения",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции для ФСУ
                    }
                },

                new MathSection
                {
                    Name = "➗ Алгебраические дроби",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции для алгебраических дробей
                    }
                },

                new MathSection
                {
                    Name = "√ Квадратные корни",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции для квадратных корней
                    }
                },

                new MathSection
                {
                    Name = "🔲 Квадратные уравнения",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции для квадратных уравнений
                    }
                },

                new MathSection
                {
                    Name = "⚖️ Неравенства",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции для неравенств
                    }
                },

                new MathSection
                {
                    Name = "🔀 Системы уравнений и неравенств",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции
                    }
                },

                new MathSection
                {
                    Name = "🎲 Комбинаторика",
                    Functions = new FunctionBase[]
                    {
                        new Nod(),
                        new Nok(),
                    }
                },

                new MathSection
                {
                    Name = "🔢 Последовательности",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции
                    }
                },

                new MathSection
                {
                    Name = "📐 Тригонометрия",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте функции
                    }
                },

                new MathSection
                {
                    Name = "🎯 Теория вероятностей",
                    Functions = new FunctionBase[]
                    {
                        new PercentOfNumberFunction(),
                    }
                },

                new MathSection
                {
                    Name = "📊 Элементы статистики",
                    Functions = new FunctionBase[]
                    {
                        // TODO: добавьте статистические функции
                    }
                },
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
                ["choose_section"]  = HandleChooseSection,
                ["choose_function"] = HandleChooseFunction,
                ["input_data"]      = HandleInputData,
                ["universal_calc"]  = HandleUniversalCalculator,
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
                UserState[chatId] = "choose_section";
                await SendSectionMenu(chatId);
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
        //    └─ choose_section
        //         └─ choose_function
        //              └─ input_data  (пошаговый или однострочный)
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

                        await AskCurrentStep(chatId, funcForBack, session);
                        return;
                    }

                    // Иначе — выходим к списку функций
                    InputSession.TryRemove(chatId, out _);
                    SelectedFunction.TryRemove(chatId, out _);
                    UserState[chatId] = "choose_function";
                    if (SelectedSection.TryGetValue(chatId, out var sec))
                        await SendFunctionMenu(chatId, sec);
                    else
                        goto default;
                    break;

                case "choose_function":
                    SelectedSection.TryRemove(chatId, out _);
                    UserState[chatId] = "choose_section";
                    await SendSectionMenu(chatId);
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

        private async Task HandleChooseSection(Message msg)
        {
            var section = Sections.FirstOrDefault(s =>
                s.Name.Equals(msg.Text, StringComparison.OrdinalIgnoreCase));

            if (section is null)
            {
                await _bot.SendMessage(msg.Chat.Id, "Выберите раздел из предложенных кнопок.");
                return;
            }

            if (section.Functions.Length == 0)
            {
                await _bot.SendMessage(msg.Chat.Id,
                    $"📭 Раздел «{section.Name}» пока не содержит функций.\n" +
                    "Выберите другой раздел или нажмите «◀️ Назад».");
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
                    await _bot.SendMessage(msg.Chat.Id,
                        $"⚠️ {error}\n\nПопробуй ещё раз:",
                        replyMarkup: BackKeyboard());
                    return;
                }

                // Сохраняем ответ и переходим к следующему шагу
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
                    }
                    catch (Exception ex)
                    {
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

        private async Task SendSectionMenu(long chatId)
        {
            var rows = Sections
                .Select(s => new KeyboardButton[] { s.Name })
                .Concat(new[] { new KeyboardButton[] { "◀️ Назад" } })
                .ToArray();

            await _bot.SendMessage(chatId, "📂 Выберите раздел:",
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
            SelectedSection.TryRemove(chatId, out _);
            SelectedFunction.TryRemove(chatId, out _);
            InputSession.TryRemove(chatId, out _);
        }
    }
}

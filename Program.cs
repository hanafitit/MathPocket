using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MathPocket
{
    class Program
    {
        private static TelegramBotClient _bot;
        private static BotHandler _handler;

        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                // Чтение токена из файла
                var token = File.ReadAllText("token.txt");
                if (string.IsNullOrEmpty(token))
                    throw new Exception("Файл token.txt пустой или токен не задан.");

                // Инициализация бота
                _bot = new TelegramBotClient(token);
                _handler = new BotHandler(_bot);

                var me = await _bot.GetMe();
                Console.WriteLine($"@{me.Username} работает... Нажми Enter для выхода");

                using var cts = new CancellationTokenSource();

                _bot.StartReceiving(
                    updateHandler: _handler.HandleUpdateAsync,
                    errorHandler: _handler.HandleErrorAsync,
                    receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                    {
                        AllowedUpdates = { } // все типы обновлений
                    }
                );

                Console.ReadLine();
                cts.Cancel();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Ошибка: файл token.txt не найден!");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при запуске бота:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}

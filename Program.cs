using System;
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
                // Токен бота
                var токен = "8023118563:AAFfqJ4IE4CxEiSovDmsUJheBHBxD6S7RGw";

                // Инициализация бота и обработчика
                _bot = new TelegramBotClient(токен);
                _handler = new BotHandler(_bot);

                // Проверка подключения
                var я = await _bot.GetMe();
                Console.WriteLine($"Бот @{я.Username} успешно запущен и работает...");

                // Токен отмены для корректного завершения
                using var источникОтмены = new CancellationTokenSource();

                // Запуск получения обновлений
                _bot.StartReceiving(
                    updateHandler: _handler.HandleUpdateAsync,
                    errorHandler: _handler.HandleErrorAsync,
                    receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                    {
                        AllowedUpdates = { }
                    },
                    cancellationToken: источникОтмены.Token
                );

                Console.WriteLine("Для остановки бота нажмите Ctrl+C");

                Console.CancelKeyPress += (отправитель, аргументы) =>
                {
                    аргументы.Cancel = true;
                    Console.WriteLine("Получен сигнал остановки. Завершение работы...");
                    источникОтмены.Cancel();
                };

                await Task.Delay(Timeout.Infinite, источникОтмены.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Бот остановлен.");
            }
            catch (Exception ошибка)
            {
                Console.WriteLine("Произошла ошибка при запуске бота:");
                Console.WriteLine(ошибка.Message);
            }
        }
    }
}
using System;
using System.Net;
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
                var токен = "8023118563:AAFfqJ4IE4CxEiSovDmsUJheBHBxD6S7RGw";

                if (string.IsNullOrWhiteSpace(токен))
                    throw new Exception("Токен не задан.");

                _bot = new TelegramBotClient(токен);
                _handler = new BotHandler(_bot);

                var я = await _bot.GetMe();
                Console.WriteLine($"Бот @{я.Username} успешно запущен и работает...");

                using var источникОтмены = new CancellationTokenSource();

                // Запуск Telegram-бота
                _bot.StartReceiving(
                    updateHandler: _handler.HandleUpdateAsync,
                    errorHandler: _handler.HandleErrorAsync,
                    receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                    {
                        AllowedUpdates = { }
                    },
                    cancellationToken: источникОтмены.Token
                );

                // Запуск простого HTTP-сервера для Render
                _ = Task.Run(() => ЗапуститьВебСервер(источникОтмены.Token));

                Console.WriteLine("Для остановки нажмите Ctrl+C");

                Console.CancelKeyPress += (отправитель, аргументы) =>
                {
                    аргументы.Cancel = true;
                    Console.WriteLine("Остановка...");
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
                Console.WriteLine("Ошибка при запуске бота:");
                Console.WriteLine(ошибка.Message);
            }
        }

        static async Task ЗапуститьВебСервер(CancellationToken токенОтмены)
        {
            // Render передаёт порт через переменную окружения PORT
            var порт = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            var слушатель = new HttpListener();
            слушатель.Prefixes.Add($"http://*:{порт}/");
            слушатель.Start();
            Console.WriteLine($"Веб-сервер запущен на порту {порт}");

            while (!токенОтмены.IsCancellationRequested)
            {
                try
                {
                    var контекст = await слушатель.GetContextAsync();
                    var ответ = контекст.Response;
                    var текст = System.Text.Encoding.UTF8.GetBytes("Бот работает!");
                    ответ.ContentLength64 = текст.Length;
                    await ответ.OutputStream.WriteAsync(текст, токенОтмены);
                    ответ.OutputStream.Close();
                }
                catch
                {
                    // игнорируем ошибки соединения
                }
            }

            слушатель.Stop();
        }
    }
}
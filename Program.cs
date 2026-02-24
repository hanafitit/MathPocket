using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MathPocket
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var writer = new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(writer);

            try
            {
                var токен = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
                            ?? "8023118563:AAFfqJ4IE4CxEiSovDmsUJheBHBxD6S7RGw";

                if (string.IsNullOrWhiteSpace(токен))
                    throw new Exception("Токен не задан.");

                var bot = new TelegramBotClient(токен);
                var handler = new BotHandler(bot);

                var я = await bot.GetMe();
                Console.WriteLine($"Бот @{я.Username} успешно запущен и работает...");

                using var источникОтмены = new CancellationTokenSource();

                bot.StartReceiving(
                    updateHandler: handler.HandleUpdateAsync,
                    errorHandler: handler.HandleErrorAsync,
                    receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                    {
                        AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
                    },
                    cancellationToken: источникОтмены.Token
                );

                _ = Task.Run(() => ЗапуститьВебСервер(источникОтмены.Token));

                Console.WriteLine("Для остановки нажмите Ctrl+C");
                Console.CancelKeyPress += (_, аргументы) =>
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
                Console.WriteLine($"Ошибка при запуске бота: {ошибка.Message}");
            }

            // Локальная функция — объявлена ВНУТРИ Main, после основного кода
            static async Task ЗапуститьВебСервер(CancellationToken токенОтмены)
            {
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
}
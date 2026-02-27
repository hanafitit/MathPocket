using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace MathPocket
{
    internal static class Program
    {
        private static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using var cts = new CancellationTokenSource();

            // Graceful shutdown по Ctrl+C
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Остановка...");
                cts.Cancel();
            };

            try
            {
                var token = Environment.GetEnvironmentVariable("8023118563:AAFfqJ4IE4CxEiSovDmsUJheBHBxD6S7RGw")
                            ?? throw new InvalidOperationException(
                                "BOT_TOKEN");

                var bot     = new TelegramBotClient(token);
                var handler = new BotHandler(bot);

                var me = await bot.GetMe(cts.Token);
                Console.WriteLine($"Бот @{me.Username} запущен.");

                bot.StartReceiving(
                    updateHandler: handler.HandleUpdateAsync,
                    errorHandler: (_, ex, source, ct) => HandlePollingError(ex, source, ct),
                    receiverOptions: new ReceiverOptions
                    {
                        AllowedUpdates = []   // получать все типы обновлений
                    },
                    cancellationToken: cts.Token
                );

                _ = RunWebServer(cts.Token);

                Console.WriteLine("Для остановки нажмите Ctrl+C.");
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Бот остановлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Environment.Exit(1);
            }
        }

        // ─── Обработчик ошибок polling ────────────────────────────

        private static async Task HandlePollingError(
            Exception exception,
            HandleErrorSource source,
            CancellationToken ct)
        {
            if (exception is ApiRequestException { Message: var msg } && msg.Contains("Conflict"))
            {
                Console.WriteLine("[Конфликт] Другой экземпляр бота активен. Ожидание 10 с...");
                await Task.Delay(10_000, ct);
            }
            else
            {
                Console.WriteLine($"[ОШИБКА POLLING] {exception.Message} (источник: {source})");
            }
        }

        // ─── Веб-сервер для health-check (Render / Railway) ───────

        private static async Task RunWebServer(CancellationToken ct)
        {
            var port    = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();
            Console.WriteLine($"Веб-сервер запущен на порту {port}.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var context  = await listener.GetContextAsync();
                    var response = context.Response;
                    var body     = Encoding.UTF8.GetBytes("OK");
                    response.ContentLength64 = body.Length;
                    await response.OutputStream.WriteAsync(body, ct);
                    response.OutputStream.Close();
                }
                catch (OperationCanceledException) { break; }
                catch { /* игнорируем разовые ошибки соединения */ }
            }

            listener.Stop();
        }
    }
}

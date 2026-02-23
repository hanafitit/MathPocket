using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
namespace MathPocket
{

    class Program

    {

        private static TelegramBotClient _bot;
        private static BotHandler _handler;

        static async Task Main()

        {
            
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            token = System.IO.File.ReadAllText("token.txt");
            _bot = new TelegramBotClient(token);
            _handler = new BotHandler(_bot);

            var me = await _bot.GetMe();

            Console.WriteLine($"@{me.Username} работает... Нажми Enter для выхода");

            using var cts = new CancellationTokenSource();
            while (true)
            {
                try
                {
                    _bot.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                {
                    AllowedUpdates = { }
                }
            );
                    await Task.Delay(-1);
                    Console.ReadLine();
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Перезапуск через 5 секунд...");
                    await Task.Delay(5000);
                }
            }
            

            
        }
    }


}





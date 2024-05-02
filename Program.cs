using TelegramBot.Classes;

string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
if (botToken == null)
{
    Console.WriteLine("Bot token is not set");
    Environment.Exit(1);
}
Bot bot = new(botToken);
bot.Start();
await bot.GetInfo();
Console.ReadLine();
bot.Stop();
using Microsoft.EntityFrameworkCore;
using TelegramBot.Data;
using TelegramBot.Repositories;
using TelegramBot.Utilities;

string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
if (botToken == null)
{
    Console.WriteLine("Bot token is not set");
    Environment.Exit(1);
}

if (!Directory.Exists("Database"))
    Directory.CreateDirectory("Database");
ApplicationDbContext context = new();
PaymentsRepository paymentsRepository = new(context);
UserRepository userRepository = new(context);
if (context.Database.GetPendingMigrations().Any())
{
    Console.WriteLine("Applying migrations");
    await context.Database.MigrateAsync();
}
Bot bot = new(botToken, paymentsRepository, userRepository);
bot.Start();
await bot.GetInfo();
await Task.Delay(-1);
bot.Stop();
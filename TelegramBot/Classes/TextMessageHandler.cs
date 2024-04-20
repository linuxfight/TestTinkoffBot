using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Classes;

public static class TextMessageHandler
{
    public static async Task OnMessageAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        switch (message.Text)
        {
            case "/start":
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет, я бот для генерации ссылок на тинькофф",
                    cancellationToken: cancellationToken
                );
                await bot.SetMyCommandsAsync(
                    commands: new List<BotCommand>()
                    {
                        new()
                        {
                            Command = "start",
                            Description = "Запустить бота"
                        },
                        new()
                        {
                            Command = "generate",
                            Description = "Сгенерировать ссылку"
                        }
                    }, cancellationToken: cancellationToken);
                break;
            case "/generate":
                var response = await TinkoffLinkGenerator.GenerateLink();
                if (response.Success)
                {
                    await bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Ссылка: [тык]({response.PaymentURL})",
                        cancellationToken: cancellationToken
                    );    
                }
                else
                {
                    await bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Ошибка: {response.ErrorCode} \n" +
                              $"{response.Status}",
                        cancellationToken: cancellationToken
                    );
                }
                break;
        }
    }
}
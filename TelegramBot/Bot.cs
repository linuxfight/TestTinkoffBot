using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBot;

public class Bot
{
    private TelegramBotClient _bot;
    private CancellationTokenSource _cts;
    private TinkoffLinkGenerator _generator;

    public Bot(string botToken)
    {
        _bot = new(botToken);
        _cts = new();
        _generator = new();
    }

    public void Start()
    {
        ReceiverOptions receiverOptions = new ()
        {
            ThrowPendingUpdates = true
        };

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
    }

    public void Stop()
    {
        _cts.Cancel();
        Environment.Exit(0);
    }

    public async Task GetInfo()
    {
        User bot = await _bot.GetMeAsync();
        Console.WriteLine($"Start listening for @{bot.Username}");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        long chatId = message.Chat.Id;

        if (messageText == "/generate")
        {
            string content = await _generator.GenerateLink();
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: content,
                cancellationToken: cancellationToken
            );
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string? ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        Environment.Exit(1);
        return null;
    }
}

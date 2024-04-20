using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBot.Classes;

public class Bot(string botToken)
{
    private readonly TelegramBotClient _bot = new TelegramBotClient(botToken);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

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
            cancellationToken: _cancellationTokenSource.Token
        );
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
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
        if (message.Text is not { })
            return;

        await TextMessageHandler.OnMessageAsync(botClient, message, cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        Environment.Exit(1);
        return null;
    }
}

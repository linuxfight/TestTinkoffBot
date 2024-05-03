using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Repositories;
using User = Telegram.Bot.Types.User;

namespace TelegramBot.Utilities;

public class Bot(string botToken, PaymentsRepository paymentsRepository, UserRepository userRepository)
{
    private readonly TelegramBotClient _bot = new(botToken);
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
        switch (update.Type)
        {
            case UpdateType.Message:
                Message? message = update.Message;
                if (message?.From == null)
                {
                    if (message != null)
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Бот работает только в ЛС", cancellationToken: cancellationToken);
                    return;
                }
        
                Models.User? user = userRepository.FirstOrDefault(x => x.Id == message.From.Id);
                if (user == null)
                {
                    user = new() { Id = message.From.Id };
                    userRepository.Add(user);
                    userRepository.Save();
                }

                await OnMessageAsync(botClient, message, cancellationToken, user);
                break;
            case UpdateType.CallbackQuery:
                CallbackQuery? callbackQuery = update.CallbackQuery;
                if (callbackQuery != null) await OnCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
                break;
        }
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
    
    private async Task OnMessageAsync(ITelegramBotClient bot, Message? message, CancellationToken cancellationToken, Models.User user)
    {
        List<InlineKeyboardButton> buttons;
        switch (message?.Text)
        {
            case "/start":
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет, я бот для генерации ссылок на тинькофф",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken
                );
                await bot.SetMyCommandsAsync(
                    commands: new List<BotCommand>
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
                        },
                        new()
                        {
                            Command = "cancel",
                            Description = "Отменить текущее действие"
                        }
                    }, cancellationToken: cancellationToken);
                break;
            case "/generate":
                Payment payment = new()
                {
                    Creator = user
                };
                paymentsRepository.Add(payment);
                paymentsRepository.Save();
                buttons = new()
                {
                    InlineKeyboardButton.WithCallbackData("\u2705", $"generate_link_{payment.Id}"),
                    InlineKeyboardButton.WithCallbackData("\u270f", $"edit_payment_{payment.Id}")
                };
                await bot.SendTextMessageAsync(message.Chat.Id, 
                    $"\ud83d\udcb5 Цена: {payment.Price} \n" +
                    $"\ud83d\udcdd Описание: {payment.Description} \n " +
                    $"\ud83d\udcde Номер телефона клиента: {payment.UserPhone} \n " +
                    $"\u2709 Почта клиента: {payment.UserEmail} \n ", 
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
                break;
            case "/cancel":
                user.State = "default";
                userRepository.Save();
                await bot.SendTextMessageAsync(message.Chat.Id, "Текущее действие отменено",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
                break;
            default:
                if (user.State != "default")
                {
                    string[] data = user.State.Replace("edit_payment_", "").Split("_");
                    string id = data[0];
                    string changeType = data[1];
                    Payment? editablePayment = paymentsRepository.FirstOrDefault(x => x.Id.ToString() == id);
                    if (editablePayment == null)
                        break;
                    switch (changeType)
                    {
                        case "phone":
                            if (message?.Text != null)
                            {
                                if (!Regex.IsMatch(message.Text, @"\+7\d{10}"))
                                    await bot.SendTextMessageAsync(message.Chat.Id,
                                        "Введите номер телефона в формате: +79123456789",
                                        replyToMessageId: message.MessageId,
                                        cancellationToken: cancellationToken);
                                else
                                {
                                    editablePayment.UserPhone = message.Text;
                                    paymentsRepository.Update(editablePayment);
                                    paymentsRepository.Save();
                                    user.State = "default";
                                    userRepository.Update(user);
                                    userRepository.Save();
                                    buttons = new()
                                    {
                                        InlineKeyboardButton.WithCallbackData("\u2705", $"generate_link_{editablePayment.Id}"),
                                        InlineKeyboardButton.WithCallbackData("\u270f", $"edit_payment_{editablePayment.Id}")
                                    };
                                    await bot.SendTextMessageAsync(message.Chat.Id, 
                                        $"\ud83d\udcb5 Цена: {editablePayment.Price} \n" +
                                        $"\ud83d\udcdd Описание: {editablePayment.Description} \n " +
                                        $"\ud83d\udcde Номер телефона клиента: {editablePayment.UserPhone} \n " +
                                        $"\u2709 Почта клиента: {editablePayment.UserEmail} \n ", 
                                        replyMarkup: new InlineKeyboardMarkup(buttons),
                                        replyToMessageId: message.MessageId,
                                        cancellationToken: cancellationToken);
                                }
                            }
                            break;
                        case "email":
                            if (message?.Text != null)
                            {
                                if (!Regex.IsMatch(message.Text, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
                                    await bot.SendTextMessageAsync(message.Chat.Id,
                                        "Введите email в формате: a@test.ru",
                                        replyToMessageId: message.MessageId,
                                        cancellationToken: cancellationToken);
                                else
                                {
                                    editablePayment.UserEmail = message.Text;
                                    paymentsRepository.Update(editablePayment);
                                    paymentsRepository.Save();
                                    user.State = "default";
                                    userRepository.Update(user);
                                    userRepository.Save();
                                    buttons = new()
                                    {
                                        InlineKeyboardButton.WithCallbackData("\u2705", $"generate_link_{editablePayment.Id}"),
                                        InlineKeyboardButton.WithCallbackData("\u270f", $"edit_payment_{editablePayment.Id}")
                                    };
                                    await bot.SendTextMessageAsync(message.Chat.Id, 
                                        $"\ud83d\udcb5 Цена: {editablePayment.Price} \n" +
                                        $"\ud83d\udcdd Описание: {editablePayment.Description} \n " +
                                        $"\ud83d\udcde Номер телефона клиента: {editablePayment.UserPhone} \n " +
                                        $"\u2709 Почта клиента: {editablePayment.UserEmail} \n ", 
                                        replyMarkup: new InlineKeyboardMarkup(buttons),
                                        replyToMessageId: message.MessageId,
                                        cancellationToken: cancellationToken);
                                }
                            }
                            break;
                        case "description":
                            if (message?.Text != null)
                            {
                                editablePayment.Description = message.Text;
                                paymentsRepository.Update(editablePayment);
                                paymentsRepository.Save();
                                user.State = "default";
                                userRepository.Update(user);
                                userRepository.Save();
                                buttons = new()
                                {
                                    InlineKeyboardButton.WithCallbackData("\u2705", $"generate_link_{editablePayment.Id}"),
                                    InlineKeyboardButton.WithCallbackData("\u270f", $"edit_payment_{editablePayment.Id}")
                                };
                                await bot.SendTextMessageAsync(message.Chat.Id, 
                                    $"\ud83d\udcb5 Цена: {editablePayment.Price} \n" +
                                    $"\ud83d\udcdd Описание: {editablePayment.Description} \n " +
                                    $"\ud83d\udcde Номер телефона клиента: {editablePayment.UserPhone} \n " +
                                    $"\u2709 Почта клиента: {editablePayment.UserEmail} \n ", 
                                    replyMarkup: new InlineKeyboardMarkup(buttons),
                                    replyToMessageId: message.MessageId,
                                    cancellationToken: cancellationToken);
                            }
                            break;
                        case "price":
                            double price;
                            if (double.TryParse(message.Text, out price))
                            {
                                editablePayment.Price = price;
                                paymentsRepository.Update(editablePayment);
                                paymentsRepository.Save();
                                user.State = "default";
                                userRepository.Update(user);
                                userRepository.Save();
                                buttons = new()
                                {
                                    InlineKeyboardButton.WithCallbackData("\u2705", $"generate_link_{editablePayment.Id}"),
                                    InlineKeyboardButton.WithCallbackData("\u270f", $"edit_payment_{editablePayment.Id}")
                                };
                                await bot.SendTextMessageAsync(message.Chat.Id, 
                                    $"\ud83d\udcb5 Цена: {editablePayment.Price} \n" +
                                    $"\ud83d\udcdd Описание: {editablePayment.Description} \n " +
                                    $"\ud83d\udcde Номер телефона клиента: {editablePayment.UserPhone} \n " +
                                    $"\u2709 Почта клиента: {editablePayment.UserEmail} \n ", 
                                    replyMarkup: new InlineKeyboardMarkup(buttons),
                                    replyToMessageId: message.MessageId,
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(message.Chat.Id, "Введите цену в формате: 3,13 или 3.13",
                                    replyToMessageId: message.MessageId,
                                    cancellationToken: cancellationToken);
                            }
                            break;
                    }
                }
                break;
        }
    }

    private async Task OnCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery? callbackQuery,
        CancellationToken cancellationToken)
    {
        switch (callbackQuery?.Data)
        {
            case { } _ when new Regex(@"generate_link_[0-9]+").IsMatch(callbackQuery.Data):
                if (callbackQuery.Message != null)
                {
                    string id = callbackQuery.Data.Replace("generate_link_", "");
                    Payment? payment = paymentsRepository.FirstOrDefault(x => x.Id.ToString() == id);
                    if (payment == null)
                    {
                        await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                        break;
                    }
                    var response = await TinkoffLinkGenerator.GenerateLink(
                        price: payment.Price,
                        description: payment.Description,
                        email: payment.UserEmail,
                        phoneNumber: payment.UserPhone
                    );
                    if (response.Success)
                    {
                        await bot.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: $"Ссылка: [тык]({response.PaymentURL})",
                            cancellationToken: cancellationToken,
                            replyToMessageId: callbackQuery.Message.MessageId,
                            parseMode: ParseMode.MarkdownV2
                        );    
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: $"Ошибка: {response.ErrorCode} \n" +
                                  $"{response.Status}",
                            replyToMessageId: callbackQuery.Message.MessageId,
                            cancellationToken: cancellationToken
                        );
                    }
                    break;
                }
                await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                break;
            case { } _ when new Regex(@"edit_payment_[0-9]+_[A-Za-z]+").IsMatch(callbackQuery.Data):
                if (callbackQuery.Message != null)
                {
                    string[] data = callbackQuery.Data.Replace("edit_payment_", "").Split("_");
                    string id = data[0];
                    string changeType = data[1];
                    Payment? payment = paymentsRepository.FirstOrDefault(x => x.Id.ToString() == id);
                    if (payment == null)
                    {
                        await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                        break;
                    }

                    string parameterToChange = "";
                    switch (changeType)
                    {
                        case "price":
                            parameterToChange = "\ud83d\udcb5";
                            break;
                        case "description":
                            parameterToChange = "\ud83d\udcdd";
                            break;
                        case "email":
                            parameterToChange = "\u2709";
                            break;
                        case "phone":
                            parameterToChange = "\ud83d\udcde";
                            break;
                    }

                    Models.User? user = userRepository.FirstOrDefault(x => x.Id == callbackQuery.From.Id);
                    if (user == null)
                    {
                        user = new() { Id = callbackQuery.From.Id };
                        userRepository.Add(user);
                    }
                    user.State = $"edit_payment_{payment.Id}_{changeType}";
                    userRepository.Update(user);
                    userRepository.Save();
                    await bot.AnswerCallbackQueryAsync(callbackQuery.Id,
                        $"Введите {parameterToChange}",
                        cancellationToken: cancellationToken);
                    await bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                        cancellationToken);
                    break;
                }
                await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                break;
            case { } _ when new Regex(@"edit_payment_[0-9]+").IsMatch(callbackQuery.Data):
                if (callbackQuery.Message != null)
                {
                    string id = callbackQuery.Data.Replace("edit_payment_", "");
                    Payment? payment = paymentsRepository.FirstOrDefault(x => x.Id.ToString() == id);
                    if (payment == null)
                    {
                        await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                        break;
                    }
                    List<InlineKeyboardButton> buttons = new()
                    {
                        InlineKeyboardButton.WithCallbackData("\ud83d\udcb5", $"edit_payment_{payment.Id}_price"),
                        InlineKeyboardButton.WithCallbackData("\ud83d\udcdd", $"edit_payment_{payment.Id}_description"),
                        InlineKeyboardButton.WithCallbackData("\u2709", $"edit_payment_{payment.Id}_email"),
                        InlineKeyboardButton.WithCallbackData("\ud83d\udcde", $"edit_payment_{payment.Id}_phone"),
                    };
                    await bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                        "Что изменить?", replyMarkup: new InlineKeyboardMarkup(buttons),
                        replyToMessageId: callbackQuery.Message.MessageId,
                        cancellationToken: cancellationToken);
                    break;
                }
                await bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Ошибка, сообщение не существует", cancellationToken: cancellationToken);
                break;
        }
    }
}
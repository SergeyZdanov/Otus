
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;




var botClient = new TelegramBotClient("6468228343:AAHEF05Oeq9PLFSufh8p8tZz_OkRfjpitXE");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

var service = new TelegramBotService(botClient, cancellationToken: cts.Token);

botClient.StartReceiving(
    updateHandler: service.HandleUpdateAsync, pollingErrorHandler: service.HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();
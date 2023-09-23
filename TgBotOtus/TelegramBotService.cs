using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBotOtus;
using TgBotOtus.Models;

public class TelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UsersStateService _usersStateService;
        private Fridge fridge;
        private Diets diets;
       // private Update updateContainer = null;
        public TelegramBotService(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            _botClient = botClient;
            _usersStateService = new();
            fridge = new Fridge( _botClient, _usersStateService, cancellationToken);
            diets = new Diets(_botClient, cancellationToken);
    }
    /// 
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
 

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
        {
            /*var fridge = new Fridge(update.CallbackQuery.From.Id, _botClient, cancellationToken);*/

            if (update.CallbackQuery.Data.Contains("FridgeEdit"))
            {

                _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                string temp = update.CallbackQuery.Data;
                temp = temp.Replace("FridgeEdit", ""); 
                if (temp == "")
                { await _botClient.SendTextMessageAsync(chatId: update.CallbackQuery.From.Id, text: "Такого продукта уже нет в Вашем холодильнике", cancellationToken: cancellationToken); }
                else 
                {
                    temp = temp.Trim();
                    fridge.DeleteFridge(update.CallbackQuery.From.Id, temp, cancellationToken);
                }             
            }

            else if (update.CallbackQuery.Data.Contains("Recept"))
            {
                    _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                    var temp1 = update.CallbackQuery.Data.Replace("Recept", "");
                    diets.Recept(update.CallbackQuery.From.Id, temp1, cancellationToken);                 
            }
            else
            {
                string callQueryState = update.CallbackQuery.Data.Split(" ")[1];

                switch (callQueryState)
                {
               /*     case "Холодильник":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                        string[] massive = new string[3] { "Посмотреть", "Редактировать", "Добавить" };
                        fridge.GenerateButtons(update.CallbackQuery.From.Id, massive, "start", cancellationToken, "Вы можете просмотреть список ваших продктутов, либо редактировать этот список");
                        break;*/
                    case "Посмотреть":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                        fridge.WatchFridge(update.CallbackQuery.From.Id, cancellationToken);
                        break;
                    case "Редактировать":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                        fridge.EditFridge(update.CallbackQuery.From.Id, cancellationToken);
                        break;
                    case "Добавить":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.fridgeEdit);
                        await _botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите продукты через запятую", cancellationToken: cancellationToken);
                        break;
                  /*  case "Диеты":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                        List<string> mas =  await diets.WatchDiets();
                        diets.GenerateButtons(update.CallbackQuery.From.Id, mas, "diets Watch", cancellationToken, "Вот список имеющихся диет:");                     
                        break;*/
                    case "Watch":
                        _usersStateService.SetState(update.CallbackQuery.From.Id, UserState.NoState);
                        var temp = update.CallbackQuery.Data.Replace("diets Watch","");
                        diets.FindDiet(update.CallbackQuery.From.Id, temp, cancellationToken);
                        break;                
                }
            }

        }
        if (update.Message is null)
            return;
        Message message = update.Message;

        var fromUser = message.From;


        if (message.Text is not { } messageText)
            return;


            long chatId = message.Chat.Id;
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if (messageText.StartsWith("/"))
        {
            await HandleCommands(chatId, messageText, cancellationToken);
        }
        else
        {
            await HandleMessageByState(fromUser, botClient, chatId, messageText, cancellationToken);
        }


    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    async Task HandleCommands(long chatId, string command, CancellationToken cancellationToken)
    {
        switch (command)
        {

            case "/start":

                /*string[] buttons = new string[] { "Холодильник", "Диеты" };*/
               var  buttons =  GenerateButtons1(_botClient, chatId, cancellationToken);
               /* InlineKeyboardMarkup but = GenerateButtons(buttons, "start", 1);*/
                
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Добро пожаловать в бота, который поможет Вам выбрать, какое блюдо приготовить",
                   replyMarkup: buttons,
                    cancellationToken: cancellationToken
                );
                break;
        
            default:
                _usersStateService.SetState(chatId, UserState.NoState);
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Неизвестная команда",
                    cancellationToken: cancellationToken
                );
                break;
        }
    }

    async Task HandleMessageByState(
       User? user,
       ITelegramBotClient telegramBotClient,
       long chatId,
       string text,
       CancellationToken cancellationToken)
    {


        if (text.Contains("Холодильник") || text.Contains("Диеты"))
        {
            if (text.Contains("Холодильник"))
            {
                _usersStateService.SetState(chatId, UserState.NoState);
                string[] massive = new string[3] { "Посмотреть", "Редактировать", "Добавить" };
                 fridge.GenerateButtons(chatId, massive, "start", cancellationToken, "Здесь Вы можете посмотреть список ваших продуктов либо редактировать этот список");                
            }
            else if (text.Contains("Диеты"))
            {
                _usersStateService.SetState(chatId, UserState.NoState);
                List<string> mas = await diets.WatchDiets();
                diets.GenerateButtons(chatId, mas, "diets Watch", cancellationToken, "Вот список имеющихся диет:");
            }
        }
        else 
        {
            UserState userState = _usersStateService.GetState(chatId);
            switch (userState)
            {
                case UserState.fridgeEdit:

                    using (ApplicationContext db = new ApplicationContext())
                    {
                        text = text.Trim();
                        text = text.Replace(" ", "");
                        text =  RemovePunctuations(text);
                        while (text.Contains(",,")) 
                        {
                            text = text.Replace(",,", ",");
                        }
                        if (text[^1] == ',') 
                        {
                            text = text.Remove(text.Length,1);
                        }
                        
                        string a = chatId.ToString();
                        OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == a);//поиск в бд
                        if (us.ReservProducts is null || us.ReservProducts == "")
                        {
                            //OtusUsers tom = new OtusUsers { IdUser = a, ReservProducts = text };
                            us.ReservProducts = text.ToLower();
                            // Добавление
                            db.OtusUsers.Update(us);
                            db.SaveChanges();
                        }
                        else
                        {
                            string temp =us.ReservProducts.ToString() + "," + text.ToLower();
                            us.ReservProducts = temp;
                            db.OtusUsers.Update(us);
                            db.SaveChanges();
                        }
                    }

                    await telegramBotClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Ваш холодильник скорректирован",
                    cancellationToken: cancellationToken
                );
                    break;
                case UserState.NoState:
                    await telegramBotClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неизвестная команда",
                        cancellationToken: cancellationToken
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userState));
            }
        }
    }





    public static InlineKeyboardButton[][] GenerateButtons(IList<string> mas, string mod, int buttonsPerRow = 0)
    {
        switch (mod)
        {
            case "start":
                InlineKeyboardButton[] dStart = new InlineKeyboardButton[mas.Count];
                for (int i = 0; i < mas.Count; i++)
                {
                    dStart[i] = InlineKeyboardButton.WithCallbackData(mas[i], "start " + mas[i]);                  
                }

                

                if (buttonsPerRow == 0)
                    return new InlineKeyboardButton[][] { dStart.ToArray() };
                else
                    return dStart.Chunk(buttonsPerRow).Select(c => c.ToArray()).ToArray();
                break;   
                
            default:
                return null;
                break;
        }
    }

    public static ReplyKeyboardMarkup GenerateButtons1(ITelegramBotClient botClient, long userId, CancellationToken cancellationToken )
    {


        ReplyKeyboardMarkup inlineKeyboard = new(new[]
        {         
          new KeyboardButton[] {"Холодильник", "Диеты"}
        });
        inlineKeyboard.ResizeKeyboard = true;

        return inlineKeyboard;


    }

    public static string RemovePunctuations(string input)
    {
        return Regex.Replace(input, "[!\"#$%&'()*+./:;<=>?@\\[\\]^_`{|}~]", string.Empty);
    }

}


using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBotOtus.Models;

namespace TgBotOtus
{
    internal class Fridge
    {
       // internal  long IdUser { get; set; }
        private readonly ITelegramBotClient _botClient;
        private InlineKeyboardMarkup? inlineKeyboard;
        CancellationToken cancellationToken;
        Message m = null;
        private readonly UsersStateService _usersStateService;


        internal Fridge( ITelegramBotClient botClient, UsersStateService usersStateService, CancellationToken CancellationToken) 
        {
            // IdUser = Id;
            _usersStateService = usersStateService;
            _botClient = botClient;
            cancellationToken = CancellationToken;
           // GenerateButtons(IdUser, massive, "start", cancellationToken);
        }

        internal async void GenerateButtons(long userId, IList<string> mas, string mod, CancellationToken cancellationToken, string textMessage, int buttonsPerRow = 1)
        {
                    InlineKeyboardButton[] dStart = new InlineKeyboardButton[mas.Count];
                    for (int i = 0; i < mas.Count; i++)
                    {
                        dStart[i] = InlineKeyboardButton.WithCallbackData(mas[i], mod+ " " + mas[i]);
                    }

              
                    if (buttonsPerRow == 0)
                    {
                        inlineKeyboard = new InlineKeyboardButton[][] { dStart.ToArray() };
                        await _botClient.SendTextMessageAsync(chatId: userId, text: "Error -> GenerateButtons -> buttonsPerRow == 0", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        inlineKeyboard = dStart.Chunk(buttonsPerRow).Select(c => c.ToArray()).ToArray();             
                        //  inlineKeyboard = inlineK;
                      var a =  await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    // _botClient.EditMessageReplyMarkupAsync(userId, a.MessageId); //Удаляет кнопки
                    }            
        }

        internal async void GenerateButtonsDelete(long userId, IList<string> mas, string mod, CancellationToken cancellationToken, string textMessage, int buttonsPerRow = 1)
        {
            InlineKeyboardButton[] dStart = new InlineKeyboardButton[mas.Count];
            for (int i = 0; i < mas.Count; i++)
            {
                dStart[i] = InlineKeyboardButton.WithCallbackData(mas[i], mod + " " + mas[i]);
            }

                inlineKeyboard = dStart.Chunk(buttonsPerRow).Select(c => c.ToArray()).ToArray();
            if (m == null)
            {
                m = await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
              
            }
            else 
            {
                if (m.Text == "Вы успешно удалили один продукт") 
                {
                    _botClient.DeleteMessageAsync(m.Chat.Id, m.MessageId); //Удаляет кнопки
                }
             
                _botClient.EditMessageReplyMarkupAsync(userId, m.MessageId); //Удаляет кнопки
                m = await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
           
            }


        }
        internal async void WatchFridge(long userId, CancellationToken cancellationToken)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                string a = userId.ToString();
                OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == a);//поиск в бд
                string[] t = us.ReservProducts.Split(",");              
                string temp="Сейчас в Вашем холодильнике следующие ингридиенты:\n"; 
                foreach (string s in t) 
                {
                    temp = temp + s+"\n";
                }
                                                                                //  _usersStateService.SetState(chatId, UserState.fridge);
                if (us is null || us.ReservProducts is null || us.ReservProducts == "")
                {
                    _usersStateService.SetState(userId, UserState.fridgeEdit);
                    await _botClient.SendTextMessageAsync(
                 chatId: userId,
                text: "Холодильник пуст, милорд \nВведите список продуктов",
                cancellationToken: cancellationToken);

                }
                else
                {
                    await _botClient.SendTextMessageAsync(
                     chatId: userId,
                     text: temp,
                     cancellationToken: cancellationToken);
                }
            }

        }

        internal async void EditFridge(long userId, CancellationToken cancellationToken)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                string a = userId.ToString();
                OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == a);//поиск в бд

                if (us is null)
                {
                 //   _usersStateService.SetState(userId, UserState.fridge);
                    await _botClient.SendTextMessageAsync(
                        chatId: userId, text: "Вас нет в базе, расскажите, какие продукты у Вас имеются", cancellationToken: cancellationToken);

                    OtusUsers ou = new OtusUsers() { IdUser = a };
                    // Добавление
                    db.OtusUsers.Update(ou);
                    db.SaveChanges();
                }
                else if (us.ReservProducts is null || us.ReservProducts == "")
                {
                   // _usersStateService.SetState(chatId, UserState.fridge);
                    await _botClient.SendTextMessageAsync(
                        chatId: userId, text: "Ваш холодильник пуск, пожалуйста, введите список продуктов", cancellationToken: cancellationToken);
                }
                else
                {

                    string temp = us.ReservProducts.ToString();
                    string[] mas = temp.Split(",");
                    GenerateButtonsDelete(userId, mas, "FridgeEdit", cancellationToken: cancellationToken, "Нажмите на название продукта, чтобы удалить его из Вашего холодильника");
                  //  await _botClient.SendTextMessageAsync(chatId: userId, text: temp, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                }
            }

        }

        internal async void DeleteFridge(long Id, string dishName,  CancellationToken cancellationToken) 
        {
           // var a = update.CallbackQuery.Data;
            using (ApplicationContext db = new ApplicationContext())
            {
                var userId = Id.ToString();
                OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == userId);//поиск в бд

                if (us.ReservProducts is null || us.ReservProducts == "")
                {
                    Console.WriteLine("Ошибочка");
                }
                else
                {
                    string products = us.ReservProducts;
                    products = products.Replace(" ", "");
                    if (products.Contains(dishName+",")) 
                    {
                        products = products.Replace(dishName + ",", "");
                    }
                    else if (products.Contains(","+dishName)) 
                    {
                        products = products.Replace(","+dishName, "");
                    }
                    else 
                    {
                        products = products.Replace(dishName, "");
                    }
                    
                    string[] mas = products.Split(",");

                    //OtusUsers tom = new OtusUsers { IdUser = a, ReservProducts = text };
                    us.ReservProducts = products;
                    // Добавление
                    db.OtusUsers.Update(us);
                    db.SaveChanges();
                    if (mas.Length == 0)
                    {
                        await _botClient.SendTextMessageAsync(chatId: userId, text: "Вы удалили все продукты из холодильника", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        GenerateButtonsDelete(Id, mas, "FridgeEdit", cancellationToken, "Вы успешно удалили один продукт");
                       // await _botClient.SendTextMessageAsync(chatId: userId, text: products, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    }
                }
            }
        }

    }
}

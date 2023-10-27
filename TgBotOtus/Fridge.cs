using System.Net.NetworkInformation;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBotOtus.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TgBotOtus
{
    internal class Fridge
    {
       // internal  long IdUser { get; set; }
        private readonly ITelegramBotClient _botClient;
        private InlineKeyboardMarkup? inlineKeyboard;
        CancellationToken cancellationToken;
        Message m = null;
        Message MIng = null;
        private readonly UsersStateService _usersStateService;
        ApplicationContext db = new ApplicationContext();
        List<string> ingestsList = new List<string>();

        internal Fridge( ITelegramBotClient botClient, UsersStateService usersStateService, CancellationToken CancellationToken) 
        {
            // IdUser = Id;
            _usersStateService = usersStateService;
            _botClient = botClient;
            cancellationToken = CancellationToken;
           // GenerateButtons(IdUser, massive, "start", cancellationToken);
        }

        internal async void GenerateButtons2(long userId, IList<string> mas, string mod, CancellationToken cancellationToken, string textMessage, int buttonsPerRow = 1)
        {
            List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>(); // Создаём массив колонок
            for (int i = 0; i < 9; i=i+3)
            { // Можно использовать и foreach
                InlineKeyboardButton button = new InlineKeyboardButton(mas[i]) { CallbackData = mas[i]+ " Category" };//Создаём кнопку
                InlineKeyboardButton button2 = new InlineKeyboardButton(mas[i+1]) { CallbackData = mas[i+1] + " Category" };//Создаём кнопку
                InlineKeyboardButton button3 = new InlineKeyboardButton(mas[i + 2]) { CallbackData = mas[i + 2] + " Category" };//Создаём кнопку
                InlineKeyboardButton[] row = new InlineKeyboardButton[3] {button, button2, button3 }; // Создаём массив кнопок,в нашем случае он будет из одного элемента
                list.Add(row);//И добавляем его
            }
            var inline = new InlineKeyboardMarkup(list);//создаём клавиатуру
  
            await _botClient.SendTextMessageAsync(chatId: userId, "---------------------------------------------------",  replyMarkup: inline, cancellationToken: cancellationToken);
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
        internal async void GenerateButtonsDeleteIng(long userId, IList<string> mas, string mod, CancellationToken cancellationToken, string textMessage, int buttonsPerRow = 1)
        {
            InlineKeyboardButton[] dStart = new InlineKeyboardButton[mas.Count];
            for (int i = 0; i < mas.Count; i++)
            {
                dStart[i] = InlineKeyboardButton.WithCallbackData(mas[i], mod + " " + mas[i]);
            }

            inlineKeyboard = dStart.Chunk(buttonsPerRow).Select(c => c.ToArray()).ToArray();
            if (MIng == null)
            {
                MIng = await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
            }
            else
            {
                if (MIng.Text == "Продукты")
                {
                    _botClient.DeleteMessageAsync(MIng.Chat.Id, MIng.MessageId); //Удаляет кнопки
                }
                _botClient.EditMessageReplyMarkupAsync(userId, MIng.MessageId); //Удаляет кнопки

                if (mod != "Close") 
                {
                    MIng = await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                }
                else 
                {

                }
               
            }
        }
        internal async void WatchFridge(long userId, CancellationToken cancellationToken)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                string a = userId.ToString();
                OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == a);//поиск в бд
                string temp = "Сейчас в Вашем холодильнике следующие ингредиенты:\n";
                if (us != null) 
                {
                    string[] t = us.ReservProducts.Split(",");
                  
                    foreach (string s in t)
                    {
                        temp = temp + s + "\n";
                    }
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
        internal async void GetCategoies(long userId, CancellationToken cancellationToken) 
        {
            var temp = db.Category.Select(x=>x.CategoryDish).ToList();
            GenerateButtons2(userId, temp, "Category", cancellationToken, "Категории продуктов");

        }
        internal async void GetIngredients(long userId, string IngName, CancellationToken cancellationToken)
        {
            var temp = db.Ingredients.Where(x => x.CategoryName == IngName).Select(x=>x.Ingredient).ToList();
            temp.Add("Закрыть.");
            GenerateButtonsDeleteIng(userId, temp, "Ingredient", cancellationToken, "Продукты");
        }
        internal async void changeIngredient(long userId, string IngName, InlineKeyboardMarkup mar, CancellationToken cancellationToken)
        {
            string par1 = "ChangeIngredient";
            string par2 = "Продукты";

            var listIng = new List<string>();
            foreach (var temp in mar.InlineKeyboard.ToList()) 
            {
               foreach(var temp1 in temp) 
                {
                    listIng.Add(temp1.Text);
                }
            }
            int index = listIng.IndexOf(IngName);
            if (listIng[index].Contains("Закрыть.")) 
            {
                listIng.Clear();
                
               SetIngredients(userId, ingestsList, cancellationToken);
                ingestsList.Clear();
                par1 = "Close";
            }
            else 
            {
                if (listIng[index].Contains("✅"))
                {
                    listIng[index] = listIng[index].Replace("✅", "");
                    ingestsList.Remove(listIng[index]);
                }
                else
                {
                    ingestsList.Add(listIng[index]);
                    listIng[index] = listIng[index] + "✅";
                }
            }       
            GenerateButtonsDeleteIng(userId, listIng, par1, cancellationToken, par2);
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
        internal async void SetIngredients(long chatId, List<string> text, CancellationToken cancellationToken) 
        {
            if (text.Count != 0) 
            {
                string temp = "";
                string a = chatId.ToString();
                using (ApplicationContext db = new ApplicationContext())
                {
                    OtusUsers? us = db.OtusUsers.FirstOrDefault(x => x.IdUser == a);//поиск в бд
                    if (us.ReservProducts is null || us.ReservProducts == "")
                    {
                        foreach (var t in text)
                        {
                            temp = temp + t + ",";
                        }
                        temp = temp.Remove(temp.Length - 1, 1);
                        us.ReservProducts = temp.ToLower();
                        db.OtusUsers.Update(us);
                        db.SaveChanges();
                    }
                    else
                    {
                        foreach (var t in text)
                        {
                            temp = temp + t + ",";
                        }
                        temp = temp.Remove(temp.Length - 1, 1);
                        string temp1 = us.ReservProducts.ToString() + "," + temp.ToLower();
                        us.ReservProducts = temp1;
                        db.OtusUsers.Update(us);
                        db.SaveChanges();
                    }
                }
            }
           
        }
    }
}

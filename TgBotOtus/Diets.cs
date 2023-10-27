using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using TgBotOtus.Models;

namespace TgBotOtus
{
    internal class Diets
    {
        private readonly ITelegramBotClient _botClient;
        private InlineKeyboardMarkup? inlineKeyboard;
        CancellationToken cancellationToken;
        ApplicationContext db = new ApplicationContext();

        internal Diets(ITelegramBotClient botClient, CancellationToken CancellationToken)
        {
            // IdUser = Id;
            _botClient = botClient;
            cancellationToken = CancellationToken;
            // GenerateButtons(IdUser, massive, "start", cancellationToken);
        }


        internal async Task<List<string>>  WatchDiets()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var us = db.DietName.Select(x => x.DietaNameOrig).ToList();//поиск в бд
                return us;                 
            }
        }


        internal async void FindDiet(long userId, string dietname, CancellationToken cancellationToken) 
        {
            IQueryable<Dishes> diets;
            using (ApplicationContext db = new ApplicationContext())
            {
                var temp = db.DietName.FirstOrDefault(x => x.DietaNameOrig == dietname.Trim());
                int id = temp.Id;

                if (id != 3)
                {
                    diets = db.Dishes.Where(x => x.DietaNameId == id);
                }
                else
                {
                    diets = db.Dishes.Select(x => x);
                }
                var ingredients = db.OtusUsers.FirstOrDefault(x => x.IdUser == userId.ToString());//поиск в бд
                var mas = ingredients.ReservProducts.ToLower().Trim().Split(",");
                int prohod = 0;
                var dishes = new List<string>();
                var dishes75 = new List<string>();

                foreach (var d in diets)
                {
                    foreach (var m in mas)
                    {
                        if (d.Ingredients.Contains(m.Trim()))
                        {
                            prohod++;
                        }
                    }
                    var tempD = d.Ingredients.Split(",");
                    if (prohod == tempD.Length)
                    {
                        dishes.Add(d.DishName);
                    }
                    if (prohod >= tempD.Length * 0.6)
                    {
                        dishes75.Add(d.DishName);
                    }
                    prohod = 0;
                }
                var dish75 = dishes75.Except(dishes).ToList();

                if (dish75.Count == 0 && dishes.Count == 0)
                {
                    await _botClient.SendTextMessageAsync(chatId: userId, text: "К сожалению, мне не удалось найти подходящего блюда", cancellationToken: cancellationToken);
                }
                else
                {
                    if (dishes.Count > 0)
                        GenerateButtons(userId, dishes, "Recept", cancellationToken,
                            "Ниже представлены названия блюд,  которые Вы можете приготовить \nНажмите на кнопку, чтобы посмотреть подробную информацию:");
                    if (dish75.Count > 0)
                        GenerateButtons(userId, dish75, "Recept", cancellationToken,
                            "Ниже представлены названия блюд, для которых не хватает нескольких ингредиентов:");
                }
            }
        }
        internal async void Recept(long userId, string dishname, CancellationToken cancellationToken) 
        {
            var dish = db.Dishes.Where(x => x.DishName == dishname.Trim());
            string temp = "";
            foreach (var d in dish) 
            {
                temp = "------------------------------------------"+"\n"+"         " + d.DishName + "\n" + "Ингредиенты: " + d.Ingredients + "\n" + "Рецепт: " + d.Recept + "\n" + "------------------------------------------";
            }
            await _botClient.SendTextMessageAsync(chatId: userId, text: temp, cancellationToken: cancellationToken);

        }

        internal async void GenerateButtons(long userId, IList<string> mas, string mod, CancellationToken cancellationToken, string textMessage, int buttonsPerRow = 1)
        {
         //  textMessage = "ыва";
          //  mas[0] = "aaa";
            InlineKeyboardButton[] dStart = new InlineKeyboardButton[mas.Count];
            for (int i = 0; i < mas.Count; i++)
            {
                dStart[i] = InlineKeyboardButton.WithCallbackData(mas[i], mod + " " + mas[i]);
            }

            if (buttonsPerRow == 0)
            {
                inlineKeyboard = new InlineKeyboardButton[][] { dStart.ToArray() };
                await _botClient.SendTextMessageAsync(chatId: userId, text: "Error -> GenerateButtons -> buttonsPerRow == 0", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
            }
            else
            {
                inlineKeyboard = dStart.Chunk(buttonsPerRow).Select(c => c.ToArray()).ToArray();
                await _botClient.SendTextMessageAsync(chatId: userId, text: textMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
            }
        }
    }
}

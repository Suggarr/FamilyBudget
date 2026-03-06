using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards
{
    public static class BackKeyboard
    {
        public static ReplyKeyboardMarkup Create()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("⬅ Главное меню")
                }
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards
{
    public static class MonthKeyboard
    {
        public static InlineKeyboardMarkup Create(int year)
        {
            var months = new[]
            {
                "Январь","Февраль","Март","Апрель",
                "Май","Июнь","Июль","Август",
                "Сентябрь","Октябрь","Ноябрь","Декабрь"
            };

            var buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < months.Length; i += 3)
            {
                buttons.Add(new[]
                {
                InlineKeyboardButton.WithCallbackData(months[i], $"report:{year}:{i+1}"),
                InlineKeyboardButton.WithCallbackData(months[i+1], $"report:{year}:{i+2}"),
                InlineKeyboardButton.WithCallbackData(months[i+2], $"report:{year}:{i+3}")
            });
            }

            return new InlineKeyboardMarkup(buttons);
        }
    }
}

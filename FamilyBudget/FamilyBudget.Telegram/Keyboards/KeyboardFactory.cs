using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards
{
    public static class KeyboardFactory
    {
        public static InlineKeyboardMarkup MainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("💸 Расход", "expense"),
                InlineKeyboardButton.WithCallbackData("💰 Доход", "income")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📁 Категории", "categories")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Отчёт", "report")
            }
        });
        }
    }
}

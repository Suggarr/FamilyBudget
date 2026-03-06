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
                InlineKeyboardButton.WithCallbackData("➕ Расход", "expense"),
                InlineKeyboardButton.WithCallbackData("💵 Доход", "income")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Отчет", "report"),
                InlineKeyboardButton.WithCallbackData("🎯 Цели", "goals"),
                InlineKeyboardButton.WithCallbackData("👨‍👩‍👧 Семья", "family")
            }
        });
        }

        public static InlineKeyboardMarkup ExpenseCategories()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🍔 Еда", "cat_food"),
                    InlineKeyboardButton.WithCallbackData("🚕 Транспорт", "cat_transport")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🏠 Дом", "cat_home"),
                    InlineKeyboardButton.WithCallbackData("🎮 Развлечения", "cat_fun")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("💊 Здоровье", "cat_health"),
                    InlineKeyboardButton.WithCallbackData("📦 Другое", "cat_other")
                }
            });
        }
    }
}

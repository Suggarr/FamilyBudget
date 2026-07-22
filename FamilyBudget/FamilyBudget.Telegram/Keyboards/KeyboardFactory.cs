using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using FamilyBudget.Core.Enums;

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
                    InlineKeyboardButton.WithCallbackData("👨‍👩‍👧 Семья", "family")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📖 История", "history"),
                    InlineKeyboardButton.WithCallbackData("💳 Мой счёт", "account")
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
        public static InlineKeyboardMarkup FamilyMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📩 Пригласить", "family_invite"),
                    InlineKeyboardButton.WithCallbackData("👥 Участники", "family_members")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🏦 Копилка", "savings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🚪 Покинуть семью", "family_leave")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️ Назад", "main_menu")
                }
            });
        }

        public static InlineKeyboardMarkup ExpenseInputOptions()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✍️ Вручную", "expense_manual"),
                    InlineKeyboardButton.WithCallbackData("📷 По фото чека", "receipt_start")
                },
                new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "main_menu") }
            });
        }

        public static InlineKeyboardMarkup ReceiptCategories(Guid receiptId)
        {
            string Callback(ExpenseCategory category) => $"receipt_confirm:{receiptId}:{(int)category}";

            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🍔 Еда", Callback(ExpenseCategory.Food)),
                    InlineKeyboardButton.WithCallbackData("🚕 Транспорт", Callback(ExpenseCategory.Transport))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🏠 Дом", Callback(ExpenseCategory.Home)),
                    InlineKeyboardButton.WithCallbackData("🎮 Развлечения", Callback(ExpenseCategory.Entertainment))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("💊 Здоровье", Callback(ExpenseCategory.Health)),
                    InlineKeyboardButton.WithCallbackData("📦 Другое", Callback(ExpenseCategory.Other))
                },
                new[] { InlineKeyboardButton.WithCallbackData("✖️ Не учитывать", $"receipt_reject:{receiptId}") }
            });
        }

        public static InlineKeyboardMarkup SavingsMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("➕ Внести", "savings_contribute"),
                    InlineKeyboardButton.WithCallbackData("➖ Снять", "savings_withdraw")
                },
                new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "family") }
            });
        }
    }
}

using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class InviteHandler
    {
        private readonly FamilyInviteService _inviteService;
        private readonly IUserService _userService;

        public InviteHandler(
            FamilyInviteService inviteService,
            IUserService userService)
        {
            _inviteService = inviteService;
            _userService = userService;
        }

        // Existing Message-based API (kept for compatibility)
        //public async Task CreateInvite(
        //    ITelegramBotClient bot,
        //    Message message)
        //{
        //    var user = await _userService.GetByTelegramIdAsync(message.From!.Id);

        //    if (user == null)
        //    {
        //        await bot.SendMessage(
        //            message.Chat.Id,
        //            "❌ Вы не зарегистрированы. Используйте /start для регистрации.");
        //        return;
        //    }

        //    var code = await _inviteService.CreateInvite(user.FamilyId);

        //    var link = $"https://t.me/YOUR_BOT_NAME?start=join_{code}";

        //    await bot.SendMessage(
        //        message.Chat.Id,
        //        $"📩 Приглашение в семью:\n\n{link}\n\n⏱ Действует 24 часа");
        //}

        // New overload: handle CallbackQuery (button press) where query.From is the user
        public async Task CreateInvite(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            var chatId = query.Message?.Chat.Id ?? query.From.Id;

            var user = await _userService.GetByTelegramIdAsync(query.From.Id);

            if (user == null)
            {
                await bot.SendMessage(
                    chatId,
                    "❌ Вы не зарегистрированы. Используйте /start для регистрации.");
                return;
            }

            var code = await _inviteService.CreateInvite(user.FamilyId);

            var link = $"https://t.me/FamilyMoneyTelegrambot?start=join_{code}";

            await bot.SendMessage(
                chatId,
                $"📩 Приглашение в семью:\n\n{link}\n\n⏱ Действует 24 часа");
        }

        public async Task JoinByInvite(
            ITelegramBotClient bot,
            Message message)
        {
            var code = message.Text!
                .Replace("/start join_", "");

            var familyId = await _inviteService.UseInvite(code);

            if (familyId == null)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "❌ Ссылка недействительна или истекла");

                return;
            }

            await _userService.JoinFamilyByInvite(
                message.From!.Id,
                message.From.Username ?? "Unknown",
                familyId.Value);

            await bot.SendMessage(
                message.Chat.Id,
                "✅ Вы присоединились к семье!");
        }
    }
}
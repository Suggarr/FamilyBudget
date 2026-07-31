using FamilyBudget.Presentation.Telegram.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Presentation.Telegram.Bot
{
    public class CommandRouter
    {
        private readonly StartHandler _startHandler;

        public CommandRouter(StartHandler startHandler)
        {
            _startHandler = startHandler;
        }

        public async Task RouteAsync(ITelegramBotClient bot, Message message)
        {
            if (message.Text == "/start")
                await _startHandler.HandleAsync(bot, message);
        }
    }
}

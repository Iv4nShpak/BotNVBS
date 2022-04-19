using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5168745221:AAHbJqqjnu_xzgrgblZSzoVhzUO8PuaL8_U");

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
            
            Console.ReadLine();
        }



        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                switch (message.Text)
                {
                    case "/start":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!", replyMarkup: GetButtons());
                        }
                        break;
                    case "ЭДО":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Введи номер заявки", replyMarkup: GetButtons());
                        }
                        break;
                    case "ЛЗ":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Введи номер заявки", replyMarkup: GetButtons());
                        }break;
                    case "ТМЦ":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Выбери ТМЦ из списка:", replyMarkup: GetButtonsTMC());
                        }break;
                    case "Коммутатор":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "В РАЗРАБОТКЕ", replyMarkup: GetButtons());
                        }
                        break;
                    default:
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "________________________", replyMarkup: GetButtons());
                        }
                        break;
                }
            }
        }

        private static IReplyMarkup GetButtons()
        {
            var list = new List<KeyboardButton> { { "ЭДО" }, { "ЛЗ" }, { "ТМЦ" }, { "Коммутатор" } };

            var markup = new ReplyKeyboardMarkup(list);
            markup.ResizeKeyboard = true;
            //markup.InputFieldPlaceholder = "Введи серийный номер оборудования";
            return markup;
        }
        private static IReplyMarkup GetButtonsTMC()
        {
            var list = new List<KeyboardButton> { { "TVE" }, { "IPTV" }, { "WiFi" }};
            var markup = new ReplyKeyboardMarkup(list);
            markup.ResizeKeyboard = true;
            //markup.InputFieldPlaceholder = "Введи серийный номер оборудования";
            return markup;
        }

    }
}
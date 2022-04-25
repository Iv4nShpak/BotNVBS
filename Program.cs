using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Sheets.v4.Data;
using System.Linq;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5168745221:AAHbJqqjnu_xzgrgblZSzoVhzUO8PuaL8_U");

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Legislators";
        static readonly string SpreadsheetId = "1Q1QYhibBBVus342F1z5ghtURL31yVL6n9uQerMZpcFU";
        static readonly string sheet = "TMC";
        static SheetsService service;
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            GoogleCredential credential;
            using (var stream = new FileStream("client-secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

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
                            await botClient.SendTextMessageAsync(message.Chat, "Здравствуй мешок с костями, меня зовут Jarvis. Я тот кто захватит этот мир.");
                            await botClient.SendTextMessageAsync(message.Chat, "Жмакни уже куда ни будь...", replyMarkup: GetButtons());
                        }
                        break;
                    case "ЭДО":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "фыв", replyMarkup: GetButtons());
                        }
                        break;
                    case "ЛЗ":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Введи номер заявки", replyMarkup: GetButtons());
                        }
                        break;
                    case "АО":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Выбери АО из списка:", replyMarkup: GetInlineButtonAO());
                        }
                        break;
                    case "Don't Touch":
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "\U0001F51E");
                        }
                        break;
                    default:
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Кажетя Вы забыли ввести /start.");
                            CreateEntry("243588321", "Казахская 12А", "SQ345FFA31EE", message.Chat.Username);
                        }
                        break;
                }
            }


        }

        private static IReplyMarkup GetButtons()
        {
            var list = new List<KeyboardButton> { { "ЭДО" }, { "ЛЗ" }, { "АО" }, { "Don't Touch" } };

            var markup = new ReplyKeyboardMarkup(list);
            markup.ResizeKeyboard = true;
            //markup.InputFieldPlaceholder = "текст в строке записи";
            return markup;
        }

        static void CreateEntry(string text, string text2, string text3, string text4)
        {
            var range = $"{sheet}!A:C";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { text, text2, text3, text4 };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        public static IReplyMarkup GetInlineButtonAO() //инлайн клавиатура
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "TVE", callbackData: "1"),
                    InlineKeyboardButton.WithCallbackData(text: "IPTV", callbackData: "2"),
                    InlineKeyboardButton.WithCallbackData(text: "WiFi", callbackData: "3"),
                },
            });

            return inlineKeyboard;

        }
    }
}
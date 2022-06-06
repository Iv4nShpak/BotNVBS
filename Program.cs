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

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5168745221:AAHbJqqjnu_xzgrgblZSzoVhzUO8PuaL8_U");

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Legislators";
        static readonly string SpreadsheetId = "1Q1QYhibBBVus342F1z5ghtURL31yVL6n9uQerMZpcFU";
        static readonly string sheet = "TMC";
        static readonly string sheet2 = "LZ";
        static readonly string sheet3 = "Cable";
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
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update.Message));

            var message = update.Message;

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (message.Text.Contains("start"))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Введи ключевое слово ЭДО, ЛЗ, РК или используй клавиши:", replyMarkup: GetButtons());
                    return;
                }
                if (message.Text == "ЭДО" || message.Text == "эдо")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Введи ключевое слово ЭДО затем номер заявки, адрес и SN установленного оборудования:", replyMarkup: GetButtons());
                    return;
                }
                if (message.Text.Contains("ЭДО") && message.Text.Length > 15 || message.Text.Contains("эдо") && message.Text.Length > 15 || message.Text.Contains("Эдо") && message.Text.Length > 15)
                {
                    CreateEntryTMC(message.Text, message.Chat.Username, message.Date.ToString());
                    await botClient.SendTextMessageAsync(message.Chat, $"Добавлено: {message.Text}");
                    return;
                }
                if (message.Text == "ЛЗ" || message.Text == "лз")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Введи ключевое слово ЛЗ затем номер заявки и тип для замены в 1С:", replyMarkup: GetButtons());
                }
                if (message.Text.Contains("ЛЗ") && message.Text.Length > 15 || message.Text.Contains("лз") && message.Text.Length > 15 || message.Text.Contains("Лз") && message.Text.Length > 15)
                {
                    CreateEntryLZ(message.Text, message.Chat.Username, message.Date.ToString());
                    await botClient.SendTextMessageAsync(message.Chat, $"Добавлено: {message.Text}");
                    return;
                }
                if (message.Text == "Перерасход кабеля")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Введи ключевое слово РК затем номер заявки и колличество потраченного кабеля:", replyMarkup: GetButtons());
                }
                if (message.Text.Contains("РК") && message.Text.Length > 10 || message.Text.Contains("рк") && message.Text.Length > 10 || message.Text.Contains("Рк") && message.Text.Length > 10)
                {
                    CreateEntryCable(message.Text, message.Chat.Username, message.Date.ToString());
                    await botClient.SendTextMessageAsync(message.Chat, $"Добавлено: {message.Text}");
                    return;
                }

            }

        }

        private static IReplyMarkup GetButtons()
        {
            var list = new List<KeyboardButton> { { "ЭДО" }, { "ЛЗ" }, { "Перерасход кабеля" }};

            var markup = new ReplyKeyboardMarkup(list);
            markup.ResizeKeyboard = true;
            //markup.InputFieldPlaceholder = "Введи серийный номер оборудования";
            return markup;
        }
        //private static IReplyMarkup GetButtonsTMC()
        //{
        //    var list = new List<KeyboardButton> { { "TVE" }, { "IPTV" }, { "WiFi" } };
        //    var markup = new ReplyKeyboardMarkup(list);
        //    markup.ResizeKeyboard = true;
        //    //markup.InputFieldPlaceholder = "Введи серийный номер оборудования";
        //    return markup;
        //}

        //private static IReplyMarkup GetInlineButton() //инлайн не робит - ищи инфу
        //{
        //    return new InlineKeyboardMarkup(new InlineKeyboardButton("Опрос портов"));
        //}

        static void CreateEntryTMC(string text, string text2, string text3)
        {
            var range = $"{sheet}!A:D";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { text, text2, text3 };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        static void CreateEntryLZ(string text, string text2, string text3)
        {
            var range = $"{sheet2}!A:D";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { text, text2, text3 };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        static void CreateEntryCable(string text, string text2, string text3)
        {
            var range = $"{sheet3}!A:D";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { text, text2, text3 };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }
    }
}
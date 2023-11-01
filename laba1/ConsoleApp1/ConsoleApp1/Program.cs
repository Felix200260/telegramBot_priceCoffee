using System; // Импорт основного пространства имен .NET
using System.Collections.Generic; // Импорт пространства имен для работы со списками
using System.Threading; // Импорт пространства имен для многопоточности
using System.Threading.Tasks; // Импорт пространства имен для асинхронного программирования
using Telegram.Bot; // Импорт библиотеки для работы с Telegram Bot API
using Telegram.Bot.Types; // Импорт типов данных Telegram Bot
using Telegram.Bot.Types.ReplyMarkups; // Импорт типов для создания клавиатур
using HtmlAgilityPack; // Импорт библиотеки для парсинга HTML

namespace ConsoleApp1 // Объявление пространства имен
{
    internal class Program // Основной класс программы
    {
        static void Main(string[] args) // Основной метод выполнения программы
        {
            var botController = new BotController(); // Создание экземпляра контроллера бота
            botController.Start(); // Запуск бота
            Console.ReadLine(); // Ожидание ввода пользователя для завершения программы
        }
    }

    public class BotController // Класс контроллера бота
    {
        private readonly ITelegramBotClient _botClient; // Экземпляр клиента Telegram Bot
        private readonly CommandHandler _commandHandler; // Экземпляр обработчика команд

        public BotController() // Конструктор класса
        {
            _botClient = new TelegramBotClient("6872800343:AAHpSRa8pOKMxhVXsdgFk6f9eHrRxoSY6YI"); // Инициализация клиента бота с токеном
            _commandHandler = new CommandHandler(_botClient); // Инициализация обработчика команд
        }

        public void Start() // Метод для запуска бота
        {
            _botClient.StartReceiving(_commandHandler.HandleUpdate, HandleError); // Начать прием сообщений от бота
        }

        private static Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken token) // Обработчик ошибок
        {
            Console.WriteLine($"Error occurred: {exception.Message}"); // Вывод ошибки в консоль
            return Task.CompletedTask; // Возвращение завершенной задачи
        }
    }

    public class CommandHandler // Класс обработчика команд
    {
        private readonly ITelegramBotClient _botClient; // Экземпляр клиента Telegram Bot
        private readonly CoffeeRepository _coffeeRepository; // Экземпляр репозитория кофе

        public CommandHandler(ITelegramBotClient botClient) // Конструктор класса
        {
            _botClient = botClient; // Инициализация клиента бота
            _coffeeRepository = new CoffeeRepository(); // Инициализация репозитория кофе
        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken token) // Обработчик обновлений от бота
        {
            // Обработка текстовых сообщений
            if (update?.Message != null) // Проверка на наличие сообщения
            {
                var message = update.Message; // Получение сообщения из обновления
                if (message.Text != null) // Проверка на наличие текста в сообщении
                {
                    if (message.Text.ToLower().Contains("привет")) // Проверка на наличие слова "привет" в тексте
                    {
                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Привет!"); // Отправка приветственного сообщения
                        await SendCoffeeOptions(message.Chat.Id); // Отправка опций кофе
                    }
                }
            }
            // Обработка кнопок inline клавиатуры
            else if (update?.CallbackQuery != null) // Проверка на наличие обратного вызова
            {
                var callbackQuery = update.CallbackQuery; // Получение данных обратного вызова

                // Обработка кнопки "Для заваривания в чашке"
                if (callbackQuery.Data == "price_chashke")
                {
                    var coffeeDataList = _coffeeRepository.ScrapeCoffeeData("https://www.zavodcoffee.ru/coffees/dlya-zavarivaniya-v-chashke"); // Получение данных о кофе для данной категории
                    if (coffeeDataList.Count == 0) // Проверка на наличие данных
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "К сожалению, не удалось получить данные о кофе для данной категории."); // Отправка сообщения об ошибке
                    }
                    else
                    {
                        foreach (var coffeeData in coffeeDataList) // Перебор данных о кофе
                        {
                            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{coffeeData.Name} - {coffeeData.Price}"); // Отправка данных о кофе
                        }
                    }
                }
                // Обработка кнопки "Для турки или гейзерки"
                else if (callbackQuery.Data == "price_turki")
                {
                    var coffeeDataList = _coffeeRepository.ScrapeCoffeeData("https://www.zavodcoffee.ru/coffees/dlya-turki-ili-geyzerki"); // Получение данных о кофе для данной категории
                    if (coffeeDataList.Count == 0) // Проверка на наличие данных
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "К сожалению, не удалось получить данные о кофе для данной категории."); // Отправка сообщения об ошибке
                    }
                    else
                    {
                        foreach (var coffeeData in coffeeDataList) // Перебор данных о кофе
                        {
                            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{coffeeData.Name} - {coffeeData.Price}"); // Отправка данных о кофе
                        }
                    }
                }
                // Закомментированный код для обработки кнопки "Для эспрессо-машин"
                /*else if (callbackQuery.Data == "price_espresso")
                {
                    var coffeeDataList = _coffeeRepository.ScrapeCoffeeDataForEspressoMachines();
                    if (coffeeDataList.Count == 0)
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "К сожалению, не удалось получить данные о кофе для данной категории.");
                    }
                    else
                    {
                        foreach (var coffeeData in coffeeDataList)
                        {
                            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{coffeeData.Name} - {coffeeData.Price}");
                        }
                    }
                }*/

                // Здесь вы можете добавить другие обработчики для других категорий кофе
            }
        }

        public async Task SendCoffeeOptions(long chatId) // Метод для отправки опций кофе
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[] // Создание inline клавиатуры
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Для заваривания в чашке", "price_chashke"),
                    InlineKeyboardButton.WithCallbackData("Для турки или гейзерки", "price_turki"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Для эспрессо-машин", "price_espresso"),
                    InlineKeyboardButton.WithCallbackData("Для фильтр-кофе", "price_filter"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Дрип пакеты", "price_drip"),
                }
            });

            await _botClient.SendTextMessageAsync(chatId, "Выберите тип кофе:", replyMarkup: inlineKeyboard); // Отправка сообщения с клавиатурой
        }
    }

    public class CoffeeRepository // Класс репозитория кофе
    {
        public List<CoffeeData> ScrapeCoffeeData(string categoryLink) // Метод для получения данных о кофе из веб-сайта
        {
            var web = new HtmlWeb(); // Создание экземпляра для загрузки веб-страницы
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"; // Установка UserAgent
            var doc = web.Load(categoryLink); // Загрузка веб-страницы

            // Выборка элементов кофе на веб-странице
            var coffeeElements = doc.DocumentNode.SelectNodes("//div[@class='grid sm:grid-cols-2 sm:gap-6 lg:grid-cols-3 xl:gap-14']/div");
            var coffeeDataList = new List<CoffeeData>(); // Список для хранения данных о кофе

            if (coffeeElements == null) // Проверка на наличие элементов
            {
                return coffeeDataList; // Возврат пустого списка
            }

            foreach (var coffeeElement in coffeeElements) // Перебор элементов кофе
            {
                // Выборка имени и цены кофе
                var nameNode = coffeeElement.SelectSingleNode(".//a[contains(@class, 'inline-block float-left font-bold text-lg xl:text-xl sm:leading-loose w-10/12')]");
                var priceNode = coffeeElement.SelectSingleNode(".//p[contains(@class, 'font-medium text-xl xl:text-2xl')]");

                if (nameNode != null && priceNode != null) // Проверка на наличие имени и цены
                {
                    coffeeDataList.Add(new CoffeeData // Добавление данных о кофе в список
                    {
                        Name = nameNode.InnerText.Trim(),
                        Price = priceNode.InnerText.Trim()
                    });
                }
            }

            return coffeeDataList; // Возврат списка с данными о кофе
        }

        public class CoffeeData // Класс для хранения данных о кофе
        {
            public string Name { get; set; } // Имя кофе
            public string Price { get; set; } // Цена кофе
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

// Пространство имен для модуля логики чат-бота
namespace ChatBot
{
    // Класс, реализующий логику чат-бота, включая ответы на запросы пользователя
    public class ModuleBot
    {
        // Объект для генерации случайных чисел, используемый для выбора случайных ответов
        private Random random;
        // Хранение имени пользователя для персонализированных ответов
        private string userName;

        // Конструктор класса, принимающий имя пользователя
        public ModuleBot(string userName)
        {
            // Инициализируем генератор случайных чисел
            random = new Random();
            // Сохраняем переданное имя пользователя
            this.userName = userName;
        }

        // Асинхронный метод, возвращающий ответ бота на запрос пользователя
        public async Task<string> GetResponseAsync(string userInput)
        {
            // Получаем словарь с регулярными выражениями и соответствующими ответами
            var responses = GetRegexResponses();
            // Проверяем каждый шаблон регулярного выражения на соответствие вводу пользователя
            foreach (var pair in responses)
            {
                // Если входное сообщение соответствует регулярному выражению
                if (pair.Key.IsMatch(userInput))
                {
                    // Выбираем случайный ответ из списка и заменяем на имя пользователя
                    string response = pair.Value[random.Next(pair.Value.Count)];
                    return response.Replace("{user}", userName);
                }
            }

            // Проверяем, является ли запрос запросом погоды, используя регулярное выражение
            Regex weatherRegex = new Regex(@"погод[ауы] (?:в|во)\s+(.+)", RegexOptions.IgnoreCase);
            if (weatherRegex.IsMatch(userInput))
            {
                // Извлекаем название города из сообщения
                Match match = weatherRegex.Match(userInput);
                string city = match.Groups[1].Value.Trim();
                // Получаем информацию о погоде асинхронно
                string weather = await GetWeatherAsync(city);
                // Возвращаем ответ с погодой, обращаясь к пользователю по имени
                return $"{userName}, вот погода: {weather}";
            }

            // Проверяем, является ли запрос математическим выражением, используя регулярное выражение
            Regex calcRegex = new Regex(@"([-+]?\d*\.?\d+)\s*([+\-*/])\s*([-+]?\d*\.?\d+)", RegexOptions.IgnoreCase);
            if (calcRegex.IsMatch(userInput))
            {
                // Извлекаем числа и операцию из сообщения
                Match match = calcRegex.Match(userInput);
                double num1 = double.Parse(match.Groups[1].Value); // Первое число
                string operation = match.Groups[2].Value; // Операция
                double num2 = double.Parse(match.Groups[3].Value); // Второе число

                // Вычисляем результат математической операции
                double result = Calculate(num1, operation, num2);
                // Возвращаем результат, обращаясь к пользователю по имени
                return $"{userName}, результат: {num1} {operation} {num2} = {result}";
            }

            // Если запрос не распознан, возвращаем сообщение об ошибке с упоминанием имени пользователя
            return $"{userName}, извини, я не понимаю твой вопрос.";
        }

        // Метод для выполнения математических операций
        private double Calculate(double num1, string operation, double num2)
        {
            // Используем switch для определения типа операции и выполнения вычислений
            switch (operation)
            {
                case "+": // Сложение
                    return num1 + num2;
                case "-": // Вычитание
                    return num1 - num2;
                case "*": // Умножение
                    return num1 * num2;
                case "/": // Деление
                    // Проверяем деление на ноль
                    if (num2 == 0)
                    {
                        throw new DivideByZeroException("Деление на ноль невозможно!");
                    }
                    return num1 / num2;
                default: // Если операция не поддерживается, выбрасываем исключение
                    throw new ArgumentException("Неподдерживаемая операция!");
            }
        }

        // Метод, возвращающий словарь с регулярными выражениями и соответствующими ответами
        private Dictionary<Regex, List<string>> GetRegexResponses()
        {
            // Создаем и возвращаем словарь с регулярными выражениями и списками возможных ответов
            return new Dictionary<Regex, List<string>>
            {
                // Регулярное выражение для приветствий
                //Символ @ перед строкой (например, @"...") создаёт дословную строку (verbatim string).
                //RegexOptions.IgnoreCase (игнорирует регистр)
                //\b — граница слова (чтобы исключить совпадения внутри слов).
                { new Regex(@"\b(привет|здравствуй|хай|хелло)\b", RegexOptions.IgnoreCase),
                    new List<string> { "Привет, {user}! Чем могу помочь?", "Здравствуй, {user}!", "Хай, {user}! Как дела?" } },
                // Регулярное выражение для вопроса "Как дела?"
                //\s+ — один или более пробельных символов (пробел, табуляция).
                // \? — необязательный знак вопроса (0 или 1 раз).
                //Позволяет боту реагировать как на утверждение ("как дела"), так и на вопрос ("как дела?").
                { new Regex(@"как\s+дела\??", RegexOptions.IgnoreCase),
                    new List<string> { "У меня всё хорошо, {user}! А у тебя?", "Дела отлично, {user}!", "Прекрасно, спасибо, {user}!" } },
                // Регулярное выражение для прощаний
                { new Regex(@"\b(пока|до\s+свидания|увидимся|чао|бай-бай)\b", RegexOptions.IgnoreCase),
                    new List<string> { "До свидания, {user}! Хорошего дня!", "Пока, {user}! Ещё увидимся.", "Чао, {user}!" } },
                // Регулярное выражение для запроса возможностей бота
                { new Regex(@"(что ты умеешь|расскажи о себе|твои возможности|возможности)", RegexOptions.IgnoreCase),
                    new List<string> {
                        "{user}, вот что я могу:\n" +
                        "1. Подсказать погоду: 'погода в [город]'\n" +
                        "2. Рассказать факт: 'расскажи факт'\n" +
                        "3. Сгенерировать пароль: 'сгенерируй пароль'\n" +
                        "4. Выполнить математические операции: '5 + 3', '10 - 2', '4 * 6', '15 / 3'\n" +
                        "5. Поздороваться и попрощаться"
                    } },
                // Регулярное выражение для благодарностей
                { new Regex(@"\b(спасибо|благодарю|спс|спасиб|пасиб)\b", RegexOptions.IgnoreCase),
                    new List<string> { "Всегда пожалуйста, {user}!", "Рад помочь, {user}!", "Обращайся, {user}!" } },
                // Регулярное выражение для запроса случайного факта
                { new Regex(@"\b(факт|расскажи факт)\b", RegexOptions.IgnoreCase),
                    new List<string> { "{user}, вот факт: " + GetRandomFact() } },
                // Регулярное выражение для генерации пароля
                { new Regex(@"\b(пароль|сгенерируй пароль|создай пароль)\b", RegexOptions.IgnoreCase),
                    new List<string> { "{user}, вот твой пароль: " + GenerateRandomPassword() } }
            };
        }

        // Асинхронный метод для получения информации о погоде в указанном городе
        private async Task<string> GetWeatherAsync(string city) //Пример зависимости
        {
            try
            {
                // Указываем API-ключ для доступа к OpenWeatherMap
                string apiKey = "2b1afb180216580514c3971824e7bf02";
                // Формируем URL для запроса погоды с учетом города, API-ключа, метрических единиц и русского языка
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

                // Используем HttpClient для асинхронного запроса к API
                using (HttpClient client = new HttpClient())
                {
                    // Выполняем GET-запрос
                    HttpResponseMessage response = await client.GetAsync(url);
                    // Проверяем, успешен ли запрос
                    if (response.IsSuccessStatusCode)
                    {
                        // Читаем ответ в формате JSON
                        string json = await response.Content.ReadAsStringAsync();
                        // Десериализуем JSON в динамический объект
                        dynamic data = JsonConvert.DeserializeObject(json);
                        // Извлекаем температуру, описание погоды и название города
                        double temp = data.main.temp;
                        string description = data.weather[0].description;
                        string cityName = data.name;
                        // Возвращаем строковое описание погоды
                        return $"в городе {cityName} {temp}°C, {description}";
                    }
                    // Возвращаем сообщение об ошибке, если город не найден
                    return "не удалось найти город.";
                }
            }
            catch (Exception)
            {
                // Обрабатываем любые исключения и возвращаем сообщение об ошибке
                return "произошла ошибка при получении погоды.";
            }
        }

        // Метод для генерации случайного факта
        private string GetRandomFact()
        {
            // Массив со списком фактов
            string[] facts = new string[]
            {
                "ИВТ и ВМК не одно и то же.",
                "В космосе невозможно услышать звук.",
                "Родиону не нравится Android Studio.",
                "Моя свадьба 18 июля."
            };
            // Возвращаем случайный факт из массива
            return facts[random.Next(facts.Length)];
        }

        // Метод для генерации случайного пароля
        private string GenerateRandomPassword()
        {
            // Определяем возможные символы для пароля
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            // Создаем массив символов длиной 12 для пароля
            char[] password = new char[12];
            // Заполняем массив случайными символами из chars
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            // Возвращаем строку из сгенерированных символов
            return new string(password);
        }
    }
}
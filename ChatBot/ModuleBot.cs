using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChatBot
{
    public class ModuleBot
    {
        private Random random;

        public ModuleBot()
        {
            random = new Random();
        }

        public async Task<string> GetResponseAsync(string userInput)
        {
            // Регулярные выражения для ответов
            var responses = GetRegexResponses();
            foreach (var pair in responses)
            {
                if (pair.Key.IsMatch(userInput))
                {
                    return pair.Value[random.Next(pair.Value.Count)];
                }
            }

            // Проверка на запрос погоды
            Regex weatherRegex = new Regex(@"погод[ауы] (?:в|во)\s+(.+)", RegexOptions.IgnoreCase);
            if (weatherRegex.IsMatch(userInput))
            {
                Match match = weatherRegex.Match(userInput);
                string city = match.Groups[1].Value.Trim();
                return await GetWeatherAsync(city);
            }

            return "Извините, я не понимаю ваш вопрос.";
        }

        private Dictionary<Regex, List<string>> GetRegexResponses()
        {
            return new Dictionary<Regex, List<string>>
            {
                { new Regex(@"\b(привет|здравствуй|хай|хелло)\b", RegexOptions.IgnoreCase),
                    new List<string> { "Привет! Чем могу помочь?", "Здравствуйте!", "Приветствую вас!", "Хай! Как дела?" } },
                { new Regex(@"как\s+дела\??", RegexOptions.IgnoreCase),
                    new List<string> { "У меня всё хорошо! Спасибо, что спросили.", "Дела отлично, а у вас?", "Живу и процветаю!", "Прекрасно, спасибо!" } },
                { new Regex(@"\b(пока|до\s+свидания|увидимся|чао|бай-бай)\b", RegexOptions.IgnoreCase),
                    new List<string> { "До свидания! Хорошего дня!", "Пока! Ещё увидимся.", "Чао!", "Всего доброго!" } },
                { new Regex(@"(что ты умеешь|расскажи о себе|твои возможности|возможности)", RegexOptions.IgnoreCase),
                    new List<string> {
                        "Вот что я могу:\n" +
                        "1. Подсказать погоду: 'погода в [город]'\n" +
                        "2. Рассказать интересный факт: 'расскажи факт'\n" +
                        "3. Сгенерировать случайный пароль: 'сгенерируй пароль'\n" +
                        "4. Поздороваться: 'привет', 'здравствуй', 'хай'\n" +
                        "5. Попрощаться: 'пока', 'до свидания'\n" +
                        "6. Узнать, как у меня дела: 'как дела?'"
                    } },
                { new Regex(@"\b(спасибо|благодарю|спс|спасиб|пасиб)\b", RegexOptions.IgnoreCase),
                    new List<string> { "Всегда пожалуйста!", "Рад помочь!", "Обращайтесь!", "Не за что!" } },
                { new Regex(@"\b(факт|расскажи факт)\b", RegexOptions.IgnoreCase),
                    new List<string> { GetRandomFact() } },
                { new Regex(@"\b(пароль|сгенерируй пароль|создай пароль)\b", RegexOptions.IgnoreCase),
                    new List<string> { $"Ваш случайный пароль: {GenerateRandomPassword()}" } }
            };
        }

        private async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                string apiKey = "2b1afb180216580514c3971824e7bf02";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(json);
                        double temp = data.main.temp;
                        string description = data.weather[0].description;
                        string cityName = data.name;
                        return $"Сейчас в городе {cityName} {temp}°C, {description}";
                    }
                    else
                    {
                        return "Извините, не удалось получить данные о погоде. Проверьте название города или API-ключ.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при запросе погоды: {ex.Message}";
            }
        }

        private string GetRandomFact()
        {
            string[] facts = new string[]
            {
                "ИВТ и ВМК не 1 и то же.",
                "В космосе невозможно услышать звук.",
                "Родиону не нравится Android Studio, но пишет диплом на нем.",
                "Моя свадьба 18 июля."
            };
            return facts[random.Next(facts.Length)];
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            char[] password = new char[12];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }
    }
}

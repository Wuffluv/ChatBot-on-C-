using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Для работы с регулярными выражениями
using System.Text.RegularExpressions;

// Для HTTP-запросов
using System.Net.Http;

// Для парсинга JSON
using Newtonsoft.Json;

namespace ChatBot
{
    public partial class Form1 : Form
    {
        // Словарь для простых ответов (без погоды)
        private Dictionary<Regex, string> responses;

        public Form1()
        {
            InitializeComponent();

            // Инициализируем словарь для базовых ответов
            responses = new Dictionary<Regex, string>()
            {
                {
                    // Приветствие
                    new Regex(@"\b(привет)\b", RegexOptions.IgnoreCase),
                    "Привет! Чем могу помочь?"
                },
                {
                    // Как дела?
                    new Regex(@"как\s+дела\??", RegexOptions.IgnoreCase),
                    "У меня всё хорошо! Спасибо, что спросили."
                },
                {
                    // Прощание
                    new Regex(@"\b(пока|до\s+свидания)\b", RegexOptions.IgnoreCase),
                    "До свидания! Хорошего дня!"
                }
            };
        }

        // Обработчик нажатия кнопки button1
        private async void button1_Click(object sender, EventArgs e)
        {
            // Текст, который ввёл пользователь
            string userInput = textBox1.Text.Trim();

            // Выводим вопрос пользователя
            listBox1.Items.Add("Пользователь: " + userInput);

            // Если пользователь ничего не ввёл
            if (string.IsNullOrEmpty(userInput))
            {
                listBox1.Items.Add("Бот: Напишите что-нибудь :)");
                return;
            }

            // Флаг, отслеживающий, нашли ли мы какую-то реакцию
            bool isMatchFound = false;

            // Сначала проверяем базовые ответы (привет, пока и т.д.)
            foreach (var pair in responses)
            {
                Regex pattern = pair.Key;
                string textResponse = pair.Value;

                if (pattern.IsMatch(userInput))
                {
                    listBox1.Items.Add("Бот: " + textResponse);
                    isMatchFound = true;
                    break;
                }
            }

            // Проверим, спрашивают ли про погоду
            // Ищем слово "погода" (в любом падеже) с форматом "погода в Городе"
            // Пример: "погода в Москве", "какая погода в Казани", "подскажи погоду в Самаре" и т.п.
            // Регулярное выражение содержит группу (.+), которая захватывает название города
            Regex weatherRegex = new Regex(@"погод[ауы] (?:в|во)\s+(.+)", RegexOptions.IgnoreCase);

            // Если строка соответствует шаблону "погода в ...", то возьмём город из группы
            if (weatherRegex.IsMatch(userInput))
            {
                // Получаем совпадение
                Match match = weatherRegex.Match(userInput);

                // Название города, захваченное в группе (.+)
                string city = match.Groups[1].Value.Trim();

                // Вызываем асинхронный метод, получаем погоду
                string weatherInfo = await GetWeatherAsync(city);

                // Выводим ответ
                listBox1.Items.Add("Бот: " + weatherInfo);
                isMatchFound = true;
            }
            else
            {
                // Если просто написали "погода" без указания города
                // Например, "какая погода?" или "погода" 
                // Мы тоже можем дать ответ, но возьмём город по умолчанию (Москва)
                Regex justWeatherRegex = new Regex(@"погод[ауы]", RegexOptions.IgnoreCase);

                if (justWeatherRegex.IsMatch(userInput))
                {
                    // Город по умолчанию
                    string defaultCity = "Москва";

                    string weatherInfo = await GetWeatherAsync(defaultCity);

                    listBox1.Items.Add("Бот: " + weatherInfo);
                    isMatchFound = true;
                }
            }

            // Если не нашли ни в словаре, ни в погодных запросах
            if (!isMatchFound)
            {
                listBox1.Items.Add("Бот: Извините, я не понимаю ваш вопрос.");
            }

            // Очищаем поле ввода
            textBox1.Clear();

            // Скроллим listBox, чтобы было видно последнее сообщение
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        /// <summary>
        /// Асинхронный метод для получения погоды из OpenWeatherMap
        /// </summary>
        /// <param name="city">Название города (например, "Москва")</param>
        /// <returns>Строка с описанием погоды</returns>
        private async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                // Ваш API-ключ (зарегистрируйтесь на https://openweathermap.org/):
                string apiKey = "2b1afb180216580514c3971824e7bf02";

                // Формируем URL запроса (обратите внимание на https вместо http)
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(json);

                        // Если город найден в API
                        double temp = data.main.temp;
                        string description = data.weather[0].description;
                        string cityName = data.name;

                        return $"Сейчас в городе {cityName} {temp}°C, {description}";
                    }
                    else
                    {
                        // Например, если неверно указан город
                        // или произошла другая ошибка (401, 404 и т.д.)
                        return "Извините, не удалось получить данные о погоде. Проверьте название города или API-ключ.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Если возникло какое-то исключение, выводим текст ошибки
                return "Ошибка при запросе погоды: " + ex.Message;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Data;

namespace ChatBot
{
    public partial class ChatBot : Form
    {
        // Словарь для шаблонов (регулярных выражений) и соответствующих ответов
        private Dictionary<Regex, List<string>> responses;
        // Генератор случайных чисел для выбора ответа из списка
        private Random random;

        public ChatBot()
        {
            InitializeComponent();

            // Инициализируем генератор случайных чисел
            random = new Random();

            // Включаем обработку нажатие клавиши enter
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // Словарь для базовых ответов
            // Регулярные выражения используются для гибкости поиска совпадений!!!!
            responses = new Dictionary<Regex, List<string>>()
            {
                {
                    new Regex(@"\b(привет|здравствуй|хай|хелло)\b", RegexOptions.IgnoreCase),
                    new List<string>() { "Привет! Чем могу помочь?", "Здравствуйте!", "Приветствую вас!", "Хай! Как дела?" }
                },
                {
                    new Regex(@"как\s+дела\??", RegexOptions.IgnoreCase),
                    new List<string>() { "У меня всё хорошо! Спасибо, что спросили.", "Дела отлично, а у вас?", "Живу и процветаю!", "Прекрасно, спасибо!" }
                },
                {
                    new Regex(@"\b(пока|до\s+свидания|увидимся|чао|бай-бай)\b", RegexOptions.IgnoreCase),
                    new List<string>() { "До свидания! Хорошего дня!", "Пока! Ещё увидимся.", "Чао!", "Всего доброго!" }
                },
                {
                    new Regex(@"(что ты умеешь|расскажи о себе|твои возможности|возможности)", RegexOptions.IgnoreCase),
                    new List<string>() { "Я умею рассказывать погоду, общаться и радовать вас!", "Я бот и могу подсказать погоду или просто поговорить.", "Мои возможности пока скромны, но я стараюсь быть полезным." }
                },
                {
                    new Regex(@"\b(спасибо|благодарю|спс|спасиб|пасиб)\b", RegexOptions.IgnoreCase),
                    new List<string>() { "Всегда пожалуйста!", "Рад помочь!", "Обращайтесь!", "Не за что!" }
                }
            };
        }

        // Обработчик события KeyDown
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                button1.PerformClick();
            }
        }

        // Обработчик нажатия кнопки
        private async void button1_Click(object sender, EventArgs e)
        {
            string userInput = textBox1.Text.Trim();
            listBox1.Items.Add("Пользователь: " + userInput);

            if (string.IsNullOrEmpty(userInput))
            {
                listBox1.Items.Add("Бот: Напишите что-нибудь :)");
                return;
            }

            bool isMatchFound = false;

            // Проверка совпадений с шаблонами из словаря responses
            foreach (var pair in responses)
            {
                Regex pattern = pair.Key;
                List<string> textResponses = pair.Value;

                if (pattern.IsMatch(userInput))
                {
                    // Выбор случайного ответа из списка textResponses
                    string randomResponse = textResponses[random.Next(textResponses.Count)];
                    listBox1.Items.Add("Бот: " + randomResponse);
                    isMatchFound = true;
                    break;
                }
            }

            // Проверка на математическое выражение (калькулятор)
            Regex mathExpressionRegex = new Regex(@"^\d+(\s*[\+\-\*/]\s*\d+)+$");
            if (mathExpressionRegex.IsMatch(userInput))
            {
                try
                {
                    double result = EvaluateExpression(userInput);
                    listBox1.Items.Add($"Бот: Результат вычислений: {result}");
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add($"Бот: Ошибка при вычислении: {ex.Message}");
                }
                isMatchFound = true;
            }

            // Проверка на запрос погоды
            Regex weatherRegex = new Regex(@"погод[ауы] (?:в|во)\s+(.+)", RegexOptions.IgnoreCase);
            if (weatherRegex.IsMatch(userInput))
            {
                Match match = weatherRegex.Match(userInput);
                string city = match.Groups[1].Value.Trim();
                string weatherInfo = await GetWeatherAsync(city);
                listBox1.Items.Add("Бот: " + weatherInfo);
                isMatchFound = true;
            }

            if (!isMatchFound)
            {
                listBox1.Items.Add("Бот: Извините, я не понимаю ваш вопрос.");
            }

            textBox1.Clear();
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        // Метод для получения погоды
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
                return "Ошибка при запросе погоды: " + ex.Message;
            }
        }

        // Метод для вычисления математического выражения
        private double EvaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.CaseSensitive = false;
            return Convert.ToDouble(table.Compute(expression, string.Empty));
        }
    }
}

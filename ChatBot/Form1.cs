using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChatBot
{
    public partial class Form1 : Form
    {
        private Dictionary<Regex, List<string>> responses;
        private Random random;

        public Form1()
        {
            InitializeComponent();

            random = new Random();

           
            this.KeyPreview = true; // Перехватывает нажатия клавиш на форму
            this.KeyDown += Form1_KeyDown;

            //словарь для базовых ответов
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
            // Проверяем, нажата ли клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true; // Предотвращаем дальнейшую обработку клавиши
                e.SuppressKeyPress = true; // Блокируем стандартный "звонок" Enter
                button1.PerformClick(); // Имитируем нажатие кнопки
            }
        }

        // Обработчик нажатия кнопки
        private async void button1_Click(object sender, EventArgs e)
        {
            // Получаем текст, введённый пользователем, и удаляем лишние пробелы
            string userInput = textBox1.Text.Trim();

            // Добавляем сообщение пользователя в listBox1 для отображения диалога
            listBox1.Items.Add("Пользователь: " + userInput);

            // Если пользователь не ввёл текст, отправляем сообщение о пустом вводе
            if (string.IsNullOrEmpty(userInput))
            {
                listBox1.Items.Add("Бот: Напишите что-нибудь :)");
                return; 
            }

            // Флаг, указывающий, найден ли подходящий ответ
            bool isMatchFound = false;

            // Проверяем текст пользователя на совпадение с регулярками
            foreach (var pair in responses)
            {
                Regex pattern = pair.Key; // Шаблон регулярного выражения
                List<string> textResponses = pair.Value; // Список возможных ответов

                // Если текст пользователя соответствует шаблону
                if (pattern.IsMatch(userInput))
                {
                    // Случайный выбор ответа из списка
                    string randomResponse = textResponses[random.Next(textResponses.Count)];
                    // Добавляем ответ бота в listBox1
                    listBox1.Items.Add("Бот: " + randomResponse);
                    isMatchFound = true; // Устанавливаем флаг, что ответ найден
                    break; // Прекращаем поиск по шаблонам, так как ответ уже найден
                }
            }

            // Проверяем, спрашивает ли пользователь про погоду с указанием конкретного города
            Regex weatherRegex = new Regex(@"погод[ауы] (?:в|во)\s+(.+)", RegexOptions.IgnoreCase);

            if (weatherRegex.IsMatch(userInput))
            {
                // Извлекаем название города из текста пользователя
                Match match = weatherRegex.Match(userInput);
                string city = match.Groups[1].Value.Trim();

                // Получаем информацию о погоде для указанного города
                string weatherInfo = await GetWeatherAsync(city);

                // Ссам ответ
                listBox1.Items.Add("Бот: " + weatherInfo);
                isMatchFound = true; // Устанавливаем флаг, что ответ найден
            }
            else
            {
                // Проверяем, упомянуто ли слово "погода" без указания города
                Regex justWeatherRegex = new Regex(@"погод[ауы]", RegexOptions.IgnoreCase);

                if (justWeatherRegex.IsMatch(userInput))
                {
                    // Если город не указан, используем город Чита
                    string defaultCity = "Чита";

                    // Получаем информацию о погоде для города по умолчанию
                    string weatherInfo = await GetWeatherAsync(defaultCity);
 
                    listBox1.Items.Add("Бот: " + weatherInfo);
                    isMatchFound = true; 
                }
            }

            // Если ни один из шаблонов не подошёл, бот сообщает, что не понимает вопрос
            if (!isMatchFound)
            {
                listBox1.Items.Add("Бот: Извините, я не понимаю ваш вопрос.");
            }

            // Очищаем поле ввода, чтобы сразу ввести новый текст
            textBox1.Clear();

            // Прокручиваем listBox1 вниз, чтобы отображалось последнее сообщение
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }


        // Метод для получения погоды
        // Асинхронно запрашивает данные о погоде из API 
        private async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                // Мой API с OpenWeather
                string apiKey = "2b1afb180216580514c3971824e7bf02";

                // Формируем URL для запроса к OpenWeatherMap
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

                //HttpClient для отправки HTTP-запроса
                using (HttpClient client = new HttpClient())
                {
                    // Выполняем GET-запрос
                    HttpResponseMessage response = await client.GetAsync(url);

                    //Првоерка
                    if (response.IsSuccessStatusCode)
                    {
                        // Считываем содержимое ответа в формате JSON
                        string json = await response.Content.ReadAsStringAsync();

                        // Парсим JSON-ответ в динамический объект
                        dynamic data = JsonConvert.DeserializeObject(json);

                        // Извлекаем  из JSON-объекта:
                        //Температуру 
                        double temp = data.main.temp;

                        //  Описание погоды (description) 
                        string description = data.weather[0].description;

                        //Название города
                        string cityName = data.name;

                        //возвращаем строку с описанием  пргоды
                        return $"Сейчас в городе {cityName} {temp}°C, {description}";
                    }
                    else
                    {
                        // Если какая -либо ошибка связанная с 401 и тд
                        return "Извините, не удалось получить данные о погоде. Проверьте название города или API-ключ.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем любые исключения, возникающие при выполнении запроса
                // (например, проблемы с сетью, недоступность API и т. д.)
                return "Ошибка при запросе погоды: " + ex.Message;
            }
        }

    }
}

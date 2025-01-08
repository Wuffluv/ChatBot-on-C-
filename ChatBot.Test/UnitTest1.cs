using ChatBot;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ChatBot.Test
{
    public class Tests
    {
        private Form1 form;

        [SetUp]
        public void Setup()
        {
            // Инициализируем объект формы перед каждым тестом
            form = new Form1();
        }

        [Test]
        public async Task Test_GetWeatherAsync_CorrectCity()
        {
            string city = "Москва";
            string result = await form.GetWeatherAsync(city);
            Assert.That(result, Does.Contain("Москва"), "Метод не возвращает информацию о городе Москва.");
            Assert.That(result, Does.Contain("°C"), "Метод не возвращает температуру.");
        }

        [Test]
        public async Task Test_GetWeatherAsync_IncorrectCity()
        {
            string city = "НеизвестныйГород";
            string result = await form.GetWeatherAsync(city);
            Assert.That(result, Does.Contain("не удалось получить данные"), "Метод не обработал некорректное название города.");
        }

        [Test]
        public void Test_ParseCityFromInput()
        {
            string input = "Какая погода в Санкт-Петербурге?";
            string city = ParseCityFromInput(input);
            Assert.That(city, Is.EqualTo("Санкт-Петербург"), "Город не был корректно извлечён из строки.");
        }

        [Test]
        public void Test_ParseCityFromInput_DefaultCity()
        {
            string input = "Какая погода?";
            string city = ParseCityFromInput(input);
            Assert.That(city, Is.EqualTo("Москва"), "Город по умолчанию не установлен корректно.");
        }

     
        private string ParseCityFromInput(string input)
        {
            // Регулярное выражение для поиска города
            var regex = new System.Text.RegularExpressions.Regex(@"погод[ауы] (?:в|во)\s+(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (regex.IsMatch(input))
            {
                var match = regex.Match(input);
                return match.Groups[1].Value.Trim();
            }

            // Если город не указан, вернем "Москва" по умолчанию
            return "Москва";
        }
    }
}

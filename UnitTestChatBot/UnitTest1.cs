using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChatBot; 

namespace UnitTestChatBot
{
    [TestClass]
    public class UnitTestChatBot
    {
        private ModuleBot bot; // Экземпляр бота для тестирования

        [TestInitialize]
        public void Setup()
        {
            // Создаем экземпляр бота с тестовым именем пользователя
            bot = new ModuleBot("ТестовыйПользователь");
        }

        [TestMethod]
        public async Task TestGreetingResponse()
        {
            // Тестовые входные данные для приветствий
            string[] greetings = { "привет", "здравствуй", "хай", "хелло" };

            foreach (string greeting in greetings)
            {
                // Получаем ответ бота
                string response = await bot.GetResponseAsync(greeting);
                // Проверяем, что ответ содержит имя пользователя и приветствие
                Assert.IsTrue(response.Contains("ТестовыйПользователь") &&
                             (response.Contains("Привет") || response.Contains("Здравствуй") ||
                              response.Contains("Хай")),
                              $"Ответ на '{greeting}' не содержит ожидаемого приветствия.");
            }
        }

        [TestMethod]
        public async Task TestFarewellResponse()
        {
            // Тестовые входные данные для прощаний
            string[] farewells = { "пока", "до свидания", "чао", "бай-бай" };

            foreach (string farewell in farewells)
            {
                // Получаем ответ бота
                string response = await bot.GetResponseAsync(farewell);
                // Проверяем, что ответ содержит имя пользователя и прощание
                Assert.IsTrue(response.Contains("ТестовыйПользователь") &&
                             (response.Contains("До свидания") || response.Contains("Пока") ||
                              response.Contains("Чао")),
                              $"Ответ на '{farewell}' не содержит ожидаемого прощания.");
            }
        }

        [TestMethod]
        public async Task TestCalculatorResponse()
        {
            // Тестовые математические выражения
            string[] calculations =
            {
                "5 + 3",    // Сложение
                "10 - 2",   // Вычитание
                "4 * 6",    // Умножение
                "15 / 3"    // Деление
            };

            // Ожидаемые результаты
            double[] expectedResults = { 8, 8, 24, 5 };

            for (int i = 0; i < calculations.Length; i++)
            {
                // Получаем ответ бота
                string response = await bot.GetResponseAsync(calculations[i]);
                // Формируем ожидаемый текст ответа
                string expected = $"ТестовыйПользователь, результат: {calculations[i].Split(' ')[0]} {calculations[i].Split(' ')[1]} {calculations[i].Split(' ')[2]} = {expectedResults[i]}";
                // Проверяем, что ответ содержит ожидаемый текст
                Assert.IsTrue(response.Contains(expected),
                              $"Ответ на '{calculations[i]}' не соответствует ожидаемому: {expected}");
            }
        }

        [TestMethod]
        public async Task TestWeatherResponse()
        {
            // неправильный ответ для теста (мы не обращаемся к реальному API)
            string mockWeatherResponse = "в городе Москва 20°C, ясно";
            // Тестовый запрос погоды
            string userInput = "погода в Москва";
            // Получаем ответ бота
            string response = await bot.GetResponseAsync(userInput);

            // Проверяем, что ответ содержит имя пользователя и неправильный результат погоды
            Assert.IsTrue(response.Contains("ТестовыйПользователь") &&
                          response.Contains(mockWeatherResponse),
                          $"Ответ на '{userInput}' не соответствует ожидаемому.");
        }
       

        [TestMethod]
        public async Task TestEmptyInput()
        {
            // Тест на пустой ввод
            string response = await bot.GetResponseAsync("");
            // Проверяем, что ответ содержит имя пользователя и подсказку
            Assert.IsTrue(response.Contains("ТестовыйПользователь") &&
                          response.Contains("напиши что-нибудь :)"),
                          "Ответ на пустой ввод не соответствует ожидаемому.");
        }
    }
}
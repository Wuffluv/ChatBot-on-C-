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
            // �������������� ������ ����� ����� ������ ������
            form = new Form1();
        }

        [Test]
        public async Task Test_GetWeatherAsync_CorrectCity()
        {
            string city = "������";
            string result = await form.GetWeatherAsync(city);
            Assert.That(result, Does.Contain("������"), "����� �� ���������� ���������� � ������ ������.");
            Assert.That(result, Does.Contain("�C"), "����� �� ���������� �����������.");
        }

        [Test]
        public async Task Test_GetWeatherAsync_IncorrectCity()
        {
            string city = "����������������";
            string result = await form.GetWeatherAsync(city);
            Assert.That(result, Does.Contain("�� ������� �������� ������"), "����� �� ��������� ������������ �������� ������.");
        }

        [Test]
        public void Test_ParseCityFromInput()
        {
            string input = "����� ������ � �����-����������?";
            string city = ParseCityFromInput(input);
            Assert.That(city, Is.EqualTo("�����-���������"), "����� �� ��� ��������� �������� �� ������.");
        }

        [Test]
        public void Test_ParseCityFromInput_DefaultCity()
        {
            string input = "����� ������?";
            string city = ParseCityFromInput(input);
            Assert.That(city, Is.EqualTo("������"), "����� �� ��������� �� ���������� ���������.");
        }

     
        private string ParseCityFromInput(string input)
        {
            // ���������� ��������� ��� ������ ������
            var regex = new System.Text.RegularExpressions.Regex(@"�����[���] (?:�|��)\s+(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (regex.IsMatch(input))
            {
                var match = regex.Match(input);
                return match.Groups[1].Value.Trim();
            }

            // ���� ����� �� ������, ������ "������" �� ���������
            return "������";
        }
    }
}

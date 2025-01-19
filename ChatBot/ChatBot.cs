using System;
using System.Windows.Forms;

namespace ChatBot
{
    public partial class ChatBot : Form
    {
        private ModuleBot bot; // Экземпляр модуля с логикой бота

        public ChatBot()
        {
            InitializeComponent();

            // Инициализируем экземпляр ModuleBot
            bot = new ModuleBot();

            // Включаем обработку нажатия клавиши Enter
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
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

            // Получаем ответ от модуля
            string response = await bot.GetResponseAsync(userInput);
            listBox1.Items.Add("Бот: " + response);

            textBox1.Clear();
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }
    }
}

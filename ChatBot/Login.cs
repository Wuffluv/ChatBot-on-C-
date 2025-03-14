using System;
using System.Windows.Forms;


//Представлние
//Контроллер


// Пространство имен для формы входа в приложение
namespace ChatBot
{
    // Класс формы входа, отвечающий за получение имени пользователя
    public partial class Login : Form
    {
        // Свойство для хранения имени пользователя, доступное только для чтения извне
        public string UserName { get; private set; }

        // Конструктор формы, инициализирующий компоненты и настройку обработки клавиш
        public Login()
        {
            InitializeComponent(); 
            // Включаем обработку клавиш на уровне формы, чтобы перехватывать события клавиатуры
            this.KeyPreview = true;
            // Привязываем обработчик события KeyDown к форме для обработки нажатий клавиш
            this.KeyDown += Login_KeyDown;
        }

        // Обработчик события нажатия клавиши Enter для формы
        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, нажата ли клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                // Отмечаем, что событие обработано, чтобы избежать стандартного поведения
                e.Handled = true;
                // Подавляем стандартное действие клавиши (например, добавление новой строки)
                e.SuppressKeyPress = true;
                // Симулируем нажатие кнопки входа
                button1.PerformClick();
            }
        }

        
        // Сохраняет введенное имя и закрывает форму
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Получаем текст из текстового поля, удаляя лишние пробелы
            string input = textBox1.Text.Trim();
            // Проверяем, не пустое ли поле ввода
            if (string.IsNullOrEmpty(input))
            {
                // Выводим предупреждение, если имя не введено
                MessageBox.Show("Пожалуйста, введите ваше имя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Прерываем выполнение метода
            }

            // Сохраняем введенное имя в свойство UserName
            UserName = input;
            this.DialogResult = DialogResult.OK;
            // Закрываем форму входа
            this.Close();
        }
    }
}
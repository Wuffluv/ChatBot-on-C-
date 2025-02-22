using System;
using System.IO;
using System.Windows.Forms;

// Пространство имен для главной формы чат-бота
namespace ChatBot
{
    // Класс главной формы, управляющей взаимодействием с пользователем и ботом
    public partial class ChatBot : Form
    {
        // Экземпляр модуля логики бота
        private ModuleBot bot;
        // Имя пользователя, введенное на форме логина
        private string userName;
        // Путь к файлу, где хранится история чата
        private string chatHistoryFile = "chat_history.txt";

        // Конструктор формы, инициализирующий компоненты и логику работы бота
        public ChatBot()
        {
            InitializeComponent(); 

            // Открываем форму входа для получения имени пользователя
            using (Login loginForm = new Login())
            {
                // Показываем форму как диалоговое окно и проверяем результат
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Сохраняем имя пользователя из формы логина
                    userName = loginForm.UserName;
                }
                else
                {
                    // Если пользователь не ввел имя, завершаем работу приложения
                    Environment.Exit(0);
                }
            }

            // Создаем экземпляр бота с именем пользователя
            bot = new ModuleBot(userName);
            // Обновляем заголовок окна, приветствуя пользователя по имени
            this.Text = $"Чат-бот - Привет, {userName}!";

            // Включаем перехват событий клавиатуры на уровне формы для обработки Enter
            this.KeyPreview = true;
            // Привязываем обработчик события KeyDown к форме
            this.KeyDown += Form1_KeyDown;

            // Загружаем историю чата из файла при запуске
            LoadChatHistory();

            // Формируем приветственное сообщение для пользователя
            string welcomeMessage = $"Бот: Привет, {userName}! Чем могу помочь?";
            // Добавляем приветственное сообщение только если его нет в истории
            if (!richTextBox1.Text.Contains(welcomeMessage))
            {
                AppendMessage(welcomeMessage);
            }
        }

        // Обработчик нажатия клавиши Enter для отправки сообщения
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, нажата ли клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                // Отмечаем, что событие обработано
                e.Handled = true;
                // Подавляем стандартное действие клавиши
                e.SuppressKeyPress = true;
                // Симулируем нажатие кнопки "Ответ"
                button1.PerformClick();
            }
        }

        // Обработчик нажатия кнопки "Ответ" для отправки сообщения пользователем
        private async void button1_Click(object sender, EventArgs e)
        {
            // Получаем текст из поля ввода, удаляя лишние пробелы
            string userInput = textBox1.Text.Trim();
            // Добавляем сообщение пользователя в чат
            AppendMessage($"{userName}: {userInput}");

            // Проверяем, пустое ли сообщение
            if (string.IsNullOrEmpty(userInput))
            {
                // Выводим подсказку, если сообщение пустое
                AppendMessage($"Бот: {userName}, напиши что-нибудь :)");
                return;
            }

            // Получаем ответ от бота асинхронно
            string response = await bot.GetResponseAsync(userInput);
            // Добавляем ответ бота в чат
            AppendMessage("Бот: " + response);

            // Очищаем поле ввода
            textBox1.Clear();
            // Устанавливаем курсор в конец текста в richTextBox1
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // Прокручиваем RichTextBox к последнему сообщению
            richTextBox1.ScrollToCaret();
        }

        // Метод для добавления сообщения в чат и сохранения в файл
        private void AppendMessage(string message)
        {
            // Добавляем сообщение в RichTextBox с переносом строки
            richTextBox1.AppendText(message + "\n");
            // Сохраняем сообщение в файл истории
            SaveChatHistory(message);
        }

        // Метод для загрузки истории чата из файла
        private void LoadChatHistory()
        {
            try
            {
                // Проверяем, существует ли файл истории
                if (File.Exists(chatHistoryFile))
                {
                    // Читаем все строки из файла
                    string[] lines = File.ReadAllLines(chatHistoryFile);
                    // Добавляем каждую строку в RichTextBox
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            richTextBox1.AppendText(line + "\n");
                        }
                    }
                    // Устанавливаем курсор в конец текста
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    // Прокручиваем вниз к последнему сообщению
                    richTextBox1.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке, если загрузка не удалась
                MessageBox.Show($"Ошибка загрузки истории чата: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для сохранения истории чата в файл
        private void SaveChatHistory(string message)
        {
            try
            {
                // Проверяем, существует ли файл, и создаем его, если нет
                if (!File.Exists(chatHistoryFile))
                {
                    File.Create(chatHistoryFile).Close();
                }

                // Открываем файл в режиме добавления и записываем новое сообщение
                using (StreamWriter writer = new StreamWriter(chatHistoryFile, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке, если сохранение не удалось
                MessageBox.Show($"Ошибка сохранения истории чата: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик нажатия на пункт меню "Открыть файл"
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем диалог открытия файла
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Устанавливаем фильтр для отображения только текстовых файлов
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                // Устанавливаем заголовок окна диалога
                openFileDialog.Title = "Открыть историю чата";

                // Показываем диалог и проверяем, был ли выбран файл
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Очищаем текущую историю в richTextBox1
                        richTextBox1.Clear();

                        // Читаем и загружаем содержимое выбранного файла
                        string[] lines = File.ReadAllLines(openFileDialog.FileName);
                        foreach (string line in lines)
                        {
                            richTextBox1.AppendText(line + "\n");
                        }
                        // Устанавливаем курсор в конец текста
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        // Прокручиваем вниз к последнему сообщению
                        richTextBox1.ScrollToCaret();

                        // Сохраняем путь к открытому файлу как текущий chatHistoryFile
                        chatHistoryFile = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        // Выводим сообщение об ошибке при открытии файла
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Обработчик нажатия на пункт меню "Сохранить"
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем диалог сохранения файла
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Устанавливаем фильтр для сохранения только текстовых файлов
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                // Устанавливаем заголовок окна диалога
                saveFileDialog.Title = "Сохранить историю чата";
                // Устанавливаем имя файла по умолчанию
                saveFileDialog.FileName = "chat_history.txt";

                // Показываем диалог и проверяем, был ли выбран путь для сохранения
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Сохраняем содержимое richTextBox1 в выбранный файл
                        File.WriteAllText(saveFileDialog.FileName, richTextBox1.Text);
                        // Обновляем путь к файлу для будущих операций
                        chatHistoryFile = saveFileDialog.FileName;
                        // Уведомляем пользователя об успешном сохранении
                        MessageBox.Show("История чата успешно сохранена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Выводим сообщение об ошибке при сохранении файла
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Обработчик нажатия на пункт меню "Об авторе"
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Показываем информацию об авторе в диалоговом окне
            MessageBox.Show("Работу выполнил Студент группы ВМК-21 Рычков Р.В.", "Об авторе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
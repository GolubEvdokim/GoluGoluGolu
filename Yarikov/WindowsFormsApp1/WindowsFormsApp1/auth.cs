using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class auth: Form
    {
        private string connectionString = "Data Source=.\\SQLExpress;Initial Catalog=ProgSystem;Integrated Security=True;";
        public auth()
        {
            InitializeComponent();
        }
        private bool VerifyPassword(string enteredPassword, string savedPasswordHash)
        {
            // Извлекаем соль из сохраненного хэша
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Хэшируем введенный пароль с использованием извлеченной соли
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Сравниваем полученный хэш с сохраненным хэшем
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }

        private void auth_Load(object sender, EventArgs e)
        {

        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            // Валидация входных данных
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для получения данных пользователя по имени пользователя
                    string query = "SELECT Пароль FROM Преподаватель WHERE Логин = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Параметризация запроса для предотвращения SQL-инъекций
                        command.Parameters.AddWithValue("@Username", username);

                        // Выполняем запрос и получаем хэш пароля из базы данных
                        string savedPasswordHash = command.ExecuteScalar() as string;

                        // Проверяем, нашли ли пользователя с таким именем пользователя
                        if (savedPasswordHash != null)
                        {
                            // Проверяем введенный пароль с хэшем из базы данных
                            if (VerifyPassword(password, savedPasswordHash)) // Используем VerifyPassword из примера регистрации!
                            {
                                MessageBox.Show("Вход выполнен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Открываем новую форму или выполняем другие действия после успешного входа:
                                MainForm mainForm = new MainForm(username);
                                mainForm.Show();

                                this.DialogResult = DialogResult.OK; // Устанавливаем результат диалога для закрытия формы
                            }
                            else
                            {
                                MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пользователь с таким именем не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void registerButton_Click(object sender, EventArgs e)
        {
            
        }

        private void registerButton_Click_1(object sender, EventArgs e)
        {
            // Создаем экземпляр формы регистрации
            RegistrationForm registrationForm = new RegistrationForm();

            // Показываем форму регистрации
            if (registrationForm.ShowDialog() == DialogResult.OK)
            {
                usernameTextBox.Text = registrationForm.RegisteredUsername;
            }
        }
    }
}

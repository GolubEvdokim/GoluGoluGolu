using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class RegistrationForm: Form
    {
        private string connectionString = "Data Source=.\\SQLExpress;Initial Catalog=ProgSystem;Integrated Security=True;";

        public string RegisteredUsername { get; set; }
        public RegistrationForm()
        {
            InitializeComponent();
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // Используем регулярное выражение для проверки формата email
            // Более строгое регулярное выражение: @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$"
            //  Более простое регулярное выражение (достаточно для большинства случаев): @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // Простое, но рабочее регулярное выражение

            try
            {
                // Используем Regex.IsMatch для проверки соответствия email регулярному выражению
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false; // В случае ошибки при проверке регулярного выражения возвращаем false
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void RegistrationForm_Load(object sender, EventArgs e)
        {

        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            // Получаем данные из текстовых полей
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;
            string fam = famTextBox.Text;
            string name = nameTextBox.Text;
            string otch = otchTextBox.Text;
            string email = emailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fam) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(otch))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Хэширование пароля
            string hashedPassword = HashPassword(password);

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Пожалуйста, введите корректный email.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Вставка данных в базу данных
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для вставки данных
                    string query = "INSERT INTO Преподаватель (Familia_teacher, Imya_teacher, Otchestvo_teacher, Логин, Пароль, Email) VALUES (@Familia, @Name, @Otch, @Username, @Password, @Email)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Параметризация запроса для предотвращения SQL-инъекций
                        command.Parameters.AddWithValue("@Familia", fam);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Otch", otch);
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@Email", email);

                        // Выполнение запроса
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RegisteredUsername = usernameTextBox.Text;
                            this.DialogResult = DialogResult.OK;
                            this.Close(); // Закрыть форму после успешной регистрации
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при регистрации. Пожалуйста, попробуйте еще раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Функция для хэширования пароля
        private string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        private bool VerifyPassword(string enteredPassword, string savedPasswordHash)
        {
            // 1. Извлекаем соль из сохраненного хэша
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16); // Первые 16 байт - соль

            // 2. Хэшируем введенный пароль с использованием извлеченной соли
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20); // Получаем 20 байт хэша

            // 3. Сравниваем полученный хэш с сохраненным хэшем
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i]) // Сравниваем с 17-го байта (пропускаем соль)
                {
                    return false; // Пароли не совпадают
                }
            }

            return true; // Пароли совпадают
        }
    }
}

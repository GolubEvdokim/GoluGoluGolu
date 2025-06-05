using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class MainForm: Form
    {

        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private string tableName = "Студент";

        private string connectionString;
        public MainForm(string username)
        {
            InitializeComponent();
            connectionString = "Data Source=(local)\\SQLEXPRESS; initial Catalog=ProgSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                dataAdapter = new SqlDataAdapter();
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
            }
        }
        private void LoadData(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM " + tableName;
                    dataAdapter.SelectCommand = new SqlCommand(query, connection);

                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                    dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;

                    AdjustColumnHeaders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void AdjustColumnHeaders()
        {
            // Пример: Переименование столбцов для лучшей читаемости
            if (dataTable.Columns.Contains("Familia_student"))
            {
                dataGridView1.Columns["Familia_student"].HeaderText = "Фамилия студента";
            }
            if (dataTable.Columns.Contains("Imya_student"))
            {
                dataGridView1.Columns["Imya_student"].HeaderText = "Имя студента";
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadData(tableName);
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // создаем новое подключение
                {
                    dataAdapter.SelectCommand.Connection = connection;
                    dataAdapter.Update(dataTable);
                    MessageBox.Show("Изменения успешно сохранены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения изменений: " + ex.Message);
            }
        }

        private void btnReload_Click_1(object sender, EventArgs e)
        {
            LoadData(tableName); // Перезагрузить данные из текущей таблицы
        }

        private void btnSwitchTable_Click_1(object sender, EventArgs e)
        {
            if (tableName == "Студент")
            {
                tableName = "Оценка";
            }
            else if (tableName == "Оценка")
            {
                tableName = "Course";
            }
            else if (tableName == "Course")
            {
                tableName = "Преподаватель";
            }
            else if (tableName == "Преподаватель")
            {
                tableName = "Посещаемость";
            }
            else
            {
                tableName = "Студент";
            }


            LoadData(tableName);
        }
    }
}

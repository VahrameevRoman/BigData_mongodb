using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static private MongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<BsonDocument> users;
        List<BsonDocument> res;
        DataTable dt;
        string id;

        public Form1()
        {
            InitializeComponent();
        }

        private void Getbibl()
        {
            users = database.GetCollection<BsonDocument>("books"); // получаем коллекцию books из БД
            res = users.Find(new BsonDocument()).Sort("{author:1}").ToList(); // получаем список документов из коллекции
            dt = new DataTable(); // Создаем источник данных для dataGridView
            dt.Columns.Add("ID");
            dt.Columns.Add("Автор");
            dt.Columns.Add("Название");
            dt.Columns.Add("Год издания");
            foreach (BsonDocument doc in res)  // Добавляем в источник данные из коллекции
            {
                dt.Rows.Add(doc["_id"], doc["author"], doc["bookname"], doc["godizd"]);
            }
            dataGridView1.DataSource = dt; // отображаем данные в dataGridView
            dataGridView1.Columns["ID"].Visible = false;
        }

        // Загрузка формы
        private void Form1_Load(object sender, EventArgs e)
        {
            client = new MongoClient("mongodb://localhost:27017");
            database = client.GetDatabase("library");
            Getbibl();
        }

        private void toolbtnadd_Click(object sender, EventArgs e)
        {
            // Создаем новый документ
            BsonDocument d = new BsonDocument {
                {"author", textBox1.Text },
                {"bookname", textBox2.Text},
                {"godizd", textBox3.Text }
            };
            users.InsertOneAsync(d); // и добавляем его в коллекцию
            Getbibl();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void toolbtnedit_Click(object sender, EventArgs e)
        {
            int ind = dataGridView1.CurrentRow.Index;
            id = Convert.ToString(dt.Rows[ind][0]);
            textBox1.Text = Convert.ToString(dt.Rows[ind][1]);
            textBox2.Text = Convert.ToString(dt.Rows[ind][2]);
            textBox3.Text = Convert.ToString(dt.Rows[ind][3]);
        }

        private void toolbtnsave_Click(object sender, EventArgs e)
        {
            BsonDocument filter = new BsonDocument(); // создаем фильтр
            filter.Add("_id", ObjectId.Parse(id)); // указываем, что будет редактироваться запись с нужным значением ключа
            // создаем новый документ для замены
            BsonDocument d = new BsonDocument {
            {"author", textBox1.Text },
            {"bookname", textBox2.Text},
            {"godizd", textBox3.Text }
            };
            users.ReplaceOneAsync(filter, d); // собственно редактируем
            Getbibl();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void toolbtndel_Click(object sender, EventArgs e)
        {
            int ind = dataGridView1.CurrentRow.Index;
            id = Convert.ToString(dt.Rows[ind][0]);
            string name = "Вы действительно хотите удалить книгу " + Convert.ToString(dt.Rows[ind][2]) + "?";
            DialogResult r = MessageBox.Show(name, "Подтвердите...", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2, 
                MessageBoxOptions.DefaultDesktopOnly);
            if (r == DialogResult.No) return;
            BsonDocument filter = new BsonDocument();
            filter.Add("_id", ObjectId.Parse(id));
            users.DeleteOneAsync(filter);
            Getbibl();
        }
    }
}
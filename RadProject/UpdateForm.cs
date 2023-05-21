using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadProject
{
    public partial class UpdateForm : Form
    {

        Dictionary<string, string[]> ru_columns = new Dictionary<string, string[]>()
        {
            ["Suppliers"] = new string[] { "Поставщик", "Адрес", "Контакты" },
            ["Products"] = new string[] { "Название", "Ед. измерения", "Кр. описание" },
            ["Deliveries"] = new string[] { "Поставщик", "Дата поставки", "Стутус оплаты" },
            ["Delivery_Details"] = new string[] { "id поставки", "id товара", "quantity" },
            ["Price_Lists"] = new string[] { "id товара", "id поставщика", "Цена" }
        };

        Dictionary<string, string[]> en_columns = new Dictionary<string, string[]>()
        {
            ["Suppliers"] = new string[] { "name", "address", "contact" },
            ["Products"] = new string[] { "name", "unit", "description" },
            ["Deliveries"] = new string[] { "suppliers_id", "delivery_date", "paid" },
            ["Delivery_Details"] = new string[] { "deliveries_id", "products_id", "quantity" },
            ["Price_Lists"] = new string[] { "products_id", "suppliers_id", "price" }
        };

        Dictionary<string, int> pay_type = new Dictionary<string, int>()
        {
            ["Оплачен"] = 1,
            ["Не оплачен"] = 0
        };

        Dictionary<int, int> client_ids = new Dictionary<int, int>();
        int temp_select_client = 0;
        bool flag = true;

        TextBox[] textBoxes;
        string[] data;

        ComboBox client_cb;
        ComboBox pay_type_cb;
        DateTimePicker date_dtp;
        ComboBox paidBox;
        ComboBox col_textBoxs;

        TextBox amount_tb;

        NpgsqlConnection con;
        string table;
        int id;

        public UpdateForm(NpgsqlConnection con, string table, int id)
        {
            InitializeComponent();
            this.con = con;
            this.table = table;
            this.id = id;

            if (table == "Suppliers" || table == "Products" || table == "Price_Lists")
            {
                draw_default_view();
            }
            else if (table == "Deliveries")
            {
                draw_contract_view();
            }
            else if (table == "Delivery_Details")
            {
                draw_contract_goods_view();
            }
        }

        private void draw_default_view() {
            data = new string[en_columns[table].Length];
            string sql = "SELECT * FROM " + table + " WHERE " + table + "_id = " + id.ToString();
            //MessageBox.Show(sql);
            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);

            NpgsqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int tb_idx = 0;
                foreach (string col in en_columns[table])
                {
                    data[tb_idx] = reader[col].ToString();
                    tb_idx++;
                }
            }
            reader.Close();

            string[] cols = this.ru_columns[table];

            for (int i = 1; i <= cols.Length; i++)
            {
                var col_lable = new Label();
                col_lable.Name = "lable" + i;
                col_lable.Location = new System.Drawing.Point(20, 40 * i);
                col_lable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular);
                col_lable.Text = cols[i - 1];
                this.Controls.Add(col_lable);
            }

            textBoxes = new TextBox[cols.Length];
            for (int i = 1; i <= cols.Length; i++)
            {
                var col_textBox = new TextBox();
                col_textBox.Name = "textBox" + i;
                col_textBox.Location = new Point(120, 40 * i - 2);
                col_textBox.Size = new Size(170, 10);
                col_textBox.Text = data[i - 1];
                this.Controls.Add(col_textBox);
                textBoxes[i - 1] = col_textBox;
            }

            update_button.Location = new Point(130, 40 * cols.Length + 40);

            this.Width = 350;
            this.Height = update_button.Location.Y + 75;
        }

        private void draw_contract_view() {
            data = new string[en_columns[table].Length];
            string sql = "SELECT * FROM " + table + " WHERE " + table + "_id = " + id.ToString();
            //MessageBox.Show(sql);
            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);

            NpgsqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int tb_idx = 0;
                foreach (string col in en_columns[table])
                {
                    data[tb_idx] = reader[col].ToString();
                    tb_idx++;
                }
            }
            reader.Close();


            List<string> clients = select_all_from_client(int.Parse(data[0]));

            string[] cols = this.ru_columns[table];

            for (int i = 1; i <= cols.Length; i++)
            {
                var col_lable = new Label();
                col_lable.Name = "lable" + i;
                col_lable.Location = new System.Drawing.Point(20, 40 * i);
                col_lable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular);
                col_lable.Text = cols[i - 1];
                this.Controls.Add(col_lable);
            }

            client_cb = new ComboBox();
            client_cb.Name = "Suppliers";
            client_cb.Location = new Point(120, 40);
            client_cb.Width = 130;
            client_cb.Height = 10;
            client_cb.Text = "Поставщик";


            //clients_idx = new Dictionary<string, int>();
            int idx = 1;
            foreach (string client in clients)
            {
                client_cb.Items.Add(client);
                //clients_idx.Add(client, idx);
                idx++;
            }
            client_cb.SelectedIndex = temp_select_client;
            this.Controls.Add(client_cb);


            date_dtp = new DateTimePicker();
            date_dtp.Format = DateTimePickerFormat.Short;
            date_dtp.Name = "delivery_date";
            date_dtp.Location = new Point(120, 80);
            date_dtp.Width = 130;
            date_dtp.Height = 10;
            date_dtp.Text = data[1];
            this.Controls.Add(date_dtp);

            paidBox = new ComboBox();
            paidBox.Name = "pay_type";
            paidBox.Location = new Point(120, 120);
            paidBox.Width = 130;
            paidBox.Height = 10;
            paidBox.Text = "Статус оплаты";
            paidBox.Items.Add("Оплачен");
            paidBox.Items.Add("Не оплачен");
            this.Controls.Add(paidBox);



            update_button.Location = new Point(120, 200);

            this.Width = 300;
            this.Height = update_button.Location.Y + 75;
        }

        private List<string> select_all_from_client(int temp_id)
        {
            string sql = "SELECT * FROM Suppliers;";
            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);
            NpgsqlDataReader reader = com.ExecuteReader();
            List<string> clients = new List<string>();

            int client_id = 0;
            while (reader.Read())
            {
                if (int.Parse(reader["suppliers_id"].ToString()) == temp_id)
                    flag = false;
                if (flag)
                    temp_select_client++;
                client_ids.Add(client_id, int.Parse(reader["suppliers_id"].ToString()));
                client_id++;
                string client = (string)reader["name"];
                clients.Add(client);
            }
            reader.Close();
            return clients;
        }

        private void draw_contract_goods_view() {
            data = new string[en_columns[table].Length];
            string sql = "SELECT * FROM " + table + " WHERE " + table + "_id = " + id.ToString();
            //MessageBox.Show(sql);
            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);

            NpgsqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int tb_idx = 0;
                foreach (string col in en_columns[table])
                {
                    data[tb_idx] = reader[col].ToString();
                    tb_idx++;
                }
            }
            reader.Close();


            var amount = new Label();
            amount.Name = "quantity";
            amount.Location = new System.Drawing.Point(20, 40);
            amount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular);
            amount.Text = "Количество";
            this.Controls.Add(amount);

            amount_tb = new TextBox();
            amount_tb.Name = "quantity";
            amount_tb.Location = new Point(120, 40);
            amount_tb.Size = new Size(70, 10);
            amount_tb.Text = data[2];
            this.Controls.Add(amount_tb);

            update_button.Location = new Point(80, 80);

            this.Width = 250;
            this.Height = update_button.Location.Y + 75;
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {

        }

        private void update_button_Click(object sender, EventArgs e)
        {
            if (table == "Suppliers")
            {
                NpgsqlCommand com = new NpgsqlCommand(@"UPDATE Suppliers SET (name, address, contact) = (:name, :address, :contact) WHERE suppliers_id = :id", this.con);
                com.Parameters.AddWithValue("name", textBoxes[0].Text);
                com.Parameters.AddWithValue("address", textBoxes[1].Text);
                com.Parameters.AddWithValue("contact", textBoxes[2].Text);
                com.Parameters.AddWithValue("id", this.id);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Products")
            {
                NpgsqlCommand com = new NpgsqlCommand(@"UPDATE Products SET (name, unit, description) = (:name, :unit, :description) WHERE products_id = :id", this.con);
                com.Parameters.AddWithValue("name", textBoxes[0].Text);
                com.Parameters.AddWithValue("unit", textBoxes[1].Text);
                com.Parameters.AddWithValue("description", textBoxes[2].Text);
                com.Parameters.AddWithValue("id", this.id);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Deliveries")
            {
                NpgsqlCommand com = new NpgsqlCommand(@"UPDATE Deliveries SET (suppliers_id, delivery_date, paid) = (:suppliers_id, :delivery_date, :paid) WHERE deliveries_id = :id", this.con);
                com.Parameters.AddWithValue("suppliers_id", client_ids[client_cb.SelectedIndex]);
                com.Parameters.AddWithValue("paid", pay_type[paidBox.SelectedItem.ToString().Trim()]);
                com.Parameters.AddWithValue("id", this.id);
                NpgsqlParameter date1 = new NpgsqlParameter("delivery_date", NpgsqlTypes.NpgsqlDbType.Date);
                date1.Value = date_dtp.Value.Date;
                com.Parameters.Add(date1);
                try
                {
                    com.ExecuteNonQuery();
                }
                catch(Npgsql.PostgresException ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка изменения статуса оплаты!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Close();
            }
            else if (table == "Delivery_Details")
            {
                NpgsqlCommand com = new NpgsqlCommand(@"UPDATE Delivery_Details SET quantity = :quantity WHERE delivery_details_id = :id", this.con);
                com.Parameters.AddWithValue("quantity", int.Parse(amount_tb.Text));
                com.Parameters.AddWithValue("id", this.id);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Price_Lists")
            {
                NpgsqlCommand com = new NpgsqlCommand(@"UPDATE Price_lists SET (products_id, suppliers_id, price) = (:products_id, :suppliers_id, :price) WHERE price_lists_id = :id", this.con);
                com.Parameters.AddWithValue("products_id", int.Parse(textBoxes[0].Text));
                com.Parameters.AddWithValue("suppliers_id", int.Parse(textBoxes[1].Text));
                com.Parameters.AddWithValue("price", float.Parse(textBoxes[2].Text));
                com.Parameters.AddWithValue("id", this.id);
                com.ExecuteNonQuery();
                Close();
            }
        }
    }
}

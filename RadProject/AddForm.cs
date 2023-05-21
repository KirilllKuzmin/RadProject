using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace RadProject
{
    public partial class AddForm : Form
    {

        Dictionary<string, string[]> en_columns = new Dictionary<string, string[]>() {
            ["Suppliers"] = new string[] { "name", "address", "contact" },
            ["Products"] = new string[] { "name", "unit", "description" },
            ["Deliveries"] = new string[] { "suppliers_id", "pay_types_id", "delivery_date" },
            ["Delivery_Details"] = new string[] { "deliveries_id", "products_id", "quantity" },
            ["Price_Lists"] = new string[] { "product_id", "suppliers_id", "price" }
        };

        Dictionary<string, string[]> ru_columns = new Dictionary<string, string[]>()
        {
            ["Suppliers"] = new string[] { "Поставщик", "Адрес", "Контакты" },
            ["Products"] = new string[] { "Название", "Ед. измерения", "Кр. описание" },
            ["Deliveries"] = new string[] { "Поставщик", "Тип платежа", "Дата поставки" },
            ["Delivery_Details"] = new string[] { "id поставки", "id товара", "quantity" },
            ["Price_Lists"] = new string[] { "id товара", "id поставщика", "Цена" }
        };

        readonly Dictionary<string, int> pay_type = new Dictionary<string, int>() {
            ["Предоплата"] = 1,
            ["Постоплата"] = 2
        };

        Dictionary<int, int> client_ids = new Dictionary<int, int>();

        TextBox[] textBoxes;

        ComboBox client_cb;
        ComboBox pay_type_cb;
        ComboBox status_cb;
        DateTimePicker date_dtp;

        DataGridView products;
        DataGridView deliveries;
        TextBox amount_tb;
        Dictionary<string, int> clients_idx;

        NpgsqlConnection con;
        string table;

        public AddForm(NpgsqlConnection con, string table)
        {
            InitializeComponent();
            this.con = con;
            this.table = table;

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
                draw_contract_products_view();
            }
        }

        private void draw_default_view() {
            //var txtR = new TextBox();
            //txtR.Name = "txtR";
            //txtR.Location = new System.Drawing.Point(20, 18);
            //txtR.Size = new System.Drawing.Size(200, 15);
            //this.Controls.Add(txtR);
            string [] cols = this.ru_columns[table];

            for (int i = 1; i <= cols.Length; i++) {
                var col_lable = new Label();
                col_lable.Name = "lable" + i;
                col_lable.Location = new System.Drawing.Point(20, 40 * i);
                col_lable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular);
                col_lable.Text = cols[i - 1];
                this.Controls.Add(col_lable);
            }

            cols = this.en_columns[table];
            textBoxes = new TextBox[cols.Length];
            for (int i = 1; i <= cols.Length; i++) {
                var col_textBox = new TextBox();
                col_textBox.Name = "textBox" + i;
                col_textBox.Location = new Point(120, 40 * i - 2);
                col_textBox.Size = new Size(100, 10);
                this.Controls.Add(col_textBox);
                textBoxes[i - 1] = col_textBox;
            }

            add_button.Location = new Point(80, 40 * cols.Length + 40);
            
            this.Width = 250;
            this.Height = add_button.Location.Y + 75;
        }

        private void draw_contract_view() {
            List<string> clients = select_all_from_client();

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


            clients_idx = new Dictionary<string, int>();
            int idx = 1;
            foreach (string client in clients) {
                client_cb.Items.Add(client);
                clients_idx.Add(client, idx);
                idx++;
            }
            
            this.Controls.Add(client_cb);


            pay_type_cb = new ComboBox();
            pay_type_cb.Name = "pay_type";
            pay_type_cb.Location = new Point(120, 80);
            pay_type_cb.Width = 130;
            pay_type_cb.Height = 10;
            pay_type_cb.Text = "Тип оплаты";
            pay_type_cb.Items.Add("Предоплата");
            pay_type_cb.Items.Add("Постоплата");
            this.Controls.Add(pay_type_cb);
            


            date_dtp = new DateTimePicker();
            date_dtp.Format = DateTimePickerFormat.Short;
            date_dtp.Name = "delivery_date";
            date_dtp.Location = new Point(120, 120);
            date_dtp.Width = 130;
            date_dtp.Height = 10;
            this.Controls.Add(date_dtp);

            add_button.Location = new Point(100, 200);

            this.Width = 300;
            this.Height = add_button.Location.Y + 75;
        }

        private List<string> select_all_from_client() {
            string sql = "SELECT * FROM Suppliers;";
            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);
            NpgsqlDataReader reader = com.ExecuteReader();
            List<string> clients = new List<string>();

            int client_id = 0;
            while (reader.Read()) {
                client_ids.Add(client_id, int.Parse(reader["suppliers_id"].ToString()));
                client_id++;
                string client = (string)reader["name"];
                clients.Add(client);
            }
            reader.Close();
            return clients;
        }

        private void draw_contract_products_view() {

            deliveries = new DataGridView();
            deliveries.Location = new Point(40, 60);
            //deliveries.Size = new Size(200, 200);
            deliveries.MinimumSize = new Size(50, 50);
            deliveries.MaximumSize = new Size(500, 250);
            deliveries.BackgroundColor = System.Drawing.SystemColors.Control;
            deliveries.Name = "Deliveries";
            deliveries.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            deliveries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            deliveries.AutoSize = true;
            deliveries.BorderStyle = BorderStyle.None;
            deliveries.AllowUserToAddRows = false;
            deliveries.DataSource = select_all_from("Deliveries");
            this.Controls.Add(deliveries);


            var col_lable = new Label();
            col_lable.Name = "lable1";
            col_lable.Location = new System.Drawing.Point(270, 20);
            col_lable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12, System.Drawing.FontStyle.Regular);
            col_lable.Text = "Накладные";
            this.Controls.Add(col_lable);



            products = new DataGridView();
            products.Location = new Point(600, 60);
            //deliveries.Size = new Size(200, 200);
            products.MinimumSize = new Size(50, 50);
            products.MaximumSize = new Size(500, 250);
            products.BackgroundColor = System.Drawing.SystemColors.Control;
            products.Name = "Products";
            products.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            products.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            products.AutoSize = true;
            products.BorderStyle = BorderStyle.None;
            products.AllowUserToAddRows = false;
            products.DataSource = select_all_from("Products");
            this.Controls.Add(products);


            var col_lable2 = new Label();
            col_lable2.Name = "lable2";
            col_lable2.Location = new System.Drawing.Point(850, 20);
            col_lable2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12, System.Drawing.FontStyle.Regular);
            col_lable2.Text = "Товары";
            this.Controls.Add(col_lable2);


            var amount = new Label();
            amount.Name = "quantity";
            amount.Location = new System.Drawing.Point(40, 325);
            amount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular);
            amount.Text = "Количество";
            this.Controls.Add(amount);

            amount_tb = new TextBox();
            amount_tb.Name = "amount_tb";
            amount_tb.Location = new Point(140, 325);
            amount_tb.Size = new Size(50, 10);
            this.Controls.Add(amount_tb);


            add_button.Location = new Point(40, 360);

            this.Width = 1150;
            this.Height = add_button.Location.Y + 75;
        }

        private DataTable select_all_from(string table_name) {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql;
            if (table_name == "Deliveries")
            {
                sql = @"select d.deliveries_id, s.""name"" as Имя_поставщика, pt.""name"" as Тип_платежа, d.delivery_date дата_поставки, d.total_price Общая_тоимость
                                 from deliveries d
                                 join suppliers s
                                   on s.suppliers_id = d.suppliers_id
                                 join pay_types pt
                                   on pt.pay_types_id = d.pay_types_id;";
            }
            else if (table_name == "Products")
            {
                sql = @"select p.products_id, pl.price_lists_id, p.""name"" as Название_товара, s.""name"" as Имя_поставщика, pl.price Цена
                                 from price_lists pl
                                 join products p
                                   on pl.products_id = p.products_id
                                 join suppliers s 
                                   on s.suppliers_id = pl.suppliers_id
                                where pl.end_date is null;";
            }
            else {
                sql = "SELECT * FROM " + table_name + ";";
            } 
                
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;

        }

        private void AddFrom_Load(object sender, EventArgs e)
        {

        }

        private void add_button_Click(object sender, EventArgs e)
        {
            if (table == "Suppliers")
            {
                NpgsqlCommand com = new NpgsqlCommand("insert into Suppliers(name, address, contact) values (:name, :address, :contact)", this.con);
                com.Parameters.AddWithValue("name", textBoxes[0].Text);
                com.Parameters.AddWithValue("address", textBoxes[1].Text);
                com.Parameters.AddWithValue("contact", textBoxes[2].Text);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Products")
            {
                NpgsqlCommand com = new NpgsqlCommand("insert into Products(name, unit, description) values (:name, :unit, :description)", this.con);
                com.Parameters.AddWithValue("name", textBoxes[0].Text);
                com.Parameters.AddWithValue("unit", textBoxes[1].Text);
                com.Parameters.AddWithValue("description", textBoxes[2].Text);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Deliveries")
            {
                NpgsqlCommand com = new NpgsqlCommand("insert into Deliveries(suppliers_id, pay_types_id, delivery_date) values (:suppliers_id, :pay_types_id, :delivery_date)", this.con);
                com.Parameters.AddWithValue("suppliers_id", client_ids[client_cb.SelectedIndex]);
                com.Parameters.AddWithValue("pay_types_id", pay_type[pay_type_cb.SelectedItem.ToString().Trim()]);
                NpgsqlParameter date1 = new NpgsqlParameter("delivery_date", NpgsqlTypes.NpgsqlDbType.Date);
                date1.Value = date_dtp.Value.Date;
                com.Parameters.Add(date1);
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Delivery_Details") {
                NpgsqlCommand com = new NpgsqlCommand("insert into Delivery_Details(deliveries_id, products_id, quantity) values (:deliveries_id, :products_id, :quantity)", this.con);
                com.Parameters.AddWithValue("deliveries_id", (int)deliveries.CurrentRow.Cells["deliveries_id"].Value);
                com.Parameters.AddWithValue("products_id", (int)products.CurrentRow.Cells["products_id"].Value);
                com.Parameters.AddWithValue("quantity", int.Parse(amount_tb.Text));
                com.ExecuteNonQuery();
                Close();
            }
            else if (table == "Price_Lists")
            {
                NpgsqlCommand com = new NpgsqlCommand("insert into Price_Lists(products_id, suppliers_id, price, start_date) values (:products_id, :suppliers_id, :price, CURRENT_DATE)", this.con);
                com.Parameters.AddWithValue("products_id", int.Parse(textBoxes[0].Text));
                com.Parameters.AddWithValue("suppliers_id", int.Parse(textBoxes[1].Text));
                com.Parameters.AddWithValue("price", int.Parse(textBoxes[2].Text));
                com.ExecuteNonQuery();
                Close();
            }
        }
    }
}

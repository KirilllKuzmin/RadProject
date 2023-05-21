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

    public partial class MainForm : Form
    {

        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        NpgsqlConnection con;

        DataGridView products;

        Dictionary<string, string[]> buttons_tables = new Dictionary<string, string[]>()
        {
            ["button1"] = new string[] { "Suppliers", "Поставщики" },
            ["button2"] = new string[] { "Products", "Товары" },
            ["button3"] = new string[] { "Deliveries", "Накладные" },
            ["button4"] = new string[] { "Delivery_Details", "Детали накладной" },
            ["button5"] = new string[] { "Price_Lists", "Прайс-листы" }
        };

        string current_table = "";

        public MainForm()
        {
            InitializeComponent();
            //dataGridView1.AutoGenerateColumns = false;
            this.con = new NpgsqlConnection(
                    "Server=localhost; Port=5432; Username=postgres; Password=5555; database=StorePurchase"
                );
            con.Open();

            products = new DataGridView();
            products.Location = new Point(10, 400);
            //contracts.Size = new Size(200, 200);
            products.Size = new Size(950, 250);
            products.MaximumSize = new Size(950, 250);
            products.BackgroundColor = System.Drawing.SystemColors.Control;
            products.Name = "Products";
            products.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            products.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            products.AutoSize = true;
            products.BorderStyle = BorderStyle.None;
            products.AllowUserToAddRows = false;
        }

        private void update_view(string table)
        {
            if (this.Controls.Contains(products))
                products.Columns.Clear();

            if (current_table == "Suppliers" || current_table == "Products")
            {
                string sql;
                if (current_table == "Suppliers")
                {
                    sql = "SELECT suppliers_id, name Имя_поставщика, address Адрес, contact Контакты FROM " + current_table + ";";
                }
                else
                {
                    sql = "SELECT products_id, name Название, unit Ед_измерения, Description Описание FROM " + current_table + ";";
                }
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                dataGridView1.DataSource = dt;
                if (current_table == "Products") 
                {
                    dataGridView1.Size = new Size(950, 300);
                } 
                else 
                { 
                    dataGridView1.Size = new Size(950, 600); 
                }
            }
            else if (current_table == "Deliveries") {
                string sql = @"select d.deliveries_id, s.""name"" as Имя_поставщика, pt.""name"" as Тип_платежа, d.delivery_date as дата_платежа, d.total_price as итоговая_цена, case when d.paid = 1 then 'Оплачен' else 'Не оплачен' end as Статус_оплаты
                                 from deliveries d
                                 join suppliers s
                                   on s.suppliers_id = d.suppliers_id
                                 join pay_types pt
                                   on pt.pay_types_id = d.pay_types_id; ";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                dataGridView1.DataSource = dt;

                dataGridView1.Size = new Size(950, 300);
            }
            else if (current_table == "Delivery_Details")
            {
                string sql = @"select dd.delivery_details_id, dd.deliveries_id, p.""name"" as Название_продукта, s.""name"" as Имя_поставщика, dd.quantity Количество
                                 from delivery_details dd
                                 join products p
                                   on p.products_id = dd.products_id
                                 join deliveries d
                                   on dd.deliveries_id = d.deliveries_id
                                 join suppliers s
                                   on s.suppliers_id = d.suppliers_id; ";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                dataGridView1.DataSource = dt;
                dataGridView1.Columns["delivery_details_id"].DisplayIndex = 0;
                dataGridView1.Size = new Size(950, 600);
            }
            else if (current_table == "Price_Lists")
            {
                string sql = @"select pl.price_lists_id, p.""name"" as Название_продукта, s.""name"" as Имя_поставщика, pl.price Цена
                                 from price_lists pl
                                 join products p
                                   on pl.products_id = p.products_id
                                 join suppliers s 
                                   on s.suppliers_id = pl.suppliers_id
                                where pl.end_date is null; ";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                dataGridView1.DataSource = dt;

                dataGridView1.Size = new Size(950, 600);
            }

            dataGridView1.Sort(dataGridView1.Columns[current_table.ToLower() + "_id"], ListSortDirection.Ascending);
        }

        private DataTable select_all_products_from_contract(int id, string current_table) {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string sql;
            if (current_table == "Deliveries")
            {
                sql = @"select p.products_id, p.""name"" Имя_поставщика, dd.quantity Количество, pl.price Цена
                                 from delivery_details dd
                                 join products p
                                   on p.products_id = dd.products_id
                                 join deliveries d 
                                   on dd.deliveries_id = d.deliveries_id
                                 join price_lists pl
                                   on p.products_id = pl.products_id and pl.suppliers_id = d.suppliers_id
                                where pl.start_date >= d.delivery_date
                                  and (pl.end_date is null or pl.end_date < d.delivery_date)
                                  and dd.deliveries_id = " + id + ";";
            }
            else
            {
                sql = @"select pl.price_lists_id, s.""name"" as Имя_поставщика, pl.price Цена
                                 from price_lists pl
                                 join products p
                                   on pl.products_id = p.products_id
                                 join suppliers s 
                                   on s.suppliers_id = pl.suppliers_id
                                where pl.end_date is null
                                  and p.products_id = " + id + ";";
            }

            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.current_table = buttons_tables["button1"][0];
            table_label.Text = buttons_tables["button1"][1];
            update_view(this.current_table);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.current_table = buttons_tables["button2"][0];
            table_label.Text = buttons_tables["button2"][1];
            update_view(this.current_table);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.current_table = buttons_tables["button3"][0];
            table_label.Text = buttons_tables["button3"][1];
            update_view(this.current_table);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.current_table = buttons_tables["button4"][0];
            table_label.Text = buttons_tables["button4"][1];
            update_view(this.current_table);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.current_table = buttons_tables["button5"][0];
            table_label.Text = buttons_tables["button5"][1];
            update_view(this.current_table);
        }

        private void отчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReportForm form = new ReportForm(this.con);
            form.ShowDialog();
        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddForm form = new AddForm(this.con, this.current_table);
            form.ShowDialog();
            update_view(current_table);
        }

        private void MainFrom_Load(object sender, EventArgs e)
        {

        }

        private void изменитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int id = (int)dataGridView1.CurrentRow.Cells[current_table + "_id"].Value;
            //MessageBox.Show(id.ToString());
            UpdateForm form = new UpdateForm(this.con, this.current_table, id);
            form.ShowDialog();
            update_view(current_table);
        }

        private void удалитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int id = (int)dataGridView1.CurrentRow.Cells[current_table + "_id"].Value;
            NpgsqlCommand com = new NpgsqlCommand("DELETE FROM " + current_table + " WHERE " + current_table + "_id = " + id + ";", this.con);
            com.Parameters.AddWithValue("id", id);
            com.ExecuteNonQuery();
            update_view(current_table);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (current_table != "Deliveries" && current_table != "Products") return;

            if (this.Controls.Contains(products))
                products.Columns.Clear();

            
            products.DataSource = select_all_products_from_contract((int)dataGridView1.CurrentRow.Cells[current_table + "_id"].Value, current_table);
            this.Controls.Add(products);
        }

    }
}

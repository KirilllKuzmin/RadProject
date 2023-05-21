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
using Microsoft.Office.Interop.Excel;

namespace RadProject
{
    public partial class ReportForm : Form
    {

        string[] report_columns = new string[] { "name", "turnover", "debt", "delay" };

        string[] ru_report_columns = new string[] { "Поставщик", "Оборот", "Долг", "Просрочка" };

        List<string[]> data = new List<string[]>();

        DataSet ds = new DataSet();
        System.Data.DataTable dt = new System.Data.DataTable();
        NpgsqlConnection con;

        public ReportForm(NpgsqlConnection con)
        {
            InitializeComponent();
            this.con = con;
            InitClients();
            //this.con = new NpgsqlConnection(
            //        "Server=localhost; Port=5432; Username=postgres; Password=2305; database=RadStore"
            //    );
            //con.Open();
            //InitClients();
        }

        public void InitClients() {
            string sql = "select * from Suppliers order by suppliers_id;";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            checkedListBox1.Items.Add("Все");
            checkedListBox1.ItemCheck += ItemCheck;
            // dataGridView1.DataSource = dt;
            foreach (DataRow row in dt.Rows) {
                var cells = row.ItemArray;
                checkedListBox1.Items.Add(cells[1] + ", " + cells[2]);
            }
        }

        private void ItemCheck(object sender, ItemCheckEventArgs e) {
            CheckedListBox lb = sender as CheckedListBox;
            if (e.Index == 0)
            {
                bool flag = e.NewValue == CheckState.Checked ? true : false;
                for (int i = 1; i < lb.Items.Count; i++)
                    lb.SetItemChecked(i, flag);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           string sql = @"select s.""name"", 
	                               sum (total_price) as turnover,
		                           sum(case 
			                            when paid = 0 
				                             and pay_types_id = 2 
			                            then d.total_price 
			                            else 0 
		                                end) as debt,
		                            sum(case 
			                            when paid = 0 
				                             and current_date - delivery_date > 180 
				                             and pay_types_id = 2 
			                            then d.total_price 
			                            else 0 
		                                end) as delay
                               from deliveries d
                               join suppliers s
                                 on d.suppliers_id = s.suppliers_id
                              where d.delivery_date between :start_date and :end_date
                                and d.suppliers_id = any(:values)
                              group by d.suppliers_id, s.""name"";";

            List<int> values = new List<int>();
            foreach (int index in checkedListBox1.CheckedIndices)
                if (!(index is 0))
                    values.Add(index);

            sql = sql + " ";

            NpgsqlCommand com = new NpgsqlCommand(sql, this.con);

            com.Parameters.AddWithValue("values", values.ToArray());

            NpgsqlParameter date1 = new NpgsqlParameter("start_date", NpgsqlTypes.NpgsqlDbType.Date);
            date1.Value = dateTimePicker1.Value.Date;
            com.Parameters.Add(date1);

            NpgsqlParameter date2 = new NpgsqlParameter("end_date", NpgsqlTypes.NpgsqlDbType.Date);
            date2.Value = dateTimePicker2.Value;
            com.Parameters.Add(date2);

            NpgsqlDataReader reader = com.ExecuteReader();
            while (reader.Read()) {
                string str = "";
                //MessageBox.Show(el);
                string[] els = new string[report_columns.Length];
                for (int i = 0; i < report_columns.Length; i++) {
                    string el = reader[report_columns[i]].ToString();
                    els[i] = el;
                }
                data.Add(els);
            }
            reader.Close();

            make_report();
        }

        private void make_report() {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            string filename = ofd.FileName;
            Microsoft.Office.Interop.Excel.Application excelObject = new Microsoft.Office.Interop.Excel.Application();
            excelObject.Visible = true;
            Workbook wb = excelObject.Workbooks.Open(filename, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            Worksheet wsh = wb.Sheets[1];
            wsh.Columns.AutoFit();

            for (int i = 0; i < ru_report_columns.Length; i++) {
                wsh.Cells[1, i + 1] = ru_report_columns[i];
            }

            for (int i = 0; i < data.Count; i++) {
                Console.WriteLine(data.Count);
                for (int j = 0; j < report_columns.Length; j++) {
                    wsh.Cells[i + 2, j + 1] = data[i][j];
                }
            }
            
            wb.Save();
            wb.Close();
        }
    }
}

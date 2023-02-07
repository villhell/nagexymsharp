using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nagexym
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        

        private void SettingForm_Load(object sender, EventArgs e)
        {
            InitDataGrid();
        }

        private void InitDataGrid()
        {
            int row = 0;

            // Check列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "Check";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColCheck;

            // 名前列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "名前";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColName;

            // Twitter列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "Twitter";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColTwitter;

            // Namespace列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "ネームスペース";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColNamespace;

            // Address列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "アドレス";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColAddress;

            // XYM列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "XYM";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColXym;

            // メッセージ列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[row].Cells[0].Value = "メッセージ";
            dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.ExcelColMessage;


            row = 0;

            // 最大手数料列
            dataGridView2.Rows.Add();
            dataGridView2.Rows[row].Cells[0].Value = "最大手数料";
            dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.MaxFee;

            // トランザクション数列
            dataGridView2.Rows.Add();
            dataGridView2.Rows[row].Cells[0].Value = "トランザクション数";
            dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.InnerTxCount;

            // アグリゲート有無
            dataGridView2.Rows.Add();
            dataGridView2.Rows[row].Cells[0].Value = "アグリゲート有無";
            dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.IsAggrigate;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("初期値を設定します。よろしいですか？", Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                int row = 0;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColCheck;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColName;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColTwitter;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColNamespace;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColAddress;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColXym;
                dataGridView1.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultExcelColMessage;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("初期値を設定します。よろしいですか？", Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                int row = 0;
                dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultMaxFee;
                dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultInnerTxCount;
                dataGridView2.Rows[row++].Cells[1].Value = Properties.Settings.Default.DefaultIsAggrigate;

            }
        }
    }
}

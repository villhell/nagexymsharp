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
        #region ヘッダー列番号
        private const int COL_NAME = 0;
        private const int COL_VALUE = 1;
        #endregion

        #region Excel設定行番号
        private const int ROW_CHECK = 0;
        private const int ROW_NAME = 1;
        private const int ROW_TWITTER = 2;
        private const int ROW_NAMESPACE = 3;
        private const int ROW_ADDRESS = 4;
        private const int ROW_XYM = 5;
        private const int ROW_MESSAGE = 6;
        #endregion

        #region 設定行番号
        private const int ROW_FEE = 0;
        private const int ROW_TX_COUNT = 1;
        private const int ROW_IS_AGGRIGATE = 2;
        #endregion

        public SettingForm()
        {
            InitializeComponent();

            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
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
            dataGridView1.Rows[ROW_CHECK].Cells[COL_NAME].Value = "Check";
            dataGridView1.Rows[ROW_CHECK].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColCheck;

            // 名前列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_NAME].Cells[COL_NAME].Value = "名前";
            dataGridView1.Rows[ROW_NAME].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColName;

            // Twitter列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_TWITTER].Cells[COL_NAME].Value = "Twitter";
            dataGridView1.Rows[ROW_TWITTER].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColTwitter;

            // Namespace列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_NAMESPACE].Cells[COL_NAME].Value = "ネームスペース";
            dataGridView1.Rows[ROW_NAMESPACE].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColNamespace;

            // Address列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_ADDRESS].Cells[COL_NAME].Value = "アドレス";
            dataGridView1.Rows[ROW_ADDRESS].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColAddress;

            // XYM列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_XYM].Cells[COL_NAME].Value = "XYM";
            dataGridView1.Rows[ROW_XYM].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColXym;

            // メッセージ列
            dataGridView1.Rows.Add();
            dataGridView1.Rows[ROW_MESSAGE].Cells[COL_NAME].Value = "メッセージ";
            dataGridView1.Rows[ROW_MESSAGE].Cells[COL_VALUE].Value = Properties.Settings.Default.ExcelColMessage;


            // 最大手数料列
            dataGridView2.Rows.Add();
            dataGridView2.Rows[ROW_FEE].Cells[COL_NAME].Value = "最大手数料";
            dataGridView2.Rows[ROW_FEE].Cells[COL_VALUE].Value = Properties.Settings.Default.MaxFee;

            // トランザクション数列
            dataGridView2.Rows.Add();
            dataGridView2.Rows[ROW_TX_COUNT].Cells[COL_NAME].Value = "トランザクション数";
            dataGridView2.Rows[ROW_TX_COUNT].Cells[COL_VALUE].Value = Properties.Settings.Default.InnerTxCount;

            // アグリゲート有無
            dataGridView2.Rows.Add();
            dataGridView2.Rows[ROW_IS_AGGRIGATE].Cells[COL_NAME].Value = "アグリゲート有無";
            dataGridView2.Rows[ROW_IS_AGGRIGATE].Cells[COL_VALUE].Value = Properties.Settings.Default.IsAggrigate;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("初期値を設定します。よろしいですか？", Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                dataGridView1.Rows[ROW_CHECK].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColCheck;
                dataGridView1.Rows[ROW_NAME].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColName;
                dataGridView1.Rows[ROW_TWITTER].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColTwitter;
                dataGridView1.Rows[ROW_NAMESPACE].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColNamespace;
                dataGridView1.Rows[ROW_ADDRESS].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColAddress;
                dataGridView1.Rows[ROW_XYM].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColXym;
                dataGridView1.Rows[ROW_MESSAGE].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultExcelColMessage;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("初期値を設定します。よろしいですか？", Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                dataGridView2.Rows[ROW_FEE].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultMaxFee;
                dataGridView2.Rows[ROW_TX_COUNT].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultInnerTxCount;
                dataGridView2.Rows[ROW_IS_AGGRIGATE].Cells[COL_VALUE].Value = Properties.Settings.Default.DefaultIsAggrigate;

            }
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            // Check列
            Properties.Settings.Default.ExcelColCheck = dataGridView1.Rows[ROW_CHECK].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_CHECK].Cells[COL_VALUE].Value : 0;

            // 名前列
            Properties.Settings.Default.ExcelColName = dataGridView1.Rows[ROW_NAME].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_NAME].Cells[COL_VALUE].Value : 0;

            // Twitter列
            Properties.Settings.Default.ExcelColTwitter = dataGridView1.Rows[ROW_TWITTER].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_TWITTER].Cells[COL_VALUE].Value : 0;

            // Namespace列
            Properties.Settings.Default.ExcelColNamespace = dataGridView1.Rows[ROW_NAMESPACE].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_NAMESPACE].Cells[COL_VALUE].Value : 0;

            // Address列
            Properties.Settings.Default.ExcelColAddress = dataGridView1.Rows[ROW_ADDRESS].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_ADDRESS].Cells[COL_VALUE].Value : 0;

            // XYM列
            Properties.Settings.Default.ExcelColXym = dataGridView1.Rows[ROW_XYM].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_XYM].Cells[COL_VALUE].Value : 0;

            // メッセージ列
            Properties.Settings.Default.ExcelColMessage = dataGridView1.Rows[ROW_MESSAGE].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_MESSAGE].Cells[COL_VALUE].Value : 0;

            // 最大手数料列
            Properties.Settings.Default.MaxFee = dataGridView1.Rows[ROW_FEE].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_FEE].Cells[COL_VALUE].Value : 0;

            // トランザクション数列
            Properties.Settings.Default.InnerTxCount = dataGridView1.Rows[ROW_TX_COUNT].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_TX_COUNT].Cells[COL_VALUE].Value : 0;

            // アグリゲート有無
            Properties.Settings.Default.IsAggrigate = dataGridView1.Rows[ROW_IS_AGGRIGATE].Cells[COL_VALUE].Value != null ? (int)dataGridView1.Rows[ROW_IS_AGGRIGATE].Cells[COL_VALUE].Value : 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

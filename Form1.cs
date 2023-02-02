using CatSdk.CryptoTypes;
using CatSdk.Facade;
using CatSdk.Nem;
using CatSdk.Symbol;
using CatSdk.Symbol.Factory;
using CatSdk.Utils;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using RestSharp;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace nagexym
{
    public partial class Form1 : Form
    {
        private const int GRID_COL_ACCOUNT = 0;
        private const int GRID_COL_TWITTER = 1;
        private const int GRID_COL_NAMESPACE = 2;
        private const int GRID_COL_ADDRESS = 3;

        private const int EXCEL_COL_ACCOUNT = 1;
        private const int EXCEL_COL_TWITTER = 2;
        private const int EXCEL_COL_ADDRESS = 5;

        /// <summary>
        /// �A�h���X�̕�����
        /// </summary>
        private const int ADDRESS_LENGTH = 39;

        public Form1()
        {
            InitializeComponent();
            txtFrom.Text= "TDMIACBKMY4GHHAKYKBPGHZRQNBVSLZ3JMNUDZY";
            txtPrivateKey.Text= "";
        }

        /// <summary>
        /// �L�����Z���{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ���M�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text)) MessageBox.Show("�󗓂�����܂��B");
            if (string.IsNullOrEmpty(txtPrivateKey.Text)) MessageBox.Show("�󗓂�����܂��B");

            Send();


        }

        private async void Send()
        {
            //var facade = new SymbolFacade(Network.TestNet);
            //var privateKey = new PrivateKey(txtPrivateKey.Text);
            //var keyPair = new KeyPair(privateKey);

            //var tx = new TransferTransactionV1
            //{
            //    Network = NetworkType.TESTNET,
            //    RecipientAddress = new UnresolvedAddress(Converter.StringToAddress(txtTo.Text)),
            //    Mosaics = new UnresolvedMosaic[]
            //    {
            //        new()
            //        {
            //            //MosaicId = new UnresolvedMosaicId(0x3A8416DB2D53B6C8),
            //            MosaicId = new UnresolvedMosaicId(0x72C0212E67A08BCE),
            //            Amount = new Amount(1000000)
            //        },
            //    },
            //    SignerPublicKey = keyPair.PublicKey,
            //    Message = Converter.Utf8ToPlainMessage("Hello, Symbol"),
            //    Fee = new Amount(1000000),
            //    Deadline = new Timestamp(facade.Network.FromDatetime<NetworkTimestamp>(DateTime.UtcNow).AddHours(2).Timestamp)
            //};

            //var signature = facade.SignTransaction(keyPair, tx);
            //var payload = TransactionsFactory.AttachSignature(tx, signature);
            //var signed = TransactionsFactory.AttachSignatureTransaction(tx, signature);
            //var hash = facade.HashTransaction(TransactionsFactory.AttachSignatureTransaction(tx, signature));
            //Console.WriteLine(payload);
            //Console.WriteLine(hash);
            //Console.WriteLine(signed.Signature);

            //const string node = "http://sym-test-01.opening-line.jp:3000";
            //using var client = new HttpClient();
            //var content = new StringContent(payload, Encoding.UTF8, "application/json");
            //var response = client.PutAsync(node + "/transactions", content).Result;
            //var responseDetailsJson = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseDetailsJson);
        }

        private void btnReadExcel_Click(object sender, EventArgs e)
        {
            try
            {
                List<ExcelRowData> rowDatas = null;
                using (var dialog = new OpenFileDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var file = dialog.FileName;
                        rowDatas = ReadExcel(file);
                    }
                };

                if (rowDatas == null) MessageBox.Show("Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B");

                // �O���b�h�Ƀf�[�^��ݒ肷��
                foreach(var rowData in rowDatas)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COL_ACCOUNT].Value = rowData.AccountName;
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COL_TWITTER].Value = rowData.TwitterUrl;
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COL_ADDRESS].Value = rowData.Address;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B");
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private List<ExcelRowData> ReadExcel(string fileName)
        {
            var rowDatas = new List<ExcelRowData>();
            
            // �g���q���`�F�b�N
            if (!Path.HasExtension(fileName))
            {
                MessageBox.Show("�g���q��.xlsx�ł͂���܂���B");
            }

            try
            {
                // �s���A�擪�s�̓w�b�_�[�Ȃ̂�2�s�ڂ����͂���
                int row = 2;

                using (var wb = new XLWorkbook(fileName))
                {
                    foreach(var ws in wb.Worksheets)
                    {
                        while(row <= ws.LastRowUsed().RowNumber())
                        {
                            var rowData = new ExcelRowData();

                            rowData.AccountName = ws.Cell(row, 1).GetString();
                            rowData.TwitterUrl = ws.Cell(row, 2).GetString();
                            //rowData.AccountIcon = ws.Cell(row, column++).GetString(), �A�C�R���͔�΂�
                            rowData.Address = ws.Cell(row, 5).GetString();
                            rowDatas.Add(rowData);
                            row++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("�G���[�����������̂ŏ����𒆒f���܂��B");
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return rowDatas;
        }



        /// <summary>
        /// �O���b�h�ɓ��͂��ꂽ�l���`�F�b�N����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnCheck_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                var accountRequest = new RestRequest("/accounts/");
                var namespaceRequest = new RestRequest("/namespaces/");
                var content = string.Empty;
                JObject json = null;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://sym-test-01.opening-line.jp:3000");
                
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var addressOrAlias = row.Cells[GRID_COL_ADDRESS].Value != null ? row.Cells[GRID_COL_ADDRESS].Value.ToString() : "";
                    
                    // �A�h���X�̕����񂩂ǂ���
                    if (IsAddressLength(addressOrAlias))
                    {

                            
                        var accountResult = await client.GetAsync(string.Format("/accounts/{0}", addressOrAlias));
                        content = await accountResult.Content.ReadAsStringAsync();
                        json = JObject.Parse(content);

                        // �A�h���X�̏�񂪎擾�ł��Ă���Ζ��Ȃ�
                        var json_account = json["account"];
                        if (json_account != null) continue;
                        // TODO:���ʗ�ł�����Ă����������������H
                    }

                    // �l�[���X�y�[�X���m�F
                    var b = Encoding.UTF8.GetBytes(addressOrAlias);
                    var ulong_namespaceid = IdGenerator.GenerateNamespaceId(b);
                    var bytes = BitConverter.GetBytes(ulong_namespaceid);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    var namespaceId = Convert.ToHexString(bytes);
                    var namespaceResult = await client.GetAsync(string.Format("/namespaces/{0}", namespaceId));
                    content = await namespaceResult.Content.ReadAsStringAsync();
                    json = JObject.Parse(content);
                    var jsonNamespace = json["namespace"];
                    if (json["namespace"] != null)
                    {
                        row.Cells[GRID_COL_NAMESPACE].Value = addressOrAlias;
                        row.Cells[GRID_COL_ADDRESS].Value = Converter.AddressToString(Converter.HexToBytes(jsonNamespace["alias"]["address"].ToString()));
                    }
                }
            }
            catch(Exception ex)
            {

            }
            //var client = new RestClient("https://umayadia-apisample.azurewebsites.net");
            //var request = new RestRequest("/api/persons");
            //var response = await client.GetAsync(request);
        }

        /// <summary>
        /// ������39�����ł��邩
        /// 
        /// 39�����łȂ���΃A�h���X�ł͂Ȃ��͂�
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool IsAddressLength(string address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                if(address.Length == ADDRESS_LENGTH)
                {
                    return true;
                }
            }
            return false;
        }

        public class ExcelRowData
        {
            /// <summary>
            /// �A�J�E���g��
            /// </summary>
            public string? AccountName { get; set; }
            /// <summary>
            /// �c�C�b�^�[��URL
            /// </summary>
            public string? TwitterUrl { get; set; }
            /// <summary>
            /// �A�C�R���i�������j
            /// </summary>
            public byte[]? AccountIcon { get; set; }
            /// <summary>
            /// �l�[���X�y�[�X
            /// </summary>
            public string? NameSpace { get; set; }
            /// <summary>
            /// �A�h���X
            /// </summary>
            public string? Address { get; set; }
        }
    }
}
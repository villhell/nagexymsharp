using CatSdk.CryptoTypes;
using CatSdk.Facade;
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
        #region �O���b�h�̗�
        private const string GRID_COLNAME_ACCOUNT = "dghAccountName";
        private const string GRID_COLNAME_TWITTER = "dghTwitter";
        private const string GRID_COLNAME_NAMESPACE = "dghNameSpace";
        private const string GRID_COLNAME_ADDRESS = "dghAddress";
        private const string GRID_COLNAME_XYM = "dghXym";
        private const string GRID_COLNAME_MESSAGE = "dghMessage";
        private const string GRID_COLNAME_CHECK = "dghCheck";
        #endregion

        /// <summary>
        /// �A�h���X�̕�����
        /// </summary>
        private const int ADDRESS_LENGTH = 39;

        public Form1()
        {
            InitializeComponent();
            txtFrom.Text= Properties.Settings.Default.FromAddress;
            txtPrivateKey.Text= "";
            //txtNodeUrl.Text = "http://sym-test-01.opening-line.jp:3000";
            txtNodeUrl.Text = Properties.Settings.Default.NodeUrl;
            //toolStripStatusLabel1.Text = "";
            toolStripProgressBar1.Value= 0;
            comboBox1.Items.Clear();
            comboBox1.Items.Add(Network.TestNet);
            comboBox1.Items.Add(Network.MainNet);
        }

        #region �L�����Z���{�^��
        /// <summary>
        /// �L�����Z���{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region ���M�{�^���N���b�N
        /// <summary>
        /// ���M�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text)) MessageBox.Show("�󗓂�����܂��B");
            if (string.IsNullOrEmpty(txtPrivateKey.Text)) MessageBox.Show("�󗓂�����܂��B");

            var network = comboBox1.SelectedItem as Network;
            if (network == null)
            {
                MessageBox.Show("�l�b�g���[�N���I������Ă��܂���B");
                return;
            }

            //SendTransferTransaction();
            SendAggregateCompleteTransaction(network);
        }
        #endregion

        #region �N���A�{�^���N���b�N
        /// <summary>
        /// �N���A�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }
        #endregion

        #region Excel�Ǎ��{�^���N���b�N
        /// <summary>
        /// Excel�Ǎ��{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnReadExcel_Click(object sender, EventArgs e)
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

                if (rowDatas == null)
                {
                    //toolStripStatusLabel1.Text = "Excel�t�@�C���̓��e��0���ł����B";
                    MessageBox.Show("Excel�t�@�C���̓��e��0���ł����B");
                    return;
                }

                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = rowDatas.Count;
                toolStripProgressBar1.Value = 0;

                //// ���[�U�[�̍s�ǉ����֎~
                //dataGridView1.AllowUserToAddRows = false;

                // �O���b�h�Ƀf�[�^��ݒ肷��
                foreach (var rowData in rowDatas)
                {
                    dataGridView1.Rows.Add();

                    await Task.Factory.StartNew(() =>
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_ACCOUNT].Value = rowData.AccountName;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_TWITTER].Value = rowData.TwitterUrl;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_ADDRESS].Value = rowData.Address;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_XYM].Value = rowData.Xym;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_MESSAGE].Value = rowData.Message;
                            //dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_CHECK].Value;
                        });
                    });
                    toolStripProgressBar1.Value = toolStripProgressBar1.Value + 1;
                }

                //toolStripStatusLabel1.Text = "Excel�t�@�C���̓Ǎ����������܂����B";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B");
                //toolStripStatusLabel1.Text = "Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B";
            }
            finally
            {
                //// ���[�U�[�̍s�ǉ�������
                //dataGridView1.AllowUserToAddRows = true;
            }
        }
        #endregion

        #region �g�����X�t�@�[�g�����U�N�V�������쐬�A�A�i�E���X����
        /// <summary>
        /// �g�����X�t�@�[�g�����U�N�V�������쐬�A�A�i�E���X����
        /// 
        /// MEMO:�Q�l�܂łɂ����Ă���
        /// </summary>
        private async void SendTransferTransaction()
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
        #endregion

        #region �O���b�h�̃A�h���X�̓��e�ŃA�O���Q�[�g�R���v���[�g�g�����U�N�V�������쐬�A�A�i�E���X����
        /// <summary>
        /// �O���b�h�̃A�h���X�̓��e�ŃA�O���Q�[�g�R���v���[�g�g�����U�N�V�������쐬�A�A�i�E���X����
        /// 
        /// �A�O���Q�[�g�R���v���[�g�ɓZ�߂���g�����U�N�V������100���܂łȂ̂�
        /// 100�����ɓZ�߂ăA�i�E���X����
        /// <param name="network"></param>
        /// </summary>
        private void SendAggregateCompleteTransaction(Network network)
        {
            var facade = new SymbolFacade(network);
            var privateKey = new PrivateKey(txtPrivateKey.Text);
            var keyPair = new KeyPair(privateKey);

            var txs = new List<IBaseTransaction>();

            // �g�����U�N�V������Z�߂���̂�100���܂łȂ̂ŃJ�E���g����
            int count = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // �`�F�b�N���ʂ�NG�̏ꍇ�̓X���[
                if (string.Equals("NG", row.Cells[GRID_COLNAME_CHECK].Value)) continue;

                string? address = row.Cells[GRID_COLNAME_ADDRESS].Value.ToString();
                string? s = row.Cells[GRID_COLNAME_XYM].Value.ToString();
                ulong xym = (ulong)double.Parse(s) * 1000000;
                string? message = row.Cells[GRID_COLNAME_MESSAGE].Value.ToString();
                byte[] bytes = Converter.Utf8ToPlainMessage(message);

                txs.Add(CreateTransaction(network, keyPair, address, xym, bytes));

                count++;

                if (count > 49)
                {
                    AnnounceAsync(network, facade, keyPair, txs);
                    txs.Clear();
                    count = 0;
                }
            }

            // �c�����g�����U�N�V�������A�i�E���X
            if (txs.Count > 0) AnnounceAsync(network, facade, keyPair, txs);
        }
        #endregion

        #region �g�����U�N�V�������A�i�E���X����
        /// <summary>
        /// �g�����U�N�V�������A�i�E���X����
        /// </summary>
        /// <param name="network"></param>
        /// <param name="facade"></param>
        /// <param name="keyPair"></param>
        /// <param name="txs"></param>
        /// <returns></returns>
        private async Task AnnounceAsync(Network network, SymbolFacade facade, KeyPair keyPair, List<IBaseTransaction> txs)
        {
            var innerTransactions = txs.ToArray();

            var merkleHash = SymbolFacade.HashEmbeddedTransactions(innerTransactions);
            
            var aggTx = new AggregateCompleteTransactionV2
            {
                Network = string.Equals(network.Name, "mainnet") ? NetworkType.MAINNET : NetworkType.TESTNET,
                Transactions = innerTransactions,
                SignerPublicKey = keyPair.PublicKey,
                Fee = new Amount(1000000),
                TransactionsHash = merkleHash,
                Deadline = new Timestamp(facade.Network.FromDatetime<NetworkTimestamp>(DateTime.UtcNow).AddHours(2).Timestamp),
            };

            var signature = facade.SignTransaction(keyPair, aggTx);
            TransactionsFactory.AttachSignature(aggTx, signature);

            var hash = facade.HashTransaction(aggTx);
            //var bobCosignature = new Cosignature
            //{
            //    Signature = bobKeyPair.Sign(hash.bytes),
            //    SignerPublicKey = bobKeyPair.PublicKey
            //};
            //aggTx.Cosignatures = new[] { bobCosignature };

            var payload = TransactionsFactory.CreatePayload(aggTx);

            string node = txtNodeUrl.Text;
            using var client = new HttpClient();
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = client.PutAsync(node + "/transactions", content).Result;
            var responseDetailsJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseDetailsJson);
        }
        #endregion

        #region �g�����U�N�V�������쐬
        /// <summary>
        /// �g�����U�N�V�������쐬
        /// </summary>
        /// <param name="network"></param>
        /// <param name="keyPair"></param>
        /// <param name="address"></param>
        /// <param name="xym"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private EmbeddedTransferTransactionV1 CreateTransaction(Network network, KeyPair keyPair, string address, ulong xym, byte[] message)
        {
            var networkType = string.Equals(network.Name, "mainnet") ? NetworkType.MAINNET : NetworkType.TESTNET;
            ulong mosaicId = (ulong)(string.Equals(network.Name, "mainnet") ? 0x6BED913FA20223F8 : 0x72C0212E67A08BCE);
            return new EmbeddedTransferTransactionV1
            {
                Network = networkType,
                SignerPublicKey = keyPair.PublicKey,
                RecipientAddress = new UnresolvedAddress(Converter.StringToAddress(address)),
                Mosaics = new UnresolvedMosaic[]
                {
                    new()
                    {
                        MosaicId = new UnresolvedMosaicId(mosaicId),
                        Amount = new Amount(xym)
                    }
                },
                Message = message
            };
        }
        #endregion

        #region Excel�t�@�C���Ǎ�
        /// <summary>
        /// Excel�t�@�C���Ǎ�
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
                //toolStripStatusLabel1.Text = "Excel�t�@�C���̓Ǎ����J�n���܂����B";

                // �s���A�擪�s�̓w�b�_�[�Ȃ̂�2�s�ڂ����͂���
                int row = 2;

                using (var wb = new XLWorkbook(fileName))
                {
                    foreach(var ws in wb.Worksheets)
                    {
                        while(row <= ws.LastRowUsed().RowNumber())
                        {
                            var rowData = new ExcelRowData();

                            rowData.AccountName = ws.Cell(row, Settings.EXCEL_COL_ACCOUNT).GetString();
                            rowData.TwitterUrl = ws.Cell(row, Settings.EXCEL_COL_TWITTER).GetString();
                            //rowData.AccountIcon = ws.Cell(row, column++).GetString(), �A�C�R���͔�΂�
                            rowData.Address = ws.Cell(row, Settings.EXCEL_COL_ADDRESS).GetString();
                            rowData.Xym = ws.Cell(row, Settings.EXCEL_COL_XYM).GetDouble();
                            rowData.Message = ws.Cell(row, Settings.EXCEL_COL_MESSAGE).GetString();
                            rowDatas.Add(rowData);
                            row++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("�G���[�����������̂ŏ����𒆒f���܂��B");
                //toolStripStatusLabel1.Text = "Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return rowDatas;
        }
        #endregion

        #region �O���b�h�ɓ��͂��ꂽ�l���`�F�b�N����
        /// <summary>
        /// �O���b�h�ɓ��͂��ꂽ�l���`�F�b�N����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnCheck_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                //var accountRequest = new RestRequest("/accounts/");
                //var namespaceRequest = new RestRequest("/namespaces/");
                var content = string.Empty;
                JObject json = null;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(txtNodeUrl.Text);
                
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var addressOrAlias = row.Cells[GRID_COLNAME_ADDRESS].Value != null ? row.Cells[GRID_COLNAME_ADDRESS].Value.ToString() : "";

                    // �A�h���X�̕����񂩂ǂ���
                    var address = await ExistsAddressAsync(addressOrAlias, client);
                    if (!string.IsNullOrEmpty(address))
                    {
                        // �A�h���X�����������ꍇ�͎���
                        row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                        continue;
                    }
                    //else
                    //{
                    //    // �A�h���X���Ȃ�
                    //    row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                    //}
                    
                    // �l�[���X�y�[�X����A�h���X���擾�ł��邩
                    address = await ExistsNamespaceAsync(addressOrAlias, client);
                    if (!string.IsNullOrEmpty(address))
                    {
                        // �l�[���X�y�[�X�A�A�h���X���O���b�h�ɐݒ�
                        row.Cells[GRID_COLNAME_NAMESPACE].Value = addressOrAlias;
                        row.Cells[GRID_COLNAME_ADDRESS].Value = address;
                        row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                    }
                    else
                    {
                        // �A�h���X�ł��l�[���X�y�[�X�ł��Ȃ�
                        row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                    }

                    // ������Ƃ����҂�
                    Task.Delay(100);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("�G���[�����������̂ŏ����𒆒f���܂��B");
                //toolStripStatusLabel1.Text = "Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        #region �A�h���X�����擾�ł��邩
        /// <summary>
        /// �A�h���X�����擾�ł��邩
        /// </summary>
        /// <param name="address"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<string?> ExistsAddressAsync(string address, HttpClient client)
        {
            var ret = string.Empty;
            
            try
            {
                // �A�h���X�̕����񂩂ǂ���
                if (IsAddressLength(address))
                {
                    var accountResult = await client.GetAsync(string.Format("/accounts/{0}", address));
                    var content = await accountResult.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);

                    // �A�h���X�̏�񂪎擾�ł��Ă���Ζ��Ȃ�
                    var json_account = json["account"];
                    
                    // �擾�ł��Ȃ����false
                    if (json_account == null) { return null; }

                    ret = json_account["address"].ToString();
                }
                return ret;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region �l�[���X�y�[�X�����擾�ł��邩
        /// <summary>
        /// �l�[���X�y�[�X�����擾�ł��邩
        /// TODO: �ԋp����̂�Address�ɂȂ��Ă�͈̂�a������
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="client"></param>
        /// <returns></returns>

        private async Task<string?> ExistsNamespaceAsync(string ns, HttpClient client)
        {
            try
            {
                var namespaceId = Utils.StringToNamespaceId(ns);
                var namespaceResult = await client.GetAsync(string.Format("/namespaces/{0}", namespaceId));
                var content = await namespaceResult.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var jsonNamespace = json["namespace"];

                if (jsonNamespace == null) { return null; }

                var jobject = jsonNamespace["alias"]["address"] ?? jsonNamespace["ownerAddress"];
                return Converter.AddressToString(Converter.HexToBytes(jobject.ToString()));
            }
            catch (Exception ex)
            {
                // �G���[�ł�false
                return null;
            }
        }
        #endregion

        #region ������39�����ł��邩
        /// <summary>
        /// ������39�����ł��邩
        /// 
        /// 39�����łȂ���΃A�h���X�ł͂Ȃ��͂�
        /// 39�����̃l�[���X�y�[�X�ł�ExistsNamespaceAsync�Ŕ��ʂł���
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
        #endregion

        /// <summary>
        /// �t�H�[�������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtNodeUrl.Text))
                Properties.Settings.Default.NodeUrl = txtNodeUrl.Text;
            if (!string.IsNullOrEmpty(txtFrom.Text))
                Properties.Settings.Default.FromAddress = txtFrom.Text;

            Properties.Settings.Default.Save();
        }
    }
}
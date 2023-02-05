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
using static nagexym.ToolStateManager;
using static System.Windows.Forms.AxHost;

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

        /// <summary>
        /// �L�����Z���g�[�N���\�[�X
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// �l�b�g���[�N
        /// </summary>
        private Network? _network;

        /// <summary>
        /// �c�[���̏��
        /// </summary>
        private ToolStateManager _toolStateManager;

        public Form1()
        {
            InitializeComponent();

            // �O��l��Ǎ���
            txtFrom.Text= Properties.Settings.Default.FromAddress;
            txtNodeUrl.Text = Properties.Settings.Default.NodeUrl;
            
            // ������
            txtPrivateKey.Text= "";
            lblStatus.Text = "";
            progressBar1.Value= 0;
            _toolStateManager = new ToolStateManager();

            // ��Ԑݒ�
            _toolStateManager.SetState(ToolState.Init);
            // �R���g���[������
            ChangeControl();
        }

        #region �L�����Z���{�^��
        /// <summary>
        /// �L�����Z���{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();

            // �L�����Z�����̃R���g���[������
            ChangeControl(true);

            var state = _toolStateManager.GetState();
            if (state == ToolState.StartReadExcel)
            {
                _toolStateManager.SetState(ToolState.Init);
            }
            else if(state == ToolState.StartCheck || state == ToolState.Checking)
            {
                _toolStateManager.SetState(ToolState.EndReadExcel);
            }
        }
        #endregion

        #region ���M�{�^���N���b�N
        /// <summary>
        /// ���M�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text))
            {
                MessageBox.Show("���Ȃ��̃A�h���X�������͂ł��B");
                return;
            }
            if (string.IsNullOrEmpty(txtPrivateKey.Text))
            {
                MessageBox.Show("���Ȃ��̔閧���������͂ł��B");
                return;
            }

            if (_network == null)
            {
                MessageBox.Show("�l�b�g���[�N�����ʂł��܂���B\n���Ȃ��̃A�h���X�̓��͂��Ċm�F���ĉ������B");
                return;
            }

            if (_toolStateManager.GetState() != ToolState.EndCheck)
            {
                MessageBox.Show("����̃A�h���X�̃`�F�b�N���I����Ă��܂���B\n�`�F�b�N�{�^���������ĉ������B");
                return;
            }
            if (!await EqualsNetwork())
            {
                MessageBox.Show("�m�[�h�Ƃ��Ȃ��̃A�h���X�̃l�b�g���[�N���قȂ��Ă��܂��B\n���͒l���������ĉ������B");
                return;
            }

            //SendTransferTransaction();

            // �萔�������擾
            var fee = await GetFee();

            SendAggregateCompleteTransaction(_network, fee);
        }

        #endregion

        #region �m�[�h�̃l�b�g���[�N�ƃA�h���X�̃l�b�g���[�N����v���Ă��邩
        /// <summary>
        /// �m�[�h�̃l�b�g���[�N�ƃA�h���X�̃l�b�g���[�N����v���Ă��邩
        /// </summary>
        /// <returns></returns>
        private async Task<bool> EqualsNetwork()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(txtNodeUrl.Text);
                var feeResult = await client.GetAsync("/network");
                var content = await feeResult.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var nodeNetwork = json["name"].ToString();

                if(string.Equals(nodeNetwork, lblNetwork.Text))
                { 
                    return true;
                }
                
            }
            catch(Exception ex)
            {
                return false;
            }
            return false;
        }
        #endregion

        #region �萔�����擾
        /// <summary>
        /// �萔�����擾
        /// </summary>
        /// <returns></returns>
        private async Task<double> GetFee()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(txtNodeUrl.Text);
            var feeResult = await client.GetAsync("/network/fees/transaction");
            var content = await feeResult.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            double fee = 0.0;
            if (json["minFeeMultiplier"] == null || json["averageFeeMultiplier"] == null)
            {
                fee = 100;
            }
            else
            {
                int min = json["minFeeMultiplier"].Value<int>();
                int avg = json["averageFeeMultiplier"].Value<int>();
                fee = (min + avg) * 0.65;
            }
            return fee;
        }
        #endregion

        #region �N���A�{�^���N���b�N
        /// <summary>
        /// �N���A�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            progressBar1.Value = 0;
            lblStatus.Text = "�N���A���܂����B";
            _toolStateManager.SetState(ToolState.Init);
        }
        #endregion

        #region Excel�Ǎ��{�^���N���b�N
        /// <summary>
        /// Excel�Ǎ��{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnReadExcel_ClickAsync(object sender, EventArgs e)
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
                    lblStatus.Text = "Excel�t�@�C���̓��e��0���ł����B";
                    MessageBox.Show("Excel�t�@�C���̓��e��0���ł����B");
                    return;
                }

                // �v���O���X�o�[�̐ݒ�
                progressBar1.Minimum = 0;
                progressBar1.Maximum = rowDatas.Count;
                progressBar1.Value = 0;

                // �L�����Z���g�[�N������
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                // �O���b�h�̏�����
                dataGridView1.Rows.Clear();

                // �O���b�h�Ƀf�[�^��ݒ肷��
                foreach (var rowData in rowDatas)
                {
                    await Task.Run(() =>
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            dataGridView1.Rows.Add();

                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_ACCOUNT].Value = rowData.AccountName;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_TWITTER].Value = rowData.TwitterUrl;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_ADDRESS].Value = rowData.Address;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_XYM].Value = rowData.Xym;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[GRID_COLNAME_MESSAGE].Value = rowData.Message;
                        });
                            
                        // �L�����Z������
                        if (cancellationToken.IsCancellationRequested)
                        {
                            //cancellationToken.ThrowIfCancellationRequested();
                            throw new OperationCanceledException();
                        }
                        cancellationToken.ThrowIfCancellationRequested();

                        Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = progressBar1.Value + 1;
                        });  
                        
                    }, cancellationToken);
                }

                // ��Ԑݒ�
                _toolStateManager.SetState(ToolState.EndReadExcel);
                // �R���g���[������
                ChangeControl();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Excel�t�@�C���̓Ǎ����~���܂����B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B");
                lblStatus.Text = "Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B";
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
        private void SendAggregateCompleteTransaction(Network network, double feeMultiplier)
        {

            // ��Ԑݒ�
            _toolStateManager.SetState(ToolState.StartSend);
            // �R���g���[������
            ChangeControl();

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
                    AnnounceAsync(network, facade, keyPair, txs, feeMultiplier);
                    txs.Clear();
                    count = 0;
                }
            }

            // �c�����g�����U�N�V�������A�i�E���X
            if (txs.Count > 0) AnnounceAsync(network, facade, keyPair, txs, feeMultiplier);

            // ��Ԑݒ�
            _toolStateManager.SetState(ToolState.EndSend);
            // �R���g���[������
            ChangeControl();
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
        private async Task AnnounceAsync(Network network, SymbolFacade facade, KeyPair keyPair, List<IBaseTransaction> txs, double feeMultiplier)
        {
            var innerTransactions = txs.ToArray();

            var merkleHash = SymbolFacade.HashEmbeddedTransactions(innerTransactions);
            
            var aggTx = new AggregateCompleteTransactionV2
            {
                Network = string.Equals(network.Name, "mainnet") ? NetworkType.MAINNET : NetworkType.TESTNET,
                Transactions = innerTransactions,
                SignerPublicKey = keyPair.PublicKey,
                //Fee = new Amount(1000000),
                TransactionsHash = merkleHash,
                Deadline = new Timestamp(facade.Network.FromDatetime<NetworkTimestamp>(DateTime.UtcNow).AddHours(2).Timestamp),
            };
            var fee = aggTx.Size * feeMultiplier;
            aggTx.Fee = new Amount((ulong)fee);
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
                // ��Ԑݒ�
                _toolStateManager.SetState(ToolState.StartReadExcel);
                // �R���g���[������
                ChangeControl();

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
                MessageBox.Show("�G���[�����������̂ŏ����𒆒f���܂��B", this.Name);
                lblStatus.Text = "Excel�t�@�C���̓Ǎ��Ɏ��s���܂����B";
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
            progressBar1.Minimum = 0;
            progressBar1.Maximum = dataGridView1.Rows.Count;
            progressBar1.Value = 0;

            if (_toolStateManager.GetState() != ToolState.EndReadExcel)
            {
                MessageBox.Show("Excel�t�@�C����ǂݍ���ł��܂���B\nExcel�Ǎ��{�^���������ăt�@�C����ǂݍ���ŉ������B");
                return;
            }

            try
            {
                // ��Ԑݒ�
                _toolStateManager.SetState(ToolState.StartCheck);
                // �R���g���[������
                ChangeControl();

                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                var content = string.Empty;
                JObject json = null;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(txtNodeUrl.Text);

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var addressOrAlias = row.Cells[GRID_COLNAME_ADDRESS].Value != null ? row.Cells[GRID_COLNAME_ADDRESS].Value.ToString() : "";

                    // ������Ƃ����҂�
                    Task.Delay(100);

                    await Task.Run(async () =>
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = progressBar1.Value + 1;
                            
                            // ��Ԑݒ�
                            _toolStateManager.SetState(ToolState.Checking);
                            // �R���g���[������
                            ChangeControl();
                        });

                        // �A�h���X�̕����񂩂ǂ���
                        var address = await ExistsAddressAsync(addressOrAlias, client);
                        if (!string.IsNullOrEmpty(address))
                        {
                            // �A�h���X�����������ꍇ�͎���
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                            });
                            return;
                        }
                    
                        // �l�[���X�y�[�X����A�h���X���擾�ł��邩
                        address = await ExistsNamespaceAsync(addressOrAlias, client);
                        if (!string.IsNullOrEmpty(address))
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                // �l�[���X�y�[�X�A�A�h���X���O���b�h�ɐݒ�
                                row.Cells[GRID_COLNAME_NAMESPACE].Value = addressOrAlias;
                                row.Cells[GRID_COLNAME_ADDRESS].Value = address;
                                row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                            });
                        }
                        else
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                // �A�h���X�ł��l�[���X�y�[�X�ł��Ȃ�
                                row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                            });
                        }

                        // �L�����Z������
                        if (cancellationToken.IsCancellationRequested)
                        {
                            //cancellationToken.ThrowIfCancellationRequested();
                            throw new OperationCanceledException();
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                        
                    }, cancellationToken);
                }

                // ��Ԑݒ�
                _toolStateManager.SetState(ToolState.EndCheck);
                // �R���g���[������
                ChangeControl();
                
            }
            catch (OperationCanceledException ex)
            {
                lblStatus.Text = "�`�F�b�N�����𒆒f���܂����B";
                MessageBox.Show("�`�F�b�N�����𒆒f���܂��B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("�G���[�����������̂ŏ����𒆒f���܂��B");
                lblStatus.Text = "�G���[�����������̂ŏ����𒆒f���܂����B";
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

        #region Form1_FormClosing
        /// <summary>
        /// Form1_FormClosing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNodeUrl.Text))
                Properties.Settings.Default.NodeUrl = txtNodeUrl.Text;
            if (!string.IsNullOrEmpty(txtFrom.Text))
                Properties.Settings.Default.FromAddress = txtFrom.Text;

            Properties.Settings.Default.Save();
        }
        #endregion

        #region txtFrom_TextChanged
        /// <summary>
        /// txtFrom_TextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrom_TextChanged(object sender, EventArgs e)
        {
            SetNetwork();
        }
        #endregion

        #region ���͂����A�h���X�����ƂɃl�b�g���[�N�𔻕ʂ���
        /// <summary>
        /// ���͂����A�h���X�����ƂɃl�b�g���[�N�𔻕ʂ���
        /// </summary>
        private void SetNetwork()
        {
            var text = txtFrom.Text;
            if (IsAddressLength(txtFrom.Text))
            {
                var identifier = text.Substring(0, 1);
                if (string.Equals(identifier, "T"))
                {
                    lblNetwork.Text = "testnet";
                    _network = Network.TestNet;
                }
                else if (string.Equals(identifier, "N"))
                {
                    lblNetwork.Text = "mainnet";
                    _network = Network.MainNet;
                }
                else
                {
                    lblNetwork.Text = "unknown network";
                    _network = null;
                }
            }
            else
            {
                lblNetwork.Text = "unknown network";
                _network = null;
            }
        }
        #endregion

        /// <summary>
        /// �c�[���̏�Ԃɍ��킹�ăR���g���[����ω�������
        /// </summary>
        private void ChangeControl(bool cancelFlg = false)
        {
            var state = _toolStateManager.GetState();

            if(cancelFlg)
            {
                switch (state)
                {
                    case ToolState.StartReadExcel:
                        lblStatus.Text = "Excel�t�@�C���̓Ǎ��𒆎~���܂����B";
                        btnCancel.Enabled = false;
                        break;
                    
                    case ToolState.StartCheck:
                    case ToolState.Checking:
                        lblStatus.Text = "�`�F�b�N�𒆎~���܂����B";
                        btnCancel.Enabled = false;
                        break;
                }

                return;
            }

            switch (state)
            {
                case ToolState.Init:
                    lblStatus.Text = "nagexymsharp���N�����܂����B";
                    btnCancel.Enabled = false;
                    break;
                case ToolState.StartReadExcel:
                    lblStatus.Text = "Excel�t�@�C���̓Ǎ����J�n���܂����B";
                    btnCancel.Enabled = true;
                    break;
                case ToolState.EndReadExcel:
                    lblStatus.Text = "Excel�t�@�C���̓Ǎ����I�����܂����B";
                    btnCancel.Enabled = false;
                    break;
                case ToolState.StartCheck:
                    lblStatus.Text = "�`�F�b�N���J�n���܂����B";
                    btnCancel.Enabled = true;
                    break;
                case ToolState.Checking:
                    lblStatus.Text = string.Format("�`�F�b�N���ł��B {0}/{1}", progressBar1.Value, progressBar1.Maximum);
                    break;
                case ToolState.EndCheck:
                    lblStatus.Text = string.Format("�`�F�b�N�����ł��B {0}/{1}", progressBar1.Value, progressBar1.Maximum);
                    btnCancel.Enabled = true;
                    break;
                case ToolState.StartSend:
                    lblStatus.Text = "���M���Ă��܂��B";
                    break;
                case ToolState.EndSend:
                    lblStatus.Text = "���M���܂����B";
                    break;
                case ToolState.Complete:
                    lblStatus.Text = "�I�����܂����B";
                    break;
            }
        }
    }
}
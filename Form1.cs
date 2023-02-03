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
        private const string GRID_COLNAME_ACCOUNT = "dghAccountName";
        private const string GRID_COLNAME_TWITTER = "dghTwitter";
        private const string GRID_COLNAME_NAMESPACE = "dghNameSpace";
        private const string GRID_COLNAME_ADDRESS = "dghAddress";
        private const string GRID_COLNAME_XYM = "dghXym";
        private const string GRID_COLNAME_MESSAGE = "dghMessage";
        private const string GRID_COLNAME_CHECK = "dghCheck";

        /// <summary>
        /// アドレスの文字数
        /// </summary>
        private const int ADDRESS_LENGTH = 39;

        public Form1()
        {
            InitializeComponent();
            txtFrom.Text= "";
            txtPrivateKey.Text= "";
            //txtNodeUrl.Text = "http://sym-test-01.opening-line.jp:3000";
            txtNodeUrl.Text = "";
            //toolStripStatusLabel1.Text = "";
            toolStripProgressBar1.Value= 0;
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 送信ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text)) MessageBox.Show("空欄があります。");
            if (string.IsNullOrEmpty(txtPrivateKey.Text)) MessageBox.Show("空欄があります。");

            //SendTransferTransaction();
            SendAggregateCompleteTransaction();
        }

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

        private async void SendAggregateCompleteTransaction()
        {
            var facade = new SymbolFacade(Network.TestNet);
            var privateKey = new PrivateKey(txtPrivateKey.Text);
            var keyPair = new KeyPair(privateKey);

            var txs = new List<IBaseTransaction>();

            // トランザクションを纏められるのは100件までなのでカウントする
            int count = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string? address = row.Cells[GRID_COLNAME_ADDRESS].Value.ToString();
                string? s = row.Cells[GRID_COLNAME_XYM].Value.ToString();
                ulong xym = (ulong)double.Parse(s) * 1000000;
                string? message = row.Cells[GRID_COLNAME_MESSAGE].Value.ToString();
                byte[] bytes= Converter.Utf8ToPlainMessage(message);

                txs.Add(CreateTransaction(keyPair, address, xym, bytes));

                count++;

                if(count > 99)
                {
                    AnnounceAsync(facade, keyPair, txs);
                    txs.Clear();
                }
            }

            // 残ったトランザクションをアナウンス
            if(txs.Count > 0) AnnounceAsync(facade, keyPair, txs);
        }

        private async Task AnnounceAsync(SymbolFacade facade, KeyPair keyPair, List<IBaseTransaction> txs)
        {
            var innerTransactions = txs.ToArray();

            var merkleHash = SymbolFacade.HashEmbeddedTransactions(innerTransactions);

            var aggTx = new AggregateCompleteTransactionV2
            {
                Network = NetworkType.TESTNET,
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

        private EmbeddedTransferTransactionV1 CreateTransaction(KeyPair keyPair, string address, ulong xym, byte[] message)
        {
            return new EmbeddedTransferTransactionV1
            {
                Network = NetworkType.TESTNET,
                SignerPublicKey = keyPair.PublicKey,
                RecipientAddress = new UnresolvedAddress(Converter.StringToAddress(address)),
                Mosaics = new UnresolvedMosaic[]
                {
                    new()
                    {
                        MosaicId = new UnresolvedMosaicId(0x72C0212E67A08BCE),
                        Amount = new Amount(xym)
                    }
                },
                Message = message
            };
        }

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
                    //toolStripStatusLabel1.Text = "Excelファイルの内容が0件でした。";
                    MessageBox.Show("Excelファイルの内容が0件でした。");
                    return;
                }

                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = rowDatas.Count;
                toolStripProgressBar1.Value = 0;

                //// ユーザーの行追加を禁止
                //dataGridView1.AllowUserToAddRows = false;

                // グリッドにデータを設定する
                foreach(var rowData in rowDatas)
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

                //toolStripStatusLabel1.Text = "Excelファイルの読込が完了しました。";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Excelファイルの読込に失敗しました。");
                //toolStripStatusLabel1.Text = "Excelファイルの読込に失敗しました。";
            }
            finally
            {
                //// ユーザーの行追加を許可
                //dataGridView1.AllowUserToAddRows = true;
            }
        }

        /// <summary>
        /// Excelファイル読込
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private List<ExcelRowData> ReadExcel(string fileName)
        {
            var rowDatas = new List<ExcelRowData>();
            
            // 拡張子をチェック
            if (!Path.HasExtension(fileName))
            {
                MessageBox.Show("拡張子が.xlsxではありません。");
            }

            try
            {
                //toolStripStatusLabel1.Text = "Excelファイルの読込を開始しました。";

                // 行数、先頭行はヘッダーなので2行目から解析する
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
                            //rowData.AccountIcon = ws.Cell(row, column++).GetString(), アイコンは飛ばす
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
                MessageBox.Show("エラーが発生したので処理を中断します。");
                //toolStripStatusLabel1.Text = "Excelファイルの読込に失敗しました。";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return rowDatas;
        }

        /// <summary>
        /// グリッドに入力された値をチェックする
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

                    // アドレスの文字列かどうか
                    var address = await ExistsAddressAsync(addressOrAlias, client);
                    if (!string.IsNullOrEmpty(address))
                    {
                        // アドレスが見つかった場合は次へ
                        row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                        continue;
                    }
                    //else
                    //{
                    //    // アドレス情報なし
                    //    row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                    //}
                    
                    // ネームスペースからアドレスが取得できるか
                    address = await ExistsNamespaceAsync(addressOrAlias, client);
                    if (!string.IsNullOrEmpty(address))
                    {
                        // ネームスペース、アドレスをグリッドに設定
                        row.Cells[GRID_COLNAME_NAMESPACE].Value = addressOrAlias;
                        row.Cells[GRID_COLNAME_ADDRESS].Value = address;
                        row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                    }
                    else
                    {
                        // アドレスでもネームスペースでもない
                        row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("エラーが発生したので処理を中断します。");
                //toolStripStatusLabel1.Text = "Excelファイルの読込に失敗しました。";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task<string?> ExistsAddressAsync(string address, HttpClient client)
        {
            var ret = string.Empty;
            
            try
            {
                // アドレスの文字列かどうか
                if (IsAddressLength(address))
                {
                    var accountResult = await client.GetAsync(string.Format("/accounts/{0}", address));
                    var content = await accountResult.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);

                    // アドレスの情報が取得できていれば問題なし
                    var json_account = json["account"];
                    
                    // 取得できなければfalse
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
                // エラーでもfalse
                return null;
            }
        }

        /// <summary>
        /// 引数が39文字であるか
        /// 
        /// 39文字でなければアドレスではないはず
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

        /// <summary>
        /// グリッドの内容をクリアする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }
    }
}
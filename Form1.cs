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
        #region グリッドの列名
        private const string GRID_COLNAME_ACCOUNT = "dghAccountName";
        private const string GRID_COLNAME_TWITTER = "dghTwitter";
        private const string GRID_COLNAME_NAMESPACE = "dghNameSpace";
        private const string GRID_COLNAME_ADDRESS = "dghAddress";
        private const string GRID_COLNAME_XYM = "dghXym";
        private const string GRID_COLNAME_MESSAGE = "dghMessage";
        private const string GRID_COLNAME_CHECK = "dghCheck";
        #endregion


        /// <summary>
        /// アドレスの文字数
        /// </summary>
        private const int ADDRESS_LENGTH = 39;

        /// <summary>
        /// キャンセルトークンソース
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// ネットワーク
        /// </summary>
        private Network? _network;

        /// <summary>
        /// ツールの状態
        /// </summary>
        private ToolStateManager _toolStateManager;

        public Form1()
        {
            InitializeComponent();

            // 前回値を読込む
            txtFrom.Text= Properties.Settings.Default.FromAddress;
            txtNodeUrl.Text = Properties.Settings.Default.NodeUrl;
            
            // 初期化
            txtPrivateKey.Text= "";
            lblStatus.Text = "";
            progressBar1.Value= 0;
            _toolStateManager = new ToolStateManager();

            // 状態設定
            _toolStateManager.SetState(ToolState.Init);
            // コントロール制御
            ChangeControl();
        }

        #region キャンセルボタン
        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();

            // キャンセル時のコントロール制御
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

        #region 送信ボタンクリック
        /// <summary>
        /// 送信ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text))
            {
                MessageBox.Show("あなたのアドレスが未入力です。");
                return;
            }
            if (string.IsNullOrEmpty(txtPrivateKey.Text))
            {
                MessageBox.Show("あなたの秘密鍵が未入力です。");
                return;
            }

            if (_network == null)
            {
                MessageBox.Show("ネットワークを識別できません。\nあなたのアドレスの入力を再確認して下さい。");
                return;
            }

            if (_toolStateManager.GetState() != ToolState.EndCheck)
            {
                MessageBox.Show("相手のアドレスのチェックが終わっていません。\nチェックボタンを押して下さい。");
                return;
            }
            if (!await EqualsNetwork())
            {
                MessageBox.Show("ノードとあなたのアドレスのネットワークが異なっています。\n入力値を見直して下さい。");
                return;
            }

            //SendTransferTransaction();

            // 手数料情報を取得
            var fee = await GetFee();

            SendAggregateCompleteTransaction(_network, fee);
        }

        #endregion

        #region ノードのネットワークとアドレスのネットワークが一致しているか
        /// <summary>
        /// ノードのネットワークとアドレスのネットワークが一致しているか
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

        #region 手数料を取得
        /// <summary>
        /// 手数料を取得
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

        #region クリアボタンクリック
        /// <summary>
        /// クリアボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            progressBar1.Value = 0;
            lblStatus.Text = "クリアしました。";
            _toolStateManager.SetState(ToolState.Init);
        }
        #endregion

        #region Excel読込ボタンクリック
        /// <summary>
        /// Excel読込ボタンクリック
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
                    lblStatus.Text = "Excelファイルの内容が0件でした。";
                    MessageBox.Show("Excelファイルの内容が0件でした。");
                    return;
                }

                // プログレスバーの設定
                progressBar1.Minimum = 0;
                progressBar1.Maximum = rowDatas.Count;
                progressBar1.Value = 0;

                // キャンセルトークン生成
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                // グリッドの初期化
                dataGridView1.Rows.Clear();

                // グリッドにデータを設定する
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
                            
                        // キャンセルする
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

                // 状態設定
                _toolStateManager.SetState(ToolState.EndReadExcel);
                // コントロール制御
                ChangeControl();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Excelファイルの読込を停止しました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excelファイルの読込に失敗しました。");
                lblStatus.Text = "Excelファイルの読込に失敗しました。";
            }
        }
        #endregion

        #region トランスファートランザクションを作成、アナウンスする
        /// <summary>
        /// トランスファートランザクションを作成、アナウンスする
        /// 
        /// MEMO:参考までにおいておく
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

        #region グリッドのアドレスの内容でアグリゲートコンプリートトランザクションを作成、アナウンスする
        /// <summary>
        /// グリッドのアドレスの内容でアグリゲートコンプリートトランザクションを作成、アナウンスする
        /// 
        /// アグリゲートコンプリートに纏められるトランザクションは100件までなので
        /// 100件毎に纏めてアナウンスする
        /// <param name="network"></param>
        /// </summary>
        private void SendAggregateCompleteTransaction(Network network, double feeMultiplier)
        {

            // 状態設定
            _toolStateManager.SetState(ToolState.StartSend);
            // コントロール制御
            ChangeControl();

            var facade = new SymbolFacade(network);
            var privateKey = new PrivateKey(txtPrivateKey.Text);
            var keyPair = new KeyPair(privateKey);

            var txs = new List<IBaseTransaction>();

            // トランザクションを纏められるのは100件までなのでカウントする
            int count = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // チェック結果がNGの場合はスルー
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

            // 残ったトランザクションをアナウンス
            if (txs.Count > 0) AnnounceAsync(network, facade, keyPair, txs, feeMultiplier);

            // 状態設定
            _toolStateManager.SetState(ToolState.EndSend);
            // コントロール制御
            ChangeControl();
        }
        #endregion

        #region トランザクションをアナウンスする
        /// <summary>
        /// トランザクションをアナウンスする
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

        #region トランザクションを作成
        /// <summary>
        /// トランザクションを作成
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

        #region Excelファイル読込
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
                // 状態設定
                _toolStateManager.SetState(ToolState.StartReadExcel);
                // コントロール制御
                ChangeControl();

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
                MessageBox.Show("エラーが発生したので処理を中断します。", this.Name);
                lblStatus.Text = "Excelファイルの読込に失敗しました。";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return rowDatas;
        }
        #endregion

        #region グリッドに入力された値をチェックする
        /// <summary>
        /// グリッドに入力された値をチェックする
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
                MessageBox.Show("Excelファイルを読み込んでいません。\nExcel読込ボタンを押してファイルを読み込んで下さい。");
                return;
            }

            try
            {
                // 状態設定
                _toolStateManager.SetState(ToolState.StartCheck);
                // コントロール制御
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

                    // ちょっとだけ待つ
                    Task.Delay(100);

                    await Task.Run(async () =>
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = progressBar1.Value + 1;
                            
                            // 状態設定
                            _toolStateManager.SetState(ToolState.Checking);
                            // コントロール制御
                            ChangeControl();
                        });

                        // アドレスの文字列かどうか
                        var address = await ExistsAddressAsync(addressOrAlias, client);
                        if (!string.IsNullOrEmpty(address))
                        {
                            // アドレスが見つかった場合は次へ
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                            });
                            return;
                        }
                    
                        // ネームスペースからアドレスが取得できるか
                        address = await ExistsNamespaceAsync(addressOrAlias, client);
                        if (!string.IsNullOrEmpty(address))
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                // ネームスペース、アドレスをグリッドに設定
                                row.Cells[GRID_COLNAME_NAMESPACE].Value = addressOrAlias;
                                row.Cells[GRID_COLNAME_ADDRESS].Value = address;
                                row.Cells[GRID_COLNAME_CHECK].Value = "OK";
                            });
                        }
                        else
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate
                            {
                                // アドレスでもネームスペースでもない
                                row.Cells[GRID_COLNAME_CHECK].Value = "NG";
                            });
                        }

                        // キャンセルする
                        if (cancellationToken.IsCancellationRequested)
                        {
                            //cancellationToken.ThrowIfCancellationRequested();
                            throw new OperationCanceledException();
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                        
                    }, cancellationToken);
                }

                // 状態設定
                _toolStateManager.SetState(ToolState.EndCheck);
                // コントロール制御
                ChangeControl();
                
            }
            catch (OperationCanceledException ex)
            {
                lblStatus.Text = "チェック処理を中断しました。";
                MessageBox.Show("チェック処理を中断します。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生したので処理を中断します。");
                lblStatus.Text = "エラーが発生したので処理を中断しました。";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        #region アドレス情報を取得できるか
        /// <summary>
        /// アドレス情報を取得できるか
        /// </summary>
        /// <param name="address"></param>
        /// <param name="client"></param>
        /// <returns></returns>
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
        #endregion

        #region ネームスペース情報を取得できるか
        /// <summary>
        /// ネームスペース情報を取得できるか
        /// TODO: 返却するのがAddressになってるのは違和感ある
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
                // エラーでもfalse
                return null;
            }
        }
        #endregion

        #region 引数が39文字であるか
        /// <summary>
        /// 引数が39文字であるか
        /// 
        /// 39文字でなければアドレスではないはず
        /// 39文字のネームスペースでもExistsNamespaceAsyncで判別できる
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

        #region 入力したアドレスをもとにネットワークを判別する
        /// <summary>
        /// 入力したアドレスをもとにネットワークを判別する
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
        /// ツールの状態に合わせてコントロールを変化させる
        /// </summary>
        private void ChangeControl(bool cancelFlg = false)
        {
            var state = _toolStateManager.GetState();

            if(cancelFlg)
            {
                switch (state)
                {
                    case ToolState.StartReadExcel:
                        lblStatus.Text = "Excelファイルの読込を中止しました。";
                        btnCancel.Enabled = false;
                        break;
                    
                    case ToolState.StartCheck:
                    case ToolState.Checking:
                        lblStatus.Text = "チェックを中止しました。";
                        btnCancel.Enabled = false;
                        break;
                }

                return;
            }

            switch (state)
            {
                case ToolState.Init:
                    lblStatus.Text = "nagexymsharpを起動しました。";
                    btnCancel.Enabled = false;
                    break;
                case ToolState.StartReadExcel:
                    lblStatus.Text = "Excelファイルの読込を開始しました。";
                    btnCancel.Enabled = true;
                    break;
                case ToolState.EndReadExcel:
                    lblStatus.Text = "Excelファイルの読込を終了しました。";
                    btnCancel.Enabled = false;
                    break;
                case ToolState.StartCheck:
                    lblStatus.Text = "チェックを開始しました。";
                    btnCancel.Enabled = true;
                    break;
                case ToolState.Checking:
                    lblStatus.Text = string.Format("チェック中です。 {0}/{1}", progressBar1.Value, progressBar1.Maximum);
                    break;
                case ToolState.EndCheck:
                    lblStatus.Text = string.Format("チェック完了です。 {0}/{1}", progressBar1.Value, progressBar1.Maximum);
                    btnCancel.Enabled = true;
                    break;
                case ToolState.StartSend:
                    lblStatus.Text = "送信しています。";
                    break;
                case ToolState.EndSend:
                    lblStatus.Text = "送信しました。";
                    break;
                case ToolState.Complete:
                    lblStatus.Text = "終了しました。";
                    break;
            }
        }
    }
}
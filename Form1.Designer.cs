namespace nagexym
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSend = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPrivateKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dghCheck = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghAccountName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghTwitter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghNameSpace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghXym = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dghMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnReadExcel = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.txtNodeUrl = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(1493, 531);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(94, 29);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "送信";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1493, 566);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 29);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "キャンセル";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFrom.Location = new System.Drawing.Point(34, 50);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(470, 27);
            this.txtFrom.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "あなたのアドレス:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 174);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "相手のアドレス:";
            // 
            // txtPrivateKey
            // 
            this.txtPrivateKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPrivateKey.Location = new System.Drawing.Point(34, 112);
            this.txtPrivateKey.Name = "txtPrivateKey";
            this.txtPrivateKey.PasswordChar = '*';
            this.txtPrivateKey.Size = new System.Drawing.Size(470, 27);
            this.txtPrivateKey.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "あなたの秘密鍵:";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dghCheck,
            this.dghAccountName,
            this.dghTwitter,
            this.dghNameSpace,
            this.dghAddress,
            this.dghXym,
            this.dghMessage});
            this.dataGridView1.Location = new System.Drawing.Point(34, 197);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 29;
            this.dataGridView1.Size = new System.Drawing.Size(1453, 399);
            this.dataGridView1.TabIndex = 3;
            // 
            // dghCheck
            // 
            this.dghCheck.HeaderText = "Check";
            this.dghCheck.MinimumWidth = 6;
            this.dghCheck.Name = "dghCheck";
            this.dghCheck.ReadOnly = true;
            this.dghCheck.Width = 77;
            // 
            // dghAccountName
            // 
            this.dghAccountName.HeaderText = "名前";
            this.dghAccountName.MinimumWidth = 6;
            this.dghAccountName.Name = "dghAccountName";
            this.dghAccountName.Width = 68;
            // 
            // dghTwitter
            // 
            this.dghTwitter.HeaderText = "Twitter";
            this.dghTwitter.MinimumWidth = 6;
            this.dghTwitter.Name = "dghTwitter";
            this.dghTwitter.Width = 83;
            // 
            // dghNameSpace
            // 
            this.dghNameSpace.HeaderText = "ネームスペース";
            this.dghNameSpace.MinimumWidth = 6;
            this.dghNameSpace.Name = "dghNameSpace";
            this.dghNameSpace.Width = 116;
            // 
            // dghAddress
            // 
            this.dghAddress.HeaderText = "アドレス";
            this.dghAddress.MinimumWidth = 6;
            this.dghAddress.Name = "dghAddress";
            this.dghAddress.Width = 83;
            // 
            // dghXym
            // 
            this.dghXym.HeaderText = "xym";
            this.dghXym.MinimumWidth = 6;
            this.dghXym.Name = "dghXym";
            this.dghXym.Width = 65;
            // 
            // dghMessage
            // 
            this.dghMessage.HeaderText = "メッセージ";
            this.dghMessage.MinimumWidth = 6;
            this.dghMessage.Name = "dghMessage";
            this.dghMessage.Width = 93;
            // 
            // btnReadExcel
            // 
            this.btnReadExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReadExcel.Location = new System.Drawing.Point(1493, 197);
            this.btnReadExcel.Name = "btnReadExcel";
            this.btnReadExcel.Size = new System.Drawing.Size(94, 29);
            this.btnReadExcel.TabIndex = 4;
            this.btnReadExcel.Text = "Excel読込";
            this.btnReadExcel.UseVisualStyleBackColor = true;
            this.btnReadExcel.Click += new System.EventHandler(this.btnReadExcel_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheck.Location = new System.Drawing.Point(1493, 232);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(94, 29);
            this.btnCheck.TabIndex = 4;
            this.btnCheck.Text = "チェック";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_ClickAsync);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(1493, 267);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(94, 29);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "クリア";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 640);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1599, 28);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(150, 20);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 609);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 20);
            this.label4.TabIndex = 2;
            this.label4.Text = "ノード:";
            // 
            // txtNodeUrl
            // 
            this.txtNodeUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNodeUrl.Location = new System.Drawing.Point(83, 609);
            this.txtNodeUrl.Name = "txtNodeUrl";
            this.txtNodeUrl.Size = new System.Drawing.Size(470, 27);
            this.txtNodeUrl.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1599, 668);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.btnReadExcel);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNodeUrl);
            this.Controls.Add(this.txtPrivateKey);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSend);
            this.Name = "Form1";
            this.Text = "nagexymsharp";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnSend;
        private Button btnCancel;
        private TextBox txtFrom;
        private Label label1;
        private Label label2;
        private TextBox txtPrivateKey;
        private Label label3;
        private DataGridView dataGridView1;
        private Button btnReadExcel;
        private Button btnCheck;
        private Button btnClear;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private DataGridViewTextBoxColumn dghCheck;
        private DataGridViewTextBoxColumn dghAccountName;
        private DataGridViewTextBoxColumn dghTwitter;
        private DataGridViewTextBoxColumn dghNameSpace;
        private DataGridViewTextBoxColumn dghAddress;
        private DataGridViewTextBoxColumn dghXym;
        private DataGridViewTextBoxColumn dghMessage;
        private Label label4;
        private TextBox txtNodeUrl;
    }
}
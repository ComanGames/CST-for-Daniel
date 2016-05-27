using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using NinjaTrader.Cbi;
using Timer = System.Timers.Timer;

namespace NinjaTrader.Strategy
{

	public class AccountInfo : Form
	{
		public class UpdatingData
		{
			private string _buyingPower;
			private string _cashValue;
			private string _realizedProfit;
			private Image _chartImage;

			public UpdatingData(string buyingPower, string cashValue, string realizedProfit, Image chartImage)
			{
				_buyingPower = buyingPower;
				_cashValue = cashValue;
				_realizedProfit = realizedProfit;
				_chartImage = chartImage;
			}

			public string BuyingPower
			{
				get { return _buyingPower; }
			}

			public string CashValue
			{
				get { return _cashValue; }
			}

			public string RealizedProfit
			{
				get { return _realizedProfit; }
			}

			public Image ChartImage
			{
				get { return _chartImage; }
			}
		}

		private ChartSlopeTrader _CST { get; set; }

		private Timer formUpdateTimer;
		private UpdatingData _updatingData;
		private ulong ticks = 0;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private Label label2;
		private Label label1;
		private NumericUpDown numericUpDown1;
		private TextBox textBox1;
		private CheckBox checkBox1;
		private Label label3;
		private TextBox textBox2;
		private Button button2;
		private Label label5;
		private TextBox textBox4;
		private Label label4;
		private TextBox textBox3;
		private const double interval = 1000/25;

		public AccountInfo(ChartSlopeTrader cst)
		{
			InitializeComponent();
			cst.VS2010_InitializeComponent_Form(tabPageChart.Controls);
			_CST = cst;
			GetData();
			UpdateData();
			_working = true;
			formUpdateTimer = new Timer(interval);
			formUpdateTimer.Elapsed += FormUpdateTimer_Elapsed;
			formUpdateTimer.Start();
		}

		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			_working = false;
			if (formUpdateTimer != null)
			{
				formUpdateTimer.Stop();
				formUpdateTimer.Close();
				formUpdateTimer.Dispose();
				formUpdateTimer = null;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		private bool first;

		private void FormUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(!_working||tabPageChart==null)
				return;
			try
			{
				GettingSettingData();
			}
			catch (Exception exception)
			{
				MessageBox.Show("hello"+exception.Message + exception.StackTrace);
			}
		}

		private void GettingSettingData()
		{
				ticks++;
				if (ticks%2 == 0)
					GetData();
				else
					UpdateData();
		}

		private void GetData()
		{
			_updatingData = new UpdatingData(RoundAndString(_CST.GetAccountValue(AccountItem.BuyingPower)),
				RoundAndString(_CST.GetAccountValue(AccountItem.CashValue)),
				RoundAndString(_CST.GetAccountValue(AccountItem.RealizedProfitLoss)),
				ResizeImage(_CST.GetChartPictureFast(), pictureBox1.Width, pictureBox1.Height) );
		}

		private void UpdateData()
		{
			if (_updatingData == null)
				return;
			lock (_updatingData)
			{
				textBoxBuyingPower.Text = _updatingData.BuyingPower;
				textBoxCashValue.Text = _updatingData.CashValue;
				textBoxRealizedProfit.Text = _updatingData.RealizedProfit;
				pictureBox1.Image = _updatingData.ChartImage;

			}
		}

		private Image ResizeImage(Bitmap image, int width, int height)
		{
			return new Bitmap(image, width, height);
		}

		private string RoundAndString(double value)
		{
			return Math.Round(value, 2).ToString();
		}

		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBoxBuyingPower = new System.Windows.Forms.TextBox();
			this.textBoxRealizedProfit = new System.Windows.Forms.TextBox();
			this.labelBuyingPower = new System.Windows.Forms.Label();
			this.textBoxCashValue = new System.Windows.Forms.TextBox();
			this.labelCashValue = new System.Windows.Forms.Label();
			this.labelRealizedProfitLoss = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.button2 = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.tabPageChart = new System.Windows.Forms.TabPage();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabPageChart.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPageChart);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Location = new System.Drawing.Point(1, -1);
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = new System.Drawing.Point(0, 0);
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(915, 577);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(907, 551);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Info";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textBoxBuyingPower);
			this.groupBox1.Controls.Add(this.textBoxRealizedProfit);
			this.groupBox1.Controls.Add(this.labelBuyingPower);
			this.groupBox1.Controls.Add(this.textBoxCashValue);
			this.groupBox1.Controls.Add(this.labelCashValue);
			this.groupBox1.Controls.Add(this.labelRealizedProfitLoss);
			this.groupBox1.Location = new System.Drawing.Point(7, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(344, 131);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Account Info";
			// 
			// textBoxBuyingPower
			// 
			this.textBoxBuyingPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textBoxBuyingPower.Location = new System.Drawing.Point(6, 19);
			this.textBoxBuyingPower.Name = "textBoxBuyingPower";
			this.textBoxBuyingPower.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.textBoxBuyingPower.Size = new System.Drawing.Size(100, 26);
			this.textBoxBuyingPower.TabIndex = 3;
			this.textBoxBuyingPower.Text = "0";
			// 
			// textBoxRealizedProfit
			// 
			this.textBoxRealizedProfit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textBoxRealizedProfit.Location = new System.Drawing.Point(6, 93);
			this.textBoxRealizedProfit.Name = "textBoxRealizedProfit";
			this.textBoxRealizedProfit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.textBoxRealizedProfit.Size = new System.Drawing.Size(100, 26);
			this.textBoxRealizedProfit.TabIndex = 5;
			this.textBoxRealizedProfit.Text = "0";
			// 
			// labelBuyingPower
			// 
			this.labelBuyingPower.AutoSize = true;
			this.labelBuyingPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelBuyingPower.Location = new System.Drawing.Point(112, 16);
			this.labelBuyingPower.Name = "labelBuyingPower";
			this.labelBuyingPower.Size = new System.Drawing.Size(162, 29);
			this.labelBuyingPower.TabIndex = 0;
			this.labelBuyingPower.Text = "Buying Power";
			// 
			// textBoxCashValue
			// 
			this.textBoxCashValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textBoxCashValue.Location = new System.Drawing.Point(6, 56);
			this.textBoxCashValue.Name = "textBoxCashValue";
			this.textBoxCashValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.textBoxCashValue.Size = new System.Drawing.Size(100, 26);
			this.textBoxCashValue.TabIndex = 4;
			this.textBoxCashValue.Text = "0";
			// 
			// labelCashValue
			// 
			this.labelCashValue.AutoSize = true;
			this.labelCashValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelCashValue.Location = new System.Drawing.Point(112, 53);
			this.labelCashValue.Name = "labelCashValue";
			this.labelCashValue.Size = new System.Drawing.Size(135, 29);
			this.labelCashValue.TabIndex = 1;
			this.labelCashValue.Text = "Cash Value";
			// 
			// labelRealizedProfitLoss
			// 
			this.labelRealizedProfitLoss.AutoSize = true;
			this.labelRealizedProfitLoss.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelRealizedProfitLoss.Location = new System.Drawing.Point(112, 90);
			this.labelRealizedProfitLoss.Name = "labelRealizedProfitLoss";
			this.labelRealizedProfitLoss.Size = new System.Drawing.Size(227, 29);
			this.labelRealizedProfitLoss.TabIndex = 2;
			this.labelRealizedProfitLoss.Text = "Realized Profit Loss";
			// 
			// tabPage2
			// 
			this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(907, 551);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "E-Mail Setting";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.button2);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.textBox4);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.textBox3);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.textBox2);
			this.groupBox2.Controls.Add(this.checkBox1);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.numericUpDown1);
			this.groupBox2.Controls.Add(this.textBox1);
			this.groupBox2.Location = new System.Drawing.Point(7, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(209, 198);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Mail setup";
			// 
			// button2
			// 
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.button2.Location = new System.Drawing.Point(6, 146);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(197, 42);
			this.button2.TabIndex = 12;
			this.button2.Text = "Test";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 120);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(51, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Send To:";
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(62, 120);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(141, 20);
			this.textBox4.TabIndex = 10;
			this.textBox4.Text = "some@mail.com";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 94);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Password:";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(62, 94);
			this.textBox3.Name = "textBox3";
			this.textBox3.PasswordChar = '*';
			this.textBox3.Size = new System.Drawing.Size(141, 20);
			this.textBox3.TabIndex = 8;
			this.textBox3.Text = "Passsword";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 68);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "User:";
			this.label3.Click += new System.EventHandler(this.label3_Click);
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(62, 68);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(141, 20);
			this.textBox2.TabIndex = 6;
			this.textBox2.Text = "Daniel";
			this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(128, 45);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(46, 17);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "SSL";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Port:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server:";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(62, 42);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(60, 20);
			this.numericUpDown1.TabIndex = 2;
			this.numericUpDown1.Value = new decimal(new int[] {
            210,
            0,
            0,
            0});
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(62, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(141, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "gmail.com";
			// 
			// tabPageChart
			// 
			this.tabPageChart.Controls.Add(this.pictureBox1);
			this.tabPageChart.Location = new System.Drawing.Point(4, 22);
			this.tabPageChart.Name = "tabPageChart";
			this.tabPageChart.Size = new System.Drawing.Size(907, 551);
			this.tabPageChart.TabIndex = 2;
			this.tabPageChart.Text = "Chart ";
			this.tabPageChart.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Location = new System.Drawing.Point(-4, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(736, 548);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// tabPage4
			// 
			this.tabPage4.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(907, 551);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Money managment";
			// 
			// AccountInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(912, 571);
			this.Controls.Add(this.tabControl1);
			this.Name = "AccountInfo";
			this.Text = "Account Info";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabPageChart.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}


		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label labelBuyingPower;
		private System.Windows.Forms.TextBox textBoxRealizedProfit;
		private System.Windows.Forms.TextBox textBoxCashValue;
		private System.Windows.Forms.TextBox textBoxBuyingPower;
		private System.Windows.Forms.Label labelRealizedProfitLoss;
		private System.Windows.Forms.Label labelCashValue;
		private System.Windows.Forms.TabPage tabPageChart;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TabPage tabPage4;
		private bool _working;

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{

		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void label3_Click(object sender, EventArgs e)
		{

		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
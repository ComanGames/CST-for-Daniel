using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
//TExt

namespace NinjaTrader.Strategy
{
	public partial class ChartSlopeTrader
	{
	#region VS2010 Controls Paste

		private Panel _mainPanel;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private GroupBox _groupBoxStatusWindow;
		private GroupBox _groupBoxQuantity;
		private NumericUpDown _numericUpDownQuantity;
		private Button _buttonUpdateQuantity;
		private GroupBox _groupBoxSlope;
		private NumericUpDown _numericUpDownSlope;
		private Button _buttonUpdateSlope;
		private Button _buttonClosePosition;
		private Button _buttonManualShort;
		private Button _buttonManualLong;
		private GroupBox _groupBoxPartialProfit;
		private CheckBox _checkBoxEnablePartialProfit;
		private Button _buttonCloseHalfPosition;
		private Label _statusLabel;
		private Label _rr50Label;
		private GroupBox _groupBoxMode;
		private CheckBox _checkBoxEnablePartialProfitAlert;
		private Button _buttonMakeHorizontalLine;
		private GroupBox _groupBoxTrailStop;
		private CheckBox _checkBoxEnableTrailStopAlert;
		private CheckBox _checkBoxEnableTrailStop;
		private CheckBox _checkBoxEnableTrailStopPreAnalyze;
		private CheckBox _checkBoxEnableTrailStopPostAnalyze;
		private NumericUpDown _numericUpDownSwingIndicatorBars;
		private NumericUpDown _numericUpDownStopLevelTicks;
		private NumericUpDown _numericUpDownHorizontalTicks;
		private Label _rr50NameLabel;
		private Label _partialMsgLabel;
		private Label _dynamicTrailingStopMsgLabel;
		private GroupBox _groupBoxStopToEntry;
		private Label _labelPipsToActivateText;
		private Label _labelPipsToActivateTextDistance;
		private NumericUpDown _numericUpDownPipTicksToActivate;
		private NumericUpDown _numericUpDownPipTicksToActivateDistance;
		private Button _buttonManualMoveStop;
		private GroupBox _groupBoxOnBarEntry;
		private GroupBox _groupBoxMail;
		private Label _label4;
		private NumericUpDown _numericUpDownBarEntry;
		private CheckBox _checkBoxEnableBarEntry;
		private CheckBox _checkBoxEnableConsecutive;
		private CheckBox _checkBoxEnableShortLongAlert;
		private Label _stopToEnterMsgLabel;
		private Label _rrLabel;
		private Label _rrNamelabel;
		private Label _label7;
		private Label _label9;
		private Label _label8;
		private Button _buttonClearSelection;
		private RadioButton _radioButtonNone;
		private RadioButton _radioButtonEntryLine;
		private RadioButton _radioButtonPartialProfit;
		private CheckBox _checkBoxOtherCurrency;
		private Label _textBoxOtherCurrency;
		private Button _buttonActivate;
		private Button _buttonInfo;
		private NinjaTrader.Strategy.AccountInfo _windowAccountInfo;
		//Colors
		private readonly Color _deactivateColor = Color.OrangeRed;
		public readonly Color _activateColor = Color.DarkBlue;
		private readonly Color _enabledColor = Color.ForestGreen;
		public readonly Color _disabledColor = Color.LightCoral;

		public void VS2010_InitializeComponent_Form(Control.ControlCollection controlCollection)
		{
			#region Creating FormVariables 

			_groupBoxStatusWindow = new GroupBox();
			_rrLabel = new Label();
			_rr50NameLabel = new Label();
			_rrNamelabel = new Label();
			_stopToEnterMsgLabel = new Label();
			_dynamicTrailingStopMsgLabel = new Label();
			_rr50Label = new Label();
			_statusLabel = new Label();
			_partialMsgLabel = new Label();
			_groupBoxPartialProfit = new GroupBox();
			_checkBoxEnablePartialProfitAlert = new CheckBox();
			_checkBoxOtherCurrency = new CheckBox();
			_textBoxOtherCurrency = new Label();
			_buttonCloseHalfPosition = new Button();
			_checkBoxEnablePartialProfit = new CheckBox();
			_groupBoxQuantity = new GroupBox();
			_buttonUpdateQuantity = new Button();
			_numericUpDownQuantity = new NumericUpDown();
			_groupBoxSlope= new GroupBox();
			_buttonUpdateSlope= new Button();
			_numericUpDownSlope= new NumericUpDown();
			_buttonClosePosition = new Button();
			_buttonManualShort = new Button();
			_buttonManualLong = new Button();
            vScrollBar1 = new VScrollBar();
			_mainPanel = new Panel();
			_groupBoxOnBarEntry = new GroupBox();
			_groupBoxMail = new GroupBox();
			_label4 = new Label();
			_numericUpDownBarEntry = new NumericUpDown();
			_checkBoxEnableBarEntry = new CheckBox();
			_checkBoxEnableConsecutive = new CheckBox();
			_checkBoxEnableShortLongAlert = new CheckBox();
			_groupBoxTrailStop = new GroupBox();
			_label9 = new Label();
			_label8 = new Label();
			_label7 = new Label();
			_numericUpDownSwingIndicatorBars = new NumericUpDown();
			_numericUpDownStopLevelTicks = new NumericUpDown();
			_checkBoxEnableTrailStopAlert = new CheckBox();
			_checkBoxEnableTrailStop = new CheckBox();
			_checkBoxEnableTrailStopPreAnalyze= new CheckBox();
			_checkBoxEnableTrailStopPostAnalyze= new CheckBox();
			_numericUpDownHorizontalTicks = new NumericUpDown();
			_groupBoxStopToEntry = new GroupBox();
			_labelPipsToActivateText = new Label();
			_labelPipsToActivateTextDistance = new Label();
			_numericUpDownPipTicksToActivate = new NumericUpDown();
			_numericUpDownPipTicksToActivateDistance = new NumericUpDown();
			_buttonManualMoveStop = new Button();
			_groupBoxMode = new GroupBox();
			_buttonMakeHorizontalLine = new Button();
			_buttonClearSelection = new Button();
			_buttonActivate = new Button();
			_buttonInfo = new Button();
			_groupBoxStatusWindow.SuspendLayout();
			_groupBoxPartialProfit.SuspendLayout();
			_groupBoxQuantity.SuspendLayout();
			((ISupportInitialize)(_numericUpDownQuantity)).BeginInit();
			_groupBoxSlope.SuspendLayout();
			((ISupportInitialize)(_numericUpDownSlope)).BeginInit();
			_mainPanel.SuspendLayout();
			_groupBoxOnBarEntry.SuspendLayout();
			_groupBoxMail.SuspendLayout();
			((ISupportInitialize)(_numericUpDownBarEntry)).BeginInit();
			_groupBoxTrailStop.SuspendLayout();
			((ISupportInitialize)(_numericUpDownSwingIndicatorBars)).BeginInit();
			((ISupportInitialize)(_numericUpDownStopLevelTicks)).BeginInit();
			((ISupportInitialize)(_numericUpDownHorizontalTicks)).BeginInit();
			_groupBoxStopToEntry.SuspendLayout();
			((ISupportInitialize)(_numericUpDownPipTicksToActivate)).BeginInit();
			_groupBoxMode.SuspendLayout();

			#endregion

			#region Buttons

			// 
			// _buttonClearSelection
			// 
			_buttonClearSelection.BackColor = _disabledColor;
			_buttonClearSelection.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonClearSelection.ForeColor = Color.White;
			_buttonClearSelection.Location = new Point(4, 97);
			_buttonClearSelection.Margin = new Padding(2);
			_buttonClearSelection.Name = "_buttonClearSelection";
			_buttonClearSelection.Size = new Size(144, 27);
			_buttonClearSelection.TabIndex = 8;
			_buttonClearSelection.Text = "CLEAR SELECTION";
			_buttonClearSelection.UseVisualStyleBackColor = false;
			_buttonClearSelection.Click += _buttonClearSelection_Click;

			// 
			// _buttonActivate
			// 

			_buttonActivate.BackColor = _activateColor;
			_buttonActivate.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_buttonActivate.ForeColor = SystemColors.Window;
			_buttonActivate.Location = new Point(4, 69);
			_buttonActivate.Margin = new Padding(2);
			_buttonActivate.Name = "_buttonActivate";
			_buttonActivate.Size = new Size(144, 26);
			_buttonActivate.TabIndex = 9;
			_buttonActivate.Text = "ACTIVATE";
			_buttonActivate.UseVisualStyleBackColor = false;
			_buttonActivate.Click += _buttonActivateClick;
			// 
			// _buttonUpdate
			// 
			_groupBoxMail.Controls.Add(_buttonInfo);
			_buttonInfo.BackColor = _activateColor;
			_buttonInfo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_buttonInfo.ForeColor = SystemColors.Window;
			_buttonInfo.Location = new Point(4, 80);
			_buttonInfo.Margin = new Padding(2);
			_buttonInfo.Name = "_buttonInfo";
			_buttonInfo.Size = new Size(144, 26);
			_buttonInfo.TabIndex = 9;
			_buttonInfo.Text = "INFO Window";
			_buttonInfo.UseVisualStyleBackColor = false;
			_buttonInfo.Click += _buttonInfoClick;
			_buttonInfo.Enabled = false;
			#endregion

			// 
			// groupBox_StatusWindow
			// 
			_groupBoxStatusWindow.BackColor = SystemColors.Control;
			//RR
			_groupBoxStatusWindow.Controls.Add(_rrLabel);
			_groupBoxStatusWindow.Controls.Add(_rr50NameLabel);
			_groupBoxStatusWindow.Controls.Add(_rrNamelabel);
			_groupBoxStatusWindow.Controls.Add(_rr50Label);
			//
			_groupBoxStatusWindow.Controls.Add(_statusLabel);
			_groupBoxStatusWindow.Controls.Add(_partialMsgLabel);
			_groupBoxStatusWindow.Controls.Add(_stopToEnterMsgLabel);
			_groupBoxStatusWindow.Controls.Add(_dynamicTrailingStopMsgLabel);
			_groupBoxStatusWindow.Location = new Point(6, 3);
			_groupBoxStatusWindow.Margin = new Padding(2);
			_groupBoxStatusWindow.Name = "groupBox_StatusWindow";
			_groupBoxStatusWindow.Padding = new Padding(2);
			_groupBoxStatusWindow.Size = new Size(154, 132);
			_groupBoxStatusWindow.TabIndex = 0;
			_groupBoxStatusWindow.TabStop = false;
			_groupBoxStatusWindow.Text = "Status Window";
			// 
			// StopToEMsgLabel
			// 
			_stopToEnterMsgLabel.BackColor = _disabledColor;
			_stopToEnterMsgLabel.BorderStyle = BorderStyle.FixedSingle;
			_stopToEnterMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_stopToEnterMsgLabel.ForeColor = SystemColors.Window;
			_stopToEnterMsgLabel.Location = new Point(78, 68);
			_stopToEnterMsgLabel.Margin = new Padding(2, 0, 2, 0);
			_stopToEnterMsgLabel.Name = "StopToEntyMsgLabel";
			_stopToEnterMsgLabel.Size = new Size(70, 22);
			_stopToEnterMsgLabel.TabIndex = 3;
			_stopToEnterMsgLabel.Text = "StE NOT";
			_stopToEnterMsgLabel.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// DynamicTrailingStopMsgLabel
			// 
			_dynamicTrailingStopMsgLabel.BackColor = _disabledColor;
			_dynamicTrailingStopMsgLabel.BorderStyle = BorderStyle.FixedSingle;
			_dynamicTrailingStopMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_dynamicTrailingStopMsgLabel.ForeColor = SystemColors.Window;
			_dynamicTrailingStopMsgLabel.Location = new Point(6, 68);
			_dynamicTrailingStopMsgLabel.Margin = new Padding(2, 0, 2, 0);
			_dynamicTrailingStopMsgLabel.Name = "DynamicTrailingStopMsgLabel";
			_dynamicTrailingStopMsgLabel.Size = new Size(68, 22);
			_dynamicTrailingStopMsgLabel.TabIndex = 3;
			_dynamicTrailingStopMsgLabel.Text = "DTS NOT";
			_dynamicTrailingStopMsgLabel.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// PartialMsgLabel
			// 
			_partialMsgLabel.BackColor = _disabledColor;
			_partialMsgLabel.BorderStyle = BorderStyle.FixedSingle;
			_partialMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_partialMsgLabel.ForeColor = SystemColors.Window;
			_partialMsgLabel.Location = new Point(6, 43);
			_partialMsgLabel.Margin = new Padding(2, 0, 2, 0);
			_partialMsgLabel.Name = "PartialMsgLabel";
			_partialMsgLabel.Size = new Size(142, 22);
			_partialMsgLabel.TabIndex = 2;
			_partialMsgLabel.Text = "50% TP Disabled";
			_partialMsgLabel.TextAlign = ContentAlignment.MiddleCenter;

			#region RR

			// 
			// RR50NameLabel
			// 
			_rr50NameLabel.AutoSize = true;
			_rr50NameLabel.Location = new Point(84, 90);
			_rr50NameLabel.Margin = new Padding(2, 0, 2, 0);
			_rr50NameLabel.Name = "RR50NameLabel";
			_rr50NameLabel.Size = new Size(55, 13);
			_rr50NameLabel.TabIndex = 2;
			_rr50NameLabel.Text = "R:R - 50%";
			// 
			// RRNamelabel
			// 
			_rrNamelabel.AutoSize = true;
			_rrNamelabel.Location = new Point(7, 90);
			_rrNamelabel.Margin = new Padding(2, 0, 2, 0);
			_rrNamelabel.Name = "RRNamelabel";
			_rrNamelabel.Size = new Size(26, 13);
			_rrNamelabel.TabIndex = 4;
			_rrNamelabel.Text = "R:R";
			// 
			// RRLabel
			// 
			_rrLabel.BackColor = Color.White;
			_rrLabel.BorderStyle = BorderStyle.FixedSingle;
			_rrLabel.Location = new Point(10, 107);
			_rrLabel.Margin = new Padding(2, 0, 2, 0);
			_rrLabel.Name = "RRLabel";
			_rrLabel.Size = new Size(34, 22);
			_rrLabel.TabIndex = 5;
			_rrLabel.Text = "3.46";
			_rrLabel.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// RR50Label
			// 
			_rr50Label.BackColor = Color.White;
			_rr50Label.BorderStyle = BorderStyle.FixedSingle;
			_rr50Label.Location = new Point(87, 107);
			_rr50Label.Margin = new Padding(2, 0, 2, 0);
			_rr50Label.Name = "RR50Label";
			_rr50Label.Size = new Size(34, 22);
			_rr50Label.TabIndex = 3;
			_rr50Label.Text = "3.46";
			_rr50Label.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// StatusLabel
			// 
			_statusLabel.BackColor = _disabledColor;
			_statusLabel.BorderStyle = BorderStyle.FixedSingle;
			_statusLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_statusLabel.ForeColor = Color.White;
			_statusLabel.Location = new Point(6, 18);
			_statusLabel.Margin = new Padding(2, 0, 2, 0);
			_statusLabel.Name = "StatusLabel";
			_statusLabel.Size = new Size(142, 22);
			_statusLabel.TabIndex = 0;
			_statusLabel.Text = "Not Active";
			_statusLabel.TextAlign = ContentAlignment.MiddleCenter;

			#endregion

			// 
			// groupBox_PartialProfit
			// 
			_groupBoxPartialProfit.BackColor = SystemColors.ControlLight;
			_groupBoxPartialProfit.Controls.Add(_buttonCloseHalfPosition);
			_groupBoxPartialProfit.Controls.Add(_checkBoxEnablePartialProfit);
			_groupBoxPartialProfit.Location = new Point(6, 374);
			_groupBoxPartialProfit.Margin = new Padding(2);
			_groupBoxPartialProfit.Name = "groupBox_PartialProfit";
			_groupBoxPartialProfit.Padding = new Padding(2);
			_groupBoxPartialProfit.Size = new Size(154, 69);
			_groupBoxPartialProfit.TabIndex = 5;
			_groupBoxPartialProfit.TabStop = false;
			_groupBoxPartialProfit.Text = "50 % Partial Take Profit";
			// 
			// button_CloseHalfPosition
			// 
			_buttonCloseHalfPosition.BackColor = _disabledColor;
			_buttonCloseHalfPosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonCloseHalfPosition.ForeColor = Color.White;
			_buttonCloseHalfPosition.Location = new Point(6, 19);
			_buttonCloseHalfPosition.Margin = new Padding(2);
			_buttonCloseHalfPosition.Name = "button_CloseHalfPosition";
			_buttonCloseHalfPosition.Size = new Size(144, 27);
			_buttonCloseHalfPosition.TabIndex = 2;
			_buttonCloseHalfPosition.Text = "MANUAL CLOSE 50%";
			_buttonCloseHalfPosition.UseVisualStyleBackColor = false;
			_buttonCloseHalfPosition.Click += _buttonCloseHalfPositionClick;
			// 
			// checkBox_EnablePartialProfit
			// 
			_checkBoxEnablePartialProfit.AutoSize = true;
			_checkBoxEnablePartialProfit.Location = new Point(8, 48);
			_checkBoxEnablePartialProfit.Margin = new Padding(2);
			_checkBoxEnablePartialProfit.Name = "checkBox_EnablePartialProfit";
			_checkBoxEnablePartialProfit.Size = new Size(59, 17);
			_checkBoxEnablePartialProfit.TabIndex = 0;
			_checkBoxEnablePartialProfit.Text = "Enable";
			_checkBoxEnablePartialProfit.UseVisualStyleBackColor = true;
			_checkBoxEnablePartialProfit.CheckedChanged += _checkBoxEnablePartialProfit_Changed;
			// 
			// groupBox_Quantity
			// 
			_groupBoxQuantity.BackColor = SystemColors.ControlLight;
			_groupBoxQuantity.Controls.Add(_buttonUpdateQuantity);
			_groupBoxQuantity.Controls.Add(_numericUpDownQuantity);
			_groupBoxQuantity.Location = new Point(6, 165);
			_groupBoxQuantity.Margin = new Padding(2);
			_groupBoxQuantity.Name = "groupBox_Quantity";
			_groupBoxQuantity.Padding = new Padding(2);
			_groupBoxQuantity.Size = new Size(154, 35);
			_groupBoxQuantity.TabIndex = 4;
			_groupBoxQuantity.TabStop = false;
			_groupBoxQuantity.Text = "Quantity";
			// 
			// button_UpdateQuantity
			// 
			_buttonUpdateQuantity.BackColor = Color.SkyBlue;
			_buttonUpdateQuantity.Location = new Point(8, 12);
			_buttonUpdateQuantity.Margin = new Padding(2);
			_buttonUpdateQuantity.Name = "button_UpdateQuantity";
			_buttonUpdateQuantity.Size = new Size(59, 20);
			_buttonUpdateQuantity.TabIndex = 1;
			_buttonUpdateQuantity.Text = "Update";
			_buttonUpdateQuantity.UseVisualStyleBackColor = false;
		    _buttonUpdateQuantity.Click += _buttonUpdateQuantity_Click;
			// 
			// numericUpDown_Quantity
			// 
			_numericUpDownQuantity.Location = new Point(71, 16);
			_numericUpDownQuantity.Margin = new Padding(2);
			_numericUpDownQuantity.Maximum = 10000000;
			_numericUpDownQuantity.Minimum = 0;
			_numericUpDownQuantity.Name = "numericUpDown_Quantity";
			_numericUpDownQuantity.Size = new Size(79, 12);
			_numericUpDownQuantity.TabIndex = 42;
			_numericUpDownQuantity.TextAlign = HorizontalAlignment.Center;
			_numericUpDownQuantity.Value = new decimal(new[] { 10, 0, 0, 0 });
			// 
			// groupBox_Slope
			// 
			_groupBoxSlope.BackColor = SystemColors.ControlLight;
			_groupBoxSlope.Controls.Add(_buttonUpdateSlope);
			_groupBoxSlope.Controls.Add(_numericUpDownSlope);
			_groupBoxSlope.Location = new Point(6, 130);
			_groupBoxSlope.Margin = new Padding(2);
			_groupBoxSlope.Name = "groupBox_Slope";
			_groupBoxSlope.Padding = new Padding(2);
			_groupBoxSlope.Size = new Size(154, 35);
			_groupBoxSlope.TabIndex = 4;
			_groupBoxSlope.TabStop = false;
			_groupBoxSlope.Text = "Slope";
            // 
			// button_UpdateSlope
			// 
			_buttonUpdateSlope.BackColor = Color.SkyBlue;
			_buttonUpdateSlope.Location = new Point(8, 12);
			_buttonUpdateSlope.Margin = new Padding(2);
			_buttonUpdateSlope.Name = "button_UpdateSlope";
			_buttonUpdateSlope.Size = new Size(59, 20);
			_buttonUpdateSlope.TabIndex = 1;
			_buttonUpdateSlope.Text = "Update";
			_buttonUpdateSlope.UseVisualStyleBackColor = false;
		    _buttonUpdateSlope.Click += _buttonUpdateSlope_Click;

            _numericUpDownSlope.Location = new Point(71, 12);
			_numericUpDownSlope.Margin = new Padding(2);
			_numericUpDownSlope.Maximum = 100;
			_numericUpDownSlope.Minimum = -100;
			_numericUpDownSlope.Name = "numericUpDown_Slope";
			_numericUpDownSlope.Size = new Size(79, 20);
			_numericUpDownSlope.TabIndex = 41;
			_numericUpDownSlope.TextAlign = HorizontalAlignment.Center;
		    _numericUpDownBarEntry.Value = 0.5M;
			_numericUpDownSlope.Increment = 1;
			// 
			// button_ClosePosition
			// 
			_buttonClosePosition.BackColor = _disabledColor;
			_buttonClosePosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonClosePosition.ForeColor = Color.White;
			_buttonClosePosition.Location = new Point(4, 154);
			_buttonClosePosition.Margin = new Padding(2);
			_buttonClosePosition.Name = "button_ClosePosition";
			_buttonClosePosition.Size = new Size(144, 27);
			_buttonClosePosition.TabIndex = 3;
			_buttonClosePosition.Text = "MANUAL CLOSE 100%";
			_buttonClosePosition.UseVisualStyleBackColor = false;
			_buttonClosePosition.Click += _buttonClosePositionClick;
			// 
			// button_ManualShort
			// 
			_buttonManualShort.BackColor = Color.MediumOrchid;
			_buttonManualShort.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonManualShort.ForeColor = Color.White;
			_buttonManualShort.Location = new Point(79, 41);
			_buttonManualShort.Margin = new Padding(2);
			_buttonManualShort.Name = "button_ManualShort";
			_buttonManualShort.Size = new Size(69, 27);
			_buttonManualShort.TabIndex = 2;
			_buttonManualShort.Text = "SHORT";
			_buttonManualShort.UseVisualStyleBackColor = false;
			_buttonManualShort.Click += _buttonShortClick;
			// 
			// button_ManualLong
			// 
			_buttonManualLong.BackColor = Color.FromArgb(194, 194, 44);
			_buttonManualLong.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonManualLong.ForeColor = Color.White;
			_buttonManualLong.Location = new Point(4, 41);
			_buttonManualLong.Margin = new Padding(2);
			_buttonManualLong.Name = "button_ManualLong";
			_buttonManualLong.Size = new Size(70, 27);
			_buttonManualLong.TabIndex = 1;
			_buttonManualLong.Text = "LONG";
			_buttonManualLong.UseVisualStyleBackColor = false;
			_buttonManualLong.Click += _buttonLongClick;
            // 
            // vScrollBar1
            // 
            //Ilona my changes are here.
            this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollBar1.Location = new System.Drawing.Point(160, -1);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(20, 900);
            this.vScrollBar1.TabIndex = 0;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);

            // 
            // MainPanel
            // 
            _mainPanel.BackColor = SystemColors.Control;
			_mainPanel.Controls.Add(_groupBoxOnBarEntry);
			_mainPanel.Controls.Add(_groupBoxMail);
			_mainPanel.Controls.Add(_groupBoxStopToEntry);
			_mainPanel.Controls.Add(_groupBoxTrailStop);
			_mainPanel.Controls.Add(_groupBoxStatusWindow);
			_mainPanel.Controls.Add(_groupBoxPartialProfit);
			_mainPanel.Controls.Add(_groupBoxQuantity);
			_mainPanel.Controls.Add(_groupBoxSlope);
			_mainPanel.Controls.Add(_groupBoxMode);
		    _mainPanel.Controls.Add(vScrollBar1);

			_mainPanel.Dock = DockStyle.Right;
			_mainPanel.Location = new Point(931, 0);
			_mainPanel.Margin = new Padding(2);
			_mainPanel.Name = "MainPanel";
			_mainPanel.Size = new Size(185, 831);
			_mainPanel.TabIndex = 1;
			// 
			// groupBox1
			// 
			_groupBoxOnBarEntry.Controls.Add(_label4);
			_groupBoxOnBarEntry.Controls.Add(_numericUpDownBarEntry);
			_groupBoxOnBarEntry.Controls.Add(_checkBoxEnableBarEntry);
			_groupBoxOnBarEntry.Controls.Add(_checkBoxEnableConsecutive);
			_groupBoxOnBarEntry.Location = new Point(6, 702);
			_groupBoxOnBarEntry.Margin = new Padding(2);
			_groupBoxOnBarEntry.Name = "groupBox1";
			_groupBoxOnBarEntry.Padding = new Padding(2);
			_groupBoxOnBarEntry.Size = new Size(154, 67);
			_groupBoxOnBarEntry.TabIndex = 2;
			_groupBoxOnBarEntry.TabStop = false;
			_groupBoxOnBarEntry.Text = "Bar Entry";
			// 
			// groupBox0
			//
			// 
			_groupBoxMail.Controls.Add(_checkBoxEnableShortLongAlert);
			_groupBoxMail.Location = new Point(6, 770);
			_groupBoxMail.Margin = new Padding(2);
			_groupBoxMail.Name = "groupBox0";
			_groupBoxMail.Padding = new Padding(2);
			_groupBoxMail.Size = new Size(154, 120);
			_groupBoxMail.TabIndex = 50;
			_groupBoxMail.TabStop = false;
			_groupBoxMail.Text = "Mail Settings";
			// 
			// label4
			// 
			_label4.AutoSize = true;
			_label4.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label4.Location = new Point(84, 21);
			_label4.Margin = new Padding(2, 0, 2, 0);
			_label4.Name = "label4";
			_label4.Size = new Size(28, 13);
			_label4.TabIndex = 13;
			_label4.Text = "Bars";
			_label4.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numericUpDown_BarEntry
			// 
			_numericUpDownBarEntry.Location = new Point(116, 18);
			_numericUpDownBarEntry.Margin = new Padding(2);
			_numericUpDownBarEntry.Maximum = new decimal(new[] { 99, 0, 0, 0 });
			_numericUpDownBarEntry.Name = "numericUpDown_BarEntry";
			_numericUpDownBarEntry.Size = new Size(34, 20);
			_numericUpDownBarEntry.TabIndex = 12;
			_numericUpDownBarEntry.TextAlign = HorizontalAlignment.Center;
			_numericUpDownBarEntry.Value = new decimal(new[] { 1, 0, 0, 0 });
			// 
			// checkBox_EnableBarEntry
			// 
			_checkBoxEnableBarEntry.AutoSize = true;
			_checkBoxEnableBarEntry.Location = new Point(10, 20);
			_checkBoxEnableBarEntry.Margin = new Padding(2);
			_checkBoxEnableBarEntry.Name = "checkBox_EnableBarEntry";
			_checkBoxEnableBarEntry.Size = new Size(59, 17);
			_checkBoxEnableBarEntry.TabIndex = 5;
			_checkBoxEnableBarEntry.Text = "Enable";
			_checkBoxEnableBarEntry.UseVisualStyleBackColor = true;
			_checkBoxEnableBarEntry.CheckedChanged += _checkBoxEnableBarEntry_CheckedChanged;
			// 
			// checkBox_EnableConsecutive
			// 
			_checkBoxEnableConsecutive.AutoSize = true;
			_checkBoxEnableConsecutive.Location = new Point(10, 43);
			_checkBoxEnableConsecutive.Margin = new Padding(2);
			_checkBoxEnableConsecutive.Name = "checkBox_EnableConsecutive";
			_checkBoxEnableConsecutive.Size = new Size(59, 17);
			_checkBoxEnableConsecutive.TabIndex = 5;
			_checkBoxEnableConsecutive.Text = "Consecutive";
			_checkBoxEnableConsecutive.UseVisualStyleBackColor = true;
			_checkBoxEnableConsecutive.CheckedChanged += _checkBoxEnableConsecutive_CheckedChanged;
			// 			 
			// checkBox_EnableBarEntry
			// 
			_checkBoxEnableShortLongAlert.AutoSize = true;
			_checkBoxEnableShortLongAlert.Location = new Point(10, 20);
			_checkBoxEnableShortLongAlert.Margin = new Padding(2);
			_checkBoxEnableShortLongAlert.Name = "checkBox_EnableMailEntry";
			_checkBoxEnableShortLongAlert.Size = new Size(140, 17);
			_checkBoxEnableShortLongAlert.TabIndex = 5;
			_checkBoxEnableShortLongAlert.Text = "Enable LONG/SHORT";
			_checkBoxEnableShortLongAlert.UseVisualStyleBackColor = true;
			_checkBoxEnableShortLongAlert.Checked = true;
			// 
			// checkBox_EnablePartialProfitAlert
			// 
			_groupBoxMail.Controls.Add(_checkBoxEnablePartialProfitAlert);
			_checkBoxEnablePartialProfitAlert.AutoSize = true;
			_checkBoxEnablePartialProfitAlert.Location = new Point(10, 40);
			_checkBoxEnablePartialProfitAlert.Margin = new Padding(2);
			_checkBoxEnablePartialProfitAlert.Name = "checkBox_EnablePartialProfitAlert";
			_checkBoxEnablePartialProfitAlert.Size = new Size(140, 17);
			_checkBoxEnablePartialProfitAlert.TabIndex = 5;
			_checkBoxEnablePartialProfitAlert.Text = "Enable Partial Profit";
			_checkBoxEnablePartialProfitAlert.UseVisualStyleBackColor = true;
			_checkBoxEnablePartialProfitAlert.CheckedChanged += _checkBoxEnablePartialProfitAlert_CheckedChanged;
			// 
			// checkBox_EnableTrailStopAlert
			// 
			_groupBoxMail.Controls.Add(_checkBoxEnableTrailStopAlert);
			_checkBoxEnableTrailStopAlert.AutoSize = true;
			_checkBoxEnableTrailStopAlert.Location = new Point(10, 60);
			_checkBoxEnableTrailStopAlert.Margin = new Padding(2);
			_checkBoxEnableTrailStopAlert.Name = "checkBox_EnableTrailStopAlert";
			_checkBoxEnableTrailStopAlert.Size = new Size(140, 17);
			_checkBoxEnableTrailStopAlert.TabIndex = 5;
			_checkBoxEnableTrailStopAlert.Text = "Enable DTS";
			_checkBoxEnableTrailStopAlert.UseVisualStyleBackColor = true;
			_checkBoxEnableTrailStopAlert.CheckedChanged += _checkBoxEnableTrailStopAlert_CheckedChanged;
			// 
			// groupBox_TrailStop
			// 
			_groupBoxTrailStop.BackColor = SystemColors.ControlLight;
			_groupBoxTrailStop.Controls.Add(_label9);
			_groupBoxTrailStop.Controls.Add(_label8);
			_groupBoxTrailStop.Controls.Add(_label7);
			_groupBoxTrailStop.Controls.Add(_numericUpDownSwingIndicatorBars);
			_groupBoxTrailStop.Controls.Add(_numericUpDownStopLevelTicks);
			_groupBoxTrailStop.Controls.Add(_numericUpDownHorizontalTicks);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStop);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStopPreAnalyze);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStopPostAnalyze);
			_groupBoxTrailStop.Location = new Point(6, 599);
			_groupBoxTrailStop.Margin = new Padding(2);
			_groupBoxTrailStop.Name = "groupBox_TrailStop";
			_groupBoxTrailStop.Padding = new Padding(2);
			_groupBoxTrailStop.Size = new Size(154, 103);
			_groupBoxTrailStop.TabIndex = 7;
			_groupBoxTrailStop.TabStop = false;
			_groupBoxTrailStop.Text = "Dynamic Trail Stop";
			// 
			// label9
			// 
			_label9.AutoSize = true;
			_label9.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label9.Location = new Point(5, 82);
			_label9.Margin = new Padding(2, 0, 2, 0);
			_label9.Name = "label9";
			_label9.Size = new Size(109, 13);
			_label9.TabIndex = 16;
			_label9.Text = "Horizontal [pips/ticks]";
			_label9.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label8
			// 
			_label8.AutoSize = true;
			_label8.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label8.Location = new Point(5, 61);
			_label8.Margin = new Padding(2, 0, 2, 0);
			_label8.Name = "label8";
			_label8.Size = new Size(113, 13);
			_label8.TabIndex = 15;
			_label8.Text = "Stop Level [pips/ticks]";
			_label8.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			_label7.AutoSize = true;
			_label7.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label7.Location = new Point(5, 39);
			_label7.Margin = new Padding(2, 0, 2, 0);
			_label7.Name = "label7";
			_label7.Size = new Size(109, 13);
			_label7.TabIndex = 14;
			_label7.Text = "Swing Indicator [bars]";
			_label7.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numericUpDown_SwingIndicatorBars
			// 
			_numericUpDownSwingIndicatorBars.Location = new Point(118, 37);
			_numericUpDownSwingIndicatorBars.Margin = new Padding(2);
			_numericUpDownSwingIndicatorBars.Maximum = new decimal(new[] { 99, 0, 0, 0 });
			_numericUpDownSwingIndicatorBars.Name = "numericUpDown_SwingIndicatorBars";
			_numericUpDownSwingIndicatorBars.Size = new Size(34, 20);
			_numericUpDownSwingIndicatorBars.TabIndex = 11;
			_numericUpDownSwingIndicatorBars.TextAlign = HorizontalAlignment.Center;
			_numericUpDownSwingIndicatorBars.Value = new decimal(new[] { 4, 0, 0, 0 }); // 
			// numericUpDown_HorizonCrossTicks
			// 
			_numericUpDownStopLevelTicks.Location = new Point(118, 58);
			_numericUpDownStopLevelTicks.Margin = new Padding(2);
			_numericUpDownStopLevelTicks.Maximum = new decimal(new[] { 99, 0, 0, 0 });
			_numericUpDownStopLevelTicks.Name = "numericUpDown_HorizCrossTicks";
			_numericUpDownStopLevelTicks.Size = new Size(34, 20);
			_numericUpDownStopLevelTicks.TabIndex = 12;
			_numericUpDownStopLevelTicks.TextAlign = HorizontalAlignment.Center;
			_numericUpDownStopLevelTicks.Value = new decimal(new[] { 4, 0, 0, 0 });
			// 
			// checkBox_EnableTrailStop
			// 
			_checkBoxEnableTrailStop.AutoSize = true;
			_checkBoxEnableTrailStop.Location = new Point(9, 18);
			_checkBoxEnableTrailStop.Margin = new Padding(2);
			_checkBoxEnableTrailStop.Name = "checkBox_EnableTrailStop";
			_checkBoxEnableTrailStop.Size = new Size(30, 17);
			_checkBoxEnableTrailStop.TabIndex = 4;
			_checkBoxEnableTrailStop.Text = "Enable";
			_checkBoxEnableTrailStop.UseVisualStyleBackColor = true;
			_checkBoxEnableTrailStop.CheckedChanged += _checkBoxEnableTrailStopChanged;
			// 
			// checkBox_EnableTrailStop
			// 
			_checkBoxEnableTrailStopPreAnalyze.AutoSize = true;
			_checkBoxEnableTrailStopPreAnalyze.Location = new Point(68, 18);
			_checkBoxEnableTrailStopPreAnalyze.Margin = new Padding(2);
			_checkBoxEnableTrailStopPreAnalyze.Name = "checkBox_EnableTrailStop";
			_checkBoxEnableTrailStopPreAnalyze.Size = new Size(20, 17);
			_checkBoxEnableTrailStopPreAnalyze.TabIndex = 4;
			_checkBoxEnableTrailStopPreAnalyze.Text = "Pre";
			_checkBoxEnableTrailStopPreAnalyze.UseVisualStyleBackColor = true;
			_checkBoxEnableTrailStopPreAnalyze.CheckedChanged += _checkBoxEnableTrailStopPreAnalyzeChanged;
			// 
			// checkBox_EnableTrailStop
			// 
			_checkBoxEnableTrailStopPostAnalyze.AutoSize = true;
			_checkBoxEnableTrailStopPostAnalyze.Location = new Point(110, 18);
			_checkBoxEnableTrailStopPostAnalyze.Margin = new Padding(2);
			_checkBoxEnableTrailStopPostAnalyze.Name = "checkBox_EnableTrailStop";
			_checkBoxEnableTrailStopPostAnalyze.Size = new Size(30, 17);
			_checkBoxEnableTrailStopPostAnalyze.TabIndex = 4;
			_checkBoxEnableTrailStopPostAnalyze.Text = "Post";
			_checkBoxEnableTrailStopPostAnalyze.UseVisualStyleBackColor = true;
			_checkBoxEnableTrailStopPostAnalyze.Enabled = false;
			//
			// Text Box what present other Currency
			// 
			_textBoxOtherCurrency.Location = new Point(80, 18);
			_textBoxOtherCurrency.Margin = new Padding(2);
			_textBoxOtherCurrency.Name = "TextBoxOtherCurrency";
			_textBoxOtherCurrency.Size = new Size(40, 17);
			_textBoxOtherCurrency.Text = "6A";

			// 
			// numericUpDown_StopLevelTicks
			// 
			_numericUpDownHorizontalTicks.Location = new Point(118, 79);
			_numericUpDownHorizontalTicks.Margin = new Padding(2);
			_numericUpDownHorizontalTicks.Maximum = new decimal(new[] { 99, 0, 0, 0 });
			_numericUpDownHorizontalTicks.Name = "numericUpDown_StopLevelTicks";
			_numericUpDownHorizontalTicks.Size = new Size(34, 20);
			_numericUpDownHorizontalTicks.TabIndex = 13;
			_numericUpDownHorizontalTicks.TextAlign = HorizontalAlignment.Center;
			_numericUpDownHorizontalTicks.Value = new decimal(new[] { 9, 0, 0, 0 });
			_radioButtonNone = new RadioButton();
			_radioButtonEntryLine = new RadioButton();
			_radioButtonPartialProfit = new RadioButton();

			_radioButtonNone.AutoSize = true;
			_radioButtonNone.Location = new Point(5, 94);
			_radioButtonNone.Name = "radioButtonNone";
			_radioButtonNone.Size = new Size(51, 17);
			_radioButtonNone.TabIndex = 0;
			_radioButtonNone.TabStop = true;
			_radioButtonNone.Text = "None";
			_radioButtonNone.UseVisualStyleBackColor = true;
			_radioButtonNone.CheckedChanged += _radioBoxNone;
			_radioButtonNone.Checked = true;

			_radioButtonEntryLine.AutoSize = true;
			_radioButtonEntryLine.Location = new Point(5, 112);
			_radioButtonEntryLine.Name = "radioButtonEntryLine";
			_radioButtonEntryLine.Size = new Size(72, 17);
			_radioButtonEntryLine.TabIndex = 1;
			_radioButtonEntryLine.TabStop = true;
			_radioButtonEntryLine.Text = "Entry Line";
			_radioButtonEntryLine.UseVisualStyleBackColor = true;
			_radioButtonEntryLine.CheckedChanged += _radioBoxEntryLine;

			_radioButtonPartialProfit.AutoSize = true;
			_radioButtonPartialProfit.Location = new Point(5, 130);
			_radioButtonPartialProfit.Name = "radioButtonPartialProfit";
			_radioButtonPartialProfit.Size = new Size(81, 17);
			_radioButtonPartialProfit.TabIndex = 2;
			_radioButtonPartialProfit.TabStop = true;
			_radioButtonPartialProfit.Text = "Partial Profit";
			_radioButtonPartialProfit.UseVisualStyleBackColor = true;
			_radioButtonPartialProfit.CheckedChanged += _radioBoxPartialProfit;
			// 
			// groupBox_StopToEntry
			// 
			_groupBoxStopToEntry.Controls.Add(_radioButtonNone);
			_groupBoxStopToEntry.Controls.Add(_radioButtonEntryLine);
			_groupBoxStopToEntry.Controls.Add(_radioButtonPartialProfit);
			_groupBoxStopToEntry.Controls.Add(_labelPipsToActivateText);
			_groupBoxStopToEntry.Controls.Add(_labelPipsToActivateTextDistance);
			_groupBoxStopToEntry.Controls.Add(_numericUpDownPipTicksToActivate);
			_groupBoxStopToEntry.Controls.Add(_numericUpDownPipTicksToActivateDistance);
			_groupBoxStopToEntry.Controls.Add(_buttonManualMoveStop);
			_groupBoxStopToEntry.Location = new Point(6, 446);
			_groupBoxStopToEntry.Margin = new Padding(2);
			_groupBoxStopToEntry.Name = "groupBox_StopToEntry";
			_groupBoxStopToEntry.Padding = new Padding(2);
			_groupBoxStopToEntry.Size = new Size(154, 152);
			_groupBoxStopToEntry.TabIndex = 2;
			_groupBoxStopToEntry.TabStop = false;
			_groupBoxStopToEntry.Text = "Stop to Entry/Partial Profit";
			// 
			// label3
			// 
			_labelPipsToActivateText.AutoSize = true;
			_labelPipsToActivateText.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_labelPipsToActivateText.Location = new Point(5, 54);
			_labelPipsToActivateText.Margin = new Padding(2, 0, 2, 0);
			_labelPipsToActivateText.Name = "label3";
			_labelPipsToActivateText.Size = new Size(111, 13);
			_labelPipsToActivateText.TabIndex = 13;
			_labelPipsToActivateText.Text = "Pips to activate";
			_labelPipsToActivateText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			_labelPipsToActivateTextDistance.AutoSize = true;
			_labelPipsToActivateTextDistance.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_labelPipsToActivateTextDistance.Location = new Point(5, 75);
			_labelPipsToActivateTextDistance.Margin = new Padding(2, 0, 2, 0);
			_labelPipsToActivateTextDistance.Name = "Pips to activate Distance";
			_labelPipsToActivateTextDistance.Size = new Size(111, 13);
			_labelPipsToActivateTextDistance.TabIndex = 13;
			_labelPipsToActivateTextDistance.Text = "Distance";
			_labelPipsToActivateTextDistance.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numericUpDown_PipTicksToActivate
			// 
			_numericUpDownPipTicksToActivate.Location = new Point(90, 51);
			_numericUpDownPipTicksToActivate.Margin = new Padding(2);
			_numericUpDownPipTicksToActivate.Maximum = new decimal(new[] { 10000, 0, 0, 0 });
			_numericUpDownPipTicksToActivate.Name = "numericUpDown_PipTicksToActivate";
			_numericUpDownPipTicksToActivate.Size = new Size(60, 20);
			_numericUpDownPipTicksToActivate.TabIndex = 12;
			_numericUpDownPipTicksToActivate.TextAlign = HorizontalAlignment.Center;
			_numericUpDownPipTicksToActivate.Value = new decimal(new[] { 10, 0, 0, 0 });
			// 
			// numericUpDown_PipTicksToActivateDistance
			// 
			_numericUpDownPipTicksToActivateDistance.Location = new Point(90, 74);
			_numericUpDownPipTicksToActivateDistance.Maximum = new decimal(new[] { 10000000, 0, 0, 0 });
			_numericUpDownPipTicksToActivateDistance.Minimum = 0;
			_numericUpDownPipTicksToActivateDistance.Size = new Size(60, 20);
			_numericUpDownPipTicksToActivateDistance.TextAlign = HorizontalAlignment.Center;
			_numericUpDownPipTicksToActivateDistance.Value = new decimal(new[] { 5, 0, 0, 0 });
			// 
			// button_ManualMoveStop
			// 
			_buttonManualMoveStop.BackColor = _disabledColor;
			_buttonManualMoveStop.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonManualMoveStop.ForeColor = Color.White;
			_buttonManualMoveStop.Location = new Point(5, 19);
			_buttonManualMoveStop.Margin = new Padding(2);
			_buttonManualMoveStop.Name = "button_ManualMoveStop";
			_buttonManualMoveStop.Size = new Size(145, 27);
			_buttonManualMoveStop.TabIndex = 3;
			_buttonManualMoveStop.Text = "MANUAL MOVE STOP";
			_buttonManualMoveStop.UseVisualStyleBackColor = false;
			_buttonManualMoveStop.Click += _buttonManualMoveStopOnClick;
			// 
			// groupBox_Mode
			// 
			_groupBoxMode.BackColor = SystemColors.Control;
			_groupBoxMode.Controls.Add(_buttonActivate);
			_groupBoxMode.Controls.Add(_buttonClearSelection);
			_groupBoxMode.Controls.Add(_buttonMakeHorizontalLine);
			_groupBoxMode.Controls.Add(_checkBoxOtherCurrency);
			_groupBoxMode.Controls.Add(_textBoxOtherCurrency);
			_groupBoxMode.Controls.Add(_buttonManualLong);
			_groupBoxMode.Controls.Add(_buttonManualShort);
			_groupBoxMode.Controls.Add(_buttonClosePosition);
			_groupBoxMode.Location = new Point(6, 181);
			_groupBoxMode.Margin = new Padding(2);
			_groupBoxMode.Name = "groupBox_Mode";
			_groupBoxMode.Padding = new Padding(2);
			_groupBoxMode.Size = new Size(154, 189);
			_groupBoxMode.TabIndex = 6;
			_groupBoxMode.TabStop = false;
			_groupBoxMode.Text = "Mode";
			// 
			// label1
			// 
			_checkBoxOtherCurrency.AutoSize = true;
			_checkBoxOtherCurrency.Location = new Point(6, 21);
			_checkBoxOtherCurrency.Margin = new Padding(2, 0, 2, 0);
			_checkBoxOtherCurrency.Name = "_checkBoxOtherCurrency";
			_checkBoxOtherCurrency.Size = new Size(63, 13);
			_checkBoxOtherCurrency.TabIndex = 7;
			_checkBoxOtherCurrency.Text = "Trade on:";
			_checkBoxOtherCurrency.CheckedChanged += _checkBoxOtherCurrencyOnCheckedChanged;
			//
			//Text box for name of other currency
			// 
			_textBoxOtherCurrency.Location = new Point(80, 21);
			_textBoxOtherCurrency.Margin = new Padding(2, 0, 2, 0);
			_textBoxOtherCurrency.Name = "TextBoxOtherCurreny";
			_textBoxOtherCurrency.Size = new Size(68, 13);
			_textBoxOtherCurrency.TabIndex = 8;
			_textBoxOtherCurrency.Text = _otherInstrumentName;
			_textBoxOtherCurrency.Enabled = false;

			// button_MakeHorizonLine
			// 
			_buttonMakeHorizontalLine.BackColor = Color.SkyBlue;
			_buttonMakeHorizontalLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_buttonMakeHorizontalLine.Location = new Point(4, 126);
			_buttonMakeHorizontalLine.Margin = new Padding(2);
			_buttonMakeHorizontalLine.Name = "button_MakeHorizLine";
			_buttonMakeHorizontalLine.Size = new Size(144, 26);
			_buttonMakeHorizontalLine.TabIndex = 4;
			_buttonMakeHorizontalLine.Text = "MAKE HORIZONTAL";
			_buttonMakeHorizontalLine.UseVisualStyleBackColor = false;
			_buttonMakeHorizontalLine.Click += button_MakeHorizontalLine_Click;

			//Some text to put here
			_groupBoxStatusWindow.ResumeLayout(false);
			_groupBoxStatusWindow.PerformLayout();
			_groupBoxPartialProfit.ResumeLayout(false);
			_groupBoxPartialProfit.PerformLayout();
			_groupBoxQuantity.ResumeLayout(false);
			((ISupportInitialize)(_numericUpDownQuantity)).EndInit();
			_mainPanel.ResumeLayout(false);
			_groupBoxOnBarEntry.ResumeLayout(false);
			_groupBoxOnBarEntry.PerformLayout();
			((ISupportInitialize)(_numericUpDownBarEntry)).EndInit();

			_groupBoxTrailStop.ResumeLayout(false);
			_groupBoxTrailStop.PerformLayout();
			((ISupportInitialize)(_numericUpDownSwingIndicatorBars)).EndInit();
			((ISupportInitialize)(_numericUpDownStopLevelTicks)).EndInit();
			((ISupportInitialize)(_numericUpDownHorizontalTicks)).EndInit();
			_groupBoxStopToEntry.ResumeLayout(false);
			_groupBoxStopToEntry.PerformLayout();
			((ISupportInitialize)(_numericUpDownPipTicksToActivate)).EndInit();
			_groupBoxMode.ResumeLayout(false);
			_groupBoxMode.PerformLayout();
			controlCollection.Add(_mainPanel);
		}

	    private void _buttonUpdateSlope_Click(object sender, EventArgs e)
	    {
            IRay ray;
            if (GetSelectedRay(out ray))
            {
//                MessageBox.Show();
                MakeRaySlope(ray,_numericUpDownSlope.Value);
                // ReSharper disable once UseNullPropagation
                if (_currentRayContainer != null)
                {

                }
                // ReSharper disable once UseNullPropagation
                if (_currentRayContainer != null)
                    _currentRayContainer.Update();
                UpdateForms();
            }
            else
                MassageIfLineNotSelected();
	    }
        private int scrollMutliplayer = 5;
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
//            textBox1.Text = vScrollBar1.Value.ToString();
//            groupBox1.Location = new Point(groupBox1.Location.X, startBoxPostion - (vScrollBar1.Value * scrollMutliplayer));
        }

        private void VS2010_UnInitializeComponent()
		{
			if (_mainPanel != null)
			{
				// Remove and Dispose
				ChartControl.Controls.Remove(_mainPanel);
				_mainPanel.Dispose();
				_mainPanel = null;
			}
		}
		private void _buttonUpdateQuantity_Click(object sender, EventArgs e)
		{
			if (_strategyState == StrategyState.NotActive || _strategyState == StrategyState.Exit)
			{
				MessageBox.Show("You are in position or you are not active yet");
				return;
			}
			enterQuantity = (int) _numericUpDownQuantity.Value;
		}

		private void _checkBoxEnableTrailStopPreAnalyzeChanged(object sender, EventArgs e)
		{
		}

		private void _checkBoxEnableConsecutive_CheckedChanged(object sender, EventArgs e)
		{
		}

		#endregion

		private void _buttonLongClick(object sender, EventArgs e)
		{
			IRay ray;
			//Check are we able to use ray
			if (!CanUseRay(out ray))
				return;

			//We crating the container for the rays
			// ReSharper disable once UseNullPropagation
			if (_currentRayContainer != null)
				_currentRayContainer.Clear();
			_currentRayContainer = new RayContainer(MarketPosition.Long, ray, this, _checkBoxEnablePartialProfit.Checked,
				_checkBoxEnableTrailStop.Checked);
			_currentRayContainer.Update();
			UpdateForms();
		}

		private void _buttonShortClick(object sender, EventArgs e)
		{
			IRay ray;
			//Check are we able to use ray
			if (!CanUseRay(out ray))
				return;
			//We are creating container for a rays
			// ReSharper disable once UseNullPropagation
			if (_currentRayContainer != null)
				_currentRayContainer.Clear();
			_currentRayContainer = new RayContainer(MarketPosition.Short, ray, this, _checkBoxEnablePartialProfit.Checked,
				_checkBoxEnableTrailStop.Checked);
			_currentRayContainer.Update();
			UpdateForms();

		}

		private void _buttonInfoClick(object sender, EventArgs eventArgs)
		{
			MessageBox.Show("NOthing happens ");
//			try
//			{
//				_windowAccountInfo = new AccountInfo(this);
//				_windowAccountInfo.Show();
//			}
//			catch (Exception e)
//			{
//				MessageBox.Show(e.Message+e.StackTrace);
//			}
		}
	}
}

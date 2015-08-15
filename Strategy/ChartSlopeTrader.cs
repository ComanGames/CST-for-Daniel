#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using NinjaTrader.Cbi;
using System.Windows.Forms;
using NinjaTrader.Gui.Chart;

#endregion

namespace NinjaTrader.Strategy
{

    [Description("Trade Slope Lines")]
    public class ChartSlopeTrader : Strategy
    {

		#region VS2010 Controls Paste

		private GroupBox groupBox_StatusWindow;
        private GroupBox groupBox_Quantity;
        private NumericUpDown numericUpDown_Quantity;
        private Button button_ClosePosition;
        private Button button_ManualShort;
        private Button button_ManualLong;
        private Panel MainPanel;
        private Button button_UpdateQuantity;
        private GroupBox groupBox_PartialProfit;
        private CheckBox checkBox_EnablePartialProfit;
        private Button button_CloseHalfPosition;
        private Label StatusLabel;
        private Label RR50Label;
        private ComboBox comboBox_OrderType;
        private GroupBox groupBox_Mode;
        private CheckBox checkBox_EnablePartialProfitAlert;
        private Button button_MakeHorizLine;
        private GroupBox groupBox_TrailStop;
        private CheckBox checkBox_EnableTrailStopAlert;
        private CheckBox checkBox_EnableTrailStop;
        private NumericUpDown numericUpDown_SwingIndicatorBars;
        private NumericUpDown numericUpDown_HorizCrossTicks;
        private NumericUpDown numericUpDown_StopLevelTicks;
        private Label label1;
        private Label RR50NameLabel;
        private Label PartialMsgLabel;
        private Label DynamicTrailingStopMsgLabel;
        private GroupBox groupBox_StopToEntry;
        private Label label3;
        private NumericUpDown numericUpDown_PipTicksToActivate;
        private Button button_ManualMoveStop;
        private CheckBox checkBox_MoveSL_PartialProfitLine;
        private CheckBox checkBox_MoveSL_EntryLine;
        private GroupBox groupBox1;
        private Label label4;
        private NumericUpDown numericUpDown_BarEntry;
        private CheckBox checkBox_EnableBarEntry;
        private Label StopToEntyMsgLabel;
        private Label RRLabel;
        private Label RRNamelabel;
        private Label label7;
        private Label label9;
        private Label label8;
        private Button button1;
        private Button button2;

        #endregion

        #region Variables
        
        // Toolstrip button
        private const string strPVProfMenutxt = "SlopeTrader";
        private ToolStrip MyToolStrip = null;
        private ToolStripButton MyTsButton = null;
        private ToolStripSeparator MyTsSeparator;

	    private bool _showPanel;
	    private Color _enabledColor = Color.ForestGreen;
	    private Color _disabledColor = Color.LightCoral;
	    private bool _isCountingRR;
	    private bool _isRRAfter;
	    private bool doBigger;
	    private string _pleaseSelectRay = "Please Select Ray";
	    private RayContainer _currentRayContainer;
	    #endregion

        protected override void Initialize()
        {
            CalculateOnBarClose = false;
	        Enabled = true;
        }


        protected override void OnStartUp()
        {
	        // Initialize Forms
	        VS2010_InitializeComponent_Form();
	        // Add Toolbar Button
	        ButtonToThetop();
        }

        protected override void OnTermination()
        {
            if (this.MainPanel != null)
            {
                // Remove and Dispose
                ChartControl.Controls.Remove(this.MainPanel);
                this.MainPanel.Dispose();
                this.MainPanel = null;
            }

            // Remove My Toolstrip Button
            if ((MyToolStrip != null) && (MyTsButton != null))
            {
                MyToolStrip.Items.Remove(MyTsButton);
                MyToolStrip.Items.Remove(MyTsSeparator);
            }

            MyToolStrip = null;
            MyTsButton = null;
            MyTsSeparator = null;
        }

        protected override void OnBarUpdate()
        {
	       UpdateRR();
		}
		private void UpdateRR()
	    {
			//Todo: write those RR functionlity
			if (_isCountingRR)
			{

				double Risk =	0;
				double Reward =	1;
			    if (!_isRRAfter)
				{
					//Before a LONG or SHORT position has been bough or sold:
					//Formula(before):
					//Risk = abs([Entry Line] - [Stop])
					//Reward = abs([Profit Target] - [Entry Line]).

			    }
			    else
			    {
					//After we are in a LONG or SHORT position, looking for a profit:
					//Formula(after):
					//Risk = abs([Current Position Price] - [Stop]),
					//Reward = abs([Profit Target] - [Current
				//position price])
			    }

				if(Reward!=null)
					RRLabel.Text = (Risk/Reward).ToString();

			}
			//Todo: Write the 50% RR funinality 
			//R: R 50 % indicator: Same as above but when 50 % Partial Take Profit line is enabled use
			//[50 % Partial Take Profit] Line in the calculation. That way we can see R:R for both lines
	    }

		#region Misc Routines

	    public bool GetSelectedRay(out IRay ray)
	    {
		    ray = null;
		    //Instance for over result 
		    IRay result = null;
		    //Getting Reflection black door open
		    Type chartControlType = typeof (ChartControl);
		    //Now we want to get access to  secreat field 
		    FieldInfo fi = chartControlType.GetField("selectedObject", BindingFlags.NonPublic | BindingFlags.Instance);
		    //Now if realy got this one 
		    if (fi != null)
		    {
			    //if we free from null error
			    if (base.ChartControl != null && fi.GetValue(base.ChartControl) != null)
			    {
				    //Getting the instance of the object
				    object clickedObject = fi.GetValue(base.ChartControl);
				    //Checking if ti posible to convert
				    if (clickedObject is IRay)
				    {
					    ray = (IRay) clickedObject;
					    return true;
					    //Converting 
				    }
			    }
		    }
		    return false;
	    }

	    private void ButtonToThetop()
        {
            Control[] tscontrol = ChartControl.Controls.Find("tsrTool", false);

            if (tscontrol.Length > 0)
            {
                MyTsButton = new ToolStripButton();
                MyTsButton.Click += vShowForm_Panel;

                MyTsSeparator = new ToolStripSeparator();

                MyToolStrip = (ToolStrip)tscontrol[0];
                MyToolStrip.Items.Add(MyTsSeparator);
                MyToolStrip.Items.Add(MyTsButton);

                MyTsButton.ForeColor = Color.Black;
				SetCSTButtonText();
            }
        }

        private void vShowForm_Panel(object s, EventArgs e)
        {
	        MainPanel.Visible = !MainPanel.Visible;
	        SetCSTButtonText();
        }

	    private void SetCSTButtonText()
	    {
		    if (MainPanel.Visible)
		    {
			    MyTsButton.Text = "Close CST Control";
			    MyTsButton.BackColor =_disabledColor;
		    }
		    else 
		    {
			    MyTsButton.Text = "Show CST Control";
			    MyTsButton.BackColor = _enabledColor;
		    }
	    }

	    #endregion

	    private void OnLongAndShortClick()
	    {
		    MessageBox.Show("We pressed the Short Or Long");
		    _isCountingRR = true;
		    _isRRAfter = false;
	    }

	    #region Click Events FORM 01

	    private void button_ManualLong_Click(object sender, EventArgs e)
	    {
		    IRay ray;
		    if (!GetSelectedRay(out ray))
		    {
			    MessageBox.Show(_pleaseSelectRay);
			    return;
		    }
			//We crating the continer for the rays
			_currentRayContainer = new RayContainer(MarketPosition.Long,ray,this);
		    _currentRayContainer.Update();
		    OnLongAndShortClick();
		 }

	    private void button_ManualShort_Click(object sender, EventArgs e)
        {
		    IRay ray;
		    if (!GetSelectedRay(out ray))
		    {
			    MessageBox.Show(_pleaseSelectRay);
			    return;
		    }
			//We are creating container for a rays
			_currentRayContainer = new RayContainer(MarketPosition.Short,ray,this);
		    _currentRayContainer.Update();
			OnLongAndShortClick();
        }

        private void button_ClosePosition_Click(object sender, EventArgs e)
        {

        }

        private void button_UpdateQuantity_Click(object sender, EventArgs e)
        {

        }

        private void button_CloseHalfPosition_Click(object sender, EventArgs e)
        {

        }

        private void comboBox_OrderType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button_MakeHorizizontalLine_Click(object sender, EventArgs e)
        {
	        IRay ray;
	        if (GetSelectedRay(out ray))
	        {
				if(ray==null)
					MessageBox.Show("Over ray is null");
		        double averagePrice = (ray.Anchor1Y + ray.Anchor2Y)/2;
		        ChartRay rayToUse = ray as ChartRay;
		        rayToUse.StartY = averagePrice;
		        rayToUse.EndY = averagePrice;
				UpdateForms();
	        }
			else
		        MassageIfLineNotSelected();

        }

	    private void UpdateForms()
	    {
			if(doBigger)
				ChartControl.Size = new Size(ChartControl.Size.Width + 1, ChartControl.Size.Height);
			else
				ChartControl.Size = new Size(ChartControl.Size.Width - 1, ChartControl.Size.Height);
		    doBigger = !doBigger;
		    ChartControl.Update();
	    }

	    private static void MassageIfLineNotSelected()
	    {
		    MessageBox.Show("Please select line First");
	    }

	    private void checkBox_EnableTrailStop_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Enable50Profit_Changed(object sender, EventArgs e)
        {
			//Todo:make logic to to Particl Close 
	        if (checkBox_EnablePartialProfit.Checked)
	        {
		        PartialMsgLabel.Text = "50% TP Enabled";
		        PartialMsgLabel.BackColor = _enabledColor;
	        }
	        else
	        {
		        PartialMsgLabel.Text = "50% TP Disabled";
		        PartialMsgLabel.BackColor = _disabledColor;
            }
        }

        private void checkBox_EnablePartialProfitAlert_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_EnableTrailStopAlert_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown_SwingIndicatorBars_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown_HorizCrossTicks_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown_StopLevelTicks_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button_ManualMoveStop_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown_PipTicksToActivate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_MoveSL_EntryLine_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_MoveSL_SupportLine_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_MoveSL_ResistLine_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_MoveSL_PartialProfitLine_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_EnableBarEntry_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown_BarEntry_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label_SwingIndicatorBars_Click(object sender, EventArgs e)
        {

        }

        private void label_StopLevelTicks_Click(object sender, EventArgs e)
        {

        }

        private void label_HorizCrossTicks_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox_StopToEntry_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
		// LOGICS ON HOW TO USE THE GUI BUTTONS TOGETHER WITH NINJASCRIPT /DW
		//		
		//        private void button_ManualLong_Click(object sender, EventArgs e)
		//        {
		//            StatusLabel.Text = "Long Active";
		//            StatusLabel.BackColor = Color.ForestGreen;
		//        }
		//
		//        private void button_ManualShort_Click(object sender, EventArgs e)
		//        {
		//            StatusLabel.Text = "Short Active";
		//            StatusLabel.BackColor = Color.MediumOrchid;
		//        }
		//
		//        private void button_ClosePosition_Click(object sender, EventArgs e)
		//        {
		//            StatusLabel.Text = "Not Active";
		//            StatusLabel.BackColor = Color.LightCoral;
		//        }
		//
		//        private void button_UpdateQuantity_Click(object sender, EventArgs e)
		//        {
		//            groupBox_Quantity.Text = "Quantity = " + numericUpDown_Quantity.Value.ToString();
		//        }
		//
		//        private void button_CloseHalfPosition_Click(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void comboBox_OrderType_SelectedIndexChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void button_MakeHorizizontalLine_Click(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void checkBox_EnableTrailStop_CheckedChanged(object sender, EventArgs e)
		//        {
		//            if (checkBox_EnableTrailStop.Checked)
		//            {
		//                DynamicTrailingStopMsgLabel.Text = "Active";
		//            }
		//            else
		//            {
		//                DynamicTrailingStopMsgLabel.Text = "In-act.";
		//            }
		//        }
		//
		//        private void Enable50Profit_Changed(object sender, EventArgs e)
		//        {
		//            if (checkBox_EnablePartialProfit.Checked)
		//            {
		//                PartialMsgLabel.Text = "Active";
		//            }
		//            else
		//            {
		//                PartialMsgLabel.Text = "In-act.";
		//            }
		//        }
		//
		//        private void checkBox_EnablePartialProfitAlert_CheckedChanged(object sender, EventArgs e)
		//        {
		//            if (checkBox_EnablePartialProfitAlert.Checked)
		//            {
		//                PartialMsgLabel.Text = "Active";
		//            }
		//            else
		//            {
		//                PartialMsgLabel.Text = "In-act.";
		//            }
		//        }
		//
		//        private void checkBox_EnableTrailStopAlert_CheckedChanged(object sender, EventArgs e)
		//        {
		//            if (checkBox_EnableTrailStopAlert.Checked)
		//            {
		//                DynamicTrailingStopMsgLabel.Text = "Active";
		//            }
		//            else
		//            {
		//                DynamicTrailingStopMsgLabel.Text = "In-act.";
		//            }
		//        }
		//
		//        private void numericUpDown_SwingIndicatorBars_ValueChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void numericUpDown_HorizCrossTicks_ValueChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void numericUpDown_StopLevelTicks_ValueChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void button_ManualMoveStop_Click(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void numericUpDown_PipTicksToActivate_ValueChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void checkBox_MoveSL_EntryLine_CheckedChanged(object sender, EventArgs e)
		//        {
		//            StopToEntyMsgLabel.Text = "In-act.";
		//            if (checkBox_MoveSL_EntryLine.Checked) { StopToEntyMsgLabel.Text = "SL to Entry Line"; }
		//        }
		//
		//        private void checkBox_MoveSL_SupportLine_CheckedChanged(object sender, EventArgs e)
		//        {
		//            StopToEntyMsgLabel.Text = "In-act.";
		//            if (checkBox_MoveSL_SupportLine.Checked) { StopToEntyMsgLabel.Text = "SL to Support Line"; }
		//        }
		//
		//        private void checkBox_MoveSL_ResistLine_CheckedChanged(object sender, EventArgs e)
		//        {
		//            StopToEntyMsgLabel.Text = "In-act.";
		//            if (checkBox_MoveSL_ResistLine.Checked) { StopToEntyMsgLabel.Text = "SL to Resistance Line"; }
		//        }
		//
		//        private void checkBox_MoveSL_PartialProfitLine_CheckedChanged(object sender, EventArgs e)
		//        {
		//            StopToEntyMsgLabel.Text = "In-act.";
		//            if (checkBox_MoveSL_PartialProfitLine.Checked) { StopToEntyMsgLabel.Text = "SL to Partial Profit Line"; }
		//        }
		//
		//        private void checkBox_EnableBarEntry_CheckedChanged(object sender, EventArgs e)
		//        {
		//
		//        }
		//
		//        private void numericUpDown_BarEntry_ValueChanged(object sender, EventArgs e)
		//        {
		//
		//        }

		#endregion

		#region FORM 01 - VS2010 Controls Initialization

		private void VS2010_InitializeComponent_Form()
        {
            this.groupBox_StatusWindow = new System.Windows.Forms.GroupBox();
            this.RRLabel = new System.Windows.Forms.Label();
            this.RR50NameLabel = new System.Windows.Forms.Label();
            this.RRNamelabel = new System.Windows.Forms.Label();
            this.StopToEntyMsgLabel = new System.Windows.Forms.Label();
            this.DynamicTrailingStopMsgLabel = new System.Windows.Forms.Label();
            this.RR50Label = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.PartialMsgLabel = new System.Windows.Forms.Label();
            this.comboBox_OrderType = new System.Windows.Forms.ComboBox();
            this.groupBox_PartialProfit = new System.Windows.Forms.GroupBox();
            this.checkBox_EnablePartialProfitAlert = new System.Windows.Forms.CheckBox();
            this.button_CloseHalfPosition = new System.Windows.Forms.Button();
            this.checkBox_EnablePartialProfit = new System.Windows.Forms.CheckBox();
            this.groupBox_Quantity = new System.Windows.Forms.GroupBox();
            this.button_UpdateQuantity = new System.Windows.Forms.Button();
            this.numericUpDown_Quantity = new System.Windows.Forms.NumericUpDown();
            this.button_ClosePosition = new System.Windows.Forms.Button();
            this.button_ManualShort = new System.Windows.Forms.Button();
            this.button_ManualLong = new System.Windows.Forms.Button();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown_BarEntry = new System.Windows.Forms.NumericUpDown();
            this.checkBox_EnableBarEntry = new System.Windows.Forms.CheckBox();
            this.groupBox_TrailStop = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown_SwingIndicatorBars = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_HorizCrossTicks = new System.Windows.Forms.NumericUpDown();
            this.checkBox_EnableTrailStopAlert = new System.Windows.Forms.CheckBox();
            this.checkBox_EnableTrailStop = new System.Windows.Forms.CheckBox();
            this.numericUpDown_StopLevelTicks = new System.Windows.Forms.NumericUpDown();
            this.groupBox_StopToEntry = new System.Windows.Forms.GroupBox();
            this.checkBox_MoveSL_PartialProfitLine = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_PipTicksToActivate = new System.Windows.Forms.NumericUpDown();
            this.button_ManualMoveStop = new System.Windows.Forms.Button();
            this.checkBox_MoveSL_EntryLine = new System.Windows.Forms.CheckBox();
            this.groupBox_Mode = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_MakeHorizLine = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox_StatusWindow.SuspendLayout();
            this.groupBox_PartialProfit.SuspendLayout();
            this.groupBox_Quantity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Quantity)).BeginInit();
            this.MainPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_BarEntry)).BeginInit();
            this.groupBox_TrailStop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SwingIndicatorBars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_HorizCrossTicks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_StopLevelTicks)).BeginInit();
            this.groupBox_StopToEntry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PipTicksToActivate)).BeginInit();
            this.groupBox_Mode.SuspendLayout();
//            this.SuspendLayout();
            // 
            // groupBox_StatusWindow
            // 
            this.groupBox_StatusWindow.BackColor = System.Drawing.SystemColors.Control;
			//RR
            this.groupBox_StatusWindow.Controls.Add(this.RRLabel);
            this.groupBox_StatusWindow.Controls.Add(this.RR50NameLabel);
            this.groupBox_StatusWindow.Controls.Add(this.RRNamelabel);
	        this.groupBox_StatusWindow.Controls.Add(this.RR50Label);
			//
	        this.groupBox_StatusWindow.Controls.Add(this.StatusLabel);
	        this.groupBox_StatusWindow.Controls.Add(this.PartialMsgLabel);
	        this.groupBox_StatusWindow.Controls.Add(this.StopToEntyMsgLabel);
	        this.groupBox_StatusWindow.Controls.Add(this.DynamicTrailingStopMsgLabel);
	        this.groupBox_StatusWindow.Location = new System.Drawing.Point(6, 3);
            this.groupBox_StatusWindow.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_StatusWindow.Name = "groupBox_StatusWindow";
            this.groupBox_StatusWindow.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_StatusWindow.Size = new System.Drawing.Size(154, 132);
            this.groupBox_StatusWindow.TabIndex = 0;
            this.groupBox_StatusWindow.TabStop = false;
            this.groupBox_StatusWindow.Text = "Status Window";
// 
	        // StopToEntyMsgLabel
	        // 
	        this.StopToEntyMsgLabel.BackColor = _disabledColor;
	        this.StopToEntyMsgLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.StopToEntyMsgLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.StopToEntyMsgLabel.ForeColor = System.Drawing.SystemColors.Window;
	        this.StopToEntyMsgLabel.Location = new System.Drawing.Point(78, 68);
	        this.StopToEntyMsgLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
	        this.StopToEntyMsgLabel.Name = "StopToEntyMsgLabel";
	        this.StopToEntyMsgLabel.Size = new System.Drawing.Size(70, 22);
	        this.StopToEntyMsgLabel.TabIndex = 3;
	        this.StopToEntyMsgLabel.Text = "StE NOT";
	        this.StopToEntyMsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	        // 
	        // DynamicTrailingStopMsgLabel
	        // 
	        this.DynamicTrailingStopMsgLabel.BackColor = _disabledColor ;
	        this.DynamicTrailingStopMsgLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.DynamicTrailingStopMsgLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.DynamicTrailingStopMsgLabel.ForeColor = System.Drawing.SystemColors.Window;
	        this.DynamicTrailingStopMsgLabel.Location = new System.Drawing.Point(6, 68);
	        this.DynamicTrailingStopMsgLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
	        this.DynamicTrailingStopMsgLabel.Name = "DynamicTrailingStopMsgLabel";
	        this.DynamicTrailingStopMsgLabel.Size = new System.Drawing.Size(68, 22);
	        this.DynamicTrailingStopMsgLabel.TabIndex = 3;
	        this.DynamicTrailingStopMsgLabel.Text = "DTS NOT";
	        this.DynamicTrailingStopMsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	        // 
	        // PartialMsgLabel
	        // 
	        this.PartialMsgLabel.BackColor = _disabledColor ;
	        this.PartialMsgLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.PartialMsgLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.PartialMsgLabel.ForeColor = System.Drawing.SystemColors.Window;
	        this.PartialMsgLabel.Location = new System.Drawing.Point(6, 43);
	        this.PartialMsgLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
	        this.PartialMsgLabel.Name = "PartialMsgLabel";
	        this.PartialMsgLabel.Size = new System.Drawing.Size(142, 22);
	        this.PartialMsgLabel.TabIndex = 2;
	        this.PartialMsgLabel.Text = "50% TP Disabled";
	        this.PartialMsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

	        #region RR

	        // 
	        // RR50NameLabel
	        // 
	        this.RR50NameLabel.AutoSize = true;
	        this.RR50NameLabel.Location = new Point(84, 90);
	        this.RR50NameLabel.Margin = new Padding(2, 0, 2, 0);
	        this.RR50NameLabel.Name = "RR50NameLabel";
	        this.RR50NameLabel.Size = new Size(55, 13);
	        this.RR50NameLabel.TabIndex = 2;
	        this.RR50NameLabel.Text = "R:R - 50%";
	        // 
	        // RRNamelabel
	        // 
	        this.RRNamelabel.AutoSize = true;
	        this.RRNamelabel.Location = new Point(7, 90);
	        this.RRNamelabel.Margin = new Padding(2, 0, 2, 0);
	        this.RRNamelabel.Name = "RRNamelabel";
	        this.RRNamelabel.Size = new Size(26, 13);
	        this.RRNamelabel.TabIndex = 4;
	        this.RRNamelabel.Text = "R:R";
	        // 
	        // RRLabel
	        // 
	        this.RRLabel.BackColor = Color.White;
	        this.RRLabel.BorderStyle = BorderStyle.FixedSingle;
	        this.RRLabel.Location = new Point(10, 106);
	        this.RRLabel.Margin = new Padding(2, 0, 2, 0);
	        this.RRLabel.Name = "RRLabel";
	        this.RRLabel.Size = new Size(34, 22);
	        this.RRLabel.TabIndex = 5;
	        this.RRLabel.Text = "3.46";
	        this.RRLabel.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // RR50Label
	        // 
	        this.RR50Label.BackColor = Color.White;
	        this.RR50Label.BorderStyle = BorderStyle.FixedSingle;
	        this.RR50Label.Location = new Point(87, 106);
	        this.RR50Label.Margin = new Padding(2, 0, 2, 0);
	        this.RR50Label.Name = "RR50Label";
	        this.RR50Label.Size = new Size(34, 22);
	        this.RR50Label.TabIndex = 3;
	        this.RR50Label.Text = "3.46";
	        this.RR50Label.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // StatusLabel
	        // 
	        this.StatusLabel.BackColor = _disabledColor ;
	        this.StatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.StatusLabel.ForeColor = System.Drawing.Color.White;
	        this.StatusLabel.Location = new System.Drawing.Point(6, 18);
	        this.StatusLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
	        this.StatusLabel.Name = "StatusLabel";
	        this.StatusLabel.Size = new System.Drawing.Size(142, 22);
	        this.StatusLabel.TabIndex = 0;
	        this.StatusLabel.Text = "Not Active";
	        this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

	        #endregion

	        // 
	        // comboBox_OrderType
	        // 
	        this.comboBox_OrderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
	        this.comboBox_OrderType.FormattingEnabled = true;
	        this.comboBox_OrderType.Items.AddRange(new object[] {
            "LIMIT",
            "MARKET"});
	        comboBox_OrderType.SelectedIndex=0;
	        this.comboBox_OrderType.Location = new System.Drawing.Point(80, 17);
	        this.comboBox_OrderType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_OrderType.Name = "comboBox_OrderType";
            this.comboBox_OrderType.Size = new System.Drawing.Size(66, 21);
            this.comboBox_OrderType.TabIndex = 4;
            this.comboBox_OrderType.SelectedIndexChanged += new System.EventHandler(this.comboBox_OrderType_SelectedIndexChanged);
            // 
            // groupBox_PartialProfit
            // 
            this.groupBox_PartialProfit.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox_PartialProfit.Controls.Add(this.checkBox_EnablePartialProfitAlert);
            this.groupBox_PartialProfit.Controls.Add(this.button_CloseHalfPosition);
            this.groupBox_PartialProfit.Controls.Add(this.checkBox_EnablePartialProfit);
            this.groupBox_PartialProfit.Location = new System.Drawing.Point(6, 374);
            this.groupBox_PartialProfit.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_PartialProfit.Name = "groupBox_PartialProfit";
            this.groupBox_PartialProfit.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_PartialProfit.Size = new System.Drawing.Size(154, 89);
            this.groupBox_PartialProfit.TabIndex = 5;
            this.groupBox_PartialProfit.TabStop = false;
            this.groupBox_PartialProfit.Text = "50 % Partial Take Profit";
            // 
            // checkBox_EnablePartialProfitAlert
            // 
            this.checkBox_EnablePartialProfitAlert.AutoSize = true;
            this.checkBox_EnablePartialProfitAlert.Location = new System.Drawing.Point(8, 68);
            this.checkBox_EnablePartialProfitAlert.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_EnablePartialProfitAlert.Name = "checkBox_EnablePartialProfitAlert";
            this.checkBox_EnablePartialProfitAlert.Size = new System.Drawing.Size(111, 17);
            this.checkBox_EnablePartialProfitAlert.TabIndex = 3;
            this.checkBox_EnablePartialProfitAlert.Text = "Enable Email Alert";
            this.checkBox_EnablePartialProfitAlert.UseVisualStyleBackColor = true;
            this.checkBox_EnablePartialProfitAlert.CheckedChanged += new System.EventHandler(this.checkBox_EnablePartialProfitAlert_CheckedChanged);
            // 
            // button_CloseHalfPosition
            // 
            this.button_CloseHalfPosition.BackColor = _disabledColor ;
            this.button_CloseHalfPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_CloseHalfPosition.ForeColor = System.Drawing.Color.White;
            this.button_CloseHalfPosition.Location = new System.Drawing.Point(6, 19);
            this.button_CloseHalfPosition.Margin = new System.Windows.Forms.Padding(2);
            this.button_CloseHalfPosition.Name = "button_CloseHalfPosition";
            this.button_CloseHalfPosition.Size = new System.Drawing.Size(144, 27);
            this.button_CloseHalfPosition.TabIndex = 2;
            this.button_CloseHalfPosition.Text = "MANUAL CLOSE 50%";
            this.button_CloseHalfPosition.UseVisualStyleBackColor = false;
            this.button_CloseHalfPosition.Click += new System.EventHandler(this.button_CloseHalfPosition_Click);
            // 
            // checkBox_EnablePartialProfit
            // 
            this.checkBox_EnablePartialProfit.AutoSize = true;
            this.checkBox_EnablePartialProfit.Location = new System.Drawing.Point(8, 50);
            this.checkBox_EnablePartialProfit.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_EnablePartialProfit.Name = "checkBox_EnablePartialProfit";
            this.checkBox_EnablePartialProfit.Size = new System.Drawing.Size(59, 17);
            this.checkBox_EnablePartialProfit.TabIndex = 0;
            this.checkBox_EnablePartialProfit.Text = "Enable";
            this.checkBox_EnablePartialProfit.UseVisualStyleBackColor = true;
            this.checkBox_EnablePartialProfit.CheckedChanged += new System.EventHandler(this.Enable50Profit_Changed);
            // 
            // groupBox_Quantity
            // 
            this.groupBox_Quantity.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox_Quantity.Controls.Add(this.button_UpdateQuantity);
            this.groupBox_Quantity.Controls.Add(this.numericUpDown_Quantity);
            this.groupBox_Quantity.Location = new System.Drawing.Point(6, 137);
            this.groupBox_Quantity.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_Quantity.Name = "groupBox_Quantity";
            this.groupBox_Quantity.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_Quantity.Size = new System.Drawing.Size(154, 41);
            this.groupBox_Quantity.TabIndex = 4;
            this.groupBox_Quantity.TabStop = false;
            this.groupBox_Quantity.Text = "Quantity";
            // 
            // button_UpdateQuantity
            // 
            this.button_UpdateQuantity.BackColor = System.Drawing.Color.SkyBlue;
            this.button_UpdateQuantity.Location = new System.Drawing.Point(8, 15);
            this.button_UpdateQuantity.Margin = new System.Windows.Forms.Padding(2);
            this.button_UpdateQuantity.Name = "button_UpdateQuantity";
            this.button_UpdateQuantity.Size = new System.Drawing.Size(59, 22);
            this.button_UpdateQuantity.TabIndex = 1;
            this.button_UpdateQuantity.Text = "Update";
            this.button_UpdateQuantity.UseVisualStyleBackColor = false;
            this.button_UpdateQuantity.Click += new System.EventHandler(this.button_UpdateQuantity_Click);
            // 
            // numericUpDown_Quantity
            // 
            this.numericUpDown_Quantity.Location = new System.Drawing.Point(71, 16);
            this.numericUpDown_Quantity.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_Quantity.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDown_Quantity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Quantity.Name = "numericUpDown_Quantity";
            this.numericUpDown_Quantity.Size = new System.Drawing.Size(79, 20);
            this.numericUpDown_Quantity.TabIndex = 0;
            this.numericUpDown_Quantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_Quantity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // button_ClosePosition
            // 
            this.button_ClosePosition.BackColor = _disabledColor ;
            this.button_ClosePosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ClosePosition.ForeColor = System.Drawing.Color.White;
            this.button_ClosePosition.Location = new System.Drawing.Point(4, 154);
            this.button_ClosePosition.Margin = new System.Windows.Forms.Padding(2);
            this.button_ClosePosition.Name = "button_ClosePosition";
            this.button_ClosePosition.Size = new System.Drawing.Size(144, 27);
            this.button_ClosePosition.TabIndex = 3;
            this.button_ClosePosition.Text = "MANUAL CLOSE 100%";
            this.button_ClosePosition.UseVisualStyleBackColor = false;
            this.button_ClosePosition.Click += new System.EventHandler(this.button_ClosePosition_Click);
            // 
            // button_ManualShort
            // 
            this.button_ManualShort.BackColor = System.Drawing.Color.MediumOrchid;
            this.button_ManualShort.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ManualShort.ForeColor = System.Drawing.Color.White;
            this.button_ManualShort.Location = new System.Drawing.Point(79, 41);
            this.button_ManualShort.Margin = new System.Windows.Forms.Padding(2);
            this.button_ManualShort.Name = "button_ManualShort";
            this.button_ManualShort.Size = new System.Drawing.Size(69, 27);
            this.button_ManualShort.TabIndex = 2;
            this.button_ManualShort.Text = "SHORT";
            this.button_ManualShort.UseVisualStyleBackColor = false;
            this.button_ManualShort.Click += new System.EventHandler(this.button_ManualShort_Click);
            // 
            // button_ManualLong
            // 
	        this.button_ManualLong.BackColor = _disabledColor;
            this.button_ManualLong.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ManualLong.ForeColor = System.Drawing.Color.White;
            this.button_ManualLong.Location = new System.Drawing.Point(4, 41);
            this.button_ManualLong.Margin = new System.Windows.Forms.Padding(2);
            this.button_ManualLong.Name = "button_ManualLong";
            this.button_ManualLong.Size = new System.Drawing.Size(70, 27);
            this.button_ManualLong.TabIndex = 1;
            this.button_ManualLong.Text = "LONG";
            this.button_ManualLong.UseVisualStyleBackColor = false;
            this.button_ManualLong.Click += new System.EventHandler(this.button_ManualLong_Click);
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.SystemColors.Control;
            this.MainPanel.Controls.Add(this.groupBox1);
            this.MainPanel.Controls.Add(this.groupBox_StopToEntry);
            this.MainPanel.Controls.Add(this.groupBox_TrailStop);
            this.MainPanel.Controls.Add(this.groupBox_StatusWindow);
            this.MainPanel.Controls.Add(this.groupBox_PartialProfit);
            this.MainPanel.Controls.Add(this.groupBox_Quantity);
            this.MainPanel.Controls.Add(this.groupBox_Mode);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.MainPanel.Location = new System.Drawing.Point(931, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(2);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(165, 831);
            this.MainPanel.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numericUpDown_BarEntry);
            this.groupBox1.Controls.Add(this.checkBox_EnableBarEntry);
            this.groupBox1.Location = new System.Drawing.Point(6, 704);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(154, 47);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bar Entry";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(84, 21);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Bars";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDown_BarEntry
            // 
            this.numericUpDown_BarEntry.Location = new System.Drawing.Point(116, 18);
            this.numericUpDown_BarEntry.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_BarEntry.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDown_BarEntry.Name = "numericUpDown_BarEntry";
            this.numericUpDown_BarEntry.Size = new System.Drawing.Size(34, 20);
            this.numericUpDown_BarEntry.TabIndex = 12;
            this.numericUpDown_BarEntry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_BarEntry.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_BarEntry.ValueChanged += new System.EventHandler(this.numericUpDown_BarEntry_ValueChanged);
            // 
            // checkBox_EnableBarEntry
            // 
            this.checkBox_EnableBarEntry.AutoSize = true;
            this.checkBox_EnableBarEntry.Location = new System.Drawing.Point(10, 20);
            this.checkBox_EnableBarEntry.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_EnableBarEntry.Name = "checkBox_EnableBarEntry";
            this.checkBox_EnableBarEntry.Size = new System.Drawing.Size(59, 17);
            this.checkBox_EnableBarEntry.TabIndex = 5;
            this.checkBox_EnableBarEntry.Text = "Enable";
            this.checkBox_EnableBarEntry.UseVisualStyleBackColor = true;
            this.checkBox_EnableBarEntry.CheckedChanged += new System.EventHandler(this.checkBox_EnableBarEntry_CheckedChanged);
            // 
            // groupBox_TrailStop
            // 
            this.groupBox_TrailStop.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox_TrailStop.Controls.Add(this.label9);
            this.groupBox_TrailStop.Controls.Add(this.label8);
            this.groupBox_TrailStop.Controls.Add(this.label7);
            this.groupBox_TrailStop.Controls.Add(this.numericUpDown_SwingIndicatorBars);
            this.groupBox_TrailStop.Controls.Add(this.numericUpDown_HorizCrossTicks);
            this.groupBox_TrailStop.Controls.Add(this.checkBox_EnableTrailStopAlert);
            this.groupBox_TrailStop.Controls.Add(this.checkBox_EnableTrailStop);
            this.groupBox_TrailStop.Controls.Add(this.numericUpDown_StopLevelTicks);
            this.groupBox_TrailStop.Location = new System.Drawing.Point(6, 579);
            this.groupBox_TrailStop.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_TrailStop.Name = "groupBox_TrailStop";
            this.groupBox_TrailStop.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_TrailStop.Size = new System.Drawing.Size(154, 123);
            this.groupBox_TrailStop.TabIndex = 7;
            this.groupBox_TrailStop.TabStop = false;
            this.groupBox_TrailStop.Text = "Dynamic Trail Stop";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(5, 102);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(109, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Horizontal [pips/ticks]";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(5, 81);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Stop Level [pips/ticks]";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(5, 59);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(109, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Swing Indicator [bars]";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // numericUpDown_SwingIndicatorBars
            // 
            this.numericUpDown_SwingIndicatorBars.Location = new System.Drawing.Point(118, 57);
            this.numericUpDown_SwingIndicatorBars.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_SwingIndicatorBars.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDown_SwingIndicatorBars.Name = "numericUpDown_SwingIndicatorBars";
            this.numericUpDown_SwingIndicatorBars.Size = new System.Drawing.Size(34, 20);
            this.numericUpDown_SwingIndicatorBars.TabIndex = 11;
            this.numericUpDown_SwingIndicatorBars.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_SwingIndicatorBars.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown_SwingIndicatorBars.ValueChanged += new System.EventHandler(this.numericUpDown_SwingIndicatorBars_ValueChanged);
            // 
            // numericUpDown_HorizCrossTicks
            // 
            this.numericUpDown_HorizCrossTicks.Location = new System.Drawing.Point(118, 78);
            this.numericUpDown_HorizCrossTicks.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_HorizCrossTicks.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDown_HorizCrossTicks.Name = "numericUpDown_HorizCrossTicks";
            this.numericUpDown_HorizCrossTicks.Size = new System.Drawing.Size(34, 20);
            this.numericUpDown_HorizCrossTicks.TabIndex = 12;
            this.numericUpDown_HorizCrossTicks.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_HorizCrossTicks.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown_HorizCrossTicks.ValueChanged += new System.EventHandler(this.numericUpDown_HorizCrossTicks_ValueChanged);
            // 
            // checkBox_EnableTrailStopAlert
            // 
            this.checkBox_EnableTrailStopAlert.AutoSize = true;
            this.checkBox_EnableTrailStopAlert.Location = new System.Drawing.Point(9, 35);
            this.checkBox_EnableTrailStopAlert.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_EnableTrailStopAlert.Name = "checkBox_EnableTrailStopAlert";
            this.checkBox_EnableTrailStopAlert.Size = new System.Drawing.Size(111, 17);
            this.checkBox_EnableTrailStopAlert.TabIndex = 5;
            this.checkBox_EnableTrailStopAlert.Text = "Enable Email Alert";
            this.checkBox_EnableTrailStopAlert.UseVisualStyleBackColor = true;
            this.checkBox_EnableTrailStopAlert.CheckedChanged += new System.EventHandler(this.checkBox_EnableTrailStopAlert_CheckedChanged);
            // 
            // checkBox_EnableTrailStop
            // 
            this.checkBox_EnableTrailStop.AutoSize = true;
            this.checkBox_EnableTrailStop.Location = new System.Drawing.Point(9, 18);
            this.checkBox_EnableTrailStop.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_EnableTrailStop.Name = "checkBox_EnableTrailStop";
            this.checkBox_EnableTrailStop.Size = new System.Drawing.Size(59, 17);
            this.checkBox_EnableTrailStop.TabIndex = 4;
            this.checkBox_EnableTrailStop.Text = "Enable";
            this.checkBox_EnableTrailStop.UseVisualStyleBackColor = true;
            this.checkBox_EnableTrailStop.CheckedChanged += new System.EventHandler(this.checkBox_EnableTrailStop_CheckedChanged);
            // 
            // numericUpDown_StopLevelTicks
            // 
            this.numericUpDown_StopLevelTicks.Location = new System.Drawing.Point(118, 99);
            this.numericUpDown_StopLevelTicks.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_StopLevelTicks.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDown_StopLevelTicks.Name = "numericUpDown_StopLevelTicks";
            this.numericUpDown_StopLevelTicks.Size = new System.Drawing.Size(34, 20);
            this.numericUpDown_StopLevelTicks.TabIndex = 13;
            this.numericUpDown_StopLevelTicks.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_StopLevelTicks.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDown_StopLevelTicks.ValueChanged += new System.EventHandler(this.numericUpDown_StopLevelTicks_ValueChanged);
            // 
            // groupBox_StopToEntry
            // 
            this.groupBox_StopToEntry.Controls.Add(this.checkBox_MoveSL_PartialProfitLine);
            this.groupBox_StopToEntry.Controls.Add(this.label3);
            this.groupBox_StopToEntry.Controls.Add(this.numericUpDown_PipTicksToActivate);
            this.groupBox_StopToEntry.Controls.Add(this.button_ManualMoveStop);
            this.groupBox_StopToEntry.Controls.Add(this.checkBox_MoveSL_EntryLine);
            this.groupBox_StopToEntry.Location = new System.Drawing.Point(6, 466);
            this.groupBox_StopToEntry.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_StopToEntry.Name = "groupBox_StopToEntry";
            this.groupBox_StopToEntry.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_StopToEntry.Size = new System.Drawing.Size(154, 109);
            this.groupBox_StopToEntry.TabIndex = 2;
            this.groupBox_StopToEntry.TabStop = false;
            this.groupBox_StopToEntry.Text = "Stop to Entry/Partial Profit";
            this.groupBox_StopToEntry.Enter += new System.EventHandler(this.groupBox_StopToEntry_Enter);
            // 
            // checkBox_MoveSL_PartialProfitLine
            // 
            this.checkBox_MoveSL_PartialProfitLine.AutoSize = true;
            this.checkBox_MoveSL_PartialProfitLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_MoveSL_PartialProfitLine.Location = new System.Drawing.Point(8, 89);
            this.checkBox_MoveSL_PartialProfitLine.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_MoveSL_PartialProfitLine.Name = "checkBox_MoveSL_PartialProfitLine";
            this.checkBox_MoveSL_PartialProfitLine.Size = new System.Drawing.Size(149, 17);
            this.checkBox_MoveSL_PartialProfitLine.TabIndex = 6;
            this.checkBox_MoveSL_PartialProfitLine.Text = "Move Stop to Partial Profit";
            this.checkBox_MoveSL_PartialProfitLine.UseVisualStyleBackColor = true;
            this.checkBox_MoveSL_PartialProfitLine.CheckedChanged += new System.EventHandler(this.checkBox_MoveSL_PartialProfitLine_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(5, 54);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Pips/Ticks to activate";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDown_PipTicksToActivate
            // 
            this.numericUpDown_PipTicksToActivate.Location = new System.Drawing.Point(116, 51);
            this.numericUpDown_PipTicksToActivate.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_PipTicksToActivate.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDown_PipTicksToActivate.Name = "numericUpDown_PipTicksToActivate";
            this.numericUpDown_PipTicksToActivate.Size = new System.Drawing.Size(34, 20);
            this.numericUpDown_PipTicksToActivate.TabIndex = 12;
            this.numericUpDown_PipTicksToActivate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_PipTicksToActivate.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_PipTicksToActivate.ValueChanged += new System.EventHandler(this.numericUpDown_PipTicksToActivate_ValueChanged);
            // 
            // button_ManualMoveStop
            // 
            this.button_ManualMoveStop.BackColor = _disabledColor ;
            this.button_ManualMoveStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ManualMoveStop.ForeColor = System.Drawing.Color.White;
            this.button_ManualMoveStop.Location = new System.Drawing.Point(5, 19);
            this.button_ManualMoveStop.Margin = new System.Windows.Forms.Padding(2);
            this.button_ManualMoveStop.Name = "button_ManualMoveStop";
            this.button_ManualMoveStop.Size = new System.Drawing.Size(145, 27);
            this.button_ManualMoveStop.TabIndex = 3;
            this.button_ManualMoveStop.Text = "MANUAL MOVE STOP";
            this.button_ManualMoveStop.UseVisualStyleBackColor = false;
            this.button_ManualMoveStop.Click += new System.EventHandler(this.button_ManualMoveStop_Click);
            // 
            // checkBox_MoveSL_EntryLine
            // 
            this.checkBox_MoveSL_EntryLine.AutoSize = true;
            this.checkBox_MoveSL_EntryLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_MoveSL_EntryLine.Location = new System.Drawing.Point(8, 73);
            this.checkBox_MoveSL_EntryLine.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_MoveSL_EntryLine.Name = "checkBox_MoveSL_EntryLine";
            this.checkBox_MoveSL_EntryLine.Size = new System.Drawing.Size(140, 17);
            this.checkBox_MoveSL_EntryLine.TabIndex = 3;
            this.checkBox_MoveSL_EntryLine.Text = "Move Stop to Entry Line";
            this.checkBox_MoveSL_EntryLine.UseVisualStyleBackColor = true;
            this.checkBox_MoveSL_EntryLine.CheckedChanged += new System.EventHandler(this.checkBox_MoveSL_EntryLine_CheckedChanged);
            // 
            // groupBox_Mode
            // 
            this.groupBox_Mode.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox_Mode.Controls.Add(this.button2);
            this.groupBox_Mode.Controls.Add(this.button1);
            this.groupBox_Mode.Controls.Add(this.button_MakeHorizLine);
            this.groupBox_Mode.Controls.Add(this.label1);
            this.groupBox_Mode.Controls.Add(this.comboBox_OrderType);
            this.groupBox_Mode.Controls.Add(this.button_ManualLong);
            this.groupBox_Mode.Controls.Add(this.button_ManualShort);
            this.groupBox_Mode.Controls.Add(this.button_ClosePosition);
            this.groupBox_Mode.Location = new System.Drawing.Point(6, 181);
            this.groupBox_Mode.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_Mode.Name = "groupBox_Mode";
            this.groupBox_Mode.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_Mode.Size = new System.Drawing.Size(154, 189);
            this.groupBox_Mode.TabIndex = 6;
            this.groupBox_Mode.TabStop = false;
            this.groupBox_Mode.Text = "Mode";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Order Type:";
            // 
            // button_MakeHorizLine
            // 
            this.button_MakeHorizLine.BackColor = System.Drawing.Color.SkyBlue;
            this.button_MakeHorizLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_MakeHorizLine.Location = new System.Drawing.Point(4, 126);
            this.button_MakeHorizLine.Margin = new System.Windows.Forms.Padding(2);
            this.button_MakeHorizLine.Name = "button_MakeHorizLine";
            this.button_MakeHorizLine.Size = new System.Drawing.Size(144, 26);
            this.button_MakeHorizLine.TabIndex = 4;
            this.button_MakeHorizLine.Text = "MAKE HORIZONTAL";
            this.button_MakeHorizLine.UseVisualStyleBackColor = false;
            this.button_MakeHorizLine.Click += new System.EventHandler(this.button_MakeHorizizontalLine_Click);
            // 
            // button1
            // 
            this.button1.BackColor = _disabledColor ;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(4, 97);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 27);
            this.button1.TabIndex = 8;
            this.button1.Text = "CLEAR SELECTION";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DarkBlue;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.SystemColors.Window;
            this.button2.Location = new System.Drawing.Point(4, 69);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(144, 26);
            this.button2.TabIndex = 9;
            this.button2.Text = "ACTIVATE";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(1096, 831);
//            this.Controls.Add(this.MainPanel);
//            this.Margin = new System.Windows.Forms.Padding(2);
//            this.Name = "Form1";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "the ";
//            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox_StatusWindow.ResumeLayout(false);
            this.groupBox_StatusWindow.PerformLayout();
            this.groupBox_PartialProfit.ResumeLayout(false);
            this.groupBox_PartialProfit.PerformLayout();
            this.groupBox_Quantity.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Quantity)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_BarEntry)).EndInit();
            this.groupBox_TrailStop.ResumeLayout(false);
            this.groupBox_TrailStop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SwingIndicatorBars)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_HorizCrossTicks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_StopLevelTicks)).EndInit();
            this.groupBox_StopToEntry.ResumeLayout(false);
            this.groupBox_StopToEntry.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PipTicksToActivate)).EndInit();
            this.groupBox_Mode.ResumeLayout(false);
            this.groupBox_Mode.PerformLayout();
//            this.ResumeLayout(false);

            this.ChartControl.Controls.Add(this.MainPanel);
        }

        #endregion

        #region Properties
        #endregion
    }

	internal class RayContainer
	{
		public MarketPosition PositionType;
		public static int TicksTarget  =15;

		private IRay _originRay;
		private IRay _entryRay;
		private IRay _stopRay;
		private IRay _profitTarteRay;
		private static Color EnterColor = Color.DarkRed;
		private static Color TPColor = Color.Lime;
		private static Color StopColor = Color.Red ;

		public RayContainer(MarketPosition marketPosition, IRay ray, Strategy strategy)
		{
			double distance = TicksTarget * strategy.TickSize;
			if (marketPosition == MarketPosition.Long)
				distance *= -1;

			_originRay = ray;

			_entryRay = strategy.DrawRay("Enter", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance*.3, ray.Anchor2BarsAgo, ray.Anchor2Y - distance*.3,
				EnterColor, DashStyle.Solid, 2);

			_stopRay = strategy.DrawRay("Stop", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y + distance, ray.Anchor2BarsAgo, ray.Anchor2Y + distance,
				StopColor, DashStyle.Dash, 2);

			_profitTarteRay = strategy.DrawRay("TakeProfit", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance, ray.Anchor2BarsAgo, ray.Anchor2Y - distance,
				TPColor , DashStyle.Dash, 2);


		}

		public void Update()
		{
			//Todo:Create here functionality what happing when we change thprice 

			//Drawing dots and counting the positions 

			//Drawing the text what is same to those dots

		}

		public void SetOrder(bool isFlat)
		{
			if (isFlat)
			{
				//We are only opening the order for Enter to the market
			}
			{
				//We making stop loss
				//And Take profit Order
			}
			
		}

		public void Clear()
		{
				
		}

	}

	internal sealed class RayUtilits
	{
		public static double GetPriceForBar(IRay ray)
		{
			return 0;
		} 
	}
}

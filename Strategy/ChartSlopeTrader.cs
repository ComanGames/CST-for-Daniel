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
        private Button _buttonClearSelection;
        private Button _buttonActivate;

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
	    private bool _isActive;
	    private bool _isCountingRR;
	    private bool _isRRAfter;
	    private string _pleaseSelectRay = "Please Select Ray";
	    private RayContainer _currentRayContainer;
	    private IOrder currentOrder;
	    private bool doBigger;

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
	        ChartControl.ChartPanel.MouseMove += MouseMoveAction;
	        // Add Toolbar Button
	        ButtonToThetop();
        }

	    private void MouseMoveAction(object sender, MouseEventArgs e)
	    {
		    UpdateGraphics();
	    }

	    protected override void OnTermination()
        {
            if (MainPanel != null)
            {
                // Remove and Dispose
                ChartControl.Controls.Remove(MainPanel);
                MainPanel.Dispose();
                MainPanel = null;
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
	        UpdateGraphics();
	        if (_isActive && _currentRayContainer != null)
		        UpdateOrders();
        }

	    private void UpdateOrders()
	    {
		    if (Position.MarketPosition == MarketPosition.Flat)
		    {
			    if (_currentRayContainer.PositionType == MarketPosition.Short)
				  currentOrder=EnterShortStop((int)numericUpDown_Quantity.Value, RayContainer.RayPrice(_currentRayContainer._entryRay));
			    else
				  currentOrder=EnterLongStop((int)numericUpDown_Quantity.Value, RayContainer.RayPrice(_currentRayContainer._entryRay));
		    }
		    else
		    {
			   SetProfitTarget(CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer._profitTargetRay));
			   SetStopLoss(CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer._stopRay));
		    }
	    }

	    private void UpdateGraphics()
	    {
		    if (_currentRayContainer != null)
		    {
				UpdateRR();
				_currentRayContainer.Update();
		    }
			
	    }

	    private void UpdateRR()
	    {
			//Todo: write those RR functionlity

		    if (_currentRayContainer!=null)
		    {
			    double Risk = 0;
			    double Reward = 1;
			    if (MarketPosition.Flat==Position.MarketPosition)
			    {
				    Risk =
					    Math.Abs(RayContainer.RayPrice(_currentRayContainer._entryRay) -
					             RayContainer.RayPrice(_currentRayContainer._stopRay));
				    Reward = 
					    Math.Abs(RayContainer.RayPrice(_currentRayContainer._profitTargetRay) -
					             RayContainer.RayPrice(_currentRayContainer._entryRay));

			    }
			    else
			    {
				    //After we are in a LONG or SHORT position, looking for a profit:
				    //Formula(after):
				    Risk =
					    Math.Abs(Close[0]-
						 RayContainer.RayPrice(_currentRayContainer._stopRay));

					Reward=
					    Math.Abs(Close[0]-
						 RayContainer.RayPrice(_currentRayContainer._profitTargetRay));
			    }

			    if (Reward != 0)
				    RRLabel.Text = Math.Round((Risk/Reward),3).ToString();
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
			    if (ChartControl != null && fi.GetValue(ChartControl) != null)
			    {
				    //Getting the instance of the object
				    object clickedObject = fi.GetValue(ChartControl);
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


	    #region Click Events FORM 01

	    private void button_ManualLong_Click(object sender, EventArgs e)
	    {
		    IRay ray;
		    if (!GetSelectedRay(out ray))
		    {
			    MessageBox.Show(_pleaseSelectRay);
			    return;
		    }
		    else
		    {
		    }
		    if (_isActive)
		    {
			    MessageBox.Show("We already active we can change position while we Active");
				return;
		    }
			//We crating the continer for the rays
			_currentRayContainer = new RayContainer(MarketPosition.Long,ray,this);
		    _currentRayContainer.Update();
			UpdateForms();
		 }

	    private void button_ManualShort_Click(object sender, EventArgs e)
        {
		    IRay ray;
		    if (!GetSelectedRay(out ray))
		    {
			    MessageBox.Show(_pleaseSelectRay);
			    return;
		    }
		    if (_isActive)
		    {
			    MessageBox.Show("We already active we can change position while we Active");
				return;
		    }
			//We are creating container for a rays
		    _currentRayContainer = new RayContainer(MarketPosition.Short, ray, this);
		    _currentRayContainer.Update();
			UpdateForms();

        }

        private void button_MakeHorizizontalLine_Click(object sender, EventArgs e)
        {
	        IRay ray;
	        if (GetSelectedRay(out ray))
	        {
		        double averagePrice = RayContainer.RayPrice(ray);
		        ChartRay rayToUse = ray as ChartRay;
		        rayToUse.StartY = averagePrice;
		        rayToUse.EndY = averagePrice;
				if(_currentRayContainer!=null)
					_currentRayContainer.Update();
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

	    private void _buttonActivateClick(object sender, EventArgs e)
	    {
		    if (_currentRayContainer == null)
		    {
			    MessageBox.Show("You don't have lines to trade with");
			    return;
		    }
		    if (!_isActive)
		    {
			    _isActive = true;
			    StatusLabel.BackColor = _enabledColor;
			    if (_currentRayContainer.PositionType == MarketPosition.Long)
			    {
				    StatusLabel.Text = "Active: Long Position";
			    }
			    else
			    {
				    StatusLabel.Text = "Active: Short Position";
			    }
		    }
		    else
		    {
			    MessageBox.Show("You already active");
		    }
	    }

	    private void _buttonClearSelection_Click(object sender, EventArgs e)
		{
		    if (MarketPosition.Flat != Position.MarketPosition)
		    {
			    MessageBox.Show("You are trading close or diactivate to clear");
				return;
		    }

			if (_currentRayContainer != null)
			{
				_currentRayContainer.Clear();
				_currentRayContainer = null;
				if (_isActive)
					DeActivate();
				UpdateForms();
			}
		}

	    private void DeActivate()
	    {
		    StatusLabel.Text = "Not Active";
		    StatusLabel.BackColor = _disabledColor;
		    _isActive = false;
	    }

	    #endregion

		#region FORM 01 - VS2010 Controls Initialization

		private void VS2010_InitializeComponent_Form()
        {
			#region Creating FormVariables 

			groupBox_StatusWindow = new GroupBox();
			RRLabel = new Label();
			RR50NameLabel = new Label();
			RRNamelabel = new Label();
			StopToEntyMsgLabel = new Label();
			DynamicTrailingStopMsgLabel = new Label();
			RR50Label = new Label();
			StatusLabel = new Label();
			PartialMsgLabel = new Label();
			comboBox_OrderType = new ComboBox();
			groupBox_PartialProfit = new GroupBox();
			checkBox_EnablePartialProfitAlert = new CheckBox();
			button_CloseHalfPosition = new Button();
			checkBox_EnablePartialProfit = new CheckBox();
			groupBox_Quantity = new GroupBox();
			button_UpdateQuantity = new Button();
			numericUpDown_Quantity = new NumericUpDown();
			button_ClosePosition = new Button();
			button_ManualShort = new Button();
			button_ManualLong = new Button();
			MainPanel = new Panel();
			groupBox1 = new GroupBox();
			label4 = new Label();
			numericUpDown_BarEntry = new NumericUpDown();
			checkBox_EnableBarEntry = new CheckBox();
			groupBox_TrailStop = new GroupBox();
			label9 = new Label();
			label8 = new Label();
			label7 = new Label();
			numericUpDown_SwingIndicatorBars = new NumericUpDown();
			numericUpDown_HorizCrossTicks = new NumericUpDown();
			checkBox_EnableTrailStopAlert = new CheckBox();
			checkBox_EnableTrailStop = new CheckBox();
			numericUpDown_StopLevelTicks = new NumericUpDown();
			groupBox_StopToEntry = new GroupBox();
			checkBox_MoveSL_PartialProfitLine = new CheckBox();
			label3 = new Label();
			numericUpDown_PipTicksToActivate = new NumericUpDown();
			button_ManualMoveStop = new Button();
			checkBox_MoveSL_EntryLine = new CheckBox();
			groupBox_Mode = new GroupBox();
			label1 = new Label();
			button_MakeHorizLine = new Button();
			_buttonClearSelection = new Button();
			_buttonActivate = new Button();
			groupBox_StatusWindow.SuspendLayout();
			groupBox_PartialProfit.SuspendLayout();
			groupBox_Quantity.SuspendLayout();
			((ISupportInitialize) (numericUpDown_Quantity)).BeginInit();
			MainPanel.SuspendLayout();
			groupBox1.SuspendLayout();
			((ISupportInitialize) (numericUpDown_BarEntry)).BeginInit();
			groupBox_TrailStop.SuspendLayout();
			((ISupportInitialize) (numericUpDown_SwingIndicatorBars)).BeginInit();
			((ISupportInitialize) (numericUpDown_HorizCrossTicks)).BeginInit();
			((ISupportInitialize) (numericUpDown_StopLevelTicks)).BeginInit();
			groupBox_StopToEntry.SuspendLayout();
			((ISupportInitialize) (numericUpDown_PipTicksToActivate)).BeginInit();
			groupBox_Mode.SuspendLayout();

			#endregion

			#region Buttons

			// 
			// _buttonClearSelection
			// 
			_buttonClearSelection.BackColor = _disabledColor;
			_buttonClearSelection.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte) (0)));
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
			_buttonActivate.BackColor = Color.DarkBlue;
			_buttonActivate.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte) (0)));
			_buttonActivate.ForeColor = SystemColors.Window;
			_buttonActivate.Location = new Point(4, 69);
			_buttonActivate.Margin = new Padding(2);
			_buttonActivate.Name = "_buttonActivate";
			_buttonActivate.Size = new Size(144, 26);
			_buttonActivate.TabIndex = 9;
			_buttonActivate.Text = "ACTIVATE";
			_buttonActivate.UseVisualStyleBackColor = false;
			_buttonActivate.Click += _buttonActivateClick;
			#endregion
            // 
            // groupBox_StatusWindow
            // 
            groupBox_StatusWindow.BackColor = SystemColors.Control;
			//RR
            groupBox_StatusWindow.Controls.Add(RRLabel);
            groupBox_StatusWindow.Controls.Add(RR50NameLabel);
            groupBox_StatusWindow.Controls.Add(RRNamelabel);
	        groupBox_StatusWindow.Controls.Add(RR50Label);
			//
	        groupBox_StatusWindow.Controls.Add(StatusLabel);
	        groupBox_StatusWindow.Controls.Add(PartialMsgLabel);
	        groupBox_StatusWindow.Controls.Add(StopToEntyMsgLabel);
	        groupBox_StatusWindow.Controls.Add(DynamicTrailingStopMsgLabel);
	        groupBox_StatusWindow.Location = new Point(6, 3);
            groupBox_StatusWindow.Margin = new Padding(2);
            groupBox_StatusWindow.Name = "groupBox_StatusWindow";
            groupBox_StatusWindow.Padding = new Padding(2);
            groupBox_StatusWindow.Size = new Size(154, 132);
            groupBox_StatusWindow.TabIndex = 0;
            groupBox_StatusWindow.TabStop = false;
            groupBox_StatusWindow.Text = "Status Window";
// 
	        // StopToEntyMsgLabel
	        // 
	        StopToEntyMsgLabel.BackColor = _disabledColor;
	        StopToEntyMsgLabel.BorderStyle = BorderStyle.FixedSingle;
	        StopToEntyMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
	        StopToEntyMsgLabel.ForeColor = SystemColors.Window;
	        StopToEntyMsgLabel.Location = new Point(78, 68);
	        StopToEntyMsgLabel.Margin = new Padding(2, 0, 2, 0);
	        StopToEntyMsgLabel.Name = "StopToEntyMsgLabel";
	        StopToEntyMsgLabel.Size = new Size(70, 22);
	        StopToEntyMsgLabel.TabIndex = 3;
	        StopToEntyMsgLabel.Text = "StE NOT";
	        StopToEntyMsgLabel.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // DynamicTrailingStopMsgLabel
	        // 
	        DynamicTrailingStopMsgLabel.BackColor = _disabledColor ;
	        DynamicTrailingStopMsgLabel.BorderStyle = BorderStyle.FixedSingle;
	        DynamicTrailingStopMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
	        DynamicTrailingStopMsgLabel.ForeColor = SystemColors.Window;
	        DynamicTrailingStopMsgLabel.Location = new Point(6, 68);
	        DynamicTrailingStopMsgLabel.Margin = new Padding(2, 0, 2, 0);
	        DynamicTrailingStopMsgLabel.Name = "DynamicTrailingStopMsgLabel";
	        DynamicTrailingStopMsgLabel.Size = new Size(68, 22);
	        DynamicTrailingStopMsgLabel.TabIndex = 3;
	        DynamicTrailingStopMsgLabel.Text = "DTS NOT";
	        DynamicTrailingStopMsgLabel.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // PartialMsgLabel
	        // 
	        PartialMsgLabel.BackColor = _disabledColor ;
	        PartialMsgLabel.BorderStyle = BorderStyle.FixedSingle;
	        PartialMsgLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
	        PartialMsgLabel.ForeColor = SystemColors.Window;
	        PartialMsgLabel.Location = new Point(6, 43);
	        PartialMsgLabel.Margin = new Padding(2, 0, 2, 0);
	        PartialMsgLabel.Name = "PartialMsgLabel";
	        PartialMsgLabel.Size = new Size(142, 22);
	        PartialMsgLabel.TabIndex = 2;
	        PartialMsgLabel.Text = "50% TP Disabled";
	        PartialMsgLabel.TextAlign = ContentAlignment.MiddleCenter;

	        #region RR

	        // 
	        // RR50NameLabel
	        // 
	        RR50NameLabel.AutoSize = true;
	        RR50NameLabel.Location = new Point(84, 90);
	        RR50NameLabel.Margin = new Padding(2, 0, 2, 0);
	        RR50NameLabel.Name = "RR50NameLabel";
	        RR50NameLabel.Size = new Size(55, 13);
	        RR50NameLabel.TabIndex = 2;
	        RR50NameLabel.Text = "R:R - 50%";
	        // 
	        // RRNamelabel
	        // 
	        RRNamelabel.AutoSize = true;
	        RRNamelabel.Location = new Point(7, 90);
	        RRNamelabel.Margin = new Padding(2, 0, 2, 0);
	        RRNamelabel.Name = "RRNamelabel";
	        RRNamelabel.Size = new Size(26, 13);
	        RRNamelabel.TabIndex = 4;
	        RRNamelabel.Text = "R:R";
	        // 
	        // RRLabel
	        // 
	        RRLabel.BackColor = Color.White;
	        RRLabel.BorderStyle = BorderStyle.FixedSingle;
	        RRLabel.Location = new Point(10, 106);
	        RRLabel.Margin = new Padding(2, 0, 2, 0);
	        RRLabel.Name = "RRLabel";
	        RRLabel.Size = new Size(34, 22);
	        RRLabel.TabIndex = 5;
	        RRLabel.Text = "3.46";
	        RRLabel.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // RR50Label
	        // 
	        RR50Label.BackColor = Color.White;
	        RR50Label.BorderStyle = BorderStyle.FixedSingle;
	        RR50Label.Location = new Point(87, 106);
	        RR50Label.Margin = new Padding(2, 0, 2, 0);
	        RR50Label.Name = "RR50Label";
	        RR50Label.Size = new Size(34, 22);
	        RR50Label.TabIndex = 3;
	        RR50Label.Text = "3.46";
	        RR50Label.TextAlign = ContentAlignment.MiddleCenter;
	        // 
	        // StatusLabel
	        // 
	        StatusLabel.BackColor = _disabledColor ;
	        StatusLabel.BorderStyle = BorderStyle.FixedSingle;
	        StatusLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
	        StatusLabel.ForeColor = Color.White;
	        StatusLabel.Location = new Point(6, 18);
	        StatusLabel.Margin = new Padding(2, 0, 2, 0);
	        StatusLabel.Name = "StatusLabel";
	        StatusLabel.Size = new Size(142, 22);
	        StatusLabel.TabIndex = 0;
	        StatusLabel.Text = "Not Active";
	        StatusLabel.TextAlign = ContentAlignment.MiddleCenter;

	        #endregion

	        // 
	        // comboBox_OrderType
	        // 
	        comboBox_OrderType.DropDownStyle = ComboBoxStyle.DropDownList;
	        comboBox_OrderType.FormattingEnabled = true;
	        comboBox_OrderType.Items.AddRange(new object[] {
            "LIMIT",
            "MARKET"});
	        comboBox_OrderType.SelectedIndex=0;
	        comboBox_OrderType.Location = new Point(80, 17);
	        comboBox_OrderType.Margin = new Padding(2);
            comboBox_OrderType.Name = "comboBox_OrderType";
            comboBox_OrderType.Size = new Size(66, 21);
            comboBox_OrderType.TabIndex = 4;
            // 
            // groupBox_PartialProfit
            // 
            groupBox_PartialProfit.BackColor = SystemColors.ControlLight;
            groupBox_PartialProfit.Controls.Add(checkBox_EnablePartialProfitAlert);
            groupBox_PartialProfit.Controls.Add(button_CloseHalfPosition);
            groupBox_PartialProfit.Controls.Add(checkBox_EnablePartialProfit);
            groupBox_PartialProfit.Location = new Point(6, 374);
            groupBox_PartialProfit.Margin = new Padding(2);
            groupBox_PartialProfit.Name = "groupBox_PartialProfit";
            groupBox_PartialProfit.Padding = new Padding(2);
            groupBox_PartialProfit.Size = new Size(154, 89);
            groupBox_PartialProfit.TabIndex = 5;
            groupBox_PartialProfit.TabStop = false;
            groupBox_PartialProfit.Text = "50 % Partial Take Profit";
            // 
            // checkBox_EnablePartialProfitAlert
            // 
            checkBox_EnablePartialProfitAlert.AutoSize = true;
            checkBox_EnablePartialProfitAlert.Location = new Point(8, 68);
            checkBox_EnablePartialProfitAlert.Margin = new Padding(2);
            checkBox_EnablePartialProfitAlert.Name = "checkBox_EnablePartialProfitAlert";
            checkBox_EnablePartialProfitAlert.Size = new Size(111, 17);
            checkBox_EnablePartialProfitAlert.TabIndex = 3;
            checkBox_EnablePartialProfitAlert.Text = "Enable Email Alert";
            checkBox_EnablePartialProfitAlert.UseVisualStyleBackColor = true;
            // 
            // button_CloseHalfPosition
            // 
            button_CloseHalfPosition.BackColor = _disabledColor ;
            button_CloseHalfPosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button_CloseHalfPosition.ForeColor = Color.White;
            button_CloseHalfPosition.Location = new Point(6, 19);
            button_CloseHalfPosition.Margin = new Padding(2);
            button_CloseHalfPosition.Name = "button_CloseHalfPosition";
            button_CloseHalfPosition.Size = new Size(144, 27);
            button_CloseHalfPosition.TabIndex = 2;
            button_CloseHalfPosition.Text = "MANUAL CLOSE 50%";
            button_CloseHalfPosition.UseVisualStyleBackColor = false;
            // 
            // checkBox_EnablePartialProfit
            // 
            checkBox_EnablePartialProfit.AutoSize = true;
            checkBox_EnablePartialProfit.Location = new Point(8, 50);
            checkBox_EnablePartialProfit.Margin = new Padding(2);
            checkBox_EnablePartialProfit.Name = "checkBox_EnablePartialProfit";
            checkBox_EnablePartialProfit.Size = new Size(59, 17);
            checkBox_EnablePartialProfit.TabIndex = 0;
            checkBox_EnablePartialProfit.Text = "Enable";
            checkBox_EnablePartialProfit.UseVisualStyleBackColor = true;
            checkBox_EnablePartialProfit.CheckedChanged += Enable50Profit_Changed;
            // 
            // groupBox_Quantity
            // 
            groupBox_Quantity.BackColor = SystemColors.ControlLight;
            groupBox_Quantity.Controls.Add(button_UpdateQuantity);
            groupBox_Quantity.Controls.Add(numericUpDown_Quantity);
            groupBox_Quantity.Location = new Point(6, 137);
            groupBox_Quantity.Margin = new Padding(2);
            groupBox_Quantity.Name = "groupBox_Quantity";
            groupBox_Quantity.Padding = new Padding(2);
            groupBox_Quantity.Size = new Size(154, 41);
            groupBox_Quantity.TabIndex = 4;
            groupBox_Quantity.TabStop = false;
            groupBox_Quantity.Text = "Quantity";
            // 
            // button_UpdateQuantity
            // 
            button_UpdateQuantity.BackColor = Color.SkyBlue;
            button_UpdateQuantity.Location = new Point(8, 15);
            button_UpdateQuantity.Margin = new Padding(2);
            button_UpdateQuantity.Name = "button_UpdateQuantity";
            button_UpdateQuantity.Size = new Size(59, 22);
            button_UpdateQuantity.TabIndex = 1;
            button_UpdateQuantity.Text = "Update";
            button_UpdateQuantity.UseVisualStyleBackColor = false;
            // 
            // numericUpDown_Quantity
            // 
            numericUpDown_Quantity.Location = new Point(71, 16);
            numericUpDown_Quantity.Margin = new Padding(2);
            numericUpDown_Quantity.Maximum = new decimal(new int[] { 999, 0, 0, 0});
            numericUpDown_Quantity.Minimum = new decimal(new int[] { 1, 0, 0, 0});
            numericUpDown_Quantity.Name = "numericUpDown_Quantity";
            numericUpDown_Quantity.Size = new Size(79, 20);
            numericUpDown_Quantity.TabIndex = 0;
            numericUpDown_Quantity.TextAlign = HorizontalAlignment.Center;
            numericUpDown_Quantity.Value = new decimal(new int[] { 1, 0, 0, 0});
            // 
            // button_ClosePosition
            // 
            button_ClosePosition.BackColor = _disabledColor ;
            button_ClosePosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button_ClosePosition.ForeColor = Color.White;
            button_ClosePosition.Location = new Point(4, 154);
            button_ClosePosition.Margin = new Padding(2);
            button_ClosePosition.Name = "button_ClosePosition";
            button_ClosePosition.Size = new Size(144, 27);
            button_ClosePosition.TabIndex = 3;
            button_ClosePosition.Text = "MANUAL CLOSE 100%";
            button_ClosePosition.UseVisualStyleBackColor = false;
			button_ClosePosition.Click += button_ClosePositionClick;
            // 
            // button_ManualShort
            // 
            button_ManualShort.BackColor = Color.MediumOrchid;
            button_ManualShort.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button_ManualShort.ForeColor = Color.White;
            button_ManualShort.Location = new Point(79, 41);
            button_ManualShort.Margin = new Padding(2);
            button_ManualShort.Name = "button_ManualShort";
            button_ManualShort.Size = new Size(69, 27);
            button_ManualShort.TabIndex = 2;
            button_ManualShort.Text = "SHORT";
            button_ManualShort.UseVisualStyleBackColor = false;
            button_ManualShort.Click += button_ManualShort_Click;
            // 
            // button_ManualLong
            // 
	        button_ManualLong.BackColor = _disabledColor;
            button_ManualLong.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button_ManualLong.ForeColor = Color.White;
            button_ManualLong.Location = new Point(4, 41);
            button_ManualLong.Margin = new Padding(2);
            button_ManualLong.Name = "button_ManualLong";
            button_ManualLong.Size = new Size(70, 27);
            button_ManualLong.TabIndex = 1;
            button_ManualLong.Text = "LONG";
            button_ManualLong.UseVisualStyleBackColor = false;
            button_ManualLong.Click += button_ManualLong_Click;
            // 
            // MainPanel
            // 
            MainPanel.BackColor = SystemColors.Control;
            MainPanel.Controls.Add(groupBox1);
            MainPanel.Controls.Add(groupBox_StopToEntry);
            MainPanel.Controls.Add(groupBox_TrailStop);
            MainPanel.Controls.Add(groupBox_StatusWindow);
            MainPanel.Controls.Add(groupBox_PartialProfit);
            MainPanel.Controls.Add(groupBox_Quantity);
            MainPanel.Controls.Add(groupBox_Mode);
            MainPanel.Dock = DockStyle.Right;
            MainPanel.Location = new Point(931, 0);
            MainPanel.Margin = new Padding(2);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(165, 831);
            MainPanel.TabIndex = 1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(numericUpDown_BarEntry);
            groupBox1.Controls.Add(checkBox_EnableBarEntry);
            groupBox1.Location = new Point(6, 704);
            groupBox1.Margin = new Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2);
            groupBox1.Size = new Size(154, 47);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Bar Entry";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label4.Location = new Point(84, 21);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(28, 13);
            label4.TabIndex = 13;
            label4.Text = "Bars";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numericUpDown_BarEntry
            // 
            numericUpDown_BarEntry.Location = new Point(116, 18);
            numericUpDown_BarEntry.Margin = new Padding(2);
            numericUpDown_BarEntry.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            numericUpDown_BarEntry.Name = "numericUpDown_BarEntry";
            numericUpDown_BarEntry.Size = new Size(34, 20);
            numericUpDown_BarEntry.TabIndex = 12;
            numericUpDown_BarEntry.TextAlign = HorizontalAlignment.Center;
            numericUpDown_BarEntry.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // checkBox_EnableBarEntry
            // 
            checkBox_EnableBarEntry.AutoSize = true;
            checkBox_EnableBarEntry.Location = new Point(10, 20);
            checkBox_EnableBarEntry.Margin = new Padding(2);
            checkBox_EnableBarEntry.Name = "checkBox_EnableBarEntry";
            checkBox_EnableBarEntry.Size = new Size(59, 17);
            checkBox_EnableBarEntry.TabIndex = 5;
            checkBox_EnableBarEntry.Text = "Enable";
            checkBox_EnableBarEntry.UseVisualStyleBackColor = true;
            // 
            // groupBox_TrailStop
            // 
            groupBox_TrailStop.BackColor = SystemColors.ControlLight;
            groupBox_TrailStop.Controls.Add(label9);
            groupBox_TrailStop.Controls.Add(label8);
            groupBox_TrailStop.Controls.Add(label7);
            groupBox_TrailStop.Controls.Add(numericUpDown_SwingIndicatorBars);
            groupBox_TrailStop.Controls.Add(numericUpDown_HorizCrossTicks);
            groupBox_TrailStop.Controls.Add(checkBox_EnableTrailStopAlert);
            groupBox_TrailStop.Controls.Add(checkBox_EnableTrailStop);
            groupBox_TrailStop.Controls.Add(numericUpDown_StopLevelTicks);
            groupBox_TrailStop.Location = new Point(6, 579);
            groupBox_TrailStop.Margin = new Padding(2);
            groupBox_TrailStop.Name = "groupBox_TrailStop";
            groupBox_TrailStop.Padding = new Padding(2);
            groupBox_TrailStop.Size = new Size(154, 123);
            groupBox_TrailStop.TabIndex = 7;
            groupBox_TrailStop.TabStop = false;
            groupBox_TrailStop.Text = "Dynamic Trail Stop";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label9.Location = new Point(5, 102);
            label9.Margin = new Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new Size(109, 13);
            label9.TabIndex = 16;
            label9.Text = "Horizontal [pips/ticks]";
            label9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label8.Location = new Point(5, 81);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(113, 13);
            label8.TabIndex = 15;
            label8.Text = "Stop Level [pips/ticks]";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label7.Location = new Point(5, 59);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(109, 13);
            label7.TabIndex = 14;
            label7.Text = "Swing Indicator [bars]";
            label7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numericUpDown_SwingIndicatorBars
            // 
            numericUpDown_SwingIndicatorBars.Location = new Point(118, 57);
            numericUpDown_SwingIndicatorBars.Margin = new Padding(2);
            numericUpDown_SwingIndicatorBars.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            numericUpDown_SwingIndicatorBars.Name = "numericUpDown_SwingIndicatorBars";
            numericUpDown_SwingIndicatorBars.Size = new Size(34, 20);
            numericUpDown_SwingIndicatorBars.TabIndex = 11;
            numericUpDown_SwingIndicatorBars.TextAlign = HorizontalAlignment.Center;
            numericUpDown_SwingIndicatorBars.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // numericUpDown_HorizCrossTicks
            // 
            numericUpDown_HorizCrossTicks.Location = new Point(118, 78);
            numericUpDown_HorizCrossTicks.Margin = new Padding(2);
            numericUpDown_HorizCrossTicks.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            numericUpDown_HorizCrossTicks.Name = "numericUpDown_HorizCrossTicks";
            numericUpDown_HorizCrossTicks.Size = new Size(34, 20);
            numericUpDown_HorizCrossTicks.TabIndex = 12;
            numericUpDown_HorizCrossTicks.TextAlign = HorizontalAlignment.Center;
            numericUpDown_HorizCrossTicks.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // checkBox_EnableTrailStopAlert
            // 
            checkBox_EnableTrailStopAlert.AutoSize = true;
            checkBox_EnableTrailStopAlert.Location = new Point(9, 35);
            checkBox_EnableTrailStopAlert.Margin = new Padding(2);
            checkBox_EnableTrailStopAlert.Name = "checkBox_EnableTrailStopAlert";
            checkBox_EnableTrailStopAlert.Size = new Size(111, 17);
            checkBox_EnableTrailStopAlert.TabIndex = 5;
            checkBox_EnableTrailStopAlert.Text = "Enable Email Alert";
            checkBox_EnableTrailStopAlert.UseVisualStyleBackColor = true;
            // 
            // checkBox_EnableTrailStop
            // 
            checkBox_EnableTrailStop.AutoSize = true;
            checkBox_EnableTrailStop.Location = new Point(9, 18);
            checkBox_EnableTrailStop.Margin = new Padding(2);
            checkBox_EnableTrailStop.Name = "checkBox_EnableTrailStop";
            checkBox_EnableTrailStop.Size = new Size(59, 17);
            checkBox_EnableTrailStop.TabIndex = 4;
            checkBox_EnableTrailStop.Text = "Enable";
            checkBox_EnableTrailStop.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_StopLevelTicks
            // 
            numericUpDown_StopLevelTicks.Location = new Point(118, 99);
            numericUpDown_StopLevelTicks.Margin = new Padding(2);
            numericUpDown_StopLevelTicks.Maximum = new decimal(new int[] { 99, 0, 0, 0});
            numericUpDown_StopLevelTicks.Name = "numericUpDown_StopLevelTicks";
            numericUpDown_StopLevelTicks.Size = new Size(34, 20);
            numericUpDown_StopLevelTicks.TabIndex = 13;
            numericUpDown_StopLevelTicks.TextAlign = HorizontalAlignment.Center;
            numericUpDown_StopLevelTicks.Value = new decimal(new int[] { 9, 0, 0, 0});
            // 
            // groupBox_StopToEntry
            // 
            groupBox_StopToEntry.Controls.Add(checkBox_MoveSL_PartialProfitLine);
            groupBox_StopToEntry.Controls.Add(label3);
            groupBox_StopToEntry.Controls.Add(numericUpDown_PipTicksToActivate);
            groupBox_StopToEntry.Controls.Add(button_ManualMoveStop);
            groupBox_StopToEntry.Controls.Add(checkBox_MoveSL_EntryLine);
            groupBox_StopToEntry.Location = new Point(6, 466);
            groupBox_StopToEntry.Margin = new Padding(2);
            groupBox_StopToEntry.Name = "groupBox_StopToEntry";
            groupBox_StopToEntry.Padding = new Padding(2);
            groupBox_StopToEntry.Size = new Size(154, 109);
            groupBox_StopToEntry.TabIndex = 2;
            groupBox_StopToEntry.TabStop = false;
            groupBox_StopToEntry.Text = "Stop to Entry/Partial Profit";
            // 
            // checkBox_MoveSL_PartialProfitLine
            // 
            checkBox_MoveSL_PartialProfitLine.AutoSize = true;
            checkBox_MoveSL_PartialProfitLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            checkBox_MoveSL_PartialProfitLine.Location = new Point(8, 89);
            checkBox_MoveSL_PartialProfitLine.Margin = new Padding(2);
            checkBox_MoveSL_PartialProfitLine.Name = "checkBox_MoveSL_PartialProfitLine";
            checkBox_MoveSL_PartialProfitLine.Size = new Size(149, 17);
            checkBox_MoveSL_PartialProfitLine.TabIndex = 6;
            checkBox_MoveSL_PartialProfitLine.Text = "Move Stop to Partial Profit";
            checkBox_MoveSL_PartialProfitLine.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label3.Location = new Point(5, 54);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(111, 13);
            label3.TabIndex = 13;
            label3.Text = "Pips/Ticks to activate";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numericUpDown_PipTicksToActivate
            // 
            numericUpDown_PipTicksToActivate.Location = new Point(116, 51);
            numericUpDown_PipTicksToActivate.Margin = new Padding(2);
            numericUpDown_PipTicksToActivate.Maximum = new decimal(new int[] { 99, 0, 0, 0});
            numericUpDown_PipTicksToActivate.Name = "numericUpDown_PipTicksToActivate";
            numericUpDown_PipTicksToActivate.Size = new Size(34, 20);
            numericUpDown_PipTicksToActivate.TabIndex = 12;
            numericUpDown_PipTicksToActivate.TextAlign = HorizontalAlignment.Center;
            numericUpDown_PipTicksToActivate.Value = new decimal(new int[] { 10, 0, 0, 0});
            // 
            // button_ManualMoveStop
            // 
            button_ManualMoveStop.BackColor = _disabledColor ;
            button_ManualMoveStop.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button_ManualMoveStop.ForeColor = Color.White;
            button_ManualMoveStop.Location = new Point(5, 19);
            button_ManualMoveStop.Margin = new Padding(2);
            button_ManualMoveStop.Name = "button_ManualMoveStop";
            button_ManualMoveStop.Size = new Size(145, 27);
            button_ManualMoveStop.TabIndex = 3;
            button_ManualMoveStop.Text = "MANUAL MOVE STOP";
            button_ManualMoveStop.UseVisualStyleBackColor = false;
            // 
            // checkBox_MoveSL_EntryLine
            // 
            checkBox_MoveSL_EntryLine.AutoSize = true;
            checkBox_MoveSL_EntryLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            checkBox_MoveSL_EntryLine.Location = new Point(8, 73);
            checkBox_MoveSL_EntryLine.Margin = new Padding(2);
            checkBox_MoveSL_EntryLine.Name = "checkBox_MoveSL_EntryLine";
            checkBox_MoveSL_EntryLine.Size = new Size(140, 17);
            checkBox_MoveSL_EntryLine.TabIndex = 3;
            checkBox_MoveSL_EntryLine.Text = "Move Stop to Entry Line";
            checkBox_MoveSL_EntryLine.UseVisualStyleBackColor = true;
            // 
            // groupBox_Mode
            // 
            groupBox_Mode.BackColor = SystemColors.Control;
            groupBox_Mode.Controls.Add(_buttonActivate);
            groupBox_Mode.Controls.Add(_buttonClearSelection);
            groupBox_Mode.Controls.Add(button_MakeHorizLine);
            groupBox_Mode.Controls.Add(label1);
            groupBox_Mode.Controls.Add(comboBox_OrderType);
            groupBox_Mode.Controls.Add(button_ManualLong);
            groupBox_Mode.Controls.Add(button_ManualShort);
            groupBox_Mode.Controls.Add(button_ClosePosition);
            groupBox_Mode.Location = new Point(6, 181);
            groupBox_Mode.Margin = new Padding(2);
            groupBox_Mode.Name = "groupBox_Mode";
            groupBox_Mode.Padding = new Padding(2);
            groupBox_Mode.Size = new Size(154, 189);
            groupBox_Mode.TabIndex = 6;
            groupBox_Mode.TabStop = false;
            groupBox_Mode.Text = "Mode";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 21);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(63, 13);
            label1.TabIndex = 7;
            label1.Text = "Order Type:";
            // 
            // button_MakeHorizLine
            // 
            button_MakeHorizLine.BackColor = Color.SkyBlue;
            button_MakeHorizLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button_MakeHorizLine.Location = new Point(4, 126);
            button_MakeHorizLine.Margin = new Padding(2);
            button_MakeHorizLine.Name = "button_MakeHorizLine";
            button_MakeHorizLine.Size = new Size(144, 26);
            button_MakeHorizLine.TabIndex = 4;
            button_MakeHorizLine.Text = "MAKE HORIZONTAL";
            button_MakeHorizLine.UseVisualStyleBackColor = false;
			button_MakeHorizLine.Click += button_MakeHorizizontalLine_Click;

			//Some text to put here
            groupBox_StatusWindow.ResumeLayout(false);
            groupBox_StatusWindow.PerformLayout();
            groupBox_PartialProfit.ResumeLayout(false);
            groupBox_PartialProfit.PerformLayout();
            groupBox_Quantity.ResumeLayout(false);
            ((ISupportInitialize)(numericUpDown_Quantity)).EndInit();
            MainPanel.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((ISupportInitialize)(numericUpDown_BarEntry)).EndInit();
            groupBox_TrailStop.ResumeLayout(false);
            groupBox_TrailStop.PerformLayout();
            ((ISupportInitialize)(numericUpDown_SwingIndicatorBars)).EndInit();
            ((ISupportInitialize)(numericUpDown_HorizCrossTicks)).EndInit();
            ((ISupportInitialize)(numericUpDown_StopLevelTicks)).EndInit();
            groupBox_StopToEntry.ResumeLayout(false);
            groupBox_StopToEntry.PerformLayout();
            ((ISupportInitialize)(numericUpDown_PipTicksToActivate)).EndInit();
            groupBox_Mode.ResumeLayout(false);
            groupBox_Mode.PerformLayout();

            ChartControl.Controls.Add(MainPanel);
        }

	    private void button_ClosePositionClick(object sender, EventArgs e)
	    {
		    if (Position.MarketPosition == MarketPosition.Flat)
		    {
			    MessageBox.Show("We have nothing to close");
			    return;
		    }
			if(MarketPosition.Short==Position.MarketPosition)
				ExitShort();

			if(MarketPosition.Long==Position.MarketPosition)
				ExitLong();
	    }

	    #endregion

		#region Properties
		#endregion
	}

	internal class RayContainer
	{
		private static Color EnterColor = Color.DarkRed;
		private static Color TPColor = Color.Lime;
		private static Color StopColor = Color.Red;

		public MarketPosition PositionType;
		public static int TicksTarget = 5;

		public IRay _originRay;
		public IRay _entryRay;
		public IRay _stopRay;
		public IRay _profitTargetRay;

		private Color DotColor = Color.Black;
		private Color TextColror = Color.Black;
		private IDot _eDot;
		private IText _eText;

		private IDot _sDot;
		private IText _sText;
		private IDot _tpDot;
		private IText _tpText;
		private int _textShift = 10;

		private Strategy _Strategy { get; set; }
		
		public RayContainer(MarketPosition marketPosition, IRay ray, Strategy strategy)
		{
			//Initialization over variables
			_Strategy = strategy;
			PositionType = marketPosition;

			double distance = TicksTarget * strategy.TickSize;
			//Setting oposit direaction if we are in the long
			if (marketPosition == MarketPosition.Long)
				distance *= -1;

			//Crating the lines
			_originRay = ray;

			_entryRay = strategy.DrawRay("Enter", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance*.3, ray.Anchor2BarsAgo, ray.Anchor2Y - distance*.3,
				EnterColor, DashStyle.Solid, 2);

			_stopRay = strategy.DrawRay("Stop", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y + distance, ray.Anchor2BarsAgo, ray.Anchor2Y + distance,
				StopColor, DashStyle.Dash, 2);

			_profitTargetRay = strategy.DrawRay("TakeProfit", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance, ray.Anchor2BarsAgo, ray.Anchor2Y - distance,
				TPColor , DashStyle.Dash, 2);

			//Unlocking those rays if want to make them clear
			_entryRay.Locked = false;
			_stopRay.Locked = false;
			_profitTargetRay.Locked = false;
		}

		public void Update()
		{
			//For enter Ray
			_eDot = _Strategy.DrawDot("enterDot", true, 0, RayPrice(_entryRay), DotColor);
			string s = RayPrice(_entryRay).ToString();
			_eText = _Strategy.DrawText("enterText", TextForma(s), 0, RayPrice(_entryRay), TextColror);

			//For stop Ray
			_sDot=_Strategy.DrawDot("stopDot", true, 0, RayPrice(_stopRay), DotColor);
			string text = RayPrice(_stopRay).ToString();
			_sText=_Strategy.DrawText("stopText", TextForma(text), 0, RayPrice(_stopRay), TextColror);

			//For TP Ray
			_tpDot=_Strategy.DrawDot("TPDot", true, 0, RayPrice(_profitTargetRay), DotColor);
			string priceText = RayPrice(_profitTargetRay).ToString();
			_tpText=_Strategy.DrawText("TPText",TextForma(priceText),  0, RayPrice(_profitTargetRay), TextColror);
		}

		private string TextForma(string priceText)
		{
			return new string(' ', priceText.Length+_textShift)+priceText;
		}

		public static double RayPrice(IRay ray)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y)/(ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);
			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance*ray.Anchor2BarsAgo)+ray.Anchor2Y;
			return Math.Round(rayPrice,5);
		} 

		public void Clear()
		{
			//Reomving lines
			_Strategy.RemoveDrawObject(_entryRay);
			_Strategy.RemoveDrawObject(_stopRay);
			_Strategy.RemoveDrawObject(_profitTargetRay);

			//Removing Dots
			_Strategy.RemoveDrawObject(_eDot);
			_Strategy.RemoveDrawObject(_sDot);
			_Strategy.RemoveDrawObject(_tpDot);

			//Removing Text
			_Strategy.RemoveDrawObject(_sText);
			_Strategy.RemoveDrawObject(_eText);
			_Strategy.RemoveDrawObject(_tpText);
		}

	}

}

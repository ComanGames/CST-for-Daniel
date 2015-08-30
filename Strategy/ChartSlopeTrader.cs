#region Using declarations

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;

#endregion

// ReSharper disable once CheckNamespace
namespace NinjaTrader.Strategy
{
	internal class RayContainer
	{
		private static readonly Color EnterColor = Color.DarkRed;
		private static readonly Color TpColor = Color.Lime;
		private static readonly Color StopColor = Color.Red;
		private static readonly Color HcColor = Color.Purple;

		public MarketPosition PositionType;
		public static int TicksTarget = 5;

		public IRay OriginRay;
		public IRay EntryRay;
		public IRay StopRay;
		public IRay HalfCloseRay;
		public IRay ProfitTargetRay;

		private readonly Color _dotColor = Color.Black;
		private readonly Color _textColor = Color.Black;
		private IDot _eDot;
		private IText _eText;

		private IDot _sDot;
		private IText _sText;
		private IDot _tpDot;
		private IText _tpText;
		private IText _hcText;
		private IDot _hcDot;

		private readonly int _textShift = 10;
		public bool ClosingHalf;
		private readonly Strategy _strategy;
		private readonly double _distance;
		

		public RayContainer(MarketPosition marketPosition, IRay ray, Strategy strategy, bool isCloseHalf, bool isDts)
		{
			//Initialization global variables
			_strategy = strategy;
			PositionType = marketPosition;
			_distance = TicksTarget*strategy.TickSize;

			//Set some local variables
			double distance = _distance;
			//Setting  up down if we are in the long
			if (marketPosition == MarketPosition.Long)
				distance *= -1;

			//Crating the lines
			OriginRay = ray;

			EntryRay = strategy.DrawRay("Enter", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance*.3, ray.Anchor2BarsAgo, ray.Anchor2Y - distance*.3,
				EnterColor, DashStyle.Solid, 2);

			StopRay = strategy.DrawRay("Stop", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y + distance, ray.Anchor2BarsAgo, ray.Anchor2Y + distance,
				StopColor, DashStyle.Dash, 2);

			ProfitTargetRay = strategy.DrawRay("TakeProfit", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y - distance, ray.Anchor2BarsAgo, ray.Anchor2Y - distance,
				TpColor, DashStyle.Dash, 2);

			if (isDts)
			{
				MadeStopRayHorizontal();
			}
			if (isCloseHalf)
			{
				PartialProfitEnable();
			}
			//Unlocking those rays if want to make them clear
			EntryRay.Locked = false;
			StopRay.Locked = false;
			ProfitTargetRay.Locked = false;
		}

		private void MadeStopRayHorizontal()
		{
			double position = RayPrice(StopRay);
			StopRay.Anchor1Y = position;
			StopRay.Anchor2Y = position;
		}

		public void Update()
		{
			//For enter Ray
			_eDot = _strategy.DrawDot("enterDot", true, 0, RayPrice(EntryRay), _dotColor);
			double s = RayPrice(EntryRay);
			_eText = _strategy.DrawText("enterText", TextForma(s), 0, RayPrice(EntryRay), _textColor);

			//For stop Ray
			_sDot = _strategy.DrawDot("stopDot", true, 0, RayPrice(StopRay), _dotColor);
			double text = RayPrice(StopRay);
			_sText = _strategy.DrawText("stopText", TextForma(text), 0, RayPrice(StopRay), _textColor);

			//For TP Ray
			_tpDot = _strategy.DrawDot("TPDot", true, 0, RayPrice(ProfitTargetRay), _dotColor);
			double priceText = RayPrice(ProfitTargetRay);
			_tpText = _strategy.DrawText("TPText", TextForma(priceText), 0, RayPrice(ProfitTargetRay), _textColor);
			if (ClosingHalf)
			{
				_hcDot = _strategy.DrawDot("HCDot", true, 0, RayPrice(HalfCloseRay), _dotColor);
				double priceT = RayPrice(HalfCloseRay);
				_hcText = _strategy.DrawText("HCText", TextForma(priceT), 0, RayPrice(HalfCloseRay), _textColor);
			}
		}

		private string TextForma(double price)
		{
			string priceText = Math.Round(price, 4).ToString(CultureInfo.InvariantCulture);
			return new string(' ', priceText.Length + _textShift) + priceText;
		}

		public static double RayPrice(IRay ray)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y)/(ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);
			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance*ray.Anchor2BarsAgo) + ray.Anchor2Y;
			return Math.Round(rayPrice, 5);
		}

		public void Clear()
		{
			//Removing lines
			_strategy.RemoveDrawObject(EntryRay);
			_strategy.RemoveDrawObject(StopRay);
			_strategy.RemoveDrawObject(ProfitTargetRay);

			//Removing Dots
			_strategy.RemoveDrawObject(_eDot);
			_strategy.RemoveDrawObject(_sDot);
			_strategy.RemoveDrawObject(_tpDot);

			//Removing Text
			_strategy.RemoveDrawObject(_sText);
			_strategy.RemoveDrawObject(_eText);
			_strategy.RemoveDrawObject(_tpText);

			if (ClosingHalf)
			{
				_strategy.RemoveDrawObject(HalfCloseRay);
				_strategy.RemoveDrawObject(_hcText);
				_strategy.RemoveDrawObject(_hcDot);
			}
		}

		public void PartialProfitEnable()
		{
			ClosingHalf = true;
			//Distance we will count from profit Ray
			double d = _distance;
			if (PositionType == MarketPosition.Long)
				d *= -1;

			//Drawing the ray
			HalfCloseRay = _strategy.DrawRay("HalfClose", false,
				ProfitTargetRay.Anchor1BarsAgo, ProfitTargetRay.Anchor1Y + d*.6, ProfitTargetRay.Anchor2BarsAgo,
				ProfitTargetRay.Anchor2Y + d*.6,
				HcColor, DashStyle.Dash, 2);

			HalfCloseRay.Locked = false;
		}

		public void ParialProfitDisable()
		{
			ClosingHalf = false;
			//Remove the ray if we got it already
			if (HalfCloseRay != null)
				_strategy.RemoveDrawObject(HalfCloseRay);
		}
	}

	[Description("Trade Slope Lines")]
	public class ChartSlopeTrader : Strategy
	{


		#region Variables
		private ToolStrip _myToolStrip;
		private ToolStripButton _myTsButton;
		private ToolStripSeparator _myTsSeparator;

		private readonly Color _enabledColor = Color.ForestGreen;
		private readonly Color _disabledColor = Color.LightCoral;
		private bool _showPanel;
		private bool _isActive;
		private bool _isCountingRr;
		private bool _isRrAfter;
		private string _pleaseSelectRay = "Please Select Ray";
		private RayContainer _currentRayContainer;
		private IOrder _currentOrder;
		private bool _doBigger;
		private bool isDTS;

		public const uint LimitShift = 10;

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

		protected override void OnTermination()
		{
			if (_mainPanel != null)
			{
				// Remove and Dispose
				ChartControl.Controls.Remove(_mainPanel);
				_mainPanel.Dispose();
				_mainPanel = null;
			}

			// Remove My Toolstrip Button
			if ((_myToolStrip != null) && (_myTsButton != null))
			{
				_myToolStrip.Items.Remove(_myTsButton);
				_myToolStrip.Items.Remove(_myTsSeparator);
			}

			_myToolStrip = null;
			_myTsButton = null;
			_myTsSeparator = null;
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
					OrderTriggerEnterShortStop();
				else
					OrderTriggerEnterLongStop();
			}
			else
			{

				if (_currentRayContainer.ClosingHalf)
				{
					if (_currentRayContainer.PositionType == MarketPosition.Short)
					{
						if (Close[0] < RayContainer.RayPrice(_currentRayContainer.HalfCloseRay))
						{
							ExitShortStop(_currentOrder.Quantity/2, RayContainer.RayPrice(_currentRayContainer.HalfCloseRay));
							_currentRayContainer.ClosingHalf = false;
						}
					}
					else
					{
						if (Close[0] > RayContainer.RayPrice(_currentRayContainer.HalfCloseRay))
						{
							ExitLongStop(_currentOrder.Quantity/2, RayContainer.RayPrice(_currentRayContainer.HalfCloseRay));
							_currentRayContainer.ClosingHalf = false;
						}
					}
				}
				else
				{
					SetProfitTarget(CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.ProfitTargetRay));
					SetStopLoss(CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.StopRay));
				}
			}
		}

		private void OrderTriggerEnterLongStop()
		{
			int quantity = (int) _numericUpDownQuantity.Value;
			double stopPrice = RayContainer.RayPrice(_currentRayContainer.EntryRay);
				EnterLongStop(quantity,stopPrice);
		}

		private void OrderTriggerEnterShortStop()
		{
			int quantity = (int) _numericUpDownQuantity.Value;
			double stopPrice = RayContainer.RayPrice(_currentRayContainer.EntryRay);
			EnterShortStop(quantity, stopPrice);
		}


		private void SetSLandTp()
		{
			IRay entryRay = _currentRayContainer.EntryRay;
			if (_currentRayContainer.PositionType == MarketPosition.Long)
			{
				if (Close[0] > RayContainer.RayPrice(entryRay))
					SetTrailStop("TrailingStopCST", CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.ProfitTargetRay), true);
				else
					SetStopLoss("StopLossCST", CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.StopRay), false);
			}
			else
			{
				if (Close[0] < RayContainer.RayPrice(entryRay))
					SetTrailStop("TrailingStopCST", CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.ProfitTargetRay), true);
				else
					SetStopLoss("StopLossCST", CalculationMode.Price, RayContainer.RayPrice(_currentRayContainer.StopRay), false);
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
				double risk;
				double reward;
				if (MarketPosition.Flat==Position.MarketPosition)
				{
					risk =
						Math.Abs(RayContainer.RayPrice(_currentRayContainer.EntryRay) -
						         RayContainer.RayPrice(_currentRayContainer.StopRay));
					reward = 
						Math.Abs(RayContainer.RayPrice(_currentRayContainer.ProfitTargetRay) -
						         RayContainer.RayPrice(_currentRayContainer.EntryRay));

				}
				else
				{
					//After we are in a LONG or SHORT position, looking for a profit:
					//Formula(after):
					risk =
						Math.Abs(Close[0]-
						         RayContainer.RayPrice(_currentRayContainer.StopRay));

					reward=
						Math.Abs(Close[0]-
						         RayContainer.RayPrice(_currentRayContainer.ProfitTargetRay));
				}

				if (Math.Abs(reward) > 0.00000000001)
					_rrLabel.Text = Math.Round((reward/risk),2).ToString(CultureInfo.InvariantCulture);
				//For 50% RR 
				if (_currentRayContainer.ClosingHalf)
				{
					if (MarketPosition.Flat == Position.MarketPosition)
					{
						risk =
							Math.Abs(RayContainer.RayPrice(_currentRayContainer.EntryRay) -
							         RayContainer.RayPrice(_currentRayContainer.StopRay));
						reward =
							Math.Abs(RayContainer.RayPrice(_currentRayContainer.HalfCloseRay) -
							         RayContainer.RayPrice(_currentRayContainer.EntryRay));

					}
					else
					{
						//After we are in a LONG or SHORT position, looking for a profit:
						//Formula(after):
						risk =
							Math.Abs(Close[0] -
							         RayContainer.RayPrice(_currentRayContainer.StopRay));

						reward =
							Math.Abs(Close[0] -
							         RayContainer.RayPrice(_currentRayContainer.HalfCloseRay));
					}
					if (Math.Abs(reward) > 0.000000001)
						_rr50Label.Text = Math.Round((reward / risk), 2).ToString();

				}

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
			//Now if rely got this one 
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
				_myTsButton = new ToolStripButton();
				_myTsButton.Click += vShowForm_Panel;

				_myTsSeparator = new ToolStripSeparator();

				_myToolStrip = (ToolStrip)tscontrol[0];
				_myToolStrip.Items.Add(_myTsSeparator);
				_myToolStrip.Items.Add(_myTsButton);

				_myTsButton.ForeColor = Color.Black;
				SetCstButtonText();
			}
		}

		private void SetCstButtonText()
		{
			if (_mainPanel.Visible)
			{
				_myTsButton.Text = "Close CST Control";
				_myTsButton.BackColor =_disabledColor;
			}
			else 
			{
				_myTsButton.Text = "Show CST Control";
				_myTsButton.BackColor = _enabledColor;
			}
		}

		private void UpdateForms()
		{
			if(_doBigger)
				ChartControl.Size = new Size(ChartControl.Size.Width + 1, ChartControl.Size.Height);
			else
				ChartControl.Size = new Size(ChartControl.Size.Width - 1, ChartControl.Size.Height);
			_doBigger = !_doBigger;
			ChartControl.Update();
		}
		private static void MassageIfLineNotSelected()
		{
			MessageBox.Show("Please select line First");
		}


		private void vShowForm_Panel(object s, EventArgs e)
		{
			_mainPanel.Visible = !_mainPanel.Visible;
			SetCstButtonText();
		}

		private void MouseMoveAction(object sender, MouseEventArgs e)
		{
			UpdateGraphics();
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
			if (_isActive)
			{
				MessageBox.Show("We already active we can change position while we Active");
				return;
			}
			//We crating the continer for the rays
			if(_currentRayContainer!=null)
				_currentRayContainer.Clear();
			_currentRayContainer = new RayContainer(MarketPosition.Long, ray, this, _checkBoxEnablePartialProfit.Checked,
				isDTS);
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
			if(_currentRayContainer!=null)
				_currentRayContainer.Clear();
			_currentRayContainer = new RayContainer(MarketPosition.Short, ray, this, _checkBoxEnablePartialProfit.Checked,
				isDTS);
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

		private void Enable50Profit_Changed(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				MessageBox.Show("Please select ray and Long or Short mode first");
				return;
			}
			//Todo:make logic to to Particl Close 
			if (_checkBoxEnablePartialProfit.Checked)
			{
				_partialMsgLabel.Text = "50% TP Enabled";
				_partialMsgLabel.BackColor = _enabledColor;
				_currentRayContainer.PartialProfitEnable();
			}
			else
			{
				_partialMsgLabel.Text = "50% TP Disabled";
				_partialMsgLabel.BackColor = _disabledColor;
				_currentRayContainer.ParialProfitDisable();
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
				_statusLabel.BackColor = _enabledColor;
				if (_currentRayContainer.PositionType == MarketPosition.Long)
				{
					_statusLabel.Text = "Active: Long Position";
				}
				else
				{
					_statusLabel.Text = "Active: Short Position";
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
			_statusLabel.Text = "Not Active";
			_statusLabel.BackColor = _disabledColor;
			_isActive = false;
		}

		#endregion

		#region FORM 01 - VS2010 Controls Initialization

		private void _checkBoxEnableTrailStopChnged(object sender, EventArgs e)
		{
			isDTS = _checkBoxEnableTrailStop.Checked;
			if (_checkBoxEnableTrailStop.Checked)
			{
				_dynamicTrailingStopMsgLabel.Text = "DTS";
				_dynamicTrailingStopMsgLabel.BackColor = _enabledColor;
			}
			else
			{
				_dynamicTrailingStopMsgLabel.Text = "DTS NOT";
				_dynamicTrailingStopMsgLabel.BackColor = _disabledColor;
			}
		}

		private void _checkBoxMoveSlEntryLineCheckChange(object sender, EventArgs e)
		{
			if (_checkBoxMoveSlEntryLine.Checked)
			{
				_stopToEnterMsgLabel.Text = "STE";
				_stopToEnterMsgLabel.BackColor = _enabledColor;
			}
			else
			{
				_stopToEnterMsgLabel.Text = "STE NOT";
				_stopToEnterMsgLabel.BackColor = _disabledColor;
			}
			
		}

		private void button_CloseHalfPositionClick(object sender, EventArgs e)
		{
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				MessageBox.Show("No open position to close hulf");
				return;
			}
			if (Position.MarketPosition == MarketPosition.Short)
				ExitShort(_currentOrder.Quantity/2);
			else
				ExitLong(_currentOrder.Quantity/2);
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
		#region VS2010 Controls Paste
		private GroupBox _groupBoxStatusWindow;
		private GroupBox _groupBoxQuantity;
		private NumericUpDown _numericUpDownQuantity;
		private Button _buttonClosePosition;
		private Button _buttonManualShort;
		private Button _buttonManualLong;
		private Panel _mainPanel;
		private Button _buttonUpdateQuantity;
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
		private NumericUpDown _numericUpDownSwingIndicatorBars;
		private NumericUpDown _numericUpDownHCrossTicks;
		private NumericUpDown _numericUpDownStopLevelTicks;
		private Label _label1;
		private Label _rr50NameLabel;
		private Label _partialMsgLabel;
		private Label _dynamicTrailingStopMsgLabel;
		private GroupBox _groupBoxStopToEntry;
		private Label _label3;
		private NumericUpDown _numericUpDownPipTicksToActivate;
		private Button _buttonManualMoveStop;
		private CheckBox _checkBoxMoveSlPartialProfitLine;
		private CheckBox _checkBoxMoveSlEntryLine;
		private GroupBox _groupBox1;
		private Label _label4;
		private NumericUpDown _numericUpDownBarEntry;
		private CheckBox _checkBoxEnableBarEntry;
		private Label _stopToEnterMsgLabel;
		private Label _rrLabel;
		private Label _rrNamelabel;
		private Label _label7;
		private Label _label9;
		private Label _label8;
		private Button _buttonClearSelection;
		private void VS2010_InitializeComponent_Form()
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
			_buttonCloseHalfPosition = new Button();
			_checkBoxEnablePartialProfit = new CheckBox();
			_groupBoxQuantity = new GroupBox();
			_buttonUpdateQuantity = new Button();
			_numericUpDownQuantity = new NumericUpDown();
			_buttonClosePosition = new Button();
			_buttonManualShort = new Button();
			_buttonManualLong = new Button();
			_mainPanel = new Panel();
			_groupBox1 = new GroupBox();
			_label4 = new Label();
			_numericUpDownBarEntry = new NumericUpDown();
			_checkBoxEnableBarEntry = new CheckBox();
			_groupBoxTrailStop = new GroupBox();
			_label9 = new Label();
			_label8 = new Label();
			_label7 = new Label();
			_numericUpDownSwingIndicatorBars = new NumericUpDown();
			_numericUpDownHCrossTicks = new NumericUpDown();
			_checkBoxEnableTrailStopAlert = new CheckBox();
			_checkBoxEnableTrailStop = new CheckBox();
			_numericUpDownStopLevelTicks = new NumericUpDown();
			_groupBoxStopToEntry = new GroupBox();
			_checkBoxMoveSlPartialProfitLine = new CheckBox();
			_label3 = new Label();
			_numericUpDownPipTicksToActivate = new NumericUpDown();
			_buttonManualMoveStop = new Button();
			_checkBoxMoveSlEntryLine = new CheckBox();
			_groupBoxMode = new GroupBox();
			_label1 = new Label();
			_buttonMakeHorizontalLine = new Button();
			_buttonClearSelection = new Button();
			_buttonActivate = new Button();
			_groupBoxStatusWindow.SuspendLayout();
			_groupBoxPartialProfit.SuspendLayout();
			_groupBoxQuantity.SuspendLayout();
			((ISupportInitialize) (_numericUpDownQuantity)).BeginInit();
			_mainPanel.SuspendLayout();
			_groupBox1.SuspendLayout();
			((ISupportInitialize) (_numericUpDownBarEntry)).BeginInit();
			_groupBoxTrailStop.SuspendLayout();
			((ISupportInitialize) (_numericUpDownSwingIndicatorBars)).BeginInit();
			((ISupportInitialize) (_numericUpDownHCrossTicks)).BeginInit();
			((ISupportInitialize) (_numericUpDownStopLevelTicks)).BeginInit();
			_groupBoxStopToEntry.SuspendLayout();
			((ISupportInitialize) (_numericUpDownPipTicksToActivate)).BeginInit();
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
			_buttonActivate.BackColor = Color.DarkBlue;
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
			_dynamicTrailingStopMsgLabel.BackColor = _disabledColor ;
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
			_partialMsgLabel.BackColor = _disabledColor ;
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
			_rrLabel.Location = new Point(10, 106);
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
			_rr50Label.Location = new Point(87, 106);
			_rr50Label.Margin = new Padding(2, 0, 2, 0);
			_rr50Label.Name = "RR50Label";
			_rr50Label.Size = new Size(34, 22);
			_rr50Label.TabIndex = 3;
			_rr50Label.Text = "3.46";
			_rr50Label.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// StatusLabel
			// 
			_statusLabel.BackColor = _disabledColor ;
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
			_groupBoxPartialProfit.Controls.Add(_checkBoxEnablePartialProfitAlert);
			_groupBoxPartialProfit.Controls.Add(_buttonCloseHalfPosition);
			_groupBoxPartialProfit.Controls.Add(_checkBoxEnablePartialProfit);
			_groupBoxPartialProfit.Location = new Point(6, 374);
			_groupBoxPartialProfit.Margin = new Padding(2);
			_groupBoxPartialProfit.Name = "groupBox_PartialProfit";
			_groupBoxPartialProfit.Padding = new Padding(2);
			_groupBoxPartialProfit.Size = new Size(154, 89);
			_groupBoxPartialProfit.TabIndex = 5;
			_groupBoxPartialProfit.TabStop = false;
			_groupBoxPartialProfit.Text = "50 % Partial Take Profit";
			// 
			// checkBox_EnablePartialProfitAlert
			// 
			_checkBoxEnablePartialProfitAlert.AutoSize = true;
			_checkBoxEnablePartialProfitAlert.Location = new Point(8, 68);
			_checkBoxEnablePartialProfitAlert.Margin = new Padding(2);
			_checkBoxEnablePartialProfitAlert.Name = "checkBox_EnablePartialProfitAlert";
			_checkBoxEnablePartialProfitAlert.Size = new Size(111, 17);
			_checkBoxEnablePartialProfitAlert.TabIndex = 3;
			_checkBoxEnablePartialProfitAlert.Text = "Enable Email Alert";
			_checkBoxEnablePartialProfitAlert.UseVisualStyleBackColor = true;
			// 
			// button_CloseHalfPosition
			// 
			_buttonCloseHalfPosition.BackColor = _disabledColor ;
			_buttonCloseHalfPosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonCloseHalfPosition.ForeColor = Color.White;
			_buttonCloseHalfPosition.Location = new Point(6, 19);
			_buttonCloseHalfPosition.Margin = new Padding(2);
			_buttonCloseHalfPosition.Name = "button_CloseHalfPosition";
			_buttonCloseHalfPosition.Size = new Size(144, 27);
			_buttonCloseHalfPosition.TabIndex = 2;
			_buttonCloseHalfPosition.Text = "MANUAL CLOSE 50%";
			_buttonCloseHalfPosition.UseVisualStyleBackColor = false;
			_buttonCloseHalfPosition.Click += button_CloseHalfPositionClick;
			// 
			// checkBox_EnablePartialProfit
			// 
			_checkBoxEnablePartialProfit.AutoSize = true;
			_checkBoxEnablePartialProfit.Location = new Point(8, 50);
			_checkBoxEnablePartialProfit.Margin = new Padding(2);
			_checkBoxEnablePartialProfit.Name = "checkBox_EnablePartialProfit";
			_checkBoxEnablePartialProfit.Size = new Size(59, 17);
			_checkBoxEnablePartialProfit.TabIndex = 0;
			_checkBoxEnablePartialProfit.Text = "Enable";
			_checkBoxEnablePartialProfit.UseVisualStyleBackColor = true;
			_checkBoxEnablePartialProfit.CheckedChanged += Enable50Profit_Changed;
			// 
			// groupBox_Quantity
			// 
			_groupBoxQuantity.BackColor = SystemColors.ControlLight;
			_groupBoxQuantity.Controls.Add(_buttonUpdateQuantity);
			_groupBoxQuantity.Controls.Add(_numericUpDownQuantity);
			_groupBoxQuantity.Location = new Point(6, 137);
			_groupBoxQuantity.Margin = new Padding(2);
			_groupBoxQuantity.Name = "groupBox_Quantity";
			_groupBoxQuantity.Padding = new Padding(2);
			_groupBoxQuantity.Size = new Size(154, 41);
			_groupBoxQuantity.TabIndex = 4;
			_groupBoxQuantity.TabStop = false;
			_groupBoxQuantity.Text = "Quantity";
			// 
			// button_UpdateQuantity
			// 
			_buttonUpdateQuantity.BackColor = Color.SkyBlue;
			_buttonUpdateQuantity.Location = new Point(8, 15);
			_buttonUpdateQuantity.Margin = new Padding(2);
			_buttonUpdateQuantity.Name = "button_UpdateQuantity";
			_buttonUpdateQuantity.Size = new Size(59, 22);
			_buttonUpdateQuantity.TabIndex = 1;
			_buttonUpdateQuantity.Text = "Update";
			_buttonUpdateQuantity.UseVisualStyleBackColor = false;
			// 
			// numericUpDown_Quantity
			// 
			_numericUpDownQuantity.Location = new Point(71, 16);
			_numericUpDownQuantity.Margin = new Padding(2);
			_numericUpDownQuantity.Maximum = new decimal(new[] { 999, 0, 0, 0});
			_numericUpDownQuantity.Minimum = new decimal(new[] { 1, 0, 0, 0});
			_numericUpDownQuantity.Name = "numericUpDown_Quantity";
			_numericUpDownQuantity.Size = new Size(79, 20);
			_numericUpDownQuantity.TabIndex = 0;
			_numericUpDownQuantity.TextAlign = HorizontalAlignment.Center;
			_numericUpDownQuantity.Value = new decimal(new[] { 1, 0, 0, 0});
			// 
			// button_ClosePosition
			// 
			_buttonClosePosition.BackColor = _disabledColor ;
			_buttonClosePosition.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonClosePosition.ForeColor = Color.White;
			_buttonClosePosition.Location = new Point(4, 154);
			_buttonClosePosition.Margin = new Padding(2);
			_buttonClosePosition.Name = "button_ClosePosition";
			_buttonClosePosition.Size = new Size(144, 27);
			_buttonClosePosition.TabIndex = 3;
			_buttonClosePosition.Text = "MANUAL CLOSE 100%";
			_buttonClosePosition.UseVisualStyleBackColor = false;
			_buttonClosePosition.Click += button_ClosePositionClick;
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
			_buttonManualShort.Click += button_ManualShort_Click;
			// 
			// button_ManualLong
			// 
			_buttonManualLong.BackColor = _disabledColor;
			_buttonManualLong.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonManualLong.ForeColor = Color.White;
			_buttonManualLong.Location = new Point(4, 41);
			_buttonManualLong.Margin = new Padding(2);
			_buttonManualLong.Name = "button_ManualLong";
			_buttonManualLong.Size = new Size(70, 27);
			_buttonManualLong.TabIndex = 1;
			_buttonManualLong.Text = "LONG";
			_buttonManualLong.UseVisualStyleBackColor = false;
			_buttonManualLong.Click += button_ManualLong_Click;
			// 
			// MainPanel
			// 
			_mainPanel.BackColor = SystemColors.Control;
			_mainPanel.Controls.Add(_groupBox1);
			_mainPanel.Controls.Add(_groupBoxStopToEntry);
			_mainPanel.Controls.Add(_groupBoxTrailStop);
			_mainPanel.Controls.Add(_groupBoxStatusWindow);
			_mainPanel.Controls.Add(_groupBoxPartialProfit);
			_mainPanel.Controls.Add(_groupBoxQuantity);
			_mainPanel.Controls.Add(_groupBoxMode);
			_mainPanel.Dock = DockStyle.Right;
			_mainPanel.Location = new Point(931, 0);
			_mainPanel.Margin = new Padding(2);
			_mainPanel.Name = "MainPanel";
			_mainPanel.Size = new Size(165, 831);
			_mainPanel.TabIndex = 1;
			// 
			// groupBox1
			// 
			_groupBox1.Controls.Add(_label4);
			_groupBox1.Controls.Add(_numericUpDownBarEntry);
			_groupBox1.Controls.Add(_checkBoxEnableBarEntry);
			_groupBox1.Location = new Point(6, 704);
			_groupBox1.Margin = new Padding(2);
			_groupBox1.Name = "groupBox1";
			_groupBox1.Padding = new Padding(2);
			_groupBox1.Size = new Size(154, 47);
			_groupBox1.TabIndex = 2;
			_groupBox1.TabStop = false;
			_groupBox1.Text = "Bar Entry";
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
			_numericUpDownBarEntry.Maximum = new decimal(new[] {
				99,
				0,
				0,
				0});
			_numericUpDownBarEntry.Name = "numericUpDown_BarEntry";
			_numericUpDownBarEntry.Size = new Size(34, 20);
			_numericUpDownBarEntry.TabIndex = 12;
			_numericUpDownBarEntry.TextAlign = HorizontalAlignment.Center;
			_numericUpDownBarEntry.Value = new decimal(new[] {
				1,
				0,
				0,
				0});
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
			// 
			// groupBox_TrailStop
			// 
			_groupBoxTrailStop.BackColor = SystemColors.ControlLight;
			_groupBoxTrailStop.Controls.Add(_label9);
			_groupBoxTrailStop.Controls.Add(_label8);
			_groupBoxTrailStop.Controls.Add(_label7);
			_groupBoxTrailStop.Controls.Add(_numericUpDownSwingIndicatorBars);
			_groupBoxTrailStop.Controls.Add(_numericUpDownHCrossTicks);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStopAlert);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStop);
			_groupBoxTrailStop.Controls.Add(_numericUpDownStopLevelTicks);
			_groupBoxTrailStop.Location = new Point(6, 579);
			_groupBoxTrailStop.Margin = new Padding(2);
			_groupBoxTrailStop.Name = "groupBox_TrailStop";
			_groupBoxTrailStop.Padding = new Padding(2);
			_groupBoxTrailStop.Size = new Size(154, 123);
			_groupBoxTrailStop.TabIndex = 7;
			_groupBoxTrailStop.TabStop = false;
			_groupBoxTrailStop.Text = "Dynamic Trail Stop";
			// 
			// label9
			// 
			_label9.AutoSize = true;
			_label9.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label9.Location = new Point(5, 102);
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
			_label8.Location = new Point(5, 81);
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
			_label7.Location = new Point(5, 59);
			_label7.Margin = new Padding(2, 0, 2, 0);
			_label7.Name = "label7";
			_label7.Size = new Size(109, 13);
			_label7.TabIndex = 14;
			_label7.Text = "Swing Indicator [bars]";
			_label7.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numericUpDown_SwingIndicatorBars
			// 
			_numericUpDownSwingIndicatorBars.Location = new Point(118, 57);
			_numericUpDownSwingIndicatorBars.Margin = new Padding(2);
			_numericUpDownSwingIndicatorBars.Maximum = new decimal(new[] {
				99,
				0,
				0,
				0});
			_numericUpDownSwingIndicatorBars.Name = "numericUpDown_SwingIndicatorBars";
			_numericUpDownSwingIndicatorBars.Size = new Size(34, 20);
			_numericUpDownSwingIndicatorBars.TabIndex = 11;
			_numericUpDownSwingIndicatorBars.TextAlign = HorizontalAlignment.Center;
			_numericUpDownSwingIndicatorBars.Value = new decimal(new[] {
				4,
				0,
				0,
				0});
			// 
			// numericUpDown_HorizonCrossTicks
			// 
			_numericUpDownHCrossTicks.Location = new Point(118, 78);
			_numericUpDownHCrossTicks.Margin = new Padding(2);
			_numericUpDownHCrossTicks.Maximum = new decimal(new[] {
				99,
				0,
				0,
				0});
			_numericUpDownHCrossTicks.Name = "numericUpDown_HorizCrossTicks";
			_numericUpDownHCrossTicks.Size = new Size(34, 20);
			_numericUpDownHCrossTicks.TabIndex = 12;
			_numericUpDownHCrossTicks.TextAlign = HorizontalAlignment.Center;
			_numericUpDownHCrossTicks.Value = new decimal(new[] {
				4,
				0,
				0,
				0});
			// 
			// checkBox_EnableTrailStopAlert
			// 
			_checkBoxEnableTrailStopAlert.AutoSize = true;
			_checkBoxEnableTrailStopAlert.Location = new Point(9, 35);
			_checkBoxEnableTrailStopAlert.Margin = new Padding(2);
			_checkBoxEnableTrailStopAlert.Name = "checkBox_EnableTrailStopAlert";
			_checkBoxEnableTrailStopAlert.Size = new Size(111, 17);
			_checkBoxEnableTrailStopAlert.TabIndex = 5;
			_checkBoxEnableTrailStopAlert.Text = "Enable Email Alert";
			_checkBoxEnableTrailStopAlert.UseVisualStyleBackColor = true;
			// 
			// checkBox_EnableTrailStop
			// 
			_checkBoxEnableTrailStop.AutoSize = true;
			_checkBoxEnableTrailStop.Location = new Point(9, 18);
			_checkBoxEnableTrailStop.Margin = new Padding(2);
			_checkBoxEnableTrailStop.Name = "checkBox_EnableTrailStop";
			_checkBoxEnableTrailStop.Size = new Size(59, 17);
			_checkBoxEnableTrailStop.TabIndex = 4;
			_checkBoxEnableTrailStop.Text = "Enable";
			_checkBoxEnableTrailStop.UseVisualStyleBackColor = true;
			_checkBoxEnableTrailStop.CheckedChanged += _checkBoxEnableTrailStopChnged;
			// 
			// numericUpDown_StopLevelTicks
			// 
			_numericUpDownStopLevelTicks.Location = new Point(118, 99);
			_numericUpDownStopLevelTicks.Margin = new Padding(2);
			_numericUpDownStopLevelTicks.Maximum = new decimal(new[] { 99, 0, 0, 0});
			_numericUpDownStopLevelTicks.Name = "numericUpDown_StopLevelTicks";
			_numericUpDownStopLevelTicks.Size = new Size(34, 20);
			_numericUpDownStopLevelTicks.TabIndex = 13;
			_numericUpDownStopLevelTicks.TextAlign = HorizontalAlignment.Center;
			_numericUpDownStopLevelTicks.Value = new decimal(new[] { 9, 0, 0, 0});
			// 
			// groupBox_StopToEntry
			// 
			_groupBoxStopToEntry.Controls.Add(_checkBoxMoveSlPartialProfitLine);
			_groupBoxStopToEntry.Controls.Add(_label3);
			_groupBoxStopToEntry.Controls.Add(_numericUpDownPipTicksToActivate);
			_groupBoxStopToEntry.Controls.Add(_buttonManualMoveStop);
			_groupBoxStopToEntry.Controls.Add(_checkBoxMoveSlEntryLine);
			_groupBoxStopToEntry.Location = new Point(6, 466);
			_groupBoxStopToEntry.Margin = new Padding(2);
			_groupBoxStopToEntry.Name = "groupBox_StopToEntry";
			_groupBoxStopToEntry.Padding = new Padding(2);
			_groupBoxStopToEntry.Size = new Size(154, 109);
			_groupBoxStopToEntry.TabIndex = 2;
			_groupBoxStopToEntry.TabStop = false;
			_groupBoxStopToEntry.Text = "Stop to Entry/Partial Profit";
			// 
			// checkBox_MoveSL_PartialProfitLine
			// 
			_checkBoxMoveSlPartialProfitLine.AutoSize = true;
			_checkBoxMoveSlPartialProfitLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_checkBoxMoveSlPartialProfitLine.Location = new Point(8, 89);
			_checkBoxMoveSlPartialProfitLine.Margin = new Padding(2);
			_checkBoxMoveSlPartialProfitLine.Name = "checkBox_MoveSL_PartialProfitLine";
			_checkBoxMoveSlPartialProfitLine.Size = new Size(149, 17);
			_checkBoxMoveSlPartialProfitLine.TabIndex = 6;
			_checkBoxMoveSlPartialProfitLine.Text = "Move Stop to Partial Profit";
			_checkBoxMoveSlPartialProfitLine.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			_label3.AutoSize = true;
			_label3.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_label3.Location = new Point(5, 54);
			_label3.Margin = new Padding(2, 0, 2, 0);
			_label3.Name = "label3";
			_label3.Size = new Size(111, 13);
			_label3.TabIndex = 13;
			_label3.Text = "Pips/Ticks to activate";
			_label3.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numericUpDown_PipTicksToActivate
			// 
			_numericUpDownPipTicksToActivate.Location = new Point(116, 51);
			_numericUpDownPipTicksToActivate.Margin = new Padding(2);
			_numericUpDownPipTicksToActivate.Maximum = new decimal(new[] { 99, 0, 0, 0});
			_numericUpDownPipTicksToActivate.Name = "numericUpDown_PipTicksToActivate";
			_numericUpDownPipTicksToActivate.Size = new Size(34, 20);
			_numericUpDownPipTicksToActivate.TabIndex = 12;
			_numericUpDownPipTicksToActivate.TextAlign = HorizontalAlignment.Center;
			_numericUpDownPipTicksToActivate.Value = new decimal(new[] { 10, 0, 0, 0});
			// 
			// button_ManualMoveStop
			// 
			_buttonManualMoveStop.BackColor = _disabledColor ;
			_buttonManualMoveStop.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			_buttonManualMoveStop.ForeColor = Color.White;
			_buttonManualMoveStop.Location = new Point(5, 19);
			_buttonManualMoveStop.Margin = new Padding(2);
			_buttonManualMoveStop.Name = "button_ManualMoveStop";
			_buttonManualMoveStop.Size = new Size(145, 27);
			_buttonManualMoveStop.TabIndex = 3;
			_buttonManualMoveStop.Text = "MANUAL MOVE STOP";
			_buttonManualMoveStop.UseVisualStyleBackColor = false;
			// 
			// checkBox_MoveSL_EntryLine
			// 
			_checkBoxMoveSlEntryLine.AutoSize = true;
			_checkBoxMoveSlEntryLine.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			_checkBoxMoveSlEntryLine.Location = new Point(8, 73);
			_checkBoxMoveSlEntryLine.Margin = new Padding(2);
			_checkBoxMoveSlEntryLine.Name = "checkBox_MoveSL_EntryLine";
			_checkBoxMoveSlEntryLine.Size = new Size(140, 17);
			_checkBoxMoveSlEntryLine.TabIndex = 3;
			_checkBoxMoveSlEntryLine.Text = "Move Stop to Entry Line";
			_checkBoxMoveSlEntryLine.UseVisualStyleBackColor = true;
			_checkBoxMoveSlEntryLine.CheckedChanged += _checkBoxMoveSlEntryLineCheckChange;
			// 
			// groupBox_Mode
			// 
			_groupBoxMode.BackColor = SystemColors.Control;
			_groupBoxMode.Controls.Add(_buttonActivate);
			_groupBoxMode.Controls.Add(_buttonClearSelection);
			_groupBoxMode.Controls.Add(_buttonMakeHorizontalLine);
			_groupBoxMode.Controls.Add(_label1);
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
			_label1.AutoSize = true;
			_label1.Location = new Point(6, 21);
			_label1.Margin = new Padding(2, 0, 2, 0);
			_label1.Name = "label1";
			_label1.Size = new Size(63, 13);
			_label1.TabIndex = 7;
			_label1.Text = "Order Type:";
			// 
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
			_buttonMakeHorizontalLine.Click += button_MakeHorizizontalLine_Click;

			//Some text to put here
			_groupBoxStatusWindow.ResumeLayout(false);
			_groupBoxStatusWindow.PerformLayout();
			_groupBoxPartialProfit.ResumeLayout(false);
			_groupBoxPartialProfit.PerformLayout();
			_groupBoxQuantity.ResumeLayout(false);
			((ISupportInitialize)(_numericUpDownQuantity)).EndInit();
			_mainPanel.ResumeLayout(false);
			_groupBox1.ResumeLayout(false);
			_groupBox1.PerformLayout();
			((ISupportInitialize)(_numericUpDownBarEntry)).EndInit();
			_groupBoxTrailStop.ResumeLayout(false);
			_groupBoxTrailStop.PerformLayout();
			((ISupportInitialize)(_numericUpDownSwingIndicatorBars)).EndInit();
			((ISupportInitialize)(_numericUpDownHCrossTicks)).EndInit();
			((ISupportInitialize)(_numericUpDownStopLevelTicks)).EndInit();
			_groupBoxStopToEntry.ResumeLayout(false);
			_groupBoxStopToEntry.PerformLayout();
			((ISupportInitialize)(_numericUpDownPipTicksToActivate)).EndInit();
			_groupBoxMode.ResumeLayout(false);
			_groupBoxMode.PerformLayout();

			ChartControl.Controls.Add(_mainPanel);
		}

		private Button _buttonActivate;

		#endregion
	}
}

#region Using declarations
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using Instrument = NinjaTrader.Cbi.Instrument;
using System.IO;
using System.Threading;

#endregion

// ReSharper disable once CheckNamespace
namespace NinjaTrader.Strategy
{
	//Hello Daniel 
	// I want to show how the CST looks to work from my side	
	//So, here is the build that i sen you lest time 
	//and it almost all the same at it was month ago
	/// <summary>
	/// Example of the Chart Slope Trade
	/// </summary>
	[Description("Trade Slope Lines")]
	public class ChartSlopeTrader : Strategy
	{
		private enum StrategyState
		{
			NotActive,  //When we do not activate our lines
			Enter,      //When we waiting to enter to the market 
			Exit,       //When we trying to exit by using the SL and TP
		}

		#region Variables

		public const uint LimitShift = 10;
		private ToolStrip _myToolStrip;
		private ToolStripButton _myTsButton;
		private ToolStripSeparator _myTsSeparator;

		private readonly Color _enabledColor = Color.ForestGreen;
		private readonly Color _disabledColor = Color.LightCoral;
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private string _pleaseSelectRay = "Please Select Ray";
		private RayContainer _currentRayContainer;
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private IOrder _currentOrder;

		private StrategyState _strategyState = StrategyState.NotActive;

		#endregion

		#region Initialization & Uninitialization
		private bool _canUseOtherInstrument;

		protected override void Initialize()
		{

			try
			{
				BarsRequired = 1;//To test even if we got 1 bar on our chart
				AddOtherCurrency();
				CalculateOnBarClose = false;
				Enabled = true;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + e.Source+e.Data);
			}
		}

		private void AddOtherCurrency()
		{
			_canUseOtherInstrument = IsInstrument();
			_strategyState = StrategyState.NotActive;
			//If i can not use other Instrument show that i can not use other instrument
			if (!_canUseOtherInstrument)
			{
				_checkBoxOtherCurrency.Enabled = false;
				_checkBoxOtherCurrency.Text = "Wrong:";
				_checkBoxOtherCurrency.ForeColor = Color.Red;
			} //Here we add soem functionality to trade with other currancy gm

			if (_canUseOtherInstrument)
				Add(_otherInstrumentName, PeriodType.Minute, 1);
		}

		protected override void OnStartUp()
		{
			try
			{
				// Initialize Forms			
				ChartControl.ChartPanel.MouseUp += MouseUpAction;
				VS2010_InitializeComponent_Form();

				// Add Toolbar Button
				ButtonToThetop();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private bool IsInstrument()
		{
			foreach (Instrument instrument in Instrument.GetObjects())
			{
				if (instrument.FullName == _otherInstrumentName)
				{
					return true;
				}
			}
			return false;
		}

		protected override void OnTermination()
		{
			if (_currentRayContainer != null)
			{
				_currentRayContainer.Clear();
				_currentRayContainer = null;
			}
			VS2010_UnInitializeComponent();

			// Remove My ToolStrip Button
			RemoveButtonsOnTop();
			ChartControl.ChartPanel.MouseUp -= MouseUpAction;
		}

		private void RemoveButtonsOnTop()
		{
			if ((_myToolStrip != null) && (_myTsButton != null))
			{
				_myToolStrip.Items.Remove(_myTsButton);
				_myToolStrip.Items.Remove(_myTsSeparator);
			}

			_myToolStrip = null;
			_myTsButton = null;
			_myTsSeparator = null;
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

		#endregion

		#region  Updates

		private void MouseUpAction(object sender, MouseEventArgs e)
		{
			try
			{
				UpdateGraphics();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		protected override void OnPositionUpdate(IPosition position)
		{
			try
			{
				if (_strategyState == StrategyState.Enter && position.MarketPosition != MarketPosition.Flat)
				{
					SendMailEntryLine();
					_strategyState = StrategyState.Exit;
				}
				//here we made decativation after our order worked
				if (_strategyState == StrategyState.Exit && position.MarketPosition == MarketPosition.Flat)
				{
					SendExitMails();
					SetNotActive();
				}
				base.OnPositionUpdate(position);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + e.Source+e.Data);
			}
		}

		private void SetNotActive()
		{
			CancelAllOrders(false, true);
			_strategyState = StrategyState.NotActive;
			SetVisualToNotActive();
			if (_deActivate)
			{
				_currentOrder = null;
				_deActivate = false;
			}
		}

		private void SendExitMails()
		{
			if (!_deActivate)
			{					
					//Rays prices
					double profitLinePrice = RayPrice(_currentRayContainer.ProfitTargetRay);
					double stopLinePrice = RayPrice(_currentRayContainer.StopRay);
					//Distance from current Price to ray
					double distanceToProfit = Math.Abs(_currentPrice - profitLinePrice);
					double distanceToStop = Math.Abs(_currentPrice - stopLinePrice);

				if (_currentDynamicTrailingStop != null && _checkBoxEnableTrailStopAlert.Checked)
				{
					if (distanceToProfit <= distanceToStop)
						SendMail_dtsLineTP();
					else
						SendMail_dtsLineSL();
				}
				else
				{
					if (distanceToProfit <= distanceToStop)
						SendMailProfitLine();
					else
						SendMailStopLine();
				}
			}
		}

		private void SendMail_dtsLineTP()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("DTS {0} line triggered with TP@{1} ", positionType, _currentPrice);
			string formater = TextFormater("Cross Below/Above SL/TP line @"+_currentPrice, false, false);
			SendMail(topic,formater);
		}
		private void SendMail_dtsLineSL()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("DTS {0} line triggered with SL@{1}", positionType,_currentPrice);
			string formater = TextFormater("Cross Below/Above SL line @"+_currentPrice, false, false);
			SendMail(topic,formater);
		}

		private void SendMailPartialProfitLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("50% Partial {0} TP line triggered @{1}", positionType, _currentPrice);
			string formater = TextFormater("Cross Below/Above 50% line @"+_currentPrice, false, true);
			SendMail(topic,formater);
		}
		private void SendMailProfitLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("TP {0} line triggered TP@{1} ", positionType, _currentPrice);
			string formater = TextFormater( "Cross Below/Above TP line @"+_currentPrice, false, false);
			SendMail(topic,formater);
		}
		private void SendMailStopLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("SL {0} line triggered SL@{1}", positionType, _currentPrice);
			string formater = TextFormater( "Cross Below/Above SL line @"+_currentPrice, false, false);
			SendMail(topic,formater);
		}

		private void SendMailEntryLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("Entry {0} line triggered @{1}" , positionType, _currentPrice);	
			string formater = TextFormater( "Cross Below / Above Entry line @"+_currentPrice, true, false);
			SendMail(topic,formater);
		}


		public double _currentPrice { get; set; }
		protected override void OnBarUpdate()
		{
			try
			{
				if (_deActivate)
				{
					DeActivation();
					return;
				}
				if (BarsInProgress == 0)
				{
					//Work with our main chart
					SettingVariables(); // ALWAys FIRST!!!!!
					if (FirstTickOfBar)
					{
						UpdateGraphics();
						ProportionalDistance = GetDistance();
					}
					if (_currentRayContainer != null)
					{
						UpdateRr();
						if (!_checkBoxOtherCurrency.Checked)
							ThisInstrumentOrders();
						if (!_radioButtonNone.Checked)
							StopToEntryOrParialProfit();
						if (_checkBoxEnableTrailStop.Checked && FirstTickOfBar)
							UpdateDts();
					}
					//Trading on other instrument
				}
				else if (_currentRayContainer!=null&&_canUseOtherInstrument && _checkBoxOtherCurrency.Checked && BarsInProgress == 1)
					OtherInstrumentOrders(); // We are only making the lines trigger on other order and that all
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Source + e.Data + e.StackTrace);
			}
		}

		private double GetDistance()
		{
			if (Close.Count > 3&&High!=null&&High.Count>3)
				try
				{
					ProportionalDistance = GetMaxPrice() - GetMinPrice();
				}
				catch (Exception e)
				{
					ProportionalDistance = 5*TickSize;
				}
			else
				ProportionalDistance = 5*TickSize;
			return ProportionalDistance;
		}

		private double GetMaxPrice()
		{
			int length = Math.Min(30, High.Count-1);
			double result = High[0];
			for (int i = 0; i < length; i++)
				result = Math.Max(result, High[i]);
			return result;
		}

		private double GetMinPrice()
		{
			int length = Math.Min(30, Low.Count-1);
			double result = Low[0];
			for (int i = 0; i < length; i++)
				result = Math.Min(result, Low[i]);

			return result;
		}
		private void SettingVariables()
		{
			if (Position.MarketPosition != MarketPosition.Flat)
				UpdateProfitValue();
			if(Close.Count!=0)
			_currentPrice = Close[0];
		}

		private void UpdateProfitValue()
		{
			_profitLoss = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Currency),4);
			_profitPercent = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Percent), 4);
		}

		private void OtherInstrumentOrders()
		{
			if (_strategyState == StrategyState.Enter)
				OtherInstrumentEntryOrder();
			else if (_strategyState == StrategyState.Exit)
				OtherInstrumentExitOrders();

		}

		private void OtherInstrumentEntryOrder()
		{
			//WE set our parameters
			int quantity = (int)_numericUpDownQuantity.Value;
			double entryPrice = RayPrice(_currentRayContainer.EntryRay);

			if (_currentRayContainer.PositionType == MarketPosition.Long && _currentPrice < entryPrice)
				EnterLong(quantity);
			else if (_currentRayContainer.PositionType == MarketPosition.Short && _currentPrice > entryPrice)
				EnterShort(quantity);
		}

		private void OtherInstrumentExitOrders()
		{
			double profitPrice = RayPrice(_currentRayContainer.ProfitTargetRay);
			double exitPrice = RayPrice(_currentRayContainer.StopRay);
			if (_currentRayContainer.PositionType == MarketPosition.Long)
			{
				if (_currentPrice >= profitPrice || _currentPrice <= exitPrice)
					ExitLong();
			}
			else if (_currentRayContainer.PositionType == MarketPosition.Short)
			{
				if (_currentPrice <= profitPrice || _currentPrice >= exitPrice)
					ExitShort();
			}
				
		}

		private void ThisInstrumentOrders()
		{
			//Setting SL and TP for first activation of order
			if (_firstOrderSet)
			{
				SetSLAndTP();
				_firstOrderSet = false;
			}
			if (_strategyState == StrategyState.Enter)
				UpdateEnterOrders();
			else if (_strategyState == StrategyState.Exit)
				UpdateExitOrders();
		}


		private void UpdateEnterOrders()
		{
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				CreateShortLimit();
			else
				CreateLongLimit();
		}

		private int _activation = 0;
		private void UpdateExitOrders()
		{
			SetSLAndTP();
			if (_currentRayContainer.ClosingHalf)
				UpdateClosingHalf();
			if (_closeHalfNow)
				CloseHalfNow();
		}

		private void SetSLAndTP()
		{
			//Old way to work
			//			SetProfitTarget(CalculationMode.Price,RayPrice(_currentRayContainer.ProfitTargetRay));
			//			SetStopLoss(CalculationMode.Price,RayPrice(_currentRayContainer.StopRay))
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStop(Position.Quantity, RayPrice(_currentRayContainer.StopRay));
				ExitLongLimit(Position.Quantity, RayPrice(_currentRayContainer.ProfitTargetRay));
			}
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStop(Position.Quantity, RayPrice(_currentRayContainer.StopRay));
				ExitShortLimit(Position.Quantity, RayPrice(_currentRayContainer.ProfitTargetRay));
			}
			else
			{
				CancelAllOrders(false, true);
			}
		}

		private void CloseHalfNow()
		{
			int quantity = Position.Quantity / 2;
			double d = (5 * TickSize);
			if (Position.MarketPosition == MarketPosition.Long)
				ExitLong(quantity);
			else if (Position.MarketPosition == MarketPosition.Flat)
				ExitShort(quantity);
			_closeHalfNow = false;
		}

		private void UpdateClosingHalf()
		{
			if (_currentRayContainer.PositionType == MarketPosition.Short)
			{
				if (_currentPrice < RayPrice(_currentRayContainer.HalfCloseRay))
				{
					if (_checkBoxEnablePartialProfitAlert.Checked)
						SendMailPartialProfitLine();
					CloseHalfPositionAndRemoveRay();
				}
			}
			else
			{
				if (_currentPrice > RayPrice(_currentRayContainer.HalfCloseRay))
				{
					if (_checkBoxEnablePartialProfitAlert.Checked)
						SendMailPartialProfitLine();
					CloseHalfPositionAndRemoveRay();
				}
			}
		}

		private void CloseHalfPositionAndRemoveRay()
		{
			int quantity = Position.Quantity / 2;
			double d = (5 * TickSize);
			if (Position.MarketPosition == MarketPosition.Long)
				ExitLong(quantity);
			else if (Position.MarketPosition == MarketPosition.Short)
				ExitShort(quantity);
			_currentRayContainer.ParialProfitDisable();
			_doNoRemoveAltLine = true;
			_checkBoxEnablePartialProfit.Checked = false;
			_doNoRemoveAltLine = false;
		}

		private void StopToEntryOrParialProfit()
		{
			if (_radioButtonEntryLine.Checked)
			{
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					double distance = TickSize * (double)_numericUpDownPipTicksToActivate.Value;
					if (Position.MarketPosition == MarketPosition.Long)
					{
						if (_currentPrice >= RayPrice(_currentRayContainer.EntryRay) + distance)
							MoveStopLineTo(_currentRayContainer.EntryRay, -1);
					}
					else
					{
						if (_currentPrice <= RayPrice(_currentRayContainer.EntryRay) - distance)
							MoveStopLineTo(_currentRayContainer.EntryRay, 1);
					}
				}
			}
			if (_radioButtonPartialProfit.Checked||_currentRayContainer.IsAlternativeLine)
			{
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					double distance = TickSize * (double)_numericUpDownPipTicksToActivate.Value;
					IRay ray = GetHulfRay();
					if (Position.MarketPosition == MarketPosition.Long)
					{
						if (_currentPrice >= RayPrice(ray) + distance)
							MoveStopLineTo(ray, -1);
					}
					else
					{
						if (_currentPrice <= RayPrice(ray) - distance)
							MoveStopLineTo(ray, 1);
					}

				}

			}
		}

		private IRay GetHulfRay()
		{
			return _currentRayContainer.IsAlternativeLine? _currentRayContainer.AlternativeRay: _currentRayContainer.HalfCloseRay;
		}

		private void CreateLongLimit()
		{
			int quantity = (int)_numericUpDownQuantity.Value;
			double stopPrice = RayPrice(_currentRayContainer.EntryRay);
			_currentOrder = EnterLongLimit(quantity, stopPrice);
		}

		private void CreateShortLimit()
		{
			int quantity = (int)_numericUpDownQuantity.Value;
			double stopPrice = RayPrice(_currentRayContainer.EntryRay);
			_currentOrder = EnterShortLimit(quantity, stopPrice);
		}

		private void UpdateGraphics()
		{
			if (_currentRayContainer != null)
			{
				UpdateRr();
				_currentRayContainer.Update();
				_mainPanel.Invalidate();
			}

		}

		private void UpdateRr()
		{
			//=====================================================================
			//								For  RR 
			//=====================================================================
			if (_currentRayContainer != null)
			{
				double risk;
				double reward;
				if (_strategyState == StrategyState.NotActive || _strategyState == StrategyState.Enter)
				{
					risk =
						Math.Abs(RayPriceNotRound(_currentRayContainer.EntryRay) - RayPriceNotRound(_currentRayContainer.StopRay));
					reward =
						Math.Abs(RayPriceNotRound(_currentRayContainer.ProfitTargetRay) - RayPriceNotRound(_currentRayContainer.EntryRay));

				}
				else
				{
					//After we are in a LONG or SHORT position, looking for a profit:
					//Formula(after):
					risk = Math.Abs(_currentPrice -
					                RayPriceNotRound(_currentRayContainer.StopRay));

					reward = Math.Abs(_currentPrice -
					                  RayPriceNotRound(_currentRayContainer.ProfitTargetRay));
				}

				if (Math.Abs(reward) > 0.00000000001)
					_rrLabel.Text = Math.Round((reward / risk), 2).ToString(CultureInfo.InvariantCulture);
				//=====================================================================
				//								For 50% RR 
				//=====================================================================
				if (_currentRayContainer.ClosingHalf)
				{
					if (_strategyState == StrategyState.NotActive || _strategyState == StrategyState.Enter)
					{
						risk =
							Math.Abs(RayPriceNotRound(_currentRayContainer.EntryRay) -
							         RayPriceNotRound(_currentRayContainer.StopRay));
						reward =
							Math.Abs(RayPriceNotRound(_currentRayContainer.HalfCloseRay) -
							         RayPriceNotRound(_currentRayContainer.EntryRay));

					}
					else
					{
						//After we are in a LONG or SHORT position, looking for a profit:
						//Formula(after):
						risk =
							Math.Abs(_currentPrice -
							         RayPriceNotRound(_currentRayContainer.StopRay));

						reward =
							Math.Abs(_currentPrice -
							         RayPriceNotRound(_currentRayContainer.HalfCloseRay));
					}
					if (_checkBoxEnablePartialProfit.Enabled)
					{
						if (Math.Abs(reward) > 0.000000001)
							_rr50Label.Text = Math.Round((reward / risk), 2).ToString(CultureInfo.InvariantCulture);
					}

				}
				else
					_rr50Label.Text = 0.ToString();

			}

		}

		private void UpdateForms()
		{
			ChartControl.ChartPanel.Invalidate();
		}

		#endregion

		#region DTS

		private void DisableDts()
		{
			//Disabling the radio buttons
			_radioButtonNone.Enabled = true;
			_radioButtonEntryLine.Enabled = true;
			_radioButtonPartialProfit.Enabled = true;
			_radioButtonNone.Checked = true;

			//Removing lock from stopRay if we got it 
			if (_currentRayContainer != null) _currentRayContainer.StopRay.Locked = false;
			_currentDynamicTrailingStop = null;

		}

		private void EnableDts()
		{
			if (!_radioButtonNone.Checked)
			{
				MessageBox.Show("You stop to entry active. So i will turn off it for you");
				_radioButtonNone.Checked = true;
			}
			//Disabling the radio buttons
			_radioButtonNone.Enabled = false;
			_radioButtonEntryLine.Enabled = false;
			_radioButtonPartialProfit.Enabled = false;

			//Making over Stop Line Horizontal Locked and turned off
			MakeRayHorizontal(_currentRayContainer.StopRay);
			_currentRayContainer.StopRay.Locked = true;
			//Full update to see the changes what we made
			_currentDynamicTrailingStop = new DynamicTrailingStop(this, _currentRayContainer);
			ChartControl.ChartPanel.Invalidate();
		}

		private void UpdateDts()
		{
			if (_currentDynamicTrailingStop != null)
			{
				_currentDynamicTrailingStop.NewBar();
				if(Low.Count>1&&High.Count>1)
					_currentDynamicTrailingStop.Update(Low[1], High[1]);

			}
			else
				MessageBox.Show("Some Problems with DTS pleas restart it or restart CST");
		}

		#endregion

		public bool GetSelectedRay(out IRay ray)
		{
			ray = null;
			//Instance for over result 
			//Getting Reflection black door open
			Type chartControlType = typeof(ChartControl);
			//Now we want to get access to field 
			FieldInfo fi = chartControlType.GetField("selectedObject", BindingFlags.NonPublic | BindingFlags.Instance);
			//Now if rely got this one 
			if (fi != null)
			{
				//if we free from null error
				if (ChartControl != null && fi.GetValue(ChartControl) != null)
				{
					//Getting the instance of the object
					object clickedObject = fi.GetValue(ChartControl);
					//Checking if we could convert
					var o = clickedObject as IRay;
					if (o != null)
					{
						ray = o;
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
				_myTsButton.BackColor = _disabledColor;
			}
			else
			{
				_myTsButton.Text = "Show CST Control";
				_myTsButton.BackColor = _enabledColor;
			}
		}

		private static void MassageIfLineNotSelected()
		{
			MessageBox.Show("Please select line First");
		}


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

		/// <summary>
		/// The  method that checks can we use the Ray or not
		/// </summary>
		/// <param name="ray"></param>
		/// <returns></returns>
		private bool CanUseRay(out IRay ray)
		{
			if (!GetSelectedRay(out ray))
			{
				//If this ray is not exist
				MessageBox.Show(_pleaseSelectRay);
				return false;
			}
			if (!ray.UserDrawn)
			{
				//If ray was created not by user
				MessageBox.Show("You can't use this ray");
				return false;
			}
			if (_strategyState != StrategyState.NotActive)
			{
				//If strateys are active we can't change the state
				MessageBox.Show("We are active. Please Deactivate first");
				return false;
			}
			return true;
		}

		private void button_MakeHorizontalLine_Click(object sender, EventArgs e)
		{
			IRay ray;
			if (GetSelectedRay(out ray))
			{
				MakeRayHorizontal(ray);
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

		private void MakeRayHorizontal(IRay ray)
		{
			double averagePrice = RayPrice(ray);
			ChartRay rayToUse = ray as ChartRay;
			if (rayToUse != null)
			{
				rayToUse.StartY = averagePrice;
				rayToUse.EndY = averagePrice;
			}
		}

		private bool _firstOrderSet;

		private bool _deActivate;

		private void _buttonActivateClick(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				MessageBox.Show("You don't have lines to trade with");
				return;
			}

			if (_strategyState == StrategyState.NotActive)
			{
				_firstOrderSet = true;

				//Making over buttons now Deactivate Buttons
				_buttonActivate.Text = "DEACTIVATE";
				_buttonActivate.BackColor = _deactivateColor;

				//Set status
				_strategyState = StrategyState.Enter;

				//Make visual  Effects
				_statusLabel.BackColor = _enabledColor;
				_statusLabel.Text = _currentRayContainer.PositionType == MarketPosition.Long
					? "Active: Long Position"
					: "Active: Short Position";
				_deActivate = false;
			}
			//If we got Enter or Exit We should wait till them will be changes to what we want from them
			else
			{
				_deActivate = true;
			}
		}

		private void DeActivation()
		{
			if (_checkBoxOtherCurrency.Checked)
			{
				if (_strategyState == StrategyState.Exit)
				{
					int quantity = Position.Quantity;
					if (Position.MarketPosition == MarketPosition.Long)
						ExitLong(quantity);
					else if (Position.MarketPosition == MarketPosition.Short)
						ExitShort(quantity);
				}
				_deActivate = false;
			}
			else
			{
				//if we want to enter we chancel order
				if (_strategyState == StrategyState.Enter)
				{
					//if it is not cancalled yet we cancel it
					if (_currentOrder.OrderState != OrderState.Cancelled)
						CancelAllOrders(true, false);
					//But in other case we cleaning after our self
					else
					{
						_deActivate = false;
						_currentOrder = null;
					}
				}


				//If we waiting till exit We close position
				else if (_strategyState == StrategyState.Exit)
				{
					if (Position.MarketPosition != MarketPosition.Flat)
					{
						int quantity = Position.Quantity;
						double d = (5*TickSize);
						if (Position.MarketPosition == MarketPosition.Long)
							ExitLongLimit(quantity, _currentPrice - d);
						else if (Position.MarketPosition == MarketPosition.Short)
							ExitShortLimit(quantity, _currentPrice + d);
					}
				}
			}

			//For other currancy it is very easy
			if (_deActivate) return;
			//here we set our position as default
			_strategyState = StrategyState.NotActive;
			//Set our form to not active
			SetVisualToNotActive();
		}

		private void SetVisualToNotActive()
		{
			if (_currentRayContainer != null && _currentRayContainer.IsAlternativeLine)
				_currentRayContainer.RemoveAlternativeRay();

			_buttonActivate.Text = "ACTIVATE";
			_buttonActivate.BackColor = _activateColor;
			_statusLabel.BackColor = _disabledColor;
			_statusLabel.Text = "Not Active";
		}

		private bool _closeHalfNow;

		private void _buttonCloseHalfPositionClick(object sender, EventArgs e)
		{
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				MessageBox.Show("No open position to close half");
				return;
			}
			if (Position.Quantity <= 1)
			{
				MessageBox.Show("We got quantity 1 or less");
				return;
			}
			_closeHalfNow = true;
		}

		private void _buttonClearSelection_Click(object sender, EventArgs e)
		{

			if (_currentRayContainer == null)
			{
				MessageBox.Show("You have nothing to clear");
				return;
			}
			if (_strategyState != StrategyState.NotActive)
			{
				MessageBox.Show("You are trading close or deactivate to clear");
				return;
			}

			if (_currentRayContainer != null)
			{
				_checkBoxEnableTrailStop.Checked = false;
				_checkBoxEnablePartialProfit.Checked = false;
				_radioButtonNone.Checked = true;
				_currentRayContainer.Clear();
				_currentRayContainer = null;
				UpdateForms();
			}
		}

		private void _checkBoxEnablePartialProfit_Changed(object sender, EventArgs e)
		{
			if (_checkBoxEnablePartialProfit.Checked)
			{
				//If we have no lines at all
				if (_currentRayContainer == null)
				{
					MessageBox.Show("Please select ray and Long or Short mode first");
					_checkBoxEnablePartialProfit.Checked = false;
					_checkBoxEnablePartialProfit.Update();
					return;
				}

				//If we are not trading yet but we got very low quantity in quantity  window
				if ((_strategyState == StrategyState.Enter || _strategyState == StrategyState.NotActive) && _numericUpDownQuantity.Value < 2)
				{
					MessageBox.Show("You got quantity 1 or less");
					_checkBoxEnablePartialProfit.Checked = false;
					_checkBoxEnablePartialProfit.Update();
					return;
				}
				//If we are trading and our order quantiy is to low
				if (_strategyState == StrategyState.Exit && _numericUpDownQuantity.Value< 2)
				{
					MessageBox.Show("You got quantity 1 or less");
					_checkBoxEnablePartialProfit.Checked = false;
					_checkBoxEnablePartialProfit.Update();
					return;
				}
				//What we de if everyting is okay
				_partialMsgLabel.Text = "50% TP Enabled";
				_partialMsgLabel.BackColor = _enabledColor;
				// ReSharper disable once UseNullPropagation
				if (_strategyState == StrategyState.Exit)
					_currentRayContainer.PartialProfitEnable(true);
				else
					_currentRayContainer.PartialProfitEnable(false);
			}
			else
			{
				_checkBoxEnablePartialProfitAlert.Checked = false;
				_partialMsgLabel.Text = "50% TP Disabled";
				_partialMsgLabel.BackColor = _disabledColor;
				// ReSharper disable once UseNullPropagation
				if (_currentRayContainer != null)
				{
					if (!_doNoRemoveAltLine)
					{
						_currentRayContainer.ParialProfitDisable();
						_currentRayContainer.RemoveAlternativeRay();
					}
				}
			}
			UpdateForms();
		}

		private void vShowForm_Panel(object s, EventArgs e)
		{
			_mainPanel.Visible = !_mainPanel.Visible;
			SetCstButtonText();
		}

		private void _checkBoxEnableTrailStopChanged(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				if (_checkBoxEnableTrailStop.Checked)
				{
					MessageBox.Show("First you should create the Rays to get Dynamic Trailing Stop");
					_checkBoxEnableTrailStop.Checked = false;
				}
				return;
			}
			if (_checkBoxEnableTrailStop.Checked)
			{
				_dynamicTrailingStopMsgLabel.Text = "DTS";
				_dynamicTrailingStopMsgLabel.BackColor = _enabledColor;
				EnableDts();
				UpdateDts();
			}
			else
			{
				_checkBoxEnableTrailStopAlert.Checked = false;
				DisableDts();
				_dynamicTrailingStopMsgLabel.Text = "DTS NOT";
				_dynamicTrailingStopMsgLabel.BackColor = _disabledColor;
			}
		}

		private void _radioBoxEntryLine(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				MessageBox.Show("Please select Ray line and Long or Short mode first");
				_radioButtonNone.Checked = true;
				return;
			}
			SteMassageEnable();
		}

		private void _radioBoxPartialProfit(object sender, EventArgs e)
		{
			if (_radioButtonPartialProfit.Checked)
			{
				if (_currentRayContainer == null)
				{
					MessageBox.Show("Please select Ray line and Long or Short mode first");
					_radioButtonNone.Checked = true;
					return;
				}
				if (_currentRayContainer.HalfCloseRay == null)
				{
					MessageBox.Show("Please first activate 50% line");
					_radioButtonNone.Checked = true;
					return;
				}
			}
			SteMassageEnable();
		}

		private void SteMassageEnable()
		{
			_stopToEnterMsgLabel.Text = "STE";
			_stopToEnterMsgLabel.BackColor = _enabledColor;
		}

		private void _radioBoxNone(object sender, EventArgs e)
		{
			_stopToEnterMsgLabel.Text = "STE NOT";
			_stopToEnterMsgLabel.BackColor = _disabledColor;
		}

		private void button_ClosePositionClick(object sender, EventArgs e)
		{
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				MessageBox.Show("We have nothing to close");
				return;
			}
			if (MarketPosition.Short == Position.MarketPosition)
				ExitShort();

			if (MarketPosition.Long == Position.MarketPosition)
				ExitLong();
		}

		private void _checkBoxEnableTrailStopAlert_CheckedChanged(object sender, EventArgs e)
		{
			if (!_checkBoxEnableTrailStop.Checked && _checkBoxEnableTrailStopAlert.Checked)
			{
				MessageBox.Show("First you should turn on the DTS");
				_checkBoxEnableTrailStopAlert.Checked = false;
				return;
			}

			_dynamicTrailingStopMsgLabel.Text = _checkBoxEnableTrailStopAlert.Checked ? "DTS + E" : "DTS";
		}


		private void _checkBoxEnablePartialProfitAlert_CheckedChanged(object sender, EventArgs e)
		{
			if (!_checkBoxEnablePartialProfit.Checked && _checkBoxEnablePartialProfitAlert.Checked)
			{
				MessageBox.Show("First you should turn on the Partial Profit ");
				_checkBoxEnablePartialProfitAlert.Checked = false;
				return;
			}
			_partialMsgLabel.Text = _checkBoxEnablePartialProfitAlert.Checked ? "50% TP Enabled + E" : "50% TP Enabled";
			UpdateForms();
		}

		private void _buttonManualMoveStopOnClick(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				MessageBox.Show("First you should create lines to move to them");
				return;
			}
			if (_radioButtonNone.Checked)
			{
				MessageBox.Show("You should select the Partial Profit or Entry Line");
				return;
			}
			if (_radioButtonEntryLine.Checked)
			{
				if (_currentRayContainer.PositionType == MarketPosition.Long)
					MoveStopLineTo(_currentRayContainer.EntryRay, -1);
				if (_currentRayContainer.PositionType == MarketPosition.Short)
					MoveStopLineTo(_currentRayContainer.EntryRay, 1);
			}
			else if (_radioButtonEntryLine.Checked)
			{
				if (!_checkBoxEnablePartialProfit.Checked&&!_currentRayContainer.IsAlternativeLine)
				{
					MessageBox.Show("You should first activate Partial Profit to move to it");
					return;
				}

				IRay ray = GetHulfRay();

				if (_currentRayContainer.PositionType == MarketPosition.Long)
					MoveStopLineTo(ray, -1);

				if (_currentRayContainer.PositionType == MarketPosition.Short)
					MoveStopLineTo(ray, 1);
			}
			UpdateForms();
		}

		private void MoveStopLineTo(IRay ray, int distance)
		{
			_currentRayContainer.StopRay.Anchor1BarsAgo = ray.Anchor1BarsAgo;
			_currentRayContainer.StopRay.Anchor2BarsAgo = ray.Anchor2BarsAgo;
			double rayDistance = RayPriceNotRound(ray) - RayPriceNotRound(_currentRayContainer.StopRay);
			

			_currentRayContainer.StopRay.Anchor1Y += rayDistance + (_currentRayContainer.Distance * distance / 20);
			_currentRayContainer.StopRay.Anchor2Y += rayDistance + (_currentRayContainer.Distance * distance / 20);

			_currentRayContainer.Update();
		}

		public int SwingIndicatorBars
		{
			get
			{
				if (_numericUpDownSwingIndicatorBars != null) return (int)_numericUpDownSwingIndicatorBars.Value;
				return 1;
			}
		}

		public int StopLevel
		{
			get
			{
				if (_numericUpDownStopLevelTicks != null) return (int)_numericUpDownStopLevelTicks.Value;
				return 0;
			}
		}

		public int SwingHorizontal
		{
			get
			{
				if (_numericUpDownHorizontalTicks != null) return (int)_numericUpDownHorizontalTicks.Value;
				return 0;
			}
		}

		private string _otherInstrumentName = "$EURUSD";

		[Description("The other currency that we will use for trading")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayName("Second currency")]
		public string OtherInstrumentName
		{
			get { return _otherInstrumentName; }
			set { _otherInstrumentName = value; }
		}


		[Description("The Email Address to what we will be sending the lists")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayName("E-Mail")]
		public string E_MailAddress
		{
			get { return _mailAddress; }
			set { _mailAddress = value; }
		}

		[Description("The Email login to GMAIL!")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayName(" From E-Mail Login")]
		public string E_MailLogin
		{
			get { return _eMailLogin; }
			set { _eMailLogin = value; }
		}

		[Description("The Email password to GMAIL!")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayName("From E-Mail Password")]
		public string E_MailPassword
		{
			get { return _eMailPassword; }
			set { _eMailPassword = value; }
		}

		#region VS2010 Controls Paste

		private Panel _mainPanel;
		private GroupBox _groupBoxStatusWindow;
		private GroupBox _groupBoxQuantity;
		private NumericUpDown _numericUpDownQuantity;
		private Button _buttonClosePosition;
		private Button _buttonManualShort;
		private Button _buttonManualLong;
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
		private NumericUpDown _numericUpDownStopLevelTicks;
		private NumericUpDown _numericUpDownHorizontalTicks;
		private Label _rr50NameLabel;
		private Label _partialMsgLabel;
		private Label _dynamicTrailingStopMsgLabel;
		private GroupBox _groupBoxStopToEntry;
		private Label _label3;
		private NumericUpDown _numericUpDownPipTicksToActivate;
		private Button _buttonManualMoveStop;
		private GroupBox _groupBox1;
		private GroupBox _groupBoxMail;
		private Label _label4;
		private NumericUpDown _numericUpDownBarEntry;
		private CheckBox _checkBoxEnableBarEntry;
		private CheckBox _checkBoxEnableMailGlobal;
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
		private readonly Color _activateColor = Color.DarkBlue;

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
			_checkBoxOtherCurrency = new CheckBox();
			_textBoxOtherCurrency = new Label();
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
			_groupBoxMail = new GroupBox();
			_label4 = new Label();
			_numericUpDownBarEntry = new NumericUpDown();
			_checkBoxEnableBarEntry = new CheckBox();
			_checkBoxEnableMailGlobal = new CheckBox();
			_groupBoxTrailStop = new GroupBox();
			_label9 = new Label();
			_label8 = new Label();
			_label7 = new Label();
			_numericUpDownSwingIndicatorBars = new NumericUpDown();
			_numericUpDownStopLevelTicks = new NumericUpDown();
			_checkBoxEnableTrailStopAlert = new CheckBox();
			_checkBoxEnableTrailStop = new CheckBox();
			_numericUpDownHorizontalTicks = new NumericUpDown();
			_groupBoxStopToEntry = new GroupBox();
			_label3 = new Label();
			_numericUpDownPipTicksToActivate = new NumericUpDown();
			_buttonManualMoveStop = new Button();
			_groupBoxMode = new GroupBox();
			_buttonMakeHorizontalLine = new Button();
			_buttonClearSelection = new Button();
			_buttonActivate = new Button();
			_groupBoxStatusWindow.SuspendLayout();
			_groupBoxPartialProfit.SuspendLayout();
			_groupBoxQuantity.SuspendLayout();
			((ISupportInitialize)(_numericUpDownQuantity)).BeginInit();
			_mainPanel.SuspendLayout();
			_groupBox1.SuspendLayout();
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
			_checkBoxEnablePartialProfitAlert.CheckedChanged += _checkBoxEnablePartialProfitAlert_CheckedChanged;
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
			_checkBoxEnablePartialProfit.Location = new Point(8, 50);
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
			_numericUpDownQuantity.Maximum = new decimal(new[] { 1000000, 0, 0, 0 });
			_numericUpDownQuantity.Minimum = new decimal(new[] { 1, 0, 0, 0 });
			_numericUpDownQuantity.Name = "numericUpDown_Quantity";
			_numericUpDownQuantity.Size = new Size(79, 20);
			_numericUpDownQuantity.TabIndex = 0;
			_numericUpDownQuantity.TextAlign = HorizontalAlignment.Center;
			_numericUpDownQuantity.Value = new decimal(new[] { 10000, 0, 0, 0 });
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
			// MainPanel
			// 
			_mainPanel.BackColor = SystemColors.Control;
			_mainPanel.Controls.Add(_groupBox1);
			_mainPanel.Controls.Add(_groupBoxMail);
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
			_groupBox1.Location = new Point(6, 722);
			_groupBox1.Margin = new Padding(2);
			_groupBox1.Name = "groupBox1";
			_groupBox1.Padding = new Padding(2);
			_groupBox1.Size = new Size(154, 47);
			_groupBox1.TabIndex = 2;
			_groupBox1.TabStop = false;
			_groupBox1.Text = "Bar Entry";
			// 
			// groupBox0
			//
			// 
			_groupBoxMail.Controls.Add(_checkBoxEnableMailGlobal);
			_groupBoxMail.Location = new Point(6, 780);
			_groupBoxMail.Margin = new Padding(2);
			_groupBoxMail.Name = "groupBox0";
			_groupBoxMail.Padding = new Padding(2);
			_groupBoxMail.Size = new Size(154, 47);
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
			// 			 
			// checkBox_EnableBarEntry
			// 
			_checkBoxEnableMailGlobal.AutoSize = true;
			_checkBoxEnableMailGlobal.Location = new Point(10, 20);
			_checkBoxEnableMailGlobal.Margin = new Padding(2);
			_checkBoxEnableMailGlobal.Name = "checkBox_EnableMailEntry";
			_checkBoxEnableMailGlobal.Size = new Size(59, 17);
			_checkBoxEnableMailGlobal.TabIndex = 5;
			_checkBoxEnableMailGlobal.Text = "Enable";
			_checkBoxEnableMailGlobal.UseVisualStyleBackColor = true;
			_checkBoxEnableMailGlobal.Checked = true;
			// 
			// groupBox_TrailStop
			// 
			_groupBoxTrailStop.BackColor = SystemColors.ControlLight;
			_groupBoxTrailStop.Controls.Add(_label9);
			_groupBoxTrailStop.Controls.Add(_label8);
			_groupBoxTrailStop.Controls.Add(_label7);
			_groupBoxTrailStop.Controls.Add(_numericUpDownSwingIndicatorBars);
			_groupBoxTrailStop.Controls.Add(_numericUpDownStopLevelTicks);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStopAlert);
			_groupBoxTrailStop.Controls.Add(_checkBoxEnableTrailStop);
			_groupBoxTrailStop.Controls.Add(_numericUpDownHorizontalTicks);
			_groupBoxTrailStop.Location = new Point(6, 597);
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
			_numericUpDownSwingIndicatorBars.Maximum = new decimal(new[]
			{
				99,
				0,
				0,
				0
			});
			_numericUpDownSwingIndicatorBars.Name = "numericUpDown_SwingIndicatorBars";
			_numericUpDownSwingIndicatorBars.Size = new Size(34, 20);
			_numericUpDownSwingIndicatorBars.TabIndex = 11;
			_numericUpDownSwingIndicatorBars.TextAlign = HorizontalAlignment.Center;
			_numericUpDownSwingIndicatorBars.Value = new decimal(new[]
			{
				4,
				0,
				0,
				0
			});
			// 
			// numericUpDown_HorizonCrossTicks
			// 
			_numericUpDownStopLevelTicks.Location = new Point(118, 78);
			_numericUpDownStopLevelTicks.Margin = new Padding(2);
			_numericUpDownStopLevelTicks.Maximum = new decimal(new[]
			{
				99,
				0,
				0,
				0
			});
			_numericUpDownStopLevelTicks.Name = "numericUpDown_HorizCrossTicks";
			_numericUpDownStopLevelTicks.Size = new Size(34, 20);
			_numericUpDownStopLevelTicks.TabIndex = 12;
			_numericUpDownStopLevelTicks.TextAlign = HorizontalAlignment.Center;
			_numericUpDownStopLevelTicks.Value = new decimal(new[]
			{
				4,
				0,
				0,
				0
			});
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
			_checkBoxEnableTrailStopAlert.CheckedChanged += _checkBoxEnableTrailStopAlert_CheckedChanged;
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
			_checkBoxEnableTrailStop.CheckedChanged += _checkBoxEnableTrailStopChanged;
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
			_numericUpDownHorizontalTicks.Location = new Point(118, 99);
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
			_radioButtonNone.Location = new Point(5, 74);
			_radioButtonNone.Name = "radioButtonNone";
			_radioButtonNone.Size = new Size(51, 17);
			_radioButtonNone.TabIndex = 0;
			_radioButtonNone.TabStop = true;
			_radioButtonNone.Text = "None";
			_radioButtonNone.UseVisualStyleBackColor = true;
			_radioButtonNone.CheckedChanged += _radioBoxNone;
			_radioButtonNone.Checked = true;

			_radioButtonEntryLine.AutoSize = true;
			_radioButtonEntryLine.Location = new Point(5, 92);
			_radioButtonEntryLine.Name = "radioButtonEntryLine";
			_radioButtonEntryLine.Size = new Size(72, 17);
			_radioButtonEntryLine.TabIndex = 1;
			_radioButtonEntryLine.TabStop = true;
			_radioButtonEntryLine.Text = "Entry Line";
			_radioButtonEntryLine.UseVisualStyleBackColor = true;
			_radioButtonEntryLine.CheckedChanged += _radioBoxEntryLine;

			_radioButtonPartialProfit.AutoSize = true;
			_radioButtonPartialProfit.Location = new Point(5, 110);
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
			_groupBoxStopToEntry.Controls.Add(_label3);
			_groupBoxStopToEntry.Controls.Add(_numericUpDownPipTicksToActivate);
			_groupBoxStopToEntry.Controls.Add(_buttonManualMoveStop);
			_groupBoxStopToEntry.Location = new Point(6, 466);
			_groupBoxStopToEntry.Margin = new Padding(2);
			_groupBoxStopToEntry.Name = "groupBox_StopToEntry";
			_groupBoxStopToEntry.Padding = new Padding(2);
			_groupBoxStopToEntry.Size = new Size(154, 127);
			_groupBoxStopToEntry.TabIndex = 2;
			_groupBoxStopToEntry.TabStop = false;
			_groupBoxStopToEntry.Text = "Stop to Entry/Partial Profit";
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
			_label3.Text = "Pips to activate";
			_label3.TextAlign = ContentAlignment.MiddleLeft;
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
			_checkBoxOtherCurrency.CheckedChanged += CheckBoxOtherCurrencyOnCheckedChanged;
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
			_groupBox1.ResumeLayout(false);
			_groupBox1.PerformLayout();
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
			ChartControl.Controls.Add(_mainPanel);
		}

		private void CheckBoxOtherCurrencyOnCheckedChanged(object sender, EventArgs eventArgs)
		{
			_textBoxOtherCurrency.Enabled = _checkBoxOtherCurrency.Checked;
		}

		private Button _buttonActivate;
		private readonly Color _deactivateColor = Color.OrangeRed;
		private DynamicTrailingStop _currentDynamicTrailingStop;
		private int _millisecondsTimeout = 15000;
		private double _profitLoss;
		private double _profitPercent;

//		private string _mailAddress = "daniel@danielwardzynski.com";
//		private string _eMailLogin = "chartslopetrader";
//		private string _eMailPassword = "123qwe456rty";
		private string _mailAddress = "comman.games@outlook.com";
		private string _eMailLogin = "alfaa.gen";
		private string _eMailPassword = "Train@concentration";
		private bool _doNoRemoveAltLine;
		public double ProportionalDistance { get;  set; }
		

		#endregion

		public double RayPriceNotRound(IRay ray)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y) / (ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);
			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance * ray.Anchor2BarsAgo) + ray.Anchor2Y;
			return rayPrice;
		}
		public double RayPrice(IRay ray)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y) / (ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);
			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance * ray.Anchor2BarsAgo) + ray.Anchor2Y;
			return Instrument.MasterInstrument.Round2TickSize(rayPrice);
		}

		private string SendingText { get; set; }
		private string SendingTopic { get;  set; }
		


		
		private void SendMail(string topic, string text)
		{
			if (_checkBoxEnableMailGlobal.Checked)
			{
				SendingText = text;
				SendingTopic = "CST Alert: " + topic;
				Thread mailSendingThread = new Thread(MailSender);
				mailSendingThread.Start();
			}
		}

		private void MailSender()
		{
			try
			{
				//Getting and converting the image
				Bitmap chartPicture = GetChartPicture();
				Bitmap cstPanelPicture = GetCSTPanelPicture();

				int width = chartPicture.Width + cstPanelPicture.Width;
				int height = chartPicture.Height;
				var bitmap = new Bitmap(width, height);
					using (var canvas = Graphics.FromImage(bitmap))
					{
						canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
						canvas.DrawImage(chartPicture, new Rectangle(0, 0, width, height), new Rectangle(0, 0, width, chartPicture.Height + 1), GraphicsUnit.Pixel);
						canvas.DrawImage(cstPanelPicture, chartPicture.Width, 0);
						canvas.Save();
					}
				//Writing picture to the stream
				Stream stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Jpeg);
				stream.Position = 0;
				chartPicture.Dispose();
				cstPanelPicture.Dispose();


				//Creating HTML
				LinkedResource inline = new LinkedResource(stream, MediaTypeNames.Image.Jpeg);
				inline.ContentId = Guid.NewGuid().ToString();

				string htmlBody = "<html><body><br><img src=\"cid:filename\">" + SendingText +
				                  String.Format(@"<img src=""cid:{0}"" />", inline.ContentId) +
				                  "</body></html>";

				AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
				avHtml.LinkedResources.Add(inline);


				//Creating file and add to it text
				MailMessage mail = new MailMessage();
				mail.AlternateViews.Add(avHtml);

				//Add the file to the E-Mail
				mail.IsBodyHtml = true;


				//Transaction settings 
				SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
				mail.Subject = SendingTopic;
				mail.From = new MailAddress(_eMailLogin + "@gmail.com");
				mail.To.Add(_mailAddress);
				SmtpServer.Port = 587;
				SmtpServer.Credentials = new System.Net.NetworkCredential(_eMailLogin, _eMailPassword);
				SmtpServer.EnableSsl = true;
				Thread.Sleep(_millisecondsTimeout);
				SmtpServer.Send(mail);

				//Cleaning after our self 
				bitmap.Dispose();
				stream.Position = 0;
				stream.Dispose();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private bool IsUpLefthite(Bitmap chartPicture, int weight, int height, int i)
		{
			for (int j = weight-1; j > weight-i; j -=2)
			{
				for (int k = height-1; k > height-i; k-=2)
				{
					if (chartPicture.GetPixel(j, k) != Color.White)
					{
						return false;
					}
				}

			}
			return true;

		}

		private string TextFormater(string action, bool isEntry, bool isPartialProfit)
		{
			StringBuilder textResult = new StringBuilder();
			string time = DateTime.Now.ToString("HH:mm"); ;
			string symbol = Position.Instrument.FullName;

			//Now Adding formated text

			textResult.AppendFormat("<p>Action:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", action);
			textResult.AppendFormat("<p>Time:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", time);

			textResult.AppendFormat("<p>Symbol:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", symbol);

			if (isPartialProfit)
				textResult.AppendFormat("<p>Quantity:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", _numericUpDownQuantity.Value/2);
			else
				textResult.AppendFormat("<p>Quantity:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", _numericUpDownQuantity.Value);

			textResult.AppendFormat("<p>Position:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", _currentRayContainer.PositionType);

			if(!isEntry&&!isPartialProfit)
				textResult.AppendFormat(
					"<p>Profit:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}&nbsp;USD &nbsp;AND&nbsp;{1}&nbsp;%</p>",
					_profitLoss, _profitPercent*100);
			if(isPartialProfit)
				textResult.AppendFormat( "<p>Profit:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}&nbsp;USD &nbsp;AND&nbsp;{1}&nbsp;%</p>", _profitLoss*0.5, _profitPercent*50); 

			return textResult.ToString();
		}





		private Bitmap GetChartPicture()
		{
			Bitmap bmp;
			 bmp = new Bitmap(ChartControl.ChartPanel.Width, ChartControl.ChartPanel.Height, PixelFormat.Format16bppRgb555);
			int counter = 0;
			do
			{
				Thread.Sleep(500);
				ChartControl.ChartPanel.DrawToBitmap(bmp, ChartControl.ChartPanel.ClientRectangle);
				counter++;
				if (counter > 20)
					break;
			} while (IsUpLefthite(bmp,bmp.Width,bmp.Height,30));

			return bmp;
		}
		private Bitmap GetCSTPanelPicture()
		{
			Bitmap bmp = new Bitmap(_mainPanel.Width, _mainPanel.Height, PixelFormat.Format16bppRgb555);
			_mainPanel.DrawToBitmap(bmp, _mainPanel.ClientRectangle);
			return bmp;
		}
	}

	//Let's take a look how it works

	internal class RayContainer
	{
		#region Variables

		private static readonly Color EnterColor = Color.DarkRed;
		private static readonly Color TpColor = Color.Lime;
		private static readonly Color StopColor = Color.Red;
		private static readonly Color HcColor = Color.Purple;
		private static readonly Color AColor = Color.FromArgb(141, 0, 229);

		public MarketPosition PositionType;
		public static int TicksTarget = 5;

		public IRay OriginRay;
		public IRay EntryRay;
		public IRay StopRay;
		public IRay HalfCloseRay;
		public IRay ProfitTargetRay;
		public IRay AlternativeRay;

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

		public readonly int TextShift = 10;
		public bool ClosingHalf;
		private readonly ChartSlopeTrader _strategy;
		public double Distance;
		public bool IsAlternativeLine { get; private set; }
		#endregion
		public RayContainer(MarketPosition marketPosition, IRay ray, ChartSlopeTrader strategy, bool isCloseHalf, bool isDts)
		{
			//Initialization global variables
			_strategy = strategy;
			PositionType = marketPosition;
			Distance = strategy.ProportionalDistance; 
			IsAlternativeLine = false;

			//Set some local variables
			double distance = Distance;
			//Setting  up down if we are in the long
			if (marketPosition == MarketPosition.Long)
				distance *= -1;

			//Crating the lines
			OriginRay = ray;

			EntryRay = strategy.DrawRay("Enter", false,
				ray.Anchor1BarsAgo, ray.Anchor1Y + (distance * .3), ray.Anchor2BarsAgo, ray.Anchor2Y + (distance * .3),
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
				PartialProfitEnable(false);
			}
			//Unlocking those rays if want to make them clear
			EntryRay.Locked = false;
			StopRay.Locked = false;
			ProfitTargetRay.Locked = false;
		}


		private void MadeStopRayHorizontal()
		{
			double position = _strategy.RayPrice(StopRay);
			StopRay.Anchor1Y = position;
			StopRay.Anchor2Y = position;
		}

		public void Update()
		{
			double d10 = Distance / 100;
			//For enter Ray
			_eDot = _strategy.DrawDot("enterDot", true, 0, RealRayPrice(EntryRay), _dotColor);
			double s = RealRayPrice(EntryRay);
			_eText = _strategy.DrawText("enterText", TextForma(s), 0, RealRayPrice(EntryRay) + d10, _textColor);

			//For stop Ray
			_sDot = _strategy.DrawDot("stopDot", true, 0, RealRayPrice(StopRay), _dotColor);
			double text = RealRayPrice(StopRay);
			_sText = _strategy.DrawText("stopText", TextForma(text), 0, RealRayPrice(StopRay) + d10, _textColor);

			//For TP Ray
			_tpDot = _strategy.DrawDot("TPDot", true, 0, RealRayPrice(ProfitTargetRay), _dotColor);
			double priceText = RealRayPrice(ProfitTargetRay);
			_tpText = _strategy.DrawText("TPText", TextForma(priceText), 0, RealRayPrice(ProfitTargetRay) + d10, _textColor);


			//Check if have turn on the Closing Half and is The ray exist because without ray we will got the error
			if (ClosingHalf && HalfCloseRay != null)
			{
				double priceT = RealRayPrice(HalfCloseRay);
				_hcDot = _strategy.DrawDot("HCDot", true, 0, priceT, _dotColor);

				_hcText = _strategy.DrawText("HCText", TextForma(priceT), 0, RealRayPrice(HalfCloseRay) + d10, _textColor);
			}
		}

		private string TextForma(double price)
		{
			string priceText =
				_strategy.Instrument.MasterInstrument.Round2TickSize(price).ToString(CultureInfo.InvariantCulture) + "\n";
			return new string(' ', priceText.Length + TextShift) + priceText;
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

			ClearHalfLine();
		}

		public void ClearHalfLine()
		{
			//Removing half line 
			if (HalfCloseRay != null) _strategy.RemoveDrawObject(HalfCloseRay);
			if (_hcText != null) _strategy.RemoveDrawObject(_hcText);
			if (_hcDot != null) _strategy.RemoveDrawObject(_hcDot);
			ClosingHalf = false;
		}

		public void PartialProfitEnable(bool exit)
		{
			if (IsAlternativeLine)
			{
				_strategy.RemoveDrawObject(AlternativeRay);
				IsAlternativeLine = false;
			}
			ClosingHalf = true;
			//Distance we will count from profit Ray
			double distance = (_tpDot.Y - _strategy._currentPrice) / 2;
			if (!exit)
				distance = (_tpDot.Y - _eDot.Y) / 2;
			//Drawing the ray
			HalfCloseRay = _strategy.DrawRay("HalfClose", false,
				(ProfitTargetRay.Anchor1BarsAgo),
				(ProfitTargetRay.Anchor1Y - distance),
				(ProfitTargetRay.Anchor2BarsAgo),
				(ProfitTargetRay.Anchor2Y - distance),
				HcColor, DashStyle.Dash, 2);

			Update();
			HalfCloseRay.Locked = false;
		}

		public void ParialProfitDisable()
		{
			CreateAlternativeLine();
			ClosingHalf = false;
			//Remove the ray if we got it already
			ClearHalfLine();
		}

		private void CreateAlternativeLine()
		{
			IsAlternativeLine = true;
			AlternativeRay = _strategy.DrawRay("Alternative Ray", true, HalfCloseRay.Anchor1BarsAgo, HalfCloseRay.Anchor1Y,
				HalfCloseRay.Anchor2BarsAgo, HalfCloseRay.Anchor2Y, AColor, DashStyle.Solid, 2);
			AlternativeRay.Locked = true;
		}



		public double RealRayPrice(IRay ray)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y) / (ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);
			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance * ray.Anchor2BarsAgo) + ray.Anchor2Y;
			return rayPrice;
		}

		public void RemoveAlternativeRay()
		{
			if (IsAlternativeLine)
			{
				_strategy.RemoveDrawObject(AlternativeRay);
				IsAlternativeLine = false;
			}
		}
	}

	internal class DynamicTrailingStop
	{
		public struct Slope
		{
			public int Bar;
			public double Price;

			public Slope(int bar, double price)
			{
				Bar = bar;
				Price = price;
			}
		}

		private static readonly Color SlopeLineColor = Color.Yellow;
		private readonly ChartSlopeTrader _strategy;
		private readonly RayContainer _rayContainer;
		public MarketPosition PositonType;
		private bool _isWaitSlope;
		private Slope _lastSlope;
		private Slope _currentSlope;
		private IRay _ray;
		private IDot _dot;
		private IText _text;

		public DynamicTrailingStop(ChartSlopeTrader strategy, RayContainer rayContainer)
		{
			_strategy = strategy;
			_rayContainer = rayContainer;
			PositonType = rayContainer.PositionType;
			_currentSlope = GetLastSlope(0, 255);
			if (Math.Abs(_currentSlope.Price) > 0.0001)
			{
				UpdateSlopeLine(_currentSlope);
				SetStopRay(rayContainer.PositionType == MarketPosition.Long
					? LocalMin(0, _currentSlope.Bar)
					: LocalMax(0, _currentSlope.Bar));
				if(strategy.High.Count>1&&strategy.Low.Count>1)
				Update(_strategy.Low[1], _strategy.High[1]);
			}
			else
			{
				MessageBox.Show("First Slope with such configuration cannot be find\n Reset configuration and try again");
			}
		}

		~DynamicTrailingStop()
		{
			if (_ray != null)
			{
				_strategy.RemoveDrawObject(_ray);
				_strategy.RemoveDrawObject(_dot);
				_strategy.RemoveDrawObject(_text);
				_strategy.ChartControl.ChartPanel.Invalidate();
			}

		}

		private void SetStopRay(double price)
		{
			_rayContainer.StopRay.Anchor1Y = price;
			_rayContainer.StopRay.Anchor2Y = price;
			_rayContainer.StopRay.Anchor1BarsAgo = _currentSlope.Bar;
			_rayContainer.StopRay.Anchor2BarsAgo = _currentSlope.Bar - 1;

		}

		private string TextForma(double price)
		{
			string priceText = price.ToString(CultureInfo.InvariantCulture) + "\n";
			return new string(' ', priceText.Length + _rayContainer.TextShift) + priceText;
		}

		public void UpdateSlopeLine(Slope slope)
		{
			double price = _strategy.Instrument.MasterInstrument.Round2TickSize(slope.Price);
			_ray = _strategy.DrawRay("Slope", false, slope.Bar, price, slope.Bar - 1, price, SlopeLineColor, DashStyle.Solid, 2);
			_dot = _strategy.DrawDot("slopeDot", false, 0, price, Color.Black);
			_text = _strategy.DrawText("HCText", TextForma(price), 0, price, Color.Black);
		}

		public Slope GetLastSlope(int from, int till)
		{
			int swing = _strategy.SwingIndicatorBars;
			till = Math.Min(till, _strategy.Low.Count);
			if (PositonType == MarketPosition.Long)
				till = Math.Min(till, _strategy.High.Count);
			Slope result = new Slope(0, 0);
			for (int i = from + swing; i < till; i++)
			{
				int count = 0;
				for (int j = 0; j < swing; j++)
				{
					if (PositonType == MarketPosition.Short && _strategy.Low[i] < _strategy.Low[i - j])
						count++;

					else if (PositonType == MarketPosition.Long && _strategy.High[i] > _strategy.High[i - j])
						count++;

				}
				if (count == swing - 1)
				{
					int barsAgo = i;

					if (PositonType == MarketPosition.Short)
						result = new Slope(barsAgo, _strategy.Low[barsAgo]);
					else if (PositonType == MarketPosition.Long)
						result = new Slope(barsAgo, _strategy.High[barsAgo]);
					//No need to search forward
					break;
				}
			}
			//Now we find is it the most small or big local to do not look stupid
			while (result.Bar + 1 < till)
			{
				if (PositonType == MarketPosition.Long)
				{
					if (_strategy.High[result.Bar + 1] >= _strategy.High[result.Bar])
						result = new Slope(result.Bar + 1, _strategy.High[result.Bar + 1]);
					else
						break;
				}
				else
				{
					if (_strategy.Low[result.Bar + 1] <= _strategy.Low[result.Bar])
						result = new Slope(result.Bar + 1, _strategy.Low[result.Bar + 1]);
					else
						break;
				}

			}

			return result;
		}

		public double LocalMin(int start, int end)
		{
			double result = _strategy.Low[start];
			for (int i = start; i <= end; i++)
				result = Math.Min(_strategy.Low[i], result);
			return result;
		}

		public double LocalMax(int start, int end)
		{
			double result = _strategy.High[start];
			for (int i = start; i <= end; i++)
				result = Math.Max(_strategy.High[i], result);
			return result;
		}

		public void NewBar()
		{
			_currentSlope.Bar++;
			_lastSlope.Bar++;
		}

		public void Update(double low, double high)
		{
			//If we do not wait for new Slow
			if (!_isWaitSlope)
			{
				if (PositonType == MarketPosition.Long && high > _currentSlope.Price)
				{
					_isWaitSlope = true;
					_lastSlope = _currentSlope;
					SetStopRay(LocalMin(0, _lastSlope.Bar));
				}
				else if (PositonType == MarketPosition.Short && low < _currentSlope.Price)
				{
					_isWaitSlope = true;
					_lastSlope = _currentSlope;
					SetStopRay(LocalMax(0, _lastSlope.Bar));
				}
			}
			else
			{
				int swing = _strategy.SwingIndicatorBars;
				Slope tempSlope = GetLastSlope(0, _lastSlope.Bar - swing);
				if (Math.Abs(tempSlope.Price) > 0.00001 && Math.Abs(tempSlope.Price - _lastSlope.Price) > 0.00001)
				{
					//That is the moment when we get new  
					_currentSlope = tempSlope;
					UpdateSlopeLine(_currentSlope);
					_isWaitSlope = false;

				}
			}

		}
	}
}

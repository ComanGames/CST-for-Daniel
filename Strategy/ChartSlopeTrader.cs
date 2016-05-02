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
    //Hello Yura I need those lines to be removed 
	/// <summary>
	/// Example of the Chart Slope Trade
	/// </summary>
	[Description("Trade Slope Lines")]
	public partial class ChartSlopeTrader : Strategy
	{
        //What it's mean?
		private enum StrategyState:byte
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

		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private string _pleaseSelectRay = "Please Select Ray";
		private RayContainer _currentRayContainer;
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private IOrder _currentOrder;

		private StrategyState _strategyState = StrategyState.NotActive;
		private DynamicTrailingStop _currentDynamicTrailingStop;
		private int _millisecondsTimeout = 15000;
		private double _profitLoss;
		private double _profitPercent;

		private bool _doNoRemoveAltLine;
		private int EntryLineTouches = 0;
		private double _ohterProfitLoss;
		private double _otherProfitPercent;
		private bool _wasPrtialProfit;
		private bool _canUseOtherInstrument;
		private bool _firstOrderSet;
		private bool _deActivate;
		private int _realQuantity;

		#endregion

		#region Mail Settings

//		private string _mailAddress = "daniel@danielwardzynski.com";
//		private string _eMailLogin = "chartslopetrader";
//		private string _eMailPassword = "123qwe456rty";
		private string _mailAddress = "comman.games@outlook.com";
		private string _eMailLogin = "alfaa.gen";
		private string _eMailPassword = "Train@concentration";
		#endregion

		#region AuroProportys

		public double ProportionalDistance { get; set; }

		private string SendingText { get; set; }

		private string SendingTopic { get; set; }

		public double _currentPrice { get; set; }
		#endregion

		#region proportys

		public int SwingIndicatorBars
		{
			get
			{
				if (_numericUpDownSwingIndicatorBars != null) return (int)_numericUpDownSwingIndicatorBars.Value;
				return 1;
			}
		}

		public double StopLevel
		{
			get
			{
				if (_numericUpDownStopLevelTicks != null) return (double)_numericUpDownStopLevelTicks.Value * RealTickSize;
				return 0;
			}
		}

		public double SwingHorizontal
		{
			get
			{
				if (_numericUpDownHorizontalTicks != null) return (double)_numericUpDownHorizontalTicks.Value * RealTickSize;
				return 0;
			}
		}


		#endregion

		private string _otherInstrumentName = "$EURUSD";
		#region Initialization & Uninitialization

		protected override void Initialize()
		{

			try
			{
				BarsRequired = 0;//To test even if we got 1 bar on our chart
				//Setting not quit when we have some problem
			RealtimeErrorHandling = RealtimeErrorHandling.TakeNoAction;
				MyInstrument = Instrument.MasterInstrument;

//			  AddOtherCurrency();
				CalculateOnBarClose = false;
				Enabled = true;
				_currentOrder = null;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + e.Source + e.Data);
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
				ChartControl.Disposed += ChartControlOnDisposed;
				ChartControl.ChartPanel.MouseUp += MouseUpAction;
				VS2010_InitializeComponent_Form(ChartControl.Controls);

				// Add Toolbar Button
				ButtonToThetop();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private void ChartControlOnDisposed(object sender, EventArgs eventArgs)
		{
			OnTermination();
			this.Disable();
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
					_realQuantity = (int)_numericUpDownQuantity.Value;
					if (_checkBoxEnableShortLongAlert.Checked)
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
				MessageBox.Show(e.Message + e.Source + e.Data);
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
						SendMailProfitLine();
					else
						SendMail_dtsLineSL();
				}
				else
				{
					if (_checkBoxEnableShortLongAlert.Checked)
					{
						if (distanceToProfit <= distanceToStop)
							SendMailProfitLine();
						else
							SendMailStopLine();
					}
				}
			}
		}

		private void SendMail_dtsLineSL()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("DTS {0} Stope line triggered at @{1}", positionType, _currentPrice);
			string from = "Above";
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				from = "Below";
			string formater = TextFormater(String.Format("Cross {0} SL line @{1}",from , _currentPrice), false, false);
			SendMail(topic, formater);
		}
		internal void SendMail_dtsStopLineMoved()
		{

			if (_checkBoxEnableTrailStopAlert.Checked)
			{
				string positionType = _currentRayContainer.PositionType.ToString();
				double rayPrice = RayPrice(_currentRayContainer.StopRay);

				string topic = String.Format("DTS {0} Stop line moved to@{1}", positionType, rayPrice);
				string formater = TextFormater("DTS moved STOP Line to @" + rayPrice, true, false);
				SendMail(topic, formater);

			}
		}
		internal void SendMail_dtsNewSlopeLineNew(double slopePrice)
		{
			if (_checkBoxEnableTrailStopAlert.Checked)
			{
				string positionType = _currentRayContainer.PositionType.ToString();
				string topic = String.Format("DTS {0} moved yellow line to @{1}", positionType, slopePrice);
				string formater = TextFormater("DTS moved INDICATOR Line to @" + slopePrice, true, false);
				SendMail(topic, formater);
			}
		}

		private void SendMailPartialProfitLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("50% Partial {0} TP line triggered @{1}", positionType, _currentPrice);
			//Below or above
			string from = "Below";
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				from = "Above";
			string formater = TextFormater(String.Format("Cross {0} 50% line @{1}", from, _currentPrice), false, true);
			SendMail(topic, formater);
		}
		private void SendMailProfitLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("TP {0} line triggered TP@{1} ", positionType, _currentPrice);
			//Below or above
			string from = "Below";
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				from = "Above";
			string formater = TextFormater(String.Format("Cross {0} TP line @{1}",from, _currentPrice), false, false);
			SendMail(topic, formater);
		}
		private void SendMailStopLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("SL {0} line triggered SL@{1}", positionType, _currentPrice);
			//Below or above
			string from = "Above";
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				from = "Below";
			string formater = TextFormater(String.Format("Cross {0} SL line @{1}",from , _currentPrice), false, false);
			SendMail(topic, formater);
		}

		private void SendMailEntryLine()
		{
			string positionType = _currentRayContainer.PositionType.ToString();
			string topic = String.Format("Entry {0} line triggered @{1}", positionType, _currentPrice);
			//Below orabove
			string from = "Above";
			if (_currentRayContainer.PositionType == MarketPosition.Short)
				from = "Below";
			string formater = TextFormater(String.Format("Cross {0} Entry line @{1}",from , _currentPrice), true, false);
			SendMail(topic, formater);
		}

		private bool weGotError = false;
		protected override void OnOrderUpdate(IOrder order)
		{
			if(order.OrderState == OrderState.Rejected && order.Error != ErrorCode.NoError)
				{ 
				weGotError = true;
				_deActivate = true;
				MessageBox.Show(" From Yura:You put wrong order probably quantity is two big or you place it in the wrong place. Next will be from NT with more Details. Do not wory with strategy all ok but this order can not be accept");
				return;
			 }
		}
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
					SettingVariablesAndGraphics();

					if (_currentRayContainer != null)
					{
						if (!_checkBoxOtherCurrency.Checked)
							ThisInstrumentOrders();
						UpdateFeatures();
					}
					//Trading on other instrument
				}
				else if (_currentRayContainer != null && _canUseOtherInstrument && _checkBoxOtherCurrency.Checked && BarsInProgress == 1)
					OtherInstrumentOrders(); // We are only making the lines trigger on other order and that all
			}
			catch (Exception e)
			{
				if (firstException)
				{
					firstException = false;
					MessageBox.Show("Hello it is massage from Yura sand the screen with those code to Yura to be fixed:" + e.Source + e.Data + e.StackTrace);
				}
			}
		}

		private bool firstException = true;
		private void UpdateFeatures()
		{
			UpdateRr();
			if (!_radioButtonNone.Checked)
				StopToEntryOrParialProfit();
			if (_checkBoxEnableBarEntry.Checked)
				UpdateEntryLineTouches();
			if (_currentDynamicTrailingStop != null)
				UpdateDts();
		}

		private bool _isTouching;
		private int _realBarTocuhCount;

		private void UpdateEntryLineTouches()
		{
			if (_checkBoxEnableConsecutive.Checked)
				BarEntryConsecutive();
			else
				BarEntryNonConsecutive();
		}

		private void BarEntryNonConsecutive()
		{
			if (EntryLineTouches > 1)
			{
				if (FirstTickOfBar)
				{
					double entryLinePrice = RealRayPrice(_currentRayContainer.EntryRay,1);
					double priceLow = Low[1];
					double priceHigh = High[1];

					if (entryLinePrice > priceLow&&entryLinePrice < priceHigh)
						EntryLineNumeriUpdate();
				}
			}
			else
			{
				double entryLinePrice = RayPrice(_currentRayContainer.EntryRay);

				if (_currentRayContainer.PositionType == MarketPosition.Long &&_currentPrice<=entryLinePrice
					&&Math.Abs(_currentPrice-entryLinePrice)<2*RealTickSize)
					EntryLineNumeriUpdate();
				else if (_currentRayContainer.PositionType == MarketPosition.Short&&_currentPrice>=entryLinePrice
					&&Math.Abs(_currentPrice-entryLinePrice)<2*RealTickSize)
					EntryLineNumeriUpdate();
			}

		}

		private void BarEntryConsecutive()
		{
			if (EntryLineTouches > 1)
			{
				if (FirstTickOfBar)
				{
					double entryLinePrice = RealRayPrice(_currentRayContainer.EntryRay,1);
					double priceLow = Low[1];
					double priceHigh = High[1];

					if (entryLinePrice >= priceLow && entryLinePrice <= priceHigh)
						EntryLineNumeriUpdate();
					else
						ReSetLineTouches();
				}
			}
			else
			{

				if (FirstTickOfBar)
				{
					double entryLinePricePrev = RealRayPrice(_currentRayContainer.EntryRay,1);
					double priceLow = Low[1];
					double priceHigh = High[1];

					if (entryLinePricePrev < priceLow || entryLinePricePrev > priceHigh)
						ReSetLineTouches();
				}
				double entryLinePrice = RayPrice(_currentRayContainer.EntryRay);
				if (_currentRayContainer.PositionType == MarketPosition.Long &&_currentPrice<=entryLinePrice&&_currentPrice-entryLinePrice>=(-RealTickSize))
					EntryLineNumeriUpdate();
				else if (_currentRayContainer.PositionType == MarketPosition.Short&&_currentPrice>=entryLinePrice&&_currentPrice-entryLinePrice<=RealTickSize)
					EntryLineNumeriUpdate();
			}

		}

		private void ReSetLineTouches()
		{
			EntryLineTouches= _realBarTocuhCount;
			_numericUpDownBarEntry.Value = EntryLineTouches;
			_mainPanel.Invalidate();
			_numericUpDownBarEntry.Update();
		}

		private void EntryLineNumeriUpdate()
		{
			EntryLineTouches--;
			_numericUpDownBarEntry.Value = EntryLineTouches;
			_numericUpDownBarEntry.Update();
			_mainPanel.Invalidate();
			if (EntryLineTouches <= 0)
			{
				_checkBoxEnableBarEntry.Checked = false;
				if (_currentRayContainer.PositionType == MarketPosition.Long)
					EnterLong((int)_numericUpDownQuantity.Value);
				else if (_currentRayContainer.PositionType == MarketPosition.Short)
					EnterShort((int)_numericUpDownQuantity.Value);
			}
		}

		private double GetDistance()
		{
			if (Close.Count > 3 && High != null && High.Count > 3)
				try
				{
					ProportionalDistance = GetMaxPrice() - GetMinPrice();
				}
				catch (Exception e)
				{
					ProportionalDistance = 5 * RealTickSize;
				}
			else
				ProportionalDistance = 5 * RealTickSize;
			return ProportionalDistance;
		}

		private double GetMaxPrice()
		{
			int length = Math.Min(8, High.Count - 1);
			double result = High[0];
			for (int i = 0; i < length; i++)
				result = Math.Max(result, High[i]);
			return result;
		}

		private double GetMinPrice()
		{
			int length = Math.Min(8, Low.Count - 1);
			double result = Low[0];
			for (int i = 0; i < length; i++)
				result = Math.Min(result, Low[i]);

			return result;
		}
		private void SettingVariablesAndGraphics()
		{

			if (Close.Count != 0)
				_currentPrice = Close[0];
			//Updating the profit
			if (Position.MarketPosition != MarketPosition.Flat)
			{
				_profitLoss = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Currency), 4);
				_profitPercent = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Percent), 4);
			}
			RealTickSize = TickSize;
			if (FirstTickOfBar)
			{
				ProportionalDistance = GetDistance();
				UpdateGraphics();
				MyInstrument = Instrument.MasterInstrument;
			}
		}

		public double RealTickSize = 0.0001;


		private void OtherInstrumentOrders()
		{
			OtherCurrencyUpdateProfitValue();
			if (_strategyState == StrategyState.Enter)
				OtherInstrumentEntryOrder();
			else if (_strategyState == StrategyState.Exit)
				OtherInstrumentExitOrders();

		}
		private void OtherCurrencyUpdateProfitValue()
		{
			_ohterProfitLoss = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Currency), 4);
			_otherProfitPercent = Math.Round(Position.GetProfitLoss(_currentPrice, PerformanceUnit.Percent), 4);
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
			if (_strategyState == StrategyState.Enter && !_checkBoxEnableBarEntry.Checked)
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
			double d = (5 * RealTickSize);
			if (Position.MarketPosition == MarketPosition.Long)
				ExitLong(quantity);
			else if (Position.MarketPosition == MarketPosition.Short)
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
			double d = (5 * RealTickSize);
			if (Position.MarketPosition == MarketPosition.Long)
				ExitLong(quantity);
			else if (Position.MarketPosition == MarketPosition.Short)
				ExitShort(quantity);
			_currentRayContainer.ParialProfitDisable();
			_doNoRemoveAltLine = true;
			_checkBoxEnablePartialProfit.Checked = false;
			_doNoRemoveAltLine = false;
			_wasPrtialProfit = true;
		}

		private void StopToEntryOrParialProfit()
		{
			if (_radioButtonEntryLine.Checked)
			{
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					double distance = RealTickSize * (double)_numericUpDownPipTicksToActivate.Value;
					if (Position.MarketPosition == MarketPosition.Long)
					{
						if (_currentPrice >= RayPrice(_currentRayContainer.EntryRay) + distance)
							MoveStopLineTo(_currentRayContainer.EntryRay, (int)(-1 * _numericUpDownPipTicksToActivateDistance.Value));
					}
					else
					{
						if (_currentPrice <= RayPrice(_currentRayContainer.EntryRay) - distance)
							MoveStopLineTo(_currentRayContainer.EntryRay, (int)_numericUpDownPipTicksToActivateDistance.Value);
					}
				}
			}
			if (_radioButtonPartialProfit.Checked && _currentRayContainer.IsAlternativeLine)
			{
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					double distance = RealTickSize * (double)_numericUpDownPipTicksToActivate.Value;
					IRay ray = GetHulfRay();
					if (Position.MarketPosition == MarketPosition.Long)
					{
						if (_currentPrice >= RayPrice(ray) + distance)
							MoveStopLineTo(ray, (int)-_numericUpDownPipTicksToActivateDistance.Value);
					}
					else
					{
						if (_currentPrice <= RayPrice(ray) - distance)
							MoveStopLineTo(ray, (int)_numericUpDownPipTicksToActivateDistance.Value);
					}

				}

			}
		}

		private IRay GetHulfRay()
		{
			return _currentRayContainer.IsAlternativeLine ? _currentRayContainer.AlternativeRay : _currentRayContainer.HalfCloseRay;
		}

		private void CreateLongLimit()
		{
			int quantity = enterQunatity;
			double stopPrice = RayPrice(_currentRayContainer.EntryRay);
			_currentOrder = EnterLongLimit(quantity, stopPrice);
		}

		private void CreateShortLimit()
		{
			int quantity = enterQunatity;
			double stopPrice = RayPrice(_currentRayContainer.EntryRay);
			_currentOrder = EnterShortLimit(quantity, stopPrice);
		}

		private void UpdateGraphics()
		{
			if (_currentRayContainer != null)
			{
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
			_mainPanel.Invalidate();
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
			if (_currentRayContainer != null)
			{
				_currentRayContainer.StopRay.Locked = false;
				_currentDynamicTrailingStop.Clear();
				_currentDynamicTrailingStop = null;
			}
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

			//Full update to see the changes what we made
			_currentDynamicTrailingStop = new DynamicTrailingStop(this, _currentRayContainer,_checkBoxEnableTrailStopPreAnalyze.Checked,_checkBoxEnableTrailStopPostAnalyze.Checked);
			ChartControl.ChartPanel.Invalidate();
		}

		private void UpdateDts()
		{
			if (_currentDynamicTrailingStop != null)
			{
				_currentDynamicTrailingStop.UpdateEntry(_currentPrice);
				if (FirstTickOfBar)
				{
					_currentDynamicTrailingStop.NewBar();
					_currentDynamicTrailingStop.UpdateFirstBar();
				}
			}
			else
			{
				MessageBox.Show("Some Problems with DTS pleas restart it or restart CST");
				if (_currentRayContainer == null)
				{
					MessageBox.Show("Create the lines for DTS or reset if they already exist");
					_checkBoxEnableTrailStop.Checked = false;
				}
				else
					EnableDts();
			}
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

		private void _buttonActivateClick(object sender, EventArgs e)
		{
			if (_currentRayContainer == null)
			{
				MessageBox.Show("You don't have lines to trade with");
				return;
			}
			if (_strategyState == StrategyState.NotActive)
			{
				SetActiveVariables();
				SetVisualActive();
			}
			//If we got Enter or Exit We should wait till them will be changes to what we want from them
			else 
			{
				_deActivate = true;
			}
		}

		private void SetActiveVariables()
		{

			if (_checkBoxEnableBarEntry.Checked)
				ActivateBarEntry();
			 enterQunatity = (int) _numericUpDownQuantity.Value;
			_firstOrderSet = true;
			_strategyState = StrategyState.Enter;
			_deActivate = false;
			_wasPrtialProfit = false;
		}

		private void ActivateBarEntry()
		{
			
			EntryLineTouches = (int)_numericUpDownBarEntry.Value;
			_realBarTocuhCount = EntryLineTouches;
			_numericUpDownBarEntry.Enabled = false;
			_checkBoxEnableConsecutive.Enabled = false;
		}

		private void SetVisualActive()
		{
			//Making over buttons now Deactivate Buttons
			_buttonActivate.Text = "DEACTIVATE";
			_buttonActivate.BackColor = _deactivateColor;
			//Set status
			//Make visual  Effects
			_statusLabel.BackColor = _enabledColor;
			_statusLabel.Text = _currentRayContainer.PositionType == MarketPosition.Long
				? "Active: Long Position"
				: "Active: Short Position";
		}

		private void DeActivation()
		{

			if (weGotError)
			{
				_currentOrder = null;
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					OtherWayToClosefullPosition();
					return;
				}
				Deactivate();
				CancelAllOrders(true,true);
				_deActivate = false;
				weGotError = false;
				return;

			}
			if (_checkBoxEnableBarEntry.Checked)
			{
				_checkBoxEnableBarEntry.Checked = false;
				Deactivate();
			}
			//Two deactivations if something goes wrong
			if (_deActivate && _strategyState == StrategyState.Enter && (_currentOrder == null) && Position.MarketPosition == MarketPosition.Flat)
			{
				CancelAllOrders(true, false);
				Deactivate();
				_deActivate = false;
				return;
			}

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
					//if it is not canceled yet we cancel it
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
						OtherWayToClosefullPosition();
					}
				}
			}

			//For other currancy it is very easy
			if (_deActivate) return;
			//here we set our position as default
			Deactivate();
		}

		private void OtherWayToClosefullPosition()
		{
			double d = (5*RealTickSize);
			if (Position.MarketPosition == MarketPosition.Long)
				ExitLongLimit(_currentPrice - d);
			else if (Position.MarketPosition == MarketPosition.Short)
				ExitShortLimit(_currentPrice + d);
		}

		private void Deactivate()
		{
			_strategyState = StrategyState.NotActive;
			//Set our form to not active
			SetVisualToNotActive();
		}

		private void SetVisualToNotActive()
		{
			if (_currentRayContainer != null && _currentRayContainer.IsAlternativeLine)
				_currentRayContainer.RemoveAlternativeRay();
			_checkBoxEnableTrailStop.Checked = false;
			_checkBoxEnablePartialProfit.Checked = false;
			_radioButtonNone.Checked = true;

			_buttonActivate.Text = "ACTIVATE";
			_buttonActivate.BackColor = _activateColor;
			_statusLabel.BackColor = _disabledColor;
			_statusLabel.Text = "Not Active";

		}

		private bool _closeHalfNow;

		private void _buttonCloseHalfPositionClick(object sender, EventArgs e)
		{
			if (_strategyState != StrategyState.Exit)
			{
				MessageBox.Show("No open position to close half");
				return;
			}
			if (_realQuantity <= 1)
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
				if (_strategyState == StrategyState.Exit && _numericUpDownQuantity.Value < 2)
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
					_checkBoxEnableTrailStop.Checked = false;
					MessageBox.Show("First you should create the Rays to get Dynamic Trailing Stop");
				}
				return;
			}
			if (_checkBoxEnableTrailStop.Checked&&_numericUpDownSwingIndicatorBars.Value<=1)
			{
				MessageBox.Show("You can not acitvate DTS with Swing Indicator 1 or less only when swing indicator>1");
				_checkBoxEnableTrailStop.Checked = false;
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

		private void _buttonClosePositionClick(object sender, EventArgs e)
		{
			if (_strategyState != StrategyState.Exit)
			{
				MessageBox.Show("We are not active so we have nothing to close");
				return;
			}
			_deActivate = true;
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
				if (!_checkBoxEnablePartialProfit.Checked && !_currentRayContainer.IsAlternativeLine)
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


			_currentRayContainer.StopRay.Anchor1Y += rayDistance + (RealTickSize * distance);
			_currentRayContainer.StopRay.Anchor2Y += rayDistance + (RealTickSize * distance);

			_currentRayContainer.Update();
		}




		private void _checkBoxEnableBarEntry_CheckedChanged(object sender, EventArgs e)
		{
			if (_checkBoxEnableBarEntry.Checked)
			{
				if (_strategyState == StrategyState.NotActive || _numericUpDownBarEntry.Value == 0)
				{
					_checkBoxEnableBarEntry.Checked = false;
					MessageBox.Show("You are got Bar to Entry Zero or You are not active yet");
					return;

				}
				if (_strategyState == StrategyState.Exit )
				{
					_checkBoxEnableBarEntry.Checked = false;
					MessageBox.Show("You can not activate bar entry when you are already in position");
					return;

				}
				if (_strategyState == StrategyState.Enter)
				{
					CancelAllOrders(true, false);
					_currentOrder = null;
					//Some changes
					ActivateBarEntry();
				}
			}
			else
			{
				DeActivateBarEntry();
			}

		}

		private void DeActivateBarEntry()
		{
			_numericUpDownBarEntry.Enabled = true;
			_checkBoxEnableConsecutive.Enabled = true;
			_isTouching = false;
			_numericUpDownBarEntry.Value = _realBarTocuhCount;
		}

		private void _checkBoxOtherCurrencyOnCheckedChanged(object sender, EventArgs eventArgs)
		{
			_textBoxOtherCurrency.Enabled = _checkBoxOtherCurrency.Checked;
		}

		public double RealRayPrice(IRay ray, int ago)
		{
			//So how much step per bar we got here
			double oneBarDistance = (ray.Anchor1Y - ray.Anchor2Y) / (ray.Anchor1BarsAgo - ray.Anchor2BarsAgo);

			//Now how add the count of those steps to over lest price and then return 
			double rayPrice = (-oneBarDistance * (ray.Anchor2BarsAgo -ago)) + ray.Anchor2Y;
			return rayPrice;
		}
		public double RayPriceNotRound(IRay ray)
		{
			//So how much step per bar we got here
			return RealRayPrice(ray,0);
		}
		public double RayPrice(IRay ray)
		{
			return Instrument.MasterInstrument.Round2TickSize( RayPriceNotRound(ray));
		}


		private void SendMail(string topic, string text)
		{
			SendingText = text;
			SendingTopic = "CST Alert: " + topic;
			Thread mailSendingThread = new Thread(MailSender);
			mailSendingThread.Start();
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

				string htmlBody = "<html><body>" + SendingText +
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
				MessageBox.Show("Some problems with your Internet\n E-Mail:" + SendingTopic + "\nWas not sanded");
				//Todo: remove second massage
				MessageBox.Show("We got some error"+ex.Message+ex.StackTrace);

				Thread.CurrentThread.Abort();
			}
		}

		private string TextFormater(string action, bool isEntry, bool isPartialProfit)
		{
			try
			{
				StringBuilder textResult = new StringBuilder();
				string time = DateTime.Now.ToString("HH:mm"); ;
				string symbol = _checkBoxOtherCurrency.Checked ? _otherInstrumentName : Position.Instrument.FullName;

				//Now Adding formated text

				textResult.AppendFormat("<pre>Action:		{0}</pre>", action);

				textResult.AppendFormat("<pre>Time:		{0}</pre>", time);

				textResult.AppendFormat("<pre>Symbol:		{0}</pre>", symbol);

				textResult.AppendFormat("<pre>Position:	{0}</pre>", _currentRayContainer.PositionType);

				textResult = AddToTextQunaityAndProfit(isEntry,isPartialProfit, textResult);

				return textResult.ToString();
			}
			catch (Exception e)
			{
				MessageBox.Show("Problems with formating text");
			}
			return "some problem with this e-mail text";

		}

		private int lastQuantity = 0;
		public MasterInstrument MyInstrument;
		private int enterQunatity;

		private StringBuilder AddToTextQunaityAndProfit(bool isEntry, bool isPartialProfit, StringBuilder textResult)
		{
			int quantity = 0;
			if (isPartialProfit)
			{
				quantity = _realQuantity / 2;
				lastQuantity = quantity;
			}
			else if (_wasPrtialProfit)
				quantity = _realQuantity - lastQuantity;
			else
				quantity = _realQuantity;

			textResult.AppendFormat("<pre>Quantity:	{0}</pre>", quantity);


			double profitCurrancy = 0;
			double profitProcents = 0;
			if (!isEntry)
			{
				if (!_checkBoxOtherCurrency.Checked)
				{
					if (!isPartialProfit && !_wasPrtialProfit)
					{
						profitCurrancy = _profitLoss;
						profitProcents = _profitPercent*100;
					}
					else if (isPartialProfit)
					{
						double value = (double) quantity/_realQuantity;
						profitCurrancy = _profitLoss*value;
						profitProcents = _profitPercent*value*100;
					}
					else if (_wasPrtialProfit)
					{
						profitCurrancy = _profitLoss;
						profitProcents = _profitPercent*50;
					}
				}
				else
				{
					profitCurrancy = _ohterProfitLoss;
					profitProcents = _otherProfitPercent*100;
				}
				textResult.AppendFormat("<pre>Profit:		{0}USD & {1}%</pre>", profitCurrancy, Math.Round(profitProcents, 3));
			}
			return textResult;
		}

		public Bitmap GetChartPicture()
		{
			Bitmap bmp;
			bmp = GetChartPictureFast();
			int counter = 0;
			do
			{
				Thread.Sleep(1500);
				ChartControl.ChartPanel.DrawToBitmap(bmp, ChartControl.ChartPanel.ClientRectangle);
				counter++;
				if (counter > 3)
					break;
			} while (true);

			return bmp;
		}

		public Bitmap GetChartPictureFast()
		{
			Bitmap bmp;
			bmp = new Bitmap(ChartControl.ChartPanel.Width, ChartControl.ChartPanel.Height, PixelFormat.Format16bppRgb555);
			ChartControl.ChartPanel.DrawToBitmap(bmp, ChartControl.ChartPanel.ClientRectangle);
			return bmp;
		}

		private Bitmap GetCSTPanelPicture()
		{
			Bitmap bmp = new Bitmap(_mainPanel.Width, _mainPanel.Height, PixelFormat.Format16bppRgb555);
			_mainPanel.DrawToBitmap(bmp, _mainPanel.ClientRectangle);
			return bmp;
		}

		#region Visible Proportys

//		[Description("The other currency that we will use for trading")]
//		[GridCategory("Parameters")]
//		[Gui.Design.DisplayName("Second currency")]
//		public string OtherInstrumentName
//		{
//			get { return _otherInstrumentName; }
//			set { _otherInstrumentName = value; }
//		}


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

		#endregion
	}
	//Let's take a look how it works
} 
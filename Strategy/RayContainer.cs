using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;

namespace NinjaTrader.Strategy
{
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
			Distance = strategy.ProportionalDistance < _strategy.TickSize * 3 ? 3 * strategy.TickSize : strategy.ProportionalDistance;
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
			double distance;
			//to Made it between price and entry line
			//				= (_tpDot.Y - _strategy._currentPrice) / 2;
			//			if (!exit)
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
				IsAlternativeLine = false;
				_strategy.RemoveDrawObject(AlternativeRay);
			}
		}
	}
}
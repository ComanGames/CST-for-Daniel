using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;

// ReSharper disable once CheckNamespace
namespace NinjaTrader.Strategy
{
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
		private int _lastMinimumOrMaximum;
		private bool _isFirstSlope ;
		const int PreAnalyzeValue = 64;

		public DynamicTrailingStop(ChartSlopeTrader strategy, RayContainer rayContainer,bool isPreAnalyze, bool isPost)
		{
			//Setting the variables
			_strategy = strategy;
			_rayContainer = rayContainer;
			_isFirstSlope = true;
			PositonType = rayContainer.PositionType;
			_isWaitSlope = true;
			_lastMinimumOrMaximum = 0;
			_lastSlope = new Slope(0, 0);
			NewBar();
			UpdateFirstBar();

			if (isPreAnalyze)
			{
				//Getting the first slope
				Slope tempSlope = GetFirstStlope(0, Math.Min(PreAnalyzeValue, strategy.Low.Count - 1),isPost);
				if (_strategy.SwingIndicatorBars <= 1)
				{
					MessageBox.Show("You can not start pre-analyze with one or less bars");
					return;
				}
				if (PositonType == MarketPosition.Long && Math.Abs(tempSlope.Price - strategy.High[1]) < strategy.RealTickSize)
				{
					MessageBox.Show("Your swing price want to be set as same as previous bar");
					return;
				}
				if (PositonType == MarketPosition.Short && Math.Abs(tempSlope.Price - strategy.Low[1]) < strategy.RealTickSize)
				{
					MessageBox.Show("Your swing price want to be set as same as previous bar");
					return;
					
				}
				_currentSlope = tempSlope;
				UpdateSlopeLine(_currentSlope);
				_isFirstSlope = false;
				_isWaitSlope = true;
				_strategy.SendMail_dtsNewSlopeLineNew(_currentSlope.Price);
			}
			else
			{
				_isWaitSlope = true;

			}
		}

		~DynamicTrailingStop()
		{
			Clear();
		}

		public void Clear()
		{
			_rayContainer.StopRay.Locked = false;
			_strategy.RemoveDrawObject(_ray);
			_strategy.RemoveDrawObject(_dot);
			_strategy.RemoveDrawObject(_text);
			_strategy.ChartControl.ChartPanel.Invalidate();
		}

		private void SetStopRay(double price)
		{
			if (Math.Abs(price) < 0.1)
				return;
			_rayContainer.StopRay.Anchor1Y = price;
			_rayContainer.StopRay.Anchor2Y = price;
			_rayContainer.StopRay.Anchor1BarsAgo = _currentSlope.Bar;
			_rayContainer.StopRay.Anchor2BarsAgo = _currentSlope.Bar - 3;
			_rayContainer.StopRay.Locked = true;
			_strategy.ChartControl.ChartPanel.Invalidate();

		}

		private string TextForma(double price)
		{
			string priceText = price.ToString(CultureInfo.InvariantCulture) + "\n";
			return new string(' ', priceText.Length + _rayContainer.TextShift) + priceText;
		}

		public void UpdateSlopeLine(Slope slope)
		{
			double price = _strategy.Instrument.MasterInstrument.Round2TickSize(slope.Price);
			_ray = _strategy.DrawRay("Slope", false, slope.Bar, price, slope.Bar - 3, price, SlopeLineColor, DashStyle.Solid, 2);
			_dot = _strategy.DrawDot("slopeDot", false, 0, price, Color.Black);
			_text = _strategy.DrawText("SlopeText", TextForma(price), 0, price, Color.Black);
			_strategy.ChartControl.ChartPanel.Invalidate();
		}

		public void UpdateExistingSlopeRay(IRay ray)
		{
			
			double price = _strategy.RayPrice(ray);
			_dot = _strategy.DrawDot("slopeDot", false, 0, price, Color.Black);
			_text = _strategy.DrawText("SlopeText", TextForma(price), 0, price, Color.Black);
			_strategy.ChartControl.ChartPanel.Invalidate();
		}

		public Slope GetFirstStlope(int from, int till,bool isPostResult)
		{
			Slope slope =GetLastSlope(from, till);
			if(isPostResult)
				slope = PostResult(till, slope);
			return slope;
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
			return result;
		}

		private Slope PostResult(int till, Slope result)
		{
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
			int postion = 0;
			return LocalMin(start, end, ref postion);
		}

		public double LocalMin(int start, int end, ref int position)
		{
			double result = _strategy.Low[start];
			for (int i = start; i <= end; i++)
			{
				if (_strategy.Low[i] <= result)
				{
					result = _strategy.Low[i];
					position = i;
				}
			}
			return result;
		}

		public double LocalMax(int start, int end)
		{
			int position = 0;
			return LocalMax(start, end, ref position);
		}

		public double LocalMax(int start, int end, ref int position)
		{
			double result = _strategy.High[start];
			for (int i = start; i <= end; i++)
			{
				if (_strategy.High[i] >= result)
				{
					result = _strategy.High[i];
					position = i;
				}
			}
			return result;
		}

		public void NewBar()
		{
			_currentSlope.Bar++;
			_lastSlope.Bar++;
			_lastMinimumOrMaximum++;
			if (_ray != null)
			{
				UpdateExistingSlopeRay(_ray);
			}
		}

		public void UpdateFirstBar()
		{
			//If we do not wait for new Slow
			if (_isWaitSlope) //We wait for a slope
			{
				Slope tempSlope = GetLastSlope(0, Math.Max(_lastMinimumOrMaximum, _lastSlope.Bar));
				if (Math.Abs(tempSlope.Price) < 0.01)
					return;

				if ((PositonType == MarketPosition.Long && tempSlope.Price > _lastSlope.Price)
					|| (PositonType == MarketPosition.Short && tempSlope.Price < _lastSlope.Price)
					||_isFirstSlope)
				{
					_isFirstSlope = false;
					_currentSlope = tempSlope;
					UpdateSlopeLine(_currentSlope);
					_isWaitSlope = false;
					_strategy.SendMail_dtsNewSlopeLineNew(_currentSlope.Price);
				}

			}
		}

		public void UpdateEntry(double currentPrice)
		{
			if (!_isWaitSlope)
			{
				if (PositonType == MarketPosition.Long && currentPrice > _currentSlope.Price + _strategy.SwingHorizontal)
				{
					if(currentPrice<_rayContainer.RealRayPrice(_rayContainer.EntryRay))
					{
						SetVariablesWithoutStop();
						return;
					}
					_lastSlope = _currentSlope;
					SetStopRay(LocalMin(0, _currentSlope.Bar, ref _lastMinimumOrMaximum) - _strategy.StopLevel);
					SavingSlopeAndEmail();
				}
				else if (PositonType == MarketPosition.Short && currentPrice < _currentSlope.Price - _strategy.SwingHorizontal)
				{
					if(currentPrice>_rayContainer.RealRayPrice(_rayContainer.EntryRay))
					{
						SetVariablesWithoutStop();
						return;
					}
					_lastSlope = _currentSlope;
					SetStopRay(LocalMax(0, _currentSlope.Bar, ref _lastMinimumOrMaximum) + _strategy.StopLevel);
					SavingSlopeAndEmail();
				}
			}

		}

		private void SetVariablesWithoutStop()
		{
			_lastSlope = _currentSlope;
			_isWaitSlope = true;
		}

		private void SavingSlopeAndEmail()
		{
			_isWaitSlope = true;
			_strategy.SendMail_dtsStopLineMoved();
		}
	}
}
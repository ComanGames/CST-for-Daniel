// 
// Copyright (C) 2009, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// This strategy demonstrates how to run certain calculation logic every tick, while only running entry calculations once per bar.
    /// </summary>
    [Description("This strategy demonstrates how to run certain calculation logic every tick, while only running entry calculations once per bar.")]
    public class SampleEnterOnceExitEveryTick : Strategy
    {
        #region Variables
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			// Set the plot colors.
			LinReg(Median, 10).Plots[0].Pen.Color = Color.RoyalBlue;
			SMA(Median, 10).Plots[0].Pen.Color = Color.Red;
			
			// Add indicators to the chart.
			Add(LinReg(Median, 10));
			Add(SMA(Median, 10));
			
			/* With CalculateOnBarClose = false, OnBarUpdate() gets called on every tick.
			   We can then filter for the first tick for entries and let the exit conditions run on every OnBarUpdate(). */
			CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		protected override void OnBarUpdate()
		{
			// Return if historical--this sample utilizes tick information, so is necessary to run in real-time.
			if (Historical)
				return;
			
			/* FirstTickOfBar specifies that this section of code will only run once per bar, at the close of the bar as indicated by the open of the next bar.
			   NinjaTrader decides a bar is closed when the first tick of the new bar comes in,
			   therefore making the close of one bar and the open of the next bar virtually the same event. */
			if (FirstTickOfBar && Position.MarketPosition == MarketPosition.Flat)
			{
				/* Since we're technically running calculations at the open of the bar (with open and close being the same event),
				   we need to shift index values back one value, because the open = close = high = low at the first tick.
				   Shifting the values ensures we are using data from the recently closed bar. */
				if (LinReg(Median, 10)[1] > LinReg(Median, 10)[2] && LinReg(Median, 10)[1] > SMA(Median, 10)[1])
					EnterLong("long entry");
			}
			
			// Run these calculations (on every tick, because CalculateOnBarClose = false) only if the current position is long.
			if (Position.MarketPosition == MarketPosition.Long)
			{
				/* This CrossBelow() condition can and will generate intrabar exits (CalculateOnBarClose = false). Because this logic is run once for
				   every tick recieved, it will provide the quickest exit for your strategy, and will exit as soon as the LinReg line crosses below the SMA line. */
				if (CrossBelow(LinReg(Median, 10), SMA(Median, 10), 1))
					ExitLong("LinReg cross SMA exit", "long entry");
			}
		}
        #region Properties
        #endregion
    }
}

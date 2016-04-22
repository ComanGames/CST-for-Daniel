	#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
	
    [Description("The extension what give us extra functionality for fork now we can use a lot more")]
    public class AAForkExtension : Indicator
    {
	    enum ForkState
	    {
		    Nothing,
			First,
			Second,
			Thired,
			Line,
			Fork,
	    }
        #region Variables
		//Variable to get with what fork we are working with
	    private IAndrewsPitchfork _lestSelectedFork;
	    private IRay _oveRay;
	    private const string MyTag = "AAForkExtension";
		// To get the point position 
		private double _priceMin;
		private double _priceMax;
		private int _boundsTop;
		private int _boundsHeight;
	    private double _positionPrice;
	    private DateTime _positionTime;
	    private LinkedList<IAndrewsPitchfork> _forkList = new LinkedList<IAndrewsPitchfork>();
	    // Wizard generated variables
        // User defined variables (add any user defined variables below)
        #endregion

	    private ForkState _state;
	    #region SupportedFunctionality

	    /// <summary>
	    /// This method is used to configure the indicator and is called once before any bar data is loaded.
	    /// </summary>
	    protected override void Initialize()
	    {
		    Overlay = true;
	    }

	    protected override void OnBarUpdate()
	    {
			//Do nothing
	    }

	    /// <summary>
		/// What we do when we add the indicator ot the chart
		/// </summary>
		protected override void OnStartUp()
		{
			//Made over basic start 
			base.OnStartUp();
			//Clean Before we start
			//Now let's add some events that we want to trace
			ChartControl.ChartPanel.KeyDown		+= ActionOnKeyDown;
			ChartControl.ChartPanel.MouseMove	+= MouseMove;
		    ChartControl.ChartPanel.MouseClick	+= MouseClick;
		}


	    /// <summary>
		/// Cleaning after over self
		/// It usualson a quit when we guit for the chart
		/// </summary>
		protected override void OnTermination()
		{
			//Cleaning after over selfs 
			CleanUpTrash();
		}

	    private void CleanUpTrash()
	    {
		//Now we remove the events
		    ChartControl.ChartPanel.KeyDown		-= ActionOnKeyDown;
		    ChartControl.ChartPanel.MouseMove	-= MouseMove;
		    ChartControl.ChartPanel.MouseClick	-= MouseClick;
	    }

	    private void UpdateChart()
	    {
				switch (_state)
				{
					case ForkState.Second:
						if (_forkRay == null)
							return;
						_forkRay.EndTime = _positionTime;
						_forkRay.EndY = _positionPrice;
						break;
					case ForkState.Thired:
						IAndrewsPitchfork frork = _forkList.Last.Value;
						if (frork == null)
							return;
						frork.Anchor3Time = _positionTime;
						frork.Anchor3Y = _positionPrice;
						break;
				}
	    }

	    /// <summary>
	    /// Called on each bar update event (incoming tick)
	    /// </summary>


	    #endregion

	    #region Unsuported Functionality

	    /// <summary>
	    /// This Method we call every time we click
	    /// over mouse some where on the chart
	    /// </summary>
	    public bool DrawingFork;

	    private ILine _forkRay;


	    protected override void OnMarketData(MarketDataEventArgs e)
	    {
		    base.OnMarketData(e);
			CleanUpTrash();
	    }

	    #region Getting position on the cahrt

	    /// <summary>
		/// Normal overridet funiton just to get over variabls to work
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="bounds"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
	    public override void Plot(Graphics graphics, Rectangle bounds, double min, double max)
	    {
			UpdateChart();
			//Just saving variables to get them after when 
			//We will count the postion of over mouse on over Chart
		    _priceMin = min;
		    _priceMax = max;
		    _boundsTop = bounds.Top;
		    _boundsHeight = bounds.Height;
			//Calling base to do not miss something
		    base.Plot(graphics, bounds, min, max);
	    }

	    /// <summary>
		/// Catching the mouse moving to get t
		/// the positon of the mouse
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
	    private void MouseMove(object sender, MouseEventArgs e)
	    {
		    GetPositionOnChart(sender, e);
	    }

	    private void MouseClick(object sender, MouseEventArgs e)
	    {
		    switch (_state)
		    {
			    case ForkState.First:
				    //Drawing the starting ray
				    _forkRay = DrawLine("RayLine", false, _positionTime, _positionPrice, _positionTime, _positionPrice, Color.Black, DashStyle.DashDot, 1);
				    _state = ForkState.Second;
				    break;

			    case ForkState.Second:
				    IAndrewsPitchfork fork = DrawAndrewsPitchfork("currentFork" + _forkList.Count.ToString(), false,
					    _forkRay.StartTime, _forkRay.StartY, _forkRay.EndTime, _forkRay.EndY, _positionTime, _positionPrice,
					    Color.FromKnownColor(KnownColor.Blue), DashStyle.Custom, 1);
				    _forkList.AddLast(fork);
				    RemoveDrawObject(_forkRay);
				    _state = ForkState.Thired;
				    break;
			    case ForkState.Thired:
				    _state = ForkState.Fork;
				    break;
		    }
	    }

	    private void GetPositionOnChart(object sender, MouseEventArgs e)
	    {
			//We getting the chart what to get counting with it
		    Panel chartPanel = sender as Panel;
		    //If we get no chart we will get out from the funciton 
		    if (chartPanel == null)
			    return;

		    //Frist we get hard countation of counting over mouse position in over limits
		    _positionPrice = _priceMax - (e.Y - _boundsTop)*(_priceMax - _priceMin)/_boundsHeight;
		    //And we got converatation from the real postion to the cart possiton 
		    _positionPrice = Instrument.MasterInstrument.Round2TickSize(_positionPrice);

	    //Now we getting type of chart Control to get access to his reflection 
		    Type type = ChartControl.GetType();
		    //Now we gettting method from him using net reflecton
		    MethodInfo mi = type.GetMethod("GetTimeByX", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		    //And nwo finaly we get the position by reflection call the hidden method 
		    _positionTime = (DateTime) mi.Invoke(ChartControl, new object[] {e.X});
	    }

	    #endregion

	    /// <summary>
		/// Here we got the functionality what works when we 
		/// Press over key here we got and created all over shortcuts
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ActionOnKeyDown(object sender, KeyEventArgs e)
		{
			//if we got space we will create the fork
			if (e.KeyCode == Keys.Space)
				CreateFork();

			//if we hold shift and press some other action we go result
			if (e.Shift)
			{
				switch (e.KeyCode)
				{
					case Keys.Space:
						CreateFork();
						break;
					//Creating the ray 
					case Keys.F:
						GetRay();
						break;
					//Change statement to the Andrew F0rk
					case Keys.A:
						AndrewFork();
						break;
					//Changing the Schiff Fork 
					case Keys.S:
						SchiffFork();
						break;
					//Changing to modify schiffFork
					case Keys.D:
						ModifySchiffFork();
						break;
					case Keys.Q:
						RemovOldFork();
						break;
					case Keys.G:
						RemoveRay();
						break;
				}
			}
		}
		
	    private void RemoveRay()
	    {
		    if (_oveRay != null)
		    {
				//Reomving ray as drawing object
				RemoveDrawObject(_oveRay);
				//Making null to be sure
			    _oveRay = null;
				//Cleaning after over self
				GC.Collect();
		    }
	    }

	    /// <summary>
		/// Fork creating methods 
		/// Create fork what we 
		/// Will be drawing
		/// </summary>
	    private void CreateFork()
	    {
		    DrawingFork = true;
		    _state = ForkState.First;
		    ChangeCoursor(ChartCursor.Order );
//		    CreateChartFork();
	    }

	    private void ChangeCoursor(ChartCursor pointer)
	    {
		    ChartControl.ChartCursor =pointer;
		    ChartControl.Invalidate();
		}

	    private void CreateChartFork()
	    {
//Cleaning the chart before we start in any way we will clean it but if you want to get the new ones on the chart
		    ClearOverChart();
		    //Getting item from the context menu
		    ContextMenuStrip menuStrip = ChartControl.ContextMenuStrip;
		    //Getting Items from contex menu
		    ToolStripItemCollection menuItems = menuStrip.Items;
		    //Now getting item with Drawing Tool SubMenu
		    ToolStripMenuItem toolStripMenuItem = menuItems[19] as ToolStripMenuItem;
		    //The item what we will use as over item with what we work with 
		    ToolStripItem andrewForkItem = null;
		    //If those sub menu not equals to null
		    if (toolStripMenuItem != null)
		    {
			    //Getting sub menu items
			    ToolStripItemCollection toolStripItemCollection = toolStripMenuItem.DropDownItems;
			    //Here we get the  item
			    andrewForkItem = toolStripItemCollection[16];
		    }
		    //If we set over tiem to something
		    if (andrewForkItem != null)
		    {
			    //And here wer create over for 
			    andrewForkItem.PerformClick();
		    }
	    }

	    /// <summary>
		/// The method what give us selected 
		/// or lest selected fork 
		/// </summary>
		/// <returns></returns>
	    private IAndrewsPitchfork GetSelectedFork()
	    {
			//Instance for over result 
		    IAndrewsPitchfork result = null; 
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
					//Gettin the instance of the object
				    object clickedObject = fi.GetValue(ChartControl);
					//Checking if ti posible to convert
				    if (clickedObject is IAndrewsPitchfork)
				    {
						//Converting 
					    result = (IAndrewsPitchfork) clickedObject;
						//if we got the result at all 
					    if (result!=null)
					    {
							//Now we stay real one for moving
						    if (result.Tag != MyTag)
							    result = MadeEvilClone(result);
					    }
				    }
			    }
		    }
			//if we have result also we save lest used fork
			if(result!=null)
				_lestSelectedFork = result;
			//If we have no fork 
			if(result== null&&_lestSelectedFork!=null)
				result = _lestSelectedFork;
		    return result;
	    }

	    /// <summary>
		/// Dark function what remove the original fork and making the clone 
		/// But we pay the big price for this shadow functionality we remove
		/// All other indicators and other stuff to get the funciotnality
		/// That we want to have in over project
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
	    private IAndrewsPitchfork MadeEvilClone(IAndrewsPitchfork result)
	    {
			//Saving information about original one
			DateTime fAnchor1Time = result.Anchor1Time;
			double fAnchor1Y =		result.Anchor1Y;
			DateTime fAnchor2Time = result.Anchor2Time;
			double fAnchor2Y =		result.Anchor2Y;
			DateTime fAnchor3Time = result.Anchor3Time;
			double fAnchor3Y =		result.Anchor3Y;

			//Removing old and all other from the real chart
			ClearOverChart();

			//Creating the copy absolutly the same as original was but now we got control on it
			IAndrewsPitchfork newFork = DrawAndrewsPitchfork(MyTag, true, fAnchor1Time, fAnchor1Y, fAnchor3Time, fAnchor3Y, fAnchor2Time, fAnchor2Y,
			 //Settin the collor what we like
			 Color.FromKnownColor(KnownColor.Blue),
			 //Dash Stile and width
			 DashStyle.Solid, 1);
		    //Now we make Calculation Method what we need to have
		    newFork.CalculationMethod = CalculationMethod.StandardPitchfork;
		    //the unlocking fork to drag and drop
		    newFork.Locked = false;
		    //Removing the old for
		    _lestSelectedFork = newFork;
			//Back over new evil fork
		    return newFork;

	    }

	    private void RemovOldFork()
	    {
			ClearOverChart();
	    }

	    private void ClearOverChart()
	    {
			//Removing lest selected fork to do not create the bugs
		    _lestSelectedFork = null;
			BindingFlags bfObject = BindingFlags.Instance | BindingFlags.NonPublic;
			MethodInfo methodNt = typeof(ChartControl).GetMethod("RemoveDrawingObjectsAfterBar", bfObject);
			methodNt.Invoke(ChartControl, new Object[] { 0 });
		}

	    private void GetRay()
	    {
			//Getting the fork what we will use 
			AndrewsPitchfork fork = (AndrewsPitchfork) GetSelectedFork();
			//Only if over fork is not empty 
		    if (fork!=null)
		    {
				//First point of over fork
			    DateTime anchor1Time = fork.Anchor1Time;
			    double anchor1Y = fork.Anchor1Y;

				//Second point of over fork
			    DateTime anchor2Time = fork.MiddleTime;
			    double anchor2Y = fork.MiddleY;

				//Now we looking for second point of over ray
			    anchor2Time = AvrageTime(fork.Anchor2Time,fork.Anchor3Time);
			    anchor2Y = (fork.Anchor2Y + fork.Anchor3Y)/2;

				//What kind of fork we got such type of ray we will got
			    switch (fork.CalculationMethod)
			    {
						//Counting for modifiedSchiff
						case CalculationMethod.ModifiedSchiff:
					    anchor1Time = AvrageTime(fork.Anchor1Time, fork.Anchor3Time);
					    anchor1Y = (fork.Anchor1Y + fork.Anchor3Y)/2;
					    break;

						//Counting for Schiff fork
						case CalculationMethod.Schiff	:
					    anchor1Y = (fork.Anchor1Y + fork.Anchor3Y)/2;
						break;
			    }
				//Getting diffrence between first and second position
			    double secondPositionY = anchor2Y - anchor1Y;
			    TimeSpan secondPositionX = anchor2Time.Subtract(anchor1Time);

				//Drawing over ray
			    _oveRay = DrawRay("Ray", true, _positionTime, _positionPrice, _positionTime.Add(secondPositionX),
				    _positionPrice + secondPositionY, Color.Red, DashStyle.Solid, 2);

				//Making it drawable
			    _oveRay.Locked = false;

				//Cleaning after over self
				GC.Collect();
		    }
	    }

		/// <summary>
		/// The code what give us avarage of two dattime
		/// 
		/// </summary>
		/// <param name="anchor1Time"></param>
		/// <param name="anchor2Time"></param>
		/// <returns></returns>
	    private DateTime AvrageTime(DateTime anchor1Time, DateTime anchor2Time)
	    {
			//Make normal as default
		    DateTime bigerDateTime = anchor1Time;
		    DateTime smallDateTime = anchor2Time;

			//if we was not right changing
		    if (anchor1Time < anchor2Time)
		    {
			    bigerDateTime = anchor2Time;
			    smallDateTime = anchor1Time;
		    }
			//TimeSpan differnce of those two
		    TimeSpan ts = bigerDateTime.Subtract(smallDateTime);

			//getting the difference between those
			   ts = new TimeSpan(ts.Ticks/2);

			//Getting the result
		    return smallDateTime.Add(ts);


	    }

	    #region Fork Modification 

	    /// <summary>
	    /// Method to change over fork to Andrew Fork
	    /// </summary>
	    private void AndrewFork()
	    {
		    //First we should get the fork
		    IAndrewsPitchfork fork = GetSelectedFork();
		    if (fork!=null)
		    {
				//Making the new fork on the position of the old the only way to change the color
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(MyTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
				//Settin the collor what we like
				Color.FromKnownColor(KnownColor.Blue),
				//Dash Stile and width
				DashStyle.Solid,1);
			    //Now we make Calculation Method what we need to have
			    newFork.CalculationMethod = CalculationMethod.StandardPitchfork;
				//the unlocking fork to drag and drop
			    newFork.Locked = false;
				//Removing the old for
			    _lestSelectedFork = newFork;
		    }
		}

		/// <summary>
		/// Method to change over fork to Schiff Fork
		/// </summary>
		private void SchiffFork()
	    {
		    //First we should get the fork
		    IAndrewsPitchfork fork = GetSelectedFork();
			if (fork!=null)
		    {
				//Making the new fork on the position of the old the only way to change the color
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(MyTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
				//Settin the collor what we like
				Color.FromKnownColor(KnownColor.LightSkyBlue),
				//Dash Stile and width
				DashStyle.Solid,1);
			    //Now we make Calculation Method what we need to have
			    newFork.CalculationMethod = CalculationMethod.Schiff;
				//the unlocking fork to drag and drop
			    newFork.Locked = false;
				//Removing the old for
			    _lestSelectedFork = newFork;
		    }

	    }

	    /// <summary>
	    ///Method to change over fork to ModifySchiffFork 
	    /// </summary>
	    private void ModifySchiffFork()
	    {
		    //First we should get the fork
		    IAndrewsPitchfork fork = GetSelectedFork();
			if (fork!=null)
		    {
				//Making the new fork on the position of the old the only way to change the color
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(MyTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
				//Settin the collor what we like
				Color.FromKnownColor(KnownColor.DeepPink),
				//Dash Stile and width
				DashStyle.Solid,1);
			    //Now we make Calculation Method what we need to have
			    newFork.CalculationMethod = CalculationMethod.ModifiedSchiff;
				//the unlocking fork to drag and drop
			    newFork.Locked = false;
				//Removing the old fork
			    _lestSelectedFork = newFork;
		    }
	    }

	    #endregion


	    #endregion

	    #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Andrews
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Schiff
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries MdifiedSchiff
        {
            get { return Values[2]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries RayLine
        {
            get { return Values[3]; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator
    {
        private AAForkExtension[] _cacheAaForkExtension;

        private static AAForkExtension checkAAForkExtension = new AAForkExtension();

        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        public AAForkExtension AAForkExtension()
        {
            return AAForkExtension(Input);
        }

        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        public AAForkExtension AAForkExtension(Data.IDataSeries input)
        {
            if (_cacheAaForkExtension != null)
                for (int idx = 0; idx < _cacheAaForkExtension.Length; idx++)
                    if (_cacheAaForkExtension[idx].EqualsInput(input))
                        return _cacheAaForkExtension[idx];

            lock (checkAAForkExtension)
            {
                if (_cacheAaForkExtension != null)
                    for (int idx = 0; idx < _cacheAaForkExtension.Length; idx++)
                        if (_cacheAaForkExtension[idx].EqualsInput(input))
                            return _cacheAaForkExtension[idx];

                AAForkExtension indicator = new AAForkExtension();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                AAForkExtension[] tmp = new AAForkExtension[_cacheAaForkExtension == null ? 1 : _cacheAaForkExtension.Length + 1];
                if (_cacheAaForkExtension != null)
                    _cacheAaForkExtension.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                _cacheAaForkExtension = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.AAForkExtension AAForkExtension()
        {
            return _indicator.AAForkExtension(Input);
        }

        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        public Indicator.AAForkExtension AAForkExtension(Data.IDataSeries input)
        {
            return _indicator.AAForkExtension(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.AAForkExtension AAForkExtension()
        {
            return _indicator.AAForkExtension(Input);
        }

        /// <summary>
        /// The extension what give us extra functionality for fork now we can use a lot more
        /// </summary>
        /// <returns></returns>
        public Indicator.AAForkExtension AAForkExtension(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.AAForkExtension(input);
        }
    }
}
#endregion

	#region Using declarations
using System;
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
        #region Variables
		//Variable to get with what fork we are working with
	    private IAndrewsPitchfork _lestSelectedFork;
	    private IRay _oveRay;
	    private const string myTag = "AAForkExtension";
		// To get the point position 
		private double priceMin;
		private double priceMax;
		private int boundsTop;
		private int boundsHeight;
	    private double _positionPrice;
	    private DateTime _positionTime;
	    // Wizard generated variables
        // User defined variables (add any user defined variables below)
        #endregion

	    #region SupportedFunctionality

	    /// <summary>
	    /// This method is used to configure the indicator and is called once before any bar data is loaded.
	    /// </summary>
	    protected override void Initialize()
	    {
		    Overlay = true;

	    }

		/// <summary>
		/// What we do when we add the indicator ot the chart
		/// </summary>
		protected override void OnStartUp()
		{
			//Made over basic start 
			base.OnStartUp();
			//Now let's add some ovents that we want to trace
			ChartControl.ChartPanel.KeyDown		+= ActionOnKeyDown;
			ChartControl.ChartPanel.MouseClick	+= MouseClick;
			ChartControl.ChartPanel.MouseMove	+= MouseMove;
		}

		/// <summary>
		/// Cleaning after over self
		/// It usualson a quit when we guit for the chart
		/// </summary>
		protected override void OnTermination()
		{
			//Fist we using normal base cleaning
			base.OnTermination();

			//Now we remove the events
			ChartControl.ChartPanel.KeyDown		-= ActionOnKeyDown;
			ChartControl.ChartPanel.MouseClick	-= MouseClick;
			ChartControl.ChartPanel.MouseMove	-= MouseMove;
			ClearOverChart();

			//Force start of garbage collector
			GC.Collect();
		}

	    /// <summary>
	    /// Called on each bar update event (incoming tick)
	    /// </summary>
	    protected override void OnBarUpdate()
	    {
		    DrawingBasicFunctionality();
	    }

	    private void DrawingBasicFunctionality()
	    {
			// Use this method for calculating your indicator values. Assign a value to each
		    // plot below by replacing 'Close[0]' with your own formula.
	    }

	    #endregion

	    //#region Unsuported Functionality

	    /// <summary>
	    /// This Method we call every time we click
	    /// over mouse some where on the chart
	    /// </summary>
	    /// <param name="sender"></param>
	    /// <param name="e"></param>
	    private int _newForkClick = 0;
	    private void MouseClick(object sender, MouseEventArgs e)
	    {
			//Getting over fork changeble from the start
			//Lets check all object on the sceane
		    foreach (ChartObject co in ChartControl.ChartObjects)
		    {
				//Is one of over objects fork?
				if(co is AndrewsPitchfork)
				{
					//Is it over or not over fork?
					if(co.Tag!=myTag)
					{
						//When we creating the fork we clicked more 
						//then 2 times if not still counting
						if (_newForkClick < 2)
							_newForkClick++;
						else
						{
							//If it is over 3th click it is mean that we got the fork ready
							//Let's change the real fork with over evil clone
							MadeEvilClone(co as AndrewsPitchfork);
							//Now let's reset the counting
							_newForkClick = 0;
						}
					}
				}			    
		    }

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
			//Just saving variables to get them after when 
			//We will count the postion of over mouse on over Chart
		    priceMin = min;
		    priceMax = max;
		    boundsTop = bounds.Top;
		    boundsHeight = bounds.Height;
			//Calling base to do not miss something
		    base.Plot(graphics, bounds, min, max);
			//foreach (IDrawObject draw in DrawObjects)
			//	Print(draw.DrawType.ToString());
	    }

	    /// <summary>
		/// Catching the mouse moving to get t
		/// the positon of the mouse
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
	    private void MouseMove(object sender, MouseEventArgs e)
	    {
			//We getting the chart what to get counting with it
		    Panel chartPanel = sender as Panel;
			//If we get no chart we will get out from the funciton 
		    if (chartPanel == null)
			    return;

			//Frist we get hard countation of counting over mouse position in over limits
			double positionPrice = priceMax - (e.Y - boundsTop)*(priceMax - priceMin)/boundsHeight;
			//And we got converatation from the real postion to the cart possiton 
			_positionPrice = Instrument.MasterInstrument.Round2TickSize(positionPrice);

			//Now we getting type of chart Control to get access to his reflection 
		    Type type = ChartControl.GetType();
			//Now we gettting method from him using net reflecton
		    MethodInfo mi = type.GetMethod("GetTimeByX", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			//And nwo finaly we get the position by reflection call the hidden method 
			_positionTime = (DateTime)mi.Invoke(this.ChartControl, new object[]{ e.X});
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
			//Cleaning the chart before we start in any way we will clean it but if you want to get the new ones on the chart
			ClearOverChart();
			//Getting item from the context menu
		    ContextMenuStrip menuStrip = ChartControl.ContextMenuStrip;
			//Getting Items from contex menu
		    ToolStripItemCollection menuItems = menuStrip.Items;
			//Now getting item with Drawing Tool SubMenu
		    ToolStripMenuItem toolStripMenuItem = menuItems[19] as ToolStripMenuItem;
			//The item what we will use as over item with what we work with 
		    ToolStripItem andrewForkItem =null;
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
			   return; 
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
			    if (base.ChartControl != null && fi.GetValue(base.ChartControl) != null)
			    {
					//Gettin the instance of the object
				    object clickedObject = fi.GetValue(base.ChartControl);
					//Checking if ti posible to convert
				    if (clickedObject is IAndrewsPitchfork)
				    {
						//Converting 
					    result = (IAndrewsPitchfork) clickedObject;
						//if we got the result at all 
					    if (result!=null)
					    {
							//Now we stay real one for moving
						    if (result.Tag != myTag)
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
			IAndrewsPitchfork newFork = DrawAndrewsPitchfork(myTag, true, fAnchor1Time, fAnchor1Y, fAnchor3Time, fAnchor3Y, fAnchor2Time, fAnchor2Y,
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
			MethodInfo methodNT = typeof(ChartControl).GetMethod("RemoveDrawingObjectsAfterBar", bfObject);
			methodNT.Invoke(this.ChartControl, new Object[] { 0 });
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
			    double _secondPositionY = anchor2Y - anchor1Y;
			    TimeSpan _secondPositionX = anchor2Time.Subtract(anchor1Time);

				
				
				IRay ray2;
				double dy=(fork.Anchor2Y+fork.Anchor3Y)/2;
				DateTime dx;
				DateTime startX=anchor1Time;
				double startY=anchor1Y;
				int x1=0;int x2=0;
				if (fork.CalculationMethod==CalculationMethod.StandardPitchfork)
				{
					if (fork.Anchor3Time.Day!=fork.Anchor2Time.Day)
					{
						x1=ChartControl.GetXByTime(fork.Anchor3Time);	
						x2=ChartControl.GetXByTime(fork.Anchor2Time);	
						dx=Bars.GetTime(ConvertXtoBarIdx((x1+x2)/2));
					}
					else
						dx=ConvertTicksToDateTime( (long)((fork.Anchor3Time.Ticks+fork.Anchor2Time.Ticks)/2));
				}
				else if (fork.CalculationMethod==CalculationMethod.Schiff)
				{
					startY=(fork.Anchor1Y+fork.Anchor3Y)/2;
					if (fork.Anchor3Time.Day!=fork.Anchor2Time.Day)
					{
						x1=ChartControl.GetXByTime(fork.Anchor3Time);	
						x2=ChartControl.GetXByTime(fork.Anchor2Time);	
						dx=Bars.GetTime(ConvertXtoBarIdx((x1+x2)/2));
					}
					else
						dx=ConvertTicksToDateTime( (long)((fork.Anchor3Time.Ticks+fork.Anchor2Time.Ticks)/2));
				}
				else
				{
					if (fork.Anchor3Time.Day!=fork.Anchor2Time.Day)
					{
						x1=ChartControl.GetXByTime(fork.Anchor3Time);	
						x2=ChartControl.GetXByTime(fork.Anchor2Time);		
						dx=Bars.GetTime(ConvertXtoBarIdx((x1+x2)/2));
					}
					else
						dx=ConvertTicksToDateTime( (long)((fork.Anchor3Time.Ticks+fork.Anchor2Time.Ticks)/2));
					
					if (fork.Anchor3Time.Day!=fork.Anchor1Time.Day)
					{
						x1=ChartControl.GetXByTime(fork.Anchor3Time);	
						x2=ChartControl.GetXByTime(fork.Anchor1Time);	
						startX=Bars.GetTime(ConvertXtoBarIdx((x1+x2)/2));
					}
					else
						startX=ConvertTicksToDateTime( (long)((fork.Anchor3Time.Ticks+fork.Anchor1Time.Ticks)/2));
				}
				
				ray2=DrawRay("Ray2",false,startX,startY,dx,dy,Color.Green,DashStyle.Solid,3);
				ray2.Locked=false;
							
				//Drawing over ray
			    _oveRay = DrawRay("Ray", true, _positionTime, _positionPrice, _positionTime.Add(_secondPositionX),
				    _positionPrice + _secondPositionY, Color.Red, DashStyle.Solid, 2);

				//Making it drawable
			    _oveRay.Locked = false;

				//Cleaning after over self
				GC.Collect();
		    }
	    }

		public int ConvertXtoBarIdx(int x)
        {
            string _debug = "";
            if (ChartControl == null)
                return 0;

            int numBarsOnCanvas = 0;
            int idxFirstBar = 0;
            int idxLastBar = 0;

            if (ChartControl.LastBarPainted < BarsArray[0].Count)
            {
                numBarsOnCanvas = this.LastBarIndexPainted - this.FirstBarIndexPainted;
                idxFirstBar = this.FirstBarIndexPainted;
                idxLastBar = this.LastBarIndexPainted;
            }
            else
            {
                numBarsOnCanvas = BarsArray[0].Count - this.FirstBarIndexPainted;
                idxFirstBar = this.FirstBarIndexPainted;
                idxLastBar = BarsArray[0].Count - 1;
            }

            int firstBarX = ChartControl.GetXByBarIdx(BarsArray[0], idxFirstBar);
//			int	barWidth = ChartControl.ChartStyle.GetBarPaintWidth(Bars.BarsData.ChartStyle.BarWidthUI);
            int halfBarWidth = (int)Math.Round(((double)(ChartControl.BarWidth / (double)2)), 0, MidpointRounding.AwayFromZero);
            int margin = firstBarX + halfBarWidth;
            double ratio = 1 + ((x - margin) / (double)(ChartControl.BarSpace));
            int numberPeriods = (int)Math.Truncate(ratio);

            int barIndex = idxFirstBar + numberPeriods;
			barIndex = Math.Min(barIndex, idxLastBar);

            if (barIndex < 0)
                return 0;
	
			return barIndex;
        }
		
		static DateTime ConvertTicksToDateTime(long ticks)
          {
              DateTime datetime= new DateTime(ticks);
              return datetime;
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
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(myTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
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
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(myTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
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
				IAndrewsPitchfork newFork =  DrawAndrewsPitchfork(myTag,true,fork.Anchor1Time,fork.Anchor1Y,fork.Anchor3Time,fork.Anchor3Y,fork.Anchor2Time,fork.Anchor2Y,
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


	 //   #endregion

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
        public DataSeries Mdified_Schiff
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
    public partial class Indicator : IndicatorBase
    {
        private AAForkExtension[] cacheAAForkExtension = null;

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
            if (cacheAAForkExtension != null)
                for (int idx = 0; idx < cacheAAForkExtension.Length; idx++)
                    if (cacheAAForkExtension[idx].EqualsInput(input))
                        return cacheAAForkExtension[idx];

            lock (checkAAForkExtension)
            {
                if (cacheAAForkExtension != null)
                    for (int idx = 0; idx < cacheAAForkExtension.Length; idx++)
                        if (cacheAAForkExtension[idx].EqualsInput(input))
                            return cacheAAForkExtension[idx];

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

                AAForkExtension[] tmp = new AAForkExtension[cacheAAForkExtension == null ? 1 : cacheAAForkExtension.Length + 1];
                if (cacheAAForkExtension != null)
                    cacheAAForkExtension.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheAAForkExtension = tmp;
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

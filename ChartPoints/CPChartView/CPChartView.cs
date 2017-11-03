using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using CPTracerLib;

namespace ChartPoints
{

  public enum EChartViewMode
  {
    Drag = 1
    , Spy = 2
    , DragAndSpy = 3
    , Auto = 4
  }

  public partial class CPChartView : UserControl, ICPTracer
  {
    private Point startDragPoint;
    private IDictionary<ulong, IList<Tuple<ICPTracerDelegate, Series>> > serDelegates = new SortedDictionary<ulong, IList<Tuple<ICPTracerDelegate, Series>>>();
    private IDictionary<int, ICPTracerDelegate> depDelegates = new SortedDictionary<int, ICPTracerDelegate>();
    private Timer updateTimer;
    CPTableView traceConsumer;
    EChartViewMode mode = EChartViewMode.Auto;
    //ICPTracerDelegate unkTracer;

    public CPChartView()
    {
      InitializeComponent();
      this.Dock = DockStyle.Fill;
      this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.Dock = DockStyle.Top;
      chart.MouseDown += Chart_MouseDown;
      chart.MouseUp += Chart_MouseUp;
      chart.MouseMove += Chart_MouseMove;
      chart.MouseWheel += Chart_MouseWheel;

      RegProcessTracer(333, "test.exe");//!!! 
    }

    public void SetTraceConsumer(CPTableView _traceConsumer)
    {
      traceConsumer = _traceConsumer;
      traceConsumer.enableEvent += OnEnableTraceEnt;
      //unkTracer = CreateTracer(0, "UNKNOWN_TRACER");
    }

    private void OnEnableTraceEnt(EnableTraceEntEvArgs args)
    {
      EnableItem(args.id, args.instNum, args.flag);
    }

    EChartViewMode GetMode() { return mode; }

    private void Chart_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        chart.Cursor = Cursors.Hand;
        mode = mode | EChartViewMode.Drag;
        startDragPoint = e.Location;
      }
    }

    private void Chart_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        chart.Cursor = Cursors.Default;
        mode = mode ^ EChartViewMode.Drag;
      }
    }

    private void Chart_MouseMove(object sender, MouseEventArgs e)
    {
      if ((mode & EChartViewMode.Drag) == EChartViewMode.Drag)
      {
        //chart.Invoke((MethodInvoker)(() =>
        //{
        double dx1 = chart.ChartAreas[0].AxisX.PixelPositionToValue(Math.Max(Math.Min(e.X, chart.Size.Width - 1), 0));
        double dx2 = chart.ChartAreas[0].AxisX.PixelPositionToValue(Math.Max(Math.Min(startDragPoint.X, chart.Size.Width - 1), 0));
        double dy1 = chart.ChartAreas[0].AxisY.PixelPositionToValue(Math.Max(Math.Min(e.Y, chart.Size.Height - 1), 0));
        double dy2 = chart.ChartAreas[0].AxisY.PixelPositionToValue(Math.Max(Math.Min(startDragPoint.Y, chart.Size.Height - 1), 0));
        double dx = dx2 - dx1;
        double dy = dy2 - dy1;
        if (double.IsNaN(chart.ChartAreas[0].AxisX.ScaleView.Position))
        {
          chart.ChartAreas[0].AxisX.Minimum += dx;
          chart.ChartAreas[0].AxisX.Maximum += dx;
          chart.ChartAreas[0].AxisY.Minimum += dy;
          chart.ChartAreas[0].AxisY.Maximum += dy;
        }
        else
        {
          chart.ChartAreas[0].AxisX.ScaleView.Position += dx;
          chart.ChartAreas[0].AxisY.ScaleView.Position += dy;
        }
        startDragPoint = e.Location;
        //}));
      }
      //else
      if ((mode & EChartViewMode.Spy) == EChartViewMode.Spy)
      {
        Point mousePoint = new Point(e.X, e.Y);

        chart.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true);
        var xValue = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
        int serInd = 0;
        foreach (Series ser in chart.Series)
        {
          if (ser.Enabled)
          {
            DataPoint ltPnt = ser.Points.LastOrDefault((elem) => (elem.XValue < xValue));
            DataPoint gtPnt = ser.Points.FirstOrDefault((elem) => (elem.XValue > xValue));
            double ltDiff = double.MaxValue;
            double gtDiff = double.MaxValue;
            if (ltPnt != null)
              ltDiff = xValue - ltPnt.XValue;
            else
              ltDiff = double.NaN;
            if (gtPnt != null)
              gtDiff = gtPnt.XValue - xValue;
            DataPoint curPnt = null;
            if (!double.IsNaN(ltDiff))
            {
              if (!double.IsNaN(gtDiff))
              {
                if (ltDiff < gtDiff)
                  curPnt = ltPnt;
                else
                  curPnt = gtPnt;
              }
              else
                curPnt = ltPnt;
            }
            else if (!double.IsNaN(gtDiff))
              curPnt = gtPnt;
            if (curPnt != null)
            {
              //Debug.WriteLine(curPnt.YValues[0].ToString());
              ICPTracerDelegate deleg = null;
              if (depDelegates.TryGetValue(serInd, out deleg))
              {
                TraceEnt te = new TraceEnt();
                te.tm = (ulong)curPnt.XValue;
                te.val = curPnt.YValues[0];
                deleg.Trace(te.tm, te.val);
              }
            }
          }
          ++serInd;
        }
      }
    }

    private void UpdateView(object sender, EventArgs e)
    {
      if (chart.IsHandleCreated)
      {
        IAsyncResult asyncRes = chart.BeginInvoke((MethodInvoker)(() =>
        {
          foreach (KeyValuePair<ulong, IList<Tuple<ICPTracerDelegate, Series>>> val in serDelegates)
          {
            foreach(Tuple<ICPTracerDelegate, Series> deleg in val.Value)
              deleg.Item1.UpdateView();
          }
        }));
      }
    }

    private object traceLockObj = new object();

    public void Trace(ulong id, System.Array tms, System.Array vals)
    {
      lock (traceLockObj)
      {
        IList<Tuple<ICPTracerDelegate, Series>> delegs = null;
        if (serDelegates.TryGetValue(id, out delegs))
          delegs.ElementAt(delegs.Count - 1).Item1.Trace(tms, vals);
        //else
        //  unkTracer.Trace(tms, vals);
        if ((mode & EChartViewMode.Spy) != EChartViewMode.Spy)
          traceConsumer.Trace(id, tms, vals);
      }
    }

    public void EnableItem(ulong id, int instNum, bool flag)
    {
      IList<Tuple<ICPTracerDelegate, Series>> delegs = null;
      if (serDelegates.TryGetValue(id, out delegs))
        delegs.ElementAt(instNum).Item2.Enabled = flag;
    }

    private ICPTracerDelegate AddTracer(ulong id, string varName)
    {
      Series ser = chart.Series.Add(varName);
      ser.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;
      ser.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
      ser.ChartType = SeriesChartType.FastLine;
      ser.LegendText = varName;
      ICPTraceConsumer cons = new CPChartTraceConsumer(chart, ser);
      CPTracerDelegate cpDelegate = new CPTracerDelegate(cons);
      chart.ApplyPaletteColors();
      cpDelegate.properties.Add("color", ser.Color);
      IList<Tuple<ICPTracerDelegate, Series>> delegs = null;
      if (!serDelegates.TryGetValue(id, out delegs))
      {
        delegs = new List<Tuple<ICPTracerDelegate, Series>>();
        serDelegates.Add(id, delegs);
      }
      delegs.Add(new Tuple<ICPTracerDelegate, Series>(cpDelegate, ser));
      ICPTracerDelegate cpDepDelegate = traceConsumer.CreateTracer(id, varName);
      cpDepDelegate.SetProperty("color", ser.Color);
      depDelegates.Add(chart.Series.Count - 1, cpDepDelegate);

      if (updateTimer == null)
      {
        updateTimer = new Timer();
        updateTimer.Interval = 500;
        updateTimer.Tick += UpdateView;
        updateTimer.Start();
      }

      return cpDelegate;
    }

    public void Clear()
    {
      if (chart.InvokeRequired)
      {
        if (chart.IsHandleCreated)
        {
          chart.Invoke((MethodInvoker)(() =>
          {
            chart.Series.Clear();
          }));
        }
        else
          Console.WriteLine("");
      }
      else
        chart.Series.Clear();
      serDelegates.Clear();
      depDelegates.Clear();
      traceConsumer = null;
      if (updateTimer != null)
      {
        updateTimer.Stop();
        updateTimer = null;
      }
      FitToView(null, null);
    }

    public ICPTracerDelegate CreateTracer(ulong id, string varName)
    {
      ICPTracerDelegate cpDelegate = null;
      if (chart.InvokeRequired)
      {
        if (chart.IsHandleCreated)
        {
          chart.Invoke((MethodInvoker)(() =>
          {
            cpDelegate = AddTracer(id, varName);
          }));
        }
        else
          Console.WriteLine("");
      }
      else
        cpDelegate = AddTracer(id, varName);

      return cpDelegate;
    }

    private void FitToView(object sender, EventArgs e)
    {
      mode = EChartViewMode.Auto;
      chart.BeginInvoke((MethodInvoker)(() =>
      {
        this.spyBtn.Checked = false;
        if (chart.ChartAreas.Count > 0)
        {
          chart.ChartAreas[0].AxisX.Minimum = double.NaN;
          chart.ChartAreas[0].AxisX.Maximum = double.NaN;
          chart.ChartAreas[0].AxisY.Minimum = double.NaN;
          chart.ChartAreas[0].AxisY.Maximum = double.NaN;
          chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
          chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
        }
      }));
    }

    bool RegProcessTracer(int procID, string procName)
    {
      System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
      System.Windows.Forms.DataVisualization.Charting.Title chartTitle = new System.Windows.Forms.DataVisualization.Charting.Title();
      chartArea.Area3DStyle.Inclination = 15;
      chartArea.Area3DStyle.IsClustered = true;
      chartArea.Area3DStyle.IsRightAngleAxes = false;
      chartArea.Area3DStyle.Perspective = 10;
      chartArea.Area3DStyle.Rotation = 10;
      chartArea.Area3DStyle.WallWidth = 0;
      chartArea.AxisX.IsLabelAutoFit = false;
      chartArea.AxisX.LabelStyle.Format = "H:m:s:ff";
      chartArea.AxisX.LabelStyle.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
      chartArea.AxisX.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      chartArea.AxisY.IsLabelAutoFit = false;
      chartArea.AxisY.LabelStyle.Format = "#.##";
      chartArea.AxisY.LabelStyle.IntervalOffset = 0D;
      chartArea.AxisY.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      chartArea.CursorX.Interval = 1E-08D;
      chartArea.CursorX.IsUserEnabled = true;
      chartArea.CursorX.IsUserSelectionEnabled = true;
      chartArea.CursorX.LineColor = System.Drawing.Color.ForestGreen;
      chartArea.CursorX.SelectionColor = System.Drawing.Color.GreenYellow;
      chartArea.CursorY.Interval = 1E-08D;
      chartArea.CursorY.IsUserEnabled = true;
      chartArea.CursorY.IsUserSelectionEnabled = true;
      chartArea.CursorY.LineColor = System.Drawing.Color.ForestGreen;
      chartArea.CursorY.SelectionColor = System.Drawing.Color.GreenYellow;
      string chartName = "ChartArea" + procID.ToString();
      chartArea.Name = chartName;
      this.chart.ChartAreas.Add(chartArea);
      chartTitle.DockedToChartArea = chartName;
      chartTitle.Text = procName + "[" + procID.ToString() + "]";
      chartTitle.Name = "CATitle" + procID.ToString();
      this.chart.Titles.Add(chartTitle);

      return true;
    }
    private void SetSpyMode(object sender, EventArgs e)
    {
      if ((mode & EChartViewMode.Spy) == EChartViewMode.Spy)
        mode = mode ^ EChartViewMode.Spy;
      else
        mode = mode | EChartViewMode.Spy;
    }

    private void Zoom(AxisScaleView asv, double coeff)
    {
      double shift = coeff * (asv.ViewMaximum - asv.ViewMinimum);
      asv.Zoom(asv.ViewMinimum + shift, asv.ViewMaximum - shift);
    }

    private void OnYZoomIn(object sender, EventArgs e)
    {
      Zoom(chart.ChartAreas[0].AxisY.ScaleView, 0.05);
    }

    private void OnYZoomOut(object sender, EventArgs e)
    {
      Zoom(chart.ChartAreas[0].AxisY.ScaleView, -0.05 / 0.9);
    }

    private void OnXZoomIn(object sender, EventArgs e)
    {
      Zoom(chart.ChartAreas[0].AxisX.ScaleView, 0.05);
    }

    private void OnXZoomOut(object sender, EventArgs e)
    {
      Zoom(chart.ChartAreas[0].AxisX.ScaleView, -0.05 / 0.9);
    }

    private void OnXYZoomIn(object sender, EventArgs e)
    {
      OnYZoomIn(null, null);
      OnXZoomIn(null, null);
    }

    private void OnXYZoomOut(object sender, EventArgs e)
    {
      OnYZoomOut(null, null);
      OnXZoomOut(null, null);
    }

    private void Chart_MouseWheel(object sender, MouseEventArgs e)
    {
      double delta = 0.005 * ((int) (-e.Delta / SystemInformation.MouseWheelScrollDelta ));
      if (e.Delta > 0)
        delta /= 0.99;
      double dx1 = chart.ChartAreas[0].AxisX.PixelPositionToValue(Math.Max(Math.Min(e.X, chart.Size.Width - 1), 0));
      double dy1 = chart.ChartAreas[0].AxisY.PixelPositionToValue(Math.Max(Math.Min(e.Y, chart.Size.Height - 1), 0));
      Zoom(chart.ChartAreas[0].AxisX.ScaleView, delta);
      Zoom(chart.ChartAreas[0].AxisY.ScaleView, delta);
      double dx2 = chart.ChartAreas[0].AxisX.PixelPositionToValue(Math.Max(Math.Min(e.X, chart.Size.Width - 1), 0));
      double dy2 = chart.ChartAreas[0].AxisY.PixelPositionToValue(Math.Max(Math.Min(e.Y, chart.Size.Height - 1), 0));
      double dx = dx1 - dx2;
      double dy = dy1 - dy2;
      if (double.IsNaN(chart.ChartAreas[0].AxisX.ScaleView.Position))
      {
        chart.ChartAreas[0].AxisX.Minimum += dx;
        chart.ChartAreas[0].AxisX.Maximum += dx;
        chart.ChartAreas[0].AxisY.Minimum += dy;
        chart.ChartAreas[0].AxisY.Maximum += dy;
      }
      else
      {
        chart.ChartAreas[0].AxisX.ScaleView.Position += dx;
        chart.ChartAreas[0].AxisY.ScaleView.Position += dy;
      }
      //chart.ChartAreas[0].AxisX.Minimum -= 1.0 * delta * dx;
      //chart.ChartAreas[0].AxisX.Maximum -= 1.0 * delta * dx;
      //chart.ChartAreas[0].AxisY.Minimum -= 1.0 * delta * dy;
      //chart.ChartAreas[0].AxisY.Maximum -= 1.0 * delta * dy;
      //chart.ChartAreas[0].AxisX.ScaleView.Scroll(chart.ChartAreas[0].AxisX.ScaleView.Position + 2.0 * delta * dx);
      //chart.ChartAreas[0].AxisY.ScaleView.Scroll(chart.ChartAreas[0].AxisY.ScaleView.Position + 2.0 * delta * dy);
    }

  }
}

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

namespace ChartPoints
{

  public enum EChartViewMode
  {
    Drag          = 1
    , Spy         = 2
    , DragAndSpy  = 3
    , Auto        = 4
  }

  public partial class CPChartView : UserControl, ICPTracer
  {
    private Point startDragPoint;
    private IDictionary<ulong, Tuple<ICPTracerDelegate, Series>> serDelegates = new SortedDictionary<ulong, Tuple<ICPTracerDelegate, Series>>();
    private IDictionary<int, ICPTracerDelegate> depDelegates = new SortedDictionary<int, ICPTracerDelegate>();
    private Timer updateTimer;
    CPTableView traceConsumer;
    EChartViewMode mode = EChartViewMode.Auto;

    public CPChartView()
    {
      InitializeComponent();
      this.Dock = DockStyle.Fill;
      this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.Dock = DockStyle.Top;
      chart.MouseDown += Chart_MouseDown;
      chart.MouseUp += Chart_MouseUp;
      chart.MouseMove += Chart_MouseMove;
    }

    public void SetTraceConsumer(CPTableView _traceConsumer)
    {
      traceConsumer = _traceConsumer;
      traceConsumer.enableEvent += OnEnableTraceEnt;
    }

    private void OnEnableTraceEnt(EnableTraceEntEvArgs args)
    {
      EnableItem(args.id, args.flag);
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
      if ( (mode & EChartViewMode.Drag) == EChartViewMode.Drag)
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
        foreach(Series ser in chart.Series)
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
                deleg.Trace(curPnt.YValues[0]);
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
          foreach (KeyValuePair<ulong, Tuple<ICPTracerDelegate, Series>> val in serDelegates)
            val.Value.Item1.UpdateView();
        }));
      }
    }

    public void Trace(ulong id, double val)
    {
      Tuple<ICPTracerDelegate, Series> deleg = null;
      if (serDelegates.TryGetValue(id, out deleg))
        deleg.Item1.Trace(val);
      if ((mode & EChartViewMode.Spy) != EChartViewMode.Spy)
        traceConsumer.Trace(id, val);
    }

    public void EnableItem(ulong id, bool flag)
    {
      Tuple<ICPTracerDelegate, Series> deleg = null;
      if (serDelegates.TryGetValue(id, out deleg))
        deleg.Item2.Enabled = flag;
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
      serDelegates.Add(id, new Tuple<ICPTracerDelegate, Series>(cpDelegate, ser));

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
        chart.ChartAreas[0].AxisX.Minimum = double.NaN;
        chart.ChartAreas[0].AxisX.Maximum = double.NaN;
        chart.ChartAreas[0].AxisY.Minimum = double.NaN;
        chart.ChartAreas[0].AxisY.Maximum = double.NaN;
        chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
        chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
      }));
    }

    private void SetSpyMode(object sender, EventArgs e)
    {
      if ((mode & EChartViewMode.Spy) == EChartViewMode.Spy)
        mode = mode ^ EChartViewMode.Spy;
      else
        mode = mode | EChartViewMode.Spy;
    }
  }
}

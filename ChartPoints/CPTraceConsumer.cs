using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CPTracerLib;

namespace ChartPoints
{
  internal class CPTracerDelegate : ICPTracerDelegate
  {
    public System.Data.PropertyCollection properties { get; set; } = new System.Data.PropertyCollection();

    private ICPTraceConsumer cons;

    public void SetProperty(string key, object value)
    {
      properties[key] = value;
      cons.SetProperty(key, value);
    }

    public CPTracerDelegate(ICPTraceConsumer _cons)
    {
      cons = _cons;
    }

    public void Trace(System.Array tms, System.Array vals)
    {
      cons.Trace(tms, vals);
    }

    public void Trace(ulong tm, double val)
    {
      cons.Trace(tm, val);
    }

    public void UpdateView()
    {
      cons.UpdateView();
    }
    //public double GetTmMin()
    //{
    //  return cons.GetTmMin();
    //}

    //public double GetTmMax()
    //{
    //  return cons.GetTmMax();
    //}

  }

  public class CPChartTraceConsumer : ICPTraceConsumer
  {
    private Series series;
    private Control ctrl;
    //private int tm = 0;
    private IList<double> tms_1 = new List<double>();
    private IList<double> vals_1 = new List<double>();
    private IList<double> tms_2 = new List<double>();
    private IList<double> vals_2 = new List<double>();
    private IList<double> tms_in, tms_out, vals_in, vals_out;
    //private int ind_0 = 0;
    //private int ind_1 = 0;

    public CPChartTraceConsumer(Control _ctrl, Series _series)
    {
      ctrl = _ctrl;
      series = _series;
      tms_in = tms_1;
      tms_out = tms_2;
      vals_in = vals_1;
      vals_out = vals_2;
    }

    public void SetProperty(string key, object value)
    {}

    public void Trace(System.Array tms, System.Array vals)
    {
      lock (tms_in)
      {
        for (int i = 0; i < tms.Length; ++i)
        {
          tms_in.Add((ulong)tms.GetValue(i));
          vals_in.Add((double)vals.GetValue(i));
        }
      }
    }

    public void Trace(ulong tm, double val)
    {
      lock (tms_in)
      {
        tms_in.Add(tm);
        vals_in.Add(val);
      }
    }

    //public void Trace(TraceEnt traceEnt)
    //{
    //  lock (tms_in)
    //  {
    //    tms_in.Add(traceEnt.tm);
    //    vals_in.Add(traceEnt.val);
    //  }
    //}

    public void UpdateView()
    {
      lock (tms_in)
      {
        IList<double> tms_temp = tms_in;
        tms_in = tms_out;
        tms_out = tms_temp;
        IList<double> vals_temp = vals_in;
        vals_in = vals_out;
        vals_out = vals_temp;
      }
      for (int i = 0; i < tms_out.Count; ++i)
      {
        TimeSpan tmSpan = TimeSpan.FromMilliseconds(tms_out[i]);
        if (series.Points.Count != 0)
          series.Points.AddXY(tmSpan.TotalHours, series.Points.ElementAt(series.Points.Count - 1).YValues[0]);
        series.Points.AddXY(tmSpan.TotalHours, vals_out[i]);
      }
      tms_out.Clear();
      vals_out.Clear();
    }

  }

  public class CPTableTraceConsumer : ICPTraceConsumer
  {
    private DataGridViewRow row;
    private Control ctrl;
    private double curVal;
    bool enabled = true;

    public void SetProperty(string key, object value)
    {
      if (key == "color")
      {
        Color color = Color.Empty;
        if (value != null)
          color = (Color)value;
        if (!color.IsEmpty)
          row.Cells[1].Style.BackColor = color;
      }
      else if (key == "enable")
        enabled = (bool)value;
    }

    public CPTableTraceConsumer(Control _ctrl, DataGridViewRow _row)
    {
      ctrl = _ctrl;
      row = _row;
    }

    public void Trace(System.Array tms, System.Array vals)
    {
      curVal = (double)vals.GetValue( vals.Length - 1 );
    }

    public void Trace(ulong tm, double val)
    {
      curVal = val;
    }

    //public void Trace(TraceEnt traceEnt)
    //{
    //  curVal = traceEnt.val;
    //}

    public void UpdateView()
    {
      if (enabled)
        row.Cells[3/*CPTableView.ValueCellInd*/].Value = curVal;
    }

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartPoints
{
  internal class CPTracerDelegate : ICPTracerDelegate
  {
    private ICPTraceConsumer cons;

    public CPTracerDelegate(ICPTraceConsumer _cons)
    {
      cons = _cons;
    }
    public void Trace(double val)
    {
      cons.Trace((int)val);
    }

    public void UpdateView()
    {
      cons.UpdateView();
    }
  }

  public class CPTraceConsumer : ICPTraceConsumer
  {
    private Series series;
    private Control ctrl;
    private int tm = 0;
    private IList<double> tms = new List<double>();
    private IList<double> vals = new List<double>();

    public CPTraceConsumer(Control _ctrl, Series _series)//, string varName)
    {
      ctrl = _ctrl;
      series = _series;
      ctrl.Invoke((MethodInvoker)(() => series.Points.DataBindXY(tms, vals)));
      //ctrl.Invoke((MethodInvoker)(() => series.LegendText = varName));
      //setNameCPEvent += DelegateSetName;
      //EventHandler<CPSetNameEventArgs> temp = setNameCPEvent;
      //if(temp != null)
      //  temp(this, new CPSetNameEventArgs(varName));
      //addCPEvent += DelegateTrace;
    }

    public void Trace(double val)
    {
      tms.Add(tm++);
      vals.Add(val);
      //addCPEvent.Invoke(this, new CPTraceEventArgs(val));
      //ctrl.Invoke((MethodInvoker)(() => series.Points.AddXY(tm++, val)));
    }

    public void UpdateView()
    {
      ctrl.Invoke((MethodInvoker)(() => series.Points.DataBindXY(tms, vals)));
    }

  }
}

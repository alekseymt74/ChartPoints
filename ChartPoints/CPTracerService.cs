using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public class CPTracerService : ICPTracerService
  {
    CPChartViewTWCmd cpViewTW;
    CPTableViewTWCmd cpTableTW;

    public CPTracerService()
    {
      cpViewTW = CPChartViewTWCmd.Instance;
      cpTableTW = CPTableViewTWCmd.Instance;
    }

    public ICPTracerDelegate RegTraceEnt(ulong id, string name)
    {
      return cpViewTW.CreateTracer(id, name);
    }

    public void Trace(ulong id, System.Array tms, System.Array vals)
    {
      cpViewTW.Trace(id, tms, vals);
    }

    public void Activate()
    {
      cpViewTW.Activate();
      cpTableTW.Activate();
      cpViewTW.SetTraceConsumer(cpTableTW.GetTraceConsumer());
    }

    public void Show()
    {
      cpViewTW.Show();
      cpTableTW.Show();
    }
  }

}

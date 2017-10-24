using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public class CPTracerService : ICPTracerService
  {
    ChartPointsViewTWCommand cpViewTW;

    public CPTracerService()
    {
      cpViewTW = ChartPointsViewTWCommand.Instance;
    }
    public ICPTracerDelegate RegTraceEnt(ulong id, string name)
    {
      return cpViewTW.CreateTracer(name);
    }

    public void Trace(ulong id, double val)
    {
      ;
    }

  }

}

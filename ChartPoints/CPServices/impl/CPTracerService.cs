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
    private IDictionary<string, int> duplNames = new SortedDictionary<string, int>();
    private object regLockObj = new object();

    public CPTracerService()
    {
      cpViewTW = CPChartViewTWCmd.Instance;
      cpTableTW = CPTableViewTWCmd.Instance;
    }

    public ICPTracerDelegate RegTraceEnt(ulong id, string name)
    {
      ICPTracerDelegate deleg = null;
      lock (regLockObj)
      {
        int numDuplicates = 0;
        if (duplNames.TryGetValue(name, out numDuplicates))
          duplNames[name] = ++numDuplicates;
        else
          duplNames.Add(name, numDuplicates);

        deleg = cpViewTW.CreateTracer(id, name + " [" + numDuplicates.ToString() + "]");
      }

      return deleg;
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
      duplNames.Clear();
    }

    public void Show()
    {
      cpViewTW.Show();
      cpTableTW.Show();
    }
  }

}

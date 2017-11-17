using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public class CPProcTracer : ICPProcessTracer
  {
    public ulong id { get; }
    public string name { get; }
    private CPChartViewTWCmd cpViewTW;
    private IDictionary<string, int> duplNames = new SortedDictionary<string, int>();
    private object regLockObj = new object();

    public CPProcTracer(ulong _id, string _name)
    {
      id = _id;
      name = _name;
      cpViewTW = CPChartViewTWCmd.Instance;
    }

    public ICPTracerDelegate RegTraceEnt(ulong elem_id, string elem_name)
    {
      ICPTracerDelegate deleg = null;
      lock (regLockObj)
      {
        int numDuplicates = 0;
        if (duplNames.TryGetValue(elem_name, out numDuplicates))
          duplNames[elem_name] = ++numDuplicates;
        else
          duplNames.Add(elem_name, numDuplicates);

        deleg = cpViewTW.CreateTracer(elem_id, name + "[" + id.ToString() + "]:" + elem_name + " [" + numDuplicates.ToString() + "]");
      }

      return deleg;
    }

    public void Trace(ulong id, System.Array tms, System.Array vals)
    {
      cpViewTW.Trace(id, tms, vals);
    }

    public void Clear()
    {
      duplNames.Clear();
    }
  }

  public class CPTracerService : ICPTracerService
  {
    CPChartViewTWCmd cpViewTW;
    CPTableViewTWCmd cpTableTW;
    ISet<ICPProcessTracer> procTracers = new SortedSet<ICPProcessTracer>(Comparer<ICPProcessTracer>.Create((lh, rh) => (lh.id.CompareTo(rh.id))));

    public CPTracerService()
    {
      cpViewTW = CPChartViewTWCmd.Instance;
      cpTableTW = CPTableViewTWCmd.Instance;
    }

    public void RegProcTracer(ulong id, string name, out ICPProcessTracer tracer)
    {
      Activate();
      tracer = procTracers.FirstOrDefault((t) => (t.id == id));
      if(tracer == null)
        tracer = new CPProcTracer(id, name);
    }

    public void Activate()
    {
      if(cpViewTW == null)
        cpViewTW = CPChartViewTWCmd.Instance;
      //cpViewTW.Activate();
      if(cpTableTW == null)
        cpTableTW = CPTableViewTWCmd.Instance;
      cpTableTW.Activate();
      cpViewTW.SetTraceConsumer(cpTableTW.GetTraceConsumer());
      foreach (ICPProcessTracer pt in procTracers)
        pt.Clear();
    }

    public void Show()
    {
      cpViewTW.Show();
      cpTableTW.Show();
    }
  }

}

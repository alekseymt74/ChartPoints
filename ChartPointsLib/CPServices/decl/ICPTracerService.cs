using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPTracerLib;

namespace CPTracerLib
{
  public struct TraceEnt
  {
    public ulong tm;
    public double val;
  }
}

namespace ChartPoints.CPServices.decl
{

  public interface ICPProcessTracer
  {
    ulong id { get; }
    string name { get; }
    ICPTracerDelegate RegTraceEnt(ulong id, string name);
    void Trace(ulong id, System.Array tms, System.Array vals);
    void Clear();
  }

  public interface ICPTracerService : ICPService
  {
    void RegProcTracer(ulong id, string name, out ICPProcessTracer tracer);
    void Activate();
    void Show();
  }

  public interface ICPTraceConsumer
  {
    void SetProperty(string key, object value);
    void Trace(System.Array tms, System.Array vals);
    void Trace(ulong tm, double val);
    void UpdateView();
  }

  public interface ICPTracer
  {
    ICPTracerDelegate CreateTracer(ulong id, string varName);
    void Trace(ulong id, System.Array tms, System.Array vals);
    //void EnableItem(ulong id, bool flag);
  }

  public interface ICPTracerDelegate
  {
    System.Data.PropertyCollection properties { get; }
    void SetProperty(string key, object value);
    void Trace(System.Array tms, System.Array vals);
    void Trace(ulong tm, double val);
    void UpdateView();
  }
}

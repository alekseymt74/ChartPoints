using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public interface ICPTracerService : ICPService
  {
    ICPTracerDelegate RegTraceEnt(ulong id, string name);
    void Trace(ulong id, double val);
    void Activate();
  }

  public interface ICPTraceConsumer
  {
    void SetProperty(string key, object value);
    void Trace(double val);
    void UpdateView();
  }

  public interface ICPTracer
  {
    ICPTracerDelegate CreateTracer(ulong id, string varName);
    void Trace(ulong id, double val);
    //void EnableItem(ulong id, bool flag);
  }

  public interface ICPTracerDelegate
  {
    System.Data.PropertyCollection properties { get; }
    void SetProperty(string key, object value);
    void Trace(double val);
    void UpdateView();
  }
}

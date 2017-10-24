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
  }

  public interface ICPTraceConsumer
  {
    void Trace(double val);
    void UpdateView();
  }

  public interface ICPTracer
  {
    void Activate();
    void UpdateView();
    void Show();
    ICPTracerDelegate CreateTracer(string varName);
    //ICPTracerDelegate CreateTracer(ulong id, string varName);
    //void Trace(ulong id, double val);
    //void EnableItem(ulong id, bool flag);
  }

  public interface ICPTracerDelegate
  {
    void Trace(double val);
    void UpdateView();
  }

}

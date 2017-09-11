using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
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
  }

  public interface ICPTracerDelegate
  {
    void Trace(double val);
    void UpdateView();
  }
}

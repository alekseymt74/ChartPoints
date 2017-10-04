using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public delegate void OnCPEvent<T>( T args );
  public interface ICPEvent<T>
  {
    event OnCPEvent<T> On;
    void Fire( T args );
  }

  public interface IConstructEventArgs<T>
  {
    T obj { get; }
  }

  public interface ICPEventProvider<T>
  {
    T prov { get; }
  }

  public interface ICPEventService
  {
    void RegisterConstructEventProvider(ICPEventProvider<IConstructEvents> evProv);
    bool GetConstructEventProvider( out ICPEventProvider<IConstructEvents> evProv );
  }

  //////////////////////////////////////////////////////////////////////////

  public interface IConstructEvents
  {
    ICPEvent<IConstructEventArgs<IChartPoint>> createdCPEvent { get; }
    ICPEvent<IConstructEventArgs<ILineChartPoints>> createdLineCPsEvent { get; }
    ICPEvent<IConstructEventArgs<IFileChartPoints>> createdFileCPsEvent { get; }
    ICPEvent<IConstructEventArgs<IProjectChartPoints>> createdProjCPsEvent { get; }
  }

}

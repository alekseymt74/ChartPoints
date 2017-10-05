using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public delegate void OnCPEvent<T>( T args );
  public abstract class ICPEvent<T>
  {
    protected abstract ICPEvent<T> Add(OnCPEvent<T> cb);
    public static ICPEvent<T> operator +(ICPEvent<T> me, OnCPEvent<T> cb)
    {
      return me.Add(cb);
    }
    protected abstract ICPEvent<T> Sub(OnCPEvent<T> cb);
    public static ICPEvent<T> operator -(ICPEvent<T> me, OnCPEvent<T> cb)
    {
      return me.Sub(cb);
    }
    public abstract void Fire(T args);
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
    ICPEvent<IConstructEventArgs<IChartPoint>> createdCPEvent { get; set; }
    ICPEvent<IConstructEventArgs<ILineChartPoints>> createdLineCPsEvent { get; set; }
    ICPEvent<IConstructEventArgs<IFileChartPoints>> createdFileCPsEvent { get; set; }
    ICPEvent<IConstructEventArgs<IProjectChartPoints>> createdProjCPsEvent { get; set; }
  }

}

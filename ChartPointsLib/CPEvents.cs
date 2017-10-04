using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public class CPEvent<T> : ICPEvent<T>
  {
    private OnCPEvent<T> _on;
    private List<T> history = new List<T>();
    public event OnCPEvent<T> On
    {
      add
      {
        if (value == null)
          return;
        lock (history)
        {
          if (history.Count > 0)
          {
            foreach (T evData in history)
              value.Invoke(evData);
          }
        }
        if( _on == null )
          _on = new OnCPEvent<T>( value );
        else
          _on += value;
      }
      remove
      {
        if( _on != null )
          _on -= value;
      }
    }

    public void Fire( T args )
    {
      if (_on == null)
      {
        lock (history)
        {
          history.Add(args);
        }
      }
      else
        _on.Invoke( args );
    }
  }

  public class CPEventProvider<T> : ICPEventProvider<T>
  {
    private T theProv;
    public T prov { get { return theProv; } }

    public CPEventProvider(T _prov)
    {
      theProv = _prov;
    }
  }


  public class CPEventService : ICPEventService
  {
    private ICPEventProvider<IConstructEvents> evConstrProv;
    public void RegisterConstructEventProvider(ICPEventProvider<IConstructEvents> evProv)
    {
      evConstrProv = evProv;
    }
    public bool GetConstructEventProvider(out ICPEventProvider<IConstructEvents> evProv)
    {
      evProv = evConstrProv;
      if( evConstrProv != null)
        return true;
      return false;
    }
  }

  public class ConstructEvents : IConstructEvents
  {
    public ICPEvent<IConstructEventArgs<IChartPoint>> createdCPEvent { get; } = new CPEvent<IConstructEventArgs<IChartPoint>>();
    public ICPEvent<IConstructEventArgs<ILineChartPoints>> createdLineCPsEvent { get; } = new CPEvent<IConstructEventArgs<ILineChartPoints>>();
    public ICPEvent<IConstructEventArgs<IFileChartPoints>> createdFileCPsEvent { get; } = new CPEvent<IConstructEventArgs<IFileChartPoints>>();
    public ICPEvent<IConstructEventArgs<IProjectChartPoints>> createdProjCPsEvent { get; } = new CPEvent<IConstructEventArgs<IProjectChartPoints>>();
  }

  public class ConstructEventArgs<T> : IConstructEventArgs<T>
  {
    public T obj { get; }

    public ConstructEventArgs(T _obj)
    {
      obj = _obj;
    }
  }

  //#####################

  public class CPLineEvArgs
  {
    public ILineChartPoints lineCPs { get; }
    public IChartPoint cp { get; }

    public CPLineEvArgs( ILineChartPoints _lineCPs, IChartPoint _cp )
    {
      lineCPs = _lineCPs;
      cp = _cp;
    }
  }

  public class CPFileEvArgs
  {
    public IFileChartPoints fileCPs { get; }
    public ILineChartPoints lineCPs { get; }

    public CPFileEvArgs( IFileChartPoints _fileCPs, ILineChartPoints _lineCPs )
    {
      fileCPs = _fileCPs;
      lineCPs = _lineCPs;
    }
  }

  public class CPProjEvArgs
  {
    public IProjectChartPoints projCPs { get; }
    public IFileChartPoints fileCPs { get; }

    public CPProjEvArgs( IProjectChartPoints _projCPs, IFileChartPoints _fileCPs )
    {
      projCPs = _projCPs;
      fileCPs = _fileCPs;
    }
  }

  //#####################
  public class CPEntTrackerArgs
  {
    public ICPEntTracker entTracker { get; }
    public CPEntTrackerArgs( ICPEntTracker _entTracker )
    {
      entTracker = _entTracker;
    }
  }

  public class FileTrackerArgs
  {
    public IFileTracker fileTracker { get; }
    public FileTrackerArgs( IFileTracker _fileTracker )
    {
      fileTracker = _fileTracker;
    }
  }

  //#####################
  public class ClassVarElemTrackerArgs
  {
    //public ICPEntTracker entTracker { get; }
    //public CPEntTrackerArgs(ICPEntTracker _entTracker)
    //{
    //  entTracker = _entTracker;
    //}
  }

}

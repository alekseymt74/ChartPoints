using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public class CPEvent<T> : ICPEvent<T>
  {
    private List<T> history = new List<T>();

    private OnCPEvent<T> _event;

    protected override ICPEvent<T> Add(OnCPEvent<T> cb)
    {
      lock (history)
      {
        if (history.Count > 0)
        {
          foreach (T evData in history)
            cb.Invoke(evData);
        }
      }
      _event += cb;

      return this;
    }

    protected override ICPEvent<T> Sub(OnCPEvent<T> cb)
    {
      _event -= cb;

      return this;
    }
    public override void Fire(T args)
    {
      lock (history)
      {
        history.Add(args);
      }
      if (_event != null)
        _event.Invoke(args);
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
    private ConstructEvents constrEvents;

    public IConstructEvents GetConstructEvents()
    {
      if (constrEvents == null)
        constrEvents = new ConstructEvents();
      return constrEvents;
    }
  }

  public class ConstructEvents : IConstructEvents
  {
    public ICPEvent<IConstructEventArgs<IChartPoint>> createdCPEvent { get; set; } = new CPEvent<IConstructEventArgs<IChartPoint>>();
    public ICPEvent<IConstructEventArgs<ILineChartPoints>> createdLineCPsEvent { get; set; } = new CPEvent<IConstructEventArgs<ILineChartPoints>>();
    public ICPEvent<IConstructEventArgs<IFileChartPoints>> createdFileCPsEvent { get; set; } = new CPEvent<IConstructEventArgs<IFileChartPoints>>();
    public ICPEvent<IConstructEventArgs<IFileChartPoints>> deletedFileCPsEvent { get; set; } = new CPEvent<IConstructEventArgs<IFileChartPoints>>();
    public ICPEvent<IConstructEventArgs<IProjectChartPoints>> createdProjCPsEvent { get; set; } = new CPEvent<IConstructEventArgs<IProjectChartPoints>>();
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

  public class CPStatusEvArgs
  {
    public IChartPoint cp { get; }

    public CPStatusEvArgs(IChartPoint _cp)
    {
      cp = _cp;
    }
  }

  public class LineCPStatusEvArgs
  {
    public ILineChartPoints lineCPs { get; }

    public LineCPStatusEvArgs(ILineChartPoints _lineCPs)
    {
      lineCPs = _lineCPs;
    }
  }

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

  public class CPLineMoveEvArgs
  {
    public ILineChartPoints lineCPs { get; }
    public int prevLine { get; }
    public int newLine { get; }

    public CPLineMoveEvArgs(ILineChartPoints _lineCPs, int _prevLine, int _newLine)
    {
      lineCPs = _lineCPs;
      prevLine = _prevLine;
      newLine = _newLine;
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

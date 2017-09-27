using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  //#####################
  public delegate void OnCPEvent<T>(T args);
  public interface ICPEvent<T>
  {
    event OnCPEvent<T> On;
    void Fire(T args);
  }

  public class CPEvent<T> : ICPEvent<T>
  {
    private OnCPEvent<T> _on;
    public event OnCPEvent<T> On
    {
      add
      {
        if (_on == null)
          _on = new OnCPEvent<T>(value);
        else
          _on += value;
      }
      remove
      {
        if (_on != null)
          _on -= value;
      }
    }

    public void Fire(T args)
    {
      _on?.Invoke(args);
    }
  }

  //#####################
  public class CPLineEvArgs
  {
    public ILineChartPoints lineCPs { get; }
    public IChartPoint cp { get; }

    public CPLineEvArgs(ILineChartPoints _lineCPs, IChartPoint _cp)
    {
      lineCPs = _lineCPs;
      cp = _cp;
    }
  }

  public class CPFileEvArgs
  {
    public IFileChartPoints fileCPs { get; }
    public ILineChartPoints lineCPs { get; }

    public CPFileEvArgs(IFileChartPoints _fileCPs, ILineChartPoints _lineCPs)
    {
      fileCPs = _fileCPs;
      lineCPs = _lineCPs;
    }
  }

  public class CPProjEvArgs
  {
    public IProjectChartPoints projCPs { get; }
    public IFileChartPoints fileCPs { get; }

    public CPProjEvArgs(IProjectChartPoints _projCPs, IFileChartPoints _fileCPs)
    {
      projCPs = _projCPs;
      fileCPs = _fileCPs;
    }
  }

  //#####################
  public class CPEntTrackerArgs
  {
    public ICPEntTracker entTracker { get; }
    public CPEntTrackerArgs(ICPEntTracker _entTracker)
    {
      entTracker = _entTracker;
    }
  }

  public class FileTrackerArgs
  {
    public IFileTracker fileTracker { get; }
    public FileTrackerArgs(IFileTracker _fileTracker)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{
  /// <summary>
  /// Statuses showing the ability to toggle chartpoint
  /// </summary>
  public enum ETargetPointStatus
  {
    Available       /// ready set chartpoint at specified position
    , NotAvailable  /// can't set chartpoint at specified position
    , SwitchedOn     /// chartpoint already exists and is active
    , SwitchedOff    /// chartpoint already exists and is inactive
  }

  public interface IChartPointData
  {
    bool enabled { get; }
    string varName { get; }
    ETargetPointStatus status { get; }
    string className { get; }
    ICPLineData lineData { get; }
  }

  public interface IData<TData>
  {
    TData data { get; }
  }

  public delegate void OnCPEvent<T>(T args);
  public interface ICPEvent<T>
  {
    event OnCPEvent<T> On;
    void Fire(T args);
  }

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

  /// <summary>
  /// Interface for chartpoint object
  /// </summary>
  public interface IChartPoint : IData<IChartPointData>
  {
    void CalcInjectionPoints(/*out */CPClassLayout cpInjPoints, string fname, int _lineNum, int _linePos);

    bool ValidatePosition(int lineNum, int linePos);
  }

  public interface ITextPosition
  {
    int lineNum { get; }
    int linePos { get; }
    void Move(IChartPoint cp, int _lineNum, int _linePos);
  }

  public interface ICPLineData
  {
    ITextPosition pos { get; }
    ICPFileData fileData { get; }
  }

  public interface ILineChartPoints : IData<ICPLineData>
  {
    ICPEvent<CPLineEvArgs> addCPEvent { get; }
    ICPEvent<CPLineEvArgs> remCPEvent { get; }
    ISet<IChartPoint> chartPoints { get; }
    int Count { get; }
    IChartPoint GetChartPoint(string varName);
    bool AddChartPoint(IChartPoint chartPnt);
    bool AddChartPoint(string varName, VCCodeClass ownerClass, out IChartPoint chartPnt, bool checkExistance = true);
    bool SyncChartPoints(string fname, ISet<string> cpVarNames, VCCodeClass className);
    bool ValidatePosition(int linesAdd);
  }

  public interface ICPFileData
  {
    string fileName { get; }
    string fileFullName { get; }
    ICPProjectData projData { get; }
    void Move(ILineChartPoints lcps, IChartPoint cp, int _lineNum, int _linePos);
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

  public interface IFileChartPoints : IData<ICPFileData>
  {
    ICPEvent<CPFileEvArgs> addCPLineEvent { get; }
    ICPEvent<CPFileEvArgs> remCPLineEvent { get; }
    ISet<ILineChartPoints> linePoints { get; }
    int Count { get; }
    ILineChartPoints GetLineChartPoints(int lineNum);
    ILineChartPoints AddLineChartPoints(int lineNum, int linePos);
    bool ValidatePosition(int lineNum, int linesAdd);
  }

  public interface ICPProjectData
  {
    string projName { get; }
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

  public interface IProjectChartPoints : IData<ICPProjectData>
  {
    ICPEvent<CPProjEvArgs> addCPFileEvent { get; }
    ICPEvent<CPProjEvArgs> remCPFileEvent { get; }
    ISet<IFileChartPoints> filePoints { get; }
    int Count { get; }
    IFileChartPoints GetFileChartPoints(string fname);
    ILineChartPoints GetFileLineChartPoints(string fname, int lineNum);
    IFileChartPoints AddFileChartPoints(string fileName, string fileFullName);
  }

  public interface ICheckPoint
  {
    bool SyncChartPoints(ISet<string> cpVarNames);
    void GetAvailableVars(out List<Tuple<string, string, bool>> _availableVars);
  }

}

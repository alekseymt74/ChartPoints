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
    //string fileName { get; }
    //string fileFullName { get; }
    //int lineNum { get; }
    //int linePos { get; }
    bool enabled { get; }
    string varName { get; }
    string className { get; }
  }

  /// <summary>
  /// Interface for chartpoint object
  /// </summary>
  public interface IChartPoint
  {
    ETargetPointStatus status { get; }
    /// <summary>
    /// Tries to toggle chartpoint. If changed from initial state (Available) -> adds it to IChartPointsProcessor container
    /// </summary>
    /// <returns>result of the operation (see ChartPoints::ETargetPointStatus)</returns>
    ETargetPointStatus Toggle();
    /// <summary>
    /// Tries to remove chartpoint. If changed from state (SwitchedOn / SwitchedOff) -> removes it from IChartPointsProcessor container
    /// </summary>
    /// <returns>result of the operation (see ChartPoints::ETargetPointStatus)</returns>
    ETargetPointStatus Remove();

    void CalcInjectionPoints(/*out */CPClassLayout cpInjPoints, string fname, int _lineNum, int _linePos);
    /// <summary>
    /// Cpp class variable
    /// </summary>
    //VCCodeVariable var { get; }
    IChartPointData data { get; }

    bool ValidatePosition(int lineNum, int linePos);
  }

  public interface ILineChartPoints
  {
    int lineNum { get; }
    int linePos { get; }
    ISet<IChartPoint> chartPoints { get; }
    IChartPoint GetChartPoint(string varName);
    bool AddChartPoint(string varName, VCCodeClass ownerClass, out IChartPoint chartPnt);
    bool SyncChartPoints(string fname, ISet<string> cpVarNames, VCCodeClass className);
    bool ValidatePosition(int linesAdd);
  }

  public interface IFileChartPoints
  {
    string fileName { get; }
    string fileFullName { get; }
    ISet<ILineChartPoints> linePoints { get; }
    ILineChartPoints GetLineChartPoints(int lineNum);
    ILineChartPoints AddLineChartPoints(int lineNum, int linePos);
    bool ValidatePosition(int lineNum, int linesAdd);
    //bool AddLineChartPoints(ILineChartPoints linePnts);
  }

  public interface IProjectChartPoints
  {
    string projName { get; }
    ISet<IFileChartPoints> filePoints { get; }
    IFileChartPoints GetFileChartPoints(string fname);
    ILineChartPoints GetFileLineChartPoints(string fname, int lineNum);
    IFileChartPoints AddFileChartPoints(string fileName, string fileFullName);
  }

  public interface ICheckPoint
  {
    //bool AddChartPoint(string varName);
    bool SyncChartPoints(ISet<string> cpVarNames);
    void GetAvailableVars(out List<Tuple<string, string, bool>> _availableVars);
  }

}

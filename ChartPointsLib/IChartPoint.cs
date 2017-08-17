using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    string fileName { get; }
    string fileFullName { get; }
    int lineNum { get; }
    int linePos { get; }
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

    void CalcInjectionPoints(out CPClassLayout cpInjPoints);
    void GetAvailableVars(out List<Tuple<string, string>> availableVars);
    /// <summary>
    /// Cpp class variable
    /// </summary>
    VCCodeVariable var { get; }
    IChartPointData data { get; }
  }

}

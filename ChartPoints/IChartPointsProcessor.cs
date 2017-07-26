﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
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
    /// <summary>
    /// Position of toggled chartpoint.
    /// Exact place for code injection will calculated later
    /// </summary>
    TextPoint pnt { get; }
    /// <summary>
    /// Cpp class variable
    /// </summary>
    VCCodeVariable var { get; }
  }

  /// <summary>
  /// Main interface for operating with chartpoints
  /// </summary>
  public interface IChartPointsProcessor
  {
    /// <summary>
    /// Container of all chartpoints set in current cpp project
    /// </summary>
    IDictionary<string, IDictionary<int, IChartPoint>> chartPoints { get; }
    /// <summary>
    /// Checks the ability to insert chartpoint at specified position
    /// </summary>
    /// <param name="pnt">cursor position in document</param>
    /// <returns></returns>
    IChartPoint Check(TextPoint pnt);
    /// <summary>
    /// Returns all chartpoints in specified file
    /// </summary>
    /// <param name="fileName">Name of the cpp file</param>
    /// <returns>chartpoints sorted by line numbers in specified file</returns>
    IDictionary<int, IChartPoint> GetFileChartPoints(string fileName);
  }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{
  /// <summary>
  /// Global elements. Initialized by ChartPointsPackage
  /// </summary>
  public class Globals
  {
    public static DTE dte { get; set; }

    public static IChartPointsProcessor processor { get; set; }
    public static ICPOrchestrator orchestrator { get; set; }

    public static IChartPointTagUpdater taggerUpdater { get; set; }
    public static IVsOutputWindow outputWindow { get; set; }
  }

  /// <summary>
  /// Singleton factory for all objects used in extension
  /// Factory instance initialized in implementation of the class
  /// Provides looseness of interacting objects by overloading of factory methods
  /// Also useful for tests (see ChartPointsTests::ChartPntFactoryStub)
  /// </summary>
  public class ChartPntFactory
  {
    /// <summary>
    /// Instance of the factory
    /// Can be set only in subclasses (implementations of the factory)
    /// </summary>
    protected static ChartPntFactory factory;

    protected ChartPntFactory() {}

    public static ChartPntFactory Instance
    {
      get { return factory; }
    }

    /// <summary>
    /// Creates main ChartPoints object
    /// </summary>
    /// <returns>IChartPointsProcessor</returns>
    public virtual IChartPointsProcessor CreateProcessor() {  return null; }
    /// <summary>
    /// Creates main ChartPoints object
    /// </summary>
    /// <returns>IChartPointsProcessor</returns>
    public virtual ICPOrchestrator CreateOrchestrator() { return null; }
    public virtual IProjectChartPoints CreateProjectChartPoint(string _projName, Func<IProjectChartPoints, bool> _addFunc, Func<IProjectChartPoints, bool> _remFunc) { return null; }
    public virtual IFileChartPoints CreateFileChartPoint(string _fileName, string _fileFullName, Func<IFileChartPoints, bool> _addFunc, Func<IFileChartPoints, bool> _remFunc) { return null; }
    public virtual ILineChartPoints CreateLineChartPoint(int _lineNum, int _linePos, Func<ILineChartPoints, bool> _addFunc, Func<ILineChartPoints, bool> _remFunc) { return null; }
    //public virtual IChartPoint CreateChartPoint(TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
    //      , VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc) { return null; }
    //public virtual IChartPoint CreateChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc) { return null; }
    public virtual IChartPoint CreateChartPoint(string varName, VCCodeClass ownerClass, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc) { return null; }
  }
}

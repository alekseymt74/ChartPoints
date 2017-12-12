using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;

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

    public static Type GetType<T>(T obj)
    {
      return typeof(T);
    }

    public static string GetTypeName<T>(T obj)
    {
      return typeof(T).ToString();
    }

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
    public virtual IProjectChartPoints CreateProjectChartPoint(string _projName) { return null; }
    public virtual IFileChartPoints CreateFileChartPoint(CP.Code.IFileElem _fileElem, ICPProjectData _projData) { return null; }
    public virtual ILineChartPoints CreateLineChartPoint(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData) { return null; }
    public virtual IChartPoint CreateChartPoint(CP.Code.IClassVarElement codeElem, ICPLineData _lineData) { return null; }
  }
}

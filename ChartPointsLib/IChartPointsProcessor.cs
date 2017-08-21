using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{
  public interface IChartPointsProcessorData
  {
    /// <summary>
    /// Container of all chartpoints set in current cpp project
    /// </summary>
    ISet<IProjectChartPoints> projPoints { get; }
  }

  /// <summary>
  /// Main interface for operating with chartpoints
  /// </summary>
  public interface IChartPointsProcessor
  {
    IChartPointsProcessorData data { get; }
    IProjectChartPoints GetProjectChartPoints(string projName);
    bool AddProjectChartPoints(string projName, out IProjectChartPoints pPnts);
    /// <summary>
    /// Checks the ability to insert chartpoint at specified position
    /// </summary>
    /// <param name="pnt">cursor position in document</param>
    /// <returns></returns>
    ICheckPoint Check(string projName, TextPoint pnt);

    //bool AddProjectChartPoints(IProjectChartPoints projPnts);

    /////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns all chartpoints in specified file
    /// </summary>
    /// <param name="fileName">Name of the cpp file</param>
    /// <returns>chartpoints sorted by line numbers in specified file</returns>
    //IDictionary<int, IChartPoint> GetFileChartPoints(string fileName);
    ////bool AddChartPoint(IChartPoint chartPnt);
    //bool AddChartPoint(IChartPointData chartPntData);
    //IChartPoint GetChartPoint(IChartPointData cpData);
    //IDictionary<int, IChartPoint> GetOrCreateFileChartPoints(string fname);
  }

  public interface IChartPointsTagger
  {
    void RaiseTagsChangedEvent(IFileChartPoints fPnts);
    void RaiseTagsChangedEvent(ILineChartPoints lPnts);
    bool GetFileName(out string fn);
  }

  public interface IChartPointTagUpdater
  {
    void AddTagger(IChartPointsTagger tagger);
    void RaiseChangeTagEvent(IFileChartPoints fPnts);
    void RaiseChangeTagEvent(string fname, ILineChartPoints lPnts);
  }
}

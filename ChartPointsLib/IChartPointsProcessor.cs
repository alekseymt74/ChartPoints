using System.Collections.Generic;

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
    bool HasData();
    IProjectChartPoints GetProjectChartPoints(string projName);
    bool AddProjectChartPoints(string projName, out IProjectChartPoints pPnts);
    bool RemoveChartPoints(string projName);
  }

}

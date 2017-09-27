using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public interface ICPEntTracker
  {
    ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; }
    void Validate(int lineNum, int linesAdd);
  }

  public interface IFileTracker
  {
    string fileFullName { get; }
    ICPEvent<FileTrackerArgs> emptyFTrackerEvent { get; }
    void Add(ICPEntTracker cpValidator);
    void Validate(int lineNum, int linesAdd);
  }

  public interface ICPTrackManager
  {
    ICPEvent<FileTrackerArgs> addFTrackerEvent { get; }
    ICPEvent<FileTrackerArgs> remFTrackerEvent { get; }
    IFileTracker GetFileTracker(string fileFullName);
    void Register(IChartPoint cp);
    void Register(ILineChartPoints lcp);
    void Register(IFileChartPoints fcp);
  }

}

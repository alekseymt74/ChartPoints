using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public interface ICPEntTracker
  {
    ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; set; }
    void Validate(int lineNum, int linesAdd);
  }

  public interface IFileTracker
  {
    string fileFullName { get; }
    ICPEvent<FileTrackerArgs> emptyFTrackerEvent { get; set; }
    void Add(ICPEntTracker cpValidator);
    void Validate(int lineNum, int linesAdd);
  }

  public interface ICPTrackManager
  {
    ICPEvent<FileTrackerArgs> addFTrackerEvent { get; set; }
    ICPEvent<FileTrackerArgs> remFTrackerEvent { get; set; }
    IFileTracker GetFileTracker(string fileFullName);
    void Register(IChartPoint cp);
    void Register(ILineChartPoints lcp);
    void Register(IFileChartPoints fcp);
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  //public class CPValidator
  //{
  //  private IChartPoint cp;
  //  private int lineNum;
  //  private int linePos;

  //  public CPValidator(IChartPoint _cp, int _lineNum, int _linePos)
  //  {
  //    cp = _cp;
  //    lineNum = _lineNum;
  //    linePos = _linePos;
  //  }
  //  public void Validate(int lineNum, int linesAdd)
  //  {
  //    bool valid = cp.ValidatePosition(lineNum + linesAdd, linePos);
  //  }
  //}

  public class FileCPValidator : ICPValidator
  {
    private IFileChartPoints fcps;

    public FileCPValidator()
    {
    }
    public void Validate(int lineNum, int linesAdd)
    {
      //bool valid = cp.ValidatePosition(lineNum + linesAdd, linePos);
    }
  }
  public interface IFileTracker
  {
    void Add(ICPValidator cpValidator);
    void Validate(int lineNum, int linesAdd);
  }
  public class FileTracker : IFileTracker
  {
    //public string fname;
    IList<ICPValidator> cps = new List<ICPValidator>();

    public void Add(ICPValidator cpValidator)
    {
      cps.Add(cpValidator);
    }

    public void Validate(int lineNum, int linesAdd)
    {
      foreach (ICPValidator cp in cps)
      {
        //cp.ValidatePosition(linesAdd);
      }
    }
  }

  public class CPTracker : ICPTracker
  {
    private IDictionary<string, IFileTracker> filesTrackers = new SortedDictionary<string, IFileTracker>();
    public void Add(string fileFullName, ICPValidator cpValidator)
    {
      IFileTracker fTracker = null;
      if (!filesTrackers.TryGetValue(fileFullName, out fTracker))
      {
        fTracker = new FileTracker();
        filesTrackers.Add(fileFullName, fTracker);
      }
      fTracker.Add(cpValidator);
    }
  }

}

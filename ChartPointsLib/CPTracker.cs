using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChartPoints
{

  public class CPTracker : ICPEntTracker
  {
    public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; } = new CPEvent<CPEntTrackerArgs>();
    private IChartPoint cp;

    public CPTracker(IChartPoint _cp)
    {
      cp = _cp;
    }
    public void Validate(int lineNum, int linesAdd)
    {
      //bool valid = cp.ValidatePosition(lineNum + linesAdd, linePos);
    }
  }

  public class LineCPTracker : ICPEntTracker
  {
    public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; } = new CPEvent<CPEntTrackerArgs>();
    private ILineChartPoints lcps;

    public LineCPTracker(ILineChartPoints _lcps)
    {
      lcps = _lcps;
      lcps.addCPEvent.On += OnAddCp;
      lcps.remCPEvent.On += OnRemCp;
    }

    private void OnRemCp(CPLineEvArgs args)
    {
      //if (args.lineCPs.Count == 0) // !!! POTENTIAL STATUS UPDATE
      {
        Globals.taggerUpdater.RaiseChangeTagEvent(lcps.data.fileData.fileFullName, lcps);
        //emptyCpEvent.Fire(new CPEntTrackerArgs(this));
      }
    }

    private void OnAddCp(CPLineEvArgs args)
    {
      //if (args.lineCPs.Count == 1) // !!! POTENTIAL STATUS UPDATE
        Globals.taggerUpdater.RaiseChangeTagEvent(lcps.data.fileData.fileFullName, lcps);
    }

    public void Validate(int lineNum, int linesAdd)
    {
      //bool valid = cp.ValidatePosition(lineNum + linesAdd, linePos);
    }
  }

  public class FileCPTracker : ICPEntTracker
  {
    public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; } = new CPEvent<CPEntTrackerArgs>();
    private IFileChartPoints fcps;

    public FileCPTracker(IFileChartPoints _fcps)
    {
      fcps = _fcps;
      fcps.addCPLineEvent.On += OnAddCpLine;
      fcps.remCPLineEvent.On += OnRemCpLine;
    }

    private void OnAddCpLine(CPFileEvArgs args)
    {
      //if (args.lineCPs.Count == 1)
      if(Globals.taggerUpdater != null)
        Globals.taggerUpdater.RaiseChangeTagEvent(fcps.data.fileFullName, args.lineCPs);
    }

    private void OnRemCpLine(CPFileEvArgs args)
    {
      //if (args.lineCPs.Count == 0)
      {
        Globals.taggerUpdater.RaiseChangeTagEvent(fcps.data.fileFullName, args.lineCPs);
        if(args.fileCPs.Count == 0)
          emptyCpEvent.Fire(new CPEntTrackerArgs(this));
      }
    }

    public void Validate(int lineNum, int linesAdd)
    {
      fcps.ValidatePosition(lineNum, linesAdd);
    }
  }

  public class FileTracker : IFileTracker
  {
    private Mutex mtx = new Mutex();
    private bool validation;
    public string fileFullName { get; set; }
    public ICPEvent<FileTrackerArgs> emptyFTrackerEvent { get; } = new CPEvent<FileTrackerArgs>();
    IList<ICPEntTracker> entTrackers = new List<ICPEntTracker>();

    public FileTracker(string _fileFullName)
    {
      validation = false;
      fileFullName = _fileFullName;
    }

    public void Add(ICPEntTracker cpValidator)
    {
      entTrackers.Add(cpValidator);
      cpValidator.emptyCpEvent.On += OnEmptyCpEvent;
    }

    private void OnEmptyCpEvent(CPEntTrackerArgs args)
    {
      if (validation)//mtx.WaitOne())
      {
        Task.Run(
          () =>
          {
            mtx.WaitOne();
            entTrackers.Remove(args.entTracker);
            if (entTrackers.Count == 0)
              emptyFTrackerEvent.Fire(new FileTrackerArgs(this));
            mtx.ReleaseMutex();
          });
      }
      else
      {
        entTrackers.Remove(args.entTracker);
        if (entTrackers.Count == 0)
          emptyFTrackerEvent.Fire(new FileTrackerArgs(this));
      }
    }

    public void Validate(int lineNum, int linesAdd)
    {
      mtx.WaitOne();
      validation = true;
      foreach (ICPEntTracker ent in entTrackers)
        ent.Validate(lineNum, linesAdd);
      validation = false;
      mtx.ReleaseMutex();
    }
  }

  public class CPTrackManager : ICPTrackManager
  {
    public ICPEvent<FileTrackerArgs> addFTrackerEvent { get; } = new CPEvent<FileTrackerArgs>();
    public ICPEvent<FileTrackerArgs> remFTrackerEvent { get; } = new CPEvent<FileTrackerArgs>();
    private ISet<IFileTracker> filesTrackers = new SortedSet<IFileTracker>(Comparer<IFileTracker>.Create((lh, rh) => (String.Compare(lh.fileFullName, rh.fileFullName, StringComparison.Ordinal))));
    protected IFileTracker AddFileTracker(string fileFullName)
    {
      IFileTracker fTracker = GetFileTracker(fileFullName);
      if (fTracker == null)
      {
        fTracker = new FileTracker(fileFullName);
        fTracker.emptyFTrackerEvent.On += OnEmptyCpEvent;
        filesTrackers.Add(fTracker);
      }

      return fTracker;
    }
    public IFileTracker GetFileTracker(string fileFullName)
    {
      return filesTrackers.FirstOrDefault((lp) => (lp.fileFullName == fileFullName));
    }

    private void OnEmptyCpEvent(FileTrackerArgs args)
    {
      IFileTracker fTracker = GetFileTracker(args.fileTracker.fileFullName);
      if (fTracker != null)
      {
        filesTrackers.Remove(args.fileTracker);
        remFTrackerEvent.Fire(new FileTrackerArgs(args.fileTracker));
      }
    }

    public void Register(IChartPoint cp)
    {
      //IFileTracker fTracker = AddFileTracker(cp.data.lineData.fileData.fileFullName);
      //ICPEntTracker cpValidator = new CPTracker(cp);
      //fTracker.Add(cpValidator);
    }

    public void Register(ILineChartPoints lcp)
    {
      IFileTracker fTracker = AddFileTracker(lcp.data.fileData.fileFullName);
      ICPEntTracker cpValidator = new LineCPTracker(lcp);
      fTracker.Add(cpValidator);
      addFTrackerEvent.Fire(new FileTrackerArgs(fTracker));
    }

    public void Register(IFileChartPoints fcp)
    {
      IFileTracker fTracker = AddFileTracker(fcp.data.fileFullName);
      ICPEntTracker cpValidator = new FileCPTracker(fcp);
      fTracker.Add(cpValidator);
      addFTrackerEvent.Fire(new FileTrackerArgs(fTracker));
    }
  }

}

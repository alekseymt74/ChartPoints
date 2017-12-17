using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChartPoints.CPServices.decl;

namespace ChartPoints.CPServices.impl
{

  public class CPTracker : ICPEntTracker
  {
    public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; set; } = new CPEvent<CPEntTrackerArgs>();
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

  //public class LineCPTracker : ICPEntTracker
  //{
  //  public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; set; } = new CPEvent<CPEntTrackerArgs>();
  //  private ILineChartPoints lcps;

  //  public LineCPTracker(ILineChartPoints _lcps)
  //  {
  //    lcps = _lcps;
  //    lcps.addCPEvent += OnAddCp;
  //    lcps.remCPEvent += OnRemCp;
  //  }

  //  private void OnRemCp(CPLineEvArgs args)
  //  {
  //    //if (args.lineCPs.Count == 0) // !!! POTENTIAL STATUS UPDATE
  //    {
  //      Globals.taggerUpdater?.RaiseChangeTagEvent(lcps.data.fileData.fileFullName, lcps);
  //      //emptyCpEvent.Fire(new CPEntTrackerArgs(this));
  //    }
  //  }

  //  private void OnAddCp(CPLineEvArgs args)
  //  {
  //    //if (args.lineCPs.Count == 1) // !!! POTENTIAL STATUS UPDATE
  //      Globals.taggerUpdater?.RaiseChangeTagEvent(lcps.data.fileData.fileFullName, lcps);
  //  }

  //  public void Validate(int lineNum, int linesAdd)
  //  {
  //    //bool valid = cp.ValidatePosition(lineNum + linesAdd, linePos);
  //  }
  //}

  public class FileCPTracker : ICPEntTracker
  {
    public ICPEvent<CPEntTrackerArgs> emptyCpEvent { get; set; } = new CPEvent<CPEntTrackerArgs>();
    private IFileChartPoints fcps;

    public FileCPTracker(IFileChartPoints _fcps)
    {
      fcps = _fcps;
      fcps.addCPLineEvent += OnAddCpLine;
      fcps.remCPLineEvent += OnRemCpLine;
    }

    private void OnAddCpLine(CPFileEvArgs args)
    {
    }

    private void OnRemCpLine(CPFileEvArgs args)
    {
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
    public ICPEvent<FileTrackerArgs> emptyFTrackerEvent { get; set; } = new CPEvent<FileTrackerArgs>();
    IList<ICPEntTracker> entTrackers = new List<ICPEntTracker>();

    public FileTracker(string _fileFullName)
    {
      validation = false;
      fileFullName = _fileFullName;
    }

    public void Add(ICPEntTracker cpValidator)
    {
      entTrackers.Add(cpValidator);
      cpValidator.emptyCpEvent += OnEmptyCpEvent;
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

  public class CPTrackService : ICPTrackService
  {
    public ICPEvent<FileTrackerArgs> addFTrackerEvent { get; set; } = new CPEvent<FileTrackerArgs>();
    public ICPEvent<FileTrackerArgs> remFTrackerEvent { get; set; } = new CPEvent<FileTrackerArgs>();
    private ISet<IFileTracker> filesTrackers = new SortedSet<IFileTracker>(Comparer<IFileTracker>.Create((lh, rh) => (String.Compare(lh.fileFullName, rh.fileFullName, StringComparison.Ordinal))));

    public CPTrackService()
    {
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      ICPEventService cpEvsService;
      cpServProv.GetService<ICPEventService>(out cpEvsService);
      IConstructEvents constrEvents = cpEvsService.GetConstructEvents();
      constrEvents.createdFileCPsEvent += OnFileCPsCreated;
      constrEvents.deletedFileCPsEvent += OnFileCPsDeleted;
    }

    protected IFileTracker AddFileTracker(string fileFullName)
    {
      IFileTracker fTracker = GetFileTracker(fileFullName);
      if (fTracker == null)
      {
        fTracker = new FileTracker(fileFullName);
        fTracker.emptyFTrackerEvent += OnEmptyCpEvent;
        filesTrackers.Add(fTracker);
      }

      return fTracker;
    }

    public IFileTracker GetFileTracker(string fileFullName)
    {
      return filesTrackers.FirstOrDefault((lp) => (lp.fileFullName == fileFullName));
    }

    void OnFileCPsCreated(IConstructEventArgs<IFileChartPoints> args)
    {
      IFileTracker fTracker = AddFileTracker(args.obj.data.fileFullName);
      ICPEntTracker cpValidator = new FileCPTracker(args.obj);
      fTracker.Add(cpValidator);
      addFTrackerEvent.Fire(new FileTrackerArgs(fTracker));
    }

    void OnFileCPsDeleted(IConstructEventArgs<IFileChartPoints> args)
    {
      RemoveFileTracker(args.obj.data.fileFullName);
    }

    private void OnEmptyCpEvent(FileTrackerArgs args)
    {
      RemoveFileTracker(args.fileTracker.fileFullName);
    }

    private bool RemoveFileTracker(string fileFullName)
    {
      IFileTracker fTracker = GetFileTracker(fileFullName);
      if (fTracker != null)
      {
        filesTrackers.Remove(fTracker);
        remFTrackerEvent.Fire(new FileTrackerArgs(fTracker));

        return true;
      }

      return false;
    }
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CP.Code;
using EnvDTE80;

namespace ChartPoints
{

  [DataContract]
  public class ChartPointData : IChartPointData
  {
    [DataMember]
    public bool enabled { get; set; }
    [DataMember]
    public string name { get; set; }
    public string uniqueName { get; set; }
    public string type { get; set; }
    public EChartPointStatus status { get; set; }
    public ICPLineData lineData { get; set; }
    public ChartPointData() { }
    public ChartPointData(IChartPointData _data)
    {
      enabled = _data.enabled;
      name = _data.name;
      uniqueName = _data.uniqueName;
      type = _data.type;
    }
  }

  public class Data<T, TData, TDataImpl> : IData<TData>
    where TData : class
    where TDataImpl : TData
  {
    public TDataImpl theData { get; set; }

    public TData data
    {
      get { return theData; }
      //set { theData = (TDataImpl)value; }
    }
  }

  /// <summary>
  /// Implementation of IChartPoint interface
  /// </summary>
  public class ChartPoint : Data<ChartPoint, IChartPointData, ChartPointData>, IChartPoint
  {
    private CP.Code.IClassVarElement codeElem;
    protected ChartPoint() { }

    public ChartPoint(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData)
    {
      codeElem = _codeElem;
      codeElem.classVarChangedEvent.On += ClassVarChangedEventOn;
      codeElem.classVarDeletedEvent.On += ClassVarDeletedEventOn;
      theData = new ChartPointData
      {
        enabled = true,
        status = EChartPointStatus.SwitchedOn,
        name = codeElem.name,
        uniqueName = codeElem.uniqueName/*varName*/,
        type = codeElem.type,
        lineData = _lineData
      };
    }

    private void ClassVarChangedEventOn(ClassVarElemTrackerArgs args)
    {
      ;
    }

    private void ClassVarDeletedEventOn(ClassVarElemTrackerArgs args)
    {
      ;
    }

    public EChartPointStatus SetStatus(EChartPointStatus newStatus)
    {
      EChartPointStatus curStatus = theData.status;
      theData.status = newStatus;

      return curStatus;
    }

    public virtual CPTraceVar CalcInjectionPoints(CPClassLayout cpClassLayout, string className, out bool needDeclare)
    {
      return codeElem.CalcInjectionPoints(cpClassLayout, className, data.lineData.fileData.fileName, data.lineData.pos, out needDeclare);
    }

    public bool ValidatePosition(int lineNum, int linePos)
    {
      //////vcCodeModel.Synchronize();
      //////CodeElement theClass = null;
      //////// find class, containing specified memeber
      //////foreach (CodeElement _class in vcCodeModel.Classes)
      //////{
      //////  if (_class.Name == data.className)
      //////  {
      //////    theClass = _class;
      //////    break;
      //////  }
      //////}
      //////if (theClass != null)
      //////{
      //////  try
      //////  {
      //////    VCCodeClass vcClass = (VCCodeClass) theClass;
      //////    //CodeElement theFunc = null;
      //////    foreach (CodeElement _func in vcClass.Functions)
      //////    {
      //////      VCCodeFunction vcFunc = (VCCodeFunction) _func;
      //////      //TextPoint startFuncBody = vcFunc.StartPoint;// GetStartPoint(vsCMPart.vsCMPartBodyWithDelimiter);//vcFunc.StartPointOf[vsCMPart.vsCMPartBodyWithDelimiter, vsCMWhere.vsCMWhereDefinition];
      //////      //TextPoint endFuncBody = vcFunc.EndPoint;// GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter);//vcFunc.EndPointOf[vsCMPart.vsCMPartBodyWithDelimiter, vsCMWhere.vsCMWhereDefinition];
      //////      TextPoint startFuncBody = vcFunc.StartPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
      //////      TextPoint endFuncBody = vcFunc.EndPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
      //////      EditPoint startPnt = startFuncBody.CreateEditPoint();
      //////      EditPoint endPnt = endFuncBody.CreateEditPoint();
      //////      startPnt.FindPattern("{", (int) vsFindOptions.vsFindOptionsBackwards);
      //////      endPnt.FindPattern("}");
      //////      //if (lineNum >= startPnt.Line && linePos >= startPnt.LineCharOffset && lineNum <= endPnt.Line && linePos <= endPnt.LineCharOffset)
      //////      if ((lineNum > startPnt.Line && lineNum < endPnt.Line) ||
      //////          (lineNum == startPnt.Line && linePos >= startPnt.LineCharOffset) ||
      //////          (lineNum == endPnt.Line && linePos <= endPnt.LineCharOffset))
      //////      {
      //////        // Oh, oh you're in the body, now.. (c)
      //////        return true;
      //////      }
      //////    }
      //////    //// find VCCodeVariable
      //////    //CodeElement theVar = null;
      //////    //foreach (CodeElement _var in vcClass.Variables)
      //////    //{
      //////    //  if (_var.Name == /*"j"*/ data.varName)
      //////    //  {
      //////    //    theVar = _var;
      //////    //    break;
      //////    //  }
      //////    //}
      //////    //if (theVar != null)
      //////    //{
      //////    //}
      //////  }
      //////  catch (Exception e)
      //////  {
      //////    Console.WriteLine(e);
      //////  }
      //////}
      return false;
    }
  }

  public class TextPosition : ITextPosition
  {
    public int lineNum { get; set; }
    public int linePos { get; set; }

    delegate void UpdatePosition(IChartPoint cp, int _lineNum, int _linePos);

    private UpdatePosition updPos;

    public TextPosition(int _lineNum, int _linePos, Action<IChartPoint, int, int> _updPos)
    {
      lineNum = _lineNum;
      linePos = _linePos;
      updPos = new UpdatePosition(_updPos);
    }
    public void Move(IChartPoint cp, int _lineNum, int _linePos)
    {
      updPos(cp, _lineNum, _linePos);
    }
  }

  public class CPLineData : ICPLineData
  {
    public ITextPosition pos { get; set; }
    public ICPFileData fileData { get; set; }
  }

  public class LineChartPoints : Data<LineChartPoints, ICPLineData, CPLineData>, ILineChartPoints
  {
    private CP.Code.IClassElement codeClass;
    public ICPEvent<CPLineEvArgs> addCPEvent { get; } = new CPEvent<CPLineEvArgs>();
    public ICPEvent<CPLineEvArgs> remCPEvent { get; } = new CPEvent<CPLineEvArgs>();

    public ISet<IChartPoint> chartPoints { get; set; } =
      new SortedSet<IChartPoint>(
        Comparer<IChartPoint>.Create((lh, rh) => (lh.data.uniqueName.CompareTo(rh.data.uniqueName))));

    public int Count
    {
      get { return chartPoints.Count; }
    }

    public LineChartPoints(CP.Code.IClassElement _codeClass, int _lineNum, int _linePos, ICPFileData _fileData)
    {
      codeClass = _codeClass;
      theData = new CPLineData() {pos = new TextPosition(_lineNum, _linePos, MoveChartPoint), fileData = _fileData};
    }

    public void MoveChartPoint(IChartPoint cp, int _lineNum, int _linePos)
    {
      RemoveChartPoint(cp);
      theData.fileData.Move(this, cp, _lineNum, _linePos);
    }

    public virtual IChartPoint GetChartPoint(string varName)
    {
      return chartPoints.FirstOrDefault((lp) => (lp.data.uniqueName == varName));
    }

    public bool AddChartPoint(string varName, out IChartPoint chartPnt)
    {
      chartPnt = null;
      CP.Code.IClassVarElement codeElem = codeClass.GetVar(varName);
      if (codeElem != null)
        return AddChartPoint(codeElem, out chartPnt, false);

      return false;
    }

    public bool AddChartPoint(IChartPoint chartPnt)
    {
      if (GetChartPoint(chartPnt.data.uniqueName) == null)
      {
        chartPoints.Add(chartPnt);
        addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));
        return true;
      }

      return false;
    }

    public bool RemoveChartPoint(IChartPoint chartPnt)
    {
      bool ret = chartPoints.Remove(chartPnt);
      if (ret)
        remCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return ret;
    }

    public bool AddChartPoint(string varName, CP.Code.IClassElement codeClass, out IChartPoint chartPnt,
      bool checkExistance = true)
    {
      if (checkExistance)
      {
        chartPnt = GetChartPoint(varName);
        if (chartPnt != null)
          return false;
      }
      CP.Code.IClassVarElement codeElem = codeClass.GetVar(varName);
      chartPnt = ChartPntFactory.Instance.CreateChartPoint(codeElem, data);
      chartPoints.Add(chartPnt);
      addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return true;
    }

    public bool AddChartPoint(CP.Code.IClassVarElement codeElem, out IChartPoint chartPnt, bool checkExistance = true)
    {
      if (checkExistance)
      {
        chartPnt = GetChartPoint(codeElem.uniqueName);
        if (chartPnt != null)
          return false;
      }
      chartPnt = ChartPntFactory.Instance.CreateChartPoint(codeElem, data);
      chartPoints.Add(chartPnt);
      addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return true;
    }

    public bool SyncChartPoint(ICheckElem checkElem)//, IClassElement ownerClass)
    {
      if (checkElem.exists)
      {
        IChartPoint chartPnt = null;
        AddChartPoint(checkElem.var, out chartPnt, false);
      }
      else
      {
        IChartPoint cp = chartPoints.FirstOrDefault((lp) => (lp.data.uniqueName == checkElem.var.uniqueName));
        if (cp != null)
          RemoveChartPoint(cp);
      }

      return false;
    }

    public bool ValidatePosition(int linesAdd)
    {
      bool changed = false;
      foreach (IChartPoint cp in chartPoints)
      {
        bool cpValidated = cp.ValidatePosition(data.pos.lineNum + linesAdd, data.pos.linePos);
        changed = changed || cpValidated;
      }

      return changed;
    }

    public void CalcInjectionPoints(CPClassLayout cpInjPoints, CP.Code.IModel model)
    {
      foreach (IChartPoint cp in chartPoints)
      {
        bool needDeclare = false;
        CPTraceVar traceVar = cp.CalcInjectionPoints(cpInjPoints, this.codeClass.name, out needDeclare);
        codeClass.CalcInjectionPoints(cpInjPoints, traceVar, needDeclare);
        model.CalcInjectionPoints(cpInjPoints, traceVar);
      }
    }

  }

  public class CPFileData : ICPFileData
  {
    public string fileName { get; set;  }
    public string fileFullName { get; set; }
    public ICPProjectData projData { get; set; }

    delegate void UpdatePosition(ILineChartPoints lcps, IChartPoint cp, int _lineNum, int _linePos);

    private UpdatePosition updPos;

    public CPFileData(string _fileName, string _fileFullName, ICPProjectData _projData, Action<ILineChartPoints, IChartPoint, int, int> _updPos)
    {
      fileName = _fileName;
      fileFullName = _fileFullName.ToLower();
      projData = _projData;
      updPos = new UpdatePosition(_updPos);
    }
    public void Move(ILineChartPoints lcps ,IChartPoint cp, int _lineNum, int _linePos)
    {
      updPos(lcps, cp, _lineNum, _linePos);
    }
  }

  public class FileChartPoints : Data<FileChartPoints, ICPFileData, CPFileData>, IFileChartPoints
  {
    public ICPEvent<CPFileEvArgs> addCPLineEvent { get; } = new CPEvent<CPFileEvArgs>();
    public ICPEvent<CPFileEvArgs> remCPLineEvent { get; } = new CPEvent<CPFileEvArgs>();

    public ISet<ILineChartPoints> linePoints { get; set; }
      =
      new SortedSet<ILineChartPoints>(
        Comparer<ILineChartPoints>.Create(
          (lh, rh) =>
            (lh.data.pos.lineNum > rh.data.pos.lineNum ? 1 : lh.data.pos.lineNum < rh.data.pos.lineNum ? -1 : 0)));

    private CP.Code.IFileElem fileElem;

    public int Count
    {
      get { return linePoints.Count; }
    }

    public FileChartPoints(CP.Code.IFileElem _fileElem, ICPProjectData _projData)
    {
      fileElem = _fileElem;
      theData = new CPFileData(_fileElem.name, _fileElem.uniqueName, _projData, MoveChartPoint);
    }

    public void MoveChartPoint(ILineChartPoints _lcps, IChartPoint cp, int _lineNum, int _linePos)
    {
      ILineChartPoints lcps = AddLineChartPoints(_lineNum, _linePos);
      lcps.AddChartPoint(cp);
    }

    public ILineChartPoints GetLineChartPoints(int lineNum)
    {
      ILineChartPoints lPnts = linePoints.FirstOrDefault((lp) => (lp.data.pos.lineNum == lineNum));

      return lPnts;
    }

    protected bool AddLineChartPoints(ILineChartPoints linePnts)
    {
      ILineChartPoints lPnts = GetLineChartPoints(linePnts.data.pos.lineNum);
      if (lPnts == null)
      {
        linePoints.Add(linePnts);
        linePnts.remCPEvent.On += OnRemCp;
        addCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));

        return true;
      }

      return false;
    }

    protected bool RemoveLineChartPoints(ILineChartPoints linePnts)
    {
      bool ret = linePoints.Remove(linePnts);
      if (ret)
        remCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));

      return ret;
    }

    protected bool MoveLineChartPoints(ILineChartPoints linePnts, int linesAdd)
    {
      bool ret = linePoints.Remove(linePnts);
      if (ret)
      {
        Globals.taggerUpdater.RaiseChangeTagEvent(data.fileFullName, linePnts);
        ((TextPosition) ((LineChartPoints) linePnts).theData.pos).lineNum += linesAdd;
        linePoints.Add(linePnts);
        addCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));
      }

      return ret;
    }

    public ILineChartPoints AddLineChartPoints(int lineNum, int linePos)
    {
      ILineChartPoints lPnts = GetLineChartPoints(lineNum);
      if (lPnts == null)
      {
        CP.Code.IClassElement classElem = fileElem.GetClassFromFilePos(lineNum, linePos);
        if (classElem != null)
        {
          lPnts = ChartPntFactory.Instance.CreateLineChartPoint(classElem, lineNum, linePos, data);
          AddLineChartPoints(lPnts);
        }
      }

      return lPnts;
    }

    private void OnRemCp(CPLineEvArgs args)
    {
      if (args.lineCPs.Count == 0)
        RemoveLineChartPoints(args.lineCPs);
    }

    public bool ValidatePosition(int lineNum, int linesAdd)
    {
      bool changed = false;
      if (linesAdd != 0)
      {
        foreach (ILineChartPoints lPnts in linePoints.Reverse())
        {
          if (lPnts.data.pos.lineNum >= lineNum)
          {
            if (linesAdd < 0 && lineNum + (-linesAdd) >= lPnts.data.pos.lineNum)
              RemoveLineChartPoints(lPnts);
            else
              MoveLineChartPoints(lPnts, linesAdd);
          }
        }
      }
      //List<KeyValuePair<bool, ILineChartPoints>> changedLines = new List<KeyValuePair<bool, ILineChartPoints>>();
      //foreach (ILineChartPoints lPnts in linePoints.Reverse())
      //{
      //  if (lPnts.data.pos.lineNum >= lineNum)
      //  {
      //    bool lineChanged = lPnts.ValidatePosition(linesAdd);
      //    if (lineChanged)
      //    {
      //      if (linesAdd != 0)
      //        changedLines.Add(new KeyValuePair<bool, ILineChartPoints>(true, lPnts));
      //    }
      //    else
      //    {
      //      changedLines.Add(new KeyValuePair<bool, ILineChartPoints>(false, lPnts));
      //    }
      //    changed = changed || lineChanged;
      //  }
      //}
      //if (changedLines.Count > 0)
      //{
      //  foreach (KeyValuePair<bool, ILineChartPoints> lPnts in changedLines)
      //  {
      //    if (lPnts.Key == true)
      //      MoveLineChartPoints(lPnts.Value, linesAdd);
      //    else
      //      RemoveLineChartPoints(lPnts.Value);
      //    //linePoints.Remove(lPnts.Value);//!!!EVENT!!!
      //    ////if (RemoveLineChartPoints(lPnts.Value))
      //    //Globals.taggerUpdater.RaiseChangeTagEvent(data.fileFullName, lPnts.Value);
      //  }
      //  //foreach (KeyValuePair<bool, ILineChartPoints> lPnts in changedLines)
      //  //{
      //  //  if (lPnts.Key == true)
      //  //  {
      //  //    ((TextPosition)((LineChartPoints) lPnts.Value).theData.pos).lineNum += linesAdd;
      //  //    //linePoints.Add(lPnts.Value);
      //  //    AddLineChartPoints(lPnts.Value);
      //  //  }
      //  //}
      //  //if(linePoints.Count == 0)
      //  //  remFunc(this);
      //}

      return changed;
    }

    public void CalcInjectionPoints(CPClassLayout cpInjPoints, CP.Code.IModel model)
    {
      foreach (var lPnts in linePoints)
        lPnts.CalcInjectionPoints(cpInjPoints, model);
    }
  }

  public class CPProjectData : ICPProjectData
  {
    public string projName { get; set; }
  }

  public class ProjectChartPoints : Data<ProjectChartPoints, ICPProjectData, CPProjectData>, IProjectChartPoints
  {
    private CP.Code.IModel cpCodeModel;
    public ICPEvent<CPProjEvArgs> addCPFileEvent { get; } = new CPEvent<CPProjEvArgs>();
    public ICPEvent<CPProjEvArgs> remCPFileEvent { get; } = new CPEvent<CPProjEvArgs>();
    public ISet<IFileChartPoints> filePoints { get; set; } = new SortedSet<IFileChartPoints>(Comparer<IFileChartPoints>.Create((lh, rh) => (lh.data.fileName.CompareTo(rh.data.fileName))));

    public int Count { get { return filePoints.Count; } }

    public ProjectChartPoints(string _projName)
    {
      theData = new CPProjectData() {projName = _projName};
      DTE2 dte2 = (DTE2)Globals.dte;
      Events2 evs2 = (Events2)dte2.Events;
      cpCodeModel = new CP.Code.Model(data.projName, evs2);
    }

    public IFileChartPoints GetFileChartPoints(string fname)
    {
      IFileChartPoints fPnts = filePoints.FirstOrDefault((fp) => (fp.data.fileName == fname));

      return fPnts;
    }

    public ILineChartPoints GetFileLineChartPoints(string fname, int lineNum)
    {
      ILineChartPoints lPnts = null;
      IFileChartPoints fPnts = GetFileChartPoints(fname);
      if (fPnts != null)
        lPnts = fPnts.GetLineChartPoints(lineNum);

      return lPnts;
    }

    protected bool AddFileChartPoints(IFileChartPoints filePnts, bool checkExistance = true)
    {
      if (checkExistance)
      {
        IFileChartPoints fPnts = GetFileChartPoints(filePnts.data.fileName);
        if (fPnts != null)
          return false;
      }
      filePoints.Add(filePnts);
      filePnts.remCPLineEvent.On += OnRemLineCPs;
      addCPFileEvent.Fire(new CPProjEvArgs(this, filePnts));

      return true;
    }

    public ICheckCPPoint CheckCursorPos()
    {
      return cpCodeModel.CheckCursorPos();
    }

    private void OnRemLineCPs(CPFileEvArgs args)
    {
      if (args.fileCPs.Count == 0)
        RemoveFileChartPoints(args.fileCPs);
    }

    protected bool RemoveFileChartPoints(IFileChartPoints filePnts)
    {
      bool ret = filePoints.Remove(filePnts);
      if(ret)
        remCPFileEvent.Fire(new CPProjEvArgs(this, filePnts));

      return ret;
    }

    public IFileChartPoints AddFileChartPoints(string fileName)
    {
      IFileChartPoints fPnts = GetFileChartPoints(fileName);
      if (fPnts == null)
      {
        IFileElem fileElem = cpCodeModel.GetFile(fileName);
        if (fileElem != null)
        {
          fPnts = ChartPntFactory.Instance.CreateFileChartPoint(fileElem, data);
          AddFileChartPoints(fPnts, false);
        }
      }

      return fPnts;
    }

    public void CalcInjectionPoints(CPClassLayout cpInjPoints)
    {
      foreach (IFileChartPoints fPnts in filePoints)
        fPnts.CalcInjectionPoints(cpInjPoints, cpCodeModel);
    }

  }

}

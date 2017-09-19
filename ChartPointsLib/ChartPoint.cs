using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{

  [DataContract]
  public class ChartPointData : IChartPointData
  {
    [DataMember]
    public bool enabled { get; set; }
    [DataMember]
    public string varName { get; set; }
    public ETargetPointStatus status { get; set; }
    public string className { get; set; }
    public ICPLineData lineData { get; set; }
    public ChartPointData() { }
    public ChartPointData(IChartPointData _data)
    {
      enabled = _data.enabled;
      varName = _data.varName;
      className = _data.className;
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
      set { theData = (TDataImpl)value; }
    }
  }
  /// <summary>
  /// Implementation of IChartPoint interface
  /// </summary>
  public class ChartPoint : Data<ChartPoint, IChartPointData, ChartPointData>, IChartPoint
  {
    private VCCodeModel vcCodeModel;
    protected ChartPoint() { }

    public ChartPoint(string varName, VCCodeClass ownerClass, ICPLineData _lineData)
    {
      theData = new ChartPointData
      {
        enabled = true,
        className = (ownerClass != null ? ownerClass.Name : ""),//DisplayName
        status = ETargetPointStatus.SwitchedOn,
        varName = varName,
        lineData = _lineData
      };
      vcCodeModel = (ownerClass != null ? ownerClass.CodeModel : null);
    }

    public virtual void CalcInjectionPoints(CPClassLayout cpClassLayout, string _fname, int _lineNum, int _linePos)
    {
      CodeElement theClass = null;
      // find class, containing specified memeber
      foreach (CodeElement _class in vcCodeModel.Classes)
      {
        if (_class.Name == data.className)
        {
          theClass = _class;
          break;
        }
      }
      if (theClass != null)
      {
        VCCodeClass vcClass = (VCCodeClass)theClass;
        // find VCCodeVariable
        CodeElement theVar = null;
        foreach (CodeElement _var in vcClass.Variables)
        {
          if (_var.Name == /*"j"*/data.varName)
          {
            theVar = _var;
            break;
          }
        }
        if (theVar != null)
        {
          // check if trace var definition already exists
          CPTraceVar traceVar = null;
          if (!cpClassLayout.traceVarPos.TryGetValue(theVar.Name, out traceVar))
          {
            // add trace pos data
            traceVar = new CPTraceVar()
            {
              name = data.varName,
              type = ((VCCodeVariable)theVar).TypeString,
              className = data.className
            };
            cpClassLayout.traceVarPos.Add(traceVar.name, traceVar);
            // define trace var definition placement
            traceVar.defPos.fileName = theVar.ProjectItem.Name;
            traceVar.defPos.pos.lineNum = theVar.EndPoint.Line - 1;
            traceVar.defPos.pos.linePos = theVar.EndPoint.LineCharOffset - 1;
            // find all places, where this file included
            CodeElement theFunc = null;
            // find & store all constructors init points of this class
            foreach (CodeElement _func in vcClass.Functions)
            {
              if (_func.Name == data.className)
              {
                theFunc = _func;
                VCCodeFunction vcFunc = (VCCodeFunction)_func;
                EditPoint pnt = _func.StartPoint.CreateEditPoint();
                if (pnt.FindPattern("{"))
                  traceVar.traceVarInitPos.Add(new FilePosPnt() { fileName = _func.ProjectItem.Name, pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset } });
              }
            }
            // if no constructor found add default one
            if (traceVar.traceVarInitPos.Count == 0)
            {
              EditPoint pnt = vcClass.StartPoint.CreateEditPoint();
              if (pnt.FindPattern("{"))
                traceVar.injConstructorPos = new FilePosPnt() { fileName = vcClass.ProjectItem.Name, pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset } };
            }
          }
          traceVar.traceVarTracePos.Add(new FilePosPnt()
          {
            fileName = _fname,
            pos = {lineNum = _lineNum, linePos = _linePos}
          });
          TextPos traceInclPos = null;
          if(!cpClassLayout.traceInclPos.TryGetValue(theVar.ProjectItem.Name, out traceInclPos))
            cpClassLayout.traceInclPos.Add(theVar.ProjectItem.Name, new TextPos() {lineNum = 0, linePos = 0});
          foreach (var inclStmt in vcCodeModel.Includes)
          {
            VCCodeInclude vcinclStmt = (VCCodeInclude)inclStmt;
            CPInclude incl = null;
            string fname = Path.GetFileName(vcinclStmt.File);
            if (!cpClassLayout.includesPos.TryGetValue(new Tuple<string, string>(vcinclStmt.Name, fname), out incl))
            {
              if (vcinclStmt.Name == traceVar.defPos.fileName)
              {
                // define include placement
                FilePosText inclPos = new FilePosText()
                {
                  fileName = fname,
                  pos = { lineNum = vcinclStmt.StartPoint.Line - 1, linePos = vcinclStmt.StartPoint.LineCharOffset },
                  posEnd = { lineNum = vcinclStmt.EndPoint.Line - 1, linePos = vcinclStmt.EndPoint.LineCharOffset }
                };
                incl = new CPInclude()
                {
                  inclOrig = vcinclStmt.Name,
                  inclReplace = "__cp__." + vcinclStmt.Name,
                  pos = inclPos
                };
                cpClassLayout.includesPos.Add(new Tuple<string, string>(vcinclStmt.Name, incl.pos.fileName), incl);
              }
            }
          }
        }
      }
    }

    public bool ValidatePosition(int lineNum, int linePos)
    {
      vcCodeModel.Synchronize();
      CodeElement theClass = null;
      // find class, containing specified memeber
      foreach (CodeElement _class in vcCodeModel.Classes)
      {
        if (_class.Name == data.className)
        {
          theClass = _class;
          break;
        }
      }
      if (theClass != null)
      {
        try
        {
          VCCodeClass vcClass = (VCCodeClass) theClass;
          //CodeElement theFunc = null;
          foreach (CodeElement _func in vcClass.Functions)
          {
            VCCodeFunction vcFunc = (VCCodeFunction) _func;
            //TextPoint startFuncBody = vcFunc.StartPoint;// GetStartPoint(vsCMPart.vsCMPartBodyWithDelimiter);//vcFunc.StartPointOf[vsCMPart.vsCMPartBodyWithDelimiter, vsCMWhere.vsCMWhereDefinition];
            //TextPoint endFuncBody = vcFunc.EndPoint;// GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter);//vcFunc.EndPointOf[vsCMPart.vsCMPartBodyWithDelimiter, vsCMWhere.vsCMWhereDefinition];
            TextPoint startFuncBody = vcFunc.StartPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
            TextPoint endFuncBody = vcFunc.EndPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
            EditPoint startPnt = startFuncBody.CreateEditPoint();
            EditPoint endPnt = endFuncBody.CreateEditPoint();
            startPnt.FindPattern("{", (int) vsFindOptions.vsFindOptionsBackwards);
            endPnt.FindPattern("}");
            //if (lineNum >= startPnt.Line && linePos >= startPnt.LineCharOffset && lineNum <= endPnt.Line && linePos <= endPnt.LineCharOffset)
            if ((lineNum > startPnt.Line && lineNum < endPnt.Line) ||
                (lineNum == startPnt.Line && linePos >= startPnt.LineCharOffset) ||
                (lineNum == endPnt.Line && linePos <= endPnt.LineCharOffset))
            {
              // Oh, oh you're in the body, now.. (c)
              return true;
            }
          }
          //// find VCCodeVariable
          //CodeElement theVar = null;
          //foreach (CodeElement _var in vcClass.Variables)
          //{
          //  if (_var.Name == /*"j"*/ data.varName)
          //  {
          //    theVar = _var;
          //    break;
          //  }
          //}
          //if (theVar != null)
          //{
          //}
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
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
    public ICPEvent<CPLineEvArgs> addCPEvent { get; } = new CPEvent<CPLineEvArgs>();
    public ICPEvent<CPLineEvArgs> remCPEvent { get; } = new CPEvent<CPLineEvArgs>();

    public ISet<IChartPoint> chartPoints { get; set; } = new SortedSet<IChartPoint>(Comparer<IChartPoint>.Create((lh, rh) => (lh.data.varName.CompareTo(rh.data.varName))));

    public int Count { get { return chartPoints.Count; } }

    public LineChartPoints(int _lineNum, int _linePos, ICPFileData _fileData)
    {
      theData = new CPLineData() { pos = new TextPosition(_lineNum, _linePos, MoveChartPoint), fileData = _fileData };
    }

    public void MoveChartPoint(IChartPoint cp, int _lineNum, int _linePos)
    {
      RemoveChartPoint(cp);
      theData.fileData.Move(this, cp, _lineNum, _linePos);
    }
    public virtual IChartPoint GetChartPoint(string varName)
    {
      return chartPoints.FirstOrDefault((lp) => (lp.data.varName == varName));
    }

    public bool AddChartPoint(IChartPoint chartPnt)
    {
      if (GetChartPoint(chartPnt.data.varName) == null)
      {
        chartPoints.Add(chartPnt);
        addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));
        return true;
      }

      return false;
    }

    protected bool RemoveChartPoint(IChartPoint chartPnt)
    {
      bool ret = chartPoints.Remove(chartPnt);
      if(ret)
        remCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return ret;
    }

    public bool AddChartPoint(string varName, VCCodeClass ownerClass, out IChartPoint chartPnt, bool checkExistance = true)
    {
      if (checkExistance)
      {
        chartPnt = GetChartPoint(varName);
        if (chartPnt != null)
          return false;
      }
      chartPnt = ChartPntFactory.Instance.CreateChartPoint(varName, ownerClass, data);
      chartPoints.Add(chartPnt);
      addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return true;
    }

    public bool SyncChartPoints(string fname, ISet<string> cpVarNames, VCCodeClass className)
    {
      bool changed = false;
      int prevCount = chartPoints.Count;
      foreach (var chartPnt in chartPoints.Reverse())
      //var enumerator = chartPoints.GetEnumerator();
      //while (enumerator.MoveNext())
      {
        //IChartPoint chartPnt = enumerator.Current;
        string varName = cpVarNames.FirstOrDefault((s) => (s == chartPnt.data.varName));
        if (varName == null)
        {
          //enumerator.MoveNext();
          RemoveChartPoint(chartPnt);
          cpVarNames.Remove(varName);
          changed = true;
        }
      }
      foreach (var varName in cpVarNames)
      {
        IChartPoint chartPnt = null;
        AddChartPoint(varName, className, out chartPnt, false);
        changed = true;
      }
      //int newCount = chartPoints.Count;
      //if (newCount != prevCount && (prevCount == 0 || newCount == 0))
      //  Globals.taggerUpdater.RaiseChangeTagEvent(fname, this);


      return changed;
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
      = new SortedSet<ILineChartPoints>(Comparer<ILineChartPoints>.Create((lh, rh) => (lh.data.pos.lineNum > rh.data.pos.lineNum ? 1 : lh.data.pos.lineNum < rh.data.pos.lineNum ? -1 : 0)));
    public int Count { get { return linePoints.Count; } }

    public FileChartPoints(string _fileName, string _fileFullName, ICPProjectData _projData)
    {
      theData = new CPFileData(_fileName, _fileFullName, _projData, MoveChartPoint);
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
      if(ret)
        remCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));

      return ret;
    }
    protected bool MoveLineChartPoints(ILineChartPoints linePnts, int linesAdd)
    {
      bool ret = linePoints.Remove(linePnts);
      if (ret)
      {
        Globals.taggerUpdater.RaiseChangeTagEvent(data.fileFullName, linePnts);
        ((TextPosition)((LineChartPoints)linePnts).theData.pos).lineNum += linesAdd;
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
        lPnts = ChartPntFactory.Instance.CreateLineChartPoint(lineNum, linePos, data);
      }
      AddLineChartPoints(lPnts);

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
      List< KeyValuePair<bool, ILineChartPoints> > changedLines = new List<KeyValuePair<bool, ILineChartPoints>>();
      foreach (ILineChartPoints lPnts in linePoints.Reverse())
      {
        if (lPnts.data.pos.lineNum >= lineNum)
        {
          bool lineChanged = lPnts.ValidatePosition(linesAdd);
          if (lineChanged)
          {
            if (linesAdd != 0)
              changedLines.Add(new KeyValuePair<bool, ILineChartPoints>(true, lPnts));
          }
          else
            changedLines.Add(new KeyValuePair<bool, ILineChartPoints>(false, lPnts));
          changed = changed || lineChanged;
        }
      }
      if (changedLines.Count > 0)
      {
        foreach (KeyValuePair<bool, ILineChartPoints> lPnts in changedLines)
        {
          if (lPnts.Key == true)
            MoveLineChartPoints(lPnts.Value, linesAdd);
          else
            RemoveLineChartPoints(lPnts.Value);
          //linePoints.Remove(lPnts.Value);//!!!EVENT!!!
          ////if (RemoveLineChartPoints(lPnts.Value))
          //Globals.taggerUpdater.RaiseChangeTagEvent(data.fileFullName, lPnts.Value);
        }
        //foreach (KeyValuePair<bool, ILineChartPoints> lPnts in changedLines)
        //{
        //  if (lPnts.Key == true)
        //  {
        //    ((TextPosition)((LineChartPoints) lPnts.Value).theData.pos).lineNum += linesAdd;
        //    //linePoints.Add(lPnts.Value);
        //    AddLineChartPoints(lPnts.Value);
        //  }
        //}
        //if(linePoints.Count == 0)
        //  remFunc(this);
      }

      return changed;
    }

  }

  public class CPProjectData : ICPProjectData
  {
    public string projName { get; set; }
  }

  public class ProjectChartPoints : Data<ProjectChartPoints, ICPProjectData, CPProjectData>, IProjectChartPoints
  {
    public ICPEvent<CPProjEvArgs> addCPFileEvent { get; } = new CPEvent<CPProjEvArgs>();
    public ICPEvent<CPProjEvArgs> remCPFileEvent { get; } = new CPEvent<CPProjEvArgs>();
    public ISet<IFileChartPoints> filePoints { get; set; } = new SortedSet<IFileChartPoints>(Comparer<IFileChartPoints>.Create((lh, rh) => (lh.data.fileName.CompareTo(rh.data.fileName))));

    public int Count { get { return filePoints.Count; } }
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
    protected bool AddFileChartPoints(IFileChartPoints filePnts)
    {
      IFileChartPoints fPnts = GetFileChartPoints(filePnts.data.fileName);
      if (fPnts == null)
      {
        filePoints.Add(filePnts);
        filePnts.remCPLineEvent.On += OnRemLineCPs;
        addCPFileEvent.Fire(new CPProjEvArgs(this, filePnts));

        return true;
      }

      return false;
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
    public IFileChartPoints AddFileChartPoints(string fileName, string fileFullName)
    {
      IFileChartPoints fPnts = GetFileChartPoints(fileName);
      if (fPnts == null)
        fPnts = ChartPntFactory.Instance.CreateFileChartPoint(fileName, fileFullName, data);
      AddFileChartPoints(fPnts);

      return fPnts;
    }
  }

  public class CheckPoint : ICheckPoint
  {
    public VCCodeClass ownerClass;
    public string projName;
    public EnvDTE.Document doc;
    public int lineNum;
    public int linePos;
    private List<Tuple<string, string, bool>> availableVars;
    private ILineChartPoints lPnts;

    //public bool AddChartPoint(string varName)
    //{
    //  IProjectChartPoints pPnts = null;//Globals.processor.GetProjectChartPoints(projName);
    //  //if (pPnts == null)
    //    Globals.processor.AddProjectChartPoints(projName, out pPnts);
    //  IFileChartPoints fPnts = pPnts.GetFileChartPoints(doc.Name);
    //  if (fPnts == null)
    //    fPnts = pPnts.AddFileChartPoints(doc.Name, System.IO.Path.GetFullPath(doc.FullName).ToLower());
    //  lPnts = fPnts.GetLineChartPoints(lineNum);
    //  if (lPnts == null)
    //    lPnts = fPnts.AddLineChartPoints(lineNum, linePos);
    //  //if (chartPnt == null || chartPnt.status != ETargetPointStatus.Available)
    //  //  return false;
    //  //StoreChartPnt(chartPnt);
    //  Globals.taggerUpdater.RaiseChangeTagEvent(fPnts);

    //  return true;
    //}

    public bool SyncChartPoints(ISet<string> cpVarNames)
    {
      IProjectChartPoints pPnts = null;
      Globals.processor.AddProjectChartPoints(projName, out pPnts);
      IFileChartPoints fPnts = null;
      fPnts = pPnts.AddFileChartPoints(doc.Name, System.IO.Path.GetFullPath(doc.FullName).ToLower());
      lPnts = null;
      lPnts = fPnts.AddLineChartPoints(lineNum, linePos);
      bool changed = lPnts.SyncChartPoints(fPnts.data.fileFullName, cpVarNames, ownerClass);
      //if (changed)
      //  Globals.taggerUpdater.RaiseChangeTagEvent(fPnts);

      return changed;
    }

    public void GetAvailableVars(out List<Tuple<string, string, bool>> _availableVars)
    {
      availableVars = _availableVars = null;
      if (ownerClass == null)
        return;
      availableVars = new List<Tuple<string, string, bool>>();
      string className = ownerClass.FullName;
      CodeElements vcElems = ownerClass.Children;
      if (vcElems.Count > 0)
      {
        IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(projName);
        IFileChartPoints fPnts = pPnts?.GetFileChartPoints(doc.Name);
        lPnts = fPnts?.GetLineChartPoints(lineNum);
        foreach (CodeElement el in vcElems)
        {
          if (el.Kind == vsCMElement.vsCMElementVariable)
          {
            //CodeVariable cv = (CodeVariable) el;
            //CodeType _ct = cv.Type.CodeType;
            VCCodeVariable varElem = (VCCodeVariable)el;
            //CodeType ct = varElem.Type.CodeType;
            //if (ct != null)
            //{
            //  CodeElements ce = varElem.Type.CodeType.Bases;
            //}
            if (varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefBool
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefByte
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefChar
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDecimal
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDouble
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefFloat
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefInt
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefLong
                || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefShort)
            {
              bool exists = false;
              if (lPnts != null)
              {
                if (lPnts.GetChartPoint(varElem.DisplayName) != null)
                  exists = true;
              }
              availableVars.Add(new Tuple<string, string, bool>(varElem.DisplayName, varElem.TypeString, exists));
            }
          }
        }
        _availableVars = availableVars;
      }
    }
  }

}

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
    //[DataMember]
    //public string fileName { get; set; }
    //[DataMember]
    //public string fileFullName { get; set; }
    //[DataMember]
    //public int lineNum { get; set; }
    //[DataMember]
    //public int linePos { get; set; }
    [DataMember]
    public bool enabled { get; set; }
    [DataMember]
    public string varName { get; set; }
    public string className { get; set; }
    public ChartPointData() { }
    public ChartPointData(IChartPointData _data)
    {
      //fileName = _data.fileName;
      //fileFullName = _data.fileFullName;
      //lineNum = _data.lineNum;
      //linePos = _data.linePos;
      enabled = _data.enabled;
      varName = _data.varName;
      className = _data.className;
    }
  }
  /// <summary>
  /// Implementation of IChartPoint interface
  /// </summary>
  public class ChartPoint : IChartPoint
  {
    //private TextPoint startFuncPnt;
    //private TextPoint endFuncPnt;
    private VCCodeModel vcCodeModel;

    private Func<IChartPoint, bool> addFunc;
    private Func<IChartPoint, bool> remFunc;
    public ETargetPointStatus status { get; set; }
    //public VCCodeVariable var { get; }
    protected ChartPointData theData { get; set; }
    //protected VCCodeClass targetClassElem;

    public IChartPointData data
    {
      get { return theData; }
      set { theData = (ChartPointData)value; }
    }
    protected ChartPoint() { }
    //public ChartPoint(/*TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
    //  , */VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    //{
    //  //targetClassElem = _targetClassElem;
    //  theData = new ChartPointData
    //  {
    //    enabled = false,
    //    //lineNum = caretPnt.Line,
    //    //linePos = caretPnt.LineCharOffset,
    //    //fileName = caretPnt.Parent.Parent.Name,
    //    //fileFullName = System.IO.Path.GetFullPath(caretPnt.Parent.Parent.FullName).ToLower(),
    //    className = _targetClassElem.Name,//DisplayName
    //    varName = "j"
    //  };
    //  /*???*/
    //  //startFuncPnt = _startFuncPnt;
    //  ///*???*/
    //  //endFuncPnt = _endFuncPnt;
    //  //targetClassElem = _targetClassElem;
    //  vcCodeModel = _targetClassElem.CodeModel;
    //  status = ETargetPointStatus.Available;
    //  addFunc = _addFunc;
    //  remFunc = _remFunc;
    //}

    //public ChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    //{
    //  theData = new ChartPointData(_data);
    //  //startFuncPnt = _startFuncPnt;
    //  //endFuncPnt = _endFuncPnt;
    //  //targetClassElem = _targetClassElem;
    //  if (data.enabled)
    //    status = ETargetPointStatus.SwitchedOn;
    //  else
    //    status = ETargetPointStatus.SwitchedOff;
    //  addFunc = _addFunc;
    //  remFunc = _remFunc;
    //}

    public ChartPoint(string varName, VCCodeClass ownerClass, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    {
      theData = new ChartPointData
      {
        enabled = true,
        className = (ownerClass != null ? ownerClass.Name : ""),//DisplayName
        varName = varName
      };
      vcCodeModel = (ownerClass != null ? ownerClass.CodeModel : null);
      status = ETargetPointStatus.SwitchedOn;
      addFunc = _addFunc;
      remFunc = _remFunc;
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

    /// <summary>
    /// Try to toggle chartpoint
    /// </summary>
    /// <returns>new chartpoint status</returns>
    public ETargetPointStatus Toggle()
    {
      switch (status)
      {
        case ETargetPointStatus.Available:
          if (addFunc(this))
            status = ETargetPointStatus.SwitchedOn;
          return status;
        case ETargetPointStatus.SwitchedOff:
        case ETargetPointStatus.SwitchedOn:
          if (remFunc(this))
            status = ETargetPointStatus.Available;
          return status;
      }

      return ETargetPointStatus.NotAvailable;
    }

    /// <summary>
    /// Try to remove chartpoint
    /// </summary>
    /// <returns>new chartpoint status</returns>
    public ETargetPointStatus Remove()
    {
      switch (status)
      {
        case ETargetPointStatus.Available:
        case ETargetPointStatus.NotAvailable:
          return status;
        case ETargetPointStatus.SwitchedOff:
        case ETargetPointStatus.SwitchedOn:
          if (remFunc(this))
            status = ETargetPointStatus.Available;
          return status;
      }

      return ETargetPointStatus.NotAvailable;
    }
  }

  public class AddRemover<T>
  {
    public Func<T, bool> addFunc;
    public Func<T, bool> remFunc;
  }

  public class LineChartPoints : AddRemover<LineChartPoints>, ILineChartPoints
  {
    public int lineNum { get; set; }
    public int linePos { get; set; }
    public ISet<IChartPoint> chartPoints { get; set; } = new SortedSet<IChartPoint>(Comparer<IChartPoint>.Create((lh, rh) => (lh.data.varName.CompareTo(rh.data.varName))));

    public virtual IChartPoint GetChartPoint(string varName)
    {
      return chartPoints.FirstOrDefault((lp) => (lp.data.varName == varName));
    }

    protected bool AddChartPoint(IChartPoint chartPnt)
    {
      if (GetChartPoint(chartPnt.data.varName) == null)
      {
        chartPoints.Add(chartPnt);
        return true;
      }

      return false;
    }

    protected bool RemoveChartPoint(IChartPoint chartPnt)
    {
      bool ret = chartPoints.Remove(chartPnt);
      if (chartPoints.Count == 0)
        ret &= remFunc(this);

      return ret;
    }

    public bool AddChartPoint(string varName, VCCodeClass ownerClass, out IChartPoint chartPnt)
    {
      chartPnt = GetChartPoint(varName);
      if (chartPnt == null)
      {
        chartPnt = ChartPntFactory.Instance.CreateChartPoint(varName, ownerClass, AddChartPoint, RemoveChartPoint);
        chartPoints.Add(chartPnt);

        return true;

      }
      return false;
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
        IChartPoint chartPnt = ChartPntFactory.Instance.CreateChartPoint(varName, className, AddChartPoint, RemoveChartPoint);
        chartPoints.Add(chartPnt);
        changed = true;
      }
      int newCount = chartPoints.Count;
      if (newCount != prevCount && (prevCount == 0 || newCount == 0))
        Globals.taggerUpdater.RaiseChangeTagEvent(fname, this);


      return changed;
    }
  }

  public class FileChartPoints : AddRemover<FileChartPoints>, IFileChartPoints
  {
    public string fileName { get; set; }
    public string fileFullName { get; set; }
    public ISet<ILineChartPoints> linePoints { get; set; }
      = new SortedSet<ILineChartPoints>(Comparer<ILineChartPoints>.Create((lh, rh) => (lh.lineNum > rh.lineNum ? 1 : lh.lineNum < rh.lineNum ? -1 : 0)));

    public ILineChartPoints GetLineChartPoints(int lineNum)
    {
      ILineChartPoints lPnts = linePoints.FirstOrDefault((lp) => (lp.lineNum == lineNum));

      return lPnts;
    }
    protected bool AddLineChartPoints(ILineChartPoints linePnts)
    {
      ILineChartPoints lPnts = GetLineChartPoints(linePnts.lineNum);
      if (lPnts == null)
      {
        //((LineChartPoints)linePnts).addFunc = AddLineChartPoints;
        //((LineChartPoints)linePnts).remFunc = RemoveLineChartPoints;
        linePoints.Add(linePnts);

        return true;
      }

      return false;
    }
    protected bool RemoveLineChartPoints(ILineChartPoints linePnts)
    {
      bool ret = linePoints.Remove(linePnts);
      if (linePoints.Count == 0)
        ret &= remFunc(this);

      return ret;
    }
    public ILineChartPoints AddLineChartPoints(int lineNum, int linePos)
    {
      ILineChartPoints lPnts = GetLineChartPoints(lineNum);
      if (lPnts == null)
        lPnts = ChartPntFactory.Instance.CreateLineChartPoint(lineNum, linePos, AddLineChartPoints, RemoveLineChartPoints);
      AddLineChartPoints(lPnts);

      return lPnts;
    }
  }

  public class ProjectChartPoints : AddRemover<ProjectChartPoints>, IProjectChartPoints
  {
    public string projName { get; set; }
    public ISet<IFileChartPoints> filePoints { get; set; } = new SortedSet<IFileChartPoints>(Comparer<IFileChartPoints>.Create((lh, rh) => (lh.fileName.CompareTo(rh.fileName))));

    public IFileChartPoints GetFileChartPoints(string fname)
    {
      IFileChartPoints fPnts = filePoints.FirstOrDefault((fp) => (fp.fileName == fname));

      return fPnts;
    }
    public ILineChartPoints GetFileLineChartPoints(string fname, int lineNum)
    {
      ILineChartPoints lPnts = null;
      IFileChartPoints fPnts = GetFileChartPoints(fname);
      if (fPnts != null)
        lPnts = fPnts.GetLineChartPoints(lineNum);
      addFunc(this);

      return lPnts;
    }
    protected bool AddFileChartPoints(IFileChartPoints filePnts)
    {
      IFileChartPoints fPnts = GetFileChartPoints(filePnts.fileName);
      if (fPnts == null)
      {
        filePoints.Add(filePnts);

        return true;
      }

      return false;
    }
    protected bool RemoveFileChartPoints(IFileChartPoints filePnts)
    {
      bool ret = filePoints.Remove(filePnts);
      if (filePoints.Count == 0)
        ret &= remFunc(this);

      return ret;
    }
    public IFileChartPoints AddFileChartPoints(string fileName, string fileFullName)
    {
      IFileChartPoints fPnts = GetFileChartPoints(fileName);
      if (fPnts == null)
        fPnts = ChartPntFactory.Instance.CreateFileChartPoint(fileName, fileFullName, AddFileChartPoints, RemoveFileChartPoints);
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
      bool changed = lPnts.SyncChartPoints(fPnts.fileFullName, cpVarNames, ownerClass);
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

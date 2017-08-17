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
    public string fileName { get; set; }
    [DataMember]
    public string fileFullName { get; set; }
    [DataMember]
    public int lineNum { get; set; }
    [DataMember]
    public int linePos { get; set; }
    [DataMember]
    public bool enabled { get; set; }
    [DataMember]
    public string varName { get; set; }
    public string className { get; set; }
    public ChartPointData() { }
    public ChartPointData(IChartPointData _data)
    {
      fileName = _data.fileName;
      fileFullName = _data.fileFullName;
      lineNum = _data.lineNum;
      linePos = _data.linePos;
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
    private TextPoint startFuncPnt;
    private TextPoint endFuncPnt;
    //private VCCodeElement targetClassElem;
    private VCCodeModel vcCodeModel;

    private Func<IChartPoint, bool> addFunc;
    private Func<IChartPoint, bool> remFunc;
    public ETargetPointStatus status { get; set; }
    public VCCodeVariable var { get; }
    protected ChartPointData theData { get; set; }
    protected VCCodeClass targetClassElem;

    public IChartPointData data
    {
      get { return theData; }
      set { theData = (ChartPointData)value; }
    }
    protected ChartPoint() { }
    public ChartPoint(TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
      , VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    {
      targetClassElem = _targetClassElem;
      theData = new ChartPointData
      {
        enabled = false,
        lineNum = caretPnt.Line,
        linePos = caretPnt.LineCharOffset,
        fileName = caretPnt.Parent.Parent.Name,
        fileFullName = System.IO.Path.GetFullPath(caretPnt.Parent.Parent.FullName).ToLower(),
        className = _targetClassElem.Name,//DisplayName
        varName = "j"
      };
      /*???*/
      startFuncPnt = _startFuncPnt;
      /*???*/
      endFuncPnt = _endFuncPnt;
      //targetClassElem = _targetClassElem;
      vcCodeModel = _targetClassElem.CodeModel;
      status = ETargetPointStatus.Available;
      addFunc = _addFunc;
      remFunc = _remFunc;
    }

    public ChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    {
      theData = new ChartPointData(_data);
      //startFuncPnt = _startFuncPnt;
      //endFuncPnt = _endFuncPnt;
      //targetClassElem = _targetClassElem;
      if (data.enabled)
        status = ETargetPointStatus.SwitchedOn;
      else
        status = ETargetPointStatus.SwitchedOff;
      addFunc = _addFunc;
      remFunc = _remFunc;
    }

    public virtual void CalcInjectionPoints(out CPClassLayout cpInjPoints)
    {
      cpInjPoints = new CPClassLayout();
      CodeElement theClass = null;
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
        CodeElement theVar = null;
        foreach (CodeElement _var in vcClass.Variables)
        {
          if (_var.Name == "j"/*data.varName*/)
          {
            theVar = _var;
            break;
          }
        }
        if (theVar != null)
        {
          CPTraceVar traceVar = new CPTraceVar();
          traceVar.filePos.fileName = theVar.ProjectItem.Name;
          traceVar.filePos.pos.lineNum = theVar.EndPoint.Line - 1;
          traceVar.filePos.pos.linePos = theVar.EndPoint.LineCharOffset - 1;
          traceVar.type = ((VCCodeVariable)theVar).TypeString;
          cpInjPoints.traceVars.Add(theVar.Name, traceVar);
          foreach (var inclStmt in vcCodeModel.Includes)
          {
            VCCodeInclude vcinclStmt = (VCCodeInclude)inclStmt;
            if (vcinclStmt.Name == traceVar.filePos.fileName)
            {
              FilePosText inclPos = new FilePosText()
              {
                fileName = Path.GetFileName(vcinclStmt.File),
                pos = { lineNum = vcinclStmt.StartPoint.Line - 1, linePos = vcinclStmt.StartPoint.LineCharOffset },
                posEnd = { lineNum = vcinclStmt.EndPoint.Line - 1, linePos = vcinclStmt.EndPoint.LineCharOffset }
              };
              CPInclude incl = new CPInclude()
              {
                inclOrig = vcinclStmt.Name,
                inclReplace = "__cp__." + vcinclStmt.Name,
                pos = inclPos
              };
              cpInjPoints.includesPos.Add(incl);
            }
          }
        }
        CodeElement theFunc = null;
        bool constructorFound = false;
        foreach (CodeElement _func in vcClass.Functions)
        {
          if (_func.Name == data.className)
          {
            theFunc = _func;
            constructorFound = true;
            VCCodeFunction vcFunc = (VCCodeFunction)_func;
            EditPoint pnt = _func.StartPoint.CreateEditPoint();
            if (pnt.FindPattern("{"))
            {
              cpInjPoints.traceVarInitPos.Add(new FilePosPnt() { fileName = _func.ProjectItem.Name, pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset } });
            }
          }
        }
        if (!constructorFound)
        {
          EditPoint pnt = vcClass.StartPoint.CreateEditPoint();
          cpInjPoints.injConstructorPos = new FilePosPnt() { fileName = vcClass.ProjectItem.Name, pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset } };
          if (pnt.FindPattern("{"))
            cpInjPoints.traceVarInitPos.Add(new FilePosPnt() { fileName = vcClass.ProjectItem.Name, pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset } });
        }
      }
    }

    public void GetAvailableVars(out List<Tuple<string, string>> availableVars)
    {
      availableVars = null;
      VCCodeClass ownerClass = (VCCodeClass)targetClassElem;
      if (ownerClass == null)
        return;
      availableVars = new List<Tuple<string, string>>();
      string className = ownerClass.FullName;
      CodeElements vcElems = targetClassElem.Children;
      foreach (CodeElement el in vcElems)
      {
        if (el.Kind == vsCMElement.vsCMElementVariable)
        {
          VCCodeVariable varElem = (VCCodeVariable)el;
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
            availableVars.Add(new Tuple<string, string>(varElem.DisplayName, varElem.TypeString));
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

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CP.Code;
using EnvDTE80;
using System.Security.Permissions;
using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

namespace ChartPoints
{

  [DataContract]
  //[Serializable]
  public abstract class ChartPointData : IChartPointData//, ISerializable
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
    public ChartPointData(string _name, string _uniqueName, string _type, bool _enabled, EChartPointStatus _status, ICPLineData _lineData)
    {
      name = _name;
      uniqueName = _uniqueName;
      type = _type;
      enabled = _enabled;
      status = _status;
      lineData = _lineData;
    }
    public ChartPointData(IChartPointData _data)
    {
      enabled = _data.enabled;
      name = _data.name;
      uniqueName = _data.uniqueName;
      type = _data.type;
    }
    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("uniqueName", uniqueName);
    //  info.AddValue("enabled", enabled);
    //}
  }

  //public class Data<T, TData, TDataImpl> : IData<TData>
  //  where TData : class
  //  where TDataImpl : TData
  //{
  //  public TDataImpl theData { get; set; }

  //  public TData data
  //  {
  //    get { return theData; }
  //    //set { theData = (TDataImpl)value; }
  //  }
  //}

  /// <summary>
  /// Implementation of IChartPoint interface
  /// </summary>
  //[Serializable]
  public abstract class ChartPoint : IChartPoint//Data<ChartPoint, IChartPointData, ChartPointData>, IChartPoint, ISerializable
  {
    public IChartPointData data { get; }
    public ICPEvent<CPStatusEvArgs> cpStatusChangedEvent { get; set; } = new CPEvent<CPStatusEvArgs>();
    private CP.Code.IClassVarElement codeElem;
    protected ChartPoint() { }

    public ChartPoint(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData)
    {
      codeElem = _codeElem;
      codeElem.classVarChangedEvent += ClassVarChangedEventOn;
      codeElem.classVarDeletedEvent += ClassVarDeletedEventOn;
      data = CP.Utils.IClassFactory.GetInstance().CreateCPData(codeElem.name, codeElem.uniqueName, codeElem.type, true, EChartPointStatus.SwitchedOn, _lineData);
    }

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue(data.GetType().ToString(), data, data.GetType());
    //}

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
      if (newStatus != EChartPointStatus.SwitchedOn)
        data.enabled = false;
      else
        data.enabled = true;
      EChartPointStatus curStatus = data.status;
      if (newStatus != curStatus)
      {
        data.status = newStatus;
        cpStatusChangedEvent.Fire(new CPStatusEvArgs(this));
      }

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
    public void Invalidate()
    {
      SetStatus(EChartPointStatus.NotAvailable);
    }

    public bool Validate()
    {
      return codeElem.Validate(data.uniqueName);
    }
  }

}

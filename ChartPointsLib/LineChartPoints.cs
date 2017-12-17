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

  //[Serializable]
  public class TextPosition : ITextPosition//, ISerializable
  {
    public int lineNum { get; set; }
    public int linePos { get; set; }

    //delegate void UpdatePosition(IChartPoint cp, int _lineNum, int _linePos);

    //private UpdatePosition updPos;

    public TextPosition(int _lineNum, int _linePos)//, Action<IChartPoint, int, int> _updPos)
    {
      lineNum = _lineNum;
      linePos = _linePos;
      //updPos = new UpdatePosition(_updPos);
    }

    //public void Move(IChartPoint cp, int _lineNum, int _linePos)
    //{
    //  updPos(cp, _lineNum, _linePos);
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("lineNum", lineNum, lineNum.GetType());
    //  info.AddValue("linePos", linePos, linePos.GetType());
    //}
  }

  //[Serializable]
  public abstract class CPLineData : ICPLineData//, ISerializable
  {
    public ITextPosition pos { get; set; }
    public ICPFileData fileData { get; set; }
    public CPLineData(ITextPosition _pos, ICPFileData _fileData)
    {
      pos = _pos;
      fileData = _fileData;
    }
    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("pos", pos, pos.GetType());
    //}
  }

  //[Serializable]
  public abstract class LineChartPoints : ILineChartPoints//Data<LineChartPoints, ICPLineData, CPLineData>, ILineChartPoints, ISerializable
  {
    public ICPLineData data { get; }
    private ELineCPsStatus theStatus = ELineCPsStatus.NotAvailable;
    public ELineCPsStatus status
    {
      get { return theStatus; }
    }

    private ELineCPsStatus UpdateStatus(IChartPoint cp)
    {
      ELineCPsStatus curStatus = theStatus;
      theStatus = (ELineCPsStatus)((int)theStatus | (int)cp.data.status);
      if (curStatus != theStatus)
        lineCPStatusChangedEvent.Fire(new LineCPStatusEvArgs(this));

      return theStatus;
    }

    private ELineCPsStatus CalcStatus()
    {
      ELineCPsStatus curStatus = theStatus;
      theStatus = ELineCPsStatus.NotAvailable;
      foreach (IChartPoint cp in chartPoints)
        theStatus = (ELineCPsStatus)((int)theStatus | (int)cp.data.status);
      if (curStatus != theStatus)
        lineCPStatusChangedEvent.Fire(new LineCPStatusEvArgs(this));

      return theStatus;
    }

    private CP.Code.IClassMethodElement codeClassMethod;
    public ICPEvent<LineCPStatusEvArgs> lineCPStatusChangedEvent { get; set; } = new CPEvent<LineCPStatusEvArgs>();
    public ICPEvent<CPLineEvArgs> cpStatusChangedEvent { get; set; } = new CPEvent<CPLineEvArgs>();
    public ICPEvent<CPLineEvArgs> addCPEvent { get; set; } = new CPEvent<CPLineEvArgs>();
    public ICPEvent<CPLineEvArgs> remCPEvent { get; set; } = new CPEvent<CPLineEvArgs>();

    public ISet<IChartPoint> chartPoints { get; set; } =
      new SortedSet<IChartPoint>(
        Comparer<IChartPoint>.Create((lh, rh) => (lh.data.uniqueName.CompareTo(rh.data.uniqueName))));

    public int Count
    {
      get { return chartPoints.Count; }
    }

    public LineChartPoints(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData)
    {
      codeClassMethod = _classMethodElem;
      data = CP.Utils.IClassFactory.GetInstance().CreateLineCPsData(new TextPosition(_lineNum, _linePos), _fileData);
    }

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue(data.GetType().ToString(), data, data.GetType());
    //  info.AddValue("cpsPoints.Count", (UInt32)chartPoints.Count);
    //  foreach (IChartPoint cp in chartPoints)
    //  {
    //    info.AddValue("cp", cp, cp.GetType());
    //  }
    //}
    //public void MoveChartPoint(IChartPoint cp, int _lineNum, int _linePos)
    //{
    //  RemoveChartPoint(cp);
    //  theData.fileData.Move(this, cp, _lineNum, _linePos);
    //}

    public virtual IChartPoint GetChartPoint(string varName)
    {
      return chartPoints.FirstOrDefault((lp) => (lp.data.uniqueName == varName));
    }

    public bool AddChartPoint(string varName, out IChartPoint chartPnt)
    {
      chartPnt = null;
      IClassElement codeClass = codeClassMethod.GetClass();
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
        chartPnt.cpStatusChangedEvent += OnCPStatusChanged;
        UpdateStatus(chartPnt);
        addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));
        return true;
      }

      return false;
    }

    protected void OnCPStatusChanged(CPStatusEvArgs args)
    {
      CalcStatus();
      cpStatusChangedEvent.Fire(new CPLineEvArgs(this, args.cp));
    }

    public bool RemoveChartPoint(IChartPoint chartPnt)
    {
      bool ret = chartPoints.Remove(chartPnt);
      if (ret)
      {
        chartPnt.cpStatusChangedEvent -= OnCPStatusChanged;
        CalcStatus();
        remCPEvent.Fire(new CPLineEvArgs(this, chartPnt));
      }

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
      chartPnt = CP.Utils.IClassFactory.GetInstance().CreateCP(codeElem, data);
      chartPoints.Add(chartPnt);
      chartPnt.cpStatusChangedEvent += OnCPStatusChanged;
      UpdateStatus(chartPnt);
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
      chartPnt = CP.Utils.IClassFactory.GetInstance().CreateCP(codeElem, data);
      chartPoints.Add(chartPnt);
      chartPnt.cpStatusChangedEvent += OnCPStatusChanged;
      UpdateStatus(chartPnt);
      addCPEvent.Fire(new CPLineEvArgs(this, chartPnt));

      return true;
    }

    public bool SyncChartPoint(ICheckElem checkElem)
    {
      if (checkElem.exists)
      {
        IChartPoint chartPnt = null;
        AddChartPoint(checkElem.uniqueName, codeClassMethod.GetClass(), out chartPnt, false);
      }
      else
      {
        IChartPoint cp = chartPoints.FirstOrDefault((lp) => (lp.data.uniqueName == checkElem.uniqueName));
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
        if (cp.data.enabled)
        {
          IClassElement codeClass = codeClassMethod.GetClass();
          CPTraceVar traceVar = cp.CalcInjectionPoints(cpInjPoints, codeClass.name, out needDeclare);
          codeClass.CalcInjectionPoints(cpInjPoints, traceVar, needDeclare);
          model.CalcInjectionPoints(cpInjPoints, traceVar);
        }
      }
    }

    public void Invalidate()
    {
      foreach (IChartPoint cp in chartPoints)
        cp.Invalidate();
    }

    public bool Validate()
    {
      if(!codeClassMethod.Validate(data.pos))
      {
        Invalidate();

        return false;
      }
      foreach (IChartPoint cp in chartPoints)
      {
        if (!cp.Validate())
          cp.Invalidate();
        else if (cp.data.status == EChartPointStatus.NotAvailable)
          cp.SetStatus(EChartPointStatus.SwitchedOff);
      }

      return true;
    }

  }
}

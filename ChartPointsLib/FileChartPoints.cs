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
  public abstract class CPFileData : ICPFileData//, ISerializable
  {
    public string fileName { get; set;  }
    public string fileFullName { get; set; }
    public ICPProjectData projData { get; set; }

    //delegate void UpdatePosition(ILineChartPoints lcps, IChartPoint cp, int _lineNum, int _linePos);

    //private UpdatePosition updPos;

    public CPFileData(string _fileName, string _fileFullName, ICPProjectData _projData)//, Action<ILineChartPoints, IChartPoint, int, int> _updPos)
    {
      fileName = _fileName;
      fileFullName = _fileFullName.ToLower();
      projData = _projData;
      //updPos = new UpdatePosition(_updPos);
    }

    //private CPFileData(SerializationInfo info, StreamingContext context)
    //{
    //  fileName = info.GetString("fileName");
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("fileName", fileName);
    //}

    //public void Move(ILineChartPoints lcps ,IChartPoint cp, int _lineNum, int _linePos)
    //{
    //  updPos(lcps, cp, _lineNum, _linePos);
    //}
  }

  //[Serializable]
  public abstract class FileChartPoints : IFileChartPoints//Data<FileChartPoints, ICPFileData, CPFileData>, IFileChartPoints, ISerializable
  {
    public ICPFileData data { get; }
    public ICPEvent<CPFileEvArgs> addCPLineEvent { get; set; } = new CPEvent<CPFileEvArgs>();
    public ICPEvent<CPFileEvArgs> remCPLineEvent { get; set; } = new CPEvent<CPFileEvArgs>();
    public ICPEvent<CPLineMoveEvArgs> moveCPLineEvent { get; set; } = new CPEvent<CPLineMoveEvArgs>();

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
      data = CP.Utils.IClassFactory.GetInstance().CreateFileCPsData(_fileElem.name, _fileElem.uniqueName, _projData);
    }

    //private FileChartPoints(SerializationInfo info, StreamingContext context)
    //{
    //  theData = info.GetValue(Globals.GetTypeName(data), Globals.GetType(data)) as CPFileData;
    //  UInt32 Count = info.GetUInt32("linePoints.Count");
    //  for (uint i = 0; i < Count; ++i)
    //  {
    //    ILineChartPoints lineCPs = null;
    //    lineCPs = info.GetValue(Globals.GetTypeName(lineCPs), Globals.GetType(lineCPs)) as ILineChartPoints;
    //    AddLineChartPoints(lineCPs);
    //  }
    //}

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(data.GetType().ToString(), data, data.GetType());
      info.AddValue("linePoints.Count", (UInt32)linePoints.Count);
      foreach (ILineChartPoints lineCPs in linePoints)
      {
        info.AddValue("linePoints", lineCPs, lineCPs.GetType());
      }
    }

    //public void MoveChartPoint(ILineChartPoints _lcps, IChartPoint cp, int _lineNum, int _linePos)
    //{
    //  ILineChartPoints lcps = AddLineChartPoints(_lineNum, _linePos);
    //  lcps.AddChartPoint(cp);
    //}

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
        linePnts.remCPEvent += OnRemCp;
        addCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));

        return true;
      }

      return false;
    }

    public bool RemoveLineChartPoints(ILineChartPoints linePnts)
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
        int prevLine = ((TextPosition)((LineChartPoints)linePnts).data.pos).lineNum;
        ((TextPosition)((LineChartPoints)linePnts).data.pos).lineNum += linesAdd;
        linePoints.Add(linePnts);
        //addCPLineEvent.Fire(new CPFileEvArgs(this, linePnts));
        moveCPLineEvent.Fire(new CPLineMoveEvArgs(linePnts, prevLine, prevLine + linesAdd));
      }

      return ret;
    }

    public ILineChartPoints AddLineChartPoints(int lineNum, int linePos)
    {
      ILineChartPoints lPnts = GetLineChartPoints(lineNum);
      if (lPnts == null)
      {
        CP.Code.IClassMethodElement classMethodElem = fileElem.GetMethodFromFilePos(lineNum, linePos);
        if (classMethodElem != null)
        {
          lPnts = CP.Utils.IClassFactory.GetInstance().CreateLineCPs(classMethodElem, lineNum, linePos, data);
          AddLineChartPoints(lPnts);
        }
        //else
          //;
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
          if (lPnts.data.pos.lineNum >/*=*/ lineNum)
          {
            if (linesAdd < 0 && lineNum + (-linesAdd) > lPnts.data.pos.lineNum)
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
      //    //Globals.taggerUpdater?.RaiseChangeTagEvent(data.fileFullName, lPnts.Value);
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

    public void Invalidate()
    {
      foreach (var lPnts in linePoints)
        lPnts.Invalidate();
    }

    public bool Validate()
    {
      if(!fileElem.Validate(data.fileName))
      {
        Invalidate();

        return false;
      }
      foreach (var lPnts in linePoints)
        lPnts.Validate();

      return true;
    }
  }
}

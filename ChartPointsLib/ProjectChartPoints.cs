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
  public abstract class CPProjectData : ICPProjectData//, ISerializable
  {
    public string projName { get; set; }

    public CPProjectData(string _projName)
    {
      projName = _projName;
    }

    //private CPProjectData(SerializationInfo info, StreamingContext context)
    //{
    //  projName = info.GetString("projName");
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("projName", projName);
    //}
  }

  //[Serializable]
  public abstract class ProjectChartPoints : IProjectChartPoints// : Data<ProjectChartPoints, ICPProjectData, CPProjectData>, IProjectChartPoints//, ISerializable
  {
    public ICPProjectData data { get; }
    private CP.Code.IModel cpCodeModel;
    public ICPEvent<CPProjEvArgs> addCPFileEvent { get; set; } = new CPEvent<CPProjEvArgs>();
    public ICPEvent<CPProjEvArgs> remCPFileEvent { get; set; } = new CPEvent<CPProjEvArgs>();
    public ISet<IFileChartPoints> filePoints { get; set; } = new SortedSet<IFileChartPoints>(Comparer<IFileChartPoints>.Create((lh, rh) => (lh.data.fileName.CompareTo(rh.data.fileName))));

    public int Count { get { return filePoints.Count; } }

    public ProjectChartPoints(string _projName)
    {
      data = CP.Utils.IClassFactory.GetInstance().CreateProjCPsData(_projName);
      DTE2 dte2 = (DTE2)Globals.dte;
      Events2 evs2 = (Events2)dte2.Events;
      cpCodeModel = new CP.Code.Model(data.projName, evs2);
    }

    //private ProjectChartPoints(SerializationInfo info, StreamingContext context)
    //{
    //  theData = info.GetValue(Globals.GetTypeName(data), Globals.GetType(data)) as CPProjectData;
    //  DTE2 dte2 = (DTE2)Globals.dte;
    //  Events2 evs2 = (Events2)dte2.Events;
    //  cpCodeModel = new CP.Code.Model(data.projName, evs2);
    //  UInt32 Count = info.GetUInt32("filePoints.Count");
    //  for (uint i = 0; i < Count; ++i)
    //  {
    //    IFileChartPoints fileCPs = null;
    //    fileCPs = info.GetValue(Globals.GetTypeName(fileCPs), Globals.GetType(fileCPs)) as IFileChartPoints;
    //    AddFileChartPoints(fileCPs, false);
    //  }
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue(data.GetType().ToString(), data, data.GetType());
    //  info.AddValue("filePoints.Count", (UInt32)filePoints.Count);
    //  foreach (IFileChartPoints fileCPs in filePoints)
    //  {
    //    info.AddValue("filePoints", fileCPs, fileCPs.GetType());
    //  }
    //}

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
      filePnts.remCPLineEvent += OnRemLineCPs;
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
          fPnts = CP.Utils.IClassFactory.GetInstance().CreateFileCPs(fileElem, data);
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

    public void Invalidate()
    {
      foreach (IFileChartPoints fPnts in filePoints)
        fPnts.Invalidate();
    }

    public bool Validate()
    {
      if (!cpCodeModel.Validate())
      {
        Invalidate();

        return false;
      }
      foreach (IFileChartPoints fPnts in filePoints)
        fPnts.Validate();

      return true;
    }

  }

}

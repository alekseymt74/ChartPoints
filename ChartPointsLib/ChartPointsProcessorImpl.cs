using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ChartPoints.CPServices.impl;

namespace ChartPoints
{
  //[Serializable]
  public class ChartPointsProcessorData : IChartPointsProcessorData//, ISerializable
  {
    /// <summary>
    /// Container of all chartpoints set in current cpp project
    /// </summary>
    public ISet<IProjectChartPoints> projPoints { get; set; }
      = new SortedSet<IProjectChartPoints>(Comparer<IProjectChartPoints>.Create((lh, rh) => (lh.data.projName.CompareTo(rh.data.projName))));

    public ChartPointsProcessorData() {}

    //private ChartPointsProcessorData(SerializationInfo info, StreamingContext context)
    //{
    //  UInt32 Count = info.GetUInt32("projPoints.Count");
    //  for (uint i = 0; i < Count; ++i)
    //  {
    //    IProjectChartPoints projCPs = null;
    //    projCPs = info.GetValue(Globals.GetTypeName(projCPs), Globals.GetType(projCPs)) as IProjectChartPoints;
    //  }
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("projPoints.Count", (UInt32) projPoints.Count);
    //  foreach(IProjectChartPoints projCPs in projPoints)
    //  {
    //    info.AddValue("projPoints", projCPs, projCPs.GetType());
    //  }
    //}
  }
  /// <summary>
  /// Implementation of IChartPointsProcessor
  /// </summary>
  [Serializable]
  public class ChartPointsProcessor : IChartPointsProcessor, ISerializable
  {
    public IChartPointsProcessorData data { get; set; }

    public ChartPointsProcessor()
    {
      data = new ChartPointsProcessorData();
    }

    private ChartPointsProcessor(SerializationInfo info, StreamingContext context)
    {
      //data = info.GetValue(Globals.GetTypeName(data), Globals.GetType(data)) as IChartPointsProcessorData;
      UInt32 Count = info.GetUInt32("projPoints.Count");
      for (uint i = 0; i < Count; ++i)
      {
        IProjectChartPoints projCPs = null;
        projCPs = info.GetValue(Globals.GetTypeName(projCPs), Globals.GetType(projCPs)) as IProjectChartPoints;
        AddProjectChartPoints(projCPs);
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(Globals.GetTypeName(data), data, data.GetType());
      info.AddValue("projPoints.Count", (UInt32)data.projPoints.Count);
      foreach (IProjectChartPoints projCPs in data.projPoints)
      {
        info.AddValue("projPoints", projCPs, projCPs.GetType());
      }
    }

    public IProjectChartPoints GetProjectChartPoints(string projName)
    {
      IProjectChartPoints pPnts = data.projPoints.FirstOrDefault((pp) => (pp.data.projName == projName));

      return pPnts;
    }

    public bool HasData()
    {
      return (data.projPoints.Count != 0);
    }

    public bool AddProjectChartPoints(IProjectChartPoints pPnts)
    {
      pPnts.addCPFileEvent += AddProjectChartPoints;
      pPnts.remCPFileEvent += RemoveProjectChartPoints;
      data.projPoints.Add(pPnts);

      return true;
    }

    public bool AddProjectChartPoints(string projName, out IProjectChartPoints pPnts)
    {
      pPnts = GetProjectChartPoints(projName);
      if (pPnts == null)
      {
        pPnts = ChartPntFactory.Instance.CreateProjectChartPoint(projName);
        if(!pPnts.Validate())
        {
          pPnts = null;
          return false;
        }
        AddProjectChartPoints(pPnts);

        return true;
      }

      return false;
    }

    public bool RemoveChartPoints(string projName)
    {
      IProjectChartPoints pcp = GetProjectChartPoints(projName);
      if (pcp != null)
      {
        IFileChartPoints fCPs = null;
        while ((fCPs = pcp.filePoints.FirstOrDefault()) != null)
        {
          if (fCPs.linePoints.Count == 0)
            pcp.filePoints.Remove(fCPs);
          else
          {
            ILineChartPoints lCPs = null;
            while ((lCPs = fCPs.linePoints.FirstOrDefault()) != null)
            {
              if (lCPs.chartPoints.Count == 0)
                fCPs.RemoveLineChartPoints(lCPs);
              else
              {
                IChartPoint cp = null;
                while ((cp = lCPs.chartPoints.FirstOrDefault()) != null)
                  lCPs.RemoveChartPoint(cp);
              }
            }
          }
        }
        data.projPoints.Remove(pcp);
      }

      return true;
    }

    protected /*bool */void AddProjectChartPoints(CPProjEvArgs args)// IProjectChartPoints projPnts)
    {
      IProjectChartPoints pPnts = GetProjectChartPoints(/*projPnts*/args.projCPs.data.projName);
      if (pPnts == null)
      {
        data.projPoints.Add(/*projPnts*/args.projCPs);

        //return true;
      }

      //return false;
    }
    protected /*bool */void RemoveProjectChartPoints(CPProjEvArgs args)//IProjectChartPoints projPnts)
    {
      if (args.projCPs.Count == 0)
        data.projPoints.Remove(args.projCPs);
      //return data.projPoints.Remove(projPnts);
    }

    public void Validate()
    {
      foreach (IProjectChartPoints projCPs in data.projPoints)
      {
        bool ret = projCPs.Validate();
      }
    }

  }

}

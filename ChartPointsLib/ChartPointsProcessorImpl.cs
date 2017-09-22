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
  public class ChartPointsProcessorData : IChartPointsProcessorData
  {
    /// <summary>
    /// Container of all chartpoints set in current cpp project
    /// </summary>
    public ISet<IProjectChartPoints> projPoints { get; set; }
      = new SortedSet<IProjectChartPoints>(Comparer<IProjectChartPoints>.Create((lh, rh) => (lh.data.projName.CompareTo(rh.data.projName))));

  }
  /// <summary>
  /// Implementation of IChartPointsProcessor
  /// </summary>
  public class ChartPointsProcessor : IChartPointsProcessor
  {
    public IChartPointsProcessorData data { get; set; }

    public ChartPointsProcessor()
    {
      data = new ChartPointsProcessorData();
    }

    public IProjectChartPoints GetProjectChartPoints(string projName)
    {
      IProjectChartPoints pPnts = data.projPoints.FirstOrDefault((pp) => (pp.data.projName == projName));

      return pPnts;
    }

    public bool AddProjectChartPoints(string projName, out IProjectChartPoints pPnts)
    {
      pPnts = GetProjectChartPoints(projName);
      if (pPnts == null)
      {
        pPnts = ChartPntFactory.Instance.CreateProjectChartPoint(projName);
        pPnts.addCPFileEvent.On += AddProjectChartPoints;
        pPnts.remCPFileEvent.On += RemoveProjectChartPoints;
        data.projPoints.Add(pPnts);

        return true;
      }

      return false;
    }

    public bool RemoveChartPoints(string projName)
    {
      IProjectChartPoints pcp = GetProjectChartPoints(projName);
      if(pcp != null)
        data.projPoints.Remove(pcp);
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

  }

}

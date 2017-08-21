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
      = new SortedSet<IProjectChartPoints>(Comparer<IProjectChartPoints>.Create((lh, rh) => (lh.projName.CompareTo(rh.projName))));

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
      IProjectChartPoints pPnts = data.projPoints.FirstOrDefault((pp) => (pp.projName == projName));

      return pPnts;
    }

    public bool AddProjectChartPoints(string projName, out IProjectChartPoints pPnts)
    {
      pPnts = GetProjectChartPoints(projName);
      if (pPnts == null)
      {
        pPnts = ChartPntFactory.Instance.CreateProjectChartPoint(projName, AddProjectChartPoints, RemoveProjectChartPoints);
        data.projPoints.Add(pPnts);

        return true;
      }

      return false;
    }
    public ICheckPoint Check(string projName, TextPoint caretPnt)
    {
      CheckPoint checkPnt = null;
      VCCodeElement targetClassElem;
      for (;;)
      {
        // checks if we are in text document
        var activeDoc = Globals.dte.ActiveDocument;
        if (activeDoc == null)
          break;
        var textDoc = activeDoc.Object() as TextDocument;
        if (textDoc == null)
          break;
        // we work only with project items
        ProjectItem projItem = activeDoc.ProjectItem;
        if (projItem == null)
          break;
        // only cpp items
        FileCodeModel fcModel = projItem.FileCodeModel;
        if (fcModel == null)
          break;
        if (fcModel.Language != CodeModelLanguageConstants.vsCMLanguageVC)
          break;
        // chartpoint allowed only in class methods
        CodeElement elem = fcModel.CodeElementFromPoint(caretPnt, vsCMElement.vsCMElementFunction);
        if (elem == null)
          break;
        if (elem.Kind != vsCMElement.vsCMElementFunction)
          break;
        VCCodeFunction targetFunc = (VCCodeFunction)elem;
        if (targetFunc == null)
          break;
        // check that we are in method definition (not declaration) in case of declaration in one file & definition in other
        if (!targetFunc.File.Equals(Globals.dte.ActiveDocument.FullName, StringComparison.OrdinalIgnoreCase))
          break;
        // we are working only with class methods not global function
        targetClassElem = (VCCodeElement)targetFunc.Parent;
        if (targetClassElem == null)
          break;
        if (targetClassElem.Kind != vsCMElement.vsCMElementClass)
        {
          targetClassElem = null;
          break;
        }
        VCCodeClass ownerClass = (VCCodeClass)targetClassElem;
        ILineChartPoints lPnts = GetProjectChartPoints(projName)?.GetFileChartPoints(activeDoc.Name)?.GetLineChartPoints(caretPnt.Line);
        if (lPnts != null)
        {
          checkPnt = new CheckPoint()
          {
            doc = activeDoc,
            lineNum = lPnts.lineNum,
            linePos = lPnts.linePos,
            ownerClass = (VCCodeClass) targetClassElem,
            projName = projName
          };
          return checkPnt;
        }
        // check that we are inside method body (between '{' & '}'
        TextPoint startFuncPnt = targetFunc.StartPoint;
        TextPoint endFuncPnt = targetFunc.EndPoint;
        EditPoint startPnt = startFuncPnt.CreateEditPoint();
        EditPoint endPnt = endFuncPnt.CreateEditPoint();
        if (!startPnt.FindPattern("{"))
        {
          if (startPnt.GreaterThan(endPnt))
          {
            targetClassElem = null;
            break;
          }
        }
        startPnt.MoveToAbsoluteOffset(startPnt.AbsoluteCharOffset + 1);
        if (!endPnt.FindPattern("}", (int)vsFindOptions.vsFindOptionsBackwards))
        {
          if (endPnt.LessThan(startPnt))
          {
            targetClassElem = null;
            break;
          }
        }
        if (startPnt.GreaterThan(caretPnt) || endPnt.LessThan(caretPnt))
        {
          targetClassElem = null;
          break;
        }
        startFuncPnt = startPnt;
        endFuncPnt = endPnt;
        // all test successfully passed
        // check if chartpoint is already set
        int linePos = (caretPnt.Line == startPnt.Line ? startPnt.LineCharOffset /*+ 1*/ : 1/*0*/);
        //string fileFullName = System.IO.Path.GetFullPath(activeDoc.FullName).ToLower();
        //checkPnt = ChartPntFactory.Instance.CreateLineChartPoint(caretPnt.Line, linePos
        //  , (lp) => AddLineChartPoints(lp, projName, activeDoc.Name, fileFullName), null/*(lp) => RemoveLineChartPoints(lp, projName, activeDoc.Name, fileFullName)*/);
        checkPnt = new CheckPoint()
        {
          doc = activeDoc,
          lineNum = caretPnt.Line,
          linePos = linePos,
          ownerClass = ownerClass,
          projName = projName
        };
        break;
      }

      return checkPnt;
    }

    //public IDictionary<int, IChartPoint> GetOrCreateFileChartPoints(string fname)
    //{
    //  IDictionary<int, IChartPoint> fileChartPoints = GetFileChartPoints(fname);
    //  if (fileChartPoints == null)
    //  {
    //    fileChartPoints = new SortedDictionary<int, IChartPoint>();
    //    data.chartPoints.Add(fname, fileChartPoints);
    //  }

    //  return fileChartPoints;
    //}
    //protected void StoreLineChartPoints(ILineChartPoints chartPnt, string projName, string fileName)
    //{
    //  IDictionary<int, IChartPoint> fileChartPoints = GetOrCreateFileChartPoints(chartPnt.data.fileName);
    //  fileChartPoints.Add(chartPnt.data.lineNum, chartPnt);
    //}

    protected bool AddProjectChartPoints(IProjectChartPoints projPnts)
    {
      IProjectChartPoints pPnts = GetProjectChartPoints(projPnts.projName);
      if (pPnts == null)
      {
        data.projPoints.Add(projPnts);

        return true;
      }

      return false;
    }
    protected bool RemoveProjectChartPoints(IProjectChartPoints projPnts)
    {
      return data.projPoints.Remove(projPnts);
    }

    //protected bool AddLineChartPoints(ILineChartPoints linePnts, string projName, string fileName, string fileFullName)
    //{
    //  IProjectChartPoints pPnts = GetProjectChartPoints(projName);
    //  if (pPnts == null)
    //  {
    //    pPnts = ChartPntFactory.Instance.CreateProjectChartPoint(projName, AddProjectChartPoints, RemoveProjectChartPoints);
    //    IFileChartPoints fPnts = pPnts.AddFileChartPoints(fileName, fileFullName);
    //    fPnts.AddLineChartPoints(linePnts);
    //  }
    //  else
    //  {
    //    IFileChartPoints fPnts = pPnts.GetFileChartPoints(fileName);
    //    if (fPnts == null)
    //    {
    //      fPnts = pPnts.AddFileChartPoints(fileName, fileFullName);
    //      fPnts.AddLineChartPoints(linePnts);
    //    }
    //    else
    //      fPnts.AddLineChartPoints(linePnts);
    //  }
    //  //if (chartPnt == null || chartPnt.status != ETargetPointStatus.Available)
    //  //  return false;
    //  //StoreChartPnt(chartPnt);
    //  //Globals.taggerUpdater.RaiseChangeTagEvent(chartPnt);

    //  return true;
    //}

    //protected bool RemoveLineChartPoints(ILineChartPoints chartPnt, string projName, string fileName, string fileFullName)
    //{
    //  //if (chartPnt == null || (chartPnt.status != ETargetPointStatus.SwitchedOn && chartPnt.status != ETargetPointStatus.SwitchedOff))
    //  //  return false;
    //  //IDictionary<int, IChartPoint> fileChartPoints = GetFileChartPoints(chartPnt.data.fileName);
    //  //if (fileChartPoints == null)
    //  //  return false;
    //  //bool removed = fileChartPoints.Remove(chartPnt.data.lineNum);
    //  //if (removed && fileChartPoints.Count == 0)
    //  //  data.chartPoints.Remove(chartPnt.data.fileName);
    //  //Globals.taggerUpdater.RaiseChangeTagEvent(chartPnt/*.pnt*/);

    //  //return removed;

    //  return false;
    //}

    //public IDictionary<int, IChartPoint> GetFileChartPoints(string fileName)
    //{
    //  IDictionary<int, IChartPoint> fileChartPoints;
    //  data.chartPoints.TryGetValue(fileName, out fileChartPoints);

    //  return fileChartPoints;
    //}

    //public virtual IChartPoint GetChartPoint(IChartPointData cpData)
    //{
    //  IChartPoint cp = null;
    //  IDictionary<int, IChartPoint> fileChartPoints = GetFileChartPoints(cpData.fileName);
    //  if(fileChartPoints != null)
    //    fileChartPoints.TryGetValue(cpData.lineNum, out cp);

    //  return cp;
    //}

    //public bool AddChartPoint(IChartPointData chartPntData)
    //{
    //  IDictionary<int, IChartPoint> fileChartPoints = Globals.processor.GetOrCreateFileChartPoints(chartPntData.fileName);
    //  fileChartPoints.Add(chartPntData.lineNum, ChartPntFactory.Instance.CreateChartPoint(chartPntData, AddChartPoint, RemoveChartPoint));

    //  return true;
    //}
  }

}

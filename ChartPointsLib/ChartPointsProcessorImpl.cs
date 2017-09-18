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
        pPnts = ChartPntFactory.Instance.CreateProjectChartPoint(projName);//, AddProjectChartPoints, RemoveProjectChartPoints);
        pPnts.addCPFileEvent.On += AddProjectChartPoints;
        pPnts.remCPFileEvent.On += RemoveProjectChartPoints;
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
        VCCodeModel vcCodeModel = (VCCodeModel)projItem.ContainingProject.CodeModel;
        vcCodeModel.Synchronize();
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
        ILineChartPoints lPnts = GetProjectChartPoints(projName)?.GetFileChartPoints(activeDoc.Name)?.GetLineChartPoints(linePos);
        if (lPnts != null)
        {
          checkPnt = new CheckPoint()
          {
            doc = activeDoc,
            lineNum = lPnts.data.pos.lineNum,
            linePos = lPnts.data.pos.linePos,
            ownerClass = (VCCodeClass)targetClassElem,
            projName = projName
          };
          return checkPnt;
        }
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

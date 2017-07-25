﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{
  /// <summary>
  /// Implementation of IChartPoint interface
  /// </summary>
  public class ChartPoint : IChartPoint
  {
    private TextPoint startFuncPnt;
    private TextPoint endFuncPnt;
    private VCCodeElement targetClassElem;

    private TextPoint _textPnt;
    private VCCodeVariable _var;
    private ETargetPointStatus curStatus;
    public ETargetPointStatus status
    {
      get { return curStatus; }
      set { curStatus = value; }
    }
    public TextPoint pnt
    {
      get { return _textPnt; }
    }
    public VCCodeVariable var { get { return _var; } }

    public ChartPoint(TextPoint _startFuncPnt, TextPoint _endFuncPnt, VCCodeElement _targetClassElem)
    {
      startFuncPnt = _startFuncPnt;
      endFuncPnt = _endFuncPnt;
      targetClassElem = _targetClassElem;
      status = ETargetPointStatus.Available;
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
          return (status = ETargetPointStatus.ToggledOn);
        case ETargetPointStatus.ToggledOff:
          return (status = ETargetPointStatus.ToggledOn);
        case ETargetPointStatus.ToggledOn:
          return (status = ETargetPointStatus.ToggledOff);
      }

      return ETargetPointStatus.NotAvailable;
    }
  }

  /// <summary>
  /// Implementation of IChartPointsProcessor
  /// </summary>
  public class ChartPointsProcessor : IChartPointsProcessor
  {
    protected IDictionary<string, IDictionary<int, IChartPoint>> _chartPoints;

    public IDictionary<string, IDictionary<int, IChartPoint>> chartPoints
    {
      get { return _chartPoints; }
    }

    public ChartPointsProcessor()
    {
      _chartPoints = new SortedDictionary<string, IDictionary<int, IChartPoint>>();
    }
    public IChartPoint Check(TextPoint caretPnt)
    {
      IChartPoint targetPnt = null;
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
        // check if chartpoint is already set
        IDictionary<int, IChartPoint> fileChartPoints = GetFileChartPoints(activeDoc.FullName);
        if (fileChartPoints != null)
        {
          // if is set - return it
          IChartPoint chartPnt;
          if (fileChartPoints.TryGetValue(caretPnt.Line, out chartPnt))
            return chartPnt;
        }
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
        // create ChartPoint object & store it
        targetPnt = new ChartPoint(startFuncPnt, endFuncPnt, targetClassElem);
        if (fileChartPoints == null)
        {
          fileChartPoints = new SortedDictionary<int, IChartPoint>();
          fileChartPoints.Add(caretPnt.Line, targetPnt);
          _chartPoints.Add(activeDoc.FullName, fileChartPoints);
        }

        break;
      }

      return targetPnt;
    }

    public IDictionary<int, IChartPoint> GetFileChartPoints(string fileName)
    {
      IDictionary<int, IChartPoint> fileChartPoints;
      _chartPoints.TryGetValue(fileName, out fileChartPoints);

      return fileChartPoints;
    }
  }

}

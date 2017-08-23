using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{

  /// <summary>
  /// Implementation of IChartPntFactory
  /// </summary>
  public class ChartPntFactoryImpl : ChartPntFactory
  {
    public ChartPntFactoryImpl()
    {
      // check and set singleton factory instance
      // helps to hide the injection of another factory object, eg. factory stub in tests
      if (ChartPntFactory.factory == null)
        ChartPntFactory.factory = this;
    }

    public override IChartPointsProcessor CreateProcessor()
    {
      return new ChartPointsProcessor();
    }

    public override ICPOrchestrator CreateOrchestrator()
    {
      return new CPOrchestrator();
    }

    public override IProjectChartPoints CreateProjectChartPoint(string _projName,
      Func<IProjectChartPoints, bool> _addFunc, Func<IProjectChartPoints, bool> _remFunc)
    {
      return new ProjectChartPoints() { projName = _projName, addFunc = _addFunc, remFunc = _remFunc };
    }

    public override IFileChartPoints CreateFileChartPoint(string _fileName, string _fileFullName,
      Func<IFileChartPoints, bool> _addFunc, Func<IFileChartPoints, bool> _remFunc)
    {
      return new FileChartPoints() { fileName = _fileName, fileFullName = _fileFullName, addFunc = _addFunc, remFunc = _remFunc };
    }
    public override ILineChartPoints CreateLineChartPoint(int _lineNum, int _linePos, Func<ILineChartPoints, bool> _addFunc,
      Func<ILineChartPoints, bool> _remFunc)
    {
      return new LineChartPoints() {lineNum = _lineNum, linePos = _linePos, addFunc = _addFunc, remFunc = _remFunc };
    }

    //public override IChartPoint CreateChartPoint(TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
    //  , VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    //{
    //  return new ChartPoint(caretPnt, _startFuncPnt, _endFuncPnt, _targetClassElem, _addFunc, _remFunc);
    //}

    //public override IChartPoint CreateChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    //{
    //  return new ChartPoint(_data, _addFunc, _remFunc);
    //}
    public override IChartPoint CreateChartPoint(string varName, VCCodeClass ownerClass, Func<IChartPoint, bool> _addFunc,
      Func<IChartPoint, bool> _remFunc)
    {
      return new ChartPoint(varName, ownerClass, _addFunc, _remFunc);
    }
  }
}

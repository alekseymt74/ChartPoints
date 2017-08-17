using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;

namespace ChartPointsInstrTests
{
  public class ChartPoint : ChartPoints.ChartPoint
  {
    public ChartPoint(TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
      , VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
      : base(caretPnt, _startFuncPnt, _endFuncPnt, _targetClassElem, _addFunc, _remFunc)
    {
    }
    public ChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
      : base(_data, _addFunc, _remFunc)
    {
    }
    public ChartPoint(IChartPointData _data)
      : base(_data, null, null)
    {
    }

    public override void CalcInjectionPoints(out CPClassLayout cpInjPoints)
    {
      cpInjPoints = new CPClassLayout();
      CPTraceVar traceVar = new CPTraceVar();
      traceVar.filePos.fileName = "temp_utest.h";
      traceVar.filePos.pos.lineNum = 5;
      traceVar.filePos.pos.linePos = 6;
      traceVar.type = "int";
      cpInjPoints.traceVars.Add("j", traceVar);
      //cpInjPoints.injConstructorPos = new TextPos();
      cpInjPoints.traceVarInitPos.Add(new FilePosPnt() { fileName = "temp_utest.h", pos = { lineNum = 7, linePos = 18 }});
      cpInjPoints.traceVarInitPos.Add(new FilePosPnt() { fileName = "temp_utest.h", pos = { lineNum = 8, linePos = 25 }});
      FilePosText inclPos = new FilePosText()
      {
        fileName = "temp_utest.cpp",
        pos = { lineNum = 0, linePos = 1 },
        posEnd = { lineNum = 0, linePos = 24 }
      };
      CPInclude incl = new CPInclude()
      {
        inclOrig = "temp_utest.h",
        inclReplace = "__cp__.temp_utest.h",
        pos = inclPos
      };
      cpInjPoints.includesPos.Add(incl);
      inclPos = new FilePosText()
      {
        fileName = "test.cpp",
        pos = { lineNum = 5, linePos = 1 },
        posEnd = { lineNum = 5, linePos = 24 }
      };
      incl = new CPInclude()
      {
        inclOrig = "temp_utest.h",
        inclReplace = "__cp__.temp_utest.h",
        pos = inclPos
      };
      cpInjPoints.includesPos.Add(incl);
    }
  }

  public class ChartPointsProcessor : ChartPoints.ChartPointsProcessor
  {
    public new bool AddChartPoint(IChartPoint chartPnt)
    {
      StoreChartPnt(chartPnt);
      return true;
    }

    public void RemoveAllChartPoints()
    {
      data.chartPoints.Clear();
    }
    public override IChartPoint GetChartPoint(IChartPointData cpData)
    {
      IChartPoint chartPnt = null;
      if (cpData.varName == "j")
      {
        ((ChartPointData)cpData).className = "temp_utest";
        chartPnt = new ChartPoint(cpData);
      }

      return chartPnt;
    }
  }

  public class ChartPntInstrFactoryStub : ChartPntFactoryImpl
  {
    public ChartPntInstrFactoryStub()
    {
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
    public override IChartPoint CreateChartPoint(TextPoint caretPnt, TextPoint _startFuncPnt, TextPoint _endFuncPnt
      , VCCodeClass _targetClassElem, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    {
      return new ChartPoint(caretPnt, _startFuncPnt, _endFuncPnt, _targetClassElem, _addFunc, _remFunc);
    }

    public override IChartPoint CreateChartPoint(IChartPointData _data, Func<IChartPoint, bool> _addFunc, Func<IChartPoint, bool> _remFunc)
    {
      return new ChartPoint(_data, _addFunc, _remFunc);
    }
  }
}

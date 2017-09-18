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

    public ChartPoint(string varName, VCCodeClass ownerClass, ICPLineData _lineData)
      : base(varName, ownerClass, _lineData)
    {}

    public override void CalcInjectionPoints(/*out */CPClassLayout cpInjPoints, string _fname, int _lineNum, int _linePos)
    {
      //cpInjPoints = new CPClassLayout();
      CPTraceVar traceVar = null;//cpInjPoints.traceVarPos.FirstOrDefault((v) => (v.name == data.varName));
      if (/*traceVar == null)*/!cpInjPoints.traceVarPos.TryGetValue(data.varName, out traceVar))
      {
        traceVar = new CPTraceVar()
        {
          name = data.varName,
          type = "int",
          className = "temp_utest"/*,
          fileName = _fname,
          pos = {lineNum = _lineNum, linePos = _linePos}*/
        };
        cpInjPoints.traceVarPos.Add(data.varName, traceVar);
        // define trace var definition placement
        traceVar.defPos.fileName = "temp_utest.h";
        traceVar.defPos.pos.lineNum = 5;
        traceVar.defPos.pos.linePos = 6;
        traceVar.traceVarInitPos.Add(new FilePosPnt() {fileName = "temp_utest.h", pos = {lineNum = 7, linePos = 18 + 9}});
        traceVar.traceVarInitPos.Add(new FilePosPnt() {fileName = "temp_utest.h", pos = {lineNum = 8, linePos = 25 + 15}});
      }
      traceVar.traceVarTracePos.Add(new FilePosPnt()
      {
        fileName = _fname,
        pos = { lineNum = _lineNum, linePos = _linePos }
      });
      TextPos traceInclPos = null;
      if (!cpInjPoints.traceInclPos.TryGetValue(traceVar.defPos.fileName, out traceInclPos))
        cpInjPoints.traceInclPos.Add(traceVar.defPos.fileName, new TextPos() { lineNum = 0, linePos = 0 });
      CPInclude incl = null;
      if (!cpInjPoints.includesPos.TryGetValue(new Tuple<string, string>("temp_utest.h", "temp_utest.cpp"), out incl))
      {
        FilePosText inclPos = new FilePosText()
        {
          fileName = "temp_utest.cpp",
          pos = {lineNum = 0, linePos = 1},
          posEnd = {lineNum = 0, linePos = 24}
        };
        incl = new CPInclude()
        {
          inclOrig = "temp_utest.h",
          inclReplace = "__cp__.temp_utest.h",
          pos = inclPos
        };
        cpInjPoints.includesPos.Add(new Tuple<string, string>(incl.inclOrig, incl.pos.fileName), incl);
      }
      incl = null;
      if (!cpInjPoints.includesPos.TryGetValue(new Tuple<string, string>("temp_utest.h", "test.cpp"), out incl))
      {
        FilePosText inclPos = new FilePosText()
        {
          fileName = "test.cpp",
          pos = {lineNum = 5, linePos = 1},
          posEnd = {lineNum = 5, linePos = 24}
        };
        incl = new CPInclude()
        {
          inclOrig = "temp_utest.h",
          inclReplace = "__cp__.temp_utest.h",
          pos = inclPos
        };
        cpInjPoints.includesPos.Add(new Tuple<string, string>(incl.inclOrig, incl.pos.fileName), incl);
      }
    }
  }

  public class ChartPointsProcessor : ChartPoints.ChartPointsProcessor
  {
    public void RemoveAllChartPoints()
    {
      //data.chartPoints.Clear();
      data.projPoints.Clear();
    }
  }

  public class LineChartPoints : ChartPoints.LineChartPoints
  {
    public LineChartPoints(int _lineNum, int _linePos, ICPFileData _fileData)
      : base(_lineNum, _linePos, _fileData)
    { }
    public override IChartPoint GetChartPoint(string varName)
    {
      IChartPoint chartPnt = base.GetChartPoint(varName);
      if (chartPnt != null && varName == "j")
        ((ChartPoints.ChartPointData)chartPnt.data).className = "temp_utest";

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
    public override ILineChartPoints CreateLineChartPoint(int _lineNum, int _linePos, ICPFileData _fileData)
    {
      return new LineChartPoints(_lineNum, _linePos, _fileData);
    }
    public override IChartPoint CreateChartPoint(string varName, VCCodeClass ownerClass, ICPLineData _lineData)
    {
      return new ChartPoint(varName, ownerClass, _lineData);
    }
  }
}

using System;
using ChartPoints;

namespace ChartPointsInstrTests
{
  public class ChartPoint : ChartPoints.ChartPoint
  {

    public ChartPoint(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData)
      : base(_codeElem, _lineData)
    {}

    public override CPTraceVar CalcInjectionPoints(CPClassLayout cpInjPoints, string className, out bool needDeclare)//, string _fname, int _lineNum, int _linePos)
    {
      needDeclare = false;
      return null;
      //  //cpInjPoints = new CPClassLayout();
      //  CPTraceVar traceVar = null;//cpInjPoints.traceVarPos.FirstOrDefault((v) => (v.name == data.varName));
      //  if (/*traceVar == null)*/!cpInjPoints.traceVarPos.TryGetValue(data.name, out traceVar))
      //  {
      //    traceVar = new CPTraceVar()
      //    {
      //      name = data.name,
      //      type = "int",
      //      className = "temp_utest"/*,
      //      fileName = _fname,
      //      pos = {lineNum = _lineNum, linePos = _linePos}*/
      //    };
      //    cpInjPoints.traceVarPos.Add(data.name, traceVar);
      //    // define trace var definition placement
      //    traceVar.defPos.fileName = "temp_utest.h";
      //    traceVar.defPos.pos.lineNum = 5;
      //    traceVar.defPos.pos.linePos = 6;
      //    traceVar.traceVarInitPos.Add(new FilePosPnt() {fileName = "temp_utest.h", pos = {lineNum = 7, linePos = 18 + 9}});
      //    traceVar.traceVarInitPos.Add(new FilePosPnt() {fileName = "temp_utest.h", pos = {lineNum = 8, linePos = 25 + 15}});
      //  }
      //  traceVar.traceVarTracePos.Add(new FilePosPnt()
      //  {
      //    fileName = _fname,
      //    pos = { lineNum = _lineNum, linePos = _linePos }
      //  });
      //  TextPos traceInclPos = null;
      //  if (!cpInjPoints.traceInclPos.TryGetValue(traceVar.defPos.fileName, out traceInclPos))
      //    cpInjPoints.traceInclPos.Add(traceVar.defPos.fileName, new TextPos() { lineNum = 0, linePos = 0 });
      //  CPInclude incl = null;
      //  if (!cpInjPoints.includesPos.TryGetValue(new Tuple<string, string>("temp_utest.h", "temp_utest.cpp"), out incl))
      //  {
      //    FilePosText inclPos = new FilePosText()
      //    {
      //      fileName = "temp_utest.cpp",
      //      pos = {lineNum = 0, linePos = 1},
      //      posEnd = {lineNum = 0, linePos = 24}
      //    };
      //    incl = new CPInclude()
      //    {
      //      inclOrig = "temp_utest.h",
      //      inclReplace = "__cp__.temp_utest.h",
      //      pos = inclPos
      //    };
      //    cpInjPoints.includesPos.Add(new Tuple<string, string>(incl.inclOrig, incl.pos.fileName), incl);
      //  }
      //  incl = null;
      //  if (!cpInjPoints.includesPos.TryGetValue(new Tuple<string, string>("temp_utest.h", "test.cpp"), out incl))
      //  {
      //    FilePosText inclPos = new FilePosText()
      //    {
      //      fileName = "test.cpp",
      //      pos = {lineNum = 5, linePos = 1},
      //      posEnd = {lineNum = 5, linePos = 24}
      //    };
      //    incl = new CPInclude()
      //    {
      //      inclOrig = "temp_utest.h",
      //      inclReplace = "__cp__.temp_utest.h",
      //      pos = inclPos
      //    };
      //    cpInjPoints.includesPos.Add(new Tuple<string, string>(incl.inclOrig, incl.pos.fileName), incl);
      //  }
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
    public LineChartPoints(CP.Code.IClassElement _classElem, int _lineNum, int _linePos, ICPFileData _fileData)
      : base(_classElem, _lineNum, _linePos, _fileData)
    { }
    public override IChartPoint GetChartPoint(string varName)
    {
      IChartPoint chartPnt = base.GetChartPoint(varName);
      //////if (chartPnt != null && varName == "j")
      //////  ((ChartPoints.ChartPointData)chartPnt.data).className = "temp_utest";

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
    public override ILineChartPoints CreateLineChartPoint(CP.Code.IClassElement _classElem, int _lineNum, int _linePos, ICPFileData _fileData)
    {
      return new LineChartPoints(_classElem, _lineNum, _linePos, _fileData);
    }
    public override IChartPoint CreateChartPoint(CP.Code.IClassVarElement codeElem, ICPLineData _lineData)
    {
      return new ChartPoint(codeElem, _lineData);
    }
  }
}

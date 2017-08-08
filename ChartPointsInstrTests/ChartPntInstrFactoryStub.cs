using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;

namespace ChartPointsInstrTests
{
  //public class ChartPoint : ChartPoints.ChartPoint
  //{
  //  public ChartPoint(string _fileName, Int32 _lineNum, string _varName, bool _enabled)
  //  {
  //    theData = new ChartPointData();
  //    theData.fileName = _fileName;
  //    theData.lineNum = _lineNum;
  //    theData.varName = _varName;
  //    theData.enabled = _enabled;
  //  }
  //}
  public class ChartPointsProcessor : ChartPoints.ChartPointsProcessor
  {
    public /*override */bool AddChartPoint(IChartPoint chartPnt)
    {
      StoreChartPnt(chartPnt);
      return true;
    }

    public void RemoveAllChartPoints()
    {
      data.chartPoints.Clear();
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
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChartPoints;
using Microsoft.Build.Evaluation;

namespace ChartPointsInstrTests
{
  [TestClass]
  public class InstrUTest
  {
    public static ChartPointsInstrTests.ChartPointsProcessor processor;
    public static string testProjFName = @"e:\projects\tests\MSVS.ext\ChartPoints\cpp_test_proj\test.instrTest.vcxproj";

    [ClassInitialize]
    public static void TestClassInitialize(TestContext testContext)
    {
      //File.Copy(@"e:\projects\tests\MSVS.ext\ChartPoints\cpp_test_proj\test.vcxproj", testProjFName, true);
      ChartPntInstrFactoryStub stubFactory = new ChartPntInstrFactoryStub();
      Globals.processor = stubFactory.CreateProcessor();
      processor = (ChartPointsInstrTests.ChartPointsProcessor) Globals.processor;
    }

    void CheckChartPointData(string fileName, int lineNum, IChartPointData chartPntData)
    {
      IDictionary<int, IChartPoint> chartPnts = processor.GetFileChartPoints(fileName);
      IChartPoint chartPnt = null;
      bool ret = chartPnts.TryGetValue(lineNum, out chartPnt);
      Assert.AreEqual(ret, true);
      Assert.AreEqual(chartPnt.data.fileName, chartPntData.fileName);
      Assert.AreEqual(chartPnt.data.enabled, chartPntData.enabled);
      Assert.AreEqual(chartPnt.data.lineNum, chartPntData.lineNum);
      Assert.AreEqual(chartPnt.data.linePos, chartPntData.linePos);
      Assert.AreEqual(chartPnt.data.varName, chartPntData.varName);
    }
    [TestMethod]
    public void CheckSaveLoadChartpoints()
    {
      //string fileName = "temp_utest.h";
      //int lineNum = 7;
      //int linePos = 11;
      //bool enabled = true;
      //string varName = "i";
      ChartPointData chartPntData = new ChartPointData {fileName = "temp_utest.h", lineNum = 7, linePos = 11, varName = "i", enabled = true};
      IChartPoint chartPnt = new ChartPoint(chartPntData, null, null);//cppFName, lineNum, varName, enabled);
      processor.AddChartPoint(chartPnt);
      CheckChartPointData(chartPntData.fileName, chartPntData.lineNum, chartPntData);
      //IDictionary<int, IChartPoint> fileChartPnts = processor.GetFileChartPoints(fileName);
      //Assert.AreEqual(fileChartPnts.Count, 1);
      //KeyValuePair<int, IChartPoint> lineChartPnt = fileChartPnts.ElementAt(0);
      //Assert.AreEqual(lineChartPnt.Key, lineNum);
      //IChartPointData addedChartPntData = lineChartPnt.Value.data;
      //Assert.AreEqual(addedChartPntData.lineNum, lineNum);
      //Assert.AreEqual(addedChartPntData.linePos, linePos);
      //Assert.AreEqual(addedChartPntData.enabled, enabled);
      //Assert.AreEqual(addedChartPntData.fileName, fileName);
      //Assert.AreEqual(addedChartPntData.varName, varName);
      ICPOrchestrator cpOrchestrator = new CPOrchestrator();
      cpOrchestrator.SaveProjChartPonts(testProjFName);
      processor.RemoveAllChartPoints();
      Assert.AreEqual(processor.GetFileChartPoints(chartPntData.fileName), null);
      cpOrchestrator.LoadProjChartPoints(testProjFName);
      Assert.AreEqual(processor.GetFileChartPoints(chartPntData.fileName).Count, 1);
      CheckChartPointData(chartPntData.fileName, chartPntData.lineNum, chartPntData);
      //IDictionary<int, IChartPoint> chartPnts = processor.GetFileChartPoints(fileName);
      //chartPnt = null;
      //bool ret = chartPnts.TryGetValue(lineNum, out chartPnt);
      //Assert.AreEqual(chartPnt.data.fileName, fileName);
      //Assert.AreEqual(chartPnt.data.enabled, enabled);
      //Assert.AreEqual(chartPnt.data.lineNum, lineNum);
      //Assert.AreEqual(chartPnt.data.linePos, linePos);
      //Assert.AreEqual(chartPnt.data.varName, varName);
      cpOrchestrator.SaveProjChartPonts(testProjFName);
      cpOrchestrator.Orchestrate(testProjFName);
    }

    [TestMethod]
    public void BuildTest()
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(testProjFName);
      msbuildProj.Build();
      Assert.AreEqual(File.Exists(@"e:\projects\tests\MSVS.ext\ChartPoints\cpp_test_proj\__cp__.temp_utest.h"), true);
    }
  }
}

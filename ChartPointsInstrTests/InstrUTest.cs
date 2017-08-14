using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
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
    public static ICPOrchestrator cpOrchestrator;

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
      ChartPointData chartPntData = new ChartPointData {fileName = "temp_utest.cpp", lineNum = 4, linePos = 2, varName = "j", enabled = true};
      IChartPoint chartPnt = new ChartPoint(chartPntData);
      processor.AddChartPoint(chartPnt);
      CheckChartPointData(chartPntData.fileName, chartPntData.lineNum, chartPntData);
      cpOrchestrator = new CPOrchestrator();
      cpOrchestrator.SaveProjChartPonts(testProjFName);
      processor.RemoveAllChartPoints();
      Assert.AreEqual(processor.GetFileChartPoints(chartPntData.fileName), null);
      cpOrchestrator.LoadProjChartPoints(testProjFName);
      Assert.AreEqual(processor.GetFileChartPoints(chartPntData.fileName).Count, 1);
      CheckChartPointData(chartPntData.fileName, chartPntData.lineNum, chartPntData);
      cpOrchestrator.SaveProjChartPonts(testProjFName);
      cpOrchestrator.Orchestrate(testProjFName);
    }

    [TestMethod]
    public void BuildTest()
    {
      string address = "net.pipe://localhost/ChartPoints/IPCChartPoint";
      ServiceHost serviceHost = new ServiceHost(typeof(IPCChartPoint));
      NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
      serviceHost.AddServiceEndpoint(typeof(IIPCChartPoint), binding, address);
      serviceHost.Open();

      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(testProjFName);
      msbuildProj.Build();
      Assert.AreEqual(File.Exists(@"e:\projects\tests\MSVS.ext\ChartPoints\cpp_test_proj\__cp__.temp_utest.cpp"), true);
    }
  }
}

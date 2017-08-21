﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChartPoints;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;

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

    void CheckChartPointData(string fileName, int lineNum, CPData cpData)
    {
      IProjectChartPoints pPnts = processor.GetProjectChartPoints(cpData.projName);
      Assert.AreNotEqual(pPnts, null);
      IFileChartPoints fPnts = pPnts.GetFileChartPoints(cpData.fileName);
      Assert.AreNotEqual(fPnts, null);
      ILineChartPoints lPnts = fPnts.GetLineChartPoints(cpData.lineNum);
      Assert.AreNotEqual(lPnts, null);
      IChartPoint chartPnt = lPnts.GetChartPoint(cpData.varName);
      Assert.AreNotEqual(chartPnt, null);
      Assert.AreEqual(chartPnt.data.enabled, cpData.enabled);
    }
    [TestMethod]
    public void CheckSaveLoadChartpoints()
    {
      CPData cpData = new CPData { fileName = "temp_utest.cpp", projName = "test.instrTest", lineNum = 4, linePos = 2, varName = "j", enabled = true};

      IProjectChartPoints pPnts = null;
      Globals.processor.AddProjectChartPoints(cpData.projName, out pPnts);
      IFileChartPoints fPnts = pPnts.AddFileChartPoints(cpData.fileName, "!!!!!!!!!!!!");//Path.GetDirectoryName(projConfFile) + "\\" + cpFileElem.Include);
      ILineChartPoints lPnts = fPnts.AddLineChartPoints(cpData.lineNum, cpData.linePos);
      IChartPoint chartPnt = null;
      lPnts.AddChartPoint(cpData.varName, null/*"!!!!!!!"*/, out chartPnt);//!!!!!!!!!!!!!!!!
      CheckChartPointData(cpData.fileName, cpData.lineNum, cpData);
      cpOrchestrator = new CPOrchestrator();
      Microsoft.Build.Evaluation.Project msBuildProject = cpOrchestrator.SaveProjChartPonts(testProjFName);
      msBuildProject.Save();
      processor.RemoveAllChartPoints();
      pPnts = processor.GetProjectChartPoints(cpData.projName);
      Assert.AreEqual(pPnts, null);
      cpOrchestrator.LoadProjChartPoints(testProjFName);
      CheckChartPointData(cpData.fileName, cpData.lineNum, cpData);
      msBuildProject = cpOrchestrator.SaveProjChartPonts(testProjFName);
      msBuildProject.Save();
      msBuildProject = cpOrchestrator.Orchestrate(testProjFName);
      msBuildProject.Save();

      //IChartPoint chartPnt = new ChartPoint(cpData);
      //processor.AddChartPoint(chartPnt);
      //CheckChartPointData(cpData.fileName, cpData.lineNum, cpData);
      //cpOrchestrator = new CPOrchestrator();
      //Microsoft.Build.Evaluation.Project msBuildProject = cpOrchestrator.SaveProjChartPonts(testProjFName);
      //msBuildProject.Save();
      //processor.RemoveAllChartPoints();
      //Assert.AreEqual(processor.GetFileChartPoints(cpData.fileName), null);
      //cpOrchestrator.LoadProjChartPoints(testProjFName);
      //Assert.AreEqual(processor.GetFileChartPoints(cpData.fileName).Count, 1);
      //CheckChartPointData(cpData.fileName, cpData.lineNum, cpData);
      //msBuildProject = cpOrchestrator.SaveProjChartPonts(testProjFName);
      //msBuildProject.Save();
      //msBuildProject = cpOrchestrator.Orchestrate(testProjFName);
      //msBuildProject.Save();
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
      msbuildProj.Build(new ConsoleLogger());
      Assert.AreEqual(File.Exists(@"e:\projects\tests\MSVS.ext\ChartPoints\cpp_test_proj\__cp__.temp_utest.cpp"), true);
    }
  }
}

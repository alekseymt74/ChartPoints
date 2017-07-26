using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnvDTE;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ChartPoints;
using Microsoft.VisualStudio;
using Microsoft.CSharp.RuntimeBinder;

namespace ChartPointsTests
{
  [TestClass]
  public class InitializationUTest
  {
    public static ChartPntFactoryStub stubFactory;
    public static ChartPointsPackage chartPntPackage;
    public static DTE dte;
    public static TestProject testProj;

    [ClassInitialize]
    [HostType("VS IDE")]
    public static void TestClassInitialize(TestContext testContext)
    {
      dte = VsIdeTestHostContext.Dte;
      if (MessageBox.Show("Attach VS Experimental instance to debugger?", "Attach to debugger", MessageBoxButtons.OKCancel) == DialogResult.OK)
      {
        //var thisDte = (DTE) Marshal.GetActiveObject("VisualStudio.DTE.14.0");
        //EnvDTE.Processes processes = thisDte.Debugger.LocalProcesses;
        //foreach (EnvDTE.Process proc in processes)
        //{
        //  if (proc.Name.IndexOf("devenv.exe") != -1)
        //    proc.Attach();
        //}
      }
      var shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
      Guid packageGuid = new Guid("a6b9b46f-0163-4255-807e-b3e04aa6ca70");
      IVsPackage package;
      stubFactory = new ChartPntFactoryStub();
      int res = shellService.LoadPackage(ref packageGuid, out package);
      chartPntPackage = (ChartPointsPackage)package;
      testProj = new TestProject(dte);
      testProj.Open("e:/projects/tests/VSIX_VCCodeProject/test/test.sln");//!!!CHECK!!!
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestThePackageAcquisition()
    {
      Assert.AreNotEqual(InitializationUTest.chartPntPackage, null);
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestStubFactoryInjection()
    {
      Assert.AreEqual(InitializationUTest.stubFactory, ChartPntFactory.Instance);
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestToggleChartpoint()
    {
      TextSelection ts = testProj.OpenTestHeaderItem();
      string utestHeaderText = "#ifndef _TEMP_UTEST_H\n#define _TEMP_UTEST_H\n\n"
                  + "class temp_utest\n{\npublic:\nvoid f1(int i);\nvoid f2() { ";
      int pos_f2_body = utestHeaderText.Length;
      utestHeaderText += "}\n};\n\n#endif // _TEMP_UTEST_H";
      int posHeaderEnd = utestHeaderText.Length;
      ts.Text = utestHeaderText;
      testProj.SaveCurrentItem();
      IChartPoint chartPnt = null;
      for (int i = 1; i < pos_f2_body; ++i)
      {
        ts.MoveToAbsoluteOffset(i);
        chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
        Assert.AreEqual(chartPnt, null);
      }
      for (int i = pos_f2_body; i < pos_f2_body + 2; ++i)
      {
        ts.MoveToAbsoluteOffset(i);
        chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
        Assert.AreNotEqual(chartPnt, null);
        Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
      }
      for (int i = pos_f2_body + 2; i <= posHeaderEnd; ++i)
      {
        ts.MoveToAbsoluteOffset(i);
        chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
        Assert.AreEqual(chartPnt, null);
      }

      ts.MoveToAbsoluteOffset(pos_f2_body);
      chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
      Assert.AreNotEqual(chartPnt, null);
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
      chartPnt.Toggle();
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOn);
      chartPnt.Toggle();
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOff);
      ts.MoveToAbsoluteOffset(pos_f2_body + 1);
      chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
      Assert.AreNotEqual(chartPnt, null);
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOff);
      ts.MoveToAbsoluteOffset(pos_f2_body + 2);
      chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
      Assert.AreEqual(chartPnt, null);
      for (int i = pos_f2_body + 3; i <= posHeaderEnd; ++i)
      {
        ts.MoveToAbsoluteOffset(i);
        chartPnt = ChartPoints.Globals.processor.Check(ts.ActivePoint);
        Assert.AreEqual(chartPnt, null);
      }
    }
  }
}

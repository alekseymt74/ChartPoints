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
      testProj.Open("../../../cpp_test_proj/test.sln");//!!!CHECK!!!
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

  }
}

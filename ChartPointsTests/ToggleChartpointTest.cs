using System;
using System.Text;
using System.Collections.Generic;
using ChartPoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChartPointsTests
{
  /// <summary>
  /// Summary description for ToggleChartpoint
  /// </summary>
  [TestClass]
  public class ToggleChartpointTest
  {
    private TestProjectItem header;
    private TestProjectItem src;
    private int pos_f2_body;
    private int pos_f1_body;
    private int posHeaderEnd;

    public ToggleChartpointTest()
    {
      header = InitializationUTest.testProj.projHeaderItem;
      header.Open(true);

      string utestHeaderText = "#ifndef _TEMP_UTEST_H\n#define _TEMP_UTEST_H\n\n"
                  + "class temp_utest\n{\npublic:\nvoid f1(int i);\nvoid f2() { ";
      pos_f2_body = utestHeaderText.Length;
      utestHeaderText += "}\n};\nvoid temp_utest::f1(int i){";
      pos_f1_body = utestHeaderText.Length + 1;
      utestHeaderText += "}\n\n#endif // _TEMP_UTEST_H";
      posHeaderEnd = utestHeaderText.Length;
      header.SetContent(utestHeaderText);
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    private void CheckRange(TestProjectItem projItem, int from, int to, Action<IChartPoint> checks )
    {
      for (int i = from; i < to; ++i)
      {
        projItem.SetPos(i);
        IChartPoint chartPnt = ChartPoints.Globals.processor.Check(projItem.ActivePoint);
        checks(chartPnt);
      }
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestChartpointAvailability()
    {
      CheckRange(header, 1, pos_f2_body, cp => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_f2_body, pos_f2_body + 2, cp =>
      {
        Assert.AreNotEqual(cp, null);
        Assert.AreEqual(cp.status, ETargetPointStatus.Available);
      });
      CheckRange(header, pos_f2_body + 2, pos_f1_body, cp => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_f1_body, pos_f1_body + 1, cp =>
      {
        Assert.AreNotEqual(cp, null);
        Assert.AreEqual(cp.status, ETargetPointStatus.Available);
      });
      CheckRange(header, pos_f1_body + 1, posHeaderEnd + 1, cp => { Assert.AreEqual(cp, null); });
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestToggleChartpoint()
    {
      IChartPoint chartPnt = null;
      header.SetPos(pos_f2_body);
      chartPnt = ChartPoints.Globals.processor.Check(header.ActivePoint);
      Assert.AreNotEqual(chartPnt, null);
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
      chartPnt.Toggle();
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOn);
      chartPnt.Toggle();
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestExistingChartpoint()
    {
      header.SetPos(pos_f2_body);
      IChartPoint chartPnt1 = ChartPoints.Globals.processor.Check(header.ActivePoint);
      Assert.AreNotEqual(chartPnt1, null);
      chartPnt1.Toggle();
      header.SetPos(pos_f2_body + 1);
      IChartPoint chartPnt2 = ChartPoints.Globals.processor.Check(header.ActivePoint);
      Assert.AreEqual(chartPnt1, chartPnt2);
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestUnavailableCPOnToggledLine()
    {
      header.SetPos(pos_f2_body + 2);
      IChartPoint chartPnt = ChartPoints.Globals.processor.Check(header.ActivePoint);
      Assert.AreEqual(chartPnt, null);
    }

  }
}

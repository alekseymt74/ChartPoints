using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
    private int pos_f1_body_end;
    private int posHeaderEnd;
    private int pos_constr1_body;
    private int pos_constr2_body;

    public ToggleChartpointTest()
    {
      src = InitializationUTest.testProj.projSrcItem;
      src.Open(true);
      string utestSrcText = "#include \"temp_utest.h\"\n#include <iostream>\nvoid temp_utest::f3()\n{\n++j;\n}";
      src.SetContent(utestSrcText);

      header = InitializationUTest.testProj.projHeaderItem;
      header.Open(true);

      string utestHeaderText = "#ifndef _TEMP_UTEST_H\n#define _TEMP_UTEST_H\n\n"
                               + "class temp_utest\n{\nint j;int k;\npublic:\ntemp_utest():j(0), k(1000){";
      pos_constr1_body = utestHeaderText.Length + 1;
      utestHeaderText += "}\ntemp_utest(int _j, int _k):j(_j), k(_k){";
      pos_constr2_body = utestHeaderText.Length + 1;
      utestHeaderText += "}\nvoid f2() { ";
      pos_f2_body = utestHeaderText.Length + 1 - 1;
      utestHeaderText += "}\nvoid f3();\nvoid f1(int i){";
      pos_f1_body = utestHeaderText.Length + 1;
      utestHeaderText += "\n--k;\n";
      pos_f1_body_end = utestHeaderText.Length + 1;
      utestHeaderText += "}\n};\n\n#endif // _TEMP_UTEST_H";
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

    private void CheckRange(TestProjectItem projItem, int from, int to, Action<ICheckPoint, int> checks )
    {
      for (int i = from; i < to; ++i)
      {
        projItem.SetPos(i);
        ICheckPoint chartPnt = ChartPoints.Globals.processor.Check("test", projItem.ActivePoint);
        checks(chartPnt, projItem.ActivePoint.Line);
      }
    }

    private void CheckToggledChartPoint(ICheckPoint cp, int line)
    {
      Assert.AreNotEqual(cp, null);
      List<Tuple<string, string, bool>> _availableVars = null;
      cp.GetAvailableVars(out _availableVars);
      Assert.AreEqual(_availableVars.Count, 2);
      Tuple<string, string, bool> checkPntData = _availableVars.ElementAt(0);
      Assert.AreEqual(checkPntData.Item1, "j");
      Assert.AreEqual(checkPntData.Item2, "int");
      Tuple<string, string, bool> checkPntData1 = _availableVars.ElementAt(1);
      Assert.AreEqual(checkPntData1.Item1, "k");
      Assert.AreEqual(checkPntData1.Item2, "int");
      ////Assert.AreEqual(checkPntData.Item3, false);
      ISet<string> selVars = new SortedSet<string>() { checkPntData.Item1 };
      cp.SyncChartPoints(selVars);
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints("test");
      Assert.AreNotEqual(pPnts, null);
      IFileChartPoints fPnts = pPnts.GetFileChartPoints("temp_utest.h");
      Assert.AreNotEqual(fPnts, null);
      //Assert.AreEqual(fPnts.linePoints.Count, line);
      ILineChartPoints lPnts = fPnts.GetLineChartPoints(line);//linePoints.ElementAt(line);
      Assert.AreEqual(lPnts.chartPoints.Count, 1);
      IChartPoint chartPnt = lPnts.GetChartPoint("j");
      Assert.AreNotEqual(chartPnt, null);
      Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOn);
    }

    [TestMethod]
    [HostType("VS IDE")]
    [TestProperty("VsHiveName", "14.0Exp")]
    public void TestChartpointAvailability()
    {
      CheckRange(header, 1, pos_constr1_body, (cp, l) => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_constr1_body, pos_constr1_body + 1, (cp, l) =>
      {
        CheckToggledChartPoint(cp, l);
      });
      CheckRange(header, pos_constr1_body + 1, pos_constr2_body, (cp, l) => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_constr2_body, pos_constr2_body + 1, (cp, l) =>
      {
        CheckToggledChartPoint(cp, l);
      });
      CheckRange(header, pos_constr2_body + 1, pos_f2_body, (cp, l) => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_f2_body, pos_f2_body + 2, (cp, l) =>
      {
        CheckToggledChartPoint(cp, l);
      });
      CheckRange(header, pos_f2_body + 2, pos_f1_body, (cp, l) => { Assert.AreEqual(cp, null); });
      CheckRange(header, pos_f1_body, pos_f1_body_end + 1, (cp, l) =>
      {
        CheckToggledChartPoint(cp, l);
      });
      CheckRange(header, pos_f1_body_end + 1, posHeaderEnd + 1, (cp, l) => { Assert.AreEqual(cp, null); });
    }

    //[TestMethod]
    //[HostType("VS IDE")]
    //[TestProperty("VsHiveName", "14.0Exp")]
    //public void TestToggleChartpoint()
    //{
    //  ICheckPoint chartPnt = null;
    //  header.SetPos(pos_f2_body);
    //  chartPnt = ChartPoints.Globals.processor.Check("test", header.ActivePoint);
    //  Assert.AreNotEqual(chartPnt, null);
    //  Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
    //  chartPnt.Toggle();
    //  Assert.AreEqual(chartPnt.status, ETargetPointStatus.SwitchedOn);
    //  chartPnt.Toggle();
    //  Assert.AreEqual(chartPnt.status, ETargetPointStatus.Available);
    //}

    //[TestMethod]
    //[HostType("VS IDE")]
    //[TestProperty("VsHiveName", "14.0Exp")]
    //public void TestExistingChartpoint()
    //{
    //  header.SetPos(pos_f2_body);
    //  ICheckPoint chartPnt1 = ChartPoints.Globals.processor.Check("test", header.ActivePoint);
    //  Assert.AreNotEqual(chartPnt1, null);
    //  chartPnt1.Toggle();
    //  header.SetPos(pos_f2_body + 1);
    //  ICheckPoint chartPnt2 = ChartPoints.Globals.processor.Check("test", header.ActivePoint);
    //  Assert.AreEqual(chartPnt1, chartPnt2);
    //}

    //[TestMethod]
    //[HostType("VS IDE")]
    //[TestProperty("VsHiveName", "14.0Exp")]
    //public void TestUnavailableCPOnToggledLine()
    //{
    //  header.SetPos(pos_f2_body + 2);
    //  ICheckPoint chartPnt = ChartPoints.Globals.processor.Check("test", header.ActivePoint);
    //  Assert.AreEqual(chartPnt, null);
    //}

  }
}

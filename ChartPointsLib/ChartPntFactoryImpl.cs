using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{

  /// <summary>
  /// Implementation of IChartPntFactory
  /// </summary>
  public class ChartPntFactoryImpl : ChartPntFactory
  {
    public ChartPntFactoryImpl()
    {
      // check and set singleton factory instance
      // helps to hide the injection of another factory object, eg. factory stub in tests
      if (ChartPntFactory.factory == null)
        ChartPntFactory.factory = this;
      Globals.cpTrackManager = new CPTrackManager();
    }

    public override IChartPointsProcessor CreateProcessor()
    {
      return new ChartPointsProcessor();
    }

    public override ICPOrchestrator CreateOrchestrator()
    {
      return new CPOrchestrator();
    }

    public override IProjectChartPoints CreateProjectChartPoint(string _projName)
    {
      return new ProjectChartPoints() { theData = new CPProjectData() { projName = _projName} };
    }

    public override IFileChartPoints CreateFileChartPoint(string _fileName, string _fileFullName, ICPProjectData _projData)
    {
      IFileChartPoints fcps = new FileChartPoints(_fileName, _fileFullName, _projData);
      Globals.cpTrackManager.Register(fcps);

      return fcps;
    }
    public override ILineChartPoints CreateLineChartPoint(int _lineNum, int _linePos, ICPFileData _fileData)
    {
      ILineChartPoints lcps = new LineChartPoints(_lineNum, _linePos, _fileData);
      //Globals.cpTrackManager.Register(lcps);

      return lcps;
    }

    public override IChartPoint CreateChartPoint(string varName, VCCodeClass ownerClass, ICPLineData _lineData)
    {
      IChartPoint cp = new ChartPoint(varName, ownerClass, _lineData);
      Globals.cpTrackManager.Register(cp);

      return cp;
    }
  }
}

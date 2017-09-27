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
      return new ProjectChartPoints(_projName);
    }

    public override IFileChartPoints CreateFileChartPoint(CP.Code.IFileElem _fileElem, ICPProjectData _projData)
    {
      IFileChartPoints fcps = new FileChartPoints(_fileElem, _projData);
      Globals.cpTrackManager.Register(fcps);

      return fcps;
    }
    public override ILineChartPoints CreateLineChartPoint(CP.Code.IClassElement _classElem, int _lineNum, int _linePos, ICPFileData _fileData)
    {
      ILineChartPoints lcps = new LineChartPoints(_classElem, _lineNum, _linePos, _fileData);
      //Globals.cpTrackManager.Register(lcps);

      return lcps;
    }

    public override IChartPoint CreateChartPoint(CP.Code.IClassVarElement codeElem, ICPLineData _lineData)
    {
      IChartPoint cp = new ChartPoint(codeElem, _lineData);
      Globals.cpTrackManager.Register(cp);

      return cp;
    }
  }
}

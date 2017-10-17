using System.Collections.Generic;
using CP.Code;

namespace ChartPoints
{
  /// <summary>
  /// Statuses showing the ability to toggle chartpoint
  /// </summary>
  public enum EChartPointStatus
  {
    Available       /// ready set chartpoint at specified position
    , NotAvailable = 4 /// can't set chartpoint at specified position
    , SwitchedOn   = 1  /// chartpoint already exists and is active
    , SwitchedOff  = 2  /// chartpoint already exists and is inactive
  }

  public interface IChartPointData
  {
    bool enabled { get; }
    string name { get; }
    string uniqueName { get; }
    string type { get; }
    EChartPointStatus status { get; }
    ICPLineData lineData { get; }
  }

  public interface IData<TData>
  {
    TData data { get; }
  }

  /// <summary>
  /// Interface for chartpoint object
  /// </summary>
  public interface IChartPoint : IData<IChartPointData>
  {
    ICPEvent<CPStatusEvArgs> cpStatusChangedEvent { get; set; }
    EChartPointStatus SetStatus(EChartPointStatus newStatus);
    CPTraceVar CalcInjectionPoints(CPClassLayout cpInjPoints, string className, out bool needDeclare);

    bool ValidatePosition(int lineNum, int linePos);
    void Invalidate();
    bool Validate();
  }

  public interface ITextPosition
  {
    int lineNum { get; }
    int linePos { get; }
    //void Move(IChartPoint cp, int _lineNum, int _linePos);
  }

  public enum ELineCPsStatus
  {
    NotAvailable = 0
    , Avail = EChartPointStatus.NotAvailable
    , AvailOn = EChartPointStatus.NotAvailable | EChartPointStatus.SwitchedOn
    , AvailOnOff = EChartPointStatus.NotAvailable | EChartPointStatus.SwitchedOn | EChartPointStatus.SwitchedOff
  }

  public interface ICPLineData
  {
    ITextPosition pos { get; }
    ICPFileData fileData { get; }
  }

  public interface ILineChartPoints : IData<ICPLineData>
  {
    ELineCPsStatus status { get; }
    ICPEvent<LineCPStatusEvArgs> lineCPStatusChangedEvent { get; set; }
    ICPEvent<CPLineEvArgs> addCPEvent { get; set; }
    ICPEvent<CPLineEvArgs> remCPEvent { get; set; }
    ISet<IChartPoint> chartPoints { get; }
    int Count { get; }
    IChartPoint GetChartPoint(string varName);
    bool AddChartPoint(string varName, out IChartPoint chartPnt);
    bool AddChartPoint(IChartPoint chartPnt);
    bool AddChartPoint(CP.Code.IClassVarElement codeElem, out IChartPoint chartPnt, bool checkExistance = true);
    bool RemoveChartPoint(IChartPoint chartPnt);
    bool SyncChartPoint(ICheckElem checkElem);//, IClassElement ownerClass);
    bool ValidatePosition(int linesAdd);
    void CalcInjectionPoints(CPClassLayout cpInjPoints, CP.Code.IModel model);
    void Invalidate();
    bool Validate();
  }

  public interface ICPFileData
  {
    string fileName { get; }
    string fileFullName { get; }
    ICPProjectData projData { get; }
    //void Move(ILineChartPoints lcps, IChartPoint cp, int _lineNum, int _linePos);
  }

  public interface IFileChartPoints : IData<ICPFileData>
  {
    ICPEvent<CPFileEvArgs> addCPLineEvent { get; set; }
    ICPEvent<CPFileEvArgs> remCPLineEvent { get; set; }
    ISet<ILineChartPoints> linePoints { get; }
    int Count { get; }
    ILineChartPoints GetLineChartPoints(int lineNum);
    ILineChartPoints AddLineChartPoints(int lineNum, int linePos);
    bool ValidatePosition(int lineNum, int linesAdd);
    void CalcInjectionPoints(CPClassLayout cpInjPoints, CP.Code.IModel model);
    void Invalidate();
    bool Validate();
  }

  public interface ICPProjectData
  {
    string projName { get; }
  }

  public interface IProjectChartPoints : IData<ICPProjectData>
  {
    ICPEvent<CPProjEvArgs> addCPFileEvent { get; set; }
    ICPEvent<CPProjEvArgs> remCPFileEvent { get; set; }
    ISet<IFileChartPoints> filePoints { get; }
    int Count { get; }
    IFileChartPoints GetFileChartPoints(string fname);
    ILineChartPoints GetFileLineChartPoints(string fname, int lineNum);
    IFileChartPoints AddFileChartPoints(string fileName);
    ICheckCPPoint CheckCursorPos();
    void CalcInjectionPoints(CPClassLayout cpInjPoints);
    void Invalidate();
    bool Validate();
  }

}

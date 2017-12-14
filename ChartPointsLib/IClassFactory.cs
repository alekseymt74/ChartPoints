using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;

namespace CP.Utils
{
  // class factory interface
  public abstract partial class IClassFactory
  {
    // set custom class factory instance for DI purposes
    public static void SetInstance(IClassFactory inst)
    {
      ClassFactory.SetInstanceImpl(inst);
    }
    // returns singleton instance
    public static IClassFactory GetInstance()
    {
      return ClassFactory.GetInstanceImpl();
    }

    public abstract IChartPointsProcessorData CreateCPProcData();
    public abstract IChartPointsProcessor CreateCPProc();
    public abstract ICPOrchestrator CreateCPOrchestrator();
    public abstract ICPProjectData CreateProjCPsData(string _projName);
    public abstract IProjectChartPoints CreateProjectCPs(string _projName);
    public abstract ICPFileData CreateFileCPsData(string _fileName, string _fileFullName, ICPProjectData _projData);
    public abstract IFileChartPoints CreateFileCPs(CP.Code.IFileElem _fileElem, ICPProjectData _projData);
    public abstract ICPLineData CreateLineCPsData(ITextPosition _pos, ICPFileData _fileData);
    public abstract ILineChartPoints CreateLineCPs(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData);
    public abstract IChartPointData CreateCPData(string _name, string _uniqueName, string _type, bool _enabled, EChartPointStatus _status, ICPLineData _lineData);
    public abstract IChartPoint CreateCP(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData);
  }

} // namespace CP.Utils


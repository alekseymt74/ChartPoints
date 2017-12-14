using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

namespace CP.Utils
{
  public abstract partial class IClassFactory
  {
    // implementation of ordinal class factory
    // is accessible only by IClassFactory and class factory implementations (for DI purposes)
    // private - only outer class IClassFactory has access to static method of ClassFactory
    private class ClassFactory : IClassFactory
    {
      IConstructEvents constrEvents;
      public ClassFactory()
      {
        var cpServProv = ICPServiceProvider.GetProvider();
        ICPEventService cpEvsService = new CPEventService();
        cpServProv.RegisterService<ICPEventService>(cpEvsService);
        constrEvents = cpEvsService.GetConstructEvents();
        cpServProv.RegisterService<ICPTrackService>(new CPTrackService());
      }
      private static IClassFactory Instance;
      public static void SetInstanceImpl(IClassFactory inst)
      {
        Instance = inst;
      }
      public static IClassFactory GetInstanceImpl()
      {
        if (Instance == null)
          Instance = new ClassFactory();
        return Instance;
      }

      // IChartPointsProcessorData factory
      private class ChartPointsProcDataImpl : ChartPointsProcessorData { }
      public override IChartPointsProcessorData CreateCPProcData()
      {
        return new ChartPointsProcDataImpl();
      }

      // IChartPointsProcessor factory
      // Opens the back-door to construct ChartPointsProcessor object
      // For access to non-default constructors appropriate delegating ones need to be added
      private class ChartPointsProcImpl : ChartPointsProcessor { }
      // IChartPointsProcessor factory method implementation
      public override IChartPointsProcessor CreateCPProc()
      {
        return new ChartPointsProcImpl();
      }

      // ICPOrchestrator factory
      private class CPOrchImpl : CPOrchestrator { }
      public override ICPOrchestrator CreateCPOrchestrator()
      {
        return new CPOrchImpl();
      }

      // CPProjectData factory
      private class CPProjDataImpl : CPProjectData
      {
        public CPProjDataImpl(string _projName) : base(_projName) { }
      }
      public override ICPProjectData CreateProjCPsData(string _projName)
      {
        return new CPProjDataImpl(_projName);
      }

      // ProjectChartPoints factory
      private class ProjCPsImpl : ProjectChartPoints
      {
        public ProjCPsImpl(string _projName) : base(_projName) { }
      }
      public override IProjectChartPoints CreateProjectCPs(string _projName)
      {
        IProjectChartPoints pcps = new ProjCPsImpl(_projName);
        constrEvents.createdProjCPsEvent.Fire(new ConstructEventArgs<IProjectChartPoints>(pcps));
        pcps.remCPFileEvent += OnDelFileCPs;

        return pcps;
      }
      protected void OnDelFileCPs(CPProjEvArgs args)
      {
        constrEvents.deletedFileCPsEvent.Fire(new ConstructEventArgs<IFileChartPoints>(args.fileCPs));
      }

      // CPFileData factory
      private class CPFileDataImpl : CPFileData
      {
        public CPFileDataImpl(string _fileName, string _fileFullName, ICPProjectData _projData) : base(_fileName, _fileFullName, _projData) { }
      }
      public override ICPFileData CreateFileCPsData(string _fileName, string _fileFullName, ICPProjectData _projData)
      {
        return new CPFileDataImpl(_fileName, _fileFullName, _projData);
      }
      // FileChartPoints factory
      private class FileCPsImpl : FileChartPoints
      {
        public FileCPsImpl(CP.Code.IFileElem _fileElem, ICPProjectData _projData) : base(_fileElem, _projData) { }
      }
      public override IFileChartPoints CreateFileCPs(CP.Code.IFileElem _fileElem, ICPProjectData _projData)
      {
        IFileChartPoints fcps = new FileCPsImpl(_fileElem, _projData);
        constrEvents.createdFileCPsEvent.Fire(new ConstructEventArgs<IFileChartPoints>(fcps));

        return fcps;
      }
      // CPFileData factory
      private class CPLineDataImpl : CPLineData
      {
        public CPLineDataImpl(ITextPosition _pos, ICPFileData _fileData) : base(_pos, _fileData) { }
      }
      public override ICPLineData CreateLineCPsData(ITextPosition _pos, ICPFileData _fileData)
      {
        return new CPLineDataImpl(_pos, _fileData);
      }
      // LineChartPoints factory
      private class LineCPsImpl : LineChartPoints
      {
        public LineCPsImpl(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData) : base(_classMethodElem, _lineNum, _linePos, _fileData) { }
      }
      public override ILineChartPoints CreateLineCPs(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData)
      {
        ILineChartPoints lcps = new LineCPsImpl(_classMethodElem, _lineNum, _linePos, _fileData);
        constrEvents.createdLineCPsEvent.Fire(new ConstructEventArgs<ILineChartPoints>(lcps));

        return lcps;
      }
      // CPFileData factory
      private class CPDataImpl : ChartPointData
      {
        public CPDataImpl(string _name, string _uniqueName, string _type, bool _enabled, EChartPointStatus _status, ICPLineData _lineData)
          : base(_name, _uniqueName, _type, _enabled, _status, _lineData) { }
      }
      public override IChartPointData CreateCPData(string _name, string _uniqueName, string _type, bool _enabled, EChartPointStatus _status, ICPLineData _lineData)
      {
        return new CPDataImpl(_name, _uniqueName, _type, _enabled, _status, _lineData);
      }
      // LineChartPoints factory
      private class CPImpl : ChartPoint
      {
        public CPImpl(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData) : base(_codeElem, _lineData) { }
      }
      public override IChartPoint CreateCP(CP.Code.IClassVarElement _codeElem, ICPLineData _lineData)
      {
        IChartPoint cp = new CPImpl(_codeElem, _lineData);
        constrEvents.createdCPEvent.Fire(new ConstructEventArgs<IChartPoint>(cp));

        return cp;
      }
    }
  }
} // namespace CP.Utils

﻿using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

namespace ChartPoints
{

  /// <summary>
  /// Implementation of IChartPntFactory
  /// </summary>
  //public class ChartPntFactoryImpl : ChartPntFactory
  //{
  //  IConstructEvents constrEvents;
  //  public ChartPntFactoryImpl()
  //  {
  //    // check and set singleton factory instance
  //    // helps to hide the injection of another factory object, eg. factory stub in tests
  //    if (ChartPntFactory.factory == null)
  //      ChartPntFactory.factory = this;
  //    var cpServProv = ICPServiceProvider.GetProvider();
  //    ICPEventService cpEvsService = new CPEventService();
  //    cpServProv.RegisterService<ICPEventService>(cpEvsService);
  //    constrEvents = cpEvsService.GetConstructEvents();
  //    cpServProv.RegisterService<ICPTrackService>(new CPTrackService());
  //  }

  //  //public override IChartPointsProcessor CreateProcessor()
  //  //{
  //  //  return new ChartPointsProcessor();
  //  //}

  //  //public override ICPOrchestrator CreateOrchestrator()
  //  //{
  //  //  return new CPOrchestrator();
  //  //}

  //  //public override IProjectChartPoints CreateProjectChartPoint(string _projName)
  //  //{
  //  //  IProjectChartPoints pcps = new ProjectChartPoints(_projName);
  //  //  constrEvents.createdProjCPsEvent.Fire(new ConstructEventArgs<IProjectChartPoints>(pcps));
  //  //  pcps.remCPFileEvent += OnDelFileCPs;

  //  //  return pcps;
  //  //}

  //  //protected void OnDelFileCPs(CPProjEvArgs args)
  //  //{
  //  //  constrEvents.deletedFileCPsEvent.Fire(new ConstructEventArgs<IFileChartPoints>(args.fileCPs));
  //  //}

  //  //public override IFileChartPoints CreateFileChartPoint(CP.Code.IFileElem _fileElem, ICPProjectData _projData)
  //  //{
  //  //  IFileChartPoints fcps = new FileChartPoints(_fileElem, _projData);
  //  //  constrEvents.createdFileCPsEvent.Fire(new ConstructEventArgs<IFileChartPoints>(fcps));

  //  //  return fcps;
  //  //}
  //  //public override ILineChartPoints CreateLineChartPoint(CP.Code.IClassMethodElement _classMethodElem, int _lineNum, int _linePos, ICPFileData _fileData)
  //  //{
  //  //  ILineChartPoints lcps = new LineChartPoints(_classMethodElem, _lineNum, _linePos, _fileData);
  //  //  constrEvents.createdLineCPsEvent.Fire( new ConstructEventArgs<ILineChartPoints>( lcps ) );

  //  //  return lcps;
  //  //}

  //  //public override IChartPoint CreateChartPoint(CP.Code.IClassVarElement codeElem, ICPLineData _lineData)
  //  //{
  //  //  IChartPoint cp = new ChartPoint(codeElem, _lineData);
  //  //  constrEvents.createdCPEvent.Fire(new ConstructEventArgs<IChartPoint>(cp));

  //  //  return cp;
  //  //}
  //}
}

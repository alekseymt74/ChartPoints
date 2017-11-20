using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using EnvDTE80;
using System.Diagnostics;
using ChartPoints.CPServices.decl;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using System.Windows.Shell;
using System.Reflection;

namespace ChartPoints
{
  public class TestLogger : Microsoft.Build.Utilities.Logger
  {
    public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
    {
      //Register for the ProjectStarted, TargetStarted, and ProjectFinished events
      eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
      eventSource.TargetStarted += new TargetStartedEventHandler(eventSource_TargetStarted);
      eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
      eventSource.MessageRaised += new BuildMessageEventHandler(eventSource_MessageRaised);
    }
    void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
    {
      Console.WriteLine("Project Started: " + e.ProjectFile);
    }

    void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
    {
      Console.WriteLine("Project Started: " + e.ProjectFile);
    }

    void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
    {
      Console.WriteLine("Project Finished: " + e.ProjectFile);
    }
    void eventSource_TargetStarted(object sender, TargetStartedEventArgs e)
    {
      if (Verbosity == LoggerVerbosity.Detailed)
      {
        Console.WriteLine("Target Started: " + e.TargetName);
      }
    }
  }

  public class CPOrchestrator : ICPOrchestrator
  {
    private ServiceHost serviceHost;
    private EnvDTE.DebuggerEvents debugEvents;
    private ISet<CPTraceHandler> traceHandlers = new SortedSet<CPTraceHandler>(Comparer<CPTraceHandler>.Create((lh, rh) => (lh.id.CompareTo(rh.id))));
    ICPServiceProvider cpServProv;
    bool cpTraceFlag = false;
    private ICPDebugService debugServ;


    public CPOrchestrator()
    {
      cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.GetService<ICPDebugService>(out debugServ);
      if (debugServ != null)
      {
        debugServ.debugProcCreateCPEvent += OnProcDebugCreate;
        debugServ.debugProcDestroyCPEvent += OnProcDebugDestroy;
      }
    }

    private void OnProcDebugCreate(CPProcEvArgs args)
    {
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(Path.GetFileNameWithoutExtension(args.Name));
      if (pPnts != null && pPnts.Count > 0)
      {
        TraceTransport.Open();
        traceHandlers.Add(new CPTraceHandler(args.procId, args.Name));
        cpTraceFlag = true;
      }
    }

    private void OnProcDebugDestroy(CPProcEvArgs args)
    {
      CPTraceHandler tracer = traceHandlers.FirstOrDefault((h) => (h.id == args.procId));
      if(tracer != null)
      {
        tracer.Dispose();
        traceHandlers.Remove(tracer);
        tracer = null;
        if (traceHandlers.Count == 0)
        {
          TraceTransport.Close();
          ICPTracerService traceServ;
          cpServProv.GetService<ICPTracerService>(out traceServ);
          traceServ.Show();
        }
      }
    }

    //private TestLogger cpBuildLogger;

    public void OnBuildProjConfigBegin(string projName, string projConfig
      , string platform, string solConfig)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        try
        {
          //cpBuildLogger = new TestLogger();
          ////BuildManager bm = BuildManager.DefaultBuildManager;
          ////Globals.bmAccessor.RegisterLogger(1, cpBuildLogger);
          //ProjectCollection.GlobalProjectCollection.UnregisterAllLoggers();
          //ProjectCollection.GlobalProjectCollection.RegisterLogger(cpBuildLogger);
          EnvDTE.Project proj = Globals.dte.Solution.Projects.Item(projName);
          //!!! Needed for newly created project to update vcxproj file !!!
          //Orchestrate(proj.FullName);
          IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(proj.Name);
          if (pPnts != null)
          {
            pPnts.Validate(); }
            if (serviceHost == null)
            {
              serviceHost = new ServiceHost(typeof(IPCChartPoint));
              //if (serviceHost.State != CommunicationState.Opening && serviceHost.State != CommunicationState.Opened)
              //{
              NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
              string address = "net.pipe://localhost/IPCChartPoint/" + System.IO.Path.GetFullPath(proj.FullName).ToLower();
              serviceHost.AddServiceEndpoint(typeof(IIPCChartPoint), binding, address);
              serviceHost.Open();
              //}
            }
          //}
        }
        catch(Exception ex)
        {
          serviceHost = null;
        }
      }
    }

    public void OnBuildProjConfigDone(string projName, string projConfig
      , string platform, string solConfig, bool success)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        if (serviceHost != null)
        {
          if(serviceHost.State == CommunicationState.Opened)
            serviceHost.Close();
          serviceHost = null;
        }
      }
    }

    private void CheckAndAddSolConf(ref SolutionBuild2 solBuild, string confType)
    {
      bool needAdd = true;
      foreach (SolutionConfiguration solConf in solBuild.SolutionConfigurations)
      {
        if (solConf.Name == confType + " [ChartPoints]")
          needAdd = false;
      }
      if (needAdd)
      {
        SolutionConfiguration cpConf = solBuild.SolutionConfigurations.Add(confType + " [ChartPoints]", confType, false/*true*/);
      }
      //foreach(EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //{
      //  needAdd = true;
      //  VCProject vcProj = proj.Object as VCProject;
      //  Configurations projConfManager = vcProj.Configurations;
      //  if (projConfManager != null)
      //  {
      //    foreach (Configuration conf in projConfManager)
      //    {
      //      if (conf.ConfigurationName == confType + " [ChartPoints]")
      //        needAdd = false;
      //    }
      //    if (needAdd)
      //      vcProj.AddConfiguration(confType + " [ChartPoints]");//, confType, true);
      //  }
      //}
    }

    public bool InitSolutionConfigurations()
    {
      SolutionBuild2 solBuild = (SolutionBuild2)Globals.dte.Solution.SolutionBuild;
      CheckAndAddSolConf(ref solBuild, "Debug");
      CheckAndAddSolConf(ref solBuild, "Release");
      //IEnumerable<SolutionConfiguration> allSolConfs = ToIEnumerable<SolutionConfiguration>(solBuild.SolutionConfigurations.GetEnumerator());
      //IEnumerable<SolutionConfiguration> solConfs = allSolConfs.Where(sc => (sc.Name.Contains("|ChartPoints")));
      //IDictionary<string, SolutionConfiguration> allSolConfs = new Dictionary<string, SolutionConfiguration>();
      //foreach (SolutionConfiguration solConf in solBuild.SolutionConfigurations)
      //{
      //  if (solConf.Name == "Debug" || solConf.Name == "Release")
      //  {
      //    SolutionConfiguration cpConf
      //      = solBuild.SolutionConfigurations.Add(solConf.Name + " [ChartPoints]", solConf.Name, true);
      //  }
      //}
      //allSolConfs.Add(solConf.Name, solConf);
      //CheckAndAddSolConf(ref solBuild, ref allSolConfs, "Debug");
      //CheckAndAddSolConf(ref solBuild, ref allSolConfs, "Release");
      //foreach (SolutionConfiguration solConf in solBuild.SolutionConfigurations)
      //{
      //  if (solConf.Name.Contains("Debug") || solConf.Name.Contains("Release"))
      //  {
      //    SolutionConfiguration cpConf
      //      = solBuild.SolutionConfigurations.Add(solConf.Name + "|ChartPoints", solConf.Name, true);
      //  }
      //}
      Globals.dte.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildProjConfigBegin;
      Globals.dte.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
      debugEvents = Globals.dte.Events.DebuggerEvents;
      debugEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(DebuggerEventsOnOnEnterRunMode);//DebuggerEventsOnOnEnterRunMode;
      debugEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(DebugEventsOnOnEnterDesignMode);//DebugEventsOnOnEnterDesignMode
      //debugEvents.OnContextChanged += new _dispDebuggerEvents_OnContextChangedEventHandler(DebuggerEventsOnOnContextChanged);//DebuggerEventsOnOnContextChanged;
      //LoadChartPoints();

      return true;
    }

    private void CheckAndAddProjConf(EnvDTE.Project proj, string confType)
    {
      bool needAdd = true;
      //VCProject vcProj = proj.Object as VCProject;
      //Configurations projConfManager = vcProj.Configurations;
      //if (projConfManager != null)
      //{
      //  foreach (Configuration conf in projConfManager)
      //  {
      //    if (conf.ConfigurationName == confType + " [ChartPoints]")
      //      needAdd = false;
      //  }
      //  if (needAdd)
      //    vcProj.AddConfiguration(confType + " [ChartPoints]");//, confType, true);
      //}
      ConfigurationManager projConfManager = proj.ConfigurationManager;
      if (projConfManager != null)
      {
        foreach (Configuration conf in projConfManager)
        {
          if (conf.ConfigurationName == confType + " [ChartPoints]")
            needAdd = false;
        }
        if (needAdd)
        {
          //Configuration activeConf = projConfManager.ActiveConfiguration;
          Configurations cpConfs = projConfManager.AddConfigurationRow(confType + " [ChartPoints]", confType, false);// true);
          //Configuration srcConf = projConfManager.Item(confType);
          //foreach(Configuration cpConf in cpConfs)
          //{
          //  //Property platformProp = cpConf.Properties.Item("Platform");
          //  //platformProp.let_Value("x86");
          //}
        }
      }
    }

    public bool InitProjConfigurations(EnvDTE.Project proj)
    {
      //Orchestrate(proj.FullName);
      CheckAndAddProjConf(proj, "Debug");
      CheckAndAddProjConf(proj, "Release");

      return true;
    }

    private void CheckShowCPTW()
    {
      if (debugServ.IsChartPointsMode() && cpTraceFlag)
      {
        ICPTracerService traceServ;
        cpServProv.GetService<ICPTracerService>(out traceServ);
        traceServ.Show();
        cpTraceFlag = false;
      }
    }

    private void DebugEventsOnOnEnterDesignMode(dbgEventReason reason)
    {
      CheckShowCPTW();
      //if (IsChartPointsMode() && traceHandler != null)
      //{
      //  traceHandler.Dispose();
      //  //int gen = GC.GetGeneration(traceHandler);
      //  traceHandler = null;
      //  ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      //  ICPTracerService traceServ;
      //  cpServProv.GetService<ICPTracerService>(out traceServ);
      //  traceServ.Show();
      //  //        Globals.cpTracer.Show();
      //  //GC.Collect(gen, GCCollectionMode.Forced);
      //}
    }

    //private void DebuggerEventsOnOnContextChanged(EnvDTE.Process newProcess, Program newProgram, Thread newThread, EnvDTE.StackFrame newStackFrame)
    //{
    //  //System.Windows.Forms.MessageBox.Show("Debugger context changed.");
    //  //throw new NotImplementedException();
    //}

    private void DebuggerEventsOnOnEnterRunMode(dbgEventReason reason)
    {
      CheckShowCPTW();
      //LoadChartPoints();
      //if (IsChartPointsMode() && traceHandler == null)
      //  traceHandler = new CPTraceHandler(1);
    }

    public bool RemoveSolutionConfigurations()
    {
      SolutionBuild2 solBuild = (SolutionBuild2)Globals.dte.Solution.SolutionBuild;
      IEnumerable<SolutionConfiguration> solConfs
        = ((IEnumerable<SolutionConfiguration>)solBuild.SolutionConfigurations.GetEnumerator()).Where(
          sc => (sc.Name.Contains("|ChartPoints")));
      foreach (var sc in solConfs)
        sc.Delete();

      return true;
    }

    public bool Orchestrate(EnvDTE.Project proj)
    {
      Microsoft.Build.Evaluation.Project msBuildProject = Orchestrate(proj.FullName);
      if (msBuildProject != null)
        return true;

      return false;
    }

    private bool CreateFileFromResource(string resName, string fileName)
    {
      try
      {
        var assembly = Assembly.GetExecutingAssembly();
        string[] ress = assembly.GetManifestResourceNames();
        using (Stream stream = assembly.GetManifestResourceStream(resName))
        {
          if (stream == null)
            return false;
          using (StreamReader reader = new StreamReader(stream))
          {
            if (reader == null)
              return false;
            string fileContent = reader.ReadToEnd();
            File.WriteAllText(fileName, fileContent);
          }
        }
      }
      catch (Exception /*ex*/)
      {
        return false;
      }

      return true;
    }

    public Microsoft.Build.Evaluation.Project Orchestrate(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
      {
        msbuildProj = new Microsoft.Build.Evaluation.Project(projConfFile);
        if (msbuildProj == null)
          return null;
      }
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return null;
      ICPExtension extensionServ;
      cpServProv.GetService<ICPExtension>(out extensionServ);
      string instPath = extensionServ.GetVSIXInstallPath();
      bool need_save = false;
      string cpTargetsFullPath = Path.GetDirectoryName(projConfFile) + "\\ChartPoints.targets";
      if(!File.Exists(cpTargetsFullPath))
      {
        bool res = CreateFileFromResource("ChartPointsLib.Resources.ChartPoints.targets", cpTargetsFullPath);
      }
      IEnumerable<ProjectImportElement> importElems = projRoot.Imports.Where(ig => (ig.Project == "ChartPoints.targets"));
      if(!importElems.Any())
      {
        projRoot.AddImport("ChartPoints.targets");
        need_save = true;
      }
      IEnumerable<ProjectPropertyGroupElement> cpPropsGroups = projRoot.PropertyGroups.Where(ig => (ig.Label == "CPTargetsVariables"));
      if(!cpPropsGroups.Any())
      {
        ProjectPropertyGroupElement cpPropsGroup = projRoot.AddPropertyGroup();
        cpPropsGroup.Label = "CPTargetsVariables";
        cpPropsGroup.AddProperty("TargetFileFullPath", instPath + "\\CPInstrBuildTask.dll");
        cpPropsGroup.AddProperty("ThisProjectFullName", "$(MSBuildThisFileFullPath)");
      }
      //////IEnumerable<ProjectTargetElement> cleanTargetElems
      //////  = projRoot.Targets.Where(ig => (ig.Name == "CleanCPDeps") && ig.Tasks.Count() == 1 && ig.Tasks.ElementAt(0).Name == "CleanProjCPDeps");
      //////if (!cleanTargetElems.Any())
      //////{
      //////  ProjectTargetElement cleanTargetElem = projRoot.AddTarget("CleanCPDeps");
      //////  cleanTargetElem.BeforeTargets = "ClCompile";
      //////  ProjectPropertyGroupElement targetPropsGroup = cleanTargetElem.AddPropertyGroup();
      //////  targetPropsGroup.AddProperty("TargetFileFullPath", instPath + "\\CPInstrBuildTask.dll");
      //////  ProjectTaskElement task = cleanTargetElem.AddTask("CleanProjCPDeps");
      //////  task.Condition = "!exists('$(TargetFileFullPath)')";
      //////  task.SetParameter("ProjFullName", "$(MSBuildProjectFullPath)");
      //////  need_save = true;
      //////}
      if (need_save)
      {
        //msbuildProj.ReevaluateIfNecessary();
        msbuildProj.Save();
      }
      return msbuildProj;
    }

    public bool Build()
    {
      return false;
    }

    public bool Run()
    {
      return false;
    }

    public bool UnloadProject(EnvDTE.Project proj)
    {
      return Globals.processor.RemoveChartPoints(proj.Name);
    }
  }
}

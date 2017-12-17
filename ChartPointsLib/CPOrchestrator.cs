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
  public abstract class CPOrchestrator : ICPOrchestrator
  {
    private IDictionary<string, ServiceHost> serviceHostsCont = new SortedDictionary<string, ServiceHost>();
    private EnvDTE.DebuggerEvents debugEvents;
    private ISet<CPTraceHandler> traceHandlers = new SortedSet<CPTraceHandler>(Comparer<CPTraceHandler>.Create((lh, rh) => (lh.id.CompareTo(rh.id))));
    ICPServiceProvider cpServProv;
    bool cpTraceFlag = false;
    private ICPDebugService debugServ;
    ICPExtension extensionServ;

    public CPOrchestrator()
    {
      cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.GetService<ICPDebugService>(out debugServ);
      if (debugServ != null)
      {
        debugServ.debugProcCreateCPEvent += OnProcDebugCreate;
        debugServ.debugProcDestroyCPEvent += OnProcDebugDestroy;
      }
      cpServProv.GetService<ICPExtension>(out extensionServ);
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

    public void OnBuildProjConfigBegin(string projName, string projConfig
      , string platform, string solConfig)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        extensionServ.SetMode(EMode.Build);
        ServiceHost serviceHost = null;
        try
        {
          EnvDTE.Project proj = Globals.dte.Solution.Projects.Item(projName);
          //!!! Needed for newly created project to update vcxproj file !!!
          //Orchestrate(proj.FullName);
          IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(proj.Name);
          if (pPnts != null)
          {
            VCProject vcProj = (VCProject)proj.Object;
            VCConfiguration vcConfig = vcProj.Configurations.Item(projConfig);
            IVCCollection tools = vcConfig.Tools as IVCCollection;
            VCLinkerTool tool = tools.Item("VCLinkerTool") as VCLinkerTool;
            tool.GenerateDebugInformation = false;
            pPnts.Validate();
            if (!serviceHostsCont.TryGetValue(proj.FullName, out serviceHost))
              serviceHostsCont.Add(proj.FullName, serviceHost);
            if (serviceHost == null)
            {
              serviceHost = new ServiceHost(typeof(IPCChartPoint));
              serviceHostsCont[proj.FullName] = serviceHost;
              //if (serviceHost.State != CommunicationState.Opening && serviceHost.State != CommunicationState.Opened)
              //{
              NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
              string address = "net.pipe://localhost/IPCChartPoint/" + System.IO.Path.GetFullPath(proj.FullName).ToLower();
              serviceHost.AddServiceEndpoint(typeof(IIPCChartPoint), binding, address);
              serviceHost.Open();
              //}
            }
          }
        }
        catch(Exception /*ex*/)
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
        EnvDTE.Project proj = Globals.dte.Solution.Projects.Item(projName);
        ServiceHost serviceHost = null;
        serviceHostsCont.TryGetValue(proj.FullName, out serviceHost);
        if (serviceHost != null)
        {
          if(serviceHost.State == CommunicationState.Opened)
            serviceHost.Close();
          serviceHostsCont[proj.FullName] = null;
        }
        extensionServ.SetMode(EMode.Design);
      }
    }

    private void AdjustSolConfMatrixRow(SolutionBuild2 solBuild, string confName)
    {
      foreach (SolutionConfiguration solConf in solBuild.SolutionConfigurations)
      {
        if (solConf.Name.Contains(confName))
        {
          foreach (SolutionContext context in solConf.SolutionContexts)
            context.ConfigurationName = confName;
        }
      }
    }

    private void AdjustSolConfMatrix()
    {
      SolutionBuild2 solBuild = (SolutionBuild2)Globals.dte.Solution.SolutionBuild;
      AdjustSolConfMatrixRow(solBuild, "Debug [ChartPoints]");
      AdjustSolConfMatrixRow(solBuild, "Release [ChartPoints]");
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
    }

    public bool InitSolutionConfigurations()
    {
      SolutionBuild2 solBuild = (SolutionBuild2)Globals.dte.Solution.SolutionBuild;
      CheckAndAddSolConf(ref solBuild, "Debug");
      CheckAndAddSolConf(ref solBuild, "Release");
      AdjustSolConfMatrix();
      Globals.dte.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildProjConfigBegin;
      Globals.dte.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
      debugEvents = Globals.dte.Events.DebuggerEvents;
      debugEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(DebuggerEventsOnOnEnterRunMode);
      debugEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(DebugEventsOnOnEnterDesignMode);

      return true;
    }

    private void CheckAndAddProjConf(EnvDTE.Project proj, string confType)
    {
      bool needAdd = true;
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
          Configurations cpConfs = projConfManager.AddConfigurationRow(confType + " [ChartPoints]", confType, false/*true*/);
        }
      }
    }

    public bool InitProjConfigurations(EnvDTE.Project proj)
    {
      //Orchestrate(proj.FullName);
      CheckAndAddProjConf(proj, "Debug");
      CheckAndAddProjConf(proj, "Release");
      AdjustSolConfMatrix();

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
      extensionServ.SetMode(EMode.Design);
      CheckShowCPTW();
    }

    private void DebuggerEventsOnOnEnterRunMode(dbgEventReason reason)
    {
      extensionServ.SetMode(EMode.Run);
      CheckShowCPTW();
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
      if (need_save)
        msbuildProj.Save();

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

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

namespace ChartPoints
{
  public class CPOrchestrator : ICPOrchestrator
  {
    private ServiceHost serviceHost;
    private EnvDTE.DebuggerEvents debugEvents;
    private CPTraceHandler traceHandler;

    private bool IsChartPointsMode()
    {
      string activeConfig = (string) Globals.dte.Solution.Properties.Item("ActiveConfig").Value;
      return activeConfig.Contains(" [ChartPoints]");
    }

    public void OnBuildProjConfigBegin(string projName, string projConfig
      , string platform, string solConfig)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        EnvDTE.Project proj = Globals.dte.Solution.Projects.Item(projName);
        IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(proj.Name);
        if (pPnts != null)
          pPnts.Validate();
        //string address = "net.pipe://localhost/ChartPoints/IPCChartPoint";
        string address = "net.pipe://localhost/IPCChartPoint/" + System.IO.Path.GetFullPath(proj.FullName).ToLower();
        serviceHost = new ServiceHost(typeof(IPCChartPoint));
        NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        serviceHost.AddServiceEndpoint(typeof(IIPCChartPoint), binding, address);
        serviceHost.Open();
      }
    }

    public void OnBuildProjConfigDone(string projName, string projConfig
      , string platform, string solConfig, bool success)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        if (serviceHost != null)
        {
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
        SolutionConfiguration cpConf = solBuild.SolutionConfigurations.Add(confType + " [ChartPoints]", confType, true);
      }
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
      debugEvents.OnContextChanged += new _dispDebuggerEvents_OnContextChangedEventHandler(DebuggerEventsOnOnContextChanged);//DebuggerEventsOnOnContextChanged;
      //LoadChartPoints();

      return true;
    }

    private void DebugEventsOnOnEnterDesignMode(dbgEventReason reason)
    {
      if (IsChartPointsMode() && traceHandler != null)
      {
        traceHandler.Dispose();
        //int gen = GC.GetGeneration(traceHandler);
        traceHandler = null;
        ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
        ICPTracerService traceServ;
        cpServProv.GetService<ICPTracerService>(out traceServ);
        traceServ.Show();
        //        Globals.cpTracer.Show();
        //GC.Collect(gen, GCCollectionMode.Forced);
      }
    }

    private void DebuggerEventsOnOnContextChanged(Process newProcess, Program newProgram, Thread newThread, StackFrame newStackFrame)
    {
      //System.Windows.Forms.MessageBox.Show("Debugger context changed.");
      //throw new NotImplementedException();
    }

    private void DebuggerEventsOnOnEnterRunMode(dbgEventReason reason)
    {
      //LoadChartPoints();
      if (IsChartPointsMode() && traceHandler == null)
        traceHandler = new CPTraceHandler();
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

    public Microsoft.Build.Evaluation.Project Orchestrate(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      //Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(pr => pr.FullPath == projConfFile);
      //if (msbuildProj != null)
      //  ProjectCollection.GlobalProjectCollection.UnloadProject(msbuildProj);
      //msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      //bool remRet = ProjectCollection.GlobalProjectCollection.LoadedProjects.Remove(msbuildProj);
      //msbuildProj = null;
      if (msbuildProj == null)
      {
        msbuildProj = new Microsoft.Build.Evaluation.Project(projConfFile);
        if (msbuildProj == null)
          return null;
      }
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return null;
      //#####################################
      //IEnumerable<ProjectItemDefinitionGroupElement> defGroup = projRoot.ItemDefinitionGroups.Where(gr => (gr.Condition.Contains("[ChartPoints]") && gr.Condition.Contains("|$(Platform)")));
      //#####################################
      //Condition="'$(CONFIG)'=='DEBUG'"
      ProjectUsingTaskElement usingTaskElem;
      IEnumerable<ProjectUsingTaskElement> usingTaskElemCont
        = projRoot.UsingTasks.Where(t => (t.TaskName == "ChartPointsBuilder.CPInstrBuildTask"
        && t.AssemblyFile == @"e:\projects\tests\MSVS.ext\ChartPoints\CPInstrBuildTask\bin\Debug\CPInstrBuildTask.dll"));
      if (usingTaskElemCont.Any())
        usingTaskElem = usingTaskElemCont.ElementAt(0);
      else
      {
        usingTaskElem = projRoot.AddUsingTask("ChartPointsBuilder.CPInstrBuildTask",
          @"e:\projects\tests\MSVS.ext\ChartPoints\CPInstrBuildTask\bin\Debug\CPInstrBuildTask.dll", null);
        usingTaskElem.Condition = "$(Configuration.Contains('[ChartPoints]'))";
        ProjectTargetElement target = projRoot.AddTarget("GenerateToolOutput");
        target.BeforeTargets = "ClCompile";
        target.Condition = "$(Configuration.Contains('[ChartPoints]'))";
        ProjectTaskElement task = target.AddTask("CPInstrBuildTask");
        task.SetParameter("InputSrcFiles", "@(ClCompile)");
        task.SetParameter("InputHeaderFiles", "@(ClInclude)");
        task.SetParameter("InputChartPoints", "@(ChartPointFile)");
        task.SetParameter("ProjectName", "$(MSBuildProjectName)");
        task.SetParameter("ProjectFullName", "@(MSBuildThisFileFullPath)");
        task.AddOutputProperty("OutputSrcFiles", "OutputSrcFiles");
        task.AddOutputProperty("OutputHeaderFiles", "OutputHeaderFiles");
        task.AddOutputProperty("SrcFilesChanged", "SrcFilesChanged");
        task.AddOutputProperty("HeaderFilesChanged", "HeaderFilesChanged");

        ProjectItemGroupElement srcItemGroup = target.AddItemGroup();
        srcItemGroup.Condition = "$(SrcFilesChanged) == True";
        //ProjectItemElement addInclPath = srcItemGroup.AddItem("AdditionalIncludeDirectories", "$(TEMP)");
        //addInclPath.Include = "";
        //addInclPath.Remove = "";
        ProjectItemElement srcRemoveItem = srcItemGroup.AddItem("ClCompile", "Fake");
        srcRemoveItem.Include = "";
        srcRemoveItem.Remove = "@(ClCompile)";
        ProjectItemElement srcIncludeItem = srcItemGroup.AddItem("ClCompile", "$(OutputSrcFiles)");
        srcIncludeItem.AddMetadata("AdditionalIncludeDirectories", "$(MSBuildProjectDirectory);%(AdditionalIncludeDirectories);");

        ProjectItemGroupElement headerItemGroup = target.AddItemGroup();
        headerItemGroup.Condition = "$(HeaderFilesChanged) == True";
        ProjectItemElement headerRemoveItem = headerItemGroup.AddItem("ClInclude", "Fake");
        headerRemoveItem.Include = "";
        headerRemoveItem.Remove = "@(ClInclude)";
        ProjectItemElement headerIncludeItem = headerItemGroup.AddItem("ClInclude", "$(OutputHeaderFiles)");

        msbuildProj.ReevaluateIfNecessary();
        msbuildProj.Save();
      }
      //proj.Save();
      //msbuildProj.Save();
      //SaveProject(proj, msbuildProj);

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

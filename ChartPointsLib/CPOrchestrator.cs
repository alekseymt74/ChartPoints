﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;

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
        SaveProjChartPoints(proj);//.FullName);
        Orchestrate(proj);//.FullName);
        string address = "net.pipe://localhost/ChartPoints/IPCChartPoint";
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
        serviceHost.Close();
        serviceHost = null;
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
        Globals.cpTracer.Show();
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

    public bool SaveProjChartPoints(EnvDTE.Project proj)
    {
      //!!!!!!!!! CHECK NEED SAVE !!!!!!!!!
      Microsoft.Build.Evaluation.Project msBuildProject = SaveProjChartPoints(proj.FullName);
      if (msBuildProject != null)
      {
        SaveProject(proj, msBuildProject);
        return true;
      }

      return false;
    }

    public Microsoft.Build.Evaluation.Project SaveProjChartPoints(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return null;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return null;
      // Remove all <ItemGroup><ChartPointFile>...</ChartPointFile></ItemGroup> if any
      IEnumerable<ProjectItemGroupElement> cpGroups
        = projRoot.ItemGroups.Where(ig => (ig.Items.Where(i => (i.ItemType == "ChartPointFile"))).Any());
      if (cpGroups.Any())
      {
        foreach (var cpg in cpGroups)
          projRoot.RemoveChild(cpg);
      }
      ProjectItemGroupElement cpItemGroup = projRoot.AddItemGroup();
      IEnumerable<ProjectProperty> projNameProp = msbuildProj.AllEvaluatedProperties.Where((b => (b.Name.Equals("MSBuildProjectName"))));
      string projName = projNameProp.ElementAt(0).EvaluatedValue;

      ////msbuildProj.GlobalProperties
      ////+		[8]	"MSBuildProjectName"="test.instrTest" ["test.instrTest"]	Microsoft.Build.Evaluation.ProjectProperty {Microsoft.Build.Evaluation.ProjectProperty.ProjectPropertyNotXmlBacked}

      //IEnumerable<ProjectPropertyGroupElement> globalsGroups = projRoot.PropertyGroups.Where(ig => (ig.Label == "Globals"));
      //ProjectElement rootNS = globalsGroups.ElementAt(0).Children.Where(i => (i.Label == "RootNamespace")).FirstOrDefault();
      //string projName = rootNS.Label;
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(projName);//rootNS.Metadata.First().Value);
      if (pPnts != null)
      //if (Globals.processor.data.chartPoints.Any())
      {
        //foreach (var v in Globals.processor.data.chartPoints)
        foreach (var fPnts in pPnts.filePoints)
        {
          ProjectItemElement cpsFileItem = cpItemGroup.AddItem("ChartPointFile", fPnts.data.fileName/*v.Key*/);
          StringWriter strWriter = new StringWriter();
          XmlTextWriter xmlTxtWriter = new XmlTextWriter(strWriter);
          //xmlTxtWriter.WriteStartElement("ChartPoints");
          //foreach (var v1 in v.Value)
          foreach (var lPnts in fPnts.linePoints)
          {
            foreach (var chartPnt in lPnts.chartPoints)
            {
              //IChartPoint chartPnt = v1.Value;
              xmlTxtWriter.WriteStartElement("ChartPoint");
              {
                xmlTxtWriter.WriteElementString("Variable", chartPnt.data.varName);
                xmlTxtWriter.WriteElementString("LineNum", Convert.ToString((Int32) lPnts.data.pos.lineNum));//chartPnt.data.lineNum));
                xmlTxtWriter.WriteElementString("LinePos", Convert.ToString((Int32) lPnts.data.pos.linePos));//chartPnt.data.linePos));
                xmlTxtWriter.WriteElementString("Enabled", chartPnt.data.enabled ? "true" : "false");
              }
            }
            xmlTxtWriter.WriteEndElement();
          }
          //xmlTxtWriter.WriteEndElement();
          cpsFileItem.AddMetadata("ChartPoints", strWriter.ToString());
        }
        msbuildProj.ReevaluateIfNecessary();
      }
      //proj.Save();
      //msbuildProj.Save();
      //SaveProject(proj, msbuildProj);

      return msbuildProj;
    }

    public bool SaveChartPonts()
    {
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
        SaveProjChartPoints(proj);//.FullName);
      return true;
    }

    public bool LoadChartPoints()
    {
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      {
        if(proj.Name  != "Miscellaneous Files")
          LoadProjChartPoints(proj); //.FullName);
      }
      return true;
    }

    public bool LoadProjChartPoints(EnvDTE.Project proj)
    {
      Microsoft.Build.Evaluation.Project msBuildProject = null;
      try
      {
        msBuildProject = LoadProjChartPoints(proj.FullName);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);

        return false;
      }
      return (msBuildProject != null);
    }

    private EnvDTE.ProjectItem GetProjItem(EnvDTE.ProjectItems projItems, string fileName)
    {
      if (projItems == null)
        return null;
      foreach (EnvDTE.ProjectItem projItem in projItems)
      {
        if (projItem.Name == fileName)
          return projItem;
        EnvDTE.ProjectItem theProjItem = GetProjItem(projItem.ProjectItems, fileName);
        if (theProjItem != null)
          return theProjItem;
      }

      return null;
    }

    private EnvDTE.ProjectItem GetProjItem(string projName, string fileName)
    {
      EnvDTE.Project theProj = null;
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      {
        if (proj.Name == projName)
        {
          theProj = proj;
          break;
        }
      }
      if (theProj != null)
      {
        EnvDTE.ProjectItem projItem = GetProjItem(theProj.ProjectItems, fileName);
        if (projItem != null)
          return projItem;
      }

      return null;
    }

    private VCCodeClass GetTraceVarClassElem(CodeElement codeElem, int lineNum, int linePos)
    {
      if (codeElem.Kind == vsCMElement.vsCMElementFunction)
      {
        //if (codeElem.StartPoint.Line <= lineNum
        //    && codeElem.EndPoint.Line >= lineNum
        //    && codeElem.StartPoint.LineCharOffset <= linePos
        //    && codeElem.EndPoint.LineCharOffset >= linePos)
        if ((lineNum > codeElem.StartPoint.Line && lineNum < codeElem.EndPoint.Line) ||
            (lineNum == codeElem.StartPoint.Line && linePos >= codeElem.StartPoint.LineCharOffset) ||
            (lineNum == codeElem.EndPoint.Line && linePos <= codeElem.EndPoint.LineCharOffset))
        {
          VCCodeFunction targetFunc = (VCCodeFunction)codeElem;
          if (targetFunc != null)
          {
            VCCodeElement targetClassElem = (VCCodeElement)targetFunc.Parent;
            if (targetClassElem != null && targetClassElem.Kind == vsCMElement.vsCMElementClass)
              return (VCCodeClass)targetClassElem;
          }
        }
      }
      else
      {
        foreach (CodeElement classCodeElem in codeElem.Children)
        {
          VCCodeClass theClass = GetTraceVarClassElem(classCodeElem, lineNum, linePos);
          if (theClass != null)
            return theClass;
        }
      }

      return null;
    }

    private VCCodeClass GetTraceVarClassElem(FileCodeModel fcm, int lineNum, int linePos)
    {
      foreach (CodeElement codeElem in fcm.CodeElements)
      {
        VCCodeClass theClass = GetTraceVarClassElem(codeElem, lineNum, linePos);
        if (theClass != null)
          return theClass;
      }

      return null;
    }

    public Microsoft.Build.Evaluation.Project LoadProjChartPoints(string projConfFile)
    {
      if (projConfFile == "")
        return null;
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return null;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return null;
      IEnumerable<ProjectItemGroupElement> cpGroups
        = projRoot.ItemGroups.Where(ig => (ig.Items.Where(i => (i.ItemType == "ChartPointFile"))).Any());
      IEnumerable<ProjectProperty> projNameProp = msbuildProj.AllEvaluatedProperties.Where((b => (b.Name.Equals("MSBuildProjectName"))));
      string projName = projNameProp.ElementAt(0).EvaluatedValue;
      Globals.processor.RemoveChartPoints(projName);
      //IEnumerable<ProjectItemGroupElement> globalsGroups = projRoot.ItemGroups.Where(ig => (ig.Label == "Globals"));
      //ProjectItemElement rootNS = globalsGroups.ElementAt(0).Items.Where(i => (i.ItemType == "RootNamespace")).First();
      //string projName = rootNS.Metadata.First().Value;
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(projName);
      if (pPnts == null)
        Globals.processor.AddProjectChartPoints(projName, out pPnts);
      if (cpGroups.Any())
      {
        ICPConfLoader confLoader = new CPConfLoader();
        foreach (ProjectItemGroupElement cpg in cpGroups)
        {
          foreach (ProjectItemElement cpFileElem in cpg.Items)
          {
            //ProjectItemElement cpFileElem = cpg.Items.ElementAt(0);
            if (cpFileElem.HasMetadata)
            {
              for (;;)
              {
                EnvDTE.ProjectItem projItem = GetProjItem(projName, cpFileElem.Include);
                if (projItem == null)
                  break;
                FileCodeModel fcm = projItem.FileCodeModel;
                if (fcm == null)
                  break;
                VCFileCodeModel vcFCM = (VCFileCodeModel) fcm;
                if (vcFCM == null)
                  break;
                vcFCM.Synchronize();
                foreach (ProjectMetadataElement me in cpFileElem.Metadata)
                {
                  Action<int, CPData> addCPDataAction
                    = (i, data) =>
                    {
                      data.fileName = cpFileElem.Include;
                      data.lineNum = i;
                      data.projName = projName;
                      VCCodeClass ownerClass = GetTraceVarClassElem(fcm, data.lineNum, data.linePos);
                      if (ownerClass != null)
                      {
                        string fileFullName = System.IO.Path.GetFullPath(projItem.FileNames[0]).ToLower();
                        IFileChartPoints fPnts = pPnts.AddFileChartPoints(cpFileElem.Include, fileFullName);
                        ILineChartPoints lPnts = fPnts.AddLineChartPoints(data.lineNum, data.linePos);
                        IChartPoint chartPnt = null;
                        lPnts.AddChartPoint(data.varName, ownerClass, out chartPnt);
                      }
                    };
                  confLoader.LoadChartPoint("<" + me.Name + ">" + me.Value + "</" + me.Name + ">", addCPDataAction);
                }
                break;
              }
            }
          }
        }
      }

      return msbuildProj;
    }

    public bool Orchestrate(EnvDTE.Project proj)
    {
      Microsoft.Build.Evaluation.Project msBuildProject = Orchestrate(proj.FullName);
      if (msBuildProject != null)
      {
        SaveProject(proj, msBuildProject);
        return true;
      }

      return false;
    }

    public Microsoft.Build.Evaluation.Project Orchestrate(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return null;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return null;
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
        task.AddOutputProperty("OutputSrcFiles", "OutputSrcFiles");
        task.AddOutputProperty("OutputHeaderFiles", "OutputHeaderFiles");
        task.AddOutputProperty("SrcFilesChanged", "SrcFilesChanged");
        task.AddOutputProperty("HeaderFilesChanged", "HeaderFilesChanged");

        ProjectItemGroupElement srcItemGroup = target.AddItemGroup();
        srcItemGroup.Condition = "$(SrcFilesChanged) == True";
        ProjectItemElement srcRemoveItem = srcItemGroup.AddItem("ClCompile", "Fake");
        srcRemoveItem.Include = "";
        srcRemoveItem.Remove = "@(ClCompile)";
        ProjectItemElement srcIncludeItem = srcItemGroup.AddItem("ClCompile", "$(OutputSrcFiles)");

        ProjectItemGroupElement headerItemGroup = target.AddItemGroup();
        headerItemGroup.Condition = "$(HeaderFilesChanged) == True";
        ProjectItemElement headerRemoveItem = headerItemGroup.AddItem("ClInclude", "Fake");
        headerRemoveItem.Include = "";
        headerRemoveItem.Remove = "@(ClInclude)";
        ProjectItemElement headerIncludeItem = headerItemGroup.AddItem("ClInclude", "$(OutputHeaderFiles)");

        msbuildProj.ReevaluateIfNecessary();
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

    public bool SaveProject(EnvDTE.Project proj, Microsoft.Build.Evaluation.Project msbuildProj)
    {
      proj.Save();//proj.FullName);
      msbuildProj.Save();

      return true;
    }

    public bool UnloadProject(EnvDTE.Project proj)
    {
      return Globals.processor.RemoveChartPoints(proj.Name);
    }
  }
}

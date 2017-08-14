using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.MetadataServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Project = Microsoft.Build.Evaluation.Project;
using EnvDTE80;

namespace ChartPoints
{
  public class CPOrchestrator : ICPOrchestrator
  {
    private ServiceHost serviceHost;
    public void OnBuildProjConfigBegin(string projName, string projConfig
      , string platform, string solConfig)
    {
      if (solConfig.Contains(" [ChartPoints]"))
      {
        EnvDTE.Project proj = Globals.dte.Solution.Projects.Item(projName);
        SaveProjChartPonts(proj.FullName);
        Orchestrate(proj.FullName);
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
        ;//serviceHost.Close();
      }
    }

    //private void CheckAndAddSolConf(ref SolutionBuild2 solBuild
    //  , ref /*IEnumerable<SolutionConfiguration>*/IDictionary<string, SolutionConfiguration> solConfs, string confType)
    //{
    //  //if (!solConfs.Any(sc => (sc.Name.Contains(confType))))
    //  //{
    //    IEnumerable<KeyValuePair<string, SolutionConfiguration>> scCont = solConfs.Where(sc => (sc.Key.Contains(confType)));
    //    foreach (KeyValuePair<string, SolutionConfiguration> solConf in scCont)
    //    {
    //      SolutionConfiguration cpConf
    //        = solBuild.SolutionConfigurations.Add(solConf.Key + "|ChartPoints", solConf.Key, true);
    //    }
    //  //}
    //}
    //public static IEnumerable<T> ToIEnumerable<T>(IEnumerator enumerator)
    //{
    //  while (enumerator.MoveNext())
    //  {
    //    yield return (T)enumerator.Current;
    //  }
    //}
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

      return true;
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

    public bool SaveProjChartPonts(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return false;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return false;
      // Remove all <ItemGroup><ChartPointFile>...</ChartPointFile></ItemGroup> if any
      IEnumerable<ProjectItemGroupElement> cpGroups
        = projRoot.ItemGroups.Where(ig => (ig.Items.Where(i => (i.ItemType == "ChartPointFile"))).Any());
      if (cpGroups.Any())
      {
        foreach (var cpg in cpGroups)
          projRoot.RemoveChild(cpg);
      }
      ProjectItemGroupElement cpItemGroup = projRoot.AddItemGroup();
      if(Globals.processor.data.chartPoints.Any())
      {
        foreach (var v in Globals.processor.data.chartPoints)
        {
          ProjectItemElement cpsFileItem = cpItemGroup.AddItem("ChartPointFile", v.Key);
          StringWriter strWriter = new StringWriter();
          XmlTextWriter xmlTxtWriter = new XmlTextWriter(strWriter);
          //xmlTxtWriter.WriteStartElement("ChartPoints");
          foreach (var v1 in v.Value)
          {
            IChartPoint chartPnt = v1.Value;
            xmlTxtWriter.WriteStartElement("ChartPoint");
            {
              xmlTxtWriter.WriteElementString("Variable", chartPnt.data.varName);
              xmlTxtWriter.WriteElementString("LineNum", Convert.ToString((Int32) chartPnt.data.lineNum));
              xmlTxtWriter.WriteElementString("LinePos", Convert.ToString((Int32)chartPnt.data.linePos));
              xmlTxtWriter.WriteElementString("Enabled", chartPnt.data.enabled ? "true" : "false");
            }
            xmlTxtWriter.WriteEndElement();
          }
          //xmlTxtWriter.WriteEndElement();
          cpsFileItem.AddMetadata("ChartPoints", strWriter.ToString());
        }
        msbuildProj.ReevaluateIfNecessary();
      }
      msbuildProj.Save();
      //EnvDTE.Project Save

      return true;
    }

    public bool SaveChartPonts()
    {
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
        SaveProjChartPonts(proj.FullName);
      return true;
    }

    public bool LoadProjChartPoints(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return false;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return false;
      // Remove all <ItemGroup><ChartPointFile>...</ChartPointFile></ItemGroup> if any
      IEnumerable<ProjectItemGroupElement> cpGroups
        = projRoot.ItemGroups.Where(ig => (ig.Items.Where(i => (i.ItemType == "ChartPointFile"))).Any());
      if (cpGroups.Any())
      {
        ICPConfLoader confLoader = new CPConfLoader();
        foreach (ProjectItemGroupElement cpg in cpGroups)
        {
          ProjectItemElement cpFileElem = cpg.Items.ElementAt(0);
          if (cpFileElem.HasMetadata)
          {
            foreach (ProjectMetadataElement me in cpFileElem.Metadata)
            {
              //IDictionary<int, IChartPoint> fileChartPoints = Globals.processor.GetOrCreateFileChartPoints(cpFileElem.Include);
              Action<int, ChartPointData> addCPDataAction
                = (i, data) =>
                {
                  data.fileName = cpFileElem.Include;
                  //################################################ !!!!!!!!!! data.fileFullName !!!!!!!!!! ################################################
                  data.lineNum = i;
                  Globals.processor.AddChartPoint(data);
                };
              confLoader.LoadChartPoint("<" + me.Name + ">" + me.Value + "</" + me.Name + ">", addCPDataAction);
            }
          }
        }
      }
      return true;
    }

    public bool Orchestrate(string projConfFile)
    {
      Microsoft.Build.Evaluation.Project msbuildProj = ProjectCollection.GlobalProjectCollection.LoadProject(projConfFile);
      if (msbuildProj == null)
        return false;
      ProjectRootElement projRoot = msbuildProj.Xml;
      if (projRoot == null)
        return false;
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
        ProjectTargetElement target = projRoot.AddTarget("GenerateToolOutput");
        target.BeforeTargets = "ClCompile";
        ProjectTaskElement task = target.AddTask("CPInstrBuildTask");
        task.SetParameter("InputSrcFiles", "@(ClCompile)");
        task.SetParameter("InputHeaderFiles", "@(ClInclude)");
        task.SetParameter("InputChartPoints", "@(ChartPointFile)");
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
      msbuildProj.Save();
      //EnvDTE.Project Save

      return true;
    }
    public bool Build()
    {
      return false;
    }
    public bool Run()
    {
      return false;
    }
  }
}

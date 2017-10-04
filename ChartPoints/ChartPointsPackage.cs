//------------------------------------------------------------------------------
// <copyright file="ChartPointsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ChartPoints
{

  internal class VsSolutionEvents : IVsSolutionEvents
  {

    //rest of the code
    public int OnAfterCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
      object propItemObj = null;
      pStubHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out propItemObj);
      pRealHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out propItemObj);
      //foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //{
      //  bool ret = false;
      //  if (proj.Name != "Miscellaneous Files")
      //    ret = Globals.orchestrator.LoadProjChartPoints(proj);
      //}
      return VSConstants.S_OK;
    }

    // Somethimes it's not called...
    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
    {
      object propItemObj = null;
      pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_Name, out propItemObj);
      if (propItemObj != null)
      {
        string projName = (string)propItemObj;
        if (projName != "Miscellaneous Files")
        {
          EnvDTE.Project theProj = null;
          foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
          {
            if (proj.Name == projName)
              theProj = proj;
          }
          if (theProj != null)
          {
            Globals.orchestrator.LoadProjChartPoints(theProj);
            if (savedProjects.Contains(projName))
              savedProjects.Remove(projName);
          }
        }
      }
      //if (fAdded == 1)
      //{
      //  foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //  {
      //    bool ret = false;
      //    if (proj.Name != "Miscellaneous Files")
      //      ret = Globals.orchestrator.LoadProjChartPoints(proj);
      //  }
      //}

      return VSConstants.S_OK;
    }

    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
      Globals.orchestrator.InitSolutionConfigurations();
      string activeConfig = (string)Globals.dte.Solution.Properties.Item("ActiveConfig").Value;
      if (activeConfig.Contains(" [ChartPoints]"))
        ChartPointsViewTWCommand.Instance.Enable(true);
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      {
        if (proj.Name != "Miscellaneous Files")
        {
          Globals.orchestrator.LoadProjChartPoints(proj);
        }
      }

      return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
    {
      //object propItemObj = null;
      //pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out propItemObj);
      //if (propItemObj != null)
      //{
      //  string projName = (string)propItemObj;
      //  if (projName != "Miscellaneous Files")
      //  {
      //    EnvDTE.Project theProj = null;
      //    foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //    {
      //      if (proj.Name == projName)
      //        theProj = proj;
      //    }
      //    if (theProj != null)
      //    {
      //      Globals.orchestrator.SaveProjChartPoints(theProj);
      //      Globals.orchestrator.UnloadProject(theProj);
      //    }
      //  }
      //}
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
    {
      return VSConstants.S_OK;
    }

    private ISet<string> savedProjects = new SortedSet<string>();
    public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
    {
      object propItemObj = null;
      pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out propItemObj);
      if (propItemObj != null)
      {
        string projName = (string)propItemObj;
        if (projName != "Miscellaneous Files")
        {
          EnvDTE.Project theProj = null;
          foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
          {
            if (proj.Name == projName)
              theProj = proj;
          }
          if (theProj != null && !savedProjects.Contains(projName))
          {
            Globals.orchestrator.SaveProjChartPoints(theProj);
            Globals.orchestrator.UnloadProject(theProj);
            savedProjects.Add(projName);
          }
          pfCancel = 0;
        }
      }
      //foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //{
      //  if (proj.Name != "Miscellaneous Files")
      //  {
      //    if (!savedProjects.Contains(proj.Name))
      //    {
      //      Globals.orchestrator.SaveProjChartPoints(proj);
      //      savedProjects.Add(proj.Name);//!!!!!!! update savedProjects where needed !!!!!!!
      //      Globals.orchestrator.UnloadProject(proj);
      //      pfCancel = 0;
      //    }
      //  }
      //}

      return VSConstants.S_OK;
    }

    public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
    {
      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
    {
      return VSConstants.S_OK;
    }
  }

  internal class VSUpdateSolEvents : IVsUpdateSolutionEvents3
  {
    public int OnAfterActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
    {
      string newConfName;
      pNewActiveSlnCfg.get_DisplayName(out newConfName);
      if (newConfName.Contains(" [ChartPoints]"))
      {
        ChartPointsViewTWCommand.Instance.Enable(true);
        CPListTWCommand.Instance.Enable(true);
      }
      else
      {
        string prevConfName;
        pOldActiveSlnCfg.get_DisplayName(out prevConfName);
        if (prevConfName.Contains(" [ChartPoints]"))
        {
          ChartPointsViewTWCommand.Instance.Enable(false);
          CPListTWCommand.Instance.Enable(false);
        }
      }

      return VSConstants.S_OK;
    }

    public int OnBeforeActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
    {
      return VSConstants.S_OK;
    }
  }

  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell. These attributes tell the pkgdef creation
  /// utility what data to put into .pkgdef file.
  /// </para>
  /// <para>
  /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  /// </para>
  /// </remarks>
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
  [Guid(ChartPointsPackage.PackageGuidString)]
  //[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
  [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideToolWindow(typeof(ChartPointsViewTW))]
  [ProvideToolWindowVisibility(typeof(ChartPointsViewTW), VSConstants.UICONTEXT.SolutionExists_string)]
  [ProvideToolWindow(typeof(CPListTW))]
  [ProvideToolWindowVisibility(typeof(CPListTW), VSConstants.UICONTEXT.SolutionExists_string)]
  public sealed class ChartPointsPackage : Package
  {
    private ChartPntFactory factory;

    private IVsSolutionBuildManager3 buildManager3;

    /// <summary>
    /// ChartPointsPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "a6b9b46f-0163-4255-807e-b3e04aa6ca70";

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPointsPackage"/> class.
    /// </summary>
    public ChartPointsPackage()
    {
    }

    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();
      Globals.dte = (DTE)GetService(typeof(DTE));
      Globals.cpEventsService = new CPEventService();
      factory = new ChartPntFactoryImpl();
      Globals.processor = factory.CreateProcessor();
      Globals.orchestrator = factory.CreateOrchestrator();
      //Globals.orchestrator.InitSolutionConfigurations();
      Globals.outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      IVsSolution vsSolution = GetService(typeof(SVsSolution)) as IVsSolution;
      VsSolutionEvents solEvents = new VsSolutionEvents();
      uint solEvsCookie;
      vsSolution.AdviseSolutionEvents(solEvents, out solEvsCookie);
      buildManager3 = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager3;
      VSUpdateSolEvents solUpdateEvents = new VSUpdateSolEvents();
      uint solUpdateEvsCookie;
      buildManager3.AdviseUpdateSolutionEvents3(solUpdateEvents, out solUpdateEvsCookie);
      //EnvDTE.DebuggerEvents debugEvents = _applicationObject.Events.DebuggerEvents;

      ChartPntToggleCmd.Initialize(this);
      ChartPointsViewTWCommand.Initialize(this);
      Globals.cpTracer = ChartPointsViewTWCommand.Instance;
      CPListTWCommand.Initialize(this);
    }

    public static void StartEvents(DTE dte)
    {
      System.Windows.Forms.MessageBox.Show("Events are attached.");
    }

    #endregion
  }
}

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
using System.Diagnostics;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using Microsoft.Internal.VisualStudio.Shell;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Debugger.Interop;
using ChartPoints.CPServices.impl;
using ChartPoints.CPServices.decl;
using Microsoft.Build.Framework;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Build.Evaluation;
using System.Linq;

namespace ChartPoints
{

  internal class VsSolutionEvents : IVsSolutionEvents
  {
    private Package package;
    private ICPProps cpProps;

    public VsSolutionEvents(Package _package)
    {
      package = _package;
    }

    public void SetPropsStream(Microsoft.VisualStudio.OLE.Interop.IStream _propsStream)
    {
      if (cpProps == null)
        cpProps = new CPProps();
      cpProps.SetPropsStream(_propsStream);
    }

    private bool LoadCPProps()
    {
      if (cpProps == null)
        return false;
      cpProps.Load();

      return true;
    }

    public int OnAfterCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    private void EnableCPVsGui(bool enable)
    {
      if (enable && ChartPntToggleCmd.Instance == null)
        ChartPntToggleCmd.Initialize(package);
      if (enable && CPTableViewTWCmd.Instance == null)
        CPTableViewTWCmd.Initialize(package);
      if(CPTableViewTWCmd.Instance != null)
        CPTableViewTWCmd.Instance.Enable(enable);
      if (enable && CPChartViewTWCmd.Instance == null)
        CPChartViewTWCmd.Initialize(package);
      if(CPChartViewTWCmd.Instance != null)
        CPChartViewTWCmd.Instance.Enable(enable);
      if (enable && CPListTWCommand.Instance == null)
        CPListTWCommand.Initialize(package);
      if(CPListTWCommand.Instance != null)
        CPListTWCommand.Instance.Enable(enable);
      if(!enable)
      {
        if (CPTableViewTWCmd.Instance != null)
          CPTableViewTWCmd.Instance.Close();
        if (CPChartViewTWCmd.Instance != null)
          CPChartViewTWCmd.Instance.Close();
        if (CPListTWCommand.Instance != null)
          CPListTWCommand.Instance.Close();
      }
    }

    private bool IsCPPProject(IVsHierarchy pHierarchy, out EnvDTE.Project proj)
    {
      proj = null;
      object propProjObj = null;
      pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out propProjObj);
      if (propProjObj != null)
        proj = propProjObj as EnvDTE.Project;
      if (proj != null && proj.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" && proj.Name != "Miscellaneous Files")
        return true;
      proj = null;

      return false;
    }

    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
      EnvDTE.Project proj = null;
      if (IsCPPProject(pRealHierarchy, out proj))
      {
        Globals.orchestrator.InitProjConfigurations(proj);
        EnableCPVsGui(true);
      }

      return VSConstants.S_OK;
    }

    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
    {
      EnvDTE.Project proj = null;
      if (IsCPPProject(pHierarchy, out proj))
      {
        Globals.orchestrator.InitProjConfigurations(proj);
        EnableCPVsGui(true);
      }

      return VSConstants.S_OK;
    }

    private int CPPProjectsCount()
    {
      int num = 0;
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      {
        if (proj.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" && proj.Name != "Miscellaneous Files")
          ++num;
      }

      return num;
    }

    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
    {
      EnvDTE.Project proj = null;
      if (IsCPPProject(pHierarchy, out proj))
      {
        Globals.processor.RemoveChartPoints(proj.Name);
        if (CPPProjectsCount() == 1)
          EnableCPVsGui(false);
      }

      return VSConstants.S_OK;
    }

    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
      Globals.orchestrator.InitSolutionConfigurations();
      string activeConfig = (string)Globals.dte.Solution.Properties.Item("ActiveConfig").Value;
      if (activeConfig.Contains(" [ChartPoints]"))
      {
        if (CPChartViewTWCmd.Instance == null)
          CPChartViewTWCmd.Initialize(package);
        CPChartViewTWCmd.Instance.Enable(true);
      }
      LoadCPProps();

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
  //[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
  //[ProvideAutoLoad(VSConstants.UICONTEXT.VCProject_string)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideToolWindow(typeof(CPChartViewTW))]
  [ProvideToolWindowVisibility(typeof(CPChartViewTW), VSConstants.UICONTEXT.VCProject_string/*SolutionExists_string*/)]
  [ProvideToolWindow(typeof(CPListTW))]
  [ProvideToolWindowVisibility(typeof(CPListTW), VSConstants.UICONTEXT.VCProject_string/*SolutionExists_string*/)]
  [ProvideToolWindow(typeof(CPTableViewTW))]
  [ProvideToolWindowVisibility(typeof(CPTableViewTW), VSConstants.UICONTEXT.VCProject_string/*SolutionExists_string*/)]
  public sealed class ChartPointsPackage
    : Package
    , IVsPersistSolutionOpts
    , IVsSolutionLoadManager
  {
    private ChartPntFactory factory;

    private IVsSolutionBuildManager3 buildManager3;
    private VsSolutionEvents solEvents;

    CmdEventsHandler cmdEvsHandler;

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
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.RegisterService<ICPTracerService>(new CPTracerService());

      IVsDebugger vsDebugService = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger;
      if (vsDebugService != null)
        cpServProv.RegisterService<ICPDebugService>(new CPDebugService(vsDebugService));

      ICPExtension extensionServ = new CPExtension();
      cpServProv.RegisterService<ICPExtension>(extensionServ);

      Globals.dte = (DTE)GetService(typeof(DTE));
      factory = new ChartPntFactoryImpl();
      if(Globals.processor == null)
        Globals.processor = factory.CreateProcessor();
      Globals.orchestrator = factory.CreateOrchestrator();
      IVsSolution vsSolution = GetService(typeof(SVsSolution)) as IVsSolution;
      object objLoadMgr = this;   //the class that implements IVsSolutionManager  
      vsSolution.SetProperty((int)__VSPROPID4.VSPROPID_ActiveSolutionLoadManager, objLoadMgr);
      solEvents = new VsSolutionEvents(this);
      uint solEvsCookie;
      vsSolution.AdviseSolutionEvents(solEvents, out solEvsCookie);
      buildManager3 = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager3;
      cmdEvsHandler = new CmdEventsHandler(vsSolution);

      string vsixInstPath = extensionServ.GetVSIXInstallPath();
      string regSrvFName = vsixInstPath + "\\cper.exe";
      if (File.Exists(regSrvFName))
      {
        string message = "First time registration.\nAdministration privileges needed.";
        string caption = "ChartPoints";
        MessageBoxButtons buttons = MessageBoxButtons.OK;
        MessageBox.Show(message, caption, buttons);
        var p = new System.Diagnostics.Process();
        p.StartInfo.FileName = regSrvFName;
        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        p.StartInfo.Verb = "runas";
        if (p.Start())
        {
          p.WaitForExit();
          File.Delete(regSrvFName);
        }
      }
    }

    public int QuerySaveSolutionProps([In] IVsHierarchy pHierarchy, [Out] VSQUERYSAVESLNPROPS[] pqsspSave)//!!! NOT CALLED !!!KKKH
    {
      pqsspSave[0] = VSQUERYSAVESLNPROPS./*QSP_HasNoDirtyProps*/QSP_HasDirtyProps;
      return VSConstants.S_OK;
    }

    public int SaveSolutionProps([In] IVsHierarchy pHierarchy, [In] IVsSolutionPersistence pPersistence)
    { return VSConstants.S_OK; }
    public int WriteSolutionProps([In] IVsHierarchy pHierarchy, [In] string pszKey, [In] IPropertyBag pPropBag)
    { return VSConstants.S_OK; }
    public int ReadSolutionProps([In] IVsHierarchy pHierarchy, [In] string pszProjectName, [In] string pszProjectMk, [In] string pszKey, [In] int fPreLoad, [In] IPropertyBag pPropBag)
    { return VSConstants.S_OK; }

    private readonly string cpSuoKey = "ChartPointsData";

    public int SaveUserOptions(IVsSolutionPersistence pPersistence)
    {
      pPersistence.SavePackageUserOpts(this, cpSuoKey);

      return VSConstants.S_OK;
    }

    public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
    {
      pPersistence.LoadPackageUserOpts(this, cpSuoKey);

      return VSConstants.S_OK;
    }

    public int WriteUserOptions(Microsoft.VisualStudio.OLE.Interop.IStream pOptionsStream, string pszKey)
    {
      if(pszKey.CompareTo(cpSuoKey) == 0)
      {
        if (Globals.processor.HasData())
        {
          ICPProps cpProps = new CPProps();
          cpProps.Save(pOptionsStream);
        }
      }

      return VSConstants.S_OK;
    }

    public int ReadUserOptions(Microsoft.VisualStudio.OLE.Interop.IStream pOptionsStream, string pszKey)
    {
      if (pszKey.CompareTo(cpSuoKey) == 0)
      {
        if (pOptionsStream == null)
          return VSConstants.S_OK;
        this.solEvents.SetPropsStream(pOptionsStream);
      }

      return VSConstants.S_OK;
    }

    public int OnDisconnect()
    { return VSConstants.S_OK; }

    public int OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName, IVsSolutionLoadManagerSupport pSLMgrSupport)
    {
      Guid cppProjGuid = new Guid("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
      if (guidProjectType == cppProjGuid)
      {
        Microsoft.Build.Evaluation.Project msbuildProj = Globals.orchestrator.Orchestrate(pszFileName);
      }

      return VSConstants.S_OK;
    }

    #endregion
  }

}

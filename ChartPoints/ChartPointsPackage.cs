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

namespace ChartPoints
{

  internal class VsSolutionEvents : IVsSolutionEvents
  {
    //private Microsoft.VisualStudio.OLE.Interop.IStream propsStream;
    private MemoryStream propsStream;

    public void SetPropsStream(Microsoft.VisualStudio.OLE.Interop.IStream _propsStream)
    {
      DataStreamFromComStream pStream = new DataStreamFromComStream(_propsStream);
      if (propsStream == null)
        propsStream = new MemoryStream();
      pStream.CopyTo(propsStream);
    }

    private bool LoadCPProps()
    {
      if (propsStream == null || propsStream.Length == 0)
        return false;
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
      formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;//!!! while develop !!!
      propsStream.Position = 0;
      CPPropsDeSerializator cpPropsDeser;
      Func<CPPropsDeSerializator> desearalize = () => (CPPropsDeSerializator)formatter.Deserialize(propsStream);
      if (SynchronizationContext.Current != null)
      {
        System.Threading.Tasks.Task.Run(desearalize);
      }
      else
      {
        cpPropsDeser = desearalize();
      }
      //CPPropsDeSerializator cpPropsDeser = (CPPropsDeSerializator)formatter.Deserialize(propsStream/*pStream*/);

      return true;
    }

    public int OnAfterCloseSolution(object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
      return VSConstants.S_OK;
    }

    // Somethimes it's not called...
    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
    {

      return VSConstants.S_OK;
    }

    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
      Globals.orchestrator.InitSolutionConfigurations();
      string activeConfig = (string)Globals.dte.Solution.Properties.Item("ActiveConfig").Value;
      if (activeConfig.Contains(" [ChartPoints]"))
        CPChartViewTWCmd.Instance.Enable(true);
      LoadCPProps();

      return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
    {
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

  internal class VSUpdateSolEvents : IVsUpdateSolutionEvents3
  {
    public int OnAfterActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
    {
      string newConfName;
      pNewActiveSlnCfg.get_DisplayName(out newConfName);
      if (newConfName.Contains(" [ChartPoints]"))
      {
        CPChartViewTWCmd.Instance.Enable(true);
        CPTableViewTWCmd.Instance.Enable(true);
        CPListTWCommand.Instance.Enable(true);
      }
      else
      {
        string prevConfName;
        if (pOldActiveSlnCfg != null)
        {
          pOldActiveSlnCfg.get_DisplayName(out prevConfName);
          if (prevConfName.Contains(" [ChartPoints]"))
          {
            CPChartViewTWCmd.Instance.Enable(false);
            CPTableViewTWCmd.Instance.Enable(false);
            CPListTWCommand.Instance.Enable(false);
          }
        }
      }

      return VSConstants.S_OK;
    }

    public int OnBeforeActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
    {
      return VSConstants.S_OK;
    }
  }


  //internal class RunningDocTableEvents : IVsRunningDocTableEvents3
  //{
  //  private RunningDocumentTable rdt;

  //  public RunningDocTableEvents(RunningDocumentTable _rdt)
  //  {
  //    rdt = _rdt;
  //  }
  //  public int OnBeforeSave(uint docCookie)
  //  {
  //    return VSConstants.S_OK;
  //  }

  //  public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) { return VSConstants.S_OK; }
  //  public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld,
  //                                      uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew,
  //                                      uint itemidNew, string pszMkDocumentNew)
  //  {
  //    return VSConstants.S_OK;
  //  }

  //  public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) { return VSConstants.S_OK; }
  //  public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
  //  {
  //    return VSConstants.S_OK;
  //  }

  //  public int OnAfterSave(uint docCookie)
  //  {
  //    return VSConstants.S_OK;
  //  }
  //  public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) { return VSConstants.S_OK; }

  //  public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
  //  {
  //    return VSConstants.S_OK;
  //  }
  //}

  //#####################################################################

  sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
  {
    public override Type BindToType(string assemblyName, string typeName)
    {
      //String currentAssembly = Assembly.GetExecutingAssembly().FullName;

      //// In this case we are always using the current assembly
      //assemblyName = currentAssembly;

      //// Get the type using the typeName and assemblyName
      //Type typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
      //    typeName, assemblyName));

      //return typeToDeserialize;
      //https://techdigger.wordpress.com/2007/12/22/deserializing-data-into-a-dynamically-loaded-assembly/
      Type typeToDeserialize = null;
      try
      {
        string ToAssemblyName = assemblyName.Split(',')[0];
        Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly ass in Assemblies)
        {
          if (ass.FullName.Split(',')[0] == ToAssemblyName)
          {
            typeToDeserialize = ass.GetType(typeName);
            break;
          }
        }
      }
      catch (System.Exception exception)
      {
        throw exception;
      }

      return typeToDeserialize;
    }
  }

  [Serializable]
  public class CPPropsDeSerializator : ISerializable
  {
    public CPPropsDeSerializator() {}
    protected CPPropsDeSerializator(SerializationInfo info, StreamingContext context)
    {
      //if (Globals.processor == null)
      //  Globals.processor = factor
      //Globals.processor.DeserializeProps(info, context);
      try
      {
        UInt32 projsCount = info.GetUInt32("projPoints.Count");
        for (uint p = 0; p < projsCount; ++p)
        {
          //IProjectChartPoints projCPs = null;
          string projName = info.GetString("projName_" + p.ToString());
          Globals.processor.RemoveChartPoints(projName);
          //Globals.processor.AddProjectChartPoints(projName, out projCPs);
          UInt32 filesCount = info.GetUInt32("filePoints.Count_" + p.ToString());
          if (filesCount > 0)
          {
            IProjectChartPoints projCPs = Globals.processor.GetProjectChartPoints(projName);
            if (projCPs == null)
              Globals.processor.AddProjectChartPoints(projName, out projCPs);
            for (uint f = 0; f < filesCount; ++f)
            {
              string fileName = info.GetString("fileName_" + p.ToString() + f.ToString());
              IFileChartPoints fPnts = projCPs.AddFileChartPoints(fileName);
              if (fPnts != null)
              {
                UInt32 linesCount = info.GetUInt32("linePoints.Count_" + p.ToString() + f.ToString());
                for (uint l = 0; l < linesCount; ++l)
                {
                  //ITextPosition pos = (ITextPosition)info.GetValue("pos_" + p.ToString() + f.ToString() + l.ToString(), typeof(ITextPosition));
                  UInt32 lineNum = info.GetUInt32("lineNum_" + p.ToString() + f.ToString() + l.ToString());
                  UInt32 linePos = info.GetUInt32("linePos_" + p.ToString() + f.ToString() + l.ToString());
                  ILineChartPoints lPnts = fPnts.AddLineChartPoints(/*pos.*/(int)lineNum, /*pos.*/(int)linePos);
                  if (lPnts != null)
                  {
                    UInt32 cpsCount = info.GetUInt32("cpsPoints.Count_" + p.ToString() + f.ToString() + l.ToString());
                    for (uint cp = 0; cp < cpsCount; ++cp)
                    {
                      IChartPoint chartPnt = null;
                      string uniqueName = info.GetString("uniqueName_" + p.ToString() + f.ToString() + l.ToString() + cp.ToString());
                      bool enabled = info.GetBoolean("enabled_" + p.ToString() + f.ToString() + l.ToString() + cp.ToString());
                      if (lPnts.AddChartPoint(uniqueName, out chartPnt))
                        chartPnt.SetStatus(enabled ? EChartPointStatus.SwitchedOn : EChartPointStatus.SwitchedOff);
                    }
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("projPoints.Count", (UInt32)Globals.processor.data.projPoints.Count);
      int p = 0;
      foreach (IProjectChartPoints projCPs in Globals.processor.data.projPoints)
      {
        info.AddValue("projName_" + p.ToString(), projCPs.data.projName);
        info.AddValue("filePoints.Count_" + p.ToString(), (UInt32)projCPs.filePoints.Count);
        int f = 0;
        foreach (IFileChartPoints fileCPs in projCPs.filePoints)
        {
          info.AddValue("fileName_" + p.ToString() + f.ToString(), fileCPs.data.fileName);
          info.AddValue("linePoints.Count_" + p.ToString() + f.ToString(), (UInt32)fileCPs.linePoints.Count);
          int l = 0;
          foreach (ILineChartPoints lineCPs in fileCPs.linePoints)
          {
            //info.AddValue("pos_" + p.ToString() + f.ToString() + l.ToString(), lineCPs.data.pos, lineCPs.data.pos.GetType());
            info.AddValue("lineNum_" + p.ToString() + f.ToString() + l.ToString(), (UInt32) lineCPs.data.pos.lineNum);
            info.AddValue("linePos_" + p.ToString() + f.ToString() + l.ToString(), (UInt32) lineCPs.data.pos.linePos);
            info.AddValue("cpsPoints.Count_" + p.ToString() + f.ToString() + l.ToString(), (UInt32)lineCPs.chartPoints.Count);
            int c = 0;
            foreach (IChartPoint cp in lineCPs.chartPoints)
            {
              info.AddValue("uniqueName_" + p.ToString() + f.ToString() + l.ToString() + c.ToString(), cp.data.uniqueName);
              info.AddValue("enabled_" + p.ToString() + f.ToString() + l.ToString() + c.ToString(), cp.data.enabled);
              ++c;
            }
            ++l;
          }
          ++f;
        }
        ++p;
      }
    }
  }
  //#####################################################################


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
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideToolWindow(typeof(CPChartViewTW))]
  [ProvideToolWindowVisibility(typeof(CPChartViewTW), VSConstants.UICONTEXT.SolutionExists_string)]
  [ProvideToolWindow(typeof(CPListTW))]
  [ProvideToolWindowVisibility(typeof(CPListTW), VSConstants.UICONTEXT.SolutionExists_string)]
  [ProvideToolWindow(typeof(CPTableViewTW))]
  [ProvideToolWindowVisibility(typeof(CPTableViewTW), VSConstants.UICONTEXT.SolutionExists_string)]
  public sealed class ChartPointsPackage : Package, IVsPersistSolutionOpts, IVsSolutionLoadManager, IVsUpdateSolutionEvents2
  {
    private ChartPntFactory factory;

    private IVsSolutionBuildManager3 buildManager3;
    private VsSolutionEvents solEvents;
    //private CommandEvents cmdEvents;
    //private RunningDocumentTable rdt;

    private IVsSolutionBuildManager2 sbm;
    private uint _sbmCookie;

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
      //Globals.cpEventsService = new CPEventService();
      factory = new ChartPntFactoryImpl();
      if(Globals.processor == null)
        Globals.processor = factory.CreateProcessor();
      Globals.orchestrator = factory.CreateOrchestrator();
      //Globals.orchestrator.InitSolutionConfigurations();
      Globals.outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      IVsSolution vsSolution = GetService(typeof(SVsSolution)) as IVsSolution;
      object objLoadMgr = this;   //the class that implements IVsSolutionManager  
      vsSolution.SetProperty((int)__VSPROPID4.VSPROPID_ActiveSolutionLoadManager, objLoadMgr);
      solEvents = new VsSolutionEvents();
      uint solEvsCookie;
      vsSolution.AdviseSolutionEvents(solEvents, out solEvsCookie);
      buildManager3 = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager3;
      VSUpdateSolEvents solUpdateEvents = new VSUpdateSolEvents();
      uint solUpdateEvsCookie;
      buildManager3.AdviseUpdateSolutionEvents3(solUpdateEvents, out solUpdateEvsCookie);
      //EnvDTE.DebuggerEvents debugEvents = _applicationObject.Events.DebuggerEvents;
      //cmdEvents = Globals.dte.Events.CommandEvents;
      //cmdEvents.BeforeExecute += CmdEvents_BeforeExecute;

      //rdt = new RunningDocumentTable(this);
      //rdt.Advise(new RunningDocTableEvents(rdt));

      ChartPntToggleCmd.Initialize(this);
      CPTableViewTWCmd.Initialize(this);
      CPChartViewTWCmd.Initialize(this);
      //Globals.cpTracer = ChartPointsViewTWCommand.Instance;
      CPListTWCommand.Initialize(this);

      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.RegisterService<ICPTracerService>(new CPTracerService());

      sbm = (IVsSolutionBuildManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager));
      sbm.AdviseUpdateSolutionEvents(this, out _sbmCookie);
      IVsBuildManagerAccessor3 bma3 = (IVsBuildManagerAccessor3)ServiceProvider.GlobalProvider.GetService(typeof(SVsBuildManagerAccessor));
      //IVsMSBuildTaskFileManager
      //IVsBuildManagerAccessor3 vsBuildMgrAcc = GetService(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor3;
      //IVsSolutionBuildManager vsSolBuildMgr = GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
    }

    //private void CmdEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
    //{
    //  Command objCommand = Globals.dte.Commands.Item(Guid, ID);
    //  Debug.WriteLine("CmdEvents_BeforeExecute: " + objCommand.Name);
    //  if (objCommand.Name == "Debug.Start" || objCommand.Name == "Build.BuildSolution")
    //  {
    //    Debug.WriteLine("Debug.Start");
    //  }
    //}

    public static void StartEvents(DTE dte)
    {
      System.Windows.Forms.MessageBox.Show("Events are attached.");
    }

    public int QuerySaveSolutionProps([In] IVsHierarchy pHierarchy, [Out] VSQUERYSAVESLNPROPS[] pqsspSave)//!!! NOT CALLED !!!KKKH
    {
      pqsspSave[0] = VSQUERYSAVESLNPROPS./*QSP_HasNoDirtyProps*/QSP_HasDirtyProps;
      return VSConstants.S_OK;
    }
    public int SaveSolutionProps([In] IVsHierarchy pHierarchy, [In] IVsSolutionPersistence pPersistence)
    {
      return VSConstants.S_OK;
    }
    public int WriteSolutionProps([In] IVsHierarchy pHierarchy, [In] string pszKey, [In] IPropertyBag pPropBag)
    {
      return VSConstants.S_OK;
    }

    public int ReadSolutionProps([In] IVsHierarchy pHierarchy, [In] string pszProjectName, [In] string pszProjectMk, [In] string pszKey, [In] int fPreLoad, [In] IPropertyBag pPropBag)
    {
      return VSConstants.S_OK;
    }
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
          DataStreamFromComStream pStream = new DataStreamFromComStream(pOptionsStream);
          BinaryFormatter formatter = new BinaryFormatter();
          formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
          formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;//!!! while develop !!!
          formatter.Serialize(pStream, new CPPropsDeSerializator());
          //formatter.Serialize(pStream, Globals.processor);
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
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName, IVsSolutionLoadManagerSupport pSLMgrSupport)
    {
      Guid cppProjGuid = new Guid("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
      if (guidProjectType == cppProjGuid)
      {
        Microsoft.Build.Evaluation.Project msbuildProj = Globals.orchestrator.Orchestrate(pszFileName);
      }

      return VSConstants.S_OK;
    }

    public int UpdateSolution_Begin(ref int pfCancelUpdate)
    {
      return VSConstants.S_OK;
    }

    public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
      return VSConstants.S_OK;
    }

    public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
    {
      return VSConstants.S_OK;
    }

    public int UpdateSolution_Cancel()
    {
      return VSConstants.S_OK;
    }

    public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
    {
      return VSConstants.S_OK;
    }

    public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
    {
      IVsProjectCfg cfg = pCfgProj as IVsProjectCfg;
      return VSConstants.S_OK;
    }

    public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
    {
      return VSConstants.S_OK;
    }
    #endregion
  }
}

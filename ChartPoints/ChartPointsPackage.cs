//------------------------------------------------------------------------------
// <copyright file="ChartPointsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using Microsoft.VisualStudio.Text.Projection;

namespace ChartPoints
{

  internal class VsSolutionEvents : IVsSolutionEvents
  {

    //rest of the code
    public int OnAfterCloseSolution(object pUnkReserved)
    {
      //throw new NotImplementedException();
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
      //throw new NotImplementedException();
      //foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      //{
      //  bool ret = false;
      //  if (proj.Name != "Miscellaneous Files")
      //    ret = Globals.orchestrator.LoadProjChartPoints(proj);
      //}
      return VSConstants.S_OK;
    }

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
      //throw new NotImplementedException();
      return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
    {
      //throw new NotImplementedException();
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
      //throw new NotImplementedException();
      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
    {
      //throw new NotImplementedException();
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
        ChartPointsViewTWCommand.Instance.Enable(true);
      else
      {
        string prevConfName;
        pOldActiveSlnCfg.get_DisplayName(out prevConfName);
        if (prevConfName.Contains(" [ChartPoints]"))
          ChartPointsViewTWCommand.Instance.Enable(false);
      }

      return VSConstants.S_OK;
    }

    public int OnBeforeActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
    {
      return VSConstants.S_OK;
    }
  }

  public class FileChangeTracker
  {
    private IWpfTextView curTextView;
    public string fileFullName;
    private IFileChartPoints fPnts;

    public FileChangeTracker(IWpfTextView textView, string _fileFullName)
    {
      fileFullName = _fileFullName;
      Advise(textView);
    }

    public void Advise(IWpfTextView textView)
    {
      // !!! CHECK EQUAL - need readvise !!!
      if(curTextView != null)
        curTextView.TextBuffer.Changed -= TextBufferOnChanged;
      curTextView = textView;
      curTextView.TextBuffer.Changed += TextBufferOnChanged;
    }
    private void TextBufferOnChanged(object sender, TextContentChangedEventArgs e)
    {
      //ITextDocument textDoc;
      //IFileChartPoints fPnts1;
      //string fileName = null;
      //var rc = curTextView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);
      //if (rc)
      //{
      //  fileName = Path.GetFileName(textDoc.FilePath);
      //  EnvDTE.Document dteDoc = Globals.dte.Documents.Item(fileName);
      //  IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(dteDoc.ProjectItem.ContainingProject.Name);
      //  if (pPnts != null)
      //    fPnts1 = pPnts.GetFileChartPoints(fileName);
      //}
      string fileName = Path.GetFileName(fileFullName);
      EnvDTE.Document dteDoc = Globals.dte.Documents.Item(fileName);
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(dteDoc.ProjectItem.ContainingProject.Name);
      if (pPnts != null)
      {
        fPnts = pPnts.GetFileChartPoints(fileName);
        if (fPnts != null)
        {
          ITextSnapshot snapshot = e.After;
          foreach (ITextChange change in e.Changes)
          {
            int lineNum = snapshot.GetLineFromPosition(change.NewPosition).LineNumber;
            fPnts.ValidatePosition(lineNum + 1, change.LineCountDelta);
          }
        }
      }
    }

  }

  [ContentType("C/C++")]
  [Export(typeof(IWpfTextViewCreationListener))]
  [TextViewRole(PredefinedTextViewRoles.Editable)]
  public sealed class TextChangedListener : IWpfTextViewCreationListener, ITextChangedListener
  {
    //private IWpfTextView curTextView;
    ////private IProjectionBufferFactoryService projectionFactory;
    ////private IProjectionBuffer projBuffer;
    //private IFileChartPoints fPnts;

    private ISet<FileChangeTracker> fileTrackers
      = new SortedSet<FileChangeTracker>(Comparer<FileChangeTracker>.Create((lh, rh) => (String.Compare(lh.fileFullName, rh.fileFullName, StringComparison.Ordinal))));

    public void TextViewCreated(IWpfTextView textView)
    {
      //curTextView = textView;
      ITextDocument textDoc;
      string fileName = null;
      var rc = textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);
      if (rc)
      {
        fileName = Path.GetFileName(textDoc.FilePath);
        FileChangeTracker fTracker = fileTrackers.FirstOrDefault((ft) => (ft.fileFullName == textDoc.FilePath));
        if (fTracker == null)
        {
          fTracker = new FileChangeTracker(textView, textDoc.FilePath);
          fileTrackers.Add(fTracker);
        }
        else
          fTracker.Advise(textView);
      }
    }

    public TextChangedListener()
    {
      //projectionFactory = ChartPointsPackage.componentModel.GetService<IProjectionBufferFactoryService>();
      Globals.textChangedListener = this;
    }

    //public void TrackCurPoint()
    //{
    //  projBuffer = projectionFactory.CreateProjectionBuffer( null, new object[0], ProjectionBufferOptions.None);
    //  SnapshotPoint sp = curTextView.Caret.Position.BufferPosition;
    //  ITrackingSpan ts = sp.Snapshot.CreateTrackingSpan(curTextView.Caret.Position.BufferPosition.Position, 1, SpanTrackingMode.EdgeNegative);
    //  projBuffer.InsertSpan(0, ts);
    //  //projBuffer.PostChanged += ProjBuffer_PostChanged;
    //  projBuffer.Changed += ProjBufferOnChanged;
    //  //projBuffer.ContentTypeChanged += ProjBufferOnContentTypeChanged;
    //  //projBuffer.Changing += ProjBufferOnChanging;
    //}

    //private void ProjBufferOnChanging(object sender, TextContentChangingEventArgs textContentChangingEventArgs)
    //{
    //  System.Windows.Forms.MessageBox.Show("ProjBufferOnChanging");
    //}

    //private void ProjBufferOnContentTypeChanged(object sender, ContentTypeChangedEventArgs contentTypeChangedEventArgs)
    //{
    //  System.Windows.Forms.MessageBox.Show("ProjBufferOnContentTypeChanged");
    //}

    //private void ProjBufferOnChanged(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
    //{
    //  System.Windows.Forms.MessageBox.Show("ProjBufferOnChanged");
    //}

    //private void ProjBuffer_PostChanged(object sender, EventArgs e)
    //{
    //  System.Windows.Forms.MessageBox.Show("ProjBuffer_PostChanged");
    //}
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
  public sealed class ChartPointsPackage : Package
  {
    private ChartPntFactory factory;

    private IVsSolutionBuildManager3 buildManager3;

    //public static IComponentModel componentModel;

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
      //componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
      //Globals.dte.Events.WindowEvents.WindowActivated += OnWindowActivated;
    }

    //private void OnWindowActivated(Window GotFocus, Window LostFocus)
    //{
    //  var textManager = (IVsTextManager)this.GetService(typeof(SVsTextManager));
    //  var componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
    //  var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();
    //  IVsTextView textViewCurrent = null;
    //  textManager.GetActiveView(1, null, out textViewCurrent);
    //  IWpfTextView vpfTextView = editor.GetWpfTextView(textViewCurrent);
    //}

    public static void StartEvents(DTE dte)
    {
      System.Windows.Forms.MessageBox.Show("Events are attached.");
    }

    #endregion
  }
}

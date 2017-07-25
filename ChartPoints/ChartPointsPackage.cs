//------------------------------------------------------------------------------
// <copyright file="ChartPointsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace ChartPoints
{
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
  public sealed class ChartPointsPackage : Package
  {
    private ChartPntFactory factory;

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
      ChartPntToggleCmd.Initialize(this);
    }

    #endregion
  }
}

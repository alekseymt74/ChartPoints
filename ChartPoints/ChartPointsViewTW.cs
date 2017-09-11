//------------------------------------------------------------------------------
// <copyright file="ChartPointsViewTW.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ChartPoints
{
  using System;
  using System.Runtime.InteropServices;
  using Microsoft.VisualStudio.Shell;

  /// <summary>
  /// This class implements the tool window exposed by this package and hosts a user control.
  /// </summary>
  /// <remarks>
  /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
  /// usually implemented by the package implementer.
  /// <para>
  /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
  /// implementation of the IVsUIElementPane interface.
  /// </para>
  /// </remarks>
  [Guid("c6119b45-b5a9-4b8c-89d1-6af00ca9fd90")]
  public class ChartPointsViewTW : ToolWindowPane
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPointsViewTW"/> class.
    /// </summary>
    public ChartPointsViewTW() : base(null)
    {
      this.Caption = "ChartPointsViewTW";

      // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
      // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
      // the object returned by the Content property.
      this.Content = new ChartPointsViewTWControl();
    }

    public void Clear()
    {
      ((ChartPointsViewTWControl) this.Content).Clear();
    }

    public void UpdateView()
    {
      ((ChartPointsViewTWControl)this.Content).UpdateView();
    }

    public ICPTracerDelegate CreateTracer(string varName)
    {
      return ((ChartPointsViewTWControl)this.Content).CreateTracer(varName);
    }

  }
}

//------------------------------------------------------------------------------
// <copyright file="CPTableView.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ChartPoints
{
  using System;
  using System.Runtime.InteropServices;
  using Microsoft.VisualStudio.Shell;
  using System.Windows.Forms;

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
  [Guid("b6e764c8-c1d6-4f02-a17c-f2690ef0f8ec")]
  public class CPTableViewTW : ToolWindowPane
  {
    private CPTableView control;

    /// <summary>
    /// Initializes a new instance of the <see cref="CPTableView"/> class.
    /// </summary>
    public CPTableViewTW() : base(null)
    {
      this.Caption = "CPTableViewTW";

      // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
      // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
      // the object returned by the Content property.
      control = new CPTableView();
    }

    override public IWin32Window Window
    {
      get
      {
        return (IWin32Window)control;
      }
    }

    public void Clear()
    {
      control?.Clear();
    }

    public void Trace(ulong id, double val)
    {
      control?.Trace(id, val);
    }

    public CPTableView GetTraceConsumer()
    {
      return control;
    }

  }
}

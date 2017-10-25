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
  [Guid("c6119b45-b5a9-4b8c-89d1-6af00ca9fd90")]
  public class CPChartViewTW : ToolWindowPane
  {
    private CPChartView control;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPointsViewTW"/> class.
    /// </summary>
    public CPChartViewTW() : base(null)
    {
      this.Caption = "ChartPointsViewTW";

      // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
      // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
      // the object returned by the Content property.
      control = new CPChartView();
    }

    public void SetTraceConsumer(CPTableView _traceConsumer)
    {
      control.SetTraceConsumer(_traceConsumer);
    }

    public ICPTracerDelegate CreateTracer(ulong id, string varName)
    {
      return control.CreateTracer(id, varName);
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

    public void Trace(ulong id, System.Array tms, System.Array vals)
    {
      control?.Trace(id, tms, vals);
    }
    //public void UpdateView()
    //{
    //  ((CPChartView)this.Content).UpdateView();
    //}

    //public ICPTracerDelegate CreateTracer(string varName)
    //{
    //  return ((CPChartView)this.Content).CreateTracer(varName);
    //}

  }
}

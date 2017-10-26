﻿//------------------------------------------------------------------------------
// <copyright file="CPTableViewCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ChartPoints
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CPTableViewTWCmd
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4148;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("14366e93-ed00-403d-8d64-0061064c2054");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;
    private MenuCommand menuItem;
    private CPTableViewTW window;

    /// <summary>
    /// Initializes a new instance of the <see cref="CPTableViewCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CPTableViewTWCmd(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;

      OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
        Enable(false);
        commandService.AddCommand(menuItem);
      }
    }

    public void Activate()
    {
      window = package.FindToolWindow(typeof(ChartPoints.CPTableViewTW), 0, true) as CPTableViewTW;
      Show();
      window?.Clear();
    }

    public void Show()
    {
      IVsWindowFrame frame = window?.Frame as IVsWindowFrame;
      frame?.Show();
    }

    public void Enable(bool flag)
    {
      menuItem.Visible = flag;
      menuItem.Enabled = flag;
    }

    public CPTableView GetTraceConsumer()
    {
      return window.GetTraceConsumer();
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CPTableViewTWCmd Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package package)
    {
      Instance = new CPTableViewTWCmd(package);
    }

    /// <summary>
    /// Shows the tool window when the menu item is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void ShowToolWindow(object sender, EventArgs e)
    {
      // Get the instance number 0 of this tool window. This window is single instance so this instance
      // is actually the only one.
      // The last flag is set to true so that if the tool window does not exists it will be created.
      ToolWindowPane window = this.package.FindToolWindow(typeof(CPTableViewTW), 0, true);
      if ((null == window) || (null == window.Frame))
      {
        throw new NotSupportedException("Cannot create tool window");
      }

      IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
      Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
    }
  }
}

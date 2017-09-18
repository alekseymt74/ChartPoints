﻿//------------------------------------------------------------------------------
// <copyright file="ChartPntToggleCmd.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace ChartPoints
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class ChartPntToggleCmd
  {
    /// <summary>
    /// ChartPoint object
    /// </summary>
    private ICheckPoint checkPnt;
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("14366e93-ed00-403d-8d64-0061064c2054");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPntToggleCmd"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private ChartPntToggleCmd(Package package)
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
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuItem.BeforeQueryStatus += CheckAvailability;
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Checks the availability of insert chartpoint menu item
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckAvailability(object sender, EventArgs e)
    {
      // get the menu that fired the event
      var menuCommand = sender as OleMenuCommand;
      if (menuCommand != null)
      {
        menuCommand.Visible = false;
        menuCommand.Enabled = false;
        TextSelection sel = (TextSelection)Globals.dte.ActiveDocument.Selection;
        checkPnt = Globals.processor.Check(Globals.dte.ActiveDocument.ProjectItem.ContainingProject.Name, (TextPoint)sel.ActivePoint);
        if (checkPnt != null)
        {
          //switch (linePnts.status)
          //{
          //  case ETargetPointStatus.Available:
          //    menuCommand.Text = "Insert ChartPoint";
          //    break;
          //  case ETargetPointStatus.SwitchedOn:
          //  case ETargetPointStatus.SwitchedOff:
          //    menuCommand.Text = "Remove ChartPoint";
          //    break;
          //}
          menuCommand.Visible = true;
          menuCommand.Enabled = true;
        }
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static ChartPntToggleCmd Instance
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
      Instance = new ChartPntToggleCmd(package);
    }

    /// <summary>
    /// Toggle chartpoint
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      List<Tuple<string, string, bool>> availableVars = null;
      checkPnt.GetAvailableVars(out availableVars);
      if (availableVars.Count > 0)
      {
        SelectVarsDlg dlg = new SelectVarsDlg(ref availableVars);
        dlg.ShowDialog();
        ISet<string> selectedVars = dlg.GetSelectedVars();
        if (selectedVars != null)
        {
          if (checkPnt.SyncChartPoints(selectedVars))
          {
            //Globals.textChangedListener.TrackCurPoint();
          }
        }
      }
    }
  }
}

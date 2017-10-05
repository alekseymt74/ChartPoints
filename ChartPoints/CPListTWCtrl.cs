using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartPoints
{
  public partial class CPListTWCtrl : UserControl
  {
    public CPListTWCtrl()
    {
      InitializeComponent();
      this.list.CellDoubleClick += OnListOnCellDoubleClick;
      this.list.KeyDown += OnListOnKeyDown;
      this.list.CellValueChanged += OnCellValueChanged;
      this.list.CellMouseUp += OnCellMouseUp;
      ICPEventProvider<IConstructEvents> evConstrProv;
      if (Globals.cpEventsService.GetConstructEventProvider(out evConstrProv))
        evConstrProv.prov.createdLineCPsEvent.On += OnCreatedLineCPsEvent;
    }

    private void OnCellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
    {
      if (e.ColumnIndex == 0 && e.RowIndex != -1)
        this.list.EndEdit();
    }

    private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 0 && e.RowIndex != -1)
      {
        DataGridViewRow row = list.Rows[e.RowIndex];
        Tuple<ILineChartPoints, IChartPoint> tagData = (Tuple<ILineChartPoints, IChartPoint>)row.Tag;
        //switch(tagData.Item2.data.status)
        //{
        //  case EChartPointStatus.SwitchedOff
        //}
        tagData.Item2.SetStatus(((bool)row.Cells[0].Value) ? EChartPointStatus.SwitchedOn : EChartPointStatus.SwitchedOff);
        if(Globals.taggerUpdater != null)
          Globals.taggerUpdater.RaiseChangeTagEvent(tagData.Item1.data.fileData.fileFullName, tagData.Item1);
      }
    }

    //##########################################################
    private EnvDTE.ProjectItem GetProjItem(EnvDTE.ProjectItems projItems, string fileName)
    {
      if (projItems == null)
        return null;
      foreach (EnvDTE.ProjectItem projItem in projItems)
      {
        if (projItem.Name == fileName)
          return projItem;
        EnvDTE.ProjectItem theProjItem = GetProjItem(projItem.ProjectItems, fileName);
        if (theProjItem != null)
          return theProjItem;
      }

      return null;
    }

    private EnvDTE.ProjectItem GetProjItem(string projName, string itemFName)
    {
      foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
      {
        if (proj.Name == projName)
          return GetProjItem(proj.ProjectItems, itemFName);
      }

      return null;
    }
    //##########################################################

    private void OnListOnCellDoubleClick(object sender, DataGridViewCellEventArgs dataGridViewCellEventArgs)
    {
      if (dataGridViewCellEventArgs.RowIndex < 0)
        return;
      DataGridViewRow row = list.Rows[dataGridViewCellEventArgs.RowIndex];
      Tuple<ILineChartPoints, IChartPoint> tagData = (Tuple<ILineChartPoints, IChartPoint>)row.Tag;
      EnvDTE.ProjectItem projItem = GetProjItem(tagData.Item1.data.fileData.projData.projName, tagData.Item1.data.fileData.fileName);
      if (projItem != null)
      {
        EnvDTE.Window wnd = projItem.Open();
        wnd.Activate();
        EnvDTE.TextSelection sel = (EnvDTE.TextSelection)projItem.Document.Selection;
        sel.MoveToLineAndOffset(tagData.Item1.data.pos.lineNum, tagData.Item1.data.pos.linePos);
      }
    }

    private IChartPoint cpIgnore = null;
    private void OnListOnKeyDown(object sender, KeyEventArgs keyEventArgs)
    {
      switch (keyEventArgs.KeyCode)
      {
        case Keys.Enter:
          break;
        case Keys.Delete:
          if (list.SelectedRows.Count > 0)
          {
            DataGridViewRow row = list.SelectedRows[0];
            Tuple<ILineChartPoints, IChartPoint> tagData = (Tuple<ILineChartPoints, IChartPoint>)row.Tag;
            //tagData.Item1.remCPEvent.On -= OnRemCpEvent;
            cpIgnore = tagData.Item2;
            if (tagData.Item1.RemoveChartPoint(tagData.Item2))
            {
              if (Globals.taggerUpdater != null)
                Globals.taggerUpdater.RaiseChangeTagEvent(tagData.Item1.data.fileData.fileFullName, tagData.Item1);
            }
            //list.Rows.RemoveAt( row.Index );
          }
          break;
      }
    }

    private void OnCreatedLineCPsEvent(IConstructEventArgs<ILineChartPoints> args)
    {
      args.obj.addCPEvent.On += OnAddCpEvent;
      args.obj.remCPEvent.On += OnRemCpEvent;
    }

    private void OnRemCpEvent(CPLineEvArgs args)
    {
      int rowIndex = -1;
      Tuple<ILineChartPoints, IChartPoint> tagData = new Tuple<ILineChartPoints, IChartPoint>(args.lineCPs, args.cp);
      foreach (DataGridViewRow row in list.Rows)
      {
        if (row.Tag.Equals(tagData))
        {
          if (tagData.Item2 == cpIgnore)
            cpIgnore = null;
          else
            rowIndex = row.Index;
          break;
        }
      }
      if (rowIndex > 0)
      {
        //tagData.Item1.remCPEvent.On -= OnRemCpEvent;
        list.Rows.RemoveAt(rowIndex);
      }
    }

    private void OnAddCpEvent(CPLineEvArgs args)
    {
      int i = list.Rows.Add();
      DataGridViewRow row = list.Rows[i];
      row.Tag = new Tuple<ILineChartPoints, IChartPoint>(args.lineCPs, args.cp);
      row.Cells[0].Value = true;
      row.Cells[1].Value = args.cp.data.uniqueName;
      row.Cells[2].Value = args.cp.data.type;
      //list.AutoResizeColumns();
    }
  }
}

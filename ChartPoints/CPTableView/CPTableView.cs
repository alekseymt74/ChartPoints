using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

namespace ChartPoints
{
  public partial class CPTableView : UserControl, ICPTracer
  {
    public ICPEvent<EnableTraceEntEvArgs> enableEvent { get; set; } = new CPEvent<EnableTraceEntEvArgs>();

    static public readonly int EnabledCellInd = 0;
    static public readonly int NameCellInd = 2;
    static public readonly int ValueCellInd = 3;

    private bool ignoreCellValEvents = false;
    private IDictionary<ulong, IList<ICPTracerDelegate>> rowDelegates = new SortedDictionary<ulong, IList<ICPTracerDelegate>>();
    private IDictionary<int, KeyValuePair<ulong, int>> rowIdInds = new SortedDictionary<int, KeyValuePair<ulong, int>>();
    private Timer updateTimer;

    public CPTableView()
    {
      InitializeComponent();
      this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.Dock = DockStyle.Bottom;
    }

    private void UpdateView(object sender, EventArgs e)
    {
      if (table.IsHandleCreated)
      {
        /*IAsyncResult asyncRes = */table./*Begin*/Invoke((MethodInvoker)(() =>
        {
          foreach (KeyValuePair<ulong, IList<ICPTracerDelegate>> val in rowDelegates)
          {
            foreach(ICPTracerDelegate deleg in val.Value)
              deleg.UpdateView();
          }
        }));
      }
    }

    public void Clear()
    {
      table.Rows.Clear();
      rowDelegates.Clear();
      rowIdInds.Clear();
      if (updateTimer != null)
      {
        updateTimer.Stop();
        updateTimer = null;
      }
      ignoreCellValEvents = false;
    }

    public void Trace(ulong id, System.Array tms, System.Array vals)
    {
      IList<ICPTracerDelegate> delegs = null;
      if (rowDelegates.TryGetValue(id, out delegs))
      {
          delegs.ElementAt(delegs.Count - 1).Trace(tms, vals);
      }
    }

    private ICPTracerDelegate AddTracer(ulong id, string varName)
    {
      ignoreCellValEvents = true;
      table.Rows.Add();
      DataGridViewRow row = table.Rows[table.Rows.Count - 1];
      row.Cells[EnabledCellInd].Value = true;
      row.Cells[NameCellInd].Value = varName;
      ICPTraceConsumer cons = new CPTableTraceConsumer(table, row);
      ICPTracerDelegate cpDelegate = new CPTracerDelegate(cons);
      IList<ICPTracerDelegate> delegs = null;
      if(!rowDelegates.TryGetValue(id, out delegs))
      {
        delegs = new List<ICPTracerDelegate>();
        rowDelegates.Add(id, delegs);
      }
      delegs.Add(cpDelegate);
      rowIdInds.Add(row.Index, new KeyValuePair<ulong, int>(id, delegs.Count - 1));
      if (updateTimer == null)
      {
        updateTimer = new Timer();
        updateTimer.Interval = 100;
        updateTimer.Tick += UpdateView;
        updateTimer.Start();
      }
      ignoreCellValEvents = false;

      return cpDelegate;
    }

    public ICPTracerDelegate CreateTracer(ulong id, string varName/*, Color color*/)
    {
      ICPTracerDelegate cpDelegate = null;
      if (table.InvokeRequired)
      {
        if (table.IsHandleCreated)
        {
          table.Invoke((MethodInvoker)(() =>
          {
            cpDelegate = AddTracer(id, varName/*, color*/);
          }));
        }
        else
          Console.WriteLine("");
      }
      else
        cpDelegate = AddTracer(id, varName/*, color*/);

      return cpDelegate;
    }

    private void OnCheckChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (ignoreCellValEvents)
        return;
      if (e.ColumnIndex == EnabledCellInd && e.RowIndex != -1)
      {
        DataGridViewRow row = table.Rows[e.RowIndex];
        KeyValuePair<ulong, int> idInst;
        if (rowIdInds.TryGetValue(e.RowIndex, out idInst))
        {
          bool value = (bool)row.Cells[0].Value;
          ulong id = idInst.Key;
          enableEvent.Fire(new EnableTraceEntEvArgs(id, idInst.Value, value));
          IList<ICPTracerDelegate> delegs = null;
          if (rowDelegates.TryGetValue(id, out delegs))
          {
            delegs.ElementAt(idInst.Value).SetProperty("enable", value);
            if(!value)
              row.Cells[ValueCellInd].Value = "--";
          }
        }
      }
    }

    private void OnCellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
    {
      if (e.ColumnIndex == EnabledCellInd && e.RowIndex != -1)
        this.table.EndEdit();
    }
  }


  public class EnableTraceEntEvArgs
  {
    public ulong id { get; }
    public int instNum { get; }
    public bool flag { get; }

    public EnableTraceEntEvArgs(ulong _id, int _instNum, bool _flag)
    {
      id = _id;
      instNum = _instNum;
      flag = _flag;
    }
  }

}

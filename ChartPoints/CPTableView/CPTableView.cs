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

namespace ChartPoints
{
  public partial class CPTableView : UserControl, ICPTracer
  {
    public ICPEvent<EnableTraceEntEvArgs> enableEvent { get; set; } = new CPEvent<EnableTraceEntEvArgs>();

    static public readonly int EnabledCellInd = 0;
    static public readonly int NameCellInd = 2;
    static public readonly int ValueCellInd = 3;

    private bool ignoreCellValEvents = false;
    private IDictionary<ulong, ICPTracerDelegate> rowDelegates = new SortedDictionary<ulong, ICPTracerDelegate>();
    private IDictionary<int, ulong> rowIdInds = new SortedDictionary<int, ulong>();
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
          foreach (KeyValuePair<ulong, ICPTracerDelegate> val in rowDelegates)
            val.Value.UpdateView();
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

    public void Trace(ulong id, double val)
    {
      ICPTracerDelegate deleg = null;
      if (rowDelegates.TryGetValue(id, out deleg))
        deleg.Trace(val);
    }

    //public void EnableItem(ulong id, bool flag)
    //{
    //  ;
    //}

    private ICPTracerDelegate AddTracer(ulong id, string varName)
    {
      ignoreCellValEvents = true;
      table.Rows.Add();
      DataGridViewRow row = table.Rows[table.Rows.Count - 1];
      row.Cells[EnabledCellInd].Value = true;
      row.Cells[NameCellInd].Value = varName;
      ICPTraceConsumer cons = new CPTableTraceConsumer(table, row);
      ICPTracerDelegate cpDelegate = new CPTracerDelegate(cons);
      rowDelegates.Add(id, cpDelegate);
      rowIdInds.Add(row.Index, id);
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
        ulong id = ulong.MaxValue;
        if (rowIdInds.TryGetValue(e.RowIndex, out id))
        {
          bool value = (bool)row.Cells[0].Value;
          enableEvent.Fire(new EnableTraceEntEvArgs(id, value));
          ICPTracerDelegate deleg = null;
          if (rowDelegates.TryGetValue(id, out deleg))
          {
            deleg.SetProperty("enable", value);
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
    public bool flag { get; }

    public EnableTraceEntEvArgs(ulong _id, bool _flag)
    {
      id = _id;
      flag = _flag;
    }
  }

}

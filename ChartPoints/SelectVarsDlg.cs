using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CP.Code;

namespace ChartPoints
{
  public partial class SelectVarsDlg : Form
  {
    private ISet<string> selected_vars;
    public SelectVarsDlg(ICheckCPPoint checkPnt)
    {
      InitializeComponent();
      foreach (ICheckElem classVar in checkPnt.elems)
        AddVariable(classVar);
    }

    public void AddVariable(ICheckElem classVar)
    {
      vars_dgv.Rows.Add();
      DataGridViewRow row = vars_dgv.Rows[vars_dgv.Rows.Count - 1];
      row.Tag = classVar;
      row.Cells[0].Value = classVar.exists;
      row.Cells[1].Value = classVar.var.name;
      row.Cells[2].Value = classVar.var.type;
    }

    private void ok_btn_Click(object sender, EventArgs e)
    {
      foreach (DataGridViewRow row in vars_dgv.Rows)
      {
        ICheckElem _elem = (ICheckElem) row.Tag;
        _elem.Toggle((bool)row.Cells[0].Value);
      }
      this.Close();
    }

    public ISet<string> GetSelectedVars()
    {
      return selected_vars;
    }

  }
}

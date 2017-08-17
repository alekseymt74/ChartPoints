﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartPoints
{
  public partial class SelectVarsDlg : Form
  {
    private List<string> selected_vars;
    public SelectVarsDlg(ref List<Tuple<string, string>> classVars)
    {
      InitializeComponent();
      foreach(var classVar in classVars)
        AddVariable(classVar.Item1, classVar.Item2);
    }

    public void AddVariable(string name, string type)
    {
      //DataGridViewRow row = new DataGridViewRow();
      vars_dgv.Rows.Add();//row);
      DataGridViewRow row = vars_dgv.Rows[vars_dgv.Rows.Count - 1];
      row.Cells[0].Value = false;
      row.Cells[1].Value = name;
      row.Cells[2].Value = type;
    }

    private void ok_btn_Click(object sender, EventArgs e)
    {
      selected_vars = new List<string>();
      foreach (DataGridViewRow row in vars_dgv.Rows)
      {
        if ((bool)row.Cells[0].Value == true)
          selected_vars.Add( (string)row.Cells[1].Value);
      }
      this.Close();
    }

    public List<string> GetSelectedVars()
    {
      return selected_vars;
    }
  }
}

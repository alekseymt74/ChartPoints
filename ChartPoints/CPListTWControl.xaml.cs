//------------------------------------------------------------------------------
// <copyright file="CPListTWControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ChartPoints
{
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Controls;

  /// <summary>
  /// Interaction logic for CPListTWControl.
  /// </summary>
  public partial class CPListTWControl : UserControl
  {
    private System.Windows.Forms.DataGridViewCheckBoxColumn chk_box_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn name_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn type_col;
    //private System.Windows.Forms.DataGridViewTextBoxColumn type_col1;
    /// <summary>
    /// Initializes a new instance of the <see cref="CPListTWControl"/> class.
    /// </summary>
    public CPListTWControl()
    {
      this.InitializeComponent();
      this.chk_box_col = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.name_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.type_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      //this.type_col1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.list)).BeginInit();
      //this.SuspendLayout();
      this.list.AllowUserToAddRows = false;
      this.list.AllowUserToDeleteRows = false;
      this.list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.list.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_box_col,
            this.name_col,
            this.type_col});
      this.list.Location = new System.Drawing.Point(0, 0);
      this.list.Name = "list";
      this.list.Size = new System.Drawing.Size(499, 302);
      this.list.TabIndex = 0;
      // 
      // chk_box_col
      // 
      this.chk_box_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.chk_box_col.HeaderText = "v";
      this.chk_box_col.Name = "chk_box_col";
      this.chk_box_col.Width = 19;
      // 
      // name_col
      // 
      this.name_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.name_col.HeaderText = "Name";
      this.name_col.Name = "name_col";
      this.name_col.ReadOnly = true;
      this.name_col.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      //this.name_col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // type_col
      // 
      this.type_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.type_col.HeaderText = "Type";
      this.type_col.Name = "type_col";
      this.type_col.ReadOnly = true;
      this.type_col.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      //// 
      //// type_col1
      //// 
      //this.type_col1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      //this.type_col1.HeaderText = "Type";
      //this.type_col1.Name = "type_col1";
      //this.type_col1.ReadOnly = true;
      ((System.ComponentModel.ISupportInitialize)(this.list)).EndInit();
      //this.ResumeLayout(false);
    }

  }
}
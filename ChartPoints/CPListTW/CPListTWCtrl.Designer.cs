namespace ChartPoints
{
  partial class CPListTWCtrl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.list = new System.Windows.Forms.DataGridView();
      this.chk_col = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.name_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.type_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.list)).BeginInit();
      this.SuspendLayout();
      // 
      // list
      // 
      this.list.AllowUserToAddRows = false;
      this.list.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_col,
            this.name_col,
            this.type_col});
      this.list.Dock = System.Windows.Forms.DockStyle.Fill;
      this.list.Location = new System.Drawing.Point(0, 0);
      this.list.MultiSelect = false;
      this.list.Name = "list";
      this.list.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.list.Size = new System.Drawing.Size(150, 150);
      this.list.TabIndex = 0;
      // 
      // chk_col
      // 
      this.chk_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.chk_col.HeaderText = "v";
      this.chk_col.Name = "chk_col";
      this.chk_col.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.chk_col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.chk_col.Width = 38;
      // 
      // name_col
      // 
      this.name_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.name_col.HeaderText = "Name";
      this.name_col.Name = "name_col";
      this.name_col.ReadOnly = true;
      // 
      // type_col
      // 
      this.type_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.type_col.HeaderText = "Type";
      this.type_col.Name = "type_col";
      this.type_col.ReadOnly = true;
      // 
      // CPListTWCtrl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.list);
      this.Name = "CPListTWCtrl";
      ((System.ComponentModel.ISupportInitialize)(this.list)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataGridView list;
    private System.Windows.Forms.DataGridViewCheckBoxColumn chk_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn name_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn type_col;
  }
}

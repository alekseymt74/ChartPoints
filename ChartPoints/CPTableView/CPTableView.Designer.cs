namespace ChartPoints
{
  partial class CPTableView
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
      this.table = new System.Windows.Forms.DataGridView();
      this.enable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.color = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.val = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
      this.SuspendLayout();
      // 
      // table
      // 
      this.table.AllowUserToAddRows = false;
      this.table.AllowUserToDeleteRows = false;
      this.table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.enable,
            this.color,
            this.name,
            this.val});
      this.table.Dock = System.Windows.Forms.DockStyle.Fill;
      this.table.Location = new System.Drawing.Point(0, 0);
      this.table.Name = "table";
      this.table.Size = new System.Drawing.Size(150, 150);
      this.table.TabIndex = 2;
      this.table.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.OnCellMouseUp);
      this.table.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCheckChanged);
      // 
      // enable
      // 
      this.enable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.enable.HeaderText = "v";
      this.enable.Name = "enable";
      this.enable.Width = 19;
      // 
      // color
      // 
      this.color.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.color.HeaderText = "color";
      this.color.Name = "color";
      this.color.ReadOnly = true;
      this.color.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.color.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.color.Width = 36;
      // 
      // name
      // 
      this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.name.HeaderText = "Name";
      this.name.Name = "name";
      this.name.ReadOnly = true;
      // 
      // val
      // 
      this.val.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.val.HeaderText = "Value";
      this.val.Name = "val";
      this.val.ReadOnly = true;
      // 
      // CPTableView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.Controls.Add(this.table);
      this.Name = "CPTableView";
      ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.DataGridView table;
    private System.Windows.Forms.DataGridViewCheckBoxColumn enable;
    private System.Windows.Forms.DataGridViewTextBoxColumn color;
    private System.Windows.Forms.DataGridViewTextBoxColumn name;
    private System.Windows.Forms.DataGridViewTextBoxColumn val;
  }
}

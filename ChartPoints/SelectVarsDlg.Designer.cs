namespace ChartPoints
{
  partial class SelectVarsDlg
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.type_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.name_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.chk_box_col = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.vars_dgv = new System.Windows.Forms.DataGridView();
      this.ok_btn = new System.Windows.Forms.Button();
      this.cancel_btn = new System.Windows.Forms.Button();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.vars_dgv)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // type_col
      // 
      this.type_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.type_col.HeaderText = "Type";
      this.type_col.Name = "type_col";
      this.type_col.ReadOnly = true;
      // 
      // name_col
      // 
      this.name_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.name_col.HeaderText = "Name";
      this.name_col.Name = "name_col";
      this.name_col.ReadOnly = true;
      this.name_col.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.name_col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // chk_box_col
      // 
      this.chk_box_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.chk_box_col.HeaderText = "v";
      this.chk_box_col.Name = "chk_box_col";
      this.chk_box_col.Width = 19;
      // 
      // vars_dgv
      // 
      this.vars_dgv.AllowUserToAddRows = false;
      this.vars_dgv.AllowUserToDeleteRows = false;
      this.vars_dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.vars_dgv.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
      this.vars_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.vars_dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_box_col,
            this.name_col,
            this.type_col});
      this.tableLayoutPanel1.SetColumnSpan(this.vars_dgv, 2);
      this.vars_dgv.Dock = System.Windows.Forms.DockStyle.Fill;
      this.vars_dgv.Location = new System.Drawing.Point(3, 3);
      this.vars_dgv.Name = "vars_dgv";
      this.vars_dgv.Size = new System.Drawing.Size(309, 290);
      this.vars_dgv.TabIndex = 0;
      // 
      // ok_btn
      // 
      this.ok_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ok_btn.Location = new System.Drawing.Point(79, 299);
      this.ok_btn.Name = "ok_btn";
      this.ok_btn.Size = new System.Drawing.Size(75, 23);
      this.ok_btn.TabIndex = 1;
      this.ok_btn.Text = "OK";
      this.ok_btn.UseVisualStyleBackColor = true;
      this.ok_btn.Click += new System.EventHandler(this.ok_btn_Click);
      // 
      // cancel_btn
      // 
      this.cancel_btn.Location = new System.Drawing.Point(160, 299);
      this.cancel_btn.Name = "cancel_btn";
      this.cancel_btn.Size = new System.Drawing.Size(75, 23);
      this.cancel_btn.TabIndex = 2;
      this.cancel_btn.Text = "Cancel";
      this.cancel_btn.UseVisualStyleBackColor = true;
      this.cancel_btn.Click += new System.EventHandler(this.cancel_btn_click);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.cancel_btn, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.vars_dgv, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.ok_btn, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(315, 325);
      this.tableLayoutPanel1.TabIndex = 3;
      // 
      // SelectVarsDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(315, 325);
      this.Controls.Add(this.tableLayoutPanel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectVarsDlg";
      this.Text = "Select class variables";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.vars_dgv)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataGridViewTextBoxColumn type_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn name_col;
    private System.Windows.Forms.DataGridViewCheckBoxColumn chk_box_col;
    private System.Windows.Forms.DataGridView vars_dgv;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Button cancel_btn;
    private System.Windows.Forms.Button ok_btn;
  }
}
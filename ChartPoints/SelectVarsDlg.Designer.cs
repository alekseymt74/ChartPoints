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
      this.vars_dgv = new System.Windows.Forms.DataGridView();
      this.chk_box_col = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.name_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.type_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ok_btn = new System.Windows.Forms.Button();
      this.cancel_btn = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.vars_dgv)).BeginInit();
      this.SuspendLayout();
      // 
      // vars_dgv
      // 
      this.vars_dgv.AllowUserToAddRows = false;
      this.vars_dgv.AllowUserToDeleteRows = false;
      this.vars_dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.vars_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.vars_dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_box_col,
            this.name_col,
            this.type_col});
      this.vars_dgv.Location = new System.Drawing.Point(0, 0);
      this.vars_dgv.Name = "vars_dgv";
      this.vars_dgv.Size = new System.Drawing.Size(499, 302);
      this.vars_dgv.TabIndex = 0;
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
      this.name_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.name_col.HeaderText = "Name";
      this.name_col.Name = "name_col";
      this.name_col.ReadOnly = true;
      this.name_col.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.name_col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // type_col
      // 
      this.type_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.type_col.HeaderText = "Type";
      this.type_col.Name = "type_col";
      this.type_col.ReadOnly = true;
      // 
      // ok_btn
      // 
      this.ok_btn.Location = new System.Drawing.Point(144, 308);
      this.ok_btn.Name = "ok_btn";
      this.ok_btn.Size = new System.Drawing.Size(75, 23);
      this.ok_btn.TabIndex = 1;
      this.ok_btn.Text = "OK";
      this.ok_btn.UseVisualStyleBackColor = true;
      this.ok_btn.Click += new System.EventHandler(this.ok_btn_Click);
      // 
      // cancel_btn
      // 
      this.cancel_btn.Location = new System.Drawing.Point(242, 308);
      this.cancel_btn.Name = "cancel_btn";
      this.cancel_btn.Size = new System.Drawing.Size(75, 23);
      this.cancel_btn.TabIndex = 2;
      this.cancel_btn.Text = "Cancel";
      this.cancel_btn.UseVisualStyleBackColor = true;
      this.cancel_btn.Click += new System.EventHandler(this.cancel_btn_click);
      // 
      // SelectVarsDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(523, 344);
      this.Controls.Add(this.cancel_btn);
      this.Controls.Add(this.ok_btn);
      this.Controls.Add(this.vars_dgv);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectVarsDlg";
      this.Text = "SelectVarsDlg";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.vars_dgv)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataGridView vars_dgv;
    private System.Windows.Forms.Button ok_btn;
    private System.Windows.Forms.Button cancel_btn;
    private System.Windows.Forms.DataGridViewCheckBoxColumn chk_box_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn name_col;
    private System.Windows.Forms.DataGridViewTextBoxColumn type_col;
  }
}
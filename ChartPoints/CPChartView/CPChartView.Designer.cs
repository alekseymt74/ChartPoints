namespace ChartPoints
{
  partial class CPChartView
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
      System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
      System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
      this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.fitToViewBtn = new System.Windows.Forms.ToolStripButton();
      this.spyBtn = new System.Windows.Forms.ToolStripButton();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
      this.toolStrip1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // chart
      // 
      chartArea1.Area3DStyle.Inclination = 15;
      chartArea1.Area3DStyle.IsClustered = true;
      chartArea1.Area3DStyle.IsRightAngleAxes = false;
      chartArea1.Area3DStyle.Perspective = 10;
      chartArea1.Area3DStyle.Rotation = 10;
      chartArea1.Area3DStyle.WallWidth = 0;
      chartArea1.AxisX.LabelStyle.Format = "H:m:s:ff";
      chartArea1.AxisX.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      chartArea1.AxisY.IsLabelAutoFit = false;
      chartArea1.AxisY.LabelStyle.Format = "#.##";
      chartArea1.AxisY.LabelStyle.IntervalOffset = 0D;
      chartArea1.AxisY.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      chartArea1.CursorX.Interval = 1E-08D;
      chartArea1.CursorX.IsUserEnabled = true;
      chartArea1.CursorX.IsUserSelectionEnabled = true;
      chartArea1.CursorX.LineColor = System.Drawing.Color.ForestGreen;
      chartArea1.CursorX.SelectionColor = System.Drawing.Color.GreenYellow;
      chartArea1.CursorY.Interval = 1E-08D;
      chartArea1.CursorY.IsUserEnabled = true;
      chartArea1.CursorY.IsUserSelectionEnabled = true;
      chartArea1.CursorY.LineColor = System.Drawing.Color.ForestGreen;
      chartArea1.CursorY.SelectionColor = System.Drawing.Color.GreenYellow;
      chartArea1.Name = "ChartArea1";
      this.chart.ChartAreas.Add(chartArea1);
      this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
      legend1.Name = "Legend1";
      this.chart.Legends.Add(legend1);
      this.chart.Location = new System.Drawing.Point(3, 28);
      this.chart.Name = "chart";
      this.chart.Size = new System.Drawing.Size(490, 333);
      this.chart.TabIndex = 0;
      this.chart.Text = "chart";
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fitToViewBtn,
            this.spyBtn});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(496, 25);
      this.toolStrip1.TabIndex = 1;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // fitToViewBtn
      // 
      this.fitToViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.fitToViewBtn.Image = global::ChartPoints.Resource.FitView;
      this.fitToViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.fitToViewBtn.Name = "fitToViewBtn";
      this.fitToViewBtn.Size = new System.Drawing.Size(23, 22);
      this.fitToViewBtn.Text = "Fit to view";
      this.fitToViewBtn.Click += new System.EventHandler(this.FitToView);
      // 
      // spyBtn
      // 
      this.spyBtn.CheckOnClick = true;
      this.spyBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.spyBtn.Image = global::ChartPoints.Resource.Spy;
      this.spyBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.spyBtn.Name = "spyBtn";
      this.spyBtn.Size = new System.Drawing.Size(23, 22);
      this.spyBtn.Text = "toolStripButton1";
      this.spyBtn.ToolTipText = "Spy";
      this.spyBtn.Click += new System.EventHandler(this.SetSpyMode);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.chart, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.99689F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(496, 364);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // CPChartView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "CPChartView";
      this.Size = new System.Drawing.Size(496, 364);
      ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.DataVisualization.Charting.Chart chart;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton fitToViewBtn;
    private System.Windows.Forms.ToolStripButton spyBtn;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}

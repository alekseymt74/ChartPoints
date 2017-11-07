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
      //System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
      System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.yAxisToolStrip = new System.Windows.Forms.ToolStrip();
      this.yZoomInBtn = new System.Windows.Forms.ToolStripButton();
      this.yZoomOutBtn = new System.Windows.Forms.ToolStripButton();
      this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
      this.xAxisToolStrip = new System.Windows.Forms.ToolStrip();
      this.xZoomInBtn = new System.Windows.Forms.ToolStripButton();
      this.xZoomOutBtn = new System.Windows.Forms.ToolStripButton();
      this.mainToolStrip = new System.Windows.Forms.ToolStrip();
      this.fitToViewBtn = new System.Windows.Forms.ToolStripButton();
      this.xyZoomInBtn = new System.Windows.Forms.ToolStripButton();
      this.xyZoomOutBtn = new System.Windows.Forms.ToolStripButton();
      this.spyBtn = new System.Windows.Forms.ToolStripButton();
      this.tableLayoutPanel1.SuspendLayout();
      this.yAxisToolStrip.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
      this.xAxisToolStrip.SuspendLayout();
      this.mainToolStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.yAxisToolStrip, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.chart, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.xAxisToolStrip, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.mainToolStrip, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(233, 364);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // yAxisToolStrip
      // 
      this.yAxisToolStrip.Dock = System.Windows.Forms.DockStyle.Left;
      this.yAxisToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.yZoomInBtn,
            this.yZoomOutBtn});
      this.yAxisToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.yAxisToolStrip.Location = new System.Drawing.Point(0, 0);
      this.yAxisToolStrip.Name = "yAxisToolStrip";
      this.yAxisToolStrip.Size = new System.Drawing.Size(58, 25);
      this.yAxisToolStrip.TabIndex = 2;
      this.yAxisToolStrip.Text = "toolStrip2";
      // 
      // yZoomInBtn
      // 
      this.yZoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.yZoomInBtn.Image = global::ChartPoints.Resource.YZoomIn;
      this.yZoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.yZoomInBtn.Name = "yZoomInBtn";
      this.yZoomInBtn.Size = new System.Drawing.Size(23, 22);
      this.yZoomInBtn.Text = "toolStripButton1";
      this.yZoomInBtn.ToolTipText = "Y Zoom In";
      this.yZoomInBtn.Click += new System.EventHandler(this.OnYZoomIn);
      // 
      // yZoomOutBtn
      // 
      this.yZoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.yZoomOutBtn.Image = global::ChartPoints.Resource.YZoomOut;
      this.yZoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.yZoomOutBtn.Name = "yZoomOutBtn";
      this.yZoomOutBtn.Size = new System.Drawing.Size(23, 22);
      this.yZoomOutBtn.Text = "toolStripButton2";
      this.yZoomOutBtn.ToolTipText = "Y Zoom Out";
      this.yZoomOutBtn.Click += new System.EventHandler(this.OnYZoomOut);
      // 
      // chart
      // 
      //chartArea1.Area3DStyle.Inclination = 15;
      //chartArea1.Area3DStyle.IsClustered = true;
      //chartArea1.Area3DStyle.IsRightAngleAxes = false;
      //chartArea1.Area3DStyle.Perspective = 10;
      //chartArea1.Area3DStyle.Rotation = 10;
      //chartArea1.Area3DStyle.WallWidth = 0;
      //chartArea1.AxisX.IsLabelAutoFit = false;
      //chartArea1.AxisX.LabelStyle.Format = "H:m:s:ff";
      //chartArea1.AxisX.LabelStyle.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
      //chartArea1.AxisX.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      //chartArea1.AxisY.IsLabelAutoFit = false;
      //chartArea1.AxisY.LabelStyle.Format = "#.##";
      //chartArea1.AxisY.LabelStyle.IntervalOffset = 0D;
      //chartArea1.AxisY.ScrollBar.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      //chartArea1.CursorX.Interval = 1E-08D;
      //chartArea1.CursorX.IsUserEnabled = true;
      //chartArea1.CursorX.IsUserSelectionEnabled = true;
      //chartArea1.CursorX.LineColor = System.Drawing.Color.ForestGreen;
      //chartArea1.CursorX.SelectionColor = System.Drawing.Color.GreenYellow;
      //chartArea1.CursorY.Interval = 1E-08D;
      //chartArea1.CursorY.IsUserEnabled = true;
      //chartArea1.CursorY.IsUserSelectionEnabled = true;
      //chartArea1.CursorY.LineColor = System.Drawing.Color.ForestGreen;
      //chartArea1.CursorY.SelectionColor = System.Drawing.Color.GreenYellow;
      //chartArea1.Name = "ChartArea1";
      //this.chart.ChartAreas.Add(chartArea1);
      this.tableLayoutPanel1.SetColumnSpan(this.chart, 3);
      this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
      legend1.Name = "Legend1";
      this.chart.Legends.Add(legend1);
      this.chart.Location = new System.Drawing.Point(3, 28);
      this.chart.Name = "chart";
      this.chart.Size = new System.Drawing.Size(227, 333);
      this.chart.TabIndex = 0;
      this.chart.Text = "chart";
      // 
      // xAxisToolStrip
      // 
      this.xAxisToolStrip.Dock = System.Windows.Forms.DockStyle.Right;
      this.xAxisToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xZoomInBtn,
            this.xZoomOutBtn});
      this.xAxisToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.xAxisToolStrip.Location = new System.Drawing.Point(175, 0);
      this.xAxisToolStrip.Name = "xAxisToolStrip";
      this.xAxisToolStrip.Size = new System.Drawing.Size(58, 25);
      this.xAxisToolStrip.TabIndex = 3;
      this.xAxisToolStrip.Text = "toolStrip3";
      // 
      // xZoomInBtn
      // 
      this.xZoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.xZoomInBtn.Image = global::ChartPoints.Resource.XZoomIn;
      this.xZoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.xZoomInBtn.Name = "xZoomInBtn";
      this.xZoomInBtn.Size = new System.Drawing.Size(23, 22);
      this.xZoomInBtn.Text = "toolStripButton3";
      this.xZoomInBtn.ToolTipText = "X Zoom In";
      this.xZoomInBtn.Click += new System.EventHandler(this.OnXZoomIn);
      // 
      // xZoomOutBtn
      // 
      this.xZoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.xZoomOutBtn.Image = global::ChartPoints.Resource.XZoomOut;
      this.xZoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.xZoomOutBtn.Name = "xZoomOutBtn";
      this.xZoomOutBtn.Size = new System.Drawing.Size(23, 22);
      this.xZoomOutBtn.Text = "toolStripButton4";
      this.xZoomOutBtn.ToolTipText = "X Zoom Out";
      this.xZoomOutBtn.Click += new System.EventHandler(this.OnXZoomOut);
      // 
      // mainToolStrip
      // 
      this.mainToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.mainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
      this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fitToViewBtn,
            this.xyZoomInBtn,
            this.xyZoomOutBtn,
            this.spyBtn});
      this.mainToolStrip.Location = new System.Drawing.Point(64, 0);
      this.mainToolStrip.Name = "mainToolStrip";
      this.mainToolStrip.Size = new System.Drawing.Size(104, 25);
      this.mainToolStrip.TabIndex = 1;
      this.mainToolStrip.Text = "toolStrip1";
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
      // xyZoomInBtn
      // 
      this.xyZoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.xyZoomInBtn.Image = global::ChartPoints.Resource.XYZoomIn;
      this.xyZoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.xyZoomInBtn.Name = "xyZoomInBtn";
      this.xyZoomInBtn.Size = new System.Drawing.Size(23, 22);
      this.xyZoomInBtn.Text = "toolStripButton1";
      this.xyZoomInBtn.ToolTipText = "XY Zoom In";
      this.xyZoomInBtn.Click += new System.EventHandler(this.OnXYZoomIn);
      // 
      // xyZoomOutBtn
      // 
      this.xyZoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.xyZoomOutBtn.Image = global::ChartPoints.Resource.XYZoomOut;
      this.xyZoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.xyZoomOutBtn.Name = "xyZoomOutBtn";
      this.xyZoomOutBtn.Size = new System.Drawing.Size(23, 22);
      this.xyZoomOutBtn.Text = "toolStripButton2";
      this.xyZoomOutBtn.ToolTipText = "XY Zoom Out";
      this.xyZoomOutBtn.Click += new System.EventHandler(this.OnXYZoomOut);
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
      // CPChartView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "CPChartView";
      this.Size = new System.Drawing.Size(233, 364);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.yAxisToolStrip.ResumeLayout(false);
      this.yAxisToolStrip.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
      this.xAxisToolStrip.ResumeLayout(false);
      this.xAxisToolStrip.PerformLayout();
      this.mainToolStrip.ResumeLayout(false);
      this.mainToolStrip.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.DataVisualization.Charting.Chart chart;
    private System.Windows.Forms.ToolStrip yAxisToolStrip;
    private System.Windows.Forms.ToolStripButton yZoomInBtn;
    private System.Windows.Forms.ToolStripButton yZoomOutBtn;
    private System.Windows.Forms.ToolStrip mainToolStrip;
    private System.Windows.Forms.ToolStripButton fitToViewBtn;
    private System.Windows.Forms.ToolStripButton spyBtn;
    private System.Windows.Forms.ToolStrip xAxisToolStrip;
    private System.Windows.Forms.ToolStripButton xZoomInBtn;
    private System.Windows.Forms.ToolStripButton xZoomOutBtn;
    private System.Windows.Forms.ToolStripButton xyZoomInBtn;
    private System.Windows.Forms.ToolStripButton xyZoomOutBtn;
  }
}

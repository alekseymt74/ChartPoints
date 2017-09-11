//------------------------------------------------------------------------------
// <copyright file="ChartPointsViewTWControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartPoints
{
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Controls;

  /// <summary>
  /// Interaction logic for ChartPointsViewTWControl.
  /// </summary>
  public partial class ChartPointsViewTWControl : UserControl
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPointsViewTWControl"/> class.
    /// </summary>
    public ChartPointsViewTWControl()
    {
      this.InitializeComponent();
      // Все графики находятся в пределах области построения ChartArea, создадим ее
      chart.ChartAreas.Add(new ChartArea("Default"));

      //// Добавим линию, и назначим ее в ранее созданную область "Default"
      //chart.Series.Add(new Series("Series1"));
      //chart.Series["Series1"].ChartArea = "Default";
      //chart.Series["Series1"].ChartType = SeriesChartType.Line;

      //// добавим данные линии
      //string[] axisXData = new string[] { "a", "b", "c" };
      //double[] axisYData = new double[] { 0.1, 1.5, 1.9 };
      //chart.Series["Series1"].Points.DataBindXY(axisXData, axisYData);
    }

    public void Clear()
    {
      chart.Invoke((MethodInvoker) (() =>
      {
        chart.Series.Clear();
      }));
    }

    public void UpdateView()
    {
      chart.Invoke((MethodInvoker)(() =>
      {
        chart.DataBind();
      }));
    }

    public ICPTracerDelegate CreateTracer(string varName)
    {
      Series ser = null;
      chart.Invoke((MethodInvoker)(() =>
      {
        ser = chart.Series.Add(varName);
        ser.ChartType = SeriesChartType.Line;//StepLine;
        ser.LegendText = varName;
      }));
      ICPTraceConsumer cons = new CPTraceConsumer(chart, ser/*chart.Series[0]*/);//, varName);
      ICPTracerDelegate cpDelegate = new CPTracerDelegate(cons);

      return cpDelegate;
    }
    ///// <summary>
    ///// Handles click on the button by displaying a message box.
    ///// </summary>
    ///// <param name="sender">The event sender.</param>
    ///// <param name="e">The event args.</param>
    //[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
    //    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
    //    private void button1_Click(object sender, RoutedEventArgs e)
    //    {
    //        MessageBox.Show(
    //            string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
    //            "ChartPointsViewTW");
    //    }
  }
}
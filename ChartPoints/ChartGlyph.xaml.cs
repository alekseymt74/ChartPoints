using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartPoints
{
  /// <summary>
  /// Interaction logic for ChartGlyph.xaml
  /// </summary>
  public partial class ChartGlyph : UserControl
  {
    ILineChartPoints linePnt;
    public ChartGlyph(ILineChartPoints _linePnt)
    {
      InitializeComponent();
      object obj = this.Content;
      linePnt = _linePnt;
    }
    private void DrawStatus(DrawingContext dc, double x, double y, double w, double h, Color color)
    {
      const double pen_width = 1;
      Pen pen = new Pen(new SolidColorBrush(color), pen_width);
      SolidColorBrush br = new SolidColorBrush(color);
      br.Opacity = 0.5;
      Rect rc = new Rect(x, y, w, h);
      dc.DrawRoundedRectangle(br, pen, rc, 0.9, 0.9);
    }
    protected override void OnRender(DrawingContext dc)
    {
      uint mask = 0;
      foreach (IChartPoint cp in linePnt.chartPoints)
      {
        switch (cp.data.status)
        {
          case EChartPointStatus.SwitchedOn:
            mask |= 1;
            break;
          case EChartPointStatus.SwitchedOff:
            mask |= 1 << 1;
            break;
          case EChartPointStatus.NotAvailable:
            mask |= 1 << 2;
            break;
        }
      }
      uint count = 0;
      for (uint i = 0; i < 3; ++i)
      {
        if ((mask & (1 << (int)i)) != 0)
          ++count;
      }
      if (count > 0)
      {
        Color[] statusColors = { Brushes.Green.Color, Brushes.Yellow.Color, Brushes.Red.Color };
        double w = grdMain.ActualWidth;
        double h = grdMain.ActualHeight / count - 1;
        double y = 0;
        for (uint i = 0; i < 3; ++i)
        {
          if ((mask & (1 << (int)i)) != 0)
          {
            DrawStatus(dc, 0, y, w, h, statusColors[i]);
            y += (h + 1);
          }
        }
      }
      //DrawStatus(dc, 0, 0, grdMain.ActualWidth, grdMain.ActualHeight / 3 - 1, Brushes.Green.Color);
      //DrawStatus(dc, 0, grdMain.ActualHeight / 3, grdMain.ActualWidth, grdMain.ActualHeight / 3 - 1, Brushes.Orange.Color);
      //DrawStatus(dc, 0, 2 * grdMain.ActualHeight / 3, grdMain.ActualWidth, grdMain.ActualHeight / 3 - 1, Brushes.Red.Color);
      //EChartPointStatus st = EChartPointStatus.NotAvailable;
      //bool differ = false;
      //foreach (IChartPoint cp in linePnt.chartPoints)
      //{
      //  if( st != cp.data.status)
      //  {
      //    differ = true;
      //    break;
      //  }
      //}
      //Color penColor;
      //Color brushColor;
      //if(differ)
      //{
      //  penColor = Brushes.Yellow.Color;
      //  brushColor = Brushes.LightYellow.Color;
      //}
      //else
      //{
      //  penColor = Brushes.Green.Color;
      //  brushColor = Brushes.LightGreen.Color;
      //}
      //const double pen_width = 1;
      //Pen pen = new Pen(new SolidColorBrush(penColor), pen_width);// Brushes.Green, pen_width);
      //SolidColorBrush br = new SolidColorBrush(brushColor);// Brushes.LightGreen.Color );
      //br.Opacity = 0.5;
      //Rect rc = new Rect(0, 0, grdMain.ActualWidth, grdMain.ActualHeight);
      //dc.DrawRoundedRectangle(br, pen, rc, 0.9, 0.9);
    }
  }
}

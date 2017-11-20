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
    private uint lineMask;
    public ChartGlyph(uint _lineMask)
    {
      InitializeComponent();
      object obj = this.Content;
      lineMask = _lineMask;
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
      uint count = 0;
      for (uint i = 0; i < 3; ++i)
      {
        if ((lineMask & (1 << (int)i)) != 0)
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
          if ((lineMask & (1 << (int)i)) != 0)
          {
            DrawStatus(dc, 0, y, w, h, statusColors[i]);
            y += (h + 1);
          }
        }
      }
    }
  }
}

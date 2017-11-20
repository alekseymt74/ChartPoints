using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ChartPoints
{
  internal class ChartPointsGlyphFactory : IGlyphFactory
  {
    const double m_glyphSize = 16.0;

    public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
    {
      // Ensure we can draw a glyph for this marker.
      if (tag == null || !(tag is ChartPointTag))
      {
        return null;
      }
      ChartPointTag theTag = tag as ChartPointTag;

      return new ChartGlyph(theTag.lineMask);
    }
  }

  [Export(typeof(IGlyphFactoryProvider))]
  [Name("ChartGlyph")]
  [Order(After = "VsTextMarker")]
  [ContentType("C/C++")]
  [TagType(typeof(ChartPointTag))]
  internal sealed class ChartPointFactoryProvider : IGlyphFactoryProvider
  {
    public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
    {
      return new ChartPointsGlyphFactory();
    }
  }
}

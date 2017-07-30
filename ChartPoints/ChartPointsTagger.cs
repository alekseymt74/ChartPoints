﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ChartPoints
{
  internal class ChartPointTag : IGlyphTag
  {

  }
  public class ChartPointsTagger : ITagger<ChartPointTag>
  {
    private ITextView _view;
    public ITextBuffer _buffer;
    internal ChartPointsTagger(ITextView view, ITextBuffer buffer)
    {
      _view = view;
      _buffer = buffer;
      _view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(OnViewLayoutChanged);
    }

    void OnViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
      _buffer = ((ITextView)sender).TextBuffer;
    }

    internal void RaiseTagsChangedEvent(IChartPoint chartPnt)
    {
      var tempEvent = TagsChanged;
      if (tempEvent != null)
      {
        //_view.TextViewLines Span.FromBounds
        ITextSnapshotLine line = _buffer.CurrentSnapshot.Lines.ElementAt(chartPnt.pnt.Line - 1);
        tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(line.Start, 1)));
      }
    }

    IEnumerable<ITagSpan<ChartPointTag>> ITagger<ChartPointTag>.GetTags(NormalizedSnapshotSpanCollection spans)
    {
      IDictionary<int, IChartPoint> fileChartPoints;
      Globals.processor.chartPoints.TryGetValue(Globals.dte.ActiveDocument.FullName, out fileChartPoints);
      if (fileChartPoints != null)
      {
        foreach (SnapshotSpan span in spans)
        {
          ITextSnapshotLine line = span.Start.GetContainingLine();
          int firstLineNum = line.LineNumber;
          int lastLineNum = span.End.GetContainingLine().LineNumber;
          for (int i = firstLineNum; i <= lastLineNum; ++i)
          {
            foreach (KeyValuePair<int, IChartPoint> pair in fileChartPoints)
            {
              // TextPoint::Line is 1-based 
              // Text Snapshots - 0-based
              if (pair.Key - 1 == firstLineNum)
                yield return new TagSpan<ChartPointTag>(new SnapshotSpan(line.Start, 0), new ChartPointTag());
            }
          }
        }
      }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
  }

  public class ChartPointTagUpdater : IChartPointTagUpdater
  {
    private SortedDictionary<string, ChartPointsTagger> taggers;

    public void AddTagger(ChartPointsTagger tagger)
    {
      ITextDocument textDoc;
      var rc = tagger._buffer.Properties.TryGetProperty<ITextDocument>(
        typeof(ITextDocument), out textDoc);
      if (rc == true)
        taggers[textDoc.FilePath] = tagger;
    }
    public ChartPointTagUpdater()
    {
      taggers = new SortedDictionary<string, ChartPointsTagger>();
      //var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
      //var textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
      //IVsTextView activeView = null;
      //ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out activeView));
      //var editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
      //IWpfTextView wpfTextView = editorAdapetr.GetWpfTextView(activeView);
    }
    public void RaiseChangeTagEvent(IChartPoint chartPnt)
    {
      ChartPointsTagger tagger;
      taggers.TryGetValue(chartPnt.fileName, out tagger);
      tagger.RaiseTagsChangedEvent(chartPnt);
    }
  }

  [Export(typeof(IViewTaggerProvider))]
  [ContentType("C/C++")]
  [TagType(typeof(ChartPointTag))]
  class ChartPointsTaggerProvider : IViewTaggerProvider
  {
    public ChartPointsTaggerProvider()
    {
      Globals.taggerUpdater = new ChartPointTagUpdater();
    }

    public ITagger<T> CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
    {
      if (buffer == null)
      {
        throw new ArgumentNullException("buffer");
      }
      if (view == null)
        return null;
      //provide highlighting only on the top-level buffer
      if (view.TextBuffer != buffer)
        return null;

      ChartPointsTagger tagger = new ChartPointsTagger(view, buffer);
      Globals.taggerUpdater.AddTagger(tagger);
      return tagger as ITagger<T>;
    }

  }
}

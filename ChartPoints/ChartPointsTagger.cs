using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
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
  public class ChartPointsTagger : ITagger<ChartPointTag>, IChartPointsTagger
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
      //_buffer = ((ITextView)sender).TextBuffer;
    }

    public void RaiseTagsChangedEvent(IFileChartPoints filePoints)
    {
      var tempEvent = TagsChanged;
      if (tempEvent != null)
      {
        //_view.TextViewLines Span.FromBounds
        foreach (var linePnt in filePoints.linePoints)
        {
          ITextSnapshotLine line = _buffer.CurrentSnapshot.Lines.ElementAt(linePnt.data.pos.lineNum - 1);
          tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(line.Start, 1)));
        }
      }
    }

    public void RaiseTagsChangedEvent(ILineChartPoints lPnts)
    {
      var tempEvent = TagsChanged;
      if (tempEvent != null)
      {
        //_view.TextViewLines Span.FromBounds
        ITextSnapshotLine line = _buffer.CurrentSnapshot.Lines.ElementAt(lPnts.data.pos.lineNum - 1);
        tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(line.Start, 1)));
      }
    }
    public bool GetFileName(out string fn)
    {
      ITextDocument textDoc;
      var rc = _buffer.Properties.TryGetProperty<ITextDocument>( typeof(ITextDocument), out textDoc);
      if (rc)
      {
        fn = System.IO.Path.GetFullPath(textDoc.FilePath).ToLower();
        return true;
      }
      else
        fn = null;

      return rc;
    }


    IEnumerable<ITagSpan<ChartPointTag>> ITagger<ChartPointTag>.GetTags(NormalizedSnapshotSpanCollection spans)
    {
      //IDictionary<int, IChartPoint> fileChartPoints;
      ITextDocument thisTextDoc;
      var rc = this._buffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out thisTextDoc);
      string fileName = Path.GetFileName(thisTextDoc.FilePath);
      EnvDTE.Document dteDoc = Globals.dte.Documents.Item(fileName);
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(dteDoc.ProjectItem.ContainingProject.Name);
      if (pPnts != null)
      {
        IFileChartPoints fPnts = pPnts.GetFileChartPoints(fileName);
        //Globals.processor.data.chartPoints.TryGetValue(fileName/*Globals.dte.ActiveDocument.FullName*/, out fileChartPoints);
        if (fPnts/*fileChartPoints*/ != null)
        {
          foreach (SnapshotSpan span in spans)
          {
            ITextSnapshotLine line = span.Start.GetContainingLine();
            int firstLineNum = line.LineNumber;
            int lastLineNum = span.End.GetContainingLine().LineNumber;
            for (int i = firstLineNum; i <= lastLineNum; ++i)
            {
              //foreach (KeyValuePair<int, IChartPoint> pair in fileChartPoints)
              foreach (var linePnt in fPnts.linePoints)
              {
                //pair.Value.f();
                // TextPoint::Line is 1-based 
                // Text Snapshots - 0-based
                //if (pair.Key - 1 == firstLineNum)
                if (linePnt.data.pos.lineNum - 1 == firstLineNum)
                  yield return new TagSpan<ChartPointTag>(new SnapshotSpan(line.Start, 0), new ChartPointTag());
              }
            }
          }
        }
      }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
  }

  public class ChartPointTagUpdater : IChartPointTagUpdater
  {
    private SortedDictionary<string, IChartPointsTagger> taggers;

    public void AddTagger(IChartPointsTagger tagger)
    {
      string fn;
      if (tagger.GetFileName(out fn))
        taggers[fn] = tagger;
    }
    public ChartPointTagUpdater()
    {
      taggers = new SortedDictionary<string, IChartPointsTagger>();
      //var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
      //var textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
      //IVsTextView activeView = null;
      //ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out activeView));
      //var editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
      //IWpfTextView wpfTextView = editorAdapetr.GetWpfTextView(activeView);
    }
    public void RaiseChangeTagEvent(IFileChartPoints fPnts)
    {
      IChartPointsTagger tagger;
      if(taggers.TryGetValue(fPnts.data.fileFullName, out tagger))
        tagger?.RaiseTagsChangedEvent(fPnts);
    }

    public void RaiseChangeTagEvent(string fname, ILineChartPoints lPnts)
    {
      IChartPointsTagger tagger;
      if (taggers.TryGetValue(fname, out tagger))
        tagger?.RaiseTagsChangedEvent(lPnts);
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

      //!!! IGNORE NON-PROJECT FILES !!!
      ITextDocument thisTextDoc;
      var rc = view.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out thisTextDoc);
      string fileName = Path.GetFileName(thisTextDoc.FilePath);
      EnvDTE.Document dteDoc = Globals.dte.Documents.Item(fileName);
      if (dteDoc != null && dteDoc.ProjectItem != null && dteDoc.ProjectItem.ContainingProject != null
        && dteDoc.ProjectItem.ContainingProject.Name != "Miscellaneous Files" && dteDoc.ProjectItem.ContainingProject.UniqueName != "<MiscFiles>")
      {
        ChartPointsTagger tagger = new ChartPointsTagger(view, buffer);
        Globals.taggerUpdater.AddTagger(tagger);
        return tagger as ITagger<T>;
      }

      return null;
    }

  }
}

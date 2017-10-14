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
  /*internal*/public class ChartPointTag : IGlyphTag
  {
    public ILineChartPoints linePnt { get; }
    public ChartPointTag(ILineChartPoints _linePnt)
    {
      linePnt = _linePnt;
    }
  }

  public interface IChartPointsTagger
  {
    void RaiseTagsChangedEvent(IFileChartPoints fPnts);
    void RaiseTagsChangedEvent(ILineChartPoints lPnts);
    bool GetFileName(out string fn);
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
                  yield return new TagSpan<ChartPointTag>(new SnapshotSpan(line.Start, 0), new ChartPointTag(linePnt));
              }
            }
          }
        }
      }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
  }

  public class FileCPsObserver
  {
    public IChartPointsTagger fCPTagger { get; set; }

    public FileCPsObserver(IFileChartPoints fCPs, IChartPointsTagger _fCPTagger)
    {
      fCPTagger = _fCPTagger;
      fCPs.addCPLineEvent += OnAddCpLine;
      fCPs.remCPLineEvent += OnRemCpLine;
    }

    private void OnAddCpLine(CPFileEvArgs args)
    {
      fCPTagger?.RaiseTagsChangedEvent(args.lineCPs);
      args.lineCPs.lineCPStatusChangedEvent += OnLineCPStatusChanged;
    }

    private void OnLineCPStatusChanged(LineCPStatusEvArgs args)
    {
      fCPTagger?.RaiseTagsChangedEvent(args.lineCPs);
    }

    private void OnRemCpLine(CPFileEvArgs args)
    {
      fCPTagger?.RaiseTagsChangedEvent(args.lineCPs);
      args.lineCPs.lineCPStatusChangedEvent -= OnLineCPStatusChanged;
    }
  }

  public class FileCPTagObserver
  {
    public FileCPsObserver fCPObserver { get; set; }
    public IChartPointsTagger fCPTagger { get; set; }
    private IFileChartPoints fCPs;

    public FileCPTagObserver(IFileChartPoints fCPs)
    {
      CreateObserver(fCPs);
    }

    public FileCPTagObserver(ITextView view, ITextBuffer buffer)
    {
      CreateTagger(view, buffer);
    }

    public void CreateObserver(IFileChartPoints _fCPs)
    {
      if (fCPTagger == null)
        fCPs = _fCPs;
      else
      {
        if (fCPObserver != null)
          fCPObserver.fCPTagger = fCPTagger;//fCPObserver = null;
        else
          fCPObserver = new FileCPsObserver(fCPs, fCPTagger);
      }
    }

    public IChartPointsTagger CreateTagger(ITextView view, ITextBuffer buffer)
    {
      if (fCPTagger != null)
        fCPTagger = null;
      fCPTagger = new ChartPointsTagger(view, buffer);
      if (fCPs != null)
        CreateObserver(fCPs);

      return fCPTagger;
    }

  }

  [Export(typeof(IViewTaggerProvider))]
  [ContentType("C/C++")]
  [TagType(typeof(ChartPointTag))]
  class ChartPointsTaggerProvider : IViewTaggerProvider
  {
    private SortedDictionary<string, FileCPTagObserver> taggers = new SortedDictionary<string, FileCPTagObserver>();

    public ChartPointsTaggerProvider()
    {
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      ICPEventService cpEvsService;
      cpServProv.GetService<ICPEventService>(out cpEvsService);
      IConstructEvents constrEvents = cpEvsService.GetConstructEvents();
      constrEvents.createdFileCPsEvent += OnFileCPsCreated;
    }

    void OnFileCPsCreated(IConstructEventArgs<IFileChartPoints> args)
    {
      FileCPTagObserver fCPTagObserver;
      if (taggers.TryGetValue(args.obj.data.fileFullName, out fCPTagObserver))
        fCPTagObserver.CreateObserver(args.obj);
      else
        taggers.Add(args.obj.data.fileFullName, new FileCPTagObserver(args.obj));
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
        FileCPTagObserver fCPTagObserver;
        string fn = System.IO.Path.GetFullPath(thisTextDoc.FilePath).ToLower();
        if (taggers.TryGetValue(fn, out fCPTagObserver))
          fCPTagObserver.CreateTagger(view, buffer);
        else
        {
          fCPTagObserver = new FileCPTagObserver(view, buffer);
          taggers.Add(fn, fCPTagObserver);
        }

        return fCPTagObserver.fCPTagger as ITagger<T>;
      }

      return null;
    }

  }
}

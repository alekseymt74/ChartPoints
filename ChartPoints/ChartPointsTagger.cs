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
    public uint lineMask;
    public ChartPointTag(uint _lineMask)
    {
      lineMask = _lineMask;
    }
  }

  public interface IChartPointsTagger
  {
    void AddLine(int lineNum, uint lineStatus);
    void RemoveLine(int lineNum);
    void RaiseTagChangedEvent(int lineNum, uint newLineStatus);
    bool GetFileName(out string fn);
  }

  internal class LineMaskPair
  {
    public int lineNum;
    public uint lineMask;

    internal LineMaskPair(int _lineNum, uint _lineMask)
    {
      lineNum = _lineNum;
      lineMask = _lineMask;
    }
  }

  public class ChartPointsTagger : ITagger<ChartPointTag>, IChartPointsTagger
  {
    private ITextView _view;
    public ITextBuffer _buffer;
    private SortedSet<LineMaskPair> lines
      = new SortedSet<LineMaskPair>(Comparer<LineMaskPair>.Create((lh, rh) => (lh.lineNum.CompareTo(rh.lineNum))));
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

    private void RaiseTagChangedEvent(int lineNum)
    {
      var tempEvent = TagsChanged;
      if (tempEvent != null)
      {
        if (lineNum - 1 < _buffer.CurrentSnapshot.Lines.Count())
        {
          ITextSnapshotLine line = _buffer.CurrentSnapshot.Lines.ElementAt(lineNum - 1);
          tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(line.Start, 1)));
        }
      }
    }

    public void AddLine(int lineNum, uint lineStatus)
    {
      lines.Add(new LineMaskPair(lineNum, lineStatus));
      RaiseTagChangedEvent(lineNum);
    }

    public void RemoveLine(int lineNum)
    {
      LineMaskPair res = lines.FirstOrDefault((p) => (p.lineNum == lineNum));
      if (res != null)
      {
        lines.Remove(res);
        RaiseTagChangedEvent(lineNum);
      }
    }

    public void RaiseTagChangedEvent(int lineNum, uint newLineStatus)
    {
      LineMaskPair res = lines.FirstOrDefault((p) => (p.lineNum == lineNum));
      if (res != null)
      {
        res.lineMask = newLineStatus;
        RaiseTagChangedEvent(lineNum);
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
      foreach (SnapshotSpan span in spans)
      {
        ITextSnapshotLine line = span.Start.GetContainingLine();
        int firstLineNum = line.LineNumber;
        int lastLineNum = span.End.GetContainingLine().LineNumber;
        for (int i = firstLineNum; i <= lastLineNum; ++i)
        {
          LineMaskPair res = lines.FirstOrDefault((p) => (p.lineNum - 1 == firstLineNum));
          if (res != null)
            yield return new TagSpan<ChartPointTag>(new SnapshotSpan(line.Start, 0), new ChartPointTag(res.lineMask));
        }
      }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
  }

  public class FileCPsObserver
  {
    public IChartPointsTagger fCPTagger;
    private IFileChartPoints fCPs;

    public FileCPsObserver(IFileChartPoints _fCPs, IChartPointsTagger _fCPTagger)
    {
      fCPTagger = _fCPTagger;
      fCPs = _fCPs;
      fCPs.addCPLineEvent += OnAddCpLine;
      fCPs.remCPLineEvent += OnRemCpLine;
      fCPs.moveCPLineEvent += OnMoveCPLine;
    }

    public void SetTagger(IChartPointsTagger _fCPTagger)
    {
      fCPTagger = _fCPTagger;
      foreach(ILineChartPoints lCPs in fCPs.linePoints)
        fCPTagger.AddLine(lCPs.data.pos.lineNum, (uint) lCPs.status);
    }
    private void OnAddCpLine(CPFileEvArgs args)
    {
      fCPTagger.AddLine(args.lineCPs.data.pos.lineNum, (uint)args.lineCPs.status);
      args.lineCPs.lineCPStatusChangedEvent += OnLineCPStatusChanged;
    }

    private void OnLineCPStatusChanged(LineCPStatusEvArgs args)
    {
      fCPTagger?.RaiseTagChangedEvent(args.lineCPs.data.pos.lineNum, (uint)args.lineCPs.status);
    }

    private void OnRemCpLine(CPFileEvArgs args)
    {
      fCPTagger.RemoveLine(args.lineCPs.data.pos.lineNum);
      args.lineCPs.lineCPStatusChangedEvent -= OnLineCPStatusChanged;
    }
    private void OnMoveCPLine(CPLineMoveEvArgs args)
    {
      fCPTagger.RemoveLine(args.prevLine);
      fCPTagger.AddLine(args.newLine, (uint)args.lineCPs.status);
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
      fCPs = _fCPs;
      if (fCPTagger == null)
        ;//fCPs = _fCPs;
      else
      {
        if (fCPObserver != null)
          fCPObserver.SetTagger(fCPTagger);
        else
          fCPObserver = new FileCPsObserver(fCPs, fCPTagger);
      }
    }

    public void RemoveObserver()
    {
      fCPObserver = null;
      fCPs = null;
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
      constrEvents.deletedFileCPsEvent += OnFileCPsDeleted;
    }

    void OnFileCPsCreated(IConstructEventArgs<IFileChartPoints> args)
    {
      FileCPTagObserver fCPTagObserver;
      if (taggers.TryGetValue(args.obj.data.fileFullName, out fCPTagObserver))
        fCPTagObserver.CreateObserver(args.obj);
      else
        taggers.Add(args.obj.data.fileFullName, new FileCPTagObserver(args.obj));
    }

    void OnFileCPsDeleted(IConstructEventArgs<IFileChartPoints> args)
    {
      FileCPTagObserver fCPTagObserver;
      if (taggers.TryGetValue(args.obj.data.fileFullName, out fCPTagObserver))
        fCPTagObserver.RemoveObserver();
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
      //int height = view.Properties.Item("ActualHeight");
      //Type _t = typeof(T);
      bool b = (typeof(T) == typeof(ChartPointTag));

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

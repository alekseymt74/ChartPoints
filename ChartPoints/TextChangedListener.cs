using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

namespace ChartPoints
{
  public class FileChangeTracker
  {
    private IFileTracker fileTracker;
    public IWpfTextView textView;
    public string fileFullName;
    //private IFileChartPoints fPnts;

    public FileChangeTracker(IWpfTextView _textView, string _fileFullName, IFileTracker _fileTracker)
    {
      fileFullName = _fileFullName;
      fileTracker = _fileTracker;
      Advise(_textView);
    }

    public void Advise(IWpfTextView _textView)
    {
      // !!! CHECK EQUAL - need readvise !!!
      if (textView != null)
        textView.TextBuffer.Changed -= TextBufferOnChanged;
      textView = _textView;
      textView.TextBuffer.Changed += TextBufferOnChanged;
    }

    public void UnAdvise()
    {
      if (textView != null)
        textView.TextBuffer.Changed -= TextBufferOnChanged;
    }

    //private TextContentChangedEventArgs evArgs;
    //async private void ValidateChartPoints(TextContentChangedEventArgs e)
    //{
    //  TextContentChangedEventArgs curEvArgs = null;
    //  lock (evArgs)
    //  {
    //    curEvArgs = evArgs;
    //  }
    //}
    private void TextBufferOnChanged(object sender, TextContentChangedEventArgs e)
    {
      ITextSnapshot snapshot = e.After;
      foreach (ITextChange change in e.Changes)
      {
        int lineNum = snapshot.GetLineFromPosition(change.NewPosition).LineNumber;
        fileTracker.Validate(lineNum + 1, change.LineCountDelta);
      }
      //lock (evArgs)
      //{
      //  evArgs = e;
      //}
      //string fileName = Path.GetFileName(fileFullName);
      //EnvDTE.Document dteDoc = Globals.dte.Documents.Item(fileName);
      //IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(dteDoc.ProjectItem.ContainingProject.Name);
      //if (pPnts != null)
      //{
      //  fPnts = pPnts.GetFileChartPoints(fileName);
      //  if (fPnts != null)
      //  {
      //    ITextSnapshot snapshot = e.After;
      //    foreach (ITextChange change in e.Changes)
      //    {
      //      int lineNum = snapshot.GetLineFromPosition(change.NewPosition).LineNumber;
      //      fPnts.ValidatePosition(lineNum + 1, change.LineCountDelta);
      //    }
      //  }
      //}
    }

  }

  [ContentType("C/C++")]
  [Export(typeof(IWpfTextViewCreationListener))]
  [TextViewRole(PredefinedTextViewRoles.Editable)]
  public /*sealed*/ class TextChangedListener : IWpfTextViewCreationListener
  {
    private IDictionary<string, IWpfTextView> openedFiles = new SortedDictionary<string, IWpfTextView>();
    private ISet<FileChangeTracker> fileTrackers
      = new SortedSet<FileChangeTracker>(Comparer<FileChangeTracker>.Create((lh, rh) => (String.Compare(lh.fileFullName, rh.fileFullName, StringComparison.Ordinal))));
    private ICPTrackService cpTrackServ;

    public TextChangedListener()
    {
      var cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.GetService<ICPTrackService>(out cpTrackServ);
      if(cpTrackServ == null)
      {
        cpTrackServ = new CPTrackService();
        cpServProv.RegisterService(cpTrackServ);
      }
      cpTrackServ.addFTrackerEvent += OnAddFTracker;
      cpTrackServ.remFTrackerEvent += OnRemFTracker;
    }

    protected FileChangeTracker GetFChangeTracker(string fileFullName)
    {
      return fileTrackers.FirstOrDefault((ft) => (ft.fileFullName == fileFullName));
    }

    protected void AddView(string fileFullName, IWpfTextView view)
    {
      IWpfTextView theView = null;
      if (openedFiles.TryGetValue(fileFullName, out theView))
        theView = view;
      else
        openedFiles.Add(fileFullName, view);
    }

    public void TextViewCreated(IWpfTextView textView)
    {
      ITextDocument textDoc;
      //string fileName = null;
      var rc = textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);
      if (rc)
      {
        //fileName = Path.GetFileName(textDoc.FilePath);
        string fileFullName = System.IO.Path.GetFullPath(textDoc.FilePath).ToLower();
        IFileTracker fileTracker = cpTrackServ.GetFileTracker(fileFullName);
        FileChangeTracker fChangeTracker = GetFChangeTracker(fileFullName);
        if (fChangeTracker == null)
        {
          if (fileTracker != null)
          {
            fChangeTracker = new FileChangeTracker(textView, fileFullName, fileTracker);
            fileTrackers.Add(fChangeTracker);
          }
          else
            AddView(fileFullName, textView);
        }
        else
          fChangeTracker.Advise(textView);
      }
    }

    private void OnAddFTracker(FileTrackerArgs args)
    {
      IWpfTextView view = null;
      if (openedFiles.TryGetValue(args.fileTracker.fileFullName, out view))
      {
        FileChangeTracker fChangeTracker = GetFChangeTracker(args.fileTracker.fileFullName);
        if (fChangeTracker == null)
        {
          fChangeTracker = new FileChangeTracker(view, args.fileTracker.fileFullName, args.fileTracker);
          fileTrackers.Add(fChangeTracker);
        }
        openedFiles.Remove(args.fileTracker.fileFullName);
      }
    }

    private void OnRemFTracker(FileTrackerArgs args)
    {
      FileChangeTracker fChangeTracker = GetFChangeTracker(args.fileTracker.fileFullName);
      if (fChangeTracker != null)
      {
        fChangeTracker.UnAdvise();
        AddView(fChangeTracker.fileFullName, fChangeTracker.textView);
        fileTrackers.Remove(fChangeTracker);
      }
    }

  }

}

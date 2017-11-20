//------------------------------------------------------------------------------
// <copyright file="CPClassifier.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace ChartPoints
{
  /// <summary>
  /// Classifier that classifies all text as an instance of the "CPClassifier" classification type.
  /// </summary>
  internal class CPClassifier : IClassifier
  {
    /// <summary>
    /// Classification type.
    /// </summary>
    private readonly IClassificationType classificationType;

    /// <summary>
    /// Initializes a new instance of the <see cref="CPClassifier"/> class.
    /// </summary>
    /// <param name="registry">Classification registry.</param>
    internal CPClassifier(IClassificationTypeRegistryService registry)
    {
      this.classificationType = registry.GetClassificationType("CPClassifier");
    }

    #region IClassifier

#pragma warning disable 67

    /// <summary>
    /// An event that occurs when the classification of a span of text has changed.
    /// </summary>
    /// <remarks>
    /// This event gets raised if a non-text change would affect the classification in some way,
    /// for example typing /* would cause the classification to change in C# without directly
    /// affecting the span.
    /// </remarks>
    public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

    /// <summary>
    /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
    /// </summary>
    /// <remarks>
    /// This method scans the given SnapshotSpan for potential matches for this classification.
    /// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
    /// </remarks>
    /// <param name="span">The span currently being classified.</param>
    /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
    public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
    {
      List<ClassificationSpan> spans = null;
      if (span.Snapshot.ContentType.DisplayName == "BuildOutput")
      {
        spans = new List<ClassificationSpan>();
        ITextBuffer textBuf = span.Snapshot.TextBuffer;
        var text = span.GetText();
        int indFNameBegin = text.IndexOf("__cp__.");
        if (indFNameBegin >= 0)
        {
          if (text.IndexOf("__cp__.tracer.cpp") >= 0 || text.IndexOf("warning") >= 0)
          {
            if (textBuf.CheckEditAccess())
            {
              ITextEdit te = textBuf.CreateEdit();
              te.Delete(span);
              te.Apply();
            }
            //textBuf.Delete(span);
          }
          else if (text.IndexOf(" error ") >= 0)
          {
            int indFNameEnd = 0;
            if ((indFNameEnd = text.IndexOf(' ', indFNameBegin)) < 0)
              indFNameEnd = text.Length - 1;
            string fName = text.Substring(indFNameBegin, indFNameEnd - indFNameBegin);
            if (textBuf.CheckEditAccess())
            {
              ITextEdit te = textBuf.CreateEdit();
              te.Replace(span, "[ERROR]: Failed to add chartpoints to " + fName + ". Please first check successful build in non [ChartPoints] configuration\n");
              te.Apply();
            }
            //textBuf.Replace(span, "[ERROR]: Failed to add chartpoints to " + fName + ". Please first check successful build in non [ChartPoints] configuration\n");
          }
          else
          {
            text = text.Replace("__cp__.", "Instrumenting chartpoints for ");
            if (textBuf.CheckEditAccess())
            {
              ITextEdit te = textBuf.CreateEdit();
              te.Replace(span, text);
              te.Apply();
            }
            //textBuf.Replace(span, text);
          }
        }
      }
      else
      {
        spans = new List<ClassificationSpan>()
            {
                new ClassificationSpan(new SnapshotSpan(span.Snapshot, new Span(span.Start, span.Length)), this.classificationType)
            };
      }

      return spans;
    }

    #endregion
  }
}

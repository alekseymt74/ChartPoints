//------------------------------------------------------------------------------
// <copyright file="CPClassifierFormat.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ChartPoints
{
  /// <summary>
  /// Defines an editor format for the CPClassifier type that has a purple background
  /// and is underlined.
  /// </summary>
  [Export(typeof(EditorFormatDefinition))]
  [ClassificationType(ClassificationTypeNames = "CPClassifier")]
  [Name("CPClassifier")]
  [UserVisible(true)] // This should be visible to the end user
  [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
  internal sealed class CPClassifierFormat : ClassificationFormatDefinition
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CPClassifierFormat"/> class.
    /// </summary>
    public CPClassifierFormat()
    {
      this.DisplayName = "CPClassifier"; // Human readable version of the name
      //this.BackgroundColor = Colors.BlueViolet;
      //this.TextDecorations = System.Windows.TextDecorations.Underline;
    }
  }
}

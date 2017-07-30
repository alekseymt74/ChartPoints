//------------------------------------------------------------------------------
// <copyright file="ChartPointsEditorClassifierClassificationDefinition.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ChartPoints
{
  /// <summary>
  /// Classification type definition export for ChartPointsEditorClassifier
  /// </summary>
  internal static class ChartPointsEditorClassifierClassificationDefinition
  {
    // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

    /// <summary>
    /// Defines the "ChartPointsEditorClassifier" classification type.
    /// </summary>
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("ChartPointsEditorClassifier")]
    private static ClassificationTypeDefinition typeDefinition;

#pragma warning restore 169
  }
}

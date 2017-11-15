//------------------------------------------------------------------------------
// <copyright file="CPClassifierClassificationDefinition.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ChartPoints
{
  /// <summary>
  /// Classification type definition export for CPClassifier
  /// </summary>
  internal static class CPClassifierClassificationDefinition
  {
    // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

    /// <summary>
    /// Defines the "CPClassifier" classification type.
    /// </summary>
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("CPClassifier")]
    private static ClassificationTypeDefinition typeDefinition;

#pragma warning restore 169
  }
}

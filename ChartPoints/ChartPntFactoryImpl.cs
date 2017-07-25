using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using Microsoft.VisualStudio.VCCodeModel;

namespace ChartPoints
{

  /// <summary>
  /// Implementation of IChartPntFactory
  /// </summary>
  class ChartPntFactoryImpl : ChartPntFactory
  {
    public ChartPntFactoryImpl()
    {
      // check and set singleton factory instance
      // helps to hide the injection of another factory object, eg. factory stub in tests
      if (ChartPntFactory.factory == null)
        ChartPntFactory.factory = this;
    }

    public override IChartPointsProcessor CreateProcessor()
    {
      return new ChartPointsProcessor();
    }
  }
}

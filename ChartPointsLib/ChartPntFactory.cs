﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ChartPoints
{
  /// <summary>
  /// Global elements. Initialized by ChartPointsPackage
  /// </summary>
  public class Globals
  {
    public static DTE dte { get; set; }

    public static IChartPointsProcessor processor { get; set; }

    public static IChartPointTagUpdater taggerUpdater { get; set; }
  }

  /// <summary>
  /// Singleton factory for all objects used in extension
  /// Factory instance initialized in implementation of the class
  /// Provides looseness of interacting objects by overloading of factory methods
  /// Also useful for tests (see ChartPointsTests::ChartPntFactoryStub)
  /// </summary>
  public class ChartPntFactory
  {
    /// <summary>
    /// Instance of the factory
    /// Can be set only in subclasses (implementations of the factory)
    /// </summary>
    protected static ChartPntFactory factory;

    protected ChartPntFactory() {}

    public static ChartPntFactory Instance
    {
      get { return factory; }
    }

    /// <summary>
    /// Creates main ChartPoints object
    /// </summary>
    /// <returns>IChartPointsProcessor</returns>
    public virtual IChartPointsProcessor CreateProcessor() {  return null; }
  }
}

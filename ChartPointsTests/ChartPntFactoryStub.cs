using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;

namespace ChartPointsTests
{
  public class ChartPntFactoryStub : ChartPntFactoryImpl
  {
    public ChartPntFactoryStub()
    {
      ChartPntFactory.factory = this;
    }
  }
}

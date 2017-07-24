using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  class ChartPntFactoryImpl : ChartPntFactory
  {
    public ChartPntFactoryImpl()
    {
      if(ChartPntFactory.factory == null)
        ChartPntFactory.factory = this;
    }
  }
}

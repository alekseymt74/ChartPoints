using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public class ChartPntFactory
  {
    protected static ChartPntFactory factory;

    protected ChartPntFactory() {}

    public static ChartPntFactory Instance
    {
      get { return factory; }
    }
  }
}

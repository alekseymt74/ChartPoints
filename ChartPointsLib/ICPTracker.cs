using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{

  public interface ICPValidator
  {
    void Validate(int lineNum, int linesAdd);
  }

  public interface ICPTracker
  {
    void Add(string fileFullName, ICPValidator cpValidator);
  }

}

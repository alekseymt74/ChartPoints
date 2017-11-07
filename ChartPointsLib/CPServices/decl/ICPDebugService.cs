using ChartPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints.CPServices.decl
{
  public class CPProcEvArgs
  {
    public ulong procId { get; }
    public string Name { get; }

    public CPProcEvArgs(ulong _procId, string _procName)
    {
      procId = _procId;
      Name = _procName;
    }
  }

  public interface ICPDebugService : ICPService
  {
    ICPEvent<CPProcEvArgs> debugProcCreateCPEvent { get; set; }
    ICPEvent<CPProcEvArgs> debugProcDestroyCPEvent { get; set; }
  }

}

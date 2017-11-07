using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints.CPServices.decl
{
    public interface ICPExtension : ICPService
    {
        string GetVSIXInstallPath();
    }
}

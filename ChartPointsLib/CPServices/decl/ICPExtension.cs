using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints.CPServices.decl
{
    public enum EMode
    {
      Design
      , Build
      , Run
    }

    public interface ICPExtension : ICPService
    {
      string GetVSIXInstallPath();
      EMode GetMode();
      EMode SetMode(EMode newMode);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public interface ICPOrchestrator
  {
    bool InitSolutionConfigurations();
    bool RemoveSolutionConfigurations();
    bool Orchestrate(string projConfFile);
    bool SaveProjChartPonts(string projConfFile);
    bool LoadProjChartPoints(string projConfFile);
    bool SaveChartPonts();
    bool Build();
    bool Run();
  }
}

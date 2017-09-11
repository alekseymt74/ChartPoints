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
    bool Orchestrate(EnvDTE.Project proj);
    Microsoft.Build.Evaluation.Project Orchestrate(string projConfFile);
    bool SaveProjChartPoints(EnvDTE.Project proj);
    Microsoft.Build.Evaluation.Project SaveProjChartPoints(string projConfFile);
    bool LoadProjChartPoints(EnvDTE.Project proj);
    Microsoft.Build.Evaluation.Project LoadProjChartPoints(string projConfFile);
    bool SaveChartPonts();
    bool Build();
    bool Run();
    bool SaveProject(EnvDTE.Project proj, Microsoft.Build.Evaluation.Project msbuildProj);
    bool UnloadProject(EnvDTE.Project proj);
  }
}

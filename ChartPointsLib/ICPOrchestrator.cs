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
    bool InitProjConfigurations(EnvDTE.Project proj);
    bool Orchestrate(EnvDTE.Project proj);
    Microsoft.Build.Evaluation.Project Orchestrate(string projConfFile);
    bool Build();
    bool Run();
    bool UnloadProject(EnvDTE.Project proj);
  }
}

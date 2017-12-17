using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public class CmdEventsHandler
  {
    private IVsSolution vsSolution;
    private CommandEvents cmdEvents;
    private CommandEvents createProjCmd;
    private CommandEvents addNewProjCmd;
    private SortedSet<EnvDTE.Project> beforeAddProjsCont;

    public CmdEventsHandler(IVsSolution _vsSolution)
    {
      vsSolution = _vsSolution;
      cmdEvents = Globals.dte.Events.CommandEvents;
      string guidVSStd97 = "{5efc7975-14bc-11cf-9b2b-00aa00573819}".ToUpper();
      createProjCmd = Globals.dte.Events.CommandEvents[guidVSStd97, (int)VSConstants.VSStd97CmdID.NewProject];
      createProjCmd.BeforeExecute += NewProjCreated_BeforeExecute;
      createProjCmd.AfterExecute += NewProjCreated_AfterExecute;
      addNewProjCmd = Globals.dte.Events.CommandEvents[guidVSStd97, (int)VSConstants.VSStd97CmdID.AddNewProject];
      addNewProjCmd.BeforeExecute += NewProjCreated_BeforeExecute;
      addNewProjCmd.AfterExecute += NewProjCreated_AfterExecute;
    }

    private void AddExistingCPPProjs(out SortedSet<EnvDTE.Project> projsCont)
    {
      projsCont = null;
      if (Globals.dte.Solution.Projects.Count > 0)
      {
        projsCont = new SortedSet<EnvDTE.Project>(Comparer<EnvDTE.Project>.Create((lh, rh) => (String.Compare(lh.FullName, rh.FullName, StringComparison.Ordinal))));
        foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
        {
          if (proj.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" && proj.Name != "Miscellaneous Files")
            projsCont.Add(proj);
        }
      }
    }

    private void NewProjCreated_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
    {
      AddExistingCPPProjs(out beforeAddProjsCont);
    }

    void NewProjCreated_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
    {
      // if we are here - new project created 
      string newProjFullName = string.Empty;
      if (beforeAddProjsCont != null)
      {
        if (Globals.dte.Solution.Projects.Count > beforeAddProjsCont.Count)
        {
          SortedSet<EnvDTE.Project> afterAddProjsCont;
          AddExistingCPPProjs(out afterAddProjsCont);
          if (afterAddProjsCont.Count > beforeAddProjsCont.Count)
          {
            IEnumerable<EnvDTE.Project> newProjs = afterAddProjsCont.Except(beforeAddProjsCont);
            newProjFullName = newProjs.First().FullName;
          }
        }
      }
      else
      {
        foreach (EnvDTE.Project proj in Globals.dte.Solution.Projects)
        {
          if (proj.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" && proj.Name != "Miscellaneous Files")
          {
            newProjFullName = proj.FullName;
            break;
          }
        }
      }
      if (newProjFullName != string.Empty)
      {
        IVsSolution4 vsSolution4 = vsSolution as IVsSolution4;
        IVsHierarchy projObj;
        vsSolution.GetProjectOfUniqueName(newProjFullName, out projObj);
        System.Guid projGuid = System.Guid.Empty;
        vsSolution.GetGuidOfProject(projObj, out projGuid);
        vsSolution4.UnloadProject(projGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_LoadPendingIfNeeded);
        Globals.orchestrator.Orchestrate(newProjFullName);
        vsSolution4.ReloadProject(projGuid);
      }

      /////////////////////////////////////////
      //Globals.orchestrator.InitSolutionConfigurations();
      //IVsSolution4 vsSolution4 = GetService(typeof(SVsSolution)) as IVsSolution4;
      //IVsSolution vsSolution = vsSolution4 as IVsSolution;
      //IEnumHierarchies hierarchies;
      //vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS/*EPF_MATCHTYPE*/, System.Guid.Empty, out hierarchies);
      //IVsHierarchy[] foundHierarchies = new IVsHierarchy[1];
      //uint count;
      //while (hierarchies.Next(1, foundHierarchies, out count) == VSConstants.S_OK && count == 1)
      //{
      //  IVsProject vsProj = foundHierarchies[0] as IVsProject;
      //  EnvDTE.Project proj = null;
      //  object propProjObj = null;
      //  foundHierarchies[0].GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out propProjObj);
      //  if (propProjObj != null)
      //    proj = propProjObj as EnvDTE.Project;
      //  System.Guid projGuid = System.Guid.Empty;
      //  vsSolution.GetGuidOfProject(foundHierarchies[0], out projGuid);
      //  if (proj != null && proj.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" && proj.Name != "Miscellaneous Files")
      //  {
      //    string projName = proj.FullName;
      //    vsSolution4.UnloadProject(projGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_LoadPendingIfNeeded);
      //    Globals.orchestrator.Orchestrate(projName);
      //    vsSolution4.ReloadProject(projGuid);
      //  }
      //}
      ////if (VsSolutionEvents.newProjFullPath != null)
      ////{
      ////  vsSolution4.UnloadProject(VsSolutionEvents.newProjGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_LoadPendingIfNeeded);
      ////  Globals.orchestrator.Orchestrate(VsSolutionEvents.newProjFullPath);
      ////  vsSolution4.ReloadProject(VsSolutionEvents.newProjGuid);
      ////}
    }

  }

}

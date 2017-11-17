using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints.CPServices.decl;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using Microsoft.VisualStudio;

namespace ChartPoints.CPServices.impl
{

  class CPDebugService
    : ICPDebugService
    , IDebugEventCallback2
  {
    public ICPEvent<CPProcEvArgs> debugProcCreateCPEvent { get; set; } = new CPEvent<CPProcEvArgs>();
    public ICPEvent<CPProcEvArgs> debugProcDestroyCPEvent { get; set; } = new CPEvent<CPProcEvArgs>();

    private IVsDebugger debugService;

    public CPDebugService(IVsDebugger _debugService)
    {
      debugService = _debugService;
      debugService.AdviseDebugEventCallback(this);
    }

    private bool CreateProcEvArgs(IDebugProcess2 pProcess, out CPProcEvArgs args)
    {
      args = null;
      string name = string.Empty;
      if (pProcess.GetName((uint)enum_GETNAME_TYPE.GN_NAME/*GN_BASENAME*/, out name) != VSConstants.S_OK)
        return false;
      AD_PROCESS_ID[] adProcId = new AD_PROCESS_ID[1];
      if (pProcess.GetPhysicalProcessId(adProcId) != VSConstants.S_OK)
        return false;
      args = new CPProcEvArgs(adProcId[0].dwProcessId, name);

      return true;
    }

    private bool IsChartPointsMode()
    {
      EnvDTE.Property prop = Globals.dte.Solution.Properties.Item("ActiveConfig");
      if (prop != null)
      {
        string activeConfig = (string)Globals.dte.Solution.Properties.Item("ActiveConfig").Value;

        return activeConfig.Contains(" [ChartPoints]");
      }

      return false;
    }

    public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
    {
      if (IsChartPointsMode())
      {
        if (pEvent is IDebugProcessCreateEvent2)
        {
          CPProcEvArgs args;
          if (CreateProcEvArgs(pProcess, out args))
          {
            Debug.WriteLine("[IDebugProcessCreateEvent2]; " + args.Name);
            debugProcCreateCPEvent.Fire(args);
          }
        }
        else if (pEvent is IDebugProcessDestroyEvent2)
        {
          CPProcEvArgs args;
          if (CreateProcEvArgs(pProcess, out args))
          {
            Debug.WriteLine("[IDebugProcessDestroyEvent2]; " + args.Name);
            debugProcDestroyCPEvent.Fire(args);
          }
        }
      }
      //else if (pEvent is IDebugProgramCreateEvent2)
      //{
      //  pProgram?.GetName(out name);
      //  Debug.WriteLine("[IDebugProgramCreateEvent2]; " + name);
      //}
      //else if (pEvent is IDebugProgramDestroyEvent2)
      //{
      //  pProgram?.GetName(out name);
      //  Debug.WriteLine("[IDebugProgramDestroyEvent2]; " + name);
      //}

      ComRelease(pEngine);
      ComRelease(pProcess);
      ComRelease(pProgram);
      ComRelease(pThread);
      ComRelease(pEvent);

      return VSConstants.S_OK;
    }

    private static void ComRelease(object o)
    {
      if (o != null && System.Runtime.InteropServices.Marshal.IsComObject(o))
      {
        System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
      }
    }
  }

}

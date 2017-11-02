﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CPTracerLib;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace ChartPoints
{
  class CPTraceHandler// : IDisposable
  {
    private ICPTracerFactory tracersFactory;
    private CPProcTracer procTracer;
    private IVsOutputWindowPane outputWindowPane;
    private ICPTracerService traceServ;
    private IDictionary<UInt64, ICPTracerDelegate> traceConsumers;

    [DllImport("ole32.dll",EntryPoint = "CoInitialize",CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 CoInitialize(int reserved);

    [DllImport("Ole32.dll", /*ExactSpelling = true, */EntryPoint = "CoInitializeEx", CallingConvention = CallingConvention.StdCall)]//, SetLastError = false, PreserveSig = false)]
    static extern UInt32 CoInitializeEx(int reserved, uint dwCoInit);

    [DllImport("ole32.dll", EntryPoint = "CoCreateInstance", CallingConvention = CallingConvention.StdCall)]
    static extern UInt32 CoCreateInstance([In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
       IntPtr pUnkOuter, UInt32 dwClsContext, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
       [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

    public CPTraceHandler()
    {
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.GetService<ICPTracerService>(out traceServ);
      traceServ.Activate();
      traceConsumers = new SortedDictionary<ulong, ICPTracerDelegate>();

      Globals.outputWindow.GetPane(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, out outputWindowPane);
      if (outputWindowPane != null)
        outputWindowPane.Activate();
      //UInt32 ret = CoInitializeEx(0, 0/*COINIT_MULTITHREADED*/);
      Guid test_srv_CLSID = new Guid("EA343A3A-CF94-4210-89F5-9BDF56112CA2");
      Type test_srv_type = Type.GetTypeFromCLSID(test_srv_CLSID, true);
      Object obj = null;
      Guid IUnknownGuid = new Guid("00000000-0000-0000-C000-000000000046");
      //UInt32 dwRes = CoCreateInstance(test_srv_CLSID, IntPtr.Zero, (uint)(CLSCTX.CLSCTX_LOCAL_SERVER), IUnknownGuid, out obj);
      obj = Activator.CreateInstance(test_srv_type);
      tracersFactory = obj as ICPTracerFactory;
      foreach (EnvDTE.Process p in Globals.dte.Debugger.DebuggedProcesses)
      {
        Debug.WriteLine("$$$$$$$$$$$$$$$$$ " + p.ProcessID);
      }
      tracersFactory.CreateProcTracer(out procTracer, 1);
      //traceServ.RegProcessTracer(1, "proc_name");
      procTracer.OnRegElem += RegElem;
      procTracer.OnTrace += Trace;
    }

    private void RegElem(string name, UInt64 id, UInt16 typeID)
    {
      //Debug.WriteLine("[*** CPTraceHandler::RegElem ***]; thread id: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
      outputWindowPane.OutputString("[RegElem]; name: " + name + "\tid: " + id + "\ttypeID: " + typeID + "\n");
      ICPTracerDelegate cpDelegate = traceServ.RegTraceEnt(id, name);
    }

    //private void Trace(/*UInt64 id, */[MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_RECORD)]System.Array/*object*/ vals)
    private void Trace(ulong id, System.Array tms, System.Array vals)
    {
      //Debug.WriteLine("##################" + tms.Length.ToString());
      //Debug.WriteLine("[*** CPTraceHandler::Trace ***]; thread id: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
      traceServ.Trace(id, tms, vals);
    }

    private void CloseHandle()
    {
      if (procTracer != null)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(procTracer);
        procTracer = null;
      }
      if (tracersFactory != null)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(tracersFactory);
        tracersFactory = null;
      }
      GC.Collect();
    }

    public void Dispose()
    {
      this.CloseHandle();
      //GC.SuppressFinalize(this);
    }

    //~CPTraceHandler()
    //{
    //  this.CloseHandle();
    //}
  }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CPTracerLib;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace ChartPoints
{
  class CPTraceHandler// : IDisposable
  {
    private ICPTracerFactory test_srv;
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

    private void RegElem(string name, UInt64 id, UInt16 typeID)
    {
      outputWindowPane.OutputString("[RegElem]; name: " + name + "\tid: " + id + "\ttypeID: " + typeID + "\n");
      ICPTracerDelegate cpDelegate = traceServ.RegTraceEnt(id, name);// Globals.cpTracer.CreateTracer(name);
      //traceConsumers.Add(id, cpDelegate);
    }

    //private void Trace(/*UInt64 id, */[MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_RECORD)]System.Array/*object*/ vals)
    private void Trace(ulong id, System.Array traceEnts)//System.Array ids, System.Array vals)
    {
      traceServ.Trace(id, traceEnts);
      ////for (int i = 0; i < /*vals*/traceEnts.Length; ++i)
      //foreach(TraceEnt te in traceEnts)
      //{
      //  traceServ.Trace(te.id, te.val);
      //  ////outputWindowPane.OutputString("[Trace]; id: " + ids.GetValue(i) + "\t: " + vals.GetValue(i) + "\n");
      //  ////foreach(var v in vals)
      //  ////  outputWindowPane.OutputString("[Trace]; id: " + ((TraceEnt)v).id + "\t: " + ((TraceEnt)v).val + "\n");
      //  //ICPTracerDelegate cpDelegate = null;
      //  //if (traceConsumers.TryGetValue(/*Convert.ToUInt64(ids.GetValue(i))*/te.id, out cpDelegate))
      //  //{
      //  //  //foreach (var v in vals)
      //  //    cpDelegate.Trace(/*Convert.ToDouble(vals.GetValue(i))*/te.val);
      //  //}
      //}
      ////foreach (KeyValuePair<ulong, ICPTracerDelegate> cpDelegate in traceConsumers)
      ////{
      ////  cpDelegate.Value.UpdateView();
      ////}
      ////Globals.cpTracer.UpdateView();
    }

    public CPTraceHandler()
    {
      //tracer = _tracer;
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      cpServProv.GetService<ICPTracerService>(out traceServ);
      //      Globals.cpTracer.Activate();
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
      test_srv = obj as ICPTracerFactory;
      test_srv.CreateProcTracer(out procTracer, 1);
      procTracer.OnRegElem += RegElem;
      procTracer.OnTrace += Trace;
    }

    private void CloseHandle()
    {
      if (procTracer != null)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(procTracer);
        procTracer = null;
      }
      if (test_srv != null)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(test_srv);
        test_srv = null;
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

using System;
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

    [DllImport("ole32.dll",EntryPoint = "CoInitialize",CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 CoInitialize(int reserved);

    [DllImport("ole32.dll", EntryPoint = "CoCreateInstance", CallingConvention = CallingConvention.StdCall)]
    static extern UInt32 CoCreateInstance([In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
       IntPtr pUnkOuter, UInt32 dwClsContext, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
       [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

    private void RegElem(string name, UInt64 id, UInt16 typeID)
    {
      outputWindowPane.OutputString("[RegElem]; name: " + name + "\tid: " + id + "\ttypeID: " + typeID + "\n");
    }

    private void Trace(UInt64 id, double val)
    {
      outputWindowPane.OutputString("[Trace]; id: " + id + "\t: " + val + "\n");
    }

    public CPTraceHandler()
    {
      Globals.outputWindow.GetPane(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, out outputWindowPane);
      if (outputWindowPane != null)
        outputWindowPane.Activate();
      //UInt32 ret = CoInitialize(0);
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

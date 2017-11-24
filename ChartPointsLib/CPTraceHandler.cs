using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CPTracerLib;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace ChartPoints
{
  internal class TraceTransport
  {
    private static ICPTracerFactory tracersFactory;
    static Guid test_srv_CLSID = new Guid("EA343A3A-CF94-4210-89F5-9BDF56112CA2");
    static IDictionary<ulong, CPProcTracer> procTracers = new SortedDictionary<ulong, CPProcTracer>();

    public static bool Open()
    {
      if (tracersFactory == null)
      {
        Type test_srv_type = Type.GetTypeFromCLSID(test_srv_CLSID, true);
        Object obj = null;
        Guid IUnknownGuid = new Guid("00000000-0000-0000-C000-000000000046");
        obj = Activator.CreateInstance(test_srv_type);
        if (obj == null)
          return false;
        tracersFactory = obj as ICPTracerFactory;
        if (tracersFactory == null)
          return false;
      }

      return true;
    }

    public static void Close()
    {
      foreach (KeyValuePair<ulong, CPProcTracer> tracer in procTracers)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(tracer.Value);
      }
      procTracers.Clear();
      if (tracersFactory != null)
      {
        int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(tracersFactory);
        tracersFactory = null;
      }
      GC.Collect();
    }

    public static bool GetProcTracer(ulong id, out CPProcTracer tracer)
    {
      tracer = null;
      if (!procTracers.TryGetValue(id, out tracer) && tracersFactory != null)
      {
        tracersFactory.CreateProcTracer(out tracer, id);
        if (tracer != null)
        {
          procTracers.Add(id, tracer);

          return true;
        }
      }

      return false;
    }

    public static bool ReleaseProcTracer(ulong id, ref CPProcTracer tracer)
    {
      if (!procTracers.TryGetValue(id, out tracer))
        return false;

      procTracers.Remove(id);
      int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(tracer);//???FinalReleaseComObject???
      tracer = null;

      return true;
    }

  }

  class CPTraceHandler
  {
    private CPProcTracer procTracer;
    private IVsOutputWindowPane outputWindowPane;
    private ICPProcessTracer localProcTracer;
    public ulong id { get; }

    public CPTraceHandler(ulong _id, string name)
    {
      id = _id;
      ICPServiceProvider cpServProv = ICPServiceProvider.GetProvider();
      ICPTracerService traceServ;
      cpServProv.GetService<ICPTracerService>(out traceServ);
      traceServ.RegProcTracer(_id, name, out localProcTracer);

      Globals.outputWindow.GetPane(Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.DebugPane_guid, out outputWindowPane);
      if (outputWindowPane != null)
        outputWindowPane.Activate();
      if(TraceTransport.GetProcTracer(id, out procTracer))
      {
        procTracer.OnRegElem += RegElem;
        procTracer.OnTrace += Trace;
      }
    }

    private void RegElem(string name, UInt64 elem_id, UInt16 typeID)
    {
      //Debug.WriteLine("[*** CPTraceHandler::RegElem ***]; thread id: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
      //outputWindowPane.OutputString("$$$$$$$$$$[RegElem]; name: " + name + "\tid: " + elem_id + "\ttypeID: " + typeID + "\n");
      ICPTracerDelegate cpDelegate = localProcTracer?.RegTraceEnt(elem_id, name);
    }

    private void Trace(ulong elem_id, System.Array tms, System.Array vals)
    {
      //Debug.WriteLine("##################" + tms.Length.ToString());
      //Debug.WriteLine("[*** CPTraceHandler::Trace ***]; thread id: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
      localProcTracer?.Trace(elem_id, tms, vals);
    }

    public void Dispose()
    {
      //if (procTracer != null)
      //{
      //  int nRef = System.Runtime.InteropServices.Marshal.ReleaseComObject(procTracer);//???FinalReleaseComObject???
      //  procTracer = null;
      //}
      TraceTransport.ReleaseProcTracer(id, ref procTracer);
    }

  }
}

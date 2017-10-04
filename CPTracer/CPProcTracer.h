// CPProcTracer.h : Declaration of the CCPProcTracer

#pragma once
#include "resource.h"       // main symbols



#include "CPTracer_i.h"
#include "_ICPProcTracerEvents_CP.h"

#include <thread>
#include <atomic>
#include <queue>
#include <mutex>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;

class data_queue
{
public:
  typedef std::pair< uint64_t, double > data_ent;
  typedef std::queue< data_ent > data_cont;
private:
  data_cont data_1;
  data_cont data_2;
public:
  data_cont *in_ptr;
  data_cont *out_ptr;
  data_queue()
  {
    in_ptr = &data_1;
    out_ptr = &data_2;
  }
  void swap()
  {
    std::swap( in_ptr, out_ptr );
  }
};

// CCPProcTracer

class ATL_NO_VTABLE CCPProcTracer :
  public CComObjectRootEx<CComSingleThreadModel>,
  public CComCoClass<CCPProcTracer, &CLSID_CPProcTracer>,
  public IConnectionPointContainerImpl<CCPProcTracer>,
  public CProxy_ICPProcTracerEvents<CCPProcTracer>,
  public IDispatchImpl<ICPProcTracer, &IID_ICPProcTracer, &LIBID_CPTracerLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
  std::atomic_bool active;
  std::thread *consumer_thr;
  std::mutex mtx;
  //typedef std::queue<double> data_cont;
  //data_cont data;
  data_queue data;
  void cons_proc();
public:
  CCPProcTracer();
  ~CCPProcTracer();

  DECLARE_REGISTRY_RESOURCEID( IDR_CPPROCTRACER )


  BEGIN_COM_MAP( CCPProcTracer )
    COM_INTERFACE_ENTRY( ICPProcTracer )
    COM_INTERFACE_ENTRY( IDispatch )
    COM_INTERFACE_ENTRY( IConnectionPointContainer )
  END_COM_MAP()

  BEGIN_CONNECTION_POINT_MAP( CCPProcTracer )
    CONNECTION_POINT_ENTRY( __uuidof( _ICPProcTracerEvents ) )
  END_CONNECTION_POINT_MAP()


  DECLARE_PROTECT_FINAL_CONSTRUCT()

  HRESULT FinalConstruct()
  {
    return S_OK;
  }

  void FinalRelease()
  {
  }

public:



  STDMETHOD( RegElem )( BSTR name, ULONGLONG id, USHORT typeID );
  STDMETHOD( Trace )( ULONGLONG id, DOUBLE val );
};

OBJECT_ENTRY_AUTO( __uuidof( CPProcTracer ), CCPProcTracer )
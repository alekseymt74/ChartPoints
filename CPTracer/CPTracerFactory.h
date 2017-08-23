// CPTracerFactory.h : Declaration of the CCPTracerFactory

#pragma once
#include "resource.h"       // main symbols



#include "CPTracer_i.h"
#include "_ICPTracerFactoryEvents_CP.h"
#include <map>



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CCPTracerFactory

class ATL_NO_VTABLE CCPTracerFactory :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CCPTracerFactory, &CLSID_CPTracerFactory>,
  public IConnectionPointContainerImpl<CCPTracerFactory>,
  public CProxy_ICPTracerFactoryEvents<CCPTracerFactory>,
	public IDispatchImpl<ICPTracerFactory, &IID_ICPTracerFactory, &LIBID_CPTracerLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
  typedef std::map<ULONGLONG, CComPtr<ICPProcTracer> > tracers_cont;
  typedef tracers_cont::iterator it_tracer;
  tracers_cont tracers;
  //CComPtr<IUnknown> unk;
public:
	CCPTracerFactory()
	{
	}

  DECLARE_CLASSFACTORY_SINGLETON( CCPTracerFactory )
  DECLARE_REGISTRY_RESOURCEID( IDR_CPTRACERFACTORY )


BEGIN_COM_MAP(CCPTracerFactory)
	COM_INTERFACE_ENTRY(ICPTracerFactory)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(CCPTracerFactory)
	CONNECTION_POINT_ENTRY(__uuidof(_ICPTracerFactoryEvents))
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



  STDMETHOD( CreateProcTracer )( ICPProcTracer** procTracer, ULONGLONG id );
  STDMETHOD( GetProcTracer )( ICPProcTracer** procTracer, ULONGLONG id );
};

OBJECT_ENTRY_AUTO(__uuidof(CPTracerFactory), CCPTracerFactory)

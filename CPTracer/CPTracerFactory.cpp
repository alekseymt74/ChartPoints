// CPTracerFactory.cpp : Implementation of CCPTracerFactory

#include "stdafx.h"
#include "CPTracerFactory.h"


// CCPTracerFactory

CCPTracerFactory::~CCPTracerFactory()
{
  tracers.clear();
}

STDMETHODIMP CCPTracerFactory::CreateProcTracer( ICPProcTracer** procTracer, ULONGLONG id )
{
  IClientSecurity *pIClientSecurity;
  HRESULT _hr = this->QueryInterface( IID_IClientSecurity, (void**) & pIClientSecurity );
  if( SUCCEEDED( _hr ) )
  {
    ;
  }
  if( !procTracer )
    return E_INVALIDARG;
  *procTracer = NULL;
  HRESULT hr = S_OK;
  it_tracer it = tracers.find( id );
  //CComPtr<ICPProcTracer> tracer;
  ICPProcTracer *tracer;
  if( it == tracers.end() )
  {
    hr = CoCreateInstance( CLSID_CPProcTracer, 0, CLSCTX_LOCAL_SERVER, IID_IUnknown, (void **) &tracer );
    //hr = tracer.CoCreateInstance( CLSID_CPProcTracer, NULL, CLSCTX_LOCAL_SERVER );
    tracers[ id ] = tracer;
  }
  else
  {
    tracer = it->second;
    it->second->AddRef();
  }
  //tracer.CopyTo(procTracer);
  *procTracer = tracer;
  //tracer->AddRef();

  return hr;
}


STDMETHODIMP CCPTracerFactory::GetProcTracer( ICPProcTracer** procTracer, ULONGLONG id )
{
  if( !procTracer )
    return E_INVALIDARG;
  *procTracer = NULL;
  HRESULT hr = S_OK;
  it_tracer it = tracers.find( id );
  if( it != tracers.end() )
  {
    *procTracer = it->second;
    it->second->AddRef();
    //it->second.CopyTo( procTracer );
  }

  return hr;
}

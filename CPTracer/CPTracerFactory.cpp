// CPTracerFactory.cpp : Implementation of CCPTracerFactory

#include "stdafx.h"
#include "CPTracerFactory.h"


// CCPTracerFactory



STDMETHODIMP CCPTracerFactory::CreateProcTracer( ICPProcTracer** procTracer, ULONGLONG id )
{
  if( !procTracer )
    return E_INVALIDARG;
  *procTracer = NULL;
  HRESULT hr = S_OK;
  it_tracer it = tracers.find( id );
  CComPtr<ICPProcTracer> tracer;
  if( it == tracers.end() )
  {
    hr = CoCreateInstance( CLSID_CPProcTracer, 0, CLSCTX_ALL, IID_IUnknown, (void **) &tracer );
    tracers[ id ] = tracer;
  }
  else
    tracer = it->second;
  *procTracer = tracer;

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
    *procTracer = it->second;

  return hr;
}

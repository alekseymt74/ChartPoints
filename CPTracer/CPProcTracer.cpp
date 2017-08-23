// CPProcTracer.cpp : Implementation of CCPProcTracer

#include "stdafx.h"
#include "CPProcTracer.h"


// CCPProcTracer



STDMETHODIMP CCPProcTracer::RegElem( BSTR name, ULONGLONG id, USHORT typeID )
{
  Fire_OnRegElem( name, id, typeID );

  return S_OK;
}


STDMETHODIMP CCPProcTracer::Trace( ULONGLONG id, DOUBLE val )
{
  Fire_OnTrace( id, val );

  return S_OK;
}

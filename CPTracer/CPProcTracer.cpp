// CPProcTracer.cpp : Implementation of CCPProcTracer

#include "stdafx.h"
#include "CPProcTracer.h"
#include <intsafe.h>
#include <atlsafe.h>
#include <map>

// CCPProcTracer

std::chrono::system_clock::time_point CCPProcTracer::tm_start;

CCPProcTracer::CCPProcTracer()
  : consumer_thr( nullptr )
  , active( false )
{
  tm_start = std::chrono::system_clock::now();
}


STDMETHODIMP CCPProcTracer::RegElem( BSTR name, ULONGLONG id, USHORT typeID )
{
  if( !consumer_thr )
  {
    active = true;
    consumer_thr = new std::thread( std::bind( &CCPProcTracer::cons_proc, this ) );
  }
  Fire_OnRegElem( name, id, typeID );

  return S_OK;
}


STDMETHODIMP CCPProcTracer::Trace( ULONGLONG id, DOUBLE val )
{
  std::lock_guard< std::mutex > lock( mtx );
  std::chrono::system_clock::rep tm_ellapsed = std::chrono::duration_cast< std::chrono::milliseconds >( std::chrono::system_clock::now() - tm_start ).count();
  data.in_ptr->push( data_queue::data_ent( id, tm_ellapsed, val ) );

  return S_OK;
}

void CCPProcTracer::cons_proc()
{
  CoInitializeEx( NULL, COINIT_MULTITHREADED );
  data_queue::data_ent val;
  while( active )
  {
    {
      std::lock_guard< std::mutex > lock( mtx );
      data.swap();
    }
    HRESULT hr;
    typedef std::vector<TraceEnt> te_data;
    typedef std::map<uint64_t, te_data *> tes_data_cont;
    typedef tes_data_cont::iterator it_tes;
    tes_data_cont tes;
    while( data.out_ptr->size() )
    {
      val = data.out_ptr->front();
      data.out_ptr->pop();
      it_tes it = tes.find( val.id );
      if( it == tes.end() )
      {
        std::pair<it_tes, bool> it_ins = tes.insert( std::make_pair( val.id, new te_data() ) );
        it = it_ins.first;
      }
      TraceEnt trace_ent;
      trace_ent.tm = val.tm;
      trace_ent.val = val.val;
      it->second->push_back( trace_ent );
    }
    for( it_tes it = tes.begin(); it != tes.end(); ++it )
    {
      CComSafeArray<ULONGLONG> *tms = new CComSafeArray<ULONGLONG>( it->second->size() );
      CComSafeArray<DOUBLE> *vals = new CComSafeArray<DOUBLE>( it->second->size() );
      int i = 0;
      for( te_data::iterator it_te = it->second->begin(); it_te != it->second->end(); ++it_te )
      {
        ( *tms )[ i ] = it_te->tm;
        ( *vals )[ i ] = it_te->val;
        ++i;
      }
      Fire_OnTrace( it->first, *tms, *vals );
    }
    //if( data.out_ptr->size() )
    //{
    //  const int iLBound = 0;
    //  const int iUBound = data.out_ptr->size();
    //  SAFEARRAYBOUND rgbounds = { data.out_ptr->size(), 0 };
    //  LPSAFEARRAY psaTraceEnt = SafeArrayCreateEx( VT_RECORD, 1, &rgbounds, pRecInfo );
    //  TraceEnt *pTraceEntStruct = NULL;
    //  hr = SafeArrayAccessData( psaTraceEnt, ( void** ) &pTraceEntStruct );
    //  for( int i = 0; i < iUBound; i++ )
    //  {
    //    val = data.out_ptr->front();
    //    data.out_ptr->pop();
    //    pTraceEntStruct[ i ].tm = val.tm;
    //    pTraceEntStruct[ i ].val = val.val;
    //  }
    //  hr = SafeArrayUnaccessData( psaTraceEnt );
    //  Fire_OnTrace( psaTraceEnt );
    //  SafeArrayDestroy( psaTraceEnt );
    //}
    std::this_thread::sleep_for( std::chrono::milliseconds( 500 ) );
  }

  CoUninitialize();
}

CCPProcTracer::~CCPProcTracer()
{
  if( consumer_thr )
  {
    active = false;
    consumer_thr->join();
  }
}

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
    GUID GUID_TraceEnt_struct = __uuidof( TraceEnt );
    HRESULT hr;
    CComPtr<IRecordInfo> pRecInfo;//!!!!!!!!!!!!!! MOVE TO CLASS MEMBER !!!!!!!!!!!!
    hr = GetRecordInfoFromGuids( LIBID_CPTracerLib, 1, 0, 0, GUID_TraceEnt_struct, &pRecInfo );
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
      SAFEARRAYBOUND rgbounds = { it->second->size(), 0 };
      LPSAFEARRAY psaTraceEnt = SafeArrayCreateEx( VT_RECORD, 1, &rgbounds, pRecInfo );
      TraceEnt *pTraceEntStruct = NULL;
      hr = SafeArrayAccessData( psaTraceEnt, ( void** ) &pTraceEntStruct );
      memcpy( pTraceEntStruct, it->second->data(), sizeof( TraceEnt ) * it->second->size() );
      //int i = 0;
      //for( te_data::iterator it_te = it->second->begin(); it_te != it->second->end(); ++it_te )
      //{
      //  pTraceEntStruct[ i ].tm = it_te->tm;
      //  pTraceEntStruct[ i ].val = it_te->val;
      //  ++i;
      //}
      hr = SafeArrayUnaccessData( psaTraceEnt );
      Fire_OnTrace( it->first, psaTraceEnt );
      SafeArrayDestroy( psaTraceEnt );
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

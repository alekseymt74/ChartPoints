// CPProcTracer.cpp : Implementation of CCPProcTracer

#include "stdafx.h"
#include "CPProcTracer.h"
#include <intsafe.h>
#include <atlsafe.h>

// CCPProcTracer

CCPProcTracer::CCPProcTracer()
  : consumer_thr( nullptr )
  , active( false )
{
}

STDMETHODIMP CCPProcTracer::RegElem( BSTR name, ULONGLONG id, USHORT typeID )
{
  //CoInitializeEx( NULL, COINIT_MULTITHREADED );
  if( !consumer_thr )
  {
    active = true;
    consumer_thr = new std::thread( std::bind( &CCPProcTracer::cons_proc, this ) );
  }
  std::unique_lock<std::mutex> lock( send_trace_queue_mtx );
  it_tes it = tes.find( id );
  if( it == tes.end() )
  {
    std::pair<it_tes, bool> it_ins = tes.insert( std::make_pair( id, std::make_shared<te_data>() ) );
    it = it_ins.first;
  }
  send();
  lock.unlock();
  Fire_OnRegElem( CComBSTR(name), id, typeID );
  //CoUninitialize();

  return S_OK;
}


STDMETHODIMP CCPProcTracer::Trace( ULONGLONG id, ULONGLONG tm, DOUBLE val )
{
  //CoInitializeEx( NULL, COINIT_MULTITHREADED );
  std::lock_guard< std::mutex > lock( mtx );
  data.in_ptr->emplace/*push*/( data_ent( id, tm, val ) );
  //CoUninitialize();

  return S_OK;
}

void CCPProcTracer::send()
{
  //std::unique_lock<std::mutex> lock( send_trace_queue_mtx );
  
  data_ent val;
  {
    std::lock_guard< std::mutex > lock(mtx);
    data.swap();
  }
  //tes_data_cont tes;
  while (data.out_ptr->size())
  {
    val = data.out_ptr->front();
    it_tes it = tes.find(val.id);
    if (it != tes.end())
    {
      data.out_ptr->pop();
      TraceEnt trace_ent;
      trace_ent.tm = val.tm;
      trace_ent.val = val.val;
      it->second->push_back(trace_ent);
    }
  }
  for (it_tes it = tes.begin(); it != tes.end(); ++it)
  {
    if (it->second->size())
    {
      CComSafeArray<ULONGLONG> *tms = new CComSafeArray<ULONGLONG>(it->second->size());
      CComSafeArray<DOUBLE> *vals = new CComSafeArray<DOUBLE>(it->second->size());
      int i = 0;
      for (te_data::iterator it_te = it->second->begin(); it_te != it->second->end(); ++it_te)
      {
        (*tms)[i] = it_te->tm;
        (*vals)[i] = it_te->val;
        ++i;
      }
      it->second->clear();
      Fire_OnTrace(it->first, *tms, *vals);
    }
  }
}

void CCPProcTracer::cons_proc()
{
  CoInitializeEx( NULL, COINIT_MULTITHREADED );
  while( active )
  {
    {
      std::unique_lock<std::mutex> lock( send_trace_queue_mtx );
      send();
    }
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
    delete consumer_thr;
  }
}

// CPProcTracer.cpp : Implementation of CCPProcTracer

#include "stdafx.h"
#include "CPProcTracer.h"
#include <intsafe.h>
#include <atlsafe.h>

// CCPProcTracer

//std::chrono::system_clock::time_point CCPProcTracer::tm_start;

CCPProcTracer::CCPProcTracer()
  : consumer_thr( nullptr )
  , active( false )
{
  //tm_start = std::chrono::system_clock::now();
}


STDMETHODIMP CCPProcTracer::RegElem( BSTR name, ULONGLONG id, USHORT typeID )
{
  CComBSTR _name( name );
  std::thread thr( [ = ]()
  {
    std::lock_guard< std::mutex > lock1( mtx1 );
    send();
    if( !consumer_thr )
    {
      active = true;
      consumer_thr = new std::thread( std::bind( &CCPProcTracer::cons_proc, this ) );
    }
    it_tes it = tes.find( id );
    if( it == tes.end() )
    {
      std::pair<it_tes, bool> it_ins = tes.insert( std::make_pair( id, std::make_shared<te_data>() ) );
      it = it_ins.first;
    }
    //else
    //  send();
    Fire_OnRegElem( _name, id, typeID );
  } );
  thr.detach();

  return S_OK;
}


STDMETHODIMP CCPProcTracer::Trace( ULONGLONG id, ULONGLONG tm, DOUBLE val )
{
  std::lock_guard< std::mutex > lock( mtx );
  data.in_ptr->emplace/*push*/( data_queue::data_ent( id, tm, val ) );

  return S_OK;
}

void CCPProcTracer::send()
{
  data_queue::data_ent val;
  //std::lock_guard< std::mutex > lock1(mtx1);
  {
    std::lock_guard< std::mutex > lock(mtx);
    data.swap();
  }
  //tes_data_cont tes;
  while (data.out_ptr->size())
  {
    val = data.out_ptr->front();
    data.out_ptr->pop();
    it_tes it = tes.find(val.id);
    //if( it == tes.end() )
    //{
    //  std::pair<it_tes, bool> it_ins = tes.insert( std::make_pair( val.id, std::make_shared<te_data>() ) );
    //  it = it_ins.first;
    //}
    if (it != tes.end())
    {
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
    send();
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

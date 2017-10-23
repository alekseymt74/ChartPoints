// CPProcTracer.cpp : Implementation of CCPProcTracer

#include "stdafx.h"
#include "CPProcTracer.h"
#include <intsafe.h>
#include <atlsafe.h>

//const IID TraceEnt_IID = { 0x26E0450F,
//  0x7D1D,
//  0x4189,{
//    0x87,
//    0x35,
//    0xC0,
//    0xE8,
//    0x7B,
//    0x3A,
//    0x5E,
//    0xCD
//  }
//  
////  0xC21871A0,
////0x33EB,
////0x11D4,{
////  0xA1,
////  0x3A,
////  0xBE,
////  0x25,
////  0x73,
////  0xA1,
////  0x12,
////  0x0F
////}
//};

// CCPProcTracer

CCPProcTracer::CCPProcTracer()
  : consumer_thr(nullptr)
  , active(false)
{
}


STDMETHODIMP CCPProcTracer::RegElem(BSTR name, ULONGLONG id, USHORT typeID)
{
  if (!consumer_thr)
  {
    active = true;
    consumer_thr = new std::thread(std::bind(&CCPProcTracer::cons_proc, this));
  }
  Fire_OnRegElem(name, id, typeID);

  return S_OK;
}


STDMETHODIMP CCPProcTracer::Trace(ULONGLONG id, DOUBLE val)
{
  std::lock_guard< std::mutex > lock(mtx);
  data.in_ptr->push(std::make_pair(id, val));

  return S_OK;
}

void CCPProcTracer::cons_proc()
{
  CoInitializeEx(NULL, COINIT_MULTITHREADED);
  data_queue::data_ent val;
  while (active)
  {
    {
      std::lock_guard< std::mutex > lock(mtx);
      data.swap();
    }
    {
      if (data.out_ptr->size())
      {
        //LPTYPEINFO pTypeInfo = NULL;
        //LPTYPELIB pTypelib = NULL;
        //LPSAFEARRAY psaStudent = NULL;
        //SAFEARRAYBOUND rgbounds = { data.out_ptr->size(), 0 };
        //TraceEnt *pStudentStruct = NULL;
        //IRecordInfo* pRecInfo = NULL;

        ////// Fetch the IRecordInfo interface describing the UDT
        ////HRESULT hr = LoadRegTypeLib(LIBID_CPTracerLib, 1, 0, GetUserDefaultLCID(), &pTypelib);
        ////_ASSERT(SUCCEEDED(hr) && pTypelib);

        ////hr = pTypelib->GetTypeInfoOfGuid(TraceEnt_IID, &pTypeInfo);
        ////_ASSERT(SUCCEEDED(hr) && pTypeInfo);
        ////hr = GetRecordInfoFromTypeInfo(pTypeInfo, &pRecInfo);
        ////_ASSERT(SUCCEEDED(hr) && pRecInfo);
        //////RELEASEINTERFACE(pTypeInfo);
        ////pTypeInfo->Release();
        //////RELEASEINTERFACE(pTypelib);
        ////pTypelib->Release();
        //HRESULT hr = ::GetRecordInfoFromGuids(LIBID_CPTracerLib,
        //  1, 0,
        //  0,
        //  TraceEnt_IID,
        //  &pRecInfo);
        //if (FAILED(hr)) {
        //  HRESULT hr2 = Error(_T("Can not create RecordInfo interface for UDTVariable"));
        //  //return( hr2 );
        //}
        //psaStudent = SafeArrayCreateEx(VT_RECORD, 1, &rgbounds, pRecInfo);
        ////RELEASEINTERFACE(pRecInfo);
        //pRecInfo->Release();
        //_ASSERT(psaStudent);
        //hr = SafeArrayAccessData(psaStudent, reinterpret_cast<PVOID*>(&pStudentStruct));
        //_ASSERT(SUCCEEDED(hr) && pStudentStruct);
        //int i = 0;
        //while( data.out_ptr->size() )
        //{
        //  val = data.out_ptr->front();
        //  data.out_ptr->pop();
        //  TraceEnt ent;
        //  ent.id = val.first;
        //  ent.val = val.second;
        //  pStudentStruct[ i ].id = val.first;
        //  pStudentStruct[ i++ ].val = val.second;
        //}
        //SafeArrayUnaccessData( psaStudent );
        //Fire_OnTrace(psaStudent);

        //////CComSafeArray<ULONGLONG> *ids = new CComSafeArray<ULONGLONG>(data.out_ptr->size());
        //////CComSafeArray<DOUBLE> *vars = new CComSafeArray<DOUBLE>(data.out_ptr->size());
        //////int i = 0;
        //////while (data.out_ptr->size())
        //////{
        //////  val = data.out_ptr->front();
        //////  data.out_ptr->pop();
        //////  //TraceEnt ent;
        //////  //ent.id = val.first;
        //////  //ent.val = val.second;
        //////  (*ids)[i] = val.first;
        //////  (*vars)[i++] = val.second;
        //////}
        //////Fire_OnTrace(*ids, *vars);
        //####################################################
        GUID GUID_TraceEnt_struct = __uuidof( TraceEnt );
        HRESULT hr;
        CComPtr<IRecordInfo> pRecInfo;
        hr = GetRecordInfoFromGuids( LIBID_CPTracerLib, 1, 0, 0,
          GUID_TraceEnt_struct, &pRecInfo );
        const int iLBound = 0;
        const int iUBound = data.out_ptr->size();
        SAFEARRAYBOUND rgbounds = { data.out_ptr->size(), 0 };
        LPSAFEARRAY psaTraceEnt = SafeArrayCreateEx( VT_RECORD, 1, &rgbounds, pRecInfo );
        TraceEnt *pTraceEntStruct = NULL;
        hr = SafeArrayAccessData( psaTraceEnt, ( void** ) &pTraceEntStruct );
        for( int i = 0; i < iUBound; i++ )
        {
          val = data.out_ptr->front();
          data.out_ptr->pop();
          pTraceEntStruct[ i ].id = val.first;
          pTraceEntStruct[ i ].val = val.second;
        }
        hr = SafeArrayUnaccessData( psaTraceEnt );
        Fire_OnTrace( psaTraceEnt );
        SafeArrayDestroy( psaTraceEnt );
        //####################################################
      }
      std::this_thread::sleep_for(std::chrono::milliseconds(500));
    }
  }
  CoUninitialize();
}

CCPProcTracer::~CCPProcTracer()
{
  if (consumer_thr)
  {
    active = false;
    consumer_thr->join();
  }
}

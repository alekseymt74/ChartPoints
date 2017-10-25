#pragma once

#include <stdint.h>

const IID TraceEnt_IID = { 0x26E0450F,
0x7D1D,
0x4189,{
  0x87,
  0x35,
  0xC0,
  0xE8,
  0x7B,
  0x3A,
  0x5E,
  0xCD
}

//  0xC21871A0,
//0x33EB,
//0x11D4,{
//  0xA1,
//  0x3A,
//  0xBE,
//  0x25,
//  0x73,
//  0xA1,
//  0x12,
//  0x0F
//}
};

template<class T>
class CProxy_ICPProcTracerEvents :
  public ATL::IConnectionPointImpl<T, &__uuidof( _ICPProcTracerEvents )>
{
public:
  HRESULT Fire_OnRegElem( BSTR name, ULONGLONG id, USHORT typeID )
  {
    HRESULT hr = S_OK;
    T * pThis = static_cast< T * >( this );
    int cConnections = m_vec.GetSize();

    for( int iConnection = 0; iConnection < cConnections; iConnection++ )
    {
      pThis->Lock();
      CComPtr<IUnknown> punkConnection = m_vec.GetAt( iConnection );
      pThis->Unlock();

      IDispatch * pConnection = static_cast< IDispatch * >( punkConnection.p );

      if( pConnection )
      {
        CComVariant avarParams[ 3 ];
        avarParams[ 2 ] = name;
        avarParams[ 2 ].vt = VT_BSTR;
        avarParams[ 1 ] = id;
        avarParams[ 1 ].vt = VT_UI8;
        avarParams[ 0 ] = typeID;
        avarParams[ 0 ].vt = VT_UI2;
        CComVariant varResult;

        DISPPARAMS params = { avarParams, NULL, 3, 0 };
        hr = pConnection->Invoke( 1, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &params, &varResult, NULL, NULL );
      }
    }
    return hr;
  }
  //	HRESULT Fire_OnTrace(SAFEARRAY *pVars)
  //	{
  //		HRESULT hr = S_OK;
  //		T * pThis = static_cast<T *>(this);
  //		int cConnections = m_vec.GetSize();
  //
  //		for (int iConnection = 0; iConnection < cConnections; iConnection++)
  //		{
  //			pThis->Lock();
  //			CComPtr<IUnknown> punkConnection = m_vec.GetAt(iConnection);
  //			pThis->Unlock();
  //
  //			IDispatch * pConnection = static_cast<IDispatch *>(punkConnection.p);
  //
  //			if (pConnection)
  //			{
  //        CComVariant varResult;
  //        //VariantClear(&varResult);
  //        CComVariant avarParams;//[ 1 ];
  //        VARIANT var;
  //
  //        //VariantInit(&var);
  //        //V_VT(&var) = VT_ARRAY | VT_RECORD;
  //        //V_ARRAY(&var) = pVars;
  //        avarParams/*[0]*/.vt = /*VT_VARIANT | VT_BYREF;//*/VT_ARRAY /*| VT_BYREF */| VT_RECORD;
  //        avarParams/*[0]*/./*pvarVal*//*pvRecord*/parray = /*&var;//*/pVars;
  /////////////////////////
  //        //IRecordInfo* pRecInfo = NULL;
  //        //HRESULT hr1 = ::GetRecordInfoFromGuids(LIBID_CPTracerLib,
  //        //  1, 0,
  //        //  0,
  //        //  TraceEnt_IID,
  //        //  &pRecInfo);
  //        //avarParams.pRecInfo = pRecInfo;
  /////////////////////////
  //        DISPPARAMS params = { &avarParams, NULL, 1, 0 };
  //        EXCEPINFO  pExcepInfo;
  //        UINT       puArgErr;
  //        hr = pConnection->Invoke(2, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &params, &varResult, &pExcepInfo, &puArgErr);
  //				//CComVariant avarParams[2];
  //				//avarParams[1] = id;
  //				//avarParams[1].vt = VT_UI8;
  //				//avarParams[0] = vals;
  //    //    avarParams[ 0 ].vt = VT_ARRAY | VT_R8;// VT_R8;
  //				//CComVariant varResult;
  //
  //				//DISPPARAMS params = { avarParams, NULL, 2, 0 };
  //				//hr = pConnection->Invoke(2, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &params, &varResult, NULL, NULL);
  //			}
  //		}
  //		return hr;
  //	}
  HRESULT Fire_OnTrace( uint64_t id, SAFEARRAY *psaTraceEnt )//SAFEARRAY *ids, SAFEARRAY *vars)
  {
    //HRESULT hr = S_OK;
    //T * pThis = static_cast<T *>(this);
    //int cConnections = m_vec.GetSize();

    //for (int iConnection = 0; iConnection < cConnections; iConnection++)
    //{
    //  pThis->Lock();
    //  CComPtr<IUnknown> punkConnection = m_vec.GetAt(iConnection);
    //  pThis->Unlock();

    //  IDispatch * pConnection = static_cast<IDispatch *>(punkConnection.p);

    //  if (pConnection)
    //  {
    //    CComVariant avarParams[2];
    //    avarParams[1] = ids;
    //    avarParams[1].vt = VT_ARRAY | VT_UI8;
    //    avarParams[0] = vars;
    //    avarParams[ 0 ].vt = VT_ARRAY | VT_R8;// VT_R8;
    //    CComVariant varResult;

    //    DISPPARAMS params = { avarParams, NULL, 2, 0 };
    //    hr = pConnection->Invoke(2, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &params, &varResult, NULL, NULL);
    //  }
    //}
    //return hr;
    HRESULT hr = S_OK;
    T * pThis = static_cast< T * >( this );
    int cConnections = m_vec.GetSize();

    for( int iConnection = 0; iConnection < cConnections; iConnection++ )
    {
      pThis->Lock();
      CComPtr<IUnknown> punkConnection = m_vec.GetAt( iConnection );
      pThis->Unlock();

      IDispatch * pConnection = static_cast< IDispatch * >( punkConnection.p );

      if( pConnection )
      {
        CComVariant avarParams[ 2 ];
        avarParams[ 1 ] = id;
        avarParams[ 1 ].vt = VT_UI8;
        avarParams[ 0 ] = psaTraceEnt;
        avarParams[ 0 ].vt = VT_ARRAY | VT_RECORD;
        CComVariant varResult;

        DISPPARAMS params = { avarParams, NULL, 2, 0 };
        EXCEPINFO  pExcepInfo;
        UINT       puArgErr;
        hr = pConnection->Invoke( 2, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &params, &varResult, &pExcepInfo, &puArgErr );
      }
    }

    return hr;
  }
};


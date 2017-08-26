// CPTracer.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#define _ATL_DEBUG_INTERFACES
#include "CPTracer_i.h"
#include "xdlldata.h"


using namespace ATL;


class CCPTracerModule : public ATL::CAtlExeModuleT< CCPTracerModule >
{
public :
	DECLARE_LIBID(LIBID_CPTracerLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_CPTRACER, "{AD7685A6-CE37-41F9-808C-23DB98B0596A}")
  /*HRESULT RegisterClassObjects(
    _In_ DWORD dwClsContext,
    _In_ DWORD dwFlags ) throw( )
  {
    return AtlComModuleRegisterClassObjects( &_AtlComModule, CLSCTX_LOCAL_SERVER, REGCLS_SINGLEUSE | REGCLS_SUSPENDED );
  }*/
};

CCPTracerModule _AtlModule;



//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/, 
								LPTSTR /*lpCmdLine*/, int nShowCmd)
{
	return _AtlModule.WinMain(nShowCmd);
}


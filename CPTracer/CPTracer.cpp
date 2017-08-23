// CPTracer.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#include "CPTracer_i.h"
#include "xdlldata.h"


using namespace ATL;


class CCPTracerModule : public ATL::CAtlExeModuleT< CCPTracerModule >
{
public :
	DECLARE_LIBID(LIBID_CPTracerLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_CPTRACER, "{AD7685A6-CE37-41F9-808C-23DB98B0596A}")
	};

CCPTracerModule _AtlModule;



//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/, 
								LPTSTR /*lpCmdLine*/, int nShowCmd)
{
	return _AtlModule.WinMain(nShowCmd);
}


#include "tracer.h"
#include <iostream>
#include <chrono>
#include "../../CPTracer/CPTracer_i.h"
#include <atlcomcli.h>

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}
MIDL_DEFINE_GUID(CLSID, CLSID_CPTracerFactory, 0xEA343A3A, 0xCF94, 0x4210, 0x89, 0xF5, 0x9B, 0xDF, 0x56, 0x11, 0x2C, 0xA2);

namespace cptracer
{

  class tracer_impl : public tracer
  {
    CComPtr< ICPTracerFactory > trace_cons;
    CComPtr< ICPProcTracer > trace_elem_cons;
    static std::chrono::high_resolution_clock::time_point tm_start;
  public:
    tracer_impl();
    ~tracer_impl();
    void reg_elem(const char *name, uint64_t id, uint32_t _type_id) const override;
    void trace(uint64_t id, double val) const override;
  };

  std::chrono::high_resolution_clock::time_point tracer_impl::tm_start;

  tracer_impl::tracer_impl()
    : trace_elem_cons(nullptr)
  {
    HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
    hr = trace_cons.CoCreateInstance(CLSID_CPTracerFactory, NULL, CLSCTX_LOCAL_SERVER);
    if (hr == S_OK)
      hr = trace_cons->GetProcTracer(&trace_elem_cons, 1);
    tm_start = std::chrono::high_resolution_clock::now();
  }

  tracer_impl::~tracer_impl()
  {
    try
    {
      CoUninitialize();
    }
    catch (...)
    {
    }
}

  void tracer_impl::reg_elem(const char *name, uint64_t id, uint32_t _type_id) const
  {
    try
    {
      USES_CONVERSION;
      //std::cout << "[reg_elem]; name: " << te->get_name() << "\tid: " << te->get_id() << "\ttype_id: " << _type_id << std::endl;
      if (trace_elem_cons)
        trace_elem_cons->RegElem(CComBSTR(A2W(name)).Detach(), id, _type_id);
    }
    catch (...)
    {
    }
  }

  void tracer_impl::trace(uint64_t id, double val) const
  {
    try
    {
      // std::cout << id << ": " << val << std::endl;
      if (trace_elem_cons)
        trace_elem_cons->Trace(id, std::chrono::duration_cast< std::chrono::milliseconds >( std::chrono::high_resolution_clock::now() - tm_start ).count(), val);
    }
    catch (...)
    {
    }
  }

  CPTRACER_DLL_API bool create_tracer(tracer::tracer_ptr &_tracer)
  {
    try
    {
      _tracer = std::make_shared<tracer_impl>();

      return true;
    }
    catch (...)
    {
      _tracer = nullptr;
    }

    return false;
  }

} // namespace cptracer

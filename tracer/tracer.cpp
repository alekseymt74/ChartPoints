#include "tracer.h"
#include <atlcomcli.h>
#include "../CPTracer/CPTracer_i.h"

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}
MIDL_DEFINE_GUID( CLSID, CLSID_CPTracerFactory, 0xEA343A3A, 0xCF94, 0x4210, 0x89, 0xF5, 0x9B, 0xDF, 0x56, 0x11, 0x2C, 0xA2 );

namespace cptracer
{
  
  int32_t type_id<int8_t>::id = 0;
  int32_t type_id<int16_t>::id = 1;
  int32_t type_id<int32_t>::id = 2;

  class tracer
  {
    CComPtr< ICPTracerFactory > trace_cons;
    ICPProcTracer *trace_elem_cons;
  public:
    typedef std::shared_ptr<tracer> tracer_ptr;
    static tracer_ptr _this;
    tracer();
    ~tracer();
    static tracer_ptr instance();
    void reg_elem( const tracer_elem *te, uint32_t _type_id ) const;
    void trace( uint64_t id, double val ) const;
  };

  tracer::tracer()
    : trace_elem_cons( nullptr )
  {
    tracer::instance();
    HRESULT hr = CoInitialize( NULL );
    hr = trace_cons.CoCreateInstance( CLSID_CPTracerFactory, NULL, CLSCTX_LOCAL_SERVER );
    if( hr == S_OK )
      hr = trace_cons->GetProcTracer( &trace_elem_cons, 1 );
  }

  tracer::~tracer()
  {
    trace_elem_cons->Release();
    trace_cons.Release();
  }
    
  tracer::tracer_ptr tracer::instance()
  {
    if( !_this )
      _this = std::make_shared<tracer>();
    return _this;
  }

  void tracer::reg_elem( const tracer_elem *te, uint32_t _type_id ) const
  {
    //std::cout << "[reg_elem]; name: " << te->get_name() << "\tid: " << te->get_id() << "\ttype_id: " << _type_id << std::endl;
    trace_elem_cons->RegElem( SysAllocStringByteLen( te->get_name().c_str(), te->get_name().size() ), te->get_id(), _type_id );
  }

  void tracer::trace( uint64_t id, double val ) const
  {
    // std::cout << id << ": " << val << std::endl;
    if( trace_elem_cons )
      trace_elem_cons->Trace( id, val );
  }

  tracer::tracer_ptr tracer::_this;

  tracer_elem::tracer_elem() {}

  tracer_elem::tracer_elem( uint64_t _addr, const char *_name, uint32_t _type_id ) : addr( _addr ), name( _name )
  {
    tracer::instance()->reg_elem( this, _type_id );
  }

  void tracer_elem::reg( uint64_t _addr, const char *_name, uint32_t _type_id )
  {
    tracer::instance()->reg_elem( this, _type_id );
  }

  void tracer_elem::trace( double val ) const
  {
    tracer::instance()->trace( get_id(), val );
  }

} // namespace cptracer


#include "__cp__.tracer.h"
#include <iostream>
#include <chrono>
#include "__cp__.CPTracer_i.h"
#include <atlcomcli.h>

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}
MIDL_DEFINE_GUID( CLSID, CLSID_CPTracerFactory, 0xEA343A3A, 0xCF94, 0x4210, 0x89, 0xF5, 0x9B, 0xDF, 0x56, 0x11, 0x2C, 0xA2 );

namespace cptracer
{

  //int32_t type_id< short >::id                  = 0;
  int32_t type_id< short int >::id              = 0;
  //int32_t type_id< signed short >::id           = 0;
  //int32_t type_id< signed short int >::id       = 0;
  //int32_t type_id< unsigned short >::id         = 1;
  int32_t type_id< unsigned short int >::id     = 1;
  int32_t type_id< int >::id                    = 2;
  //int32_t type_id< signed >::id                 = 2;
  //int32_t type_id< signed int >::id             = 2;
  //int32_t type_id< unsigned >::id               = 3;
  int32_t type_id< unsigned int >::id           = 3;
  //int32_t type_id< long >::id                   = 4;
  int32_t type_id< long int >::id               = 4;
  //int32_t type_id< signed long >::id            = 4;
  //int32_t type_id< signed long int >::id        = 4;
  //int32_t type_id< unsigned long >::id          = 5;
  int32_t type_id< unsigned long int >::id      = 5;
  //int32_t type_id< long long >::id              = 6;
  int32_t type_id< long long int >::id          = 6;
  //int32_t type_id< signed long long >::id       = 6;
  //int32_t type_id< signed long long int >::id   = 6;
  //int32_t type_id< unsigned long long >::id     = 7;
  int32_t type_id< unsigned long long int >::id = 7;
  int32_t type_id< double >::id                 = 8;
  int32_t type_id< float >::id                  = 9;
  int32_t type_id< bool >::id                   = 10;
  int32_t type_id< signed char >::id            = 11;
  int32_t type_id< char >::id                   = 12;
  int32_t type_id< unsigned char >::id          = 13;

  class tracer
  {
    CComPtr< ICPTracerFactory > trace_cons;
    CComPtr< ICPProcTracer > trace_elem_cons;
    static std::chrono::high_resolution_clock::time_point tm_start;
  public:
    typedef std::shared_ptr<tracer> tracer_ptr;
    tracer();
    ~tracer();
    static tracer_ptr instance();
    void reg_elem( const tracer_elem *te, uint32_t _type_id ) const;
    void trace( uint64_t id, double val ) const;
  };

  std::chrono::high_resolution_clock::time_point tracer::tm_start;

  tracer::tracer()
    : trace_elem_cons( nullptr )
  {
    HRESULT hr = CoInitializeEx( NULL, COINIT_MULTITHREADED );
    hr = trace_cons.CoCreateInstance( CLSID_CPTracerFactory, NULL, CLSCTX_LOCAL_SERVER );
    if( hr == S_OK )
      hr = trace_cons->GetProcTracer( &trace_elem_cons, 1 );
    tm_start = std::chrono::high_resolution_clock::now();
  }

  tracer::~tracer()
  {
    CoUninitialize();
    //if( trace_elem_cons )
    //  trace_elem_cons->Release();
    //if( trace_cons )
    //  trace_cons.Release();
  }

  tracer::tracer_ptr tracer::instance()
  {
    static tracer_ptr _this = std::make_shared<tracer>();

    return _this;
  }

  void tracer::reg_elem( const tracer_elem *te, uint32_t _type_id ) const
  {
    USES_CONVERSION;
    //std::cout << "[reg_elem]; name: " << te->get_name() << "\tid: " << te->get_id() << "\ttype_id: " << _type_id << std::endl;
    if( trace_elem_cons )
      trace_elem_cons->RegElem( CComBSTR( A2W( te->get_name().c_str() ) ).Detach()/*SysAllocStringByteLen( te->get_name().c_str(), te->get_name().size() )*/, te->get_id(), _type_id );
  }

  void tracer::trace( uint64_t id, double val ) const
  {
    // std::cout << id << ": " << val << std::endl;
    if( trace_elem_cons )
    {
      std::chrono::high_resolution_clock::rep tm_ellapsed = std::chrono::duration_cast< std::chrono::milliseconds >( std::chrono::high_resolution_clock::now() - tm_start ).count();
      trace_elem_cons->Trace( id, tm_ellapsed, val );
    }
  }


  tracer_elem::tracer_elem() {}

  tracer_elem::tracer_elem( uint64_t _addr, const char *_name, uint32_t _type_id ) : addr( _addr ), name( _name )
  {
    tracer::instance()->reg_elem( this, _type_id );
  }

  void tracer_elem::reg( uint64_t _addr, const char *_name, uint32_t _type_id )
  {
    addr = _addr;
    name = _name;
    tracer::instance()->reg_elem( this, _type_id );
  }

  void tracer_elem::trace( double val ) const
  {
    tracer::instance()->trace( get_id(), val );
  }

} // namespace cptracer

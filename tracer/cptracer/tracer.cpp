#include "tracer.h"
#include <windows.h>

namespace cptracer
{

  //int32_t type_id< short >::id                  = 0;
  int32_t type_id< short int >::id = 0;
  //int32_t type_id< signed short >::id           = 0;
  //int32_t type_id< signed short int >::id       = 0;
  //int32_t type_id< unsigned short >::id         = 1;
  int32_t type_id< unsigned short int >::id = 1;
  int32_t type_id< int >::id = 2;
  //int32_t type_id< signed >::id                 = 2;
  //int32_t type_id< signed int >::id             = 2;
  //int32_t type_id< unsigned >::id               = 3;
  int32_t type_id< unsigned int >::id = 3;
  //int32_t type_id< long >::id                   = 4;
  int32_t type_id< long int >::id = 4;
  //int32_t type_id< signed long >::id            = 4;
  //int32_t type_id< signed long int >::id        = 4;
  //int32_t type_id< unsigned long >::id          = 5;
  int32_t type_id< unsigned long int >::id = 5;
  //int32_t type_id< long long >::id              = 6;
  int32_t type_id< long long int >::id = 6;
  //int32_t type_id< signed long long >::id       = 6;
  //int32_t type_id< signed long long int >::id   = 6;
  //int32_t type_id< unsigned long long >::id     = 7;
  int32_t type_id< unsigned long long int >::id = 7;
  int32_t type_id< double >::id = 8;
  int32_t type_id< float >::id = 9;
  int32_t type_id< bool >::id = 10;
  int32_t type_id< signed char >::id = 11;
  int32_t type_id< char >::id = 12;
  int32_t type_id< unsigned char >::id = 13;

  class temp
  {
  public:
    static HMODULE h_mod;
  };
  HMODULE temp::h_mod = NULL;

  tracer::tracer_ptr create_tracer_impl(tracer::tracer_ptr &_tracer)
  {
    try
    {
#ifdef _UNICODE 
      temp::h_mod = LoadLibrary(TEXT("e:/projects/tests/MSVS.ext/ChartPoints/tracer/Release/cptracer.dll"));
#else
      temp::h_mod = LoadLibrary("e:/projects/tests/MSVS.ext/ChartPoints/tracer/Release/cptracer.dll");
#endif
      if(temp::h_mod == NULL)
        return nullptr;
      create_tracer_func get_tracer = reinterpret_cast<create_tracer_func>(GetProcAddress(temp::h_mod, "create_tracer"));
      if (get_tracer == NULL)
        return nullptr;
      bool ret = (*get_tracer)(_tracer);
      if( !ret )
        return nullptr;
    }
    catch(...)
    {
      _tracer = nullptr;
    }

    return _tracer;
  }

  tracer::~tracer()
  {
    if(temp::h_mod != NULL)
      FreeLibrary(temp::h_mod);
  }

  tracer::tracer_ptr tracer::instance()
  {
    static tracer_ptr _temp;
    static tracer_ptr _this = create_tracer_impl(_temp);

    return _this;
  }


} // namespace cptracer

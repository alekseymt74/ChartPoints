#include "__cp__.tracer.h"
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

  class cpt_loader
  {
    HMODULE h_mod;
  public:
    cpt_loader() : h_mod(NULL){}
    ~cpt_loader()
    {
      if (h_mod != NULL)
        FreeLibrary(h_mod);
    }
    tracer::tracer_ptr load()
    {
      tracer::tracer_ptr _tracer;
      try
      {
#ifdef _WIN64
#define DLLNAME "cpti64.dll"
#else //_WIN32
#define DLLNAME "cpti.dll"
#endif

#define PATH2DLL ""

#define FULLNAME PATH2DLL ## "\\" ## DLLNAME

#ifdef _UNICODE 
        h_mod = LoadLibrary(TEXT(FULLNAME));
#else
        h_mod = LoadLibrary(FULLNAME);
#endif
        if (h_mod == NULL)
          return nullptr;
        create_tracer_func get_tracer = reinterpret_cast<create_tracer_func>(GetProcAddress(h_mod, "create_tracer"));
        if (get_tracer == NULL)
          return nullptr;
        bool ret = (*get_tracer)(_tracer);
        if (!ret)
          return nullptr;
      }
      catch (...)
      {
        _tracer = nullptr;
        h_mod = NULL;
      }

      return _tracer;
    }
  };
  typedef std::shared_ptr<cpt_loader> cpt_loader_ptr;

  tracer::~tracer()
  {
  }

  tracer::tracer_ptr tracer::instance()
  {
    static cpt_loader_ptr _loader = std::make_shared<cpt_loader>();
    static tracer_ptr _this = _loader->load();

    return _this;
  }


} // namespace cptracer

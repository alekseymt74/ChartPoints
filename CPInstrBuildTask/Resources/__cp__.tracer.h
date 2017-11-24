#ifndef TRACER_H
#define TRACER_H

#include <stdint.h>
#include <memory>

#if defined(CPTRACER_EXPORTS)
#define CPTRACER_DLL_API __declspec( dllexport )
#elif defined(CPTRACER_EXPORTS)
#define CPTRACER_DLL_API __declspec( dllimport )
#else
#define CPTRACER_DLL_API
#endif

namespace cptracer
{

  template< typename T > class type_id
  {
  public:
    static int32_t id;
  };

  class CPTRACER_DLL_API tracer
  {
  public:
    typedef std::shared_ptr<tracer> tracer_ptr;
  protected:
    virtual void reg_elem(const char *name, uint64_t id, uint32_t _type_id) = 0;
    virtual void trace(uint64_t id, double val) = 0;
  public:
    virtual ~tracer();
    static tracer_ptr instance();
    template< typename T >
    static void pub_reg_elem(const char *name, const T &val)
    {
      try
      {
        instance()->reg_elem(name, reinterpret_cast<uint64_t>(&val), cptracer::type_id<T>::id);
      }
      catch (...)
      {
        ;
      }
    }
    template< typename T >
    static void pub_trace(const T &val)
    {
      try
      {
        instance()->trace(reinterpret_cast<uint64_t>(&val), static_cast<double>(val));
      }
      catch (...)
      {
        ;
      }
    }
  };

  extern "C" CPTRACER_DLL_API bool create_tracer(tracer::tracer_ptr &);

  typedef bool (*create_tracer_func)(tracer::tracer_ptr &);

} // namespace cptracer

#endif // TRACER_H

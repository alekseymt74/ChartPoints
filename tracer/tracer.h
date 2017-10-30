#ifndef TRACER_H
#define TRACER_H

#include <stdint.h>
#include <string>
#include <memory>
#include <iostream>

namespace cptracer
{

  template< typename T > class type_id
  {
  public:
    static int32_t id;
  };

  class tracer
  {
    typedef std::shared_ptr<tracer> tracer_ptr;
    virtual void reg_elem(const char *name, uint64_t id, uint32_t _type_id) const = 0;
    virtual void trace(uint64_t id, double val) const = 0;
  public:
    virtual ~tracer()
    {}
    static tracer_ptr instance();
    template< typename T >
    static void pub_reg_elem(const char *name, const T &val)
    {
      instance()->reg_elem(name, reinterpret_cast<uint64_t>(&val), cptracer::type_id<T>::id);
    }
    template< typename T >
    static void pub_trace(const T &val)
    {
      instance()->trace(reinterpret_cast<uint64_t>(&val), static_cast<double>(val));
    }
  };

} // namespace cptracer

#endif // TRACER_H

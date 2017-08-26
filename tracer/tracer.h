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

  class tracer_elem
  {
    uint64_t addr;
    std::string name;
  public:
    tracer_elem();
    tracer_elem( uint64_t _addr, const char *_name, uint32_t _type_id );
    virtual void reg( uint64_t _addr, const char *_name, uint32_t _type_id );
    const std::string &get_name() const { return name; }
    uint64_t get_id() const { return addr; }
    void trace( double val ) const;
  };

  template< typename T >
  class tracer_elem_impl : public tracer_elem
  {
    T *elem;
  public:
    tracer_elem_impl(){}
    tracer_elem_impl( T &_elem, const char *_name ) : tracer_elem( (uint64_t) &_elem, _name, type_id<T>::id ), elem( &_elem )
    {}
    void trace() const
    {
      tracer_elem::trace( ( double ) *elem );
      //std::cout << get_name() << ": " << *elem << std::endl;
    }
    void reg( uint64_t _addr, const char *_name, uint32_t _type_id )
    {
      elem = (T *)(_addr);
      tracer_elem::reg( _addr, _name, _type_id );
    }
  };

} // namespace cptracer

#endif // TRACER_H

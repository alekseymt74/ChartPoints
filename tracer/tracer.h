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
    const std::string &get_name() { return name; }
    uint64_t get_id() { return addr; }
    void trace();
  };

  class tracer
  {
  public:
    typedef std::shared_ptr<tracer> tracer_ptr;
    static tracer_ptr _this;
  public:
    static tracer_ptr instance()
    {
      if( !_this )
        _this = std::make_shared<tracer>();
      return _this;
    }
    void reg_elem( tracer_elem *te, uint32_t _type_id )
    {
      std::cout << "[reg_elem]; name: " << te->get_name() << "\tid: " << te->get_id() << "\ttype_id: " << _type_id << std::endl;
    }
    void trace( tracer_elem *te )
    {
      ;// std::cout << te->get_id() << ;
    }
  };

  template< typename T >
  class tracer_elem_impl : public tracer_elem
  {
    T *elem;
  public:
    tracer_elem_impl(){}
    tracer_elem_impl( T &_elem, const char *_name ) : tracer_elem( (uint64_t) &_elem, _name, type_id<T>::id ), elem( &_elem )
    {
      ;
    }
    void trace()
    {
      tracer_elem::trace();
      std::cout << get_name() << ": " << *elem << std::endl;
    }
    void reg( uint64_t _addr, const char *_name, uint32_t _type_id )
    {
      elem = (T *)(_addr);
      tracer_elem::reg( _addr, _name, _type_id );
    }
  };

} // namespace cptracer

#endif // TRACER_H

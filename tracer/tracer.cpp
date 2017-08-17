#include "tracer.h"

namespace cptracer
{
  
  int32_t type_id<int8_t>::id = 0;
  int32_t type_id<int16_t>::id = 1;
  int32_t type_id<int32_t>::id = 2;

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

  void tracer_elem::trace()
  {
    tracer::instance()->trace( this );
  }

} // namespace cptracer


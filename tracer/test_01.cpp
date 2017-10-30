#include "test_01.h"

test_01::test_01() : i(0), j(100)
{
  //i_te.reg((uint64_t)&i, "i", cptracer::type_id<int>::id);
  cptracer::tracer::pub_reg_elem("i", i);
  //j_te.reg( (uint64_t) &j, "j", cptracer::type_id<int>::id );
  cptracer::tracer::pub_reg_elem("j", j);
}

void test_01::func_01()
{
  ++i;
  //i_te.trace();
  cptracer::tracer::pub_trace(i);
  --j;
  //j_te.trace();
}

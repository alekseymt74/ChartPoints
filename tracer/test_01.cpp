#include "test_01.h"

test_01::test_01() : i( 0 ), j( 100 )
{
  i_te.reg( (uint64_t) &i, "i", cptracer::type_id<int>::id );
  //j_te.reg( (uint64_t) &j, "j", cptracer::type_id<int>::id );
}

void test_01::func_01()
{
  ++i;
  i_te.trace();
  --j;
  //j_te.trace();
}

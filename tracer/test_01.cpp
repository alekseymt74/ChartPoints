#include "test_01.h"

test_01::test_01() : i( 0 ), j( 100 ), i_te( i, "test_01::i" ), j_te( j, "test_01::j" )
{
}

void test_01::func_01()
{
  ++i;
  i_te.trace();
  --j;
  j_te.trace();
}

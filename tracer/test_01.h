#ifndef TEST_01_H
#define TEST_01_H
#include "tracer.h"

class test_01
{
  int i;
  cptracer::tracer_elem_impl<int> i_te;
  int j;
  cptracer::tracer_elem_impl<int> j_te;
public:
  test_01();
  void func_01();
};

#endif // TEST_01_H
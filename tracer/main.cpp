// tracer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "test_01.h"
#include <thread>

int main()
{
  test_01 tst_01;
  for(int i = 0; i < 100; ++i )
  {
    tst_01.func_01();
    std::this_thread::sleep_for( std::chrono::milliseconds( 10 ) );
  }

  return 0;
}


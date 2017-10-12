#include "test_01.h"
#include <iostream>
#include <math.h>

#include "ddd.h"

#define PI 3.14159265

test_01::test_01()
: d_01(0.0), d_02(0.0)
{}

void test_01::func_01()
{
    d_01 = 4 * sin(d_02) * 180 / PI;
    d_02 += 0.01;
	//int i;
}

void test_01::func_02()
{
}

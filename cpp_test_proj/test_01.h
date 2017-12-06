#pragma once

#include <stdint.h>

typedef int my_int;

class test_01
{
	int i_01;
	unsigned int ui_01;
	float f_01;
	double d_01, d_02, d_011, d_021;
    double dy;
	long l_01;
	unsigned long ul_01;
	int32_t i32_01;
	uint32_t ui32_01;
	my_int my_i_01;
public:
    test_01(double _dy);
	void func_01();
	void func_02();
	void func_03() {}
};

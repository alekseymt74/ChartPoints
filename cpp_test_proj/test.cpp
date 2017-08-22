// test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "temp_utest.h"

int main()
{
	temp_utest tst;
	for(int i = 0; i < 1000; ++i)
	{
		tst.f3();
		tst.f1(i);
	}
    return 0;
}


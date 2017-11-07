// test_01.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <thread>

class test_01
{
	int i;
public:
	test_01() : i(0) {}
	void f()
	{
		++i;
	}
};

int main()
{
	test_01 tst_01;
	//while (true)
	for(int i = 0; i < 500; ++i)
	{
		tst_01.f();
		std::this_thread::sleep_for(std::chrono::milliseconds(10));
	}

	return 0;
}


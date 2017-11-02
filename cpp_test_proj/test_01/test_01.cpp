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
	while(true)
		std::this_thread::sleep_for(std::chrono::milliseconds(10));

	return 0;
}


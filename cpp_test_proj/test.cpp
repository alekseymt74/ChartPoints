// test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "temp_utest.h"
#include "test_01.h"
#include <iostream>
#include <chrono>
#include <thread>
#include <conio.h>

#define TM_WAIT_MS 1

int main()
{

	temp_utest tst;
    test_01 tst_01;
std::chrono::system_clock::time_point tm_start = std::chrono::system_clock::now();
	std::thread thr1([&]()
	{
		for(int i = 0; i < 3000; ++i)
		{
			tst.f3();
			//tst.f1(i);
			//tst_01.func_01();
			std::this_thread::sleep_for(std::chrono::milliseconds(TM_WAIT_MS));
		}
	});
	std::thread thr2([&]()
	{
		for(int i = 0; i < 3000; ++i)
		{
			//tst.f3();
			tst.f1(i);
			//tst_01.func_01();
			std::this_thread::sleep_for(std::chrono::milliseconds(TM_WAIT_MS));
		}
	});
	for(int i = 0; i < 3000; ++i)
	{
		//tst.f3();
		//tst.f1(i);
        tst_01.func_01();
		std::this_thread::sleep_for(std::chrono::milliseconds(TM_WAIT_MS));
	}
	thr1.join();
	thr2.join();
	std::chrono::system_clock::rep tm_ellapsed = std::chrono::duration_cast< std::chrono::milliseconds >( std::chrono::system_clock::now() - tm_start ).count();
	std::cout << tm_ellapsed << std::endl;
	_getch();

    return 0;
}


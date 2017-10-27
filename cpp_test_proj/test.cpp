// test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "temp_utest.h"
#include "test_01.h"
#include <iostream>
#include <chrono>
#include <thread>
#include <conio.h>

class test_cpp_types
{
	short 					shrt;
	short int 				shrt_i;
	signed short			sgn_shrt;
	signed short int		sgn_shrt_i;
	unsigned short			u_shrt;
	unsigned short int		u_shrt_i;
	int						i;
	signed					sgn;
	signed int				sgn_i;
	unsigned				u;
	unsigned int			ui;
	long					l;
	long int				li;
	signed long				sgn_l;
	signed long int			sgn_li;
	unsigned long			ul;
	unsigned long int		uli;
	long long				ll;
	long long int			lli;
	signed long long		sgn_ll;
	signed long long int	sgn_lli;
	unsigned long long		ull;
	unsigned long long int	ulli;
	double					d;
	float					f;
	bool					b;
	signed char				sc;
	char					c;
	unsigned char			uc;
	int8_t					i8;
	int16_t					i16;
	int32_t					i32;
	int64_t					i64;
	uint8_t					ui8;
	uint16_t				ui16;
	uint32_t				ui32;
	uint64_t				ui64;
public:
	test_cpp_types()
	{
		shrt = 0;
		shrt_i = 0;
		sgn_shrt = 0;
		sgn_shrt_i = 0;
		u_shrt = 0;
		u_shrt_i = 0;
		i = 0;
		sgn = 0;
		sgn_i = 0;
		u = 0;
		ui = 0;
		l = 0;
		li = 0;
		sgn_l = 0;
		sgn_li = 0;
		ul = 0;
		uli = 0;
		ll = 0;
		lli = 0;
		sgn_ll = 0;
		sgn_lli = 0;
		ull = 0;
		ulli = 0;
		d = 0.0;
		f = 0.0;
		b = true;
		sc = 'a';
		c = 'b';
		sc = 'c';
		uc = 0;
		i8 = 0;
		i16 = 0;
		i32 = 0;
		i64 = 0;
		ui8 = 0;
		ui16 = 0;
		ui32 = 0;
		ui64 = 0;
	}
	void test()
	{
		++shrt;
		++shrt_i;
		++sgn_shrt;
		++sgn_shrt_i;
		++u_shrt;
		++u_shrt_i;
		++i;
		++sgn;
		++sgn_i;
		++u;
		++ui;
		++l;
		++li;
		++sgn_l;
		++sgn_li;
		++ul;
		++uli;
		++ll;
		++lli;
		++sgn_ll;
		++sgn_lli;
		++ull;
		++ulli;
		d += 0.01;
		f += 0.01f;
		b = !b;
		++sc;
		++c;
		++sc;
		++uc;
		++i8;
		++i16;
		++i32;
		++i64;
		++ui8;
		++ui16;
		++ui32;
		++ui64;
	}
};

#define TM_WAIT_MS 1

int main()
{

	temp_utest tst;
    test_01 tst_01;
	test_cpp_types tst_cpp_t;
	for (int i = 0; i < 10; ++i)
	{
		tst_cpp_t.test();
		std::this_thread::sleep_for(std::chrono::milliseconds(TM_WAIT_MS));
	}
std::chrono::system_clock::time_point tm_start = std::chrono::system_clock::now();
	//std::thread thr0([&]()
	//{
	//	for(int i = 0; i < 3000; ++i)
	//	{
	//		tst_cpp_t.test();
	//		std::this_thread::sleep_for(std::chrono::milliseconds(TM_WAIT_MS));
	//	}
	//});
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
	//thr0.join();
	thr1.join();
	thr2.join();
	std::chrono::system_clock::rep tm_ellapsed = std::chrono::duration_cast< std::chrono::milliseconds >( std::chrono::system_clock::now() - tm_start ).count();
	std::cout << tm_ellapsed << std::endl;
	_getch();

    return 0;
}


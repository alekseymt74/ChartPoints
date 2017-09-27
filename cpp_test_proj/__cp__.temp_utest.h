#include "..\tracer\tracer.h"
#ifndef _TEMP_UTEST_H
#define _TEMP_UTEST_H
#include <stdint.h>
class c1
{};
typedef c1 TC1;
//
//struct s1
//{};

class temp_utest
{

TC1 _c1;
//c1 _c1;
//c1 *pc1;
//s1 _s1;
//s1 *ps1;
unsigned int ui;
unsigned long ul;
unsigned short us;
int8_t i8;
int16_t i16;
int32_t i32;
int64_t i64;
uint8_t ui8;
uint16_t ui16;
uint32_t ui32;
uint64_t ui64;
int j;int k;
cptracer::tracer_elem_impl<int> __cp_trace_j;cptracer::tracer_elem_impl<int> __cp_trace_k;
public:
temp_utest():j(0), k(1000){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_k.reg((uint64_t) &k, "k", cptracer::type_id<int>::id);
}
temp_utest(int _j, int _k):j(_j), k(_k){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_k.reg((uint64_t) &k, "k", cptracer::type_id<int>::id);
}
void f2() { }
void f3();
void f1(int i){
__cp_trace_k.trace();
--k;
}
};

#endif // _TEMP_UTEST_H

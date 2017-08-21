#include "..\tracer\tracer.h"#include "..\tracer\tracer.h"#include "..\tracer\tracer.h"
#ifndef _TEMP_UTEST_H
#define _TEMP_UTEST_H

class temp_utest
{
int j;
cptracer::tracer_elem_impl<int> __cp_trace_j;cptracer::tracer_elem_impl<int> __cp_trace_j;cptracer::tracer_elem_impl<int> __cp_trace_j;
public:
temp_utest():j(0){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);
}
temp_utest(int _j):j(_j){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);
}
//void f1(int i);
void f2() { }
void f3();
};
//void temp_utest::f1(int i){}

#endif // _TEMP_UTEST_H

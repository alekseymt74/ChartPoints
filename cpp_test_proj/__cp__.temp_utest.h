#include "..\tracer\tracer.h"
#ifndef _TEMP_UTEST_H
#define _TEMP_UTEST_H

class temp_utest
{
int j;
cptracer::tracer_elem_impl<int> __cp_trace_j;
int k;
public:
temp_utest():j(0), k(1000){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);
}
temp_utest(int _j, int _k):j(_j), k(_k){
__cp_trace_j.reg((uint64_t) &j, "j", cptracer::type_id<int>::id);
}
void f2() { }
void f3();
void f1(int i){
--k;
}
};

#endif // _TEMP_UTEST_H

#ifndef _TEMP_UTEST_H
#define _TEMP_UTEST_H

class temp_utest
{
int j;
/*trace_elem __cp_trace_j;*/
public:
temp_utest():j(0){
/*j.init();*/
}
temp_utest(int _j):j(_j){
/*j.init();*/
}
//void f1(int i);
void f2() { }
void f3();
};
//void temp_utest::f1(int i){}

#endif // _TEMP_UTEST_H

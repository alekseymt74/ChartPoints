#include "__cp__.tracer.h"
#include <iostream>
#include <chrono>
#include "../CPTracer/CPTracer_i.h"
#include <atlcomcli.h>
#include <thread>
#include <atomic>
#include <mutex>
#include <queue>
#include <limits>

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}
MIDL_DEFINE_GUID( CLSID, CLSID_CPTracerFactory, 0xEA343A3A, 0xCF94, 0x4210, 0x89, 0xF5, 0x9B, 0xDF, 0x56, 0x11, 0x2C, 0xA2 );

namespace cptracer
{

  class notifier
  {
    std::mutex mtx;
    std::condition_variable cond;
    uint32_t count;
    bool res_normal;
  public:
    notifier() : res_normal( true ), count( 0 ) {}
    operator bool() { return res_normal; }
    void inc()
    {
      if( !res_normal )
        res_normal = true;
      std::unique_lock< std::mutex > lock( mtx );
      ++count;
    }
    void reset()
    {
      {
        std::unique_lock< std::mutex > lock( mtx );
        count = 0;
        res_normal = false;
        cond.notify_one();
      }
      std::unique_lock< std::mutex > lock( mtx );
    }
    void notify()
    {
      std::unique_lock< std::mutex > lock( mtx );
      if( count > 0 )
        --count;
      cond.notify_one();
    }
    bool wait()
    {
      std::unique_lock< std::mutex > lock( mtx );
      while( count != 0 )
        cond.wait( lock );
      return res_normal;
    }
  };

  struct trace_ent
  {
    uint64_t id;
    uint64_t tm;
    double val;
    trace_ent() {}
    trace_ent( uint64_t _id, uint64_t _tm, double _val )
      : id( _id ), tm( _tm ), val( _val )
    {}
  };
  typedef trace_ent data_ent;

  struct reg_ent
  {
    uint64_t id;
    uint64_t tm;
    std::string name;
    uint32_t type;
    reg_ent() {}
    reg_ent( ULONGLONG _id, uint64_t _tm, const std::string &_name, uint32_t _type )
      : id( _id ), tm( _tm ), name( _name ), type( _type )
    {}
  };

  template<typename TData>
  class data_queue
  {
  public:
    typedef std::queue< TData > data_cont;
  private:
    data_cont data_1;
    data_cont data_2;
  public:
    data_cont *in_ptr;
    data_cont *out_ptr;
    data_queue()
    {
      in_ptr = &data_1;
      out_ptr = &data_2;
    }
    void swap()
    {
      std::swap( in_ptr, out_ptr );
    }
  };


  class tracer_impl : public tracer
  {
    std::atomic_bool trace_thr_active;
    std::thread *trace_thr;
    std::mutex swap_trace_queue_mtx;
    std::mutex send_trace_queue_mtx;
    data_queue< trace_ent > trace_data;
    void trace_proc();

    std::thread *reg_thread;
    std::atomic_bool reg_thr_active;
    std::mutex need_reg_mtx;
    std::condition_variable need_reg_cond;
    bool need_reg;
    data_queue< reg_ent > reg_data;
    void reg_proc();

    notifier notif;

    CComPtr< ICPTracerFactory > trace_cons;
    CComPtr< ICPProcTracer > trace_elem_cons;
    static std::chrono::high_resolution_clock::time_point tm_start;
    bool check_and_send_curr( uint64_t tm_before );
    void check_and_send( uint64_t tm_before );
  public:
    tracer_impl();
    ~tracer_impl();
    void reg_elem( const char *name, uint64_t id, uint32_t _type_id ) override;
    void trace( uint64_t id, double val ) override;
  };

  std::chrono::high_resolution_clock::time_point tracer_impl::tm_start;

  tracer_impl::tracer_impl()
    : trace_elem_cons( nullptr )
    , need_reg( false )
  {
    HRESULT hr = CoInitializeEx( NULL, COINIT_MULTITHREADED );
    hr = trace_cons.CoCreateInstance( CLSID_CPTracerFactory, NULL, CLSCTX_LOCAL_SERVER );
    if( hr == S_OK )
    {
      DWORD proc_id = GetCurrentProcessId();
      hr = trace_cons->GetProcTracer( &trace_elem_cons, proc_id );
      tm_start = std::chrono::high_resolution_clock::now();
      trace_thr_active = true;
      trace_thr = new std::thread( std::bind( &tracer_impl::trace_proc, this ) );
      reg_thr_active = true;
      reg_thread = new std::thread( std::bind( &tracer_impl::reg_proc, this ) );
    }
  }

  tracer_impl::~tracer_impl()
  {
    try
    {
      if( reg_thread )
      {
        std::unique_lock<std::mutex> lock( need_reg_mtx );
        reg_thr_active = false;
        need_reg = true;
        need_reg_cond.notify_one();
        lock.unlock();
        reg_thread->join();
        delete reg_thread;
      }
      if( trace_thr )
      {
        notif.reset();
        trace_thr_active = false;
        trace_thr->join();
        delete trace_thr;
      }
      CoUninitialize();
    }
    catch( ... )
    {
    }
  }

  void tracer_impl::reg_proc()
  {
    while( reg_thr_active )
    {
      {
        std::unique_lock<std::mutex> lock( need_reg_mtx );
        if( reg_data.in_ptr->empty() )
        {
          while( !need_reg )
            need_reg_cond.wait( lock );
        }
        reg_data.swap();
      }
      reg_ent val;
      while( reg_data.out_ptr->size() )
      {
        val = reg_data.out_ptr->front();
        reg_data.out_ptr->pop();
        check_and_send( val.tm );
        USES_CONVERSION;
        trace_elem_cons->RegElem( CComBSTR( A2W( val.name.c_str() ) ).Detach(), val.id, val.type );
        notif.notify();
      }
      need_reg = false;
    }
  }

  void tracer_impl::reg_elem( const char *name, uint64_t id, uint32_t _type_id )
  {
    try
    {
      if( trace_elem_cons )
      {
        notif.inc();
        std::unique_lock<std::mutex> lock( need_reg_mtx );
        reg_data.in_ptr->emplace( reg_ent( id, std::chrono::duration_cast< std::chrono::microseconds >( std::chrono::high_resolution_clock::now() - tm_start ).count(), name, _type_id ) );
        need_reg = true;
        need_reg_cond.notify_one();
        lock.unlock();
      }
    }
    catch( ... )
    {
    }
  }

  bool tracer_impl::check_and_send_curr( uint64_t tm_before )
  {
    trace_ent val;
    while( trace_data.out_ptr->size() )
    {
      val = trace_data.out_ptr->front();
      if( val.tm < tm_before )
      {
        trace_data.out_ptr->pop();
        trace_elem_cons->Trace( val.id, val.tm, val.val );
      }
      else
        return false;
    }

    return true;
  }

  void tracer_impl::check_and_send( uint64_t tm_before )
  {
    std::unique_lock<std::mutex> lock( send_trace_queue_mtx );
    bool is_out_queue_empty = true;
    if( trace_data.out_ptr->size() )
      is_out_queue_empty = check_and_send_curr( tm_before );
    if( is_out_queue_empty )
    {
      {
        std::lock_guard< std::mutex > lock( swap_trace_queue_mtx );
        trace_data.swap();
      }
      check_and_send_curr( tm_before );
    }
  }

  void tracer_impl::trace_proc()
  {
    while( trace_thr_active )
    {
      notif.wait();
      check_and_send( ( std::numeric_limits<uint64_t>::max )( ) );
      std::this_thread::sleep_for( std::chrono::milliseconds( 10 ) );
    }
  }

  void tracer_impl::trace( uint64_t id, double val )// const
  {
    try
    {
      // std::cout << id << ": " << val << std::endl;
      if( trace_elem_cons )
      {
        std::lock_guard< std::mutex > lock( swap_trace_queue_mtx );
        trace_data.in_ptr->emplace( trace_ent( id, std::chrono::duration_cast< std::chrono::microseconds >( std::chrono::high_resolution_clock::now() - tm_start ).count(), val ) );
      }

    }
    catch( ... )
    {
    }
  }

  CPTRACER_DLL_API bool create_tracer( tracer::tracer_ptr &_tracer )
  {
    try
    {
      _tracer = std::make_shared<tracer_impl>();

      return true;
    }
    catch( ... )
    {
      _tracer = nullptr;
    }

    return false;
  }

} // namespace cptracer

namespace ChartPoints.CPServices.decl
{
  // base interface for all ChartPoints services (maybe extended in the future)
  public interface ICPService
  {
  }

  // delayed service query callback 
  //public delegate void OnCPServiceCreated<T>(T args);

  // singleton service provider
  public abstract partial class ICPServiceProvider
  {
    public abstract bool RegisterService<T>(T obj) where T : ICPService;
    public abstract bool GetService<T>(out T obj) where T : class;
    // query service providing callback if service is not registered yet
    //public abstract bool GetService<T>(out T obj, OnCPServiceCreated<T> cb) where T : class;
    public static ICPServiceProvider GetProvider()
    {
      return ChartPoints.CPServices.impl.priv.ICPServiceProvider.GetProviderImpl();
    }
  }
}

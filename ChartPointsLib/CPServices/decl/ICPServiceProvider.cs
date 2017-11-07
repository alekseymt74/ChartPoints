namespace ChartPoints
{
  public interface ICPService
  {
  }

  public abstract partial class ICPServiceProvider
  {
    public abstract bool RegisterService<T>(T obj) where T : ICPService;
    public abstract bool GetService<T>(out T obj) where T : class;
    public static ICPServiceProvider GetProvider()
    {
      return impl.ICPServiceProvider.GetProviderImpl();
    }
  }
}

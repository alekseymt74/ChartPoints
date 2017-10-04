namespace ChartPoints
{
  public abstract partial class ICPServiceProvider
  {
    public abstract bool GetService<T>(out T obj) where T : class;
    public static ICPServiceProvider GetProvider()
    {
      return impl.ICPServiceProvider.GetProviderImpl();
    }
  }
}

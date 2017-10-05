using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("ChartPointsLib")]
namespace ChartPoints
{
  interface ICPEventsService
  {

  }
  namespace impl
  {
    public abstract partial class ICPServiceProvider
    {
      protected static ChartPoints.ICPServiceProvider Instance;
      internal static ChartPoints.ICPServiceProvider GetProviderImpl()
      {
        if (Instance == null)
          Instance = new ServiceProvider();
        return Instance;
      }
    }

    internal class ServiceProvider : ChartPoints.ICPServiceProvider
    {
      //private IServ01 _serv01;
      public override bool GetService<T>(out T obj)
      {
        obj = null;
        //if (typeof(T) == typeof(IServ01))
        //{
        //  if (_serv01 == null)
        //    _serv01 = new Serv01();
        //  obj = _serv01 as T;

        //  return true;
        //}

        return false;
      }
    }
  }
}

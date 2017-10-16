using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("ChartPointsLib")]
namespace ChartPoints
{
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
      private ICPEventService cpEventsService;
      public override bool GetService<T>(out T obj)
      {
        obj = null;
        if (typeof(T) == typeof(ICPEventService))
        {
          if (cpEventsService == null)
            cpEventsService = new CPEventService();
          obj = cpEventsService as T;

          return true;
        }

        return false;
      }
    }
  }
}

using System.Collections.Generic;
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
          Instance = new CPServiceProvider();
        return Instance;
      }
    }

    internal class CPServiceProvider : ChartPoints.ICPServiceProvider
    {
      private ICPEventService cpEventsService;
      private IDictionary<string, ICPService> regServices = new SortedDictionary<string, ICPService>();

      public override bool RegisterService<T>(T obj)
      {
        ICPService serv = null;
        if(regServices.TryGetValue(typeof(T).Name, out serv))
          return false;
        regServices.Add(typeof(T).Name, obj);

        return true;
      }

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
        else
        {
          ICPService serv = null;
          if (regServices.TryGetValue(typeof(T).Name, out serv))
          {
            obj = serv as T;

            return true;
          }
        }

        return false;
      }
    }
  }
}

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ChartPoints.CPServices.decl;

//[assembly: InternalsVisibleTo("ChartPointsLib")]
namespace ChartPoints.CPServices.impl
{

  namespace priv
  {
    public abstract partial class ICPServiceProvider
    {
      protected static ChartPoints.CPServices.decl.ICPServiceProvider Instance;
      internal static ChartPoints.CPServices.decl.ICPServiceProvider GetProviderImpl()
      {
        if (Instance == null)
          Instance = new CPServiceProvider();
        return Instance;
      }
    }
  }

  internal class CPServiceProvider : ChartPoints.CPServices.decl.ICPServiceProvider
  {
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
      ICPService serv = null;
      if (regServices.TryGetValue(typeof(T).Name, out serv))
      {
        obj = serv as T;

        return true;
      }

      return false;
    }
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  [DataContract]
  public class TextPos
  {
    [DataMember]
    public string fileName { get; set; }
    [DataMember]
    public int lineNum { get; set; }
    [DataMember]
    public int linePos { get; set; }
  }

  [DataContract]
  public class CPClassLayout
  {
    [DataMember]
    public TextPos traceVarPos { get; set; }
    [DataMember]
    public IList<TextPos> traceVarInitPos { get; set; }
    [DataMember]
    public TextPos injConstructorPos { get; set; }
    public CPClassLayout()
    {
      traceVarInitPos = new List<TextPos>();
    }
  }

  [ServiceContract(Namespace = "TestNamespace")]
//  [KnownType(typeof(ChartPoints.ChartPointData)]
  public interface IIPCChartPoint
  {
    [OperationContract]
    string GetClassName(ChartPointData cpData);
    [OperationContract]
    CPClassLayout GetCPClassLayout(ChartPointData cpData);
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class IPCChartPoint : IIPCChartPoint
  {
    private IDictionary<string, CPClassLayout> injData = new SortedDictionary<string, CPClassLayout>();
    public CPClassLayout GetCPClassLayout(ChartPointData cpData)
    {
      IChartPoint chartPnt = Globals.processor.GetChartPoint(cpData);
      if (chartPnt == null)
        return null;
      CPClassLayout cpInjPoints = null;
      if (injData.TryGetValue(chartPnt.data.className, out cpInjPoints) && cpInjPoints != null)
        return cpInjPoints;
      chartPnt.CalcInjectionPoints(out cpInjPoints);
      injData.Add(chartPnt.data.className, cpInjPoints);

      return cpInjPoints;
    }

    public string GetClassName(ChartPointData cpData)
    {
      IChartPoint chartPnt = Globals.processor.GetChartPoint(cpData);
      if (chartPnt == null)
        return "";

      return chartPnt.data.className;
    }
  }
}

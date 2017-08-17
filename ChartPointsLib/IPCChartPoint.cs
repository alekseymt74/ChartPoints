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
    public int lineNum { get; set; }
    [DataMember]
    public int linePos { get; set; }
  }

  [DataContract]
  public class FilePosPnt
  {
    [DataMember]
    public string fileName { get; set; }
    [DataMember]
    public TextPos pos { get; set; } = new TextPos();
  }

  [DataContract]
  public class FilePosText : FilePosPnt
  {
    [DataMember]
    public TextPos posEnd { get; set; } = new TextPos();
  }

  [DataContract]
  public class CPInclude
  {
    [DataMember]
    public FilePosText pos { get; set; } = new FilePosText();
    [DataMember]
    public string inclOrig;
    [DataMember]
    public string inclReplace;
  }

  [DataContract]
  public class CPTraceVar
  {
    [DataMember]
    public FilePosPnt filePos { get; set; } = new FilePosPnt();
    [DataMember]
    public string type { get; set; }
  }

  [DataContract]
  public class CPClassLayout
  {
    [DataMember]
    public IDictionary<string, CPTraceVar> traceVars { get; set; } = new SortedDictionary<string, CPTraceVar>();
    [DataMember]
    public IList<FilePosPnt> traceVarInitPos { get; set; } = new List<FilePosPnt>();
    [DataMember]
    public FilePosPnt injConstructorPos { get; set; }
    [DataMember]
    public IList<CPInclude> includesPos { get; set; } = new List<CPInclude>();
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

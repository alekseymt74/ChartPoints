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
  public class CPData
  {
    [DataMember]
    public int lineNum = -1;
    [DataMember]
    public int linePos = -1;
    [DataMember]
    public string className = string.Empty;
    [DataMember]
    public string projName = string.Empty;
    [DataMember]
    public string fileName = string.Empty;
    [DataMember]
    public string varName = string.Empty;
    [DataMember]
    public bool enabled = false;
  }

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
    string GetClassName(CPData cpData);
    [OperationContract]
    CPClassLayout GetCPClassLayout(CPData cpData);
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class IPCChartPoint : IIPCChartPoint
  {
    private IDictionary<string, CPClassLayout> injData = new SortedDictionary<string, CPClassLayout>();

    private IChartPoint GetChartPoint(CPData cpData)
    {
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(cpData.projName);
      if (pPnts == null)
        return null;
      IFileChartPoints fPnts = pPnts.GetFileChartPoints(cpData.fileName);
      if (fPnts == null)
        return null;
      ILineChartPoints lPnts = fPnts.GetLineChartPoints(cpData.lineNum);
      if (lPnts == null)
        return null;
      IChartPoint chartPnt = lPnts.GetChartPoint(cpData.varName);

      return chartPnt;
    }
    public CPClassLayout GetCPClassLayout(CPData cpData)
    {
      IChartPoint chartPnt = GetChartPoint(cpData);
      if (chartPnt == null)
        return null;
      CPClassLayout cpInjPoints = null;
      if (!injData.TryGetValue(chartPnt.data.className, out cpInjPoints))// && cpInjPoints != null)
      {
        cpInjPoints = new CPClassLayout();
        injData.Add(chartPnt.data.className, cpInjPoints);
      }
      chartPnt.CalcInjectionPoints(cpInjPoints);

      return cpInjPoints;
    }

    public string GetClassName(CPData cpData)
    {
      //IChartPoint chartPnt = Globals.processor.GetChartPoint(cpData);
      IChartPoint chartPnt = GetChartPoint(cpData);
      if (chartPnt == null)
        return "";

      return chartPnt.data.className;
    }
  }
}

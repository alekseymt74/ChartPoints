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
    public FilePosPnt defPos { get; set; } = new FilePosPnt();
    [DataMember]
    public IList<FilePosPnt> traceVarInitPos { get; set; } = new List<FilePosPnt>();
    [DataMember]
    public IList<FilePosPnt> traceVarTracePos { get; set; } = new List<FilePosPnt>();

    [DataMember]
    public FilePosPnt injConstructorPos { get; set; } = null;
    [DataMember]
    public string name { get; set; }
    [DataMember]
    public string type { get; set; }
    [DataMember]
    public string className { get; set; }
  }

  [DataContract]
  public class CPClassLayout
  {
    [DataMember]
    public IDictionary<string, CPTraceVar> traceVarPos { get; set; } = new SortedDictionary<string, CPTraceVar>();
    [DataMember]
    public IDictionary<string, TextPos> traceInclPos { get; set; } = new SortedDictionary<string, TextPos>();
    [DataMember]
    public IDictionary<Tuple<string, string>, CPInclude> includesPos { get; set; } = new SortedDictionary<Tuple<string, string>, CPInclude>();
  }

  [ServiceContract(Namespace = "TestNamespace")]
//  [KnownType(typeof(ChartPoints.ChartPointData)]
  public interface IIPCChartPoint
  {
    [OperationContract]
    CPClassLayout GetInjectionData(string projName);
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class IPCChartPoint : IIPCChartPoint
  {
    private IDictionary<string, CPClassLayout> injData = new SortedDictionary<string, CPClassLayout>();

    public CPClassLayout GetInjectionData(string projName)
    {
      IProjectChartPoints pPnts = Globals.processor.GetProjectChartPoints(projName);
      if (pPnts == null)
        return null;
      CPClassLayout cpClassLayout = new CPClassLayout();
      foreach (var fPnts in pPnts.filePoints)
      {
        string _fname = fPnts.data.fileName;
        foreach (var lPnts in fPnts.linePoints)
        {
          int _lineNum = lPnts.data.pos.lineNum;
          int _linePos = lPnts.data.pos.linePos;
          foreach (var chartPnt in lPnts.chartPoints)
            chartPnt.CalcInjectionPoints(cpClassLayout, _fname, _lineNum, _linePos);
        }
      }

      return cpClassLayout;
    }
  }
}

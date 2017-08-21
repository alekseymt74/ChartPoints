using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChartPoints
{
  public interface ICPConfLoader
  {
    void LoadChartPoint(string metadata, Action<int, CPData> addCPDataAction);
  }

  public class CPConfLoader : ICPConfLoader
  {
    enum ETag
    {
      Unknown
      , TagVariable
      , TagLineNum
      , TagLinePos
      , TagEnable
    };
    private bool ChartPointsFromXml(String xml, Action<int, CPData> addCPDataAction)
    {
      bool ret = false;
      try
      {
        XmlTextReader tr = new XmlTextReader(xml, XmlNodeType.Element, null);
        {
          try
          {
            CPData cp = null;
            ETag tag = ETag.Unknown;
            while (tr.Read())
            {
              switch (tr.NodeType)
              {
                case XmlNodeType.Element:
                  switch (tr.Name)
                  {
                    case "ChartPoint":
                      cp = new CPData();
                      cp.lineNum = -1;
                      cp.linePos = -1;
                      break;
                    case "Variable":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagVariable;
                      break;
                    case "LineNum":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagLineNum;
                      break;
                    case "LinePos":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagLinePos;
                      break;
                    case "Enabled":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagEnable;
                      break;
                    default:
                      tag = ETag.Unknown;
                      break;
                  }
                  break;
                case XmlNodeType.Text:
                  switch (tag)
                  {
                    case ETag.TagVariable:
                      if (cp != null)
                        cp.varName = tr.Value;
                      break;
                    case ETag.TagLineNum:
                      if (cp != null)
                        cp.lineNum = Convert.ToInt32(tr.Value, 10);
                      break;
                    case ETag.TagLinePos:
                      if (cp != null)
                        cp.linePos = Convert.ToInt32(tr.Value, 10);
                      break;
                    case ETag.TagEnable:
                      if (cp != null)
                        cp.enabled = Convert.ToBoolean(tr.Value);
                      break;
                  }
                  break;
                case XmlNodeType.EndElement:
                  if (tr.Name == "ChartPoint" && cp != null && cp.lineNum >= 0 && cp.linePos >= 0)
                  {
                    addCPDataAction(cp.lineNum, cp);
                    cp = null;
                  }
                  break;
              }
              Console.WriteLine("NodeType: {0} NodeName: {1}", tr.NodeType, tr.Name);
            }
          }
          catch (InvalidOperationException)
          {
            ;
          }
        }
      }
      catch (Exception ex)
      {
      }

      return ret;
    }

    public void LoadChartPoint(string metadata, Action<int, CPData> addCPDataAction)
    {
      metadata = metadata.Replace("xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"", "");
      bool ret = ChartPointsFromXml(metadata, addCPDataAction);
    }
  }
}

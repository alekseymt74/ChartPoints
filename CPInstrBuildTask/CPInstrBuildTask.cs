using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Task = Microsoft.Build.Utilities.Task;
using System.Collections;
using System.Xml;

namespace ChartPoints
{
  internal class ChartPoint
  {
    public bool enabled { get; set; }
    public string var { get; set; }

    internal ChartPoint()
    {
      enabled = false;
    }
  }

  internal class FileChartPoints
  {
    public IDictionary<int, ChartPoint> chartPoints { get; set; }

    internal FileChartPoints()
    {
      chartPoints = new SortedDictionary<int, ChartPoint>();
    }
  }

  public class CPInstrBuildTask : Task
    {
    [Required]
    public ITaskItem[] InputSrcFiles { get; set; }
    [Required]
    public ITaskItem[] InputHeaderFiles { get; set; }
    [Required]
    public ITaskItem[] InputChartPoints { get; set; }
    [Output]
    public ITaskItem[] OutputSrcFiles { get; set; }
    [Output]
    public ITaskItem[] OutputHeaderFiles { get; set; }
    [Output]
    public bool SrcFilesChanged { get; set; }
    [Output]
    public bool HeaderFilesChanged { get; set; }

    private IDictionary<string, FileChartPoints> filesChartPoints { get; set; }

    private bool CheckFiles(ref ITaskItem[] inTaskItems, ref ITaskItem[] outTaskItems)
    {
      //IVsMSBuildTaskFileManager hostTaskFileManager = this.HostObject as IVsMSBuildTaskFileManager;
      //if(hostTaskFileManager != null)
      //  Log.LogMessage(MessageImportance.High, "OK");
      //else
      //  Log.LogMessage(MessageImportance.High, "FAIL");
      bool changed = false;
      ArrayList items = new ArrayList();
      foreach (ITaskItem item in inTaskItems)
      {
        FileChartPoints chartPoints;
        filesChartPoints.TryGetValue(item.ItemSpec, out chartPoints);
        if (chartPoints != null)
        {
          string cpFileName = "__cp__." + item.ItemSpec;
          File.Copy(item.ItemSpec, cpFileName, true);
          string[] txt = File.ReadAllLines(cpFileName);
          FileStream fileStream = new FileStream(cpFileName, FileMode.Open, FileAccess.Write, FileShare.None);
          StreamWriter streamWriter = new StreamWriter(fileStream);
          int pos = 0;
          foreach (var cp in chartPoints.chartPoints)
          {
            for (int i = pos; i < cp.Key; ++i)
              streamWriter.WriteLine(txt[i]);
            streamWriter.WriteLine("std::cout << i << std::endl;");
            pos = cp.Key;
          }
          for (int i = pos; i < txt.Length; ++i)
            streamWriter.WriteLine(txt[i]);
          streamWriter.Close();
          ITaskItem replacedItem = new TaskItem(cpFileName);
          items.Add(replacedItem);
          changed = true;
        }
        else
          items.Add(item);
        //if (item.ItemSpec == "test_01.h")
        //{
        //  //string content;
        //  //hostTaskFileManager.GetFileContents("test_01.h", out content);
        //  //string ret = content.Replace("++i;", "std::cout << ++i << std::endl");
        //  //hostTaskFileManager.PutGeneratedFileContents("test_01.h", content);

        //  ITaskItem replacedItem = new TaskItem("test_01.my.h");
        //  items.Add(replacedItem);
        //  changed = true;
        //}
        //else if (item.ItemSpec == "test.cpp")
        //{
        //  ITaskItem replacedItem = new TaskItem("test.my.cpp");
        //  items.Add(replacedItem);
        //  changed = true;
        //}
        //else
        //{
        //  items.Add(item);
        //}
      }
      outTaskItems = (ITaskItem[])items.ToArray(typeof(ITaskItem));

      return changed;
    }
    enum ETag
    {
      Unknown
      , TagVariable
      , TagBeforeLine
      , TagEnable
    };
    private bool ChartPointsFromXml(String xml, ref FileChartPoints fileChartPoints)
    {
      bool ret = false;
      try
      {
        XmlTextReader tr = new XmlTextReader(xml, XmlNodeType.Element, null);
        {
          try
          {
            ChartPoint cp = null;
            ETag tag = ETag.Unknown;
            int beforeLine = -1;
            while (tr.Read())
            {
              switch (tr.NodeType)
              {
                case XmlNodeType.Element:
                  switch (tr.Name)
                  {
                    case "ChartPoint":
                      cp = new ChartPoint();
                      break;
                    case "Variable":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagVariable;
                      break;
                    case "BeforeLine":
                      //if (cp == null)
                      //  throw;
                      tag = ETag.TagBeforeLine;
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
                        cp.var = tr.Value;
                      break;
                    case ETag.TagBeforeLine:
                      beforeLine = Convert.ToInt32(tr.Value, 10);
                      break;
                    case ETag.TagEnable:
                      if (cp != null)
                        cp.enabled = Convert.ToBoolean(tr.Value);
                      break;
                  }
                  break;
                case XmlNodeType.EndElement:
                  if (tr.Name == "ChartPoint" && cp != null && beforeLine >= 0)
                  {
                    fileChartPoints.chartPoints.Add(beforeLine, cp);
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
    public override bool Execute()
    {
      filesChartPoints = new SortedDictionary<string, FileChartPoints>();
      foreach (var chartpointFile in InputChartPoints)
      {
        FileChartPoints fileChartPoints = new FileChartPoints();
        filesChartPoints.Add(chartpointFile.ItemSpec, fileChartPoints);
        string data = chartpointFile.GetMetadata("ChartPoints");
        data = data.Replace("xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"", "");
        bool ret = ChartPointsFromXml(data, ref fileChartPoints);
      }
      if (filesChartPoints.Any())
      {
        SrcFilesChanged = false;
        Log.LogMessage(MessageImportance.High, "***Looking for files with chartpoints...");
        var inFiles = InputSrcFiles;
        var outFiles = OutputSrcFiles;
        SrcFilesChanged = CheckFiles(ref inFiles, ref outFiles);
        if (SrcFilesChanged)
        {
          OutputSrcFiles = outFiles;
          Log.LogMessage(MessageImportance.High, "***Output source files changed");
        }
        inFiles = InputHeaderFiles;
        outFiles = OutputHeaderFiles;
        HeaderFilesChanged = CheckFiles(ref inFiles, ref outFiles);
        if (HeaderFilesChanged)
        {
          OutputHeaderFiles = outFiles;
          Log.LogMessage(MessageImportance.High, "***Output header files changed");
        }
      }

      return true;
    }
  }
}

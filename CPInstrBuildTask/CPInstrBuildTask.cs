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
using System.IO;
using ChartPoints;

namespace ChartPointsBuilder
{

  public class FileChartPoints
  {
    public IDictionary<int, ChartPointData> chartPoints { get; set; }

    internal FileChartPoints()
    {
      chartPoints = new SortedDictionary<int, ChartPointData>();
    }
  }

  public class CPConfLoader : ChartPoints.CPConfLoader
  {
    public void LoadChartPoints(ITaskItem[] InputChartPoints, Func<string, Action<int, ChartPointData> > filesChartPoints)
    {
      foreach (var item in InputChartPoints)
      {
        Action<int, ChartPointData> addCPDataAction = filesChartPoints(item.ItemSpec);
        string metadata = item.GetMetadata("ChartPoints");
        LoadChartPoint(metadata, addCPDataAction);
      }
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

    private CPConfLoader confLoader;
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
            if (cp.Key <= txt.Length)
            {
              for (int i = pos; i < cp.Key; ++i)
                streamWriter.WriteLine(txt[i]);
              string curLine = txt[cp.Key];
              if (cp.Value.linePos <= curLine.Length)
              {
                string beforeInj = curLine.Substring(0, cp.Value.linePos);
                streamWriter.WriteLine(beforeInj);
                streamWriter.WriteLine("std::cout << i << std::endl;");
                string afterInj = curLine.Substring(cp.Value.linePos, curLine.Length - cp.Value.linePos);
                streamWriter.WriteLine(afterInj);
                pos = cp.Key + 1;
              }
            }
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
      }
      outTaskItems = (ITaskItem[])items.ToArray(typeof(ITaskItem));

      return changed;
    }

    public override bool Execute()
    {
      confLoader = new CPConfLoader();
      filesChartPoints = new SortedDictionary<string, FileChartPoints>();
      IDictionary<string, FileChartPoints> retFilesChartPoints = filesChartPoints;
      confLoader.LoadChartPoints(InputChartPoints, (fname) =>
      {
        FileChartPoints fileChartPoints = new FileChartPoints();
        filesChartPoints.Add(fname, fileChartPoints);
        Action<int, ChartPointData> act = (i, data) => fileChartPoints.chartPoints.Add(i,data);
        return act;
      });
      if(filesChartPoints.Any())
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

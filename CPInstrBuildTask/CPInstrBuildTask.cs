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
using System.ServiceModel;
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

  internal class CPFileCodeInjector
  {
    private IDictionary<int, IDictionary<int, StringBuilder>> lineText = new SortedDictionary<int, IDictionary<int, StringBuilder>>();

    internal void Inject(string fileName)
    {
      string cpFileName = "__cp__." + fileName;
      File.Copy(fileName, cpFileName, true);
      string[] txt = File.ReadAllLines(cpFileName);
      FileStream fileStream = new FileStream(cpFileName, FileMode.Open, FileAccess.Write, FileShare.None);
      StreamWriter streamWriter = new StreamWriter(fileStream);
      int lineNum = 0;
      foreach (var lineTxt in lineText)
      {
        if (lineTxt.Key <= txt.Length)
        {
          for (int i = lineNum; i < lineTxt.Key; ++i)
            streamWriter.WriteLine(txt[i]);
          string curLine = txt[lineTxt.Key];
          int linePos = 0;
          foreach (var strData in lineTxt.Value)
          {
            if (strData.Key <= curLine.Length)
            {
              string beforeInj = curLine.Substring(linePos, strData.Key);
              if (beforeInj.Length > 0)
                streamWriter.WriteLine(beforeInj);
              streamWriter.WriteLine(strData.Value);//"std::cout << j << std::endl;");
              string afterInj = curLine.Substring(strData.Key, curLine.Length - strData.Key);
              if (afterInj.Length > 0)
                streamWriter.WriteLine(afterInj);
            }
          }
          lineNum = lineTxt.Key + 1;
        }
      }
      for (int i = lineNum; i < txt.Length; ++i)
        streamWriter.WriteLine(txt[i]);
      streamWriter.Close();
    }

    internal void AddText(int lineNum, int linePos, string text)
    {
      IDictionary<int, StringBuilder> strData = null;
      if (!lineText.TryGetValue(lineNum, out strData))
      {
        strData = new SortedDictionary<int, StringBuilder>();
        lineText.Add(lineNum, strData);
      }
      StringBuilder str = null;
      if (!strData.TryGetValue(linePos, out str))
      {
        str = new StringBuilder();
        strData.Add(linePos, str);
      }
      str.Append(text);
    }
  }

  internal class CPClassCodeInjector
  {
    private CPClassLayout cpClassLayout;
    private IDictionary<int, ChartPointData> cps = new SortedDictionary<int, ChartPointData>();
    internal CPClassCodeInjector(CPClassLayout _cpClassLayout)
    {
      cpClassLayout = _cpClassLayout;
    }

    internal void AddChartPointData(ChartPointData cpData)
    {
      cps.Add(cpData.lineNum, cpData);
    }

    internal void Inject(Func<string, CPFileCodeInjector> getFileInjFunc)
    {
      foreach (var cp in cps)
      {
        CPFileCodeInjector fileCPInj = getFileInjFunc(cp.Value.fileName);
        string traceVarName = "__cp_trace_" + cp.Value.varName;
        fileCPInj.AddText(cp.Value.lineNum - 1, cp.Value.linePos - 1, "/*" + traceVarName + ".trace();*/");
        CPFileCodeInjector fileCPVarInj = getFileInjFunc(cpClassLayout.traceVarPos.fileName);
        fileCPVarInj.AddText(cpClassLayout.traceVarPos.lineNum, cpClassLayout.traceVarPos.linePos
          , "/*trace_elem " + traceVarName + ";*/");
        if (cpClassLayout.injConstructorPos != null)
        {
          CPFileCodeInjector fileConstrInj = getFileInjFunc(cpClassLayout.injConstructorPos.fileName);
          fileConstrInj.AddText(cpClassLayout.injConstructorPos.lineNum, cpClassLayout.injConstructorPos.linePos
            , "public:\n" + cp.Value.className + "(){\n/*" + cp.Value.varName + ".init();*/\n" + "}");
        }
        else
        {
          foreach (var varInitPos in cpClassLayout.traceVarInitPos)
          {
            CPFileCodeInjector fileVarInitInj = getFileInjFunc(varInitPos.fileName);
            fileVarInitInj.AddText(varInitPos.lineNum, varInitPos.linePos
              , "/*" + cp.Value.varName + ".init();*/");
          }
        }
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
    private IIPCChartPoint ipcChartPnt;
    private IDictionary<string, FileChartPoints> filesChartPoints { get; set; }
    private IDictionary<string, CPClassCodeInjector> classesInjectors;
    private IDictionary<string, CPFileCodeInjector> filesInjectors;

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
        FileChartPoints fileChartPoints;
        filesChartPoints.TryGetValue(item.ItemSpec, out fileChartPoints);
        if (fileChartPoints != null)
        {
          //string cpFileName = "__cp__." + item.ItemSpec;
          //File.Copy(item.ItemSpec, cpFileName, true);
          //string[] txt = File.ReadAllLines(cpFileName);
          //FileStream fileStream = new FileStream(cpFileName, FileMode.Open, FileAccess.Write, FileShare.None);
          //StreamWriter streamWriter = new StreamWriter(fileStream);
          //int pos = 0;
          foreach (var cp in fileChartPoints.chartPoints)
          {
            //CPClassLayout cpInjPnts = ipcChartPnt.GetCPClassLayout(cp.Value);
            string cpClassName = ipcChartPnt.GetClassName(cp.Value);
            CPClassCodeInjector CPClassCodeInjector = null;
            if (!classesInjectors.TryGetValue(cpClassName, out CPClassCodeInjector))
            {
              CPClassLayout cpClassLayout = ipcChartPnt.GetCPClassLayout(cp.Value);
              CPClassCodeInjector = new CPClassCodeInjector(cpClassLayout);
              classesInjectors.Add(cpClassName, CPClassCodeInjector);
            }
            CPClassCodeInjector.AddChartPointData(cp.Value);

          }
        }
        else
          items.Add(item);
      }
      outTaskItems = (ITaskItem[])items.ToArray(typeof(ITaskItem));

      return changed;
    }

    internal CPFileCodeInjector GetFileInjector(string fileName)
    {
      CPFileCodeInjector cpfInj = null;
      if (!filesInjectors.TryGetValue(fileName, out cpfInj))
      {
        cpfInj = new CPFileCodeInjector();
        filesInjectors.Add(fileName, cpfInj);
      }

      return cpfInj;
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
        Action<int, ChartPointData> act = (i, data) =>
        {
          data.fileName = fname;
          fileChartPoints.chartPoints.Add(i, data);
        };
        return act;
      });
      if(filesChartPoints.Any())
      {
        classesInjectors = new SortedDictionary<string, CPClassCodeInjector>();
        filesInjectors = new SortedDictionary<string, CPFileCodeInjector>();
        NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        string address = "net.pipe://localhost/ChartPoints/IPCChartPoint";//!!!!!!!!!! move to vcxproj !!!!!!!!!!!!
        EndpointAddress ep = new EndpointAddress(address);
        ipcChartPnt = ChannelFactory<IIPCChartPoint>.CreateChannel(binding, ep);
        //!!!CHECK!!!
        foreach (var fileCPs in filesChartPoints)
        {
          foreach (var cp in fileCPs.Value.chartPoints)
          {
            string cpClassName = ipcChartPnt.GetClassName(cp.Value);
            CPClassCodeInjector cpClassCodeInjector = null;
            if (!classesInjectors.TryGetValue(cpClassName, out cpClassCodeInjector))
            {
              CPClassLayout cpClassLayout = ipcChartPnt.GetCPClassLayout(cp.Value);
              cpClassCodeInjector = new CPClassCodeInjector(cpClassLayout);
              classesInjectors.Add(cpClassName, cpClassCodeInjector);
            }
            cpClassCodeInjector.AddChartPointData(cp.Value);
          }
        }
        foreach (var cpInj in classesInjectors)
          cpInj.Value.Inject(GetFileInjector);
        foreach (var fileInj in filesInjectors)
          fileInj.Value.Inject(fileInj.Key);
        ArrayList items = new ArrayList();
        foreach (ITaskItem item in InputSrcFiles)
        {
          CPFileCodeInjector cpCodeInj = null;
          if (filesInjectors.TryGetValue(item.ItemSpec, out cpCodeInj))
          {
            ITaskItem replacedItem = new TaskItem("__cp__." + item.ItemSpec);
            items.Add(replacedItem);
          }
          else
            items.Add(item);
        }
        if (items.Count > 0)
        {
          OutputSrcFiles = (ITaskItem[]) items.ToArray(typeof(ITaskItem));
          SrcFilesChanged = true;
        }
        items.Clear();
        foreach (ITaskItem item in InputHeaderFiles)
        {
          CPFileCodeInjector cpCodeInj = null;
          if (filesInjectors.TryGetValue(item.ItemSpec, out cpCodeInj))
          {
            ITaskItem replacedItem = new TaskItem("__cp__." + item.ItemSpec);
            items.Add(replacedItem);
          }
          else
            items.Add(item);
        }
        if (items.Count > 0)
        {
          OutputHeaderFiles = (ITaskItem[]) items.ToArray(typeof(ITaskItem));
          HeaderFilesChanged = true;
        }
      }

      return true;
    }
  }
}

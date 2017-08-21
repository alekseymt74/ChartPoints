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

  public class TaskLogger
  {
    public static TaskLoggingHelper Log { get; set; }
  }

  public class FileChartPoints
  {
    public IDictionary<int, CPData> chartPoints { get; set; }

    internal FileChartPoints()
    {
      chartPoints = new SortedDictionary<int, CPData>();
    }
  }

  public class CPConfLoader : ChartPoints.CPConfLoader
  {
    public void LoadChartPoints(ITaskItem[] InputChartPoints, Func<string, Action<int, CPData> > filesChartPoints)
    {
      foreach (var item in InputChartPoints)
      {
        Action<int, CPData> addCPDataAction = filesChartPoints(item.ItemSpec);
        string metadata = item.GetMetadata("ChartPoints");
        LoadChartPoint(metadata, addCPDataAction);
      }
    }
  }

  internal interface ITextTransform
  {
    void Transform(StreamWriter streamWriter, string curLine, int posStart, int posEnd);
  }

  internal class TextTransformAdd : ITextTransform
  {
    private string strData = String.Empty;
    internal TextTransformAdd(string text) { strData = text; }
    public void Transform(StreamWriter streamWriter, string curLine, int posStart, int posEnd)
    {
      if (posEnd <= curLine.Length)
      {
        string beforeOrk = curLine.Substring(posStart, posEnd);
        if (beforeOrk.Length > 0)
          streamWriter.WriteLine(beforeOrk);
        streamWriter.WriteLine(strData);
        string afterOrk = curLine.Substring(posEnd, curLine.Length - posEnd);
        if (afterOrk.Length > 0)
          streamWriter.WriteLine(afterOrk);
      }
    }

    internal void Append(string text)
    {
      strData += text;
    }
  }

  internal class TextTransformReplace : ITextTransform
  {
    private string strOrigin = String.Empty;
    private string strReplace = String.Empty;
    internal TextTransformReplace(string textOrigin, string textReplace) { strOrigin = textOrigin; strReplace = textReplace; }
    public void Transform(StreamWriter streamWriter, string curLine, int posStart, int posEnd)
    {
      if (posEnd <= curLine.Length)
      {
        //int pos = curLine.IndexOf(strOrigin);
        curLine = curLine.Replace(strOrigin, strReplace);
        streamWriter.WriteLine(curLine);
      }
    }
  }

  internal class CPFileCodeOrchestrator
  {
    private IDictionary<int, IDictionary<int, ITextTransform>> lineText = new SortedDictionary<int, IDictionary<int, ITextTransform>>();

    internal void Orchestrate(string fileName)
    {
      string cpFileName = "__cp__." + fileName;
      TaskLogger.Log.LogMessage(MessageImportance.High, "$$$$$$$$$$$$$$$$$$$$$$$$$$$" + Directory.GetCurrentDirectory());
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
          int linePos = 0;//!!!TODO!!!
          foreach (var strData in lineTxt.Value)
          {
            strData.Value.Transform(streamWriter, curLine, linePos, strData.Key);
            //if (strData.Key <= curLine.Length)
            //{
            //  string beforeOrk = curLine.Substring(linePos, strData.Key);
            //  if (beforeOrk.Length > 0)
            //    streamWriter.WriteLine(beforeOrk);
            //  streamWriter.WriteLine(strData.Value);
            //  string afterOrk = curLine.Substring(strData.Key, curLine.Length - strData.Key);
            //  if (afterOrk.Length > 0)
            //    streamWriter.WriteLine(afterOrk);
              linePos = strData.Key;
            //}
          }
          lineNum = lineTxt.Key + 1;
        }
      }
      for (int i = lineNum; i < txt.Length; ++i)
        streamWriter.WriteLine(txt[i]);
      streamWriter.Close();
    }

    internal void AddTransform(int lineNum, int linePos, string text)
    {
      IDictionary<int, ITextTransform> strData = null;
      if (!lineText.TryGetValue(lineNum, out strData))
      {
        strData = new SortedDictionary<int, ITextTransform>();
        lineText.Add(lineNum, strData);
      }
      ITextTransform str = null;
      if (!strData.TryGetValue(linePos, out str))
      {
        str = new TextTransformAdd(text);
        strData.Add(linePos, str);
      }
      else
        ((TextTransformAdd)str).Append(text);
    }
    internal void AddTransform(int lineNum, int linePos, ITextTransform trans)
    {
      IDictionary<int, ITextTransform> strData = null;
      if (!lineText.TryGetValue(lineNum, out strData))
      {
        strData = new SortedDictionary<int, ITextTransform>();
        lineText.Add(lineNum, strData);
      }
      ITextTransform str = null;
      if (!strData.TryGetValue(linePos, out str))
      {
        strData.Add(linePos, trans);
      }
      else
        ;//!!!((TextTransformAdd)str).Append(text);
    }
  }


  internal class CPClassCodeOrchestrator
  {
    private CPClassLayout cpClassLayout;
    private IDictionary<int, CPData> cps = new SortedDictionary<int, CPData>();
    internal CPClassCodeOrchestrator(CPClassLayout _cpClassLayout)
    {
      cpClassLayout = _cpClassLayout;
    }

    internal void AddChartPointData(CPData cpData)
    {
      cps.Add(cpData.lineNum, cpData);
    }

    internal void Orchestrate(Func<string, CPFileCodeOrchestrator> getFileOrkFunc)
    {
      foreach (var cp in cps)
      {
        CPTraceVar traceVar = null;
        if (cpClassLayout.traceVars.TryGetValue(cp.Value.varName, out traceVar))
        {
          CPFileCodeOrchestrator fileCPOrk = getFileOrkFunc(cp.Value.fileName);
          string traceVarName = "__cp_trace_" + cp.Value.varName;
          fileCPOrk.AddTransform(cp.Value.lineNum - 1, cp.Value.linePos - 1, traceVarName + ".trace();");
          CPFileCodeOrchestrator fileCPVarOrk = getFileOrkFunc(traceVar.filePos.fileName);
          fileCPVarOrk.AddTransform(0, 0, "#include \"..\\tracer\\tracer.h\"");
          fileCPVarOrk.AddTransform(traceVar.filePos.pos.lineNum, traceVar.filePos.pos.linePos, "cptracer::tracer_elem_impl<" + traceVar.type + "> " + traceVarName + ";");
          if (cpClassLayout.injConstructorPos != null)
          {
            CPFileCodeOrchestrator fileConstrOrk = getFileOrkFunc(cpClassLayout.injConstructorPos.fileName);
            fileConstrOrk.AddTransform(cpClassLayout.injConstructorPos.pos.lineNum, cpClassLayout.injConstructorPos.pos.linePos
              , "public:\n" + cp.Value.className + "(){\n" + traceVarName + ".reg((uint64_t) &" + cp.Value.varName + ", \"" +
              cp.Value.varName + "\", cptracer::type_id<" + traceVar.type + ">::id);\n" + "}");
          }
          else
          {
            foreach (var varInitPos in cpClassLayout.traceVarInitPos)
            {
              CPFileCodeOrchestrator fileVarInitOrk = getFileOrkFunc(varInitPos.fileName);
              fileVarInitOrk.AddTransform(varInitPos.pos.lineNum, varInitPos.pos.linePos
                , traceVarName + ".reg((uint64_t) &" + cp.Value.varName + ", \"" + cp.Value.varName + "\", cptracer::type_id<" + traceVar.type + ">::id);");
            }
          }
        }
        //else !!!!!!!!!!!!!!!!!
      }
      foreach (var inclFilePos in cpClassLayout.includesPos)
      {
        CPFileCodeOrchestrator fileCPOrk = getFileOrkFunc(inclFilePos.pos.fileName);
        ITextTransform inclTrans = new TextTransformReplace(inclFilePos.inclOrig, inclFilePos.inclReplace);
        fileCPOrk.AddTransform(inclFilePos.pos.pos.lineNum, inclFilePos.pos.pos.linePos, inclTrans);
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
    [Required]
    public string ProjectName { get; set; }
    [Output]
    public ITaskItem[] OutputSrcFiles { get; set; }
    [Output]
    public ITaskItem[] OutputHeaderFiles { get; set; }
    [Output]
    public bool SrcFilesChanged { get; set; }
    [Output]
    public bool HeaderFilesChanged { get; set; }
    public static TaskLoggingHelper TaskLog;
    private CPConfLoader confLoader;
    private IIPCChartPoint ipcChartPnt;
    private IDictionary<string, FileChartPoints> filesChartPoints { get; set; }
    private IDictionary<string, CPClassCodeOrchestrator> classesOrchestrator;
    private IDictionary<string, CPFileCodeOrchestrator> filesOrchestrator;


    internal CPFileCodeOrchestrator GetFileOrchestrator(string fileName)
    {
      CPFileCodeOrchestrator cpfOrk = null;
      if (!filesOrchestrator.TryGetValue(fileName, out cpfOrk))
      {
        cpfOrk = new CPFileCodeOrchestrator();
        filesOrchestrator.Add(fileName, cpfOrk);
      }

      return cpfOrk;
    }

    public override bool Execute()
    {
      TaskLogger.Log = Log;
      confLoader = new CPConfLoader();
      filesChartPoints = new SortedDictionary<string, FileChartPoints>();
      IDictionary<string, FileChartPoints> retFilesChartPoints = filesChartPoints;
      confLoader.LoadChartPoints(InputChartPoints, (fname) =>
      {
        FileChartPoints fileChartPoints = new FileChartPoints();
        filesChartPoints.Add(fname, fileChartPoints);
        Action<int, CPData> act = (i, data) =>
        {
          data.fileName = fname;
          data.projName = ProjectName;
          fileChartPoints.chartPoints.Add(i, data);
        };
        return act;
      });
      if(filesChartPoints.Any())
      {
        classesOrchestrator = new SortedDictionary<string, CPClassCodeOrchestrator>();
        filesOrchestrator = new SortedDictionary<string, CPFileCodeOrchestrator>();
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
            CPClassCodeOrchestrator cpClassCodeOrchestrator = null;
            if (!classesOrchestrator.TryGetValue(cpClassName, out cpClassCodeOrchestrator))
            {
              CPClassLayout cpClassLayout = ipcChartPnt.GetCPClassLayout(cp.Value);
              cpClassCodeOrchestrator = new CPClassCodeOrchestrator(cpClassLayout);
              classesOrchestrator.Add(cpClassName, cpClassCodeOrchestrator);
            }
            cpClassCodeOrchestrator.AddChartPointData(cp.Value);
          }
        }
        foreach (var cpOrk in classesOrchestrator)
          cpOrk.Value.Orchestrate(GetFileOrchestrator);
        foreach (var fileOrk in filesOrchestrator)
          fileOrk.Value.Orchestrate(fileOrk.Key);
        ArrayList items = new ArrayList();
        foreach (ITaskItem item in InputSrcFiles)
        {
          CPFileCodeOrchestrator cpCodeOrk = null;
          if (filesOrchestrator.TryGetValue(item.ItemSpec, out cpCodeOrk))
          {
            ITaskItem replacedItem = new TaskItem("__cp__." + item.ItemSpec);
            items.Add(replacedItem);
          }
          else
            items.Add(item);
        }
        ITaskItem tracerItem = new TaskItem("..\\tracer\\tracer.cpp");
        items.Add(tracerItem);
        if (items.Count > 0)
        {
          OutputSrcFiles = (ITaskItem[]) items.ToArray(typeof(ITaskItem));
          SrcFilesChanged = true;
        }
        items.Clear();
        foreach (ITaskItem item in InputHeaderFiles)
        {
          CPFileCodeOrchestrator cpCodeOrk = null;
          if (filesOrchestrator.TryGetValue(item.ItemSpec, out cpCodeOrk))
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

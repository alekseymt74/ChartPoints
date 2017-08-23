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
      //TaskLogger.Log.LogMessage(MessageImportance.High, "$$$$$$$$$$$$$$$$$$$$$$$$$$$" + Directory.GetCurrentDirectory());
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
            linePos = strData.Key;
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
    private IIPCChartPoint ipcChartPnt;
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
      NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
      string address = "net.pipe://localhost/ChartPoints/IPCChartPoint"; //!!!!!!!!!! move to vcxproj !!!!!!!!!!!!
      EndpointAddress ep = new EndpointAddress(address);
      ipcChartPnt = ChannelFactory<IIPCChartPoint>.CreateChannel(binding, ep);
      CPClassLayout cpClassLayout = ipcChartPnt.GetInjectionData(ProjectName);
      filesOrchestrator = new SortedDictionary<string, CPFileCodeOrchestrator>();
      CPFileCodeOrchestrator fileCPOrk = null;
      foreach (CPTraceVar traceVar in cpClassLayout.traceVarPos.Values)
      {
        string traceVarName = "__cp_trace_" + traceVar.name;
        foreach (var tracePos in traceVar.traceVarTracePos)
        {
          fileCPOrk = GetFileOrchestrator(tracePos.fileName);
          fileCPOrk.AddTransform(tracePos.pos.lineNum - 1, tracePos.pos.linePos - 1, traceVarName + ".trace();");
        }
        fileCPOrk = GetFileOrchestrator(traceVar.defPos.fileName);
        fileCPOrk.AddTransform(traceVar.defPos.pos.lineNum, traceVar.defPos.pos.linePos,
          "cptracer::tracer_elem_impl<" + traceVar.type + "> " + traceVarName + ";");
        if (traceVar.traceVarInitPos.Count == 0 && traceVar.injConstructorPos != null)
        {
          CPFileCodeOrchestrator fileConstrOrk = GetFileOrchestrator(traceVar.injConstructorPos.fileName);
          fileConstrOrk.AddTransform(traceVar.injConstructorPos.pos.lineNum, traceVar.injConstructorPos.pos.linePos
            ,
            "public:\n" + traceVar.className + "(){\n" + traceVarName + ".reg((uint64_t) &" + traceVar.name + ", \"" +
            traceVar.name + "\", cptracer::type_id<" + traceVar.type + ">::id);\n" + "}");
        }
        else
        {
          foreach (var varInitPos in traceVar.traceVarInitPos)
          {
            fileCPOrk = GetFileOrchestrator(varInitPos.fileName);
            fileCPOrk.AddTransform(varInitPos.pos.lineNum, varInitPos.pos.linePos
              ,
              traceVarName + ".reg((uint64_t) &" + traceVar.name + ", \"" + traceVar.name + "\", cptracer::type_id<" +
              traceVar.type + ">::id);");
          }
        }
      }
      foreach (var traceInclPos in cpClassLayout.traceInclPos)
      {
        fileCPOrk = GetFileOrchestrator(traceInclPos.Key);
        fileCPOrk.AddTransform(traceInclPos.Value.lineNum, traceInclPos.Value.linePos, "#include \"..\\tracer\\tracer.h\"");
      }
      foreach (var inclFilePos in cpClassLayout.includesPos)
      {
        fileCPOrk = GetFileOrchestrator(inclFilePos.Value.pos.fileName);
        ITextTransform inclTrans = new TextTransformReplace(inclFilePos.Value.inclOrig, inclFilePos.Value.inclReplace);
        fileCPOrk.AddTransform(inclFilePos.Value.pos.pos.lineNum, inclFilePos.Value.pos.pos.linePos, inclTrans);
      }
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

      return true;
    }
  }
}

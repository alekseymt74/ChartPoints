using Microsoft.Internal.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChartPoints.CPServices.decl;

namespace ChartPoints
{

  public class CPProps : ICPProps
  {
    private MemoryStream propsStream;

    public void Save(Microsoft.VisualStudio.OLE.Interop.IStream pOptionsStream)
    {
      DataStreamFromComStream pStream = new DataStreamFromComStream(pOptionsStream);
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
      formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;//!!! while develop !!!
      formatter.Serialize(pStream, new CPPropsDeSerializator());
    }

    public void SetPropsStream(Microsoft.VisualStudio.OLE.Interop.IStream _propsStream)
    {
      DataStreamFromComStream pStream = new DataStreamFromComStream(_propsStream);
      if (propsStream != null)
        propsStream = null;
      propsStream = new MemoryStream();
      pStream.CopyTo(propsStream);
    }

    public bool Load()
    {
      if (propsStream == null || propsStream.Length == 0)
        return false;
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
      formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;//!!! while develop !!!
      propsStream.Position = 0;
      CPPropsDeSerializator cpPropsDeser;
      Func<CPPropsDeSerializator> desearalize = () => (CPPropsDeSerializator)formatter.Deserialize(propsStream);
      if (SynchronizationContext.Current != null)
      {
        System.Threading.Tasks.Task.Run(desearalize);
      }
      else
      {
        cpPropsDeser = desearalize();
      }

      return true;
    }
  }

  sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
  {
    public override Type BindToType(string assemblyName, string typeName)
    {
      //https://techdigger.wordpress.com/2007/12/22/deserializing-data-into-a-dynamically-loaded-assembly/
      Type typeToDeserialize = null;
      try
      {
        string ToAssemblyName = assemblyName.Split(',')[0];
        Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in Assemblies)
        {
          if (asm.FullName.Split(',')[0] == ToAssemblyName)
          {
            typeToDeserialize = asm.GetType(typeName);
            break;
          }
        }
      }
      catch (System.Exception exception)
      {
        throw exception;
      }

      return typeToDeserialize;
    }
  }

  [Serializable]
  public class CPPropsDeSerializator : ISerializable
  {
    public CPPropsDeSerializator() { }
    protected CPPropsDeSerializator(SerializationInfo info, StreamingContext context)
    {
      try
      {
        UInt32 projsCount = info.GetUInt32("projPoints.Count");
        for (uint p = 0; p < projsCount; ++p)
        {
          string projName = info.GetString("projName_" + p.ToString());
          Globals.processor.RemoveChartPoints(projName);
          UInt32 filesCount = info.GetUInt32("filePoints.Count_" + p.ToString());
          if (filesCount > 0)
          {
            IProjectChartPoints projCPs = Globals.processor.GetProjectChartPoints(projName);
            if (projCPs == null)
              Globals.processor.AddProjectChartPoints(projName, out projCPs);
            if (projCPs != null)
            {
              for (uint f = 0; f < filesCount; ++f)
              {
                string fileName = info.GetString("fileName_" + p.ToString() + f.ToString());
                IFileChartPoints fPnts = projCPs.AddFileChartPoints(fileName);
                if (fPnts != null)
                {
                  UInt32 linesCount = info.GetUInt32("linePoints.Count_" + p.ToString() + f.ToString());
                  for (uint l = 0; l < linesCount; ++l)
                  {
                    UInt32 lineNum = info.GetUInt32("lineNum_" + p.ToString() + f.ToString() + l.ToString());
                    UInt32 linePos = info.GetUInt32("linePos_" + p.ToString() + f.ToString() + l.ToString());
                    ILineChartPoints lPnts = fPnts.AddLineChartPoints((int)lineNum, (int)linePos);
                    if (lPnts != null)
                    {
                      UInt32 cpsCount = info.GetUInt32("cpsPoints.Count_" + p.ToString() + f.ToString() + l.ToString());
                      for (uint cp = 0; cp < cpsCount; ++cp)
                      {
                        IChartPoint chartPnt = null;
                        string uniqueName = info.GetString("uniqueName_" + p.ToString() + f.ToString() + l.ToString() + cp.ToString());
                        bool enabled = info.GetBoolean("enabled_" + p.ToString() + f.ToString() + l.ToString() + cp.ToString());
                        if (lPnts.AddChartPoint(uniqueName, out chartPnt))
                          chartPnt.SetStatus(enabled ? EChartPointStatus.SwitchedOn : EChartPointStatus.SwitchedOff);
                      }
                    }
                    if (lPnts.Count == 0)
                      fPnts.RemoveLineChartPoints(lPnts);
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("projPoints.Count", (UInt32)Globals.processor.data.projPoints.Count);
      int p = 0;
      foreach (IProjectChartPoints projCPs in Globals.processor.data.projPoints)
      {
        info.AddValue("projName_" + p.ToString(), projCPs.data.projName);
        info.AddValue("filePoints.Count_" + p.ToString(), (UInt32)projCPs.filePoints.Count);
        int f = 0;
        foreach (IFileChartPoints fileCPs in projCPs.filePoints)
        {
          info.AddValue("fileName_" + p.ToString() + f.ToString(), fileCPs.data.fileName);
          info.AddValue("linePoints.Count_" + p.ToString() + f.ToString(), (UInt32)fileCPs.linePoints.Count);
          int l = 0;
          foreach (ILineChartPoints lineCPs in fileCPs.linePoints)
          {
            info.AddValue("lineNum_" + p.ToString() + f.ToString() + l.ToString(), (UInt32)lineCPs.data.pos.lineNum);
            info.AddValue("linePos_" + p.ToString() + f.ToString() + l.ToString(), (UInt32)lineCPs.data.pos.linePos);
            info.AddValue("cpsPoints.Count_" + p.ToString() + f.ToString() + l.ToString(), (UInt32)lineCPs.chartPoints.Count);
            int c = 0;
            foreach (IChartPoint cp in lineCPs.chartPoints)
            {
              info.AddValue("uniqueName_" + p.ToString() + f.ToString() + l.ToString() + c.ToString(), cp.data.uniqueName);
              info.AddValue("enabled_" + p.ToString() + f.ToString() + l.ToString() + c.ToString(), cp.data.enabled);
              ++c;
            }
            ++l;
          }
          ++f;
        }
        ++p;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cper
{
  class Program
  {
    static void Main(string[] args)
    {
      string vsixInstPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var p = new System.Diagnostics.Process();
      p.StartInfo.FileName = "cmd.exe";
      p.StartInfo.Arguments = String.Format("/C {0} //RegServer", vsixInstPath + "\\CPTracer.exe");
      p.StartInfo.Verb = "runas";
      p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      p.Start();
    }
  }
}

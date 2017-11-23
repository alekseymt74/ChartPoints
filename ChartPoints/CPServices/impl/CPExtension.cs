using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ChartPoints.CPServices.decl;

namespace ChartPoints.CPServices.impl
{
    class CPExtension : ICPExtension
    {
        public string GetVSIXInstallPath()
        {
            string codebase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codebase, UriKind.Absolute);

            return System.IO.Path.GetDirectoryName(uri.LocalPath).ToLower();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace ChartPointsTests
{
  public class TestProject
  {
    private DTE dte;
    private ProjectItem projHeaderItem;
    private ProjectItem projSrcItem;

    public TestProject(DTE _dte) { dte = _dte; }
    public bool Open(string solutionName)
    {
      IVsSolution solutionService = (IVsSolution)VsIdeTestHostContext.ServiceProvider.GetService(typeof(IVsSolution));
      if (solutionService == null)
        return false;
      if (solutionService.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, solutionName) != VSConstants.S_OK)
        return false;
      Project proj = dte.Solution.Projects.Item(1);
      ProjectItem projHeadersFolder = proj.ProjectItems.Item("Header Files");
      if (projHeadersFolder == null)
        return false;
      projHeaderItem = projHeadersFolder.ProjectItems.Item("temp_utest.h");
      if (projHeaderItem == null)
        return false;
      ProjectItem projSrcFolder = proj.ProjectItems.Item("Source Files");
      if (projSrcFolder == null)
        return false;
      projSrcItem = projSrcFolder.ProjectItems.Item("temp_utest.cpp");
      if (projSrcItem == null)
        return false;

      return true;
    }

    public TextSelection OpenTestHeaderItem()
    {
      projHeaderItem.Open(EnvDTE.Constants.vsViewKindCode);
      TextSelection ts = null;
      if (dte.ActiveDocument != null)
      {
        ts = (TextSelection) dte.ActiveDocument.Selection;
        ts.SelectAll();
        ts.Delete();
        SaveCurrentItem();
      }

      return ts;
    }

    public bool SaveCurrentItem()
    {
      if (dte != null)
        return (dte.ActiveDocument.Save() == vsSaveStatus.vsSaveSucceeded);
      return false;
    }
  }
}

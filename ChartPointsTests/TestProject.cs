using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace ChartPointsTests
{
  public class TestProjectItem
  {
    private ProjectItem projItem;
    private TextSelection ts;

    public TextPoint ActivePoint { get { return ts.ActivePoint; } }// !!!CHECK!!! ts
    public TestProjectItem(ProjectItem _projItem)
    {
      projItem = _projItem;
    }
    public void SetPos(int pos)
    {
      ts.MoveToAbsoluteOffset(pos);
    }

    public bool SetContent(string txt)
    {
      if (!projItem.IsOpen)
        return false;
      ts.Text = txt;

      return Save();
    }
    public bool AddContent(string txt)
    {
      if (!projItem.IsOpen)
        return false;
      ts.Text += txt;

      return Save();
    }

    public bool Open(bool clear)
    {
      Window wnd = projItem.Open(EnvDTE.Constants.vsViewKindCode);
      if (wnd == null)
        return false;
      ts = (TextSelection)projItem.Document.Selection;
      if (clear)
        return Clear();

      return true;
    }

    public bool Clear()
    {
      if (!projItem.IsOpen)
        return false;
      ts.SelectAll();
      ts.Delete();

      return true;
    }

    public bool Save()
    {
      if (!projItem.IsOpen)
        return false;

      return (projItem.Document.Save() == vsSaveStatus.vsSaveSucceeded);
    }
  }

  public class TestProject
  {
    private DTE dte;
    private TestProjectItem _projHeaderItem;
    private TestProjectItem _projSrcItem;

    public TestProjectItem projHeaderItem { get { return _projHeaderItem; } }
    public TestProjectItem projSrcItem { get { return _projSrcItem; } }

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
      ProjectItem  vsProjHeaderItem = projHeadersFolder.ProjectItems.Item("temp_utest.h");
      if (vsProjHeaderItem == null)
        return false;
      _projHeaderItem = new TestProjectItem(vsProjHeaderItem);
      ProjectItem projSrcFolder = proj.ProjectItems.Item("Source Files");
      if (projSrcFolder == null)
        return false;
      ProjectItem vsProjSrcItem = projSrcFolder.ProjectItems.Item("temp_utest.cpp");
      if (vsProjSrcItem == null)
        return false;
      _projSrcItem = new TestProjectItem(vsProjSrcItem);

      return true;
    }
  }
}

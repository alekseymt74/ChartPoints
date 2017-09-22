using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartPoints;
using Microsoft.VisualStudio.VCCodeModel;

namespace CP
{
  namespace Code
  {
    public interface ICodeEelement
    {
      string name { get; }
    }

    public interface IClassVarElement : ICodeEelement
    {
      string type { get; }
    }

    public interface IClassElement : ICodeEelement
    {
      ICollection<IClassVarElement> variables { get; }
      IClassVarElement GetVar(string name);
      VCCodeClass to();//!!! HACK !!!
    }

    public interface ICheckElem
    {
      IClassVarElement var { get; }
      bool exists { get; }
      void Toggle(bool val);
      bool HasChanged();
    }

    public interface ICheckCPPoint
    {
      ICollection<ICheckElem> elems { get; }
      bool Synchronize();
    }

    public interface IModel
    {
      IClassElement GetClass(string name);
      ICheckCPPoint CheckCursorPos();
    }

  } // namespace CodeModel
} // namespace CP

using System.Collections.Generic;
using ChartPoints;

namespace CP
{
  namespace Code
  {
    public interface ICodeEelement
    {
      string name { get; }
      string uniqueName { get; }
    }

    public interface IClassVarElement : ICodeEelement
    {
      string type { get; }
      CPTraceVar CalcInjectionPoints(CPClassLayout cpClassLayout, string className, string _fname, ITextPosition pos, out bool needDeclare);
      ICPEvent<ClassVarElemTrackerArgs> classVarChangedEvent { get; set; }
      ICPEvent<ClassVarElemTrackerArgs> classVarDeletedEvent { get; set; }
    }

    public interface IClassMethodElement : ICodeEelement
    {
    }

    public interface IClassElement : ICodeEelement
    {
      ICollection<IClassVarElement> variables { get; }
      IClassVarElement GetVar(string name);
      void CalcInjectionPoints(CPClassLayout cpClassLayout, CPTraceVar traceVar, bool needDeclare);
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

    public interface IFileElem : ICodeEelement
    {
      void Synchronize();
      IClassElement GetClassFromFilePos(int lineNum, int linePos);
    }

    public interface IModel
    {
      IFileElem GetFile(string fileName);
      //IClassElement GetClass(string name);
      ICheckCPPoint CheckCursorPos();
      void CalcInjectionPoints(CPClassLayout cpInjPoints, CPTraceVar traceVar);
    }

  } // namespace CodeModel
} // namespace CP

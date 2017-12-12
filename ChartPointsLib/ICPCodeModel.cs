using System.Collections.Generic;
using ChartPoints;
using ChartPoints.CPServices.decl;
using ChartPoints.CPServices.impl;

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
      bool Validate(string name);
    }

    public interface IClassElement : ICodeEelement
    {
      IClassVarElement GetVar(string name);
      void CalcInjectionPoints(CPClassLayout cpClassLayout, CPTraceVar traceVar, bool needDeclare);
      //bool Validate();
    }

    public interface IClassMethodElement : ICodeEelement
    {
      IClassElement GetClass();
      bool Validate(ITextPosition pos);
    }

    public interface IFileElem : ICodeEelement
    {
      void Synchronize();
      IClassMethodElement GetMethodFromFilePos(int lineNum, int linePos);
      bool Validate(string fileName);
    }

    public interface IModel
    {
      void StoreClass(IClassElement classElem);
      IFileElem GetFile(string fileName);
      ICheckCPPoint CheckCursorPos();
      void CalcInjectionPoints(CPClassLayout cpInjPoints, CPTraceVar traceVar);
      bool Validate();
    }

//############################################################################

    public interface ICheckElem
    {
      //IClassVarElement var { get; }
      string name { get; }
      string uniqueName { get; }
      string type { get; }
      bool exists { get; }
      void Toggle(bool val);
      bool HasChanged();
    }

    public interface ICheckCPPoint
    {
      ICollection<ICheckElem> elems { get; }
      bool Synchronize();
    }

  } // namespace CodeModel
} // namespace CP

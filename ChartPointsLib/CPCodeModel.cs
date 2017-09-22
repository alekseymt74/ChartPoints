using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCCodeModel;
using ChartPoints;

namespace CP
{
  namespace Code
  {

    public abstract class CodeModelEnt<T> : ICodeEelement// where T : VCCodeElement, VCCodeFunction, VCCodeClass
    {
      public string name { get; }
      protected T ent;

      public CodeModelEnt(CodeElement _codeElem)
      {
        ent = (T)_codeElem;
      }
      public CodeModelEnt(T _codeElem)
      {
        ent = _codeElem;
      }

    }

    public class ClassVarElement : CodeModelEnt<VCCodeVariable>, IClassVarElement
    {
      public new string name { get { return ent.FullName; } }
      public string type { get { return ent.TypeString; } }

      public ClassVarElement(CodeElement _codeElem) : base(_codeElem)
      {}
      public ClassVarElement(VCCodeVariable _codeElem) : base(_codeElem)
      {}
    }

    public class ClassElement : CodeModelEnt<VCCodeClass>, IClassElement
    {
      public new string name { get { return ent.FullName; } }
      protected ISet<IClassVarElement> theVars;

      public ICollection<IClassVarElement> variables
      {
        get
        {
          if (theVars == null)
          {
            theVars = new SortedSet<IClassVarElement>(Comparer<IClassVarElement>.Create((lh, rh) => (String.Compare(lh.name, rh.name, StringComparison.Ordinal))));
            foreach (CodeElement _elem in ent.Variables)
            {
              VCCodeVariable varElem = (VCCodeVariable)_elem;
              if (varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefBool
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefByte
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefChar
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDecimal
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDouble
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefFloat
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefInt
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefLong
                  || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefShort)// !!! CHECK OTHER SUITABLE TYPES !!!
              {
                theVars.Add(new ClassVarElement(varElem));
              }
            }
          }
          return theVars;
        }
      }
      public ClassElement(CodeElement _codeElem) : base(_codeElem)
      {}
      public ClassElement(VCCodeClass _codeElem) : base(_codeElem)
      {}

      public IClassVarElement GetVar(string name)
      {
        foreach (CodeElement _var in ent.Variables)
        {
          if (_var.Name == name)
            return new ClassVarElement(_var);
        }

        return null;
      }
      public VCCodeClass to()
      {
        return ent;
      }
    }

    public class CheckElem : ICheckElem
    {
      public IClassVarElement var { get; }
      public bool exists { get; set; }
      private bool existsOrig;

      public CheckElem(IClassVarElement _var, bool _exists)
      {
        var = _var;
        existsOrig = exists = _exists;
      }

      public void Toggle(bool val)
      {
        exists = val;
      }

      public bool HasChanged()
      {
        return (exists != existsOrig);
      }
    }

    public class CheckCPPoint : ICheckCPPoint
    {
      private List<ICheckElem> theElems;
      private string projName;
      private string fileName;
      private string fileFullName;
      private int lineNum;
      private int linePos;
      private IClassElement classElem;
      private IProjectChartPoints pPnts;
      private IFileChartPoints fPnts;
      private ILineChartPoints lPnts;

      public ICollection<ICheckElem> elems
      {
        get
        {
          if (theElems == null)
          {
            theElems = new List<ICheckElem>();
            ICollection<IClassVarElement> vcElems = classElem.variables;
            if (vcElems.Count > 0)
            {
              pPnts = ChartPoints.Globals.processor.GetProjectChartPoints(projName);
              fPnts = pPnts?.GetFileChartPoints(fileName);
              lPnts = fPnts?.GetLineChartPoints(lineNum);
              foreach (IClassVarElement var in vcElems)
              {
                bool exists = false;
                if (lPnts != null)
                {
                  if (lPnts.GetChartPoint(var.name) != null)
                    exists = true;
                }
                theElems.Add(new CheckElem(var, exists));
              }
            }
          }
          return theElems;
        }
      }

      public CheckCPPoint(IClassElement _classElem, string _projName, string _fileName, string _fileFullName, int _lineNum, int _linePos)
      {
        classElem = _classElem;
        projName = _projName;
        fileName = _fileName;
        fileFullName = _fileFullName;
        lineNum = _lineNum;
        linePos = _linePos;
      }

      public bool Synchronize()
      {
        bool changed = false;
        foreach (ICheckElem _elem in elems)
        {
          if (_elem.HasChanged())
          {
            if(pPnts == null)
              ChartPoints.Globals.processor.AddProjectChartPoints(projName, out pPnts);
            if(fPnts == null)
              fPnts = pPnts.AddFileChartPoints(fileName, System.IO.Path.GetFullPath(fileFullName).ToLower());
            if(lPnts == null)
              lPnts = fPnts.AddLineChartPoints(lineNum, linePos);
            changed = lPnts.SyncChartPoint(_elem, classElem);
          }
        }

        return changed;
      }
    }

    public class Model : IModel
    {
      protected VCCodeModel vcCodeModel;
      protected static Events2 evs2;
      protected static CodeModelEvents cmEvs;
      protected ISet<IClassElement> classes = new SortedSet<IClassElement>(Comparer<IClassElement>.Create((lh, rh) => (String.Compare(lh.name, rh.name, StringComparison.Ordinal))));

      public Model(CodeModel _codeModel, Events2 _evs2)
      {
        vcCodeModel = (VCCodeModel) _codeModel;
        evs2 = _evs2;
        cmEvs = evs2.CodeModelEvents;
        cmEvs.ElementChanged += OnElementChanged;
        cmEvs.ElementDeleted += OnElementDeleted;
        cmEvs.ElementAdded += OnElementAdded;
      }

      public void OnElementAdded(CodeElement element)
      {
        ;
      }

      public void OnElementDeleted(object parent, CodeElement element)
      {
        ;
      }

      public void OnElementChanged(CodeElement element, vsCMChangeKind change)
      {
        ;
      }

      public IClassElement GetClass(string name)
      {
        IClassElement cpClass = classes.FirstOrDefault((v) => (v.name == name));
        if (cpClass == null)
        {
          foreach (CodeElement _class in vcCodeModel.Classes)
          {
            if (_class.Name == name)
            {
              cpClass = new ClassElement(_class);
              classes.Add(cpClass);
              return cpClass;
            }
          }
        }

        return cpClass;
      }

      public ICheckCPPoint CheckCursorPos()
      {
        ICheckCPPoint checkPnt = null;
        Document activeDoc = ChartPoints.Globals.dte.ActiveDocument;
        string projName = activeDoc.ProjectItem.ContainingProject.Name;
        TextSelection sel = (TextSelection) activeDoc.Selection;
        TextPoint caretPnt = (TextPoint) sel.ActivePoint;
        VCCodeElement targetClassElem;
        for (;;)
        {
          // checks if we are in text document
          if (activeDoc == null)
            break;
          var textDoc = activeDoc.Object() as TextDocument;
          if (textDoc == null)
            break;
          // we work only with project items
          ProjectItem projItem = activeDoc.ProjectItem;
          if (projItem == null)
            break;
          // only c++ items
          FileCodeModel fcModel = projItem.FileCodeModel;
          if (fcModel == null)
            break;
          if (fcModel.Language != CodeModelLanguageConstants.vsCMLanguageVC)
            break;
          vcCodeModel.Synchronize();// !!! MOVE TO METHOD ???
          // chartpoint allowed only in class methods
          CodeElement elem = fcModel.CodeElementFromPoint(caretPnt, vsCMElement.vsCMElementFunction);
          if (elem == null)
            break;
          if (elem.Kind != vsCMElement.vsCMElementFunction)
            break;
          VCCodeFunction targetFunc = (VCCodeFunction)elem;
          if (targetFunc == null)
            break;
          // check that we are in method definition (not declaration) in case of declaration in one file & definition in other
          if (!targetFunc.File.Equals(activeDoc.FullName, StringComparison.OrdinalIgnoreCase))
            break;
          // we are working only with class methods not global function
          targetClassElem = (VCCodeElement)targetFunc.Parent;
          if (targetClassElem == null)
            break;
          if (targetClassElem.Kind != vsCMElement.vsCMElementClass)
          {
            targetClassElem = null;
            break;
          }
          VCCodeClass ownerClass = (VCCodeClass)targetClassElem;

          TextPoint startFuncBody = targetFunc.StartPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
          TextPoint endFuncBody = targetFunc.EndPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
          EditPoint startPnt = startFuncBody.CreateEditPoint();
          EditPoint endPnt = endFuncBody.CreateEditPoint();
          startPnt.FindPattern("{", (int)vsFindOptions.vsFindOptionsBackwards);
          endPnt.FindPattern("}");
          if ((caretPnt.Line > startPnt.Line && caretPnt.Line < endPnt.Line) ||
              (caretPnt.Line == startPnt.Line && caretPnt.LineCharOffset >= startPnt.LineCharOffset) ||
              (caretPnt.Line == endPnt.Line && caretPnt.LineCharOffset <= endPnt.LineCharOffset))
          {
            // Oh, oh you're in the body, now.. (c)
            checkPnt = new CheckCPPoint(new ClassElement(ownerClass), projName, activeDoc.Name, System.IO.Path.GetFullPath(activeDoc.FullName).ToLower(), caretPnt.Line, caretPnt.LineCharOffset);
            return checkPnt;
          }
          break;
        }

        return checkPnt;
      }
    }

  } // namespace CodeModel
} // namespace CP

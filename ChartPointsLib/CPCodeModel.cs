using System;
using System.Collections.Generic;
using System.IO;
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
      public string uniqueName { get; }
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
      public new string name { get { return ent.Name; } }
      public new string uniqueName { get { return ent.FullName; } }
      public string type { get { return ent.TypeString; } }
      public ICPEvent<ClassVarElemTrackerArgs> classVarChangedEvent { get; set; } = new CPEvent<ClassVarElemTrackerArgs>();
      public ICPEvent<ClassVarElemTrackerArgs> classVarDeletedEvent { get; set; } = new CPEvent<ClassVarElemTrackerArgs>();

      public ClassVarElement(CodeElement _codeElem) : base(_codeElem)
      {}
      public ClassVarElement(VCCodeVariable _codeElem) : base(_codeElem)
      {}
      public CPTraceVar CalcInjectionPoints(CPClassLayout cpClassLayout, string className, string _fname, ITextPosition pos, out bool needDeclare)
      {
        CPTraceVar traceVar = null;
        if (!cpClassLayout.traceVarPos.TryGetValue(name, out traceVar))
        {
          needDeclare = true;
          // add trace pos data
          traceVar = new CPTraceVar()
          {
            name = name,
            type = type,
            className = className
          };
          cpClassLayout.traceVarPos.Add(traceVar.name, traceVar);
          // define trace var definition placement
          traceVar.defPos.fileName = ent.ProjectItem.Name;
          traceVar.defPos.pos.lineNum = ent.EndPoint.Line - 1;
          traceVar.defPos.pos.linePos = ent.EndPoint.LineCharOffset - 1;
        }
        else
          needDeclare = false;
        // add trace pos
        traceVar.traceVarTracePos.Add(new FilePosPnt()
        {
          fileName = _fname,
          pos = { lineNum = pos.lineNum, linePos = pos.linePos }
        });
        // check and add if need file containing original variable declaration
        TextPos traceInclPos = null;
        if (!cpClassLayout.traceInclPos.TryGetValue(ent.ProjectItem.Name, out traceInclPos))
          cpClassLayout.traceInclPos.Add(ent.ProjectItem.Name, new TextPos() { lineNum = 0, linePos = 0 });

        return traceVar;
      }
    }

    public class ClassMethodElement : CodeModelEnt<VCCodeFunction>, IClassMethodElement
    {
      public new string name { get { return ent.Name; } }
      public new string uniqueName { get { return ent.FullName; } }
      public ClassMethodElement(CodeElement _codeElem) : base(_codeElem)
      { }
      public ClassMethodElement(VCCodeFunction _codeElem) : base(_codeElem)
      { }
    }

    public class ClassElement : CodeModelEnt<VCCodeClass>, IClassElement
    {
      public new string name { get { return ent.Name; } }
      public new string uniqueName { get { return ent.FullName; } }
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

      public IClassVarElement GetVar(string uniqueName)
      {
        foreach (CodeElement _var in ent.Variables)
        {
          if (_var.FullName == uniqueName)
            return new ClassVarElement(_var);
        }

        return null;
      }

      public void CalcInjectionPoints(CPClassLayout cpClassLayout, CPTraceVar traceVar, bool needDeclare)
      {
        if (needDeclare)
        {
          // find all places, where this file included
          CodeElement theFunc = null;
          // find & store all constructors init points of this class
          foreach (CodeElement _func in /*vcClass*/ent.Functions)
          {
            if (_func.Name == ent.Name)
            {
              theFunc = _func;
              VCCodeFunction vcFunc = (VCCodeFunction) _func;
              EditPoint pnt = _func.StartPoint.CreateEditPoint();
              if (pnt.FindPattern("{"))
                traceVar.traceVarInitPos.Add(new FilePosPnt()
                {
                  fileName = _func.ProjectItem.Name,
                  pos = {lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset}
                });
            }
          }
          // if no constructor found add default one
          if (traceVar.traceVarInitPos.Count == 0)
          {
            EditPoint pnt = ent.StartPoint.CreateEditPoint();
            if (pnt.FindPattern("{"))
            {
              traceVar.injConstructorPos = new FilePosPnt()
              {
                fileName = ent.ProjectItem.Name,
                pos = {lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset}
              };
            }
          }
        }
      }
    }

    public class CheckElem : ICheckElem
    {
      public IClassVarElement var { get; }
      public bool exists { get; set; }
      private readonly bool existsOrig;

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
      private IClassMethodElement methodElem;
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
                  if (lPnts.GetChartPoint(var.uniqueName) != null)
                    exists = true;
                }
                theElems.Add(new CheckElem(var, exists));
              }
            }
          }
          return theElems;
        }
      }

      public CheckCPPoint(IClassElement _classElem, IClassMethodElement _methodElem, string _projName, string _fileName, string _fileFullName, int _lineNum, int _linePos)
      {
        classElem = _classElem;
        methodElem = _methodElem;
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
              fPnts = pPnts.AddFileChartPoints(fileName);//, System.IO.Path.GetFullPath(fileFullName).ToLower());
            if(lPnts == null)
              lPnts = fPnts.AddLineChartPoints(lineNum, linePos);
            changed = lPnts.SyncChartPoint(_elem);//, classElem);
          }
        }

        return changed;
      }
    }

    public class FileElem : IFileElem
    {
      public string name { get { return projItem.Name; } }
      public string uniqueName { get; }
      private EnvDTE.ProjectItem projItem;

      public FileElem(EnvDTE.ProjectItem _projItem)
      {
        projItem = _projItem;
        uniqueName = System.IO.Path.GetFullPath(projItem.FileNames[0]).ToLower();
      }
      public void Synchronize()
      {
        ;
      }

      private ClassElement GetTraceVarClassElem(CodeElement codeElem, int lineNum, int linePos)
      {
        if (codeElem.Kind == vsCMElement.vsCMElementFunction)
        {
          if ((lineNum > codeElem.StartPoint.Line && lineNum < codeElem.EndPoint.Line) ||
              (lineNum == codeElem.StartPoint.Line && linePos >= codeElem.StartPoint.LineCharOffset) ||
              (lineNum == codeElem.EndPoint.Line && linePos <= codeElem.EndPoint.LineCharOffset))
          {
            VCCodeFunction targetFunc = (VCCodeFunction)codeElem;
            if (targetFunc != null)
            {
              VCCodeElement targetClassElem = (VCCodeElement)targetFunc.Parent;
              if (targetClassElem != null && targetClassElem.Kind == vsCMElement.vsCMElementClass)
                return new ClassElement((VCCodeClass) targetClassElem);
            }
          }
        }
        else
        {
          foreach (CodeElement classCodeElem in codeElem.Children)
          {
            ClassElement theClass = GetTraceVarClassElem(classCodeElem, lineNum, linePos);
            if (theClass != null)
              return theClass;
          }
        }

        return null;
      }

      private ClassElement GetTraceVarClassElem(FileCodeModel fcm, int lineNum, int linePos)
      {
        foreach (CodeElement codeElem in fcm.CodeElements)
        {
          ClassElement theClass = GetTraceVarClassElem(codeElem, lineNum, linePos);
          if (theClass != null)
            return theClass;//!!! NOTIFY MODEL !!!
        }

        return null;
      }

      public IClassElement GetClassFromFilePos(int lineNum, int linePos)
      {
        return GetTraceVarClassElem(projItem.FileCodeModel, lineNum, linePos);
      }
    }

    public class Model : IModel
    {
      protected VCCodeModel vcCodeModel;
      protected EnvDTE.Project proj;
      protected static Events2 evs2;
      protected static CodeModelEvents cmEvs;
      protected ISet<IClassElement> classes = new SortedSet<IClassElement>(Comparer<IClassElement>.Create((lh, rh) => (String.Compare(lh.name, rh.name, StringComparison.Ordinal))));

      public Model(string projName, Events2 _evs2)
      {
        foreach (Project _proj in ChartPoints.Globals.dte.Solution.Projects)
        {
          if (_proj.Name == projName)
          {
            proj = _proj;
            break;
          }
        }
        if(proj != null)
          vcCodeModel = (VCCodeModel)proj.CodeModel;
        evs2 = _evs2;
        cmEvs = evs2.CodeModelEvents;
        //cmEvs.ElementChanged += OnElementChanged;
        //cmEvs.ElementDeleted += OnElementDeleted;
        //cmEvs.ElementAdded += OnElementAdded;
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

      private EnvDTE.ProjectItem GetProjItem(EnvDTE.ProjectItems projItems, string fileName)
      {
        if (projItems == null)
          return null;
        foreach (EnvDTE.ProjectItem projItem in projItems)
        {
          if (projItem.Name == fileName)
            return projItem;
          EnvDTE.ProjectItem theProjItem = GetProjItem(projItem.ProjectItems, fileName);
          if (theProjItem != null)
            return theProjItem;
        }

        return null;
      }
      public IFileElem GetFile(string fileName)
      {
        EnvDTE.ProjectItem theProjItem = GetProjItem(proj.ProjectItems, fileName);
        if(theProjItem != null)
          return new FileElem(theProjItem);
        return null;
      }

      //public IClassElement GetClass(string name)
      //{
      //  IClassElement cpClass = classes.FirstOrDefault((v) => (v.name == name));
      //  if (cpClass == null)
      //  {
      //    foreach (CodeElement _class in vcCodeModel.Classes)
      //    {
      //      if (_class.Name == name)
      //      {
      //        cpClass = new ClassElement(_class);
      //        classes.Add(cpClass);
      //        return cpClass;
      //      }
      //    }
      //  }

      //  return cpClass;
      //}

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
            int linePos = (caretPnt.Line == startPnt.Line ? startPnt.LineCharOffset /*+ 1*/ : 1/*0*/);
            checkPnt = new CheckCPPoint(new ClassElement(ownerClass), new ClassMethodElement(targetFunc), projName, activeDoc.Name, System.IO.Path.GetFullPath(activeDoc.FullName).ToLower(), caretPnt.Line, linePos);

            return checkPnt;
          }
          break;
        }

        return checkPnt;
      }

      public void CalcInjectionPoints(CPClassLayout cpInjPoints, CPTraceVar traceVar)
      {
        foreach (var inclStmt in vcCodeModel.Includes)
        {
          VCCodeInclude vcinclStmt = (VCCodeInclude)inclStmt;
          CPInclude incl = null;
          string fname = Path.GetFileName(vcinclStmt.File);
          if (!cpInjPoints.includesPos.TryGetValue(new Tuple<string, string>(vcinclStmt.Name, fname), out incl))
          {
            if (vcinclStmt.Name == traceVar.defPos.fileName)
            {
              // define include placement
              FilePosText inclPos = new FilePosText()
              {
                fileName = fname,
                pos = { lineNum = vcinclStmt.StartPoint.Line - 1, linePos = vcinclStmt.StartPoint.LineCharOffset },
                posEnd = { lineNum = vcinclStmt.EndPoint.Line - 1, linePos = vcinclStmt.EndPoint.LineCharOffset }
              };
              incl = new CPInclude()
              {
                inclOrig = vcinclStmt.Name,
                inclReplace = "__cp__." + vcinclStmt.Name,
                pos = inclPos
              };
              cpInjPoints.includesPos.Add(new Tuple<string, string>(vcinclStmt.Name, incl.pos.fileName), incl);
            }
          }
        }
      }
    }

  } // namespace CodeModel
} // namespace CP

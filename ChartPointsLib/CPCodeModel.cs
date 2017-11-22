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
using System.Diagnostics;

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
      private ClassElement classElem;
      public ICPEvent<ClassVarElemTrackerArgs> classVarChangedEvent { get; set; } = new CPEvent<ClassVarElemTrackerArgs>();
      public ICPEvent<ClassVarElemTrackerArgs> classVarDeletedEvent { get; set; } = new CPEvent<ClassVarElemTrackerArgs>();

      public ClassVarElement(CodeElement _codeElem, ClassElement _classElem) : base(_codeElem)
      {
        classElem = _classElem;
      }
      public ClassVarElement(VCCodeVariable _codeElem, ClassElement _classElem) : base(_codeElem)
      {
        classElem = _classElem;
      }
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
            uniqueName = uniqueName,
            type = type,
            className = className
          };
          cpClassLayout.traceVarPos.Add(traceVar.name, traceVar);
          // define trace var definition placement
          traceVar.defPos.fileName = ent.ProjectItem.Name;
          traceVar.defPos.pos.lineNum = ent.EndPoint.Line;// - 1;
          traceVar.defPos.pos.linePos = ent.EndPoint.LineCharOffset;// - 1;
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

      public bool Validate(string uniqueName)
      {
        //if (ent.IsZombie)
        ent = classElem.GetCodeVar(uniqueName);
        if (ent == null)
          return false;

        return true;
      }

    }

    public class ClassElement : CodeModelEnt<VCCodeClass>, IClassElement
    {
      public new string name { get { return ent.Name; } }
      public new string uniqueName { get { return ent.FullName; } }
      private ClassMethodElement classCodeElem;
      public ClassElement(CodeElement _codeElem, ClassMethodElement _classCodeElem) : base(_codeElem)
      {
        classCodeElem = _classCodeElem;
      }
      public ClassElement(VCCodeClass _codeElem, ClassMethodElement _classCodeElem) : base(_codeElem)
      {
        classCodeElem = _classCodeElem;
      }

      static public bool IsTypeSupported(VCCodeVariable varElem)
      {
        if (varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefBool
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefByte
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefChar
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDecimal
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefDouble
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefFloat
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefInt
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefLong
            || varElem.Type.TypeKind == vsCMTypeRef.vsCMTypeRefShort

            || varElem.TypeString.Contains("signed ")
            //|| varElem.TypeString == "unsigned short"
            //|| varElem.TypeString == "unsigned short int"
            //|| varElem.TypeString == "unsigned int"
            //|| varElem.TypeString == "unsigned long"
            //|| varElem.TypeString == "unsigned long int"
            //|| varElem.TypeString == "unsigned long long"
            //|| varElem.TypeString == "unsigned long long int"
            //|| varElem.TypeString == "unsigned char"
            ////|| varElem.TypeString == "signed short" // vsCMTypeRefInt
            //|| varElem.TypeString == "signed short int"
            ////|| varElem.TypeString == "signed int" //vsCMTypeRefInt
            ////|| varElem.TypeString == "signed long" // vsCMTypeRefInt
            //|| varElem.TypeString == "signed long int"
            //|| varElem.TypeString == "signed long long"
            //|| varElem.TypeString == "signed long long int"
            ////|| varElem.TypeString == "signed char" // vsCMTypeRefInt

            || varElem.TypeString.Contains("long ")
            //|| varElem.TypeString == "long int"
            //|| varElem.TypeString == "long long"
            //|| varElem.TypeString == "long long int"

            //|| varElem.TypeString == "short" // vsCMTypeRefShort
            || varElem.TypeString == "short int"
            //|| varElem.TypeString == "int" // vsCMTypeRefInt
            //|| varElem.TypeString == "signed" // vsCMTypeRefInt
            || varElem.TypeString == "unsigned"
            || varElem.TypeString == "long"
            //|| varElem.TypeString == "double" // vsCMTypeRefDouble
            //|| varElem.TypeString == "float" // vsCMTypeRefFloat
            //|| varElem.TypeString == "bool" // vsCMTypeRefBool
            //|| varElem.TypeString == "char" // vsCMTypeRefChar
            || varElem.TypeString == "int8_t"   // signed char
            || varElem.TypeString == "int16_t"  // short              
            || varElem.TypeString == "int32_t"  // int                
            || varElem.TypeString == "int64_t"  // long long          
            || varElem.TypeString == "uint8_t"  // unsigned char      
            || varElem.TypeString == "uint16_t" // unsigned short     
            || varElem.TypeString == "uint32_t" // unsigned int       
            || varElem.TypeString == "uint64_t" // unsigned long long 
          )
        {
          //Debug.WriteLine(varElem.TypeString + " : " + varElem.Type.TypeKind.ToString());
          return true;
        }
        //else
        //  Debug.WriteLine(varElem.TypeString);

        return false;
      }

      public VCCodeVariable GetCodeVar(string uniqueName)
      {
        try
        {
          foreach (CodeElement _elem in ent.Variables)
          {
            VCCodeVariable varElem = (VCCodeVariable)_elem;
            if (varElem.FullName == uniqueName)
            {
              if (IsTypeSupported(varElem))
              {
                return varElem;
              }
            }
          }
        }
        catch(Exception ex)
        {
          Debug.WriteLine(ex);
        }

        return null;
      }

      public IClassVarElement GetVar(string uniqueName)
      {
        VCCodeVariable varElem = GetCodeVar(uniqueName);
        if (varElem != null)
          return (new ClassVarElement(varElem, this));

        return null;
      }

      public void CalcInjectionPoints(CPClassLayout cpClassLayout, CPTraceVar traceVar, bool needDeclare)
      {
        if (needDeclare)
        {
          // find all places, where this file included
          CodeElement theFunc = null;
          // find & store all constructors init points of this class
          foreach (CodeElement _func in ent.Functions)
          {
            if (_func.Name == ent.Name)
            {
              theFunc = _func;
              VCCodeFunction vcFunc = (VCCodeFunction)_func;
              EditPoint pnt = _func.StartPoint.CreateEditPoint();
              if (pnt.FindPattern("{"))
                traceVar.traceVarInitPos.Add(new FilePosPnt()
                {
                  fileName = _func.ProjectItem.Name,
                  pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset }
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
                pos = { lineNum = pnt.Line - 1, linePos = pnt.LineCharOffset }
              };
            }
          }
        }
      }

      public bool Validate(VCCodeClass targetClass)
      {
        ent = targetClass;

        return true;
      }

    }

    public class ClassMethodElement : CodeModelEnt<VCCodeFunction>, IClassMethodElement
    {
      public new string name { get { return ent.Name; } }
      public new string uniqueName { get { return ent.FullName; } }
      protected ClassElement classElem;
      private FileElem fileElem;

      public ClassMethodElement(CodeElement _codeElem, FileElem _fileElem) : base(_codeElem)
      {
        classElem = GetClassImpl();
        fileElem = _fileElem;
      }

      public ClassMethodElement(VCCodeFunction _codeElem, FileElem _fileElem) : base(_codeElem)
      {
        classElem = GetClassImpl();
        fileElem = _fileElem;
      }

      private VCCodeClass GetClassCodeElem()
      {
        VCCodeElement targetClassElem = (VCCodeElement)ent.Parent;
        if (targetClassElem != null && targetClassElem.Kind == vsCMElement.vsCMElementClass)
          return (VCCodeClass)targetClassElem;

        return null;
      }

      private ClassElement GetClassImpl()
      {
        if (classElem == null)
        {
          VCCodeClass targetClassElem = GetClassCodeElem();
          if (targetClassElem != null)
            classElem = new ClassElement(targetClassElem, this);
        }

        return classElem;
      }

      public IClassElement GetClass()
      {
        return GetClassImpl();
      }

      public bool Validate(ITextPosition pos)
      {
        ent = fileElem.GetMethodCodeElemFromFilePos(pos);
        if (ent == null)
          return false;
        VCCodeClass targetClass = GetClassCodeElem();
        if (targetClass != null && !classElem.Validate(targetClass))
          return false;

        return true;
      }
    }

    public class FileElem : IFileElem
    {
      public string name { get { return projItem.Name; } }
      public string uniqueName { get; }
      private EnvDTE.ProjectItem projItem;
      private Model codeModel;

      public FileElem(EnvDTE.ProjectItem _projItem, Model _codeModel)
      {
        projItem = _projItem;
        codeModel = _codeModel;
        uniqueName = System.IO.Path.GetFullPath(projItem.FileNames[0]).ToLower();
      }
      public void Synchronize()
      {
        ;
      }

      private VCCodeFunction GetMethodFromFilePos(CodeElement codeElem, int lineNum, int linePos)
      {
        if (codeElem.Kind == vsCMElement.vsCMElementFunction)
        {
          VCCodeFunction targetFunc = codeElem as VCCodeFunction;
          if (targetFunc != null)
          {
            TextPoint startFuncBody = targetFunc.StartPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
            TextPoint endFuncBody = targetFunc.EndPointOf[vsCMPart.vsCMPartBody, vsCMWhere.vsCMWhereDefinition];
            EditPoint startPnt = startFuncBody.CreateEditPoint();
            EditPoint endPnt = endFuncBody.CreateEditPoint();
            startPnt.FindPattern("{", (int)vsFindOptions.vsFindOptionsBackwards);
            endPnt.FindPattern("}");
            if ((lineNum > startPnt.Line && lineNum < endPnt.Line) ||
                (lineNum == startPnt.Line && linePos >= startPnt.LineCharOffset) ||
                (lineNum == endPnt.Line && linePos <= endPnt.LineCharOffset))
            {
                return targetFunc;
            }
          }

          //if ((lineNum > codeElem.StartPoint.Line && lineNum < codeElem.EndPoint.Line) ||
          //  (lineNum == codeElem.StartPoint.Line && linePos >= codeElem.StartPoint.LineCharOffset) ||
          //  (lineNum == codeElem.EndPoint.Line && linePos <= codeElem.EndPoint.LineCharOffset))
          //{
          //  VCCodeFunction targetFunc = (VCCodeFunction)codeElem;
          //  if (targetFunc != null)
          //    return targetFunc;
          //}
        }
        else
        {
          foreach (CodeElement classCodeElem in codeElem.Children)
          {
            VCCodeFunction theMethod = GetMethodFromFilePos(classCodeElem, lineNum, linePos);
            if (theMethod != null)
              return theMethod;
          }
        }

        return null;
      }

      private VCCodeFunction GetMethodFromFilePos(FileCodeModel fcm, int lineNum, int linePos)
      {
        if (fcm == null)
          return null;
        foreach (CodeElement codeElem in fcm.CodeElements)
        {
          VCCodeFunction theMethod = GetMethodFromFilePos(codeElem, lineNum, linePos);
          if (theMethod != null)
            return theMethod;
        }

        return null;
      }

      public IClassMethodElement GetMethodFromFilePos(int lineNum, int linePos)
      {
        VCCodeFunction targetFunc = GetMethodFromFilePos(projItem.FileCodeModel, lineNum, linePos);
        if (targetFunc != null)
        {
          ClassMethodElement method = new ClassMethodElement(targetFunc, this);
          return method;
        }

        return null;
      }

      public VCCodeFunction GetMethodCodeElemFromFilePos(ITextPosition pos)
      {
        return GetMethodFromFilePos(projItem.FileCodeModel, pos.lineNum, pos.linePos);
      }

      public bool Validate(string fileName)
      {
        projItem = codeModel.GetProjectItem(fileName);
        if (projItem != null)
          return true;

        return false;
      }

    }

    public class Model : IModel
    {
      public ISet<IFileElem> fileElems { get; } = new SortedSet<IFileElem>(Comparer<IFileElem>.Create((lh, rh) => (String.Compare(lh.uniqueName, rh.uniqueName, StringComparison.Ordinal))));
      protected VCCodeModel vcCodeModel;
      protected EnvDTE.Project proj;
      protected static Events2 evs2;
      protected static CodeModelEvents cmEvs;
      private string projName;
      protected ISet<IClassElement> classes = new SortedSet<IClassElement>(Comparer<IClassElement>.Create((lh, rh) => (String.Compare(lh.uniqueName, rh.uniqueName, StringComparison.Ordinal))));

      public Model(string _projName, Events2 _evs2)
      {
        projName = _projName;
        proj = GetProject(projName);
        if (proj != null)
          vcCodeModel = (VCCodeModel)proj.CodeModel;
        evs2 = _evs2;
        cmEvs = evs2.CodeModelEvents;
        //cmEvs.ElementChanged += OnElementChanged;
        //cmEvs.ElementDeleted += OnElementDeleted;
        //cmEvs.ElementAdded += OnElementAdded;
      }

      protected EnvDTE.Project GetProject(string projName)
      {
        EnvDTE.Project theProj = null;
        foreach (Project _proj in ChartPoints.Globals.dte.Solution.Projects)
        {
          if (_proj.Name == projName)
          {
            theProj = _proj;
            break;
          }
        }

        return theProj;
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

      public void StoreClass(IClassElement classElem)
      {
        classes.Add(classElem);
      }

      public EnvDTE.ProjectItem GetProjectItem(string fileName)
      {
        return GetProjItem(proj.ProjectItems, fileName);
      }

      public IFileElem GetFile(string fileName)
      {
        string fUniqueName = System.IO.Path.GetFullPath(fileName).ToLower();
        IFileElem fElem = fileElems.FirstOrDefault((v) => (v.uniqueName == fUniqueName));
        if (fElem != null)
          return fElem;
        EnvDTE.ProjectItem theProjItem = GetProjItem(proj.ProjectItems, fileName);
        if (theProjItem != null)
        {
          fElem = new FileElem(theProjItem, this);
          fileElems.Add(fElem);
          return fElem;
        }
        return null;
      }

      public ICheckCPPoint CheckCursorPos()
      {
        ICheckCPPoint checkPnt = null;
        Document activeDoc = ChartPoints.Globals.dte.ActiveDocument;
        string projName = activeDoc.ProjectItem.ContainingProject.Name;
        TextSelection sel = (TextSelection)activeDoc.Selection;
        TextPoint caretPnt = (TextPoint)sel.ActivePoint;
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
            int linePos = (caretPnt.Line == startPnt.Line ? startPnt.LineCharOffset + 1 : 1/*0*/);
            checkPnt = new CheckCPPoint(ownerClass, projName, activeDoc.Name, System.IO.Path.GetFullPath(activeDoc.FullName).ToLower(), caretPnt.Line, linePos);

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

      public bool Validate()
      {
        proj = GetProject(projName);
        if (proj == null)
          return false;
        if (proj.CodeModel == null)
          return false;
        vcCodeModel = (VCCodeModel)proj.CodeModel;
        if (vcCodeModel == null)
          return false;
        vcCodeModel.Synchronize();
        vcCodeModel.SynchronizeFiles();

        return true;
      }

    }

    //########################################################################

    public class CheckElem : ICheckElem
    {
      public string name { get { return var.Name; } }
      public string uniqueName { get { return var.FullName; } }
      public string type { get { return var.TypeString; } }
      public bool exists { get; set; }
      private VCCodeVariable var { get; }
      private readonly bool existsOrig;

      public CheckElem(VCCodeVariable _var, bool _exists)
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
      private VCCodeClass ownerClass;
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
            if (ownerClass.Variables.Count != 0)
            {
              pPnts = ChartPoints.Globals.processor.GetProjectChartPoints(projName);
              fPnts = pPnts?.GetFileChartPoints(fileName);
              lPnts = fPnts?.GetLineChartPoints(lineNum);
              foreach (CodeElement var in ownerClass.Variables)
              {
                VCCodeVariable vcVar = (VCCodeVariable)var;
                if (ClassElement.IsTypeSupported(vcVar))
                {
                  bool exists = false;
                  if (lPnts != null)
                  {
                    if (lPnts.GetChartPoint(vcVar.FullName) != null)
                      exists = true;
                  }
                  theElems.Add(new CheckElem(vcVar, exists));
                }
              }
            }
          }
          return theElems;
        }
      }

      public CheckCPPoint(VCCodeClass _ownerClass, string _projName, string _fileName, string _fileFullName, int _lineNum, int _linePos)
      {
        ownerClass = _ownerClass;
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
            if (pPnts == null)
              ChartPoints.Globals.processor.AddProjectChartPoints(projName, out pPnts);
            if (fPnts == null)
              fPnts = pPnts.AddFileChartPoints(fileName);
            if (lPnts == null)
              lPnts = fPnts.AddLineChartPoints(lineNum, linePos);
            changed = lPnts.SyncChartPoint(_elem);
          }
        }

        return changed;
      }
    }


  } // namespace CodeModel
} // namespace CP

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CPInstrBuildTask {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CPInstrBuildTask.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #include &quot;__cp__.tracer.h&quot;
        ///#include &lt;windows.h&gt;
        ///
        ///namespace cptracer
        ///{
        ///
        ///  //int32_t type_id&lt; short &gt;::id                  = 0;
        ///  int32_t type_id&lt; short int &gt;::id = 0;
        ///  //int32_t type_id&lt; signed short &gt;::id           = 0;
        ///  //int32_t type_id&lt; signed short int &gt;::id       = 0;
        ///  //int32_t type_id&lt; unsigned short &gt;::id         = 1;
        ///  int32_t type_id&lt; unsigned short int &gt;::id = 1;
        ///  int32_t type_id&lt; int &gt;::id = 2;
        ///  //int32_t type_id&lt; signed &gt;::id                 = 2;
        ///  //int32_t type_id&lt; signed i [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string @__cp___tracer {
            get {
                return ResourceManager.GetString("__cp___tracer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #ifndef TRACER_H
        ///#define TRACER_H
        ///
        ///#include &lt;stdint.h&gt;
        ///#include &lt;memory&gt;
        ///
        ///#if defined(CPTRACER_EXPORTS)
        ///#define CPTRACER_DLL_API __declspec( dllexport )
        ///#elif defined(CPTRACER_EXPORTS)
        ///#define CPTRACER_DLL_API __declspec( dllimport )
        ///#else
        ///#define CPTRACER_DLL_API
        ///#endif
        ///
        ///namespace cptracer
        ///{
        ///  // variable type id wrapper declaration
        ///  template&lt; typename T &gt; class type_id
        ///  {
        ///  public:
        ///    static int32_t id;
        ///  };
        ///
        ///  // common tracer class visible to instrumented code
        ///  class CPTRACER [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string @__cp___tracer_h {
            get {
                return ResourceManager.GetString("__cp___tracer_h", resourceCulture);
            }
        }
    }
}

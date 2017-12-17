using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPoints
{
  public interface ICPProps
  {
    void Save(Microsoft.VisualStudio.OLE.Interop.IStream pOptionsStream);
    void SetPropsStream(Microsoft.VisualStudio.OLE.Interop.IStream _propsStream);
    bool Load();
  }
}

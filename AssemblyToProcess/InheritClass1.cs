using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using TestBaseClassLib;

namespace AssemblyToProcess
{
    [Tracking]
    public class InheritClass1:BaseClass
    {
        public int IntProp { get; set; }
    }
}

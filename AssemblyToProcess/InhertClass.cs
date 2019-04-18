using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{
    [Tracking]
    public class InheritedClass : ZBaseClass4
    {
        public string Prop2 { get; set; }

    }

    [Tracking]
    public class ZBaseClass2: ZBaseClass1
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp3 { get; set; }
    }

    [Tracking]
    public class ZBaseClass1
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp1 { get; set; }
    }

    [Tracking]
    public class ZBaseClass4 : ZBaseClass3
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp4 { get; set; }
    }

    [Tracking]
    public class ZBaseClass3: ZBaseClass2
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp2 { get; set; }
    }

   
}

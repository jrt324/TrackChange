using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{

    [Tracking]
    public class BaseClass1
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp1 { get; set; }
    }

    [Tracking]
    public class BaseClass11: BaseClass1
    {
        //public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public string BaseProp2 { get; set; }
    }

    [Tracking]
    public class InheritedClass : BaseClass11
    {
        public string Prop2 { get; set; }

        //string _prop2;
        //public string Prop2
        //{
        //    get
        //    {
        //        return _prop2;
        //    }
        //    set
        //    {
        //        ModifiedProperties["Prop2"] = true;
        //        _prop2 = value;
        //    }
        //}
    }
}

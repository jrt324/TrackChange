using System;
using System.Collections.Generic;
using Framework;

namespace AssemblyToProcess
{
    //[Tracking]
    public class Class1 : ITrackable
    {
        private System.Reflection.MethodBase m { get; set; }
        public Dictionary<string, bool> ModifiedProperties { get; set; }
        public bool IsTracking { get; set; }

        private DateTime? k__BackingField;
        public DateTime? Prop1
        {
            get { return k__BackingField; }
            set
            {
                var isEql = object.Equals(Prop1, value);
                if (!isEql)
                {
                    ModifiedProperties["Prop1"] = true;
                }
                k__BackingField = value;
            }
        }
    }


    public class Class2 
    {
       
        public Dictionary<string, bool> ModifiedProperties => new Dictionary<string, bool>();

        public bool IsTracking { get; set; }

        private string __prop1;


        public string Prop1
        {
            get { return __prop1; }
            set
            {
                if (Prop1 != value)
                {
                    ModifiedProperties["Prop1"] = true;
                }
                __prop1 = value;
            }
        }
    }



    [Tracking]
    public class Class3
    {
  
        public DateTime? Prop1 { get; set; }

    
        public string Test2 { get; set; }

        public int IntVal1 { get; set; }
        public int? IntVal2 { get; set; }

    }


}

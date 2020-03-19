
using System;
using System.Collections.Generic;
using Framework;

namespace TestBaseClassLib
{
    [Tracking]
    public class BaseClass : ITrackable
    {
        private string __prop1;


        public string Prop1
        {
            get { return __prop1; }
            set
            {
                // if (Prop1 != value)
                // {
                //     ModifiedProperties["Prop1"] = true;
                // }
                // var isEqual = !object.Equals(__prop1 , value);
                ModifiedProperties["Prop1"] = !object.Equals(__prop1, value);
                __prop1 = value;
            }
        }

        public virtual Dictionary<string, bool> ModifiedProperties { get; set; } = new Dictionary<string, bool>();

        public virtual bool IsTracking { get; set; }
    }
}

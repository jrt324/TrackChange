using System;
using System.Collections.Generic;

namespace Framework
{
    public class TrackingAttribute : Attribute
    {

    }

    public interface ITrackable
    {
        /// <summary>
        /// 改变的属性
        /// </summary>
        Dictionary<string, bool> ModifiedProperties { get; set; }

        /// <summary>
        /// 是否跟踪状态
        /// </summary>
        bool IsTracking { get; set; }
    }
}
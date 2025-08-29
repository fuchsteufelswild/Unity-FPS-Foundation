using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class SubGroupAttribute : ToolboxArchetypeAttribute
    {
        public override ToolboxAttribute[] Process()
        {
            return new ToolboxAttribute[]
            {
                new BeginGroupAttribute(),
                new EndGroupAttribute()
            };
        }
    }
}

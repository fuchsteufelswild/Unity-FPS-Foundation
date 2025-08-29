using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class MaxAttribute : PropertyAttribute
    {
        public float MaxValue { get; }
        public MaxAttribute(float maxValue) => MaxValue = maxValue;
    }
}

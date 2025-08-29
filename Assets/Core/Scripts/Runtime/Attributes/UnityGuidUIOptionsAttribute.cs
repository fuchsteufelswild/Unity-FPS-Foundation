using System;
using System.Diagnostics;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class UnityGuidUIOptionsAttribute : Attribute
    {
        public bool DisableForPrefabs { get; }
        public bool HasNewGuidButton { get; }

        public UnityGuidUIOptionsAttribute(bool disableForPrefabs, bool hasNewGuidButton)
        {
            DisableForPrefabs = disableForPrefabs;
            HasNewGuidButton = hasNewGuidButton;
        }
    }
}

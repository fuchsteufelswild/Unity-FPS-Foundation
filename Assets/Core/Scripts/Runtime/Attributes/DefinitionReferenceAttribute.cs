using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class DefinitionReferenceAttribute : PropertyAttribute
    {
        public Type DefinitionType { get; set; }
        public bool HasAssetReference { get; set; }
        public bool HasLabel { get; set; } = true;
        public bool HasIcon { get; set; } = true;

        public string NullElement { get; set; } = "None";
    }
}

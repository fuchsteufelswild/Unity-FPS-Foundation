using System.Diagnostics;
using System;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class OptionalCharacterBehaviourAttribute : Attribute
    {
        public Type[] Types;

        public OptionalCharacterBehaviourAttribute(params Type[] types) => Types = types;
    }
}

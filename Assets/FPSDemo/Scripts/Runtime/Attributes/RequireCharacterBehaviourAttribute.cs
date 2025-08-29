using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class RequireCharacterBehaviourAttribute : Attribute
    {
        public Type[] Types;

        public RequireCharacterBehaviourAttribute(params Type[] types) => Types = types;
    }
}
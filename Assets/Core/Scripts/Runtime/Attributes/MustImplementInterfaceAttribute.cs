using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class MustImplementInterfaceAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; private set; }

        public MustImplementInterfaceAttribute(Type interfaceType)
        {
            if(interfaceType == null || interfaceType.IsInterface == false)
            {
                InterfaceType = null;
            }
            else
            {
                InterfaceType = interfaceType;
            }
        }
    }
}

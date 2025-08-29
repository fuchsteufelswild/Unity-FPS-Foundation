using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class SpriteThumbnailAttribute : PropertyAttribute
    {
        public float Height { get; }

        public SpriteThumbnailAttribute(float height = 0f) => Height = height;
    }
}

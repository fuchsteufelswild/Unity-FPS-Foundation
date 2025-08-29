using System;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public static partial class EditorIcons
    {
        public static readonly Texture RedErrorIcon = EditorGUIUtility.IconContent("console.erroricon").image;
        public static readonly Texture YellowWarningIcon = EditorGUIUtility.IconContent("console.warnicon").image;
        public static readonly Texture BlueInfoIcon = EditorGUIUtility.IconContent("console.infoicon").image;
    }
}
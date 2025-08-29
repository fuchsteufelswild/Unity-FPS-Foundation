using System;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public struct IndentedScope : IDisposable
    {
        private const float DefaultColumnDivider = 0.4f;

        private readonly int _previousIndentLevel;
        private readonly float _previousLabelWidth;
        private readonly float _previousFieldWidth;

        public IndentedScope(float indentAmount = 15f, bool adjustLabelWidth = true, bool adjustFieldWidth = false)
        {
            _previousIndentLevel = EditorGUI.indentLevel;
            _previousLabelWidth = EditorGUIUtility.labelWidth;
            _previousFieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUI.indentLevel++;
            if (adjustLabelWidth)
            {
                EditorGUIUtility.labelWidth = Mathf.Max(0, _previousLabelWidth - indentAmount);
            }

            if(adjustFieldWidth)
            {
                EditorGUIUtility.fieldWidth = Mathf.Max(0, _previousFieldWidth - ((1 - DefaultColumnDivider) * indentAmount));
            }
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = _previousIndentLevel;
            EditorGUIUtility.labelWidth = _previousLabelWidth;
            EditorGUIUtility.fieldWidth = _previousFieldWidth;
        }
    }
}
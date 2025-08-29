using System;
using UnityEngine;

namespace Nexora.Editor
{
    public struct ColorScope : IDisposable
    {
        private readonly Color _previousColor;

        public ColorScope(Color newColor)
        {
            _previousColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose() => GUI.color = _previousColor;
    }

    public struct BackgroundColorScope : IDisposable
    {
        private readonly Color _previousColor;

        public BackgroundColorScope(Color newColor)
        {
            _previousColor = GUI.color;
            GUI.backgroundColor = newColor;
        }

        public void Dispose() => GUI.backgroundColor = _previousColor;
    }

    public struct HandlesColorScope : IDisposable
    {
        private readonly Color _previousColor;

        public HandlesColorScope(Color newColor)
        {
            _previousColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = newColor;
        }

        public void Dispose() => UnityEditor.Handles.color = _previousColor;
    }
}

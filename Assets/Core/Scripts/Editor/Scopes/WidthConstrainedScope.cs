using System;
using UnityEngine;

namespace Nexora.Editor
{
    public struct WidthConstrainedScope : IDisposable
    {
        private readonly float _widthConstraint;

        public WidthConstrainedScope(float widthConstraint)
        {
            _widthConstraint = widthConstraint;
            if(_widthConstraint > 0)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(_widthConstraint));
            }
        }

        public void Dispose()
        {
            if(_widthConstraint > 0)
            {
                GUILayout.EndHorizontal();
            }
        }
    }
}

using System;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    using Editor = UnityEditor.Editor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Wrapper for Unity's Editor system for drawing object inspectors (<see cref="Editor"/>)
    /// Provides interface for deriving inspector UI with automatic editor management.
    /// <br></br> Works both with single or multiple targets to inspect.
    /// </summary>
    /// <remarks>
    /// Throws in actions if the inspector has already been disposed.
    /// </remarks>
    public class UnityEditorInspectorWrapper : IDisposable
    {
        private Editor _managedEditor;
        private bool _isDisposed;
        private bool _hasValidTargets;
        private Vector2 _inspectorScrollPosition;

        public Object[] InspectionTargets
            => _hasValidTargets && _managedEditor != null ? _managedEditor.targets : Array.Empty<Object>();

        public Object PrimaryInspectionTarget
            => _hasValidTargets && _managedEditor != null ? _managedEditor.target : null;

        public bool HasValidInspectionTargets
            => _hasValidTargets && _managedEditor != null && _managedEditor.targets.Length > 0;

        /// <summary>
        /// Creates object inspector with the given initial targets.
        /// </summary>
        /// <param name="targetObjects">Initial objects to inspect, can be empty.</param>
        public UnityEditorInspectorWrapper(params Object[] targetObjects)
        {
            SetInspectionTargets(targetObjects);
        }

        /// <summary>
        /// Sets multiple targets for multi-object editing.
        /// </summary>
        private void SetInspectionTargets(params Object[] targetObjects)
        {
            ThrowIfDisposed();

            if(_managedEditor != null && _managedEditor.targets == targetObjects)
            {
                return;
            }

            _hasValidTargets = targetObjects?.Length > 0;
            Editor.CreateCachedEditor(targetObjects, null, ref  _managedEditor);
        }

        private void ThrowIfDisposed()
        {
            if(_isDisposed)
            {
                throw new ObjectDisposedException(nameof(UnityEditorInspectorWrapper));
            }
        }

        /// <summary>
        /// Sets a single object for inspection.
        /// </summary>
        public void SetInspectionTarget(Object targetObject)
        {
            ThrowIfDisposed();

            if(_managedEditor != null && _managedEditor.target == targetObject)
            {
                return;
            }

            _hasValidTargets = targetObject != null;
            Editor.CreateCachedEditor(targetObject, null, ref _managedEditor);
        }

        /// <summary>
        /// Renders the inspector UI within a scrollable container,
        /// automatically handles the styling and layout.
        /// </summary>
        /// <param name="containerStyle">Style for the scroll view.</param>
        /// <param name="containerLayoutOptions">Layout options for the scroll view.</param>
        public void RenderInspectorUI(GUIStyle containerStyle = null, params GUILayoutOption[] containerLayoutOptions)
        {
            ThrowIfDisposed();

            containerStyle ??= GUIStyle.none;

            _inspectorScrollPosition = GUILayout.BeginScrollView(
                _inspectorScrollPosition,
                containerStyle,
                containerLayoutOptions);

            try
            {
                RenderInspectorContent();
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }

        private void RenderInspectorContent()
        {
            if (_hasValidTargets == false || _managedEditor == null)
            {
                return;
            }
            
            using(new EditorGUI.DisabledScope(_managedEditor.target == null))
            {
                _managedEditor.OnInspectorGUI();
            }
        }

        /// <summary>
        /// Finalizer to cleanup if <see cref="Dispose"/> haven't been called explicitly.
        /// </summary>
        ~UnityEditorInspectorWrapper()
        {
            DisposeInternal();
        }

        /// <summary>
        /// Cleans up all resources and marks this editor as invalid.
        /// </summary>
        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if(_isDisposed)
            {
                return;
            }

            CleanupManagedEditor();

            _isDisposed = true;
            _hasValidTargets = false;

            return;

            void CleanupManagedEditor()
            {
                if(_managedEditor != null )
                {
                    Object.DestroyImmediate(_managedEditor);
                    _managedEditor = null;
                }
            }
        }
    }
}
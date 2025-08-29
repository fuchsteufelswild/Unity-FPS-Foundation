using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using Toolbox;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Interface for views that will work with windows.
    /// </summary>
    public interface IWindowView : IComparable<IWindowView>
    {
        /// <summary>
        /// Display order of the views in the hierarchy, 
        /// lower values appear first.
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// Human-readable name displayed in the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Optional description of the view.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Path of the icon associated with this view, optional.
        /// </summary>
        string IconPath { get; }

        /// <summary>
        /// Renders the view in the main editor area.
        /// </summary>
        void RenderViewContent();

        /// <summary>
        /// Determines if this view can handle or is relevant to the specified 
        /// <paramref name="targetObject"/>.
        /// </summary>
        /// <remarks>
        /// Used for automatic view selection based on selected objects.
        /// </remarks>
        /// <param name="targetObject">The Unity object to test compability with.</param>
        /// <returns>True if this view can work with the object.</returns>
        bool CanHandleObject(Object targetObject);

        /// <summary>
        /// Refreshes view's internal state and cached data.
        /// Called when page becomes visible or needs to update.
        /// </summary>
        void RefreshViewData();

        /// <summary>
        /// Releases resources and performs cleanup when the view is destroyed.
        /// </summary>
        void ReleaseView();

        /// <returns>True if the view has focus.</returns>
        bool HasInputFocus();

        void RequestInputFocus();

        /// <summary>
        /// Get all children <see cref="IWindowView"/> that should appear
        /// under this view in the hierarchy.
        /// </summary>
        /// <returns>Collection of all child views.</returns>
        IEnumerable<IWindowView> GetChildViews();
    }

    public abstract class WindowView : IWindowView
    {
        public virtual int SortOrder => 0;
        public abstract string DisplayName { get; }
        public virtual string Description => string.Empty;
        public virtual string IconPath => string.Empty;

        private Action<IWindowView> _selectOnWindow;

        protected WindowView(Action<IWindowView> selectOnWindow) => _selectOnWindow = selectOnWindow;

        public int CompareTo(IWindowView otherView) => SortOrder.CompareTo(otherView?.SortOrder ?? 0);

        public abstract void RenderViewContent();

        public abstract bool CanHandleObject(Object targetObject);

        public virtual void RefreshViewData() { }

        public virtual void ReleaseView() { }

        public virtual bool HasInputFocus() => false;

        public virtual void RequestInputFocus() { }

        public virtual IEnumerable<IWindowView> GetChildViews() => Array.Empty<IWindowView>();

        protected void RenderViewNavigationLinks(IEnumerable<IWindowView> views, bool includeChildrenViews = true)
        {
            foreach (var view in views)
            {
                // Render self navigation link
                RenderSingleViewLink(view);

                // Render children navigation links under this one, if needed
                if(includeChildrenViews)
                {
                    RenderChildViewLinks(view);
                }
            }
        }

        private void RenderSingleViewLink(IWindowView view)
        {
            GUILayout.Space(2f);

            if(string.IsNullOrEmpty(view.Description) == false)
            {
                EditorGUILayout.HelpBox(view.Description, UnityEditor.MessageType.Info);
            }

            if(GUILayout.Button(view.DisplayName, EditorGUIStyles.GeneralButton))
            {
                _selectOnWindow?.Invoke(view);
            }
        }

        private void RenderChildViewLinks(IWindowView parentView)
        {
            IEnumerable<IWindowView> childViews = parentView.GetChildViews();

            if(childViews.Any())
            {
                using(new EditorGUI.IndentLevelScope())
                {
                    RenderViewNavigationLinks(childViews, includeChildrenViews: false);
                }
                GUILayout.Space(8f);
            }
        }
    }

    /// <summary>
    /// Just a class type to define that the view deriving from it will be a root view.
    /// Views deriving from this class will be treated as root.
    /// </summary>
    public abstract class RootView : WindowView
    {
        protected RootView(Action<IWindowView> selectOnWindow) 
            : base(selectOnWindow)
        {
        }
    }

    public abstract class AssetRootView<T> : RootView
        where T : Object
    {
        private IEnumerable<IWindowView> _childViews;

        protected abstract string ResourcePath { get; }

        protected AssetRootView(Action<IWindowView> selectOnWindow) 
            : base(selectOnWindow)
        {
        }

        /// <summary>
        /// Custom render of the found children. Maybe like a classic folder structure.
        /// </summary>
        public override void RenderViewContent()
        {
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label(DisplayName, EditorGUIStyles.HelpBoxTitle);
            }
        }

        public override IEnumerable<IWindowView> GetChildViews()
        {
            _childViews ??= CreateChildViews();
            return _childViews;
        }

        private IEnumerable<IWindowView> CreateChildViews()
        {
            var childTypes = GetAllValidChildTypes();

            return CreateChildViews(childTypes);
        }

        private List<Type> GetAllValidChildTypes()
        {
            var childTypes = typeof(T).GetAllChildClasses();
            childTypes.RemoveAll(type => type.GetCustomAttribute(typeof(CreateAssetMenuAttribute)) == null);
            return childTypes;
        }

        private UnityObjectInspectorView[] CreateChildViews(List<Type> childTypes)
        {
            var childViews = new UnityObjectInspectorView[childTypes.Count];

            for (int i = 0; i < childViews.Length; i++)
            {
                var childType = childTypes[i];
                string childName = ObjectNames.NicifyVariableName(childType.Name);
                childViews[i] = new UnityObjectInspectorView(childName, childType, 0,
                    () => Resources.LoadAll(ResourcePath, childType).FirstOrDefault());
            }

            return childViews;
        }
    }


    /// <summary>
    /// A specialized <see cref="IWindowView"/> that is targeted at a specific Unity object type.
    /// Creates and manages Editor for a target object.
    /// </summary>
    public sealed class UnityObjectInspectorView : IWindowView
    {
        private readonly UnityEditorInspectorWrapper _objectInspector;
        private readonly Func<Object> _targetObjectProvider;
        private readonly Type _compatibleObjectType;

        public int SortOrder { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public string IconPath { get; } = string.Empty;

        public UnityObjectInspectorView(
            string displayName,
            Type compatibleType,
            int sortOrder,
            Func<Object> objectProvider,
            string description = null)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            _compatibleObjectType = compatibleType ?? throw new ArgumentNullException(nameof(compatibleType));
            _targetObjectProvider = objectProvider ?? throw new ArgumentNullException(nameof(objectProvider));

            SortOrder = sortOrder;
            Description = description ?? string.Empty;

            _objectInspector = new UnityEditorInspectorWrapper();
        }

        public int CompareTo(IWindowView other) => SortOrder.CompareTo(other?.SortOrder ?? 0);

        public bool CanHandleObject(Object targetObject) =>
            IsObjectTypeCompatible(targetObject);

        private bool IsObjectTypeCompatible(Object targetObject)
            => targetObject != null && targetObject.GetType() == _compatibleObjectType;

        public void RenderViewContent()
        {
            EnsureInspectorHasTarget();

            GUILayout.Label(DisplayName, EditorGUIStyles.HelpBoxTitle);
            _objectInspector.RenderInspectorUI(EditorStyles.helpBox);

            return;

            void EnsureInspectorHasTarget()
            {
                if (_objectInspector.HasValidInspectionTargets == false)
                {
                    var targetObject = _targetObjectProvider?.Invoke();
                    _objectInspector.SetInspectionTarget(targetObject);
                }
            }
        }

        public void RefreshViewData() { }

        public void ReleaseView() => _objectInspector?.Dispose();

        public bool HasInputFocus() => false;

        public void RequestInputFocus() { }

        public IEnumerable<IWindowView> GetChildViews() => Array.Empty<IWindowView>();
    }
}

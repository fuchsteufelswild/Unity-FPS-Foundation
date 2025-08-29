using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nexora.Editor
{
    using Object = UnityEngine.Object;

    public interface IWindow
    {
        float WindowWidth { get; }
        float WindowHeight { get; }
        float WindowMinWidth { get; }
        float WindowMinHeight { get; }
        string WindowTitle { get; }
        string WindowIconPath { get; }

        /// <summary>
        /// Selects and focuses on a specific window view that is compatible with <paramref name="targetView"/>.
        /// </summary>
        void SelectWindowView(IWindowView targetView);

        /// <summary>
        /// Finds and selects a <see cref="IWindowView"/> that can handle this type of object.
        /// </summary>
        void SelectCompatibleWindowViewFor(Object targetObject);
    }

    public abstract class Window<T> : 
        EditorWindow,
        IWindow
        where T : Window<T>
    {
        public virtual float WindowWidth { get; } = 800f;

        public virtual float WindowHeight { get; } = 550f;

        public virtual float WindowMinWidth { get; } = 600f;

        public virtual float WindowMinHeight { get; } = 400f;

        public abstract string WindowTitle { get; }

        public virtual string WindowIconPath => null;

        public abstract void SelectCompatibleWindowViewFor(Object targetObject);

        public abstract void SelectWindowView(IWindowView targetView);

        public static T GetOrCreateWindow()
        {
            bool isNewWindow = HasOpenInstances<T>() == false;
            var window = GetWindow<T>();

            if(isNewWindow)
            {
                CenterWindowOnScreen(window);
                window.minSize = new Vector2(window.WindowMinWidth, window.WindowMinHeight);
            }

            var windowIcon = Resources.Load<Texture2D>(window.WindowIconPath);
            window.titleContent = new GUIContent(window.WindowTitle, windowIcon);

            return window;
        }

        protected static void CenterWindowOnScreen(Window<T> window)
        {
            float centerX = (Screen.currentResolution.width - window.WindowWidth) * 0.5f;
            float centerY = (Screen.currentResolution.height - window.WindowHeight) * 0.5f;
            window.position = new Rect(centerX, centerY, window.WindowWidth, window.WindowHeight);
        }
    }

    public abstract class TreeBasedWindow<T> :
        Window<T>
        where T : TreeBasedWindow<T>
    {
        [SerializeField]
        private TreeViewState _windowTreeViewState;

        protected WindowTreeView _windowTreeView;

        protected abstract string TreeViewStateSessionKey { get; }

        public override void SelectWindowView(IWindowView targetView)
        {
            if (targetView == null)
            {
                Debug.LogError("Cannot select null window view");
                return;
            }

            var window = GetOrCreateWindow();
            TreeViewItem matchingTreeItem = window._windowTreeView.FindItemByPredicate(
                item => ((WindowViewTreeItem)item).WindowView?.DisplayName == targetView.DisplayName);

            if(matchingTreeItem == null)
            {
                Debug.LogError($"Window view '{targetView.DisplayName}' is not found in the tree.");
            }

            window._windowTreeView.SelectItem(matchingTreeItem,
                TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);
        }

        public override void SelectCompatibleWindowViewFor(Object targetObject)
        {
            if(targetObject == null)
            {
                Debug.LogError("Cannot select view for a null object");
                return;
            }

            var window = GetOrCreateWindow();
            TreeViewItem compatibleTreeItem = window._windowTreeView.FindItemByPredicate(
                item => ((WindowViewTreeItem)item).WindowView?.CanHandleObject(targetObject) ?? false);

            if(compatibleTreeItem == null)
            {
                Debug.LogError($"No compatible window view is found for '{targetObject.name}' of type {targetObject.GetType().Name}");
                return;
            }

            window._windowTreeView.SelectItem(compatibleTreeItem,
                TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);
        }

        private void OnEnable()
        {
            RestoreTreeViewState();
            InitializeComponents();
        }

        private void RestoreTreeViewState()
        {
            string savedStateJson = SessionState.GetString(TreeViewStateSessionKey, null);
            _windowTreeViewState = string.IsNullOrEmpty(savedStateJson) == false
                ? JsonUtility.FromJson<TreeViewState>(savedStateJson)
                : new TreeViewState();
        }

        protected virtual void InitializeComponents() 
            => _windowTreeView = new WindowTreeView(_windowTreeViewState, SelectWindowView);

        private void OnDisable()
        {
            SaveTreeViewState();
            CleanupStates();
        }

        private void SaveTreeViewState()
        {
            string stateJson = JsonUtility.ToJson(TreeViewStateSessionKey);
            SessionState.SetString(TreeViewStateSessionKey, stateJson);
        }

        protected virtual void CleanupStates() => _windowTreeView?.Dispose();

        /// <summary>
        /// Item of the <see cref="TreeView"/>, each item keeps a reference to a
        /// <see cref="IWindowView"/> so that can render it when requested.
        /// </summary>
        protected sealed class WindowViewTreeItem : TreeViewItem
        {
            public readonly IWindowView WindowView;

            public WindowViewTreeItem(IWindowView windowView, int id, int depth, string displayName)
                : base(id, depth, displayName)
            {
                WindowView = windowView;
            }
        }

        protected sealed class WindowTreeView : EnhancedTreeView
        {
            private WindowViewTreeItem _selectedWindowViewItem;

            private Action<IWindowView> _selectWindowView;

            public WindowViewTreeItem SelectedWindowViewItem => _selectedWindowViewItem;

            public WindowTreeView(TreeViewState state, Action<IWindowView> selectWindowView)
                : base(state)
            {
                _selectWindowView = selectWindowView;
                showBorder = true;
                showAlternatingRowBackgrounds = true;
                Reload();
                EnsureValidSelection();
            }

            public void Dispose()
            {
                ForEach(item => ((WindowViewTreeItem)item).WindowView?.ReleaseView());
            }

            public TreeViewItem FindItemByPredicate(Func<TreeViewItem, bool> predicate)
                => FirstOrDefault(predicate);

            protected override TreeViewItem BuildRoot()
            {
                var rootItem = new WindowViewTreeItem(null, -1, -1, "Root");
                var allWindowViewItems = BuildTreeViewHierarchy();

                // Sets parent/children relationship of all items using their "depth" field.
                SetupParentsAndChildrenFromDepths(rootItem, allWindowViewItems);

                return rootItem;
            }

            /// <summary>
            /// Builds the actual tree view back-end in a single list.  
            /// </summary>
            /// <remarks>
            /// The list structured in a way that it follows the following depth structure:
            /// e.g 0-1-1-1-2-2-3-0-1-2-0
            /// <br></br>
            /// Which means that it is the structure of root followed by its children, and then
            /// another root followed by its children. This way, the tree constructed in a correct manner.
            /// </remarks>
            private IList<TreeViewItem> BuildTreeViewHierarchy()
            {
                IWindowView[] rootWindowViews = DiscoverRootWindowViews();
                var flattenedHierarchy = new List<WindowViewTreeItem>(rootWindowViews.Length);

                foreach (var rootWindowView in rootWindowViews)
                {
                    flattenedHierarchy.AddRange(FlattenTreeViewHierarchy(rootWindowView, depth: 0));
                }

                AssignUniqueIDs(flattenedHierarchy);

                return flattenedHierarchy.ConvertAll(item => (TreeViewItem)item);
            }

            /// <returns>
            /// <see cref="IWindowView"/> array created 
            /// using the types that are derived from <see cref="RootView"/>.
            /// </returns>
            private IWindowView[] DiscoverRootWindowViews()
            {
                var rootViewTypes = typeof(RootView).Assembly.GetTypes()
                    .Where(type => type.IsAbstract == false && type.IsSubclassOf(typeof(RootView)))
                    .ToArray();

                var rootWindowViews = new IWindowView[rootViewTypes.Length];
                for (int i = 0; i < rootWindowViews.Length; i++)
                {
                    rootWindowViews[i] = (IWindowView)Activator.CreateInstance(rootViewTypes[i], _selectWindowView);
                }

                Array.Sort(rootWindowViews);
                return rootWindowViews;
            }

            /// <summary>
            /// Returns collection of <see cref="TreeViewItem"/> that are ordered from first parent to the
            /// chlidren that is deepest in the hierarchy.
            /// </summary>
            private IEnumerable<WindowViewTreeItem> FlattenTreeViewHierarchy(IWindowView windowView, int depth)
            {
                yield return new WindowViewTreeItem(windowView, 0, depth, windowView.DisplayName);

                var orderedChildViews = windowView.GetChildViews().OrderBy(window => window.SortOrder);
                foreach (IWindowView childView in orderedChildViews)
                {
                    foreach (WindowViewTreeItem flattenedChildView in FlattenTreeViewHierarchy(childView, depth + 1))
                    {
                        yield return flattenedChildView;
                    }
                }
            }

            private static void AssignUniqueIDs(List<WindowViewTreeItem> windowViewItems)
            {
                const int maxTreeItemCount = 100;

                for (int i = 0; i < windowViewItems.Count; i++)
                {
                    windowViewItems[i].id = i + maxTreeItemCount;
                }
            }

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                if(selectedIds.Count > 0)
                {
                    TreeViewItem selectedItem = FindItem(selectedIds.First(), rootItem);
                    _selectedWindowViewItem = selectedItem as WindowViewTreeItem;
                    _selectedWindowViewItem?.WindowView?.RefreshViewData();
                }
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;
        }
    }
}
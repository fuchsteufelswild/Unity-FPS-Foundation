using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nexora.Editor
{
    public enum TreeViewTraversalMethod
    {
        DepthFirst = 0,
        BreadthFirst = 1,
        LeafFirst = 2
    }

    /// <summary>
    /// <see cref="TreeView"/> with additional functionality. Handles selections better
    /// with no ghost selection ids, provides searching item/items with specific predicate
    /// or just invokes action on them, while controlling traversal method by <see cref="ITreeViewTraversalStrategy"/>.
    /// </summary>
    /// <remarks>
    /// Selection is handled by <see cref="SelectionManager"/>, and search/invoke is implemented
    /// by <see cref="ITreeViewTraversalStrategy"/> derivations.
    /// </remarks>
    public abstract class EnhancedTreeView : 
        TreeView,
        IEnumerable<TreeViewItem>
    {
        private readonly SelectionManager _selectionManager;
        private readonly ITreeViewTraversalStrategy _defaultTraversalStrategy;

        public event Action<TreeViewItem> ItemSelected;

        public event Action<IList<int>> SelectionValidated;

        public EnhancedTreeView(TreeViewState state, TreeViewTraversalMethod defaultTraversalMethod = TreeViewTraversalMethod.DepthFirst)
            : base(state)
        {
            _selectionManager = new SelectionManager(this);
            _defaultTraversalStrategy = TraversalCore.CreateTraversalStrategy(defaultTraversalMethod);
        }

        public IEnumerator<TreeViewItem> GetEnumerator() => GetEnumerator(_defaultTraversalStrategy);

        public IEnumerator<TreeViewItem> GetEnumerator(ITreeViewTraversalStrategy strategy)
        {
            return strategy?.Traverse(rootItem).GetEnumerator()
                ?? Enumerable.Empty<TreeViewItem>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<TreeViewItem> action, ITreeViewTraversalStrategy traversalStrategy =  null)
        {
            var traversal = traversalStrategy ?? _defaultTraversalStrategy;
            foreach(var item in traversal.Traverse(rootItem))
            {
                action?.Invoke(item);
            }
        }

        public TreeViewItem FirstOrDefault(Func<TreeViewItem, bool> predicate, ITreeViewTraversalStrategy traversalStrategy = null)
        {
            var traversal = traversalStrategy ?? _defaultTraversalStrategy;
            return traversal.Traverse(rootItem).FirstOrDefault(predicate);
        }

        public IEnumerable<TreeViewItem> Where(Func<TreeViewItem, bool> predicate, ITreeViewTraversalStrategy traversalStrategy = null)
        {
            var traversal = traversalStrategy ?? _defaultTraversalStrategy;
            return traversal.Traverse(rootItem).Where(predicate);
        }

        public void SelectItem(TreeViewItem item, TreeViewSelectionOptions selectionOptions = TreeViewSelectionOptions.FireSelectionChanged)
        {
            _selectionManager.SelectItem(item, selectionOptions);
            ItemSelected?.Invoke(item);
        }

        public void SelectItems(IEnumerable<TreeViewItem> items, TreeViewSelectionOptions selectionOptions = TreeViewSelectionOptions.FireSelectionChanged)
        {
            _selectionManager.SelectItems(items, selectionOptions);
        }

        public IEnumerable<TreeViewItem> GetSelectedItems() => _selectionManager.GetSelectedItems();

        protected bool HasValidSelection() => _selectionManager.HasValidSelection();

        protected void EnsureValidSelection()
        {
            _selectionManager.EnsureValidSelection();
            SelectionValidated?.Invoke(GetSelection());
        }

        protected virtual bool IsItemSelectable(TreeViewItem item)
            => item?.id > 0;

        /// <summary>
        /// Handles selection in the <see cref="TreeView"/> by enhancing current functionality. 
        /// Ensuring the selections are always valid; no ghost ids.
        /// </summary>
        protected sealed class SelectionManager
        {
            private const int EmptySelectionID = -1;
            private const int InvalidSelectionID = 0;

            private readonly EnhancedTreeView _treeView;

            private readonly List<int> _selectionCache = new(1) { InvalidSelectionID };

            public SelectionManager(EnhancedTreeView treeView) => _treeView = treeView;

            public void SelectItem(TreeViewItem item, TreeViewSelectionOptions options)
            {
                _selectionCache[0] = item?.id ?? EmptySelectionID;

                _treeView.SetSelection(_selectionCache, options);
            }

            public void SelectItems(IEnumerable<TreeViewItem> items, TreeViewSelectionOptions options)
            {
                _selectionCache.Clear();

                if (items != null)
                {
                    _selectionCache.AddRange(items
                        .Where(item => item != null)
                        .Select(item => item.id));
                }

                if (_selectionCache.IsEmpty())
                {
                    _selectionCache.Add(EmptySelectionID);
                }

                _treeView.SetSelection(_selectionCache, options);
            }

            public void EnsureValidSelection()
            {
                var rootItem = _treeView.rootItem;

                // We are using TreeViewState, so that we can modify selectedIDs
                var selectedIDs = _treeView.state.selectedIDs; 

                if(selectedIDs.Count == 0)
                {
                    _selectionCache[0] = rootItem.hasChildren ? rootItem.children.First().id : EmptySelectionID;
                    _treeView.SetSelection(_selectionCache, TreeViewSelectionOptions.FireSelectionChanged);
                }
                else
                {
                    selectedIDs.RemoveAll(id => _treeView.FindItem(id, _treeView.rootItem) == null);
                    _treeView.SetSelection(selectedIDs, TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);
                }
            }

            /// <summary>
            /// Filters <see cref="TreeView"/> selections and returns.
            /// </summary>
            public IEnumerable<TreeViewItem> GetSelectedItems()
            {
                var selectedIDs = _treeView.GetSelection();

                return selectedIDs?.Select(id => _treeView.FindItem(id, _treeView.rootItem))
                                   .Where(item => item != null)
                                   ?? Enumerable.Empty<TreeViewItem>();
            }

            public bool HasValidSelection()
            {
                var selectedIDs = _treeView.GetSelection();
                return selectedIDs.Count > 0
                    && (selectedIDs.Count == 1 && selectedIDs.First() == InvalidSelectionID) == false;
            }
        }

        public sealed class TraversalOptions
        {
            public static readonly TraversalOptions Default = new();

            public bool SkipRoot { get; set; } = false;

            /// <summary>
            /// Maximum traverse depth, <see langword="null"/> means no limit.
            /// </summary>
            public int? MaxDepth { get; set; } = null;

            public Func<TreeViewItem, bool> ItemFilter { get; set; } = null;
        }
    }
}
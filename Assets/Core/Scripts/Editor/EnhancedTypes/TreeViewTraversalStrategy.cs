using System.Collections.Generic;
using System;
using UnityEditor.IMGUI.Controls;

namespace Nexora.Editor
{
    /// <summary>
    /// Interface that provides a way to traverse method for <see cref="TreeViewItem"/>.
    /// Abides by the optional <see cref="EnhancedTreeView.TraversalOptions{T}"/>. 
    /// </summary>
    /// <typeparam name="T">Type that is derived from <see cref="TreeViewItem"/></typeparam>
    public interface ITreeViewTraversalStrategy
    {
        IEnumerable<TreeViewItem> Traverse(TreeViewItem root);
    }

    /// <summary>
    /// Provides shared functionality, essentially with <see cref="EnhancedTreeView.TraversalOptions{T}"/>.
    /// </summary>
    internal abstract class TraversalCore
    {
        private readonly EnhancedTreeView.TraversalOptions _options;

        public static ITreeViewTraversalStrategy CreateTraversalStrategy(
            TreeViewTraversalMethod traversalMethod,
            EnhancedTreeView.TraversalOptions traversalOptions = null)
        {
            return traversalMethod switch
            {
                TreeViewTraversalMethod.DepthFirst => new DepthFirstTraversal(traversalOptions),
                TreeViewTraversalMethod.BreadthFirst => new BreadthFirstTraversal(traversalOptions),
                TreeViewTraversalMethod.LeafFirst => new LeafFirstTraversal(traversalOptions),
                _ => throw new NotSupportedException()
            };
        }

        protected TraversalCore(EnhancedTreeView.TraversalOptions options)
            => _options = options ?? EnhancedTreeView.TraversalOptions.Default;

        protected bool HasPassedDepth(int depth)
            => _options.MaxDepth.HasValue && depth > _options.MaxDepth.Value;

        protected bool ShouldProcessItem(TreeViewItem item, int depth)
        {
            return _options.SkipRoot && depth == 0
                && FilterItem(item);
        }

        protected virtual bool FilterItem(TreeViewItem item)
            => _options.ItemFilter?.Invoke(item) ?? true;
    }

    internal sealed class DepthFirstTraversal :
        TraversalCore,
        ITreeViewTraversalStrategy
    {
        public DepthFirstTraversal(EnhancedTreeView.TraversalOptions options)
            : base(options)
        { }

        public IEnumerable<TreeViewItem> Traverse(TreeViewItem root)
        {
            foreach (var item in TraverseRecursive(root, 0))
            {
                yield return item;
            }
        }

        private IEnumerable<TreeViewItem> TraverseRecursive(TreeViewItem item, int depth)
        {
            if (item == null)
            {
                yield break;
            }

            if (HasPassedDepth(depth))
            {
                yield break;
            }

            if (ShouldProcessItem(item, depth))
            {
                yield return item;
            }

            if (item.hasChildren)
            {
                foreach (TreeViewItem child in item.children)
                {
                    foreach (TreeViewItem descendant in TraverseRecursive(child, depth + 1))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }

    internal sealed class BreadthFirstTraversal :
        TraversalCore,
        ITreeViewTraversalStrategy
    {
        public BreadthFirstTraversal(EnhancedTreeView.TraversalOptions options)
            : base(options)
        { }

        public IEnumerable<TreeViewItem> Traverse(TreeViewItem root)
        {
            if (root == null)
            {
                yield break;
            }

            var queue = new Queue<(TreeViewItem item, int depth)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (currentItem, depth) = queue.Dequeue();

                if (HasPassedDepth(depth))
                {
                    continue;
                }

                if (ShouldProcessItem(currentItem, depth))
                {
                    yield return currentItem;
                }

                if (currentItem.hasChildren)
                {
                    foreach (TreeViewItem child in currentItem.children)
                    {
                        if (child != null)
                        {
                            queue.Enqueue((child, depth + 1));
                        }
                    }
                }
            }
        }
    }

    internal sealed class LeafFirstTraversal :
        TraversalCore,
        ITreeViewTraversalStrategy
    {
        public LeafFirstTraversal(EnhancedTreeView.TraversalOptions options)
            : base(options)
        { }

        public IEnumerable<TreeViewItem> Traverse(TreeViewItem root)
        {
            foreach (TreeViewItem item in TraverseRecursive(root, 0))
            {
                yield return item;
            }
        }

        private IEnumerable<TreeViewItem> TraverseRecursive(TreeViewItem item, int depth)
        {
            if (item == null)
            {
                yield break;
            }

            if (HasPassedDepth(depth))
            {
                yield break;
            }

            if (item.hasChildren)
            {
                foreach (TreeViewItem child in item.children)
                {
                    foreach (TreeViewItem descendant in TraverseRecursive(child, depth + 1))
                    {
                        yield return descendant;
                    }
                }
            }

            if (ShouldProcessItem(item, depth))
            {
                yield return item;
            }
        }
    }
}
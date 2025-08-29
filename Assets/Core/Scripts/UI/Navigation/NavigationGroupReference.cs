using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.UI
{
    /// <summary>
    /// Reference to another <see cref="NavigationGroupBase"/>, propagates all calls to the concrete
    /// group, acts as a shallow copy proxy.
    /// </summary>
    public sealed class NavigationGroupReference : NavigationGroupBase
    {
        [SerializeField, NotNull, SceneObjectOnly]
        private NavigationGroupBase _referencedGroup;

        public override IReadOnlyList<InteractiveUIElementBase> RegisteredElements => _referencedGroup.RegisteredElements;
        public override InteractiveUIElementBase SelectedElement => _referencedGroup.SelectedElement;
        public override InteractiveUIElementBase HighlightedElement => _referencedGroup.HighlightedElement;

        public override event UnityAction<InteractiveUIElementBase> OnSelectedElementChanged
        {
            add => _referencedGroup.OnSelectedElementChanged += value;
            remove => _referencedGroup.OnSelectedElementChanged -= value;
        }

        public override event UnityAction<InteractiveUIElementBase> OnHighlightedElementChanged
        {
            add => _referencedGroup.OnHighlightedElementChanged += value;
            remove => _referencedGroup.OnHighlightedElementChanged -= value;
        }

        public override void HighlightElement(InteractiveUIElementBase element) => _referencedGroup.HighlightElement(element);

        public override void RegisterElement(InteractiveUIElementBase element) => _referencedGroup.RegisterElement(element);

        public override void SelectElement(InteractiveUIElementBase element) => _referencedGroup.SelectElement(element);

        public override void UnregisterElement(InteractiveUIElementBase element) => _referencedGroup.UnregisterElement(element);

        public override InteractiveUIElementBase GetDefaultElement() => _referencedGroup.GetDefaultElement();
    }
}
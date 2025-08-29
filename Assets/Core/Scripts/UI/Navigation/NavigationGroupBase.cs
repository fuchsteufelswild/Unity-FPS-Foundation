using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.UI
{
    /// <summary>
    /// Abstract class to define a navigation group for that handles navigation between 
    /// <see cref="InteractiveUIElementBase"/> that are registered to it.
    /// <br></br>
    /// Provides interface for Selecting/Highlighting and Registering/Unregistering elements.
    /// As well as events that are fired when Selected/Highlighted element is changed.
    /// </summary>
    /// <remarks>
    /// At one point only one element can be selected and one element can be highlighted.
    /// This is a classic UI behaviour we see in games where you can navigate through elements
    /// by keyboard/controller.
    /// </remarks>
    public abstract class NavigationGroupBase : MonoBehaviour
    {
        public abstract event UnityAction<InteractiveUIElementBase> OnSelectedElementChanged;
        public abstract event UnityAction<InteractiveUIElementBase> OnHighlightedElementChanged;

        public abstract IReadOnlyList<InteractiveUIElementBase> RegisteredElements { get; }
        public abstract InteractiveUIElementBase SelectedElement { get; }
        public abstract InteractiveUIElementBase HighlightedElement { get; }
        
        /// <summary>
        /// Registers <paramref name="element"/> to be a part of this navigation group.
        /// </summary>
        public abstract void RegisterElement(InteractiveUIElementBase element);

        /// <summary>
        /// Unregisters <paramref name="element"/> from this navigation group, it is no
        /// longer bound to this object.
        /// </summary>
        public abstract void UnregisterElement(InteractiveUIElementBase element);
        public abstract void SelectElement(InteractiveUIElementBase element);
        public abstract void HighlightElement(InteractiveUIElementBase element);

        public abstract InteractiveUIElementBase GetDefaultElement();

        public void SelectDefault() => GetDefaultElement().OnSelect(null);

        public void EnableAllElements()
        {
            foreach (var element in RegisteredElements)
            {
                EnableElement(element);
            }
        }
        protected void EnableElement(InteractiveUIElementBase element) => element.gameObject.SetActive(true);

        public void DisableAllElements()
        {
            foreach (var element in RegisteredElements)
            {
                DisableElement(element);
            }
        }
        protected void DisableElement(InteractiveUIElementBase element) => element.gameObject.SetActive(false);
    }
}
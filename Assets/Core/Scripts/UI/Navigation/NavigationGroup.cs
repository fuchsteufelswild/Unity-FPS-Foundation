using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nexora.UI
{
    /// <summary>
    /// Plain navigation group implementation without any specific functionality.
    /// Provides <see cref="InteractiveUIElementBase"/> to be navigated within this group,
    /// controls Selection/Deselection of the elements.
    /// </summary>
    /// <remarks>
    /// Check <see cref="NavigationGroupBase"/> and <see cref="NavigationGroupBase"/> for more details.
    /// </remarks>
    [AddComponentMenu(ComponentMenuPaths.BaseUI + nameof(NavigationGroup))]
    public class NavigationGroup : NavigationGroupBase
    {
        /// <summary>
        /// Determines policy upon <b>objects</b> when objects are registered to the this navigation group.
        /// </summary>
        public enum RegistrationPolicy
        {
            None = 0,
            EnableOnRegister = 1,
            DisableOnRegister = 2
        }

        [Tooltip("Defines the policy about what should be done to do the objects when they are registered to this object.")]
        [SerializeField]
        private RegistrationPolicy _registrationPolicy;

        [SerializeField]
        private bool _autoSelectDefaultElementOnStart;

        [SerializeField, ChildObjectOnly]
        private InteractiveUIElementBase _defaultSelection;

        [SerializeField]
        private SelectionFrame _selectionFrame;

        private readonly List<InteractiveUIElementBase> _registeredElements = new();

        private InteractiveUIElementBase _selectedElement;
        private InteractiveUIElementBase _highlightedElement;

        public override IReadOnlyList<InteractiveUIElementBase> RegisteredElements => _registeredElements;
        public override InteractiveUIElementBase SelectedElement => _selectedElement;
        public override InteractiveUIElementBase HighlightedElement => _highlightedElement;

        public override event UnityAction<InteractiveUIElementBase> OnSelectedElementChanged;
        public override event UnityAction<InteractiveUIElementBase> OnHighlightedElementChanged;

        private void Start()
        {
            if (_autoSelectDefaultElementOnStart)
            {
                EnsureDefaultSelection();
                SelectElement(GetDefaultElement());
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_defaultSelection == null)
            {
                _defaultSelection = GetComponentInChildren<InteractiveUIElementBase>();
            }
        }

        public override InteractiveUIElementBase GetDefaultElement()
        {
            return _defaultSelection != null ? _defaultSelection :
                _registeredElements.Count > 0 ? _registeredElements.First() : null;
        }

        public override void SelectElement(InteractiveUIElementBase element)
        {
            if(element == null)
            {
                return;
            }

            UpdateSelection();
            NotifyListeners();
            return;

            void UpdateSelection()
            {
                // Deselect the previous element and select the new one
                InteractiveUIElementBase previousSelection = _selectedElement;
                _selectedElement = element;

                if(previousSelection != null) 
                { 
                    previousSelection.Deselect(); 
                }

                if (_selectedElement != null)
                {
                    _selectedElement.Select();
                }
            }

            void NotifyListeners()
            {
                // We first notify the derived class, only then we inform the listeners
                OnSelectedElementChangedInternal(element);
                OnSelectedElementChanged?.Invoke(element);
            }
        }

        /// <summary>
        /// Hook for derived classes to react when selected element changes.
        /// Changing UI look or setting up internal state before other listeners
        /// are notified.
        /// </summary>
        /// <remarks>
        /// We can't simply add action to the event, because <b>this action must be
        /// the first one to run.</b>
        /// </remarks>
        protected virtual void OnSelectedElementChangedInternal(InteractiveUIElementBase element) 
        {
            _selectionFrame.SelectedElementChanged(element);
        }

        public override void HighlightElement(InteractiveUIElementBase element)
        {
            if(_highlightedElement == element)
            {
                return;
            }

            _highlightedElement = element;
            OnHighlightedElementChanged?.Invoke(element);
        }

        public override void RegisterElement(InteractiveUIElementBase element)
        {
            if (element == null)
            {
                return;
            }

            _registeredElements.Add(element);
            ApplyRegistrationPolicy();

            return;

            void ApplyRegistrationPolicy()
            {
                switch (_registrationPolicy)
                {
                    case RegistrationPolicy.EnableOnRegister:
                        EnableElement(element);
                        break;
                    case RegistrationPolicy.DisableOnRegister:
                        DisableElement(element);
                        break;
                }
            }
        }

        public override void UnregisterElement(InteractiveUIElementBase element)
        {
            if (element == null)
            {
                return;
            }

            _registeredElements.Remove(element);
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate() => EnsureDefaultSelection();

        /// <summary>
        /// Class to draw a selection frame around a selection in the <see cref="NavigationGroup"/>.
        /// </summary>
        [Serializable]
        private sealed class SelectionFrame
        {
            /// <summary>
            /// Options to determine if frame should match any property of the selected element.
            /// </summary>
            [Flags]
            private enum FrameMatchOptions
            {
                None = 0,
                MatchColor = 1 << 0,
                MatchWidth = 1 << 1,
                MatchHeight = 1 << 2,
                MatchSize = MatchWidth | MatchHeight
            }

            [Tooltip("Determines if there should be selection frame rendered around the selected element.")]
            [SerializeField]
            private bool _shouldRenderSelectionFrame;

            [Tooltip("Object to act as a selection frame around the selected element.")]
            [ShowIf(nameof(_shouldRenderSelectionFrame), true)]
            [SerializeField, NotNull, SceneObjectOnly]
            private RectTransform _selectionFrame;

            [Tooltip("Offset from the selected element, to fit into position better.")]
            [ShowIf(nameof(_shouldRenderSelectionFrame), true)]
            [SerializeField]
            private Vector2 _frameOffset = Vector2.zero;

            [Tooltip("What properties should frame match with the selected object? (Width, Height, Color etc.)")]
            [ShowIf(nameof(_shouldRenderSelectionFrame), true)]
            [SerializeField]
            private FrameMatchOptions _frameMatchOptions = FrameMatchOptions.None;

            public void SelectedElementChanged(InteractiveUIElementBase selectedElement)
            {
                if(_shouldRenderSelectionFrame == false)
                {
                    return;
                }

                if (selectedElement == null)
                {
                    _selectionFrame.gameObject.SetActive(false);
                    return;
                }

                ShowSelectionFrame(selectedElement);
            }

            private void ShowSelectionFrame(InteractiveUIElementBase selectedElement)
            {
                _selectionFrame.gameObject.SetActive(true);
                PositionFrame(selectedElement);
                ResizeFrame(selectedElement);
                MatchFrameColor(selectedElement);
            }

            /// <summary>
            /// Positions the frame to the position of <paramref name="selectedElement"/>.
            /// </summary>
            private void PositionFrame(InteractiveUIElementBase selectedElement)
            {
                _selectionFrame.SetParent(selectedElement.transform);
                _selectionFrame.anchoredPosition = _frameOffset;
                _selectionFrame.localRotation = Quaternion.identity;
                _selectionFrame.localScale = Vector3.one;

                // Ensure frame is at z = 0f, to avoid depth issues
                _selectionFrame.localPosition = _selectionFrame.localPosition.WithZ(0f);
            }

            /// <summary>
            /// Resizes frame width/height to that of <paramref name="selectedElement"/>, if needed by the
            /// <see cref="_frameMatchOptions"/>.
            /// </summary>
            private void ResizeFrame(InteractiveUIElementBase selectedElement)
            {
                bool shouldMatchWidth = EnumFlagsComparer.HasFlag(_frameMatchOptions, FrameMatchOptions.MatchWidth);
                bool shouldMatchHeight = EnumFlagsComparer.HasFlag(_frameMatchOptions, FrameMatchOptions.MatchHeight);

                if(shouldMatchWidth == false && shouldMatchHeight == false)
                {
                    return;
                }

                Vector2 frameSize = _selectionFrame.sizeDelta;
                RectTransform selectedElementTransform = (RectTransform)selectedElement.transform;
                Vector2 selectedElementSize = selectedElementTransform.sizeDelta;

                float newWidth = shouldMatchWidth ? selectedElementSize.x : frameSize.x;
                float newHeight = shouldMatchHeight ? selectedElementSize.y : frameSize.y;

                _selectionFrame.sizeDelta = new Vector2(newWidth, newHeight);
            }

            /// <summary>
            /// Changes color of the frame to that of <paramref name="selectedElement"/>, if needed by the
            /// <see cref="_frameMatchOptions"/>.
            /// </summary>
            private void MatchFrameColor(InteractiveUIElementBase selectedElement)
            {
                if(EnumFlagsComparer.HasFlag(_frameMatchOptions, FrameMatchOptions.MatchColor) == false)
                {
                    return;
                }

                if(_selectionFrame.TryGetComponent<Graphic>(out var frameGraphic) == false)
                {
                    return;
                }

                if(selectedElement.TryGetComponent<Graphic>(out var elementGraphic))
                {
                    frameGraphic.color = elementGraphic.color;
                }
            }
        }
    }
}
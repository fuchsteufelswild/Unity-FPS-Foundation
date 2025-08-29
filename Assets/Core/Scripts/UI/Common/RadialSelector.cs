using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.UI
{
    /// <summary>
    /// A UI component manages a radial menu, where items can be highlighted/selected
    /// via a directional input.
    /// </summary>
    public sealed class RadialSelector : MonoBehaviour
    {
        private const float FullCircleDegrees = 360f;

        [SerializeField, NotNull]
        private NavigationGroupBase _navigationGroup;

        [SerializeField, NotNull]
        private PanelBase _panel;

        [Tooltip("How quickly cursor responds to the input.")]
        [Range(0.1f, 25f)]
        [SerializeField]
        private float _sensitivity = 2.5f;

        [Tooltip("Angular offset for the first item.")]
        [Range(-90f, 90f)]
        [SerializeField]
        private float _angleOffset = -30f;

        [Tooltip("Maximum distance for directional input.")]
        [Range(0.1f, 25f)]
        [SerializeField]
        private float _range = 4f;

        private Vector2 _cursorDirection;

        /// <summary>
        /// Invoked when the highlighted element changes.
        /// </summary>
        public event UnityAction<InteractiveUIElementBase> HighlightedElementChanged
        {
            add => _navigationGroup.OnHighlightedElementChanged += value;
            remove => _navigationGroup.OnHighlightedElementChanged -= value;
        }

        /// <summary>
        /// Invoked when the selected element changes.
        /// </summary>
        public event UnityAction<InteractiveUIElementBase> SelectedElementChanged
        {
            add => _navigationGroup.OnSelectedElementChanged += value;
            remove => _navigationGroup.OnSelectedElementChanged -= value;
        }

        public InteractiveUIElementBase HighlightedElement => _navigationGroup.HighlightedElement;
        public InteractiveUIElementBase SelectedElement => _navigationGroup.SelectedElement;

        /// <summary>
        /// Gets all selectable elements registered to the selector.
        /// </summary>
        public IReadOnlyList<InteractiveUIElementBase> AllElements => _navigationGroup.RegisteredElements;

        public bool IsSelectorVisible => _panel.IsVisible;

        /// <summary>
        /// Shows the selector panel.
        /// </summary>
        public void ShowSelector()
        {
            _cursorDirection = Vector2.zero;
            _panel.Activate();
        }

        /// <summary>
        /// Hides the selector panel.
        /// </summary>
        public void HideSelector() => _panel.Deactivate();

        /// <summary>
        /// Toggles the selector panel.
        /// </summary>
        public void ToggleSelector()
        {
            if(IsSelectorVisible)
            {
                HideSelector();
            }
            else
            {
                ShowSelector();
            }
        }

        /// <summary>
        /// Selects the currently highlighted element in the selector.
        /// </summary>
        /// <returns>If successfuly selected.</returns>
        public bool SelectHighlightedElement()
        {
            if(HighlightedElement == null)
            {
                return false;
            }

            HighlightedElement.Select();
            return true;
        }

        /// <summary>
        /// Selects the element at highlighted <paramref name="index"/>.
        /// </summary>
        /// <returns>If successfuly selected.</returns>
        public bool SelectAtIndex(int index)
        {
            if(index < 0 || index >= AllElements.Count)
            {
                Debug.LogError("Selector index out of bounds.");
                return false;
            }

            InteractiveUIElementBase selectable = AllElements[index];
            SetHighlighted(selectable);
            selectable.Select();
            return true;
        }

        /// <summary>
        /// Sets the highlighted element, updates pointer enter/exit events.
        /// </summary>
        private void SetHighlighted(InteractiveUIElementBase newHighlighted)
        {
            if (HighlightedElement != null)
            {
                HighlightedElement.OnPointerExit(null);
            }

            newHighlighted.OnPointerEnter(null);
        }

        /// <summary>
        /// Updates the highlighted element based on the directional <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Directional input.</param>
        public void UpdateHighlightedSelection(Vector2 input)
        {
            InteractiveUIElementBase newHighlighted = GetHighlightedElement(input);

            if(newHighlighted != null && newHighlighted != HighlightedElement)
            {
                SetHighlighted(newHighlighted);
            }
        }

        private InteractiveUIElementBase GetHighlightedElement(Vector2 input)
        {
            if(input == Vector2.zero || AllElements.IsEmpty())
            {
                return null;
            }

            UpdateCursorDirection(input);
            float cursorAngle = CalculateAngleFromCursor();
            int highlightedIndex = CalculateIndexFromAngle(cursorAngle);

            return AllElements[highlightedIndex];
        }

        /// <summary>
        /// Updates the cursor direction using <paramref name="input"/> with smooth movement.
        /// </summary>
        /// <param name="input">Directional input.</param>
        private void UpdateCursorDirection(Vector2 input)
        {
            Vector2 targetDirection = input.normalized * _range;
            _cursorDirection = Vector2.Lerp(_cursorDirection, targetDirection, Time.deltaTime * _sensitivity);
        }

        /// <summary>
        /// Calculate the current angle of the cursor in the radial selector, using the cursor direction.
        /// (0-360)
        /// </summary>
        /// <returns></returns>
        private float CalculateAngleFromCursor()
        {
            // Calculate angle relative to (0 degrees, Vector2.up)
            float angle = -Vector2.SignedAngle(Vector2.up, _cursorDirection);
            return Mathf.Repeat(angle + _angleOffset, FullCircleDegrees);
        }

        /// <summary>
        /// Calculates the index of the element the cursor is currently showing,
        /// given the <paramref name="angle"/>.
        /// </summary>
        /// <param name="angle">Cursor angle in the radial selector(0-360).</param>
        /// <returns>Selected index for <paramref name="angle"/>.</returns>
        private int CalculateIndexFromAngle(float angle)
        {
            int totalElementCount = AllElements.Count;
            int index = Mathf.FloorToInt(angle * totalElementCount / FullCircleDegrees);
            return (index + totalElementCount) % totalElementCount;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(_navigationGroup == null)
            {
                return;
            }

            var elements = _navigationGroup.GetComponentsInChildren<InteractiveUIElementBase>();
            if(elements.IsEmpty())
            {
                return;
            }

            DrawRadialGizmos(elements);
        }

        private void DrawRadialGizmos(InteractiveUIElementBase[] elements)
        {
            Vector3 center = _navigationGroup.transform.position;
            float angleIncrement = FullCircleDegrees / elements.Length;

            const float RayLength = 150f;

            for(int i = 0; i < elements.Length; i++)
            {
                Gizmos.color = i == 0 ? Color.red : Color.green;

                float currentAngle = angleIncrement * i + _angleOffset;
                Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;

                Gizmos.DrawRay(center, direction * RayLength);
            }
        }
#endif
    }
}
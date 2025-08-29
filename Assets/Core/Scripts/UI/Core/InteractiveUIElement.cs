using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nexora.UI
{
    public enum UIInteractionState
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Selected = 3,
        Deselected = 4,
        Clicked = 5,
        Disabled = 6
    }

    public enum UISelectionState
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Selected = 3,
    }

    /// <summary>
    /// Abstract class to define UI elements that are interactive (interactable, selectable).
    /// Provides navigation options and the local registry elements for the element.
    /// </summary>
    /// <remarks>
    /// Every interactive UI element can be selected by default, so it implements
    /// <see cref="ISelectHandler"/> as well.
    /// </remarks>
    public abstract class InteractiveUIElementBase : 
        UIBehaviour,
        IUIBehaviour,
        ISelectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public abstract bool IsInteractable { get; protected set; }

        /// <summary>
        /// Defines if this UI object is selectable.
        /// </summary>
        /// <remarks>
        /// <b>Unselectable objects cannot be navigated, they will be just ignored.</b>
        /// </remarks>
        public abstract bool IsSelectable { get; protected set; }

        /// <summary>
        /// All interactive UI elements are registered to the global registry by default,
        /// and removed when they are destroyed.
        /// </summary>
        public abstract InteractiveUIElementRegistry Registry { get; }

        /// <summary>
        /// When navigating through keyboard/controller, how should this element behave.
        /// </summary>
        public abstract NavigationSettings.NavigationMode NavigationMode { get; }

        /// <summary>
        /// Navigation group of this element.
        /// </summary>
        public abstract NavigationGroupBase NavigationGroup { get; }

        /// <summary>
        /// Selects the this element in the <see cref="EventSystem"/>. In turn it will
        /// make a call to <see cref="ISelectHandler"/>.
        /// </summary>
        public abstract void Select();

        /// <summary>
        /// Call of <see cref="ISelectHandler"/>.
        /// </summary>
        public abstract void OnSelect(BaseEventData eventData);

        /// <summary>
        /// Deselect this element to update its look.
        /// </summary>
        public abstract void Deselect();
        
        public abstract void OnPointerEnter(PointerEventData eventData);

        public abstract void OnPointerExit(PointerEventData eventData);
    }

    /// <summary>
    /// Class to define an interactable UI element, might have different kinds of 
    /// <see cref="IUIVisualFeedback"/> to respond to the triggers. Responds to 
    /// the changes done in the UI hierarchy.
    /// </summary>
    /// <remarks>
    /// This class is humongous, but it is the only way to bring that much of a functionality
    /// in a one class. This class can be used as a standard component for any UI object.
    /// Using feedback hooks you can extend any functionality on this object by other 
    /// components. No need happened to be seen yet, if there would be any, then this
    /// class can be have some fields virtual for extension.
    /// <br></br><br></br>
    /// Responsiblities is supported/handled by classes of <see cref="SelectableState"/>, <see cref="Nexora.UI.InteractiveUIElementRegistry"/>,
    /// <see cref="CanvasGroupInteractibility"/>, <see cref="FeedbackManager"/>, <see cref="UIInteractiveElementFinder"/>.
    /// Check them to understand this class easier.
    /// </remarks>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu(ComponentMenuPaths.BaseUI + nameof(InteractiveUIElement))]
    public class InteractiveUIElement :
        InteractiveUIElementBase,
        IMoveHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [Tooltip("Is this element interactable in the UI, e.g for highlight, click etc.")]
        [SerializeField]
        private bool _isInteractable;

        [Tooltip("Is this element of the UI is selectable.")]
        [SerializeField]
        private bool _isSelectable;

        [Tooltip("Settings in the case navigation of the UI.")]
        [SerializeField]
        private NavigationSettings _navigationSettings;

        [SerializeField]
        private FeedbackManager _feedbackManager;

        /// <summary>
        /// The reason this class should have its non-static instance is to make removals 
        /// easy and efficient. Instance side of <see cref="InteractiveUIElementRegistry"/>
        /// provides a way to keep track of the registered index in the registry and also registration status
        /// for <see langword="this"/> object.
        /// </summary>
        [SerializeField, HideInInspector]
        private InteractiveUIElementRegistry _registry = new();

        private SelectableState _state = new();
        private CanvasGroupInteractibility _canvasGroupHandler = new();

        private NavigationHandler _navigationHandler;
        
        private NavigationGroupBase _navigationGroup;

        public event UnityAction<InteractiveUIElementBase> OnClicked;

        public UISelectionState CurrentSelectionState => _state.GetCurrentSelectionState(this, IsSelectable, _navigationGroup);
        public override NavigationSettings.NavigationMode NavigationMode => _navigationSettings.Mode;
        public override InteractiveUIElementRegistry Registry => _registry;
        public override NavigationGroupBase NavigationGroup => _navigationGroup;

        public override bool IsInteractable
        {
            get => _isInteractable && _canvasGroupHandler.AllowsInteraction;
            protected set
            {
                if(_isInteractable == value)
                {
                    return;
                }

                _isInteractable = value;
                HandleInteractabilityChanged();
            }
        }

        private void HandleInteractabilityChanged()
        {
            // If this element was selected, but interactability is false now
            // then unselect this element
            if(IsInteractable == false && _state.IsSelected)
            {
                _navigationGroup?.SelectElement(null);
            }

            TransitionToCurrentState();
        }

        /// <summary>
        /// Transitions the element's <see cref="UIInteractionState"/> to
        /// the current active state. And trigger all feedbacks registered
        /// to this object.
        /// </summary>
        private void TransitionToCurrentState(bool instant = false)
        {
            UIInteractionState interactionState = DetermineInteractionState();
            _feedbackManager.TriggerFeedback(interactionState, instant);
        }

        /// <summary>
        /// Using current state of canvases, self active/interactable state,
        /// and also the current selection state(mouse pointer inside, clicking etc.),
        /// determines what is the <see cref="UIInteractionState"/>.
        /// </summary>
        /// <returns></returns>
        private UIInteractionState DetermineInteractionState()
        {
            if(_isInteractable == false || _canvasGroupHandler.AllowsInteraction == false || isActiveAndEnabled == false)
            {
                return UIInteractionState.Disabled;
            }

            return CurrentSelectionState switch
            {
                UISelectionState.Normal => UIInteractionState.Normal,
                UISelectionState.Highlighted => UIInteractionState.Highlighted,
                UISelectionState.Pressed => UIInteractionState.Pressed,
                UISelectionState.Selected => UIInteractionState.Selected,
                _ => UIInteractionState.Disabled
            };
        }

        public override bool IsSelectable
        {
            get => _isSelectable && _navigationGroup != null;
            protected set
            {
                if(_isSelectable == value)
                {
                    return;
                }

                _isSelectable = value;
                HandleSelectabilityChanged();
            }
        }

        private void HandleSelectabilityChanged()
        {
            _state?.SetSelected(false);

            if(_navigationGroup == null)
            {
                return;
            }

            if(_isSelectable)
            {
                _navigationGroup.RegisterElement(this);
            }
            else
            {
                _navigationGroup.UnregisterElement(this);
            }

            TransitionToCurrentState();
        }

        protected override void Awake()
        {
            if(Application.isPlaying == false)
            {
                return;
            }

            InitializeComponents();
            _feedbackManager.InitializeFeedbacks(this);
            FindParentGroup();
        }

        private void InitializeComponents()
        {
            _navigationHandler = new NavigationHandler(this, _navigationSettings);
            _registry = new InteractiveUIElementRegistry();
        }

        private void FindParentGroup()
        {
            // ! Order of if statements should not change !, so that we will assign '_navigationGroup'
            // in any case of this object is selectable or not.
            if (transform.TryGetComponentInParent(out _navigationGroup))
            {
                if (_isSelectable)
                {
                    _navigationGroup.RegisterElement(this);
                }
            }
        }

        protected override void OnEnable()
        {
            if(_registry.IsRegistered)
            {
                return;
            }

            _registry.RegisterUIElement(this);
            _state.Reset();

            HandleSelectionIfNeeded();
            TransitionToCurrentState(instant: true);
        }

        private void HandleSelectionIfNeeded()
        {
            if(IsSelectable && _navigationGroup.SelectedElement == (InteractiveUIElementBase)this)
            {
                OnSelect(null);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (CanHandleSelection() == false)
            {
                return;
            }

            if (IsSelectable)
            {
                _state.SetSelected(true);
                _navigationGroup.SelectElement(this);
                TransitionToCurrentState();
            }
        }

        private bool CanHandleSelection()
        {
            return _canvasGroupHandler.AllowsInteraction && IsInteractable && _state.IsSelected == false;
        }

        protected override void OnDisable()
        {
            if(Application.isPlaying == false || _registry.IsRegistered == false)
            {
                return;
            }

            _registry.UnregisterUIElement(this);
            Deselect();
            _state.ClearAllStates();
            TransitionToCurrentState(instant: true);
            base.OnDisable();
        }

        public override void Deselect()
        {
            _state.SetSelected(false);
            _feedbackManager.TriggerFeedback(UIInteractionState.Deselected);
            TransitionToCurrentState();
        }

        protected override void OnDestroy()
        {
            if(Application.isPlaying == false)
            {
                return;
            }

            if(IsSelectable)
            {
                _navigationGroup.UnregisterElement(this);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _state.SetPointerDown(true);
            TransitionToCurrentState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _state.SetPointerDown(false);

            if(_state.IsPointerInside && IsInteractable)
            {
                HandleClick();
            }

            TransitionToCurrentState();
        }

        private void HandleClick()
        {
            Select();
            _feedbackManager.TriggerFeedback(UIInteractionState.Clicked, instant: true);
            OnClicked?.Invoke(this);
        }

        public override void Select()
        {
            if(IsInteractable == false || EventSystem.current == null || EventSystem.current.alreadySelecting)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _state.SetPointerInside(true);

            if(_state.IsSelected == false)
            {
                TransitionToCurrentState();
            }

            if(IsSelectable)
            {
                _navigationGroup.HighlightElement(this);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _state.SetPointerInside(false);

            if(_state.IsSelected == false)
            {
                TransitionToCurrentState();
            }

            if(IsSelectable)
            {
                _navigationGroup.HighlightElement(null);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            InteractiveUIElementBase nextSelectable = _navigationHandler.GetElementInDirection(eventData.moveDir);

            if (nextSelectable != null && nextSelectable.IsActive())
            {
                eventData.selectedObject = nextSelectable.gameObject;
            }
        }

        protected override void OnCanvasGroupChanged()
        {
            if(_canvasGroupHandler.UpdateInteractionState(transform))
            {
                TransitionToCurrentState(instant: true);
            }
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            OnCanvasGroupChanged();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            TransitionToCurrentState(instant: true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus == false && IsPressed())
            {
                _state.ClearAllStates();
                TransitionToCurrentState(instant: true);
            }
        }

        private bool IsPressed()
        {
            if(isActiveAndEnabled == false || _canvasGroupHandler.AllowsInteraction == false)
            {
                return false;
            }

            return _state.IsPointerDown;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            _feedbackManager.ValidateFeedbacks(this);

            // There is a possibility <b>OnValidate</b> might be called before
            // <b>OnEnable</b>, we just validate if it is already enabled to 
            // avoid crashes due to uniinitialized state
            if(isActiveAndEnabled)
            {
                TransitionToCurrentState(instant: true);
            }
        }

        protected override void Reset()
        {
            base.Reset();
            _navigationSettings.Mode = NavigationSettings.NavigationMode.Automatic;
        }
#endif
    }

    /// <summary>
    /// Handles the selectable state of the object, it keeps track of the state of the pointer with
    /// relation to the object and returns the current <see cref="UISelectionState"/> using the state
    /// of the pointer.
    /// </summary>
    public sealed class SelectableState
    {
        public bool IsSelected { get; private set; }
        public bool IsPointerInside { get; private set; }
        public bool IsPointerDown { get; private set; }

        public void SetSelected(bool selected) => IsSelected = selected;
        public void SetPointerDown(bool pointerDown) => IsPointerDown = pointerDown;
        public void SetPointerInside(bool pointerInside) => IsPointerInside = pointerInside;

        public UISelectionState GetCurrentSelectionState(
            InteractiveUIElementBase owner, 
            bool isSelectable, 
            NavigationGroupBase parentGroup)
        {
            if(IsPointerDown)
            {
                return UISelectionState.Pressed;
            }
            
            if(isSelectable && parentGroup.SelectedElement == owner)
            {
                return UISelectionState.Selected;
            }

            return IsPointerInside
                ? UISelectionState.Highlighted
                : UISelectionState.Normal;
        }

        public void Reset()
        {
            IsPointerDown = false;
        }

        public void ClearAllStates()
        {
            IsSelected = false;
            IsPointerDown = false;
            IsPointerInside = false;
        }
    }

    /// <summary>
    /// Handles navigation in all 4 directions for the owner <see cref="InteractiveUIElementBase"/>.
    /// </summary>
    public sealed class NavigationHandler
    {
        private readonly InteractiveUIElementBase _owner;
        private readonly NavigationSettings _navigationSettings;

        public NavigationHandler(InteractiveUIElementBase owner, NavigationSettings settings)
        {
            _owner = owner;
            _navigationSettings = settings;
        }

        public InteractiveUIElementBase GetElementInDirection(MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Left => GetElementInternal(Vector3.left, NavigationSettings.NavigationMode.Horizontal, _navigationSettings.NavigateOnLeft),
                MoveDirection.Right => GetElementInternal(Vector3.right, NavigationSettings.NavigationMode.Horizontal, _navigationSettings.NavigateOnRight),
                MoveDirection.Up => GetElementInternal(Vector3.up, NavigationSettings.NavigationMode.Vertical, _navigationSettings.NavigateOnUp),
                MoveDirection.Down => GetElementInternal(Vector3.down, NavigationSettings.NavigationMode.Vertical, _navigationSettings.NavigateOnDown),
                _ => null
            };
        }

        private InteractiveUIElementBase GetElementInternal(
            Vector3 direction, 
            NavigationSettings.NavigationMode navigationMode, 
            InteractiveUIElementBase explicitSelection)
        {
            if(_navigationSettings.Mode == NavigationSettings.NavigationMode.Explicit)
            {
                return explicitSelection;
            }

            if((_navigationSettings.Mode & navigationMode) != 0)
            {
                return UIInteractiveElementFinder.FindElement(
                    _owner, 
                    _owner.transform.rotation * direction, 
                    _navigationSettings);
            }

            return null;
        }
    }

    /// <summary>
    /// Handles Initializing, Triggering, Validating the list of <see cref="IUIVisualFeedback"/>
    /// that is attached to the <see cref="InteractiveUIElementBase"/>.
    /// </summary>
    [Serializable]
    public sealed class FeedbackManager
    {
        [SpaceArea]
        [Title("UI Feedback Callbacks")]
        [SerializeReference]
        [ReorderableList(elementLabel: "Feedback")]
        [ReferencePicker(typeof(IUIVisualFeedback), TypeGrouping.ByFlatName)]
        private IUIVisualFeedback[] _feedbacks = Array.Empty<IUIVisualFeedback>();

        public void InitializeFeedbacks(InteractiveUIElementBase owner)
        {
            foreach(var feedback in _feedbacks)
            {
                feedback?.Initialize(owner);
            }
        }

        public void TriggerFeedback(UIInteractionState interactionState, bool instant = false)
        {
            if(UnityUtils.IsQuitting)
            {
                return;
            }

            foreach(var feedback in _feedbacks)
            {
                feedback?.ExecuteFeedback(interactionState, instant);
            }
        }

#if UNITY_EDITOR
        public void ValidateFeedbacks(InteractiveUIElementBase owner)
        {
            foreach( var feedback in _feedbacks)
            {
                feedback?.EditorValidate(owner);
            }
        }
#endif
    }

    /// <summary>
    /// Handles interactability state derived from all <see cref="CanvasGroup"/> in the hierarchy
    /// of the element. 
    /// </summary>
    /// <remarks>
    /// Used for overriding <see cref="InteractiveUIElementBase"/> interactability if <see cref="CanvasGroup"/>
    /// is not interactable.
    /// </remarks>
    public sealed class CanvasGroupInteractibility
    {
        private readonly List<CanvasGroup> _canvasGroupCache = new();

        public bool AllowsInteraction { get; private set; } = true;

        /// <summary>
        /// Goes through the hierarchy of the <see cref="CanvasGroup"/> objects upwards,
        /// and updates if this transform should be interactable by <see cref="CanvasGroup"/>.
        /// </summary>
        /// <param name="transform">Object to start check from.</param>
        /// <returns>If canvas groups parents' interactability has changed.</returns>
        public bool UpdateInteractionState(Transform transform)
        {
            bool previousState = AllowsInteraction;
            AllowsInteraction = CalculateInteractionState(transform);
            return AllowsInteraction != previousState;
        }

        /// <summary>
        /// Goes up in the hierarchy starting from the current transform,
        /// and looks for <see cref="CanvasGroup"/> objects. If any of
        /// them is not interactable, then breaks and sets itself as not interactable.
        /// </summary>
        /// <remarks>
        /// If in the hierarchy have a <see cref="CanvasGroup"/> that ignores parents,
        /// then we return the value we already gathered. <b>But if any siblings still not
        /// interactable then returns not interactable.</b>
        /// </remarks>
        private bool CalculateInteractionState(Transform transform)
        {
            bool groupAllowInteraction = true;
            Transform current = transform;

            while(current != null)
            {
                current.GetComponents(_canvasGroupCache);
                bool shouldBreak = false;

                foreach(CanvasGroup group in _canvasGroupCache)
                {
                    if(group.enabled && group.interactable == false)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }

                    if(group.ignoreParentGroups)
                    {
                        shouldBreak = true;
                    }
                }

                if(shouldBreak)
                {
                    break;
                }

                current = current.parent;
            }

            return groupAllowInteraction;
        }
    }

    /// <summary>
    /// Acts a registry to create a navigation between all UI object in the scene that are selectable.
    /// Provides local variables for keeping the self state of the selectable which derives from 
    /// <see cref="InteractiveUIElementBase"/>. The static part is to keep track of all active selectables
    /// and use them navigate between them.
    /// </summary>
    /// <remarks>
    /// Check <see cref="NavigationHandler"/> to see how registry is used for navigation.
    /// </remarks>
    [Serializable]
    public sealed class InteractiveUIElementRegistry
    {
        private static InteractiveUIElementBase[] _allElements = new InteractiveUIElementBase[16];
        private static int _registeredElementCount;

        /// <summary>
        /// Local index of the element, keeps track of what index this element is taking
        /// in the static selectable array.
        /// </summary>
        /// <remarks>
        /// May change when swaps occur.
        /// </remarks>
        private int _currentIndex = -1;

        public static InteractiveUIElementBase[] AllUIElements => _allElements;

        /// <summary>
        /// Keeping track the number of enabled selectable <see cref="InteractiveUIElementBase"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="AllUIElements"/> may contain <see langword="null"/> elements.
        /// </remarks>
        public static int RegisteredElementCount => _registeredElementCount;

        /// <summary>
        /// <see langword="this"/> object is selectable and enabled?
        /// </summary>
        public bool IsRegistered { get; private set; }

        /// <summary>
        /// Adds the <paramref name="selectable"/> to the first available index in the list.
        /// </summary>
        /// <remarks>
        /// Resizes the array if list is full.
        /// </remarks>
        public void RegisterUIElement(InteractiveUIElementBase selectable)
        {
            if(IsRegistered)
            {
                return;
            }

            EnsureCapacity();

            _currentIndex = _registeredElementCount;
            _allElements[_currentIndex] = selectable;
            _registeredElementCount++;

            IsRegistered = true;
        }

        /// <summary>
        /// Doubles the list in size when there is no more space in the array.
        /// </summary>
        private static void EnsureCapacity()
        {
            if(_registeredElementCount == _allElements.Length)
            {
                var temp = new InteractiveUIElementBase[_registeredElementCount * 2];
                Array.Copy(_allElements, temp, _allElements.Length);
                _allElements = temp;
            }
        }

        /// <summary>
        /// Swaps the last element in the list to the index of <see langword="this"/> object.
        /// Essentially, just a swap.
        /// </summary>
        /// <param name="element"></param>
        public void UnregisterUIElement(InteractiveUIElementBase element)
        {
            if(IsRegistered == false)
            {
                return;
            }

            // Move the last element in the static list to the index of this object
            _registeredElementCount--;
            _allElements[_registeredElementCount].Registry._currentIndex = _currentIndex;
            _allElements[_currentIndex] = _allElements[_registeredElementCount];

            // Set the last element null, shrinking the list
            _allElements[_registeredElementCount] = null;

            IsRegistered = false;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            Array.Clear(_allElements, 0, _allElements.Length);
            _registeredElementCount = 0;
        }
#endif
    }

    /// <summary>
    /// Static class to find any <see cref="InteractiveUIElementBase"/> depending on the 
    /// owner of the caller, direction of the navigation and the <see cref="NavigationSettings"/>
    /// of the owner.
    /// </summary>
    public static class UIInteractiveElementFinder
    {
        // Might use for Editor as well?

        /// <summary>
        /// Gets the closest element in the <paramref name="direction"/> from the <paramref name="owner"/> object
        /// using <paramref name="navigationSettings"/>.
        /// </summary>
        /// <remarks>
        /// If there are no elements in the forward, but settings enable wrap around. Then gets the farthest element
        /// in the reverse direction.
        /// </remarks>
        /// <param name="owner">Caller, origin will be this object and from this object we will search in the <paramref name="direction"/>.</param>
        /// <param name="direction">Direction of the search.</param>
        public static InteractiveUIElementBase FindElement(InteractiveUIElementBase owner, Vector3 direction, NavigationSettings navigationSettings)
        {
            direction.Normalize();

            // Get the world position where the ray in the direction intersects on one of the 
            // edges of the rect of the owner's transform
            Vector3 localDirection = Quaternion.Inverse(owner.transform.rotation) * direction;
            Vector3 startPosition = 
                owner.transform.TransformPoint(GetPointOnRectEdge(owner.transform as RectTransform, localDirection));
            // -- 

            Candidate[] candidates = FindCandidates(owner, direction, startPosition);
            return SelectBestCandidate(candidates, navigationSettings.WrapAround);
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rectTransform, Vector2 direction)
        {
            if(rectTransform == null)
            {
                return Vector2.zero;
            }

            // Have the larger component to be 1 or -1 so that 
            // rect.size * 0.5f always always at one of the edges
            if (direction != Vector2.zero)
            {
                direction /= Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            }

            Rect rect = rectTransform.rect;
            direction = rect.center + Vector2.Scale(rect.size, direction * 0.5f);
            return direction;
        }

        private static Candidate[] FindCandidates(
            InteractiveUIElementBase owner, 
            Vector3 direction, 
            Vector3 startPosition)
        {
            var candidates = new List<Candidate>();
            InteractiveUIElementBase[] allElements = InteractiveUIElementRegistry.AllUIElements;
            int registeredElementCount = InteractiveUIElementRegistry.RegisteredElementCount;

            for(int i = 0; i < registeredElementCount; i++)
            {
                InteractiveUIElementBase candidate = allElements[i];
                if(IsValidCandidate(candidate, owner) == false)
                {
                    continue;
                }

                Vector3 candidatePosition = GetCandidatePosition(candidate);
                Vector3 directionToCandidate = candidatePosition - startPosition;
                float dotProduct = Vector3.Dot(direction, directionToCandidate);

                candidates.Add(new Candidate
                {
                    Element = candidate,
                    DirectionToCandidate = directionToCandidate,
                    DotProduct = dotProduct,
                    Score = dotProduct <= 0 ? 0 : (dotProduct / directionToCandidate.sqrMagnitude) // Divide for the closest element
                });
            }

            return candidates.ToArray();
        }

        private static bool IsValidCandidate(InteractiveUIElementBase candidate, InteractiveUIElementBase owner)
        {
            if (candidate == owner || 
                candidate.IsInteractable == false ||
                candidate.NavigationMode == NavigationSettings.NavigationMode.None)
            {
                return false;
            }

            // Groups are different
            if(candidate.NavigationGroup != owner.NavigationGroup)
            {
                // But they belong to the same group of selectables, but parents assigned as different objects
                if(candidate.IsSelectable && owner.IsSelectable
                && Equals(candidate.NavigationGroup.RegisteredElements, owner.NavigationGroup.RegisteredElements))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <returns><paramref name="candidate"/> position in the <b>world</b> coordinates.</returns>
        private static Vector3 GetCandidatePosition(InteractiveUIElementBase candidate)
        {
            var rectTransform = candidate.transform as RectTransform;
            Vector3 center = rectTransform?.rect.center ?? Vector3.zero;
            return candidate.transform.TransformPoint(center);
        }

        private static InteractiveUIElementBase SelectBestCandidate(Candidate[] candidates, bool wrapAround)
        {
            (InteractiveUIElementBase element, float score) bestPick = (null, float.NegativeInfinity);
            (InteractiveUIElementBase element, float score) bestWrapPick = (null, float.NegativeInfinity);

            foreach (Candidate candidate in candidates)
            {
                if (candidate.DotProduct > 0 && candidate.Score > bestPick.score)
                {
                    bestPick = (candidate.Element, candidate.Score);
                }
                else if (wrapAround && candidate.DotProduct < 0)
                {
                    // Multiply for the farthest element in the reverse direction
                    float wrapScore = -candidate.DotProduct * candidate.DirectionToCandidate.sqrMagnitude;
                    if(wrapScore > bestWrapPick.score)
                    {
                        bestWrapPick = (candidate.Element, wrapScore);
                    }
                }
            }

            return bestPick.element ?? (wrapAround ? bestWrapPick.element : null);
        }

        private struct Candidate
        {
            public InteractiveUIElementBase Element;
            public Vector3 DirectionToCandidate;
            public float DotProduct;
            public float Score;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

namespace Nexora.InputSystem
{
    /// <summary>
    /// <see cref="GameModule{T}"/> that manages Add/Remove of <see cref="InputProfile"/>,
    /// Add/Remove of cancel actions, Register/Unregister and updating states of
    /// <see cref="IInputHandler"/>. 
    /// <br></br> Uses <see cref="InputHandlerList"/>, <see cref="InputProfileStack"/>,
    /// and <see cref="CancelInputHandler"/> to handle responsibilities.
    /// </summary>
    /// <remarks>
    /// Keeps track of the current active <see cref="InputProfile"/> and fires an event
    /// when it changes via <see cref="OnInputProfileChanged"/>.
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [CreateAssetMenu(menuName = CreateMenuPath + "Input Module", fileName = nameof(InputModule))]
    public sealed partial class InputModule : GameModule<InputModule>
    {
        [SerializeField]
        private InputProfileStack _inputProfileStack;
        [SerializeField]
        private CancelInputHandler _cancelInputHandler;
        
        private InputHandlerList _inputHandlerList;

        public event UnityAction<InputProfile> OnInputProfileChanged;

        public InputProfile ActiveInputProfile => _inputProfileStack.ActiveInputProfile;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
#if UNITY_EDITOR
            _cancelInputHandler.Reset();
            _inputProfileStack.Reset();
#endif
            _cancelInputHandler.Initialize();
            _inputProfileStack.Initialize();
            _inputProfileStack.OnActiveProfileChanged += OnInputProfileChanged;
            _inputHandlerList = new InputHandlerList(_inputProfileStack);
        }

        /// <inheritdoc cref="InputHandlerList.RegisterInputHandler(IInputHandler)"/>
        public void RegisterInputHandler(IInputHandler inputHandler)
            => _inputHandlerList.RegisterInputHandler(inputHandler);

        /// <inheritdoc cref="InputHandlerList.UnregisterInputHandler(IInputHandler)"/>
        public void UnregisterInputHandler(IInputHandler inputHandler)
            => _inputHandlerList.UnregisterInputHandler(inputHandler);

        /// <inheritdoc cref="InputProfileStack.AddProfile(InputProfile)"/>
        public void PushInputProfile(InputProfile inputProfile)
            => _inputProfileStack.AddProfile(inputProfile);

        /// <inheritdoc cref="InputProfileStack.RemoveProfile(InputProfile)"/>
        public void RemoveInputProfile(InputProfile inputProfile)
            => _inputProfileStack.RemoveProfile(inputProfile);

        public bool HasInputProfile(InputProfile inputProfile)
            => _inputProfileStack.Contains(inputProfile);

        /// <inheritdoc cref="CancelInputHandler.AddCancelAction(UnityAction)"/>
        public void AddCancelAction(UnityAction action)
            => _cancelInputHandler.AddCancelAction(action);

        /// <inheritdoc cref="CancelInputHandler.RemoveCancelAction(UnityAction)"/>
        public void RemoveCancelAction(UnityAction action)
            => _cancelInputHandler.RemoveCancelAction(action);

        /// <summary>
        /// Manages registering/unregistering <see cref="IInputHandler"/> and
        /// updating all of handlers' <see cref="IInputHandler.IsEnabled"/> state
        /// whenever active profile is changed.
        /// </summary>
        private sealed class InputHandlerList
        {
            private const int DefaultInputHandlerCount = 32;

            private List<IInputHandler> _inputHandlers = new(DefaultInputHandlerCount);
            private InputProfileStack _inputProfileStack;

            public InputHandlerList(InputProfileStack inputProfileStack)
            {
                _inputProfileStack = inputProfileStack;
                inputProfileStack.OnActiveProfileChanged += ProfileChangeHandler;
            }

            /// <summary>
            /// Called whenever active <see cref="InputProfile"/> changes, updates
            /// <see cref="IInputHandler.IsEnabled"/> of all registered input handlers.
            /// </summary>
            /// <param name="newProfile"></param>
            private void ProfileChangeHandler(InputProfile newProfile)
            {
                foreach(IInputHandler inputHandler in _inputHandlers)
                {
                    UpdateHandlerState(inputHandler, newProfile.AllowedInputs);
                }
            }

            /// <summary>
            /// Registers <paramref name="inputHandler"/> and sets its <see cref="IInputHandler.IsEnabled"/>.
            /// </summary>
            public void RegisterInputHandler(IInputHandler inputHandler)
            {
                if(UnityUtils.IsQuitting)
                {
                    return;
                }

                _inputHandlers.Add(inputHandler);
                UpdateHandlerState(inputHandler, _inputProfileStack.ActiveInputProfile.AllowedInputs);
            }

            public void UnregisterInputHandler(IInputHandler inputHandler)
            {
                if(UnityUtils.IsQuitting)
                {
                    return;
                }

                _inputHandlers.Remove(inputHandler);
            }

            /// <summary>
            /// Updates the <see cref="IInputHandler.IsEnabled"/> state using the current Active <see cref="InputProfile"/>
            /// allowed inputs and <see cref="IInputHandler.EnableMode"/>. 
            /// </summary>
            /// <param name="inputHandler"></param>
            /// <param name="allowedHandlerTypes">Inputs current Active <see cref="InputProfile"/> allows.</param>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if passed enum value is invalid.</exception>
            public static void UpdateHandlerState(IInputHandler inputHandler, SerializedType[] allowedHandlerTypes)
            {
                inputHandler.IsEnabled = inputHandler.EnableMode switch
                {
                    InputEnableMode.Contextual => Array.Exists(allowedHandlerTypes, type => type.Type == inputHandler.GetType()),
                    InputEnableMode.AlwaysEnabled => true,
                    InputEnableMode.AlwaysDisabled => false,
                    InputEnableMode.ScriptControlled => inputHandler.IsEnabled,
                    _ => throw new ArgumentOutOfRangeException("Provided enable mode is not valid, make sure it is in bounds.")
                };
            }

            public void Reset()
            {
                _inputHandlers.Clear();
            }
        }

        /// <summary>
        /// Class to manage keep list of <see cref="InputProfile"/>, adding/removing. Has a default input
        /// profile in case of no special profiles are added. 
        /// </summary>
        /// <remarks>
        /// <b>Always has a valid active input profile.</b>
        /// <br></br><b>Works like a stack in terms of active profile. That is, last added profile is always
        /// the active profile.</b>
        /// <br></br>Fires an event in the case of active input profile is changed.
        /// </remarks>
        [Serializable]
        private sealed class InputProfileStack
        {
            [Tooltip("Default input profile, that is used when no other profile is present.")]
            [SerializeField, NotNull]
            private InputProfile _defaultInputProfile;

            private readonly List<InputProfile> _inputProfiles = new();

            public InputProfile ActiveInputProfile { get; private set; }

            public int InputProfileCount => _inputProfiles.Count;

            public event UnityAction<InputProfile> OnActiveProfileChanged;

            public void Initialize() => AddProfile(_defaultInputProfile);

            public bool Contains(InputProfile inputProfile)
                => _inputProfiles.IndexOf(inputProfile) != CollectionConstants.NotFound;

            /// <summary>
            /// Adds a new profile to the list, if the current active profile
            /// is not same, then changes the active profile.
            /// </summary>
            public void AddProfile(InputProfile inputProfile)
            {
                if(inputProfile == null)
                {
                    return;
                }

                _inputProfiles.Add(inputProfile);

                if(ActiveInputProfile != inputProfile)
                {
                    SetActiveProfile(inputProfile);
                }
            }

            /// <summary>
            /// Removes the input profile from the list, if the profile to be
            /// removed is the active profile, then changes the active profile
            /// to be the previous element in the list.
            /// </summary>
            /// <remarks>
            /// Input profiles list can be never empty, so adds the default
            /// input profile if list becomes empty.
            /// </remarks>
            public void RemoveProfile(InputProfile inputProfile)
            {
                if(_inputProfiles.Remove(inputProfile) && ActiveInputProfile == inputProfile)
                {
                    if(_inputProfiles.IsEmpty())
                    {
                        AddProfile(_defaultInputProfile);
                    }
                    else
                    {
                        SetActiveProfile(_inputProfiles[^1]);
                    }
                }
            }

            /// <summary>
            /// Changes the active profile and fires an event informing about the change.
            /// </summary>
            private void SetActiveProfile(InputProfile inputProfile)
            {
                ActiveInputProfile = inputProfile;
                OnActiveProfileChanged?.Invoke(inputProfile);
            }

            public void Reset()
            {
                ActiveInputProfile = null;
                _inputProfiles.Clear();
                OnActiveProfileChanged = null;
            }
        }

        /// <summary>
        /// Class to handle cancel inputs, like stack of <see cref="KeyCode.Escape"/> or 
        /// in mobile phones back button. Keeps a input for the cancel action key, 
        /// manages add/remove of actions that are fired in case of cancel input key press.
        /// <br></br> For mobile environment, back input can be mapped to exact cancel input,
        /// so that these key press will invoke cancel action as well.
        /// </summary>
        /// <remarks>
        /// <b>Same cancel action can exist only once in the list, that is no duplicates allowed.
        /// In case of duplicate, existing action just moved to the end of the list.</b>
        /// <br></br> When cancel input is pressed, last action added is popped and invoked.
        /// </remarks>
        [Serializable]
        private sealed class CancelInputHandler
        {
            [Tooltip("Cancel input key, when this input is performed then one cancel action is invoked.")]
            [SerializeField]
            private InputActionReference _cancelInput;

            private readonly List<UnityAction> _cancelActions = new();

            private int _lastActionRemoveFrame;

            public bool HasCancelAction => _cancelActions.Count > 0 || HasRemovedActionThisFrame;
            private bool HasRemovedActionThisFrame => _lastActionRemoveFrame == Time.frameCount;
            public int CancelActionsCount => _cancelActions.Count;

            /// <summary>
            /// Enables the input and attaches hook for its key press.
            /// </summary>
            public void Initialize()
            {
                _cancelInput.action.Enable();
                _cancelInput.action.performed += PopAndInvokeCancelAction;
            }
            
            /// <summary>
            /// Adds a new action to the list, if the action already exists in the list
            /// then moves it to the end of the list.
            /// </summary>
            public void AddCancelAction(UnityAction action)
            {
                if(action == null)
                {
                    return;
                }

                RemoveCancelAction(action);

                _cancelActions.Add(action);
            }

            public void RemoveCancelAction(UnityAction action)
            {
                int index = _cancelActions.IndexOf(action);
                if (index != CollectionConstants.NotFound)
                {
                    _cancelActions.RemoveAt(index);
                }
            }

            /// <summary>
            /// Removes the last added action in the list and invokes it.
            /// </summary>
            private void PopAndInvokeCancelAction(InputAction.CallbackContext context)
            {
                if(_cancelActions.IsEmpty())
                {
                    return;
                }

                UnityAction lastAddedAction = _cancelActions[^1];
                _cancelActions.RemoveLast();
                _lastActionRemoveFrame = Time.frameCount;
                lastAddedAction?.Invoke();
            }

            public void Reset()
            {
                _lastActionRemoveFrame = -1;
                _cancelActions.Clear();
            }
        }
    }

    /// <summary>
    /// Provides extensions method for easier Adding/Removing listeners to "started", "performed", "canceled"
    /// events of <see cref="InputActionReference"/>. 
    /// </summary>
    /// <remarks>
    /// Keeps tracks of the active listener count to each <see cref="InputActionReference"/>,
    /// increments them for each listener and decrements when it no longers listens to the event.
    /// Make sure Adding/Removing pairs match each other so that there wouldn't be ghost input references
    /// lingering.
    /// <br></br>
    /// <b>If listener count is 0 then <see cref="InputActionReference"/> is disabled and removed from the dictionary.</b>
    /// <br></br>
    /// <b>When the input is gets its first listener, then it is enabled.</b>
    /// </remarks>
    public static class InputActionReferenceExtensions
    {
        /// <summary>
        /// Dictionary to keep track of how many callbacks are listening to the action,
        /// to enable/disable input action when no one listens to the events.
        /// </summary>
        private static readonly Dictionary<InputActionReference, int> _actionListenerCounts = new();

#if UNITY_EDITOR
        /// <summary>
        /// Clears the dictionary so that there wouldn't be leftover records between
        /// successive plays in the editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOnReload()
        {
            foreach(var actionReference in _actionListenerCounts.Keys)
            {
                actionReference.action?.Disable();
            }
            _actionListenerCounts.Clear();
        }
#endif

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "started" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "started" event list.</param>
        public static void AddStartedListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            IncrementUsageCount(actionReference);
            actionReference.action.started += callback;
        }

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "performed" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "performed" event list.</param>
        public static void AddPerformedListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            IncrementUsageCount(actionReference);
            actionReference.action.performed += callback;
        }

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "canceled" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "canceled" event list.</param>
        public static void AddCanceledListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            IncrementUsageCount(actionReference);
            actionReference.action.canceled += callback;
        }

        /// <summary>
        /// Increments the amount of listeners registered to <paramref name="actionReference"/>.
        /// If it is the first listener, then enables the action.
        /// </summary>
        public static void IncrementUsageCount(this InputActionReference actionReference)
        {
            EnsureValid(actionReference);

            if (_actionListenerCounts.TryGetValue(actionReference, out var count))
            {
                _actionListenerCounts[actionReference]++;
            }
            else
            {
                _actionListenerCounts.Add(actionReference, 1);
                actionReference.action.Enable();
            }
        }

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "started" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "started" event list.</param>
        public static void RemoveStartedListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            actionReference.action.started -= callback;
            DecrementUsageCount(actionReference);
        }

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "performed" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "performed" event list.</param>
        public static void RemovePerformedListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            actionReference.action.performed -= callback;
            DecrementUsageCount(actionReference);
        }

        /// <summary>
        /// Adds a listener <paramref name="callback"/> to the "canceled" event of the <paramref name="actionReference"/>. 
        /// </summary>
        /// <param name="callback">Callback to add to the "canceled" event list.</param>
        public static void RemoveCanceledListener(
            this InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            EnsureValid(actionReference);
            actionReference.action.canceled -= callback;
            DecrementUsageCount(actionReference);
        }

        /// <summary>
        /// Decrements the amount of listeners registered to <paramref name="actionReference"/>.
        /// If it was the last listener registered, then disables the action.
        /// </summary>
        public static void DecrementUsageCount(this InputActionReference actionReference)
        {
            EnsureValid(actionReference);

            if (_actionListenerCounts.TryGetValue(actionReference, out var count))
            {
                if(count == 1)
                {
                    _actionListenerCounts.Remove(actionReference);
                    actionReference.action.Disable();
                }
                else
                {
                    _actionListenerCounts[actionReference] = count - 1;
                }
            }
        }

        private static void EnsureValid(InputActionReference actionReference)
        {
            Assert.IsTrue(actionReference != null, "Passed input reference is null, " +
                "make sure passing not null references.");
        }
    }
}

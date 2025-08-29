using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.ProceduralMotion;
using System;
using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Delegate informing about the change of <see cref="HandheldEquipStateType"/> of a <see cref="IHandheld"/>.
    /// </summary>
    /// <param name="equipState">New equip state.</param>
    public delegate void HandheldEquipStateChangedDelegate(HandheldEquipStateType equipState);

    public interface IHandheld :
        IMonoBehaviour,
        IHandheldCore,
        IHasActionHandler, 
        IHandheldComponents
    {
        /// <summary>
        /// Equips this handheld.
        /// </summary>
        /// <param name="transitionSpeed">Speed of the animation.</param>
        IEnumerator Equip(float transitionSpeed);

        /// <summary>
        /// Holsters this handheld.
        /// </summary>
        /// <param name="transitionSpeed">Speed of the animation.</param>
        IEnumerator Holster(float transitionSpeed);
    }

    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public abstract class Handheld : 
        MonoBehaviour,
        IHandheld,
        IMovementSpeedAdjuster
    {
        [SerializeField, NewLabel("Action Handlers")]
        private ActionHandlers _actionHandlers = new();

        [SerializeField, NewLabel("Crosshair Settings")]
        private CrosshairBase _crosshairHandler;

        [SerializeField, NewLabel("Equip/Holster Setings")]
        private HandheldEquipStateMachine _equipStateMachine;

        private GeometryManager _geometryManager;

        public ICharacter Character { get; private set; }
        public IAnimatorController Animator { get; private set; }
        public ICharacterAudioPlayer AudioPlayer { get; private set; }
        public IHandheldMotionController MotionController { get; private set; }
        public IHandheldMotionTargets MotionTargets { get; private set; }

        public HandheldEquipStateType EquipState => _equipStateMachine.CurrentStateType;

        public bool IsGeometryVisible
        {
            get => _geometryManager.IsVisible;
            set => _geometryManager.IsVisible = value;
        }

        public CompositeValue SpeedModifier { get; } = new();

        public event HandheldEquipStateChangedDelegate EquipStateChanged;
        public event CrosshairChangedDelegate CrosshairChanged;

        protected virtual void Awake()
        {
            _equipStateMachine.Initialize();
            Animator = HandheldUtility.CreateAnimator(gameObject);
            MotionController = HandheldUtility.CreateMotionController(gameObject);
            MotionTargets = HandheldUtility.CreateMotionTargets(gameObject);

            gameObject.SetActive(false);

            _geometryManager = new GeometryManager(gameObject);

            _equipStateMachine.StateChanged += OnEquipStateChanged;
            _crosshairHandler.CrosshairChanged += OnCrosshairChanged;
        }

        private void OnDestroy()
        {
            _equipStateMachine.StateChanged -= OnEquipStateChanged;
            _crosshairHandler.CrosshairChanged -= OnCrosshairChanged;
        }

        public void OnEquipStateChanged(HandheldEquipStateType newState)
        {
            EquipStateChanged?.Invoke(newState);
            OnEquipStateChangedInternal(newState);
        }

        protected virtual void OnEquipStateChangedInternal(HandheldEquipStateType newState) { }

        public void OnCrosshairChanged(int newCrosshairID)
        {
            CrosshairChanged?.Invoke(newCrosshairID);
            OnCrosshairChangedInternal(newCrosshairID);
        }

        protected virtual void OnCrosshairChangedInternal(int newCrosshairID) { }

        public void SetCharacter(ICharacter character)
        {
            if(Character != null && Character != character)
            {
                Debug.LogError("The parent character of handheld is changed, something is wrong.");
                return;
            }

            Character = character;
            AudioPlayer ??= Character?.AudioPlayer;
            OnCharacterChanged(character);
        }

        protected virtual void OnCharacterChanged(ICharacter character) { }

        IEnumerator IHandheld.Equip(float transitionSpeed)
        {
            if(EquipState is HandheldEquipStateType.Equipping or HandheldEquipStateType.Equipped)
            {
                yield break;
            }

            yield return _equipStateMachine.TransitionTo(HandheldEquipStateType.Equipping, this);
            yield return _equipStateMachine.TransitionTo(HandheldEquipStateType.Equipped, this);
        }

        IEnumerator IHandheld.Holster(float transitionSpeed)
        {
            if(EquipState is HandheldEquipStateType.Holstering or HandheldEquipStateType.Hidden)
            {
                yield break;
            }

            yield return PreHolster(transitionSpeed);

            yield return _equipStateMachine.TransitionTo(HandheldEquipStateType.Holstering, this, transitionSpeed);
            yield return _equipStateMachine.TransitionTo(HandheldEquipStateType.Hidden, this, transitionSpeed);
        }

        protected virtual IEnumerator PreHolster(float transitionSpeed)
        {
            yield break;
        }

        #region Action Handlers
        public T GetActionOfType<T>() where T : IInputActionHandler => _actionHandlers.GetActionOfType<T>();
        public IInputActionHandler GetActionOfType(Type actionType) => _actionHandlers.GetActionOfType(actionType);
        public bool TryGetActionOfType<T>(out T actionHandler) where T : IInputActionHandler => _actionHandlers.TryGetActionOfType<T>(out actionHandler);
        public bool TryGetActionOfType(Type actionType, out IInputActionHandler actionHandler) => _actionHandlers.TryGetActionOfType(actionType, out actionHandler);
        #endregion
        [Serializable]
        private sealed class ActionHandlers
        {
            [ReorderableList]
            [ReferencePicker(typeof(IInputActionHandler))]
            [SerializeReference]
            private IInputActionHandler[] _inputActionHandlers;

            public bool TryGetActionOfType<T>(out T actionHandler) where T : IInputActionHandler
            {
                actionHandler = GetActionOfType<T>();
                return actionHandler != null;
            }

            public T GetActionOfType<T>() where T : IInputActionHandler
            {
                foreach (var inputActionHandler in _inputActionHandlers)
                {
                    if (inputActionHandler is T actionHandler)
                    {
                        return actionHandler;
                    }
                }

                return default;
            }

            public bool TryGetActionOfType(Type actionType, out IInputActionHandler actionHandler)
            {
                actionHandler = GetActionOfType(actionType);
                return actionHandler != null;
            }

            public IInputActionHandler GetActionOfType(Type actionType)
            {
                foreach (var inputActionHandler in _inputActionHandlers)
                {
                    if (inputActionHandler.GetType() == actionType)
                    {
                        return inputActionHandler;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Manages the state of geometry of the handheld.
        /// </summary>
        private sealed class GeometryManager
        {
            private bool _isVisible;
            private Renderer[] _renderers;

            private readonly GameObject _gameObject;

            public GeometryManager(GameObject gameObject) => _gameObject = gameObject;

            public bool IsVisible
            {
                get => _isVisible;
                set
                {
                    if(_isVisible == value)
                    {
                        return;
                    }

                    EnsureRenderersLoaded();
                    SetRenderersEnabled(value);
                    _isVisible = value;
                }
            }

            private void EnsureRenderersLoaded()
            {
                if(_renderers != null)
                {
                    return;
                }

                _renderers = _gameObject.GetComponentsInChildren<Renderer>();
                if(_renderers.IsEmpty())
                {
                    Debug.LogError("No available renderers for the handheld");
                }
            }

            private void SetRenderersEnabled(bool enabled)
            {
                foreach(var renderer in _renderers)
                {
                    renderer.enabled = enabled;
                }
            }
        }
    }

    public sealed class NullHandheld : IHandheld
    {
        public static readonly NullHandheld Instance = new();

        private NullHandheld() { }

        public ICharacter Character => null;
        public HandheldEquipStateType EquipState => HandheldEquipStateType.Hidden;
        public bool IsGeometryVisible { get => false; set { } }
        public IHandheldMotionController MotionController => null;
        public IHandheldMotionTargets MotionTargets => null;
        public IAnimatorController Animator => null;
        public ICharacterAudioPlayer AudioPlayer => null;
        public GameObject gameObject => null;
        public Transform transform => null;
        public bool enabled { get => true; set { } }

        public event HandheldEquipStateChangedDelegate EquipStateChanged
        {
            add { }
            remove { }
        }

        public void SetCharacter(ICharacter character) { }
        public Coroutine StartCoroutine(IEnumerator routine) => CoroutineUtils.StartGlobalCoroutine(routine);
        public void StopCoroutine(Coroutine routine) => CoroutineUtils.StopGlobalCoroutine(ref routine);
        public IEnumerator Equip(float transitionSpeed) { yield break; }
        public IEnumerator Holster(float transitionSpeed) { yield break; }
        public T GetActionOfType<T>() where T : IInputActionHandler => default;
        public IInputActionHandler GetActionOfType(Type actionType) => null;
    }
}
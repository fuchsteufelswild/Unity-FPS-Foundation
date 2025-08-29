using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public enum ControllerState
    {
        None = 0,
        Equipping = 1,
        Holstering = 2
    }

    /// <summary>
    /// Delegate for denoting a change in the equipment status of the <see cref="IHandheldEquipmentController"/>.
    /// </summary>
    /// <param name="handheld">Handheld that is related to the event.</param>
    public delegate void ControllerEquipmentDelegate(IHandheld handheld);

    /// <summary>
    /// Controls the equipment of handhelds on the character.
    /// </summary>
    public interface IHandheldEquipmentController
    {
        /// <summary>
        /// Currently equipped handheld that is in hands.
        /// </summary>
        IHandheld ActiveHandheld { get; }

        /// <summary>
        /// Current state; is equipping, holstering etc.
        /// </summary>
        ControllerState EquipmentState { get; }

        /// <summary>
        /// Called when equipment state is changed.
        /// </summary>
        event ControllerEquipmentStateChangedDelegate EquipmentStateChanged;

        /// <summary>
        /// Called when equipping any handheld beings.
        /// </summary>
        event ControllerEquipmentDelegate EquipBegin;

        /// <summary>
        /// Called when equipping any handheld ends.
        /// </summary>
        event ControllerEquipmentDelegate EquipEnd;

        /// <summary>
        /// Called when holstering any handheld begins.
        /// </summary>
        event ControllerEquipmentDelegate HolsterBegin;

        /// <summary>
        /// Called when holstering any handheld ends.
        /// </summary>
        event ControllerEquipmentDelegate HolsterEnd;

        /// <summary>
        /// Tries to equip <paramref name="handheld"/> with optional <paramref name="transitionSpeed"/> and 
        /// callback informing about equip.
        /// </summary>
        /// <param name="handheld">Handheld to equip.</param>
        /// <param name="transitionSpeed">Transition speed of animations (holstering old, equipping new).</param>
        /// <param name="onEquipBegin">Callback called when equipping begins (after holstering finishes).</param>
        /// <returns>If equipping was suceesful.</returns>
        bool TryEquip(IHandheld handheld, float transitionSpeed = 1f, UnityAction onEquipBegin = null);

        /// <summary>
        /// Tries to holster <paramref name="handheld"/> with optional <paramref name="transitionSpeed"/>.
        /// </summary>
        /// <param name="handheld">Handheld to holster.</param>
        /// <param name="transitionSpeed">Transition speed of holstering animation.</param>
        /// <returns>If holstering was suceesful.</returns>
        bool TryHolster(IHandheld handheld, float transitionSpeed = 1f);

        /// <summary>
        /// Holsters all handhelds available.
        /// </summary>
        void HolsterAll();

    }

    /// <summary>
    /// Manages equipment of handhelds.
    /// </summary>
    /// <remarks>
    /// Always has a <see cref="IHandheld"/> that is called 'default handheld' in the queue.
    /// This denotes when having nothing equipped in hands, it can represent 'nothing', 'plain hands' etc.
    /// Or even a default weapon that can be used all times even if nothing is active.
    /// </remarks>
    public sealed class HandheldEquipmentController :
        IHandheldEquipmentController,
        IDisposable
    {
        private readonly HandheldEquipmentQueue _equipQueue = new();
        private readonly HandheldEquipmentStateMachine _stateMachine = new();

        private IHandheldRegistry _registry;
        private MonoBehaviour _coroutineRunner;

        private Coroutine _updateCoroutine;
        private float _transitionSpeed = 1f;
        private bool _isDisposed;

        public IHandheld ActiveHandheld => _stateMachine.ActiveHandheld;
        public ControllerState EquipmentState => _stateMachine.CurrentState;

        public event ControllerEquipmentStateChangedDelegate EquipmentStateChanged;
        public event ControllerEquipmentDelegate EquipBegin;
        public event ControllerEquipmentDelegate EquipEnd;
        public event ControllerEquipmentDelegate HolsterBegin;
        public event ControllerEquipmentDelegate HolsterEnd;

        public void Initialize(IHandheldRegistry registry, MonoBehaviour coroutineRunner)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));

            _stateMachine.StateChanged += OnStateMachineStateChanged;
        }

        private void OnStateMachineStateChanged(in ManagerEquipmentStateChangeEventArgs args)
            => EquipmentStateChanged?.Invoke(in args);

        public void SetDefaultHandheld(IHandheld handheld) => _equipQueue.SetDefaultHandheld(handheld);

        public bool TryEquip(IHandheld handheld, float transitionSpeed = 1, UnityAction onEquipBegin = null)
        {
            if(ValidateHandheld(handheld) == false)
            {
                return false;
            }

            if(_equipQueue.TryPush(handheld, onEquipBegin) == false)
            {
                return false;
            }

            _transitionSpeed = transitionSpeed;
            StartEquipmentUpdate();
            return true;
        }

        public bool TryHolster(IHandheld handheld, float transitionSpeed = 1)
        {
            if (ValidateHandheld(handheld) == false)
            {
                return false;
            }

            bool wasActiveHandheld = _equipQueue.TryPop(handheld);
            if (wasActiveHandheld)
            {
                _transitionSpeed = transitionSpeed;
                StartEquipmentUpdate();
            }

            return true;
        }

        public void HolsterAll()
        {
            _equipQueue.Clear();
            StartEquipmentUpdate();
        }

        /// <summary>
        /// Terminates current equipment routine if the transition speed is at maximum speed,
        /// so holstering will be instant. <br></br>
        /// Then creates a new routine for the equipment change.
        /// </summary>
        private void StartEquipmentUpdate()
        {
            if(_isDisposed)
            {
                return;
            }

            if(_updateCoroutine != null 
            && EquipmentState == ControllerState.Equipping 
            && _transitionSpeed >= HandheldAnimationConstants.MaximumHolsteringSpeed)
            {
                StopCurrentUpdate();
                EquipEnd?.Invoke(ActiveHandheld);
            }

            _updateCoroutine ??= _coroutineRunner.StartCoroutine(UpdateEquipmentRoutine());
        }

        private void StopCurrentUpdate()
        {
            if(_updateCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        /// <summary>
        /// Continuously try to equip the handheld that is at the top of the equip queue,
        /// holsters current one if needed.
        /// </summary>
        /// <remarks>
        /// The <see langword="while"/> loop is required, because as holstering/equipping action is not
        /// instantenous. User can continuously change handhelds which in case triggers a new equipment over
        /// and over again. In this case, we don't stop until the top of the queue is equipped successfuly.
        /// </remarks>
        private IEnumerator UpdateEquipmentRoutine()
        {
            do
            {
                HandheldQueueEntry? currentEntry = _equipQueue.Current;
                if(currentEntry.HasValue == false)
                {
                    break;
                }

                if(ActiveHandheld != null && currentEntry.Value.UniqueID != _stateMachine.ActiveHandheldID)
                {
                    yield return HolsterCurrentHandheld();
                }

                if(currentEntry.Value.Handheld != ActiveHandheld)
                {
                    yield return EquipHandheld(currentEntry.Value);
                }

            } while(_equipQueue.Current?.UniqueID != _stateMachine.ActiveHandheldID);

            _stateMachine.TransitionTo(ControllerState.None);
            _updateCoroutine = null;
        }

        private IEnumerator HolsterCurrentHandheld()
        {
            _stateMachine.TransitionTo(ControllerState.Holstering, _transitionSpeed);
            HolsterBegin?.Invoke(ActiveHandheld);

            yield return ActiveHandheld.Holster(_transitionSpeed);

            HolsterEnd?.Invoke(ActiveHandheld);
            _stateMachine.SetActiveHandheld(null, 0);
        }

        private IEnumerator EquipHandheld(HandheldQueueEntry entry)
        {
            _stateMachine.TransitionTo(ControllerState.Equipping);
            _stateMachine.SetActiveHandheld(entry.Handheld, entry.UniqueID);

            EquipBegin?.Invoke(entry.Handheld);

            if(entry.EquipCallback != null)
            {
                _coroutineRunner.InvokeNextFrame(entry.EquipCallback);
            }

            yield return entry.Handheld.Equip(_transitionSpeed);

            EquipEnd?.Invoke(entry.Handheld);
        }

        private bool ValidateHandheld(IHandheld handheld)
        {
            if(handheld != null && _registry.IsRegistered(handheld) == false)
            {
                string handheldName = handheld?.gameObject?.name ?? handheld?.ToString() ?? "Unknown";
                Debug.LogError($"The handheld {handheldName} is not registered");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }

            StopCurrentUpdate();
            _stateMachine.StateChanged -= OnStateMachineStateChanged;
            _stateMachine.Reset();

            _isDisposed = true;
        }
    }
}
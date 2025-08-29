using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public readonly struct ManagerEquipmentStateChangeEventArgs
    {
        public readonly IHandheld Handheld;
        public readonly ControllerState OldState;
        public readonly ControllerState NewState;
        public readonly float TransitionSpeed;

        public ManagerEquipmentStateChangeEventArgs(IHandheld handheld, ControllerState oldState, 
            ControllerState newState, float transitionSpeed = 1f)
        {
            Handheld = handheld;
            OldState = oldState;
            NewState = newState;
            TransitionSpeed = transitionSpeed;
        }
    }

    /// <summary>
    /// Delegate for informing about a state change in the controller.
    /// </summary>
    /// <param name="args">Information about the state change.</param>
    public delegate void ControllerEquipmentStateChangedDelegate(in ManagerEquipmentStateChangeEventArgs args);

    public sealed class HandheldEquipmentStateMachine
    {
        private ControllerState _currentState = ControllerState.None;
        private IHandheld _activeHandheld;
        private int _activeHandheldID;

        public ControllerState CurrentState => _currentState;
        public IHandheld ActiveHandheld => _activeHandheld is NullHandheld ? null : _activeHandheld;
        public int ActiveHandheldID => _activeHandheldID;

        public event ControllerEquipmentStateChangedDelegate StateChanged;

        public void SetActiveHandheld(IHandheld handheld, int uniqueID)
        {
            _activeHandheld = handheld;
            _activeHandheldID = uniqueID;
        }

        public void TransitionTo(ControllerState newState, float transitionSpeed = 1f)
        {
            if(_currentState == newState)
            {
                return;
            }

            var oldState = _currentState;
            _currentState = newState;

            var eventArgs = new ManagerEquipmentStateChangeEventArgs(_activeHandheld, oldState, newState, transitionSpeed);
            StateChanged?.Invoke(in eventArgs);
        }

        public void Reset()
        {
            _currentState = ControllerState.None;
            _activeHandheld = null;
            _activeHandheldID = 0;
        }
    }
}
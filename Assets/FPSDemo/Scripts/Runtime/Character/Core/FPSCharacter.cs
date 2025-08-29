using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.ProceduralMotion;
using Nexora.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo
{
    [SelectionBase]
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public sealed class FPSCharacter : 
        HumanCharacter, 
        IFPSCharacter,
        ISaveableComponent
    {
        public CharacterMotionHandler HeadMotion { get; private set; }
        public CharacterMotionHandler HandsMotion { get; private set; }

        public event UnityAction Initialized;

        public void Initialize(CharacterMotionHandler headMotion, CharacterMotionHandler handsMotion)
        {
            HeadMotion = headMotion;
            HandsMotion = handsMotion;

            Initialized?.Invoke();
        }

        private void OnEnable()
        {
            if(TryGetCC<IMovementController>(out var movementController))
            {
                movementController.AddStateTransitionListener(MovementStateType.None, OnMovementStateChanged);
            }
        }

        private void OnDisable()
        {
            if (TryGetCC<IMovementController>(out var movementController))
            {
                movementController.RemoveStateTransitionListener(MovementStateType.None, OnMovementStateChanged);
            }
        }

        private void OnMovementStateChanged(MovementStateType movementState)
        {
            HeadMotion.DataBroadcaster.SetCurrentState(movementState);
            HandsMotion.DataBroadcaster.SetCurrentState(movementState);
        }

        public void Load(object data) => Name = (string)data;
        public object Save() => Name;
    }
}
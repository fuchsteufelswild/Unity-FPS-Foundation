using Nexora.FPSDemo.Movement;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public abstract class ActionMovementBlocker<T> :
        HandheldActionBlocker<T>
        where T : IInputActionHandler
    {
        [Tooltip("Blocks these movemenet states while performing the action.")]
        [SerializeField]
        private MovementStateType[] _movementStatesToBlock;

        private T _actionHandler;

        private bool _movementStatesBlocked;
        private IMovementController _movementController;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            base.OnBehaviourStart(parent);
            _movementController = parent.GetCC<IMovementController>();
        }

        protected override ActionBlockerCore GetBlocker(IHandheld handheld)
        {
            BlockMovementStates(false);
            handheld.TryGetActionOfType<T>(out _actionHandler);
            return _actionHandler?.Blocker;
        }

        protected override bool Validate()
        {
            BlockMovementStates(_actionHandler?.IsPerforming ?? false);
            return base.Validate();
        }

        protected void BlockMovementStates(bool shouldBlock)
        {
            if (_movementStatesBlocked == shouldBlock)
            {
                return;
            }

            if (shouldBlock)
            {
                foreach (var movementState in _movementStatesToBlock)
                {
                    _movementController.AddStateBlocker(this, movementState);
                }
            }
            else
            {
                foreach (var movementState in _movementStatesToBlock)
                {
                    _movementController.RemoveStateBlocker(this, movementState);
                }
            }

            _movementStatesBlocked = shouldBlock;
        }
    }
}
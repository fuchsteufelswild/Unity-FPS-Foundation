using Nexora.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    public interface IMovementController : 
        IMovementModifiers, 
        IMovementStateBlocker, 
        IMovementStateEvents, 
        IMovementStateMachine,
        IStepCycleManager,
        ICharacterBehaviour
    {

    }

    public sealed class CharacterMovementController :
        ChildBehaviour<ICharacterBehaviour, ICharacter>,
        IMovementController,
        ISaveableComponent
    {
        [SerializeField]
        private MovementControllerConfig _controllerConfig;

        [SerializeField]
        private FirstPersonMovementInput _inputHandler;

        [ReorderableList(elementLabel: "State")]
        [ReferencePicker(typeof(ICharacterMovementState), TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ICharacterMovementState[] _defaultStates;

        // Core systems
        private ICharacterMotor _characterMotor;
        private MovementStateMachine _movementStateMachine = new();
        private MovementModifiers _movementModifiers;
        private StepCycleManager _stepCycleManager = new();

        public MovementStateType ActiveStateType => _movementStateMachine.ActiveStateType;
        public CompositeValue SpeedModifier => _movementModifiers.SpeedModifier;
        public CompositeValue AccelerationModifier => _movementModifiers.AccelerationModifier;
        public CompositeValue DecelerationModifier => _movementModifiers.DecelerationModifier;
        public float StepCycle => _stepCycleManager.StepCycle;

        public event UnityAction StepCycleEnded
        {
            add => _stepCycleManager.StepCycleEnded += value;
            remove => _stepCycleManager.StepCycleEnded -= value;
        }

        private void Awake() => _movementModifiers = new MovementModifiers(_controllerConfig);

        protected override void OnBehaviourStart(ICharacter parent)
        {
            base.OnBehaviourStart(parent);
            _characterMotor = parent.GetCC<ICharacterMotor>();

            foreach(ICharacterMovementState state in _defaultStates)
            {
                state.InitializeState(this, _inputHandler, _characterMotor, Parent);
            }

            _stepCycleManager.Initialize(_controllerConfig, _characterMotor, _movementStateMachine.GetActiveState);
            _movementStateMachine.Initialize(_defaultStates);
            _movementStateMachine.InitializeWithState(MovementStateType.Idle);
        }

        protected override void OnBehaviourEnable(ICharacter parent) => _characterMotor.SetMovementInputFunction(ProcessMovementInput);

        protected override void OnBehaviourDisable(ICharacter parent) => _characterMotor.SetMovementInputFunction(null);

        private Vector3 ProcessMovementInput(Vector3 velocity, out bool applyGravity, out bool snapToGround)
        {
            float deltaTime = Time.deltaTime;
            ICharacterMovementState activeState = _movementStateMachine.GetActiveState();

            if(activeState == null)
            {
                applyGravity = true;
                snapToGround = true;
                return Vector3.zero;
            }

            applyGravity = activeState.ApplyGravity;
            snapToGround = activeState.SnapToGround;

            Vector3 newVelocity = activeState.UpdateVelocity(velocity, deltaTime);

            _stepCycleManager.TickStepCycle(deltaTime);
            activeState.UpdateLogic();

            return newVelocity;
        }

        public void AddStateBlocker(Object blocker, MovementStateType stateType) 
            => _movementStateMachine?.StateBlockingSystem.AddStateBlocker(blocker, stateType);

        public void RemoveStateBlocker(Object blocker, MovementStateType stateType)
            => _movementStateMachine?.StateBlockingSystem.RemoveStateBlocker(blocker, stateType);

        public void AddStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> transitionCallback, MovementStateTransitionType transitionType = MovementStateTransitionType.Enter)
            => _movementStateMachine?.StateEventController.AddStateTransitionListener(stateType, transitionCallback, transitionType);

        public void RemoveStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> transitionCallback, MovementStateTransitionType transitionType = MovementStateTransitionType.Enter)
            => _movementStateMachine?.StateEventController.RemoveStateTransitionListener(stateType, transitionCallback, transitionType);

        public bool TrySetState(ICharacterMovementState state) => _movementStateMachine?.TrySetState(state) ?? false;
        public bool TrySetState(MovementStateType stateType) => _movementStateMachine?.TrySetState(stateType) ?? false;
        T IMovementStateMachine.GetStateOfType<T>() => _movementStateMachine?.GetStateOfType<T>();

        public void Load(object data)
        {
            var savedState = (MovementStateType)data;
            _movementStateMachine.TrySetState(savedState);
        }
        public object Save() => ActiveStateType;
    }
}
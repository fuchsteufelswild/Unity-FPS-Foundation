using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Interface for a character's movement state.
    /// </summary>
    public interface ICharacterMovementState
    {
        /// <summary>
        /// Is state currently enabled? (Initialized and not blocked)
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// The type of the state.
        /// </summary>
        MovementStateType StateType { get; }

        /// <summary>
        /// Length of the step cycle for this state.
        /// </summary>
        float StepCycleLength { get; }

        /// <summary>
        /// Should apply gravity in this state?
        /// </summary>
        bool ApplyGravity { get; }

        /// <summary>
        /// Should snap the character to the ground in this state?
        /// </summary>
        bool SnapToGround { get; }

        void InitializeState(
            IMovementController movementController,
            IMovementInputController movementInput, 
            ICharacterMotor characterMotor, 
            ICharacter character);

        /// <returns>Can transition to this state right now?</returns>
        bool CanTransitionTo();

        /// <summary>
        /// Called when this state is entered.
        /// </summary>
        /// <param name="previousStateType">State, from which we transition to this state.</param>
        void OnEnter(MovementStateType previousStateType);

        /// <summary>
        /// Called when character exits from this state.
        /// </summary>
        void OnExit();

        /// <summary>
        /// Modifies the <paramref name="currentVelocity"/> which is calculated by the rules 
        /// of the movement state.
        /// </summary>
        /// <param name="currentVelocity">Current velocity of the character (it's the calculation from the last frame).</param>
        /// <returns>An updated velocity by the state.</returns>
        Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime);

        /// <summary>
        /// Executes state logic; controls transition rules, and does all functionality related to the state.
        /// </summary>
        void UpdateLogic();
    }

    public abstract class CharacterMovementState : ICharacterMovementState
    {
        protected const float VelocityThreshold = 0.01f;
        protected const float InputThreshold = 0.1f;

        private bool _isInitialized;

        // This should not be touched by the derived classes, externally modified.
        public bool IsEnabled { get; set; } = true;

        public abstract MovementStateType StateType { get; }
        public abstract float StepCycleLength { get; }
        public abstract bool ApplyGravity { get; }
        public abstract bool SnapToGround { get; }

        protected IMovementController MovementController { get; private set; }
        protected ICharacterMotor CharacterMotor { get; private set; }
        protected IMovementInputController MovementInput { get; private set; }
        protected ICharacter Character { get; private set; }

        void ICharacterMovementState.InitializeState(
            IMovementController movementController,
            IMovementInputController movementInput, 
            ICharacterMotor characterMotor, 
            ICharacter character)
        {
            if(_isInitialized)
            {
                return;
            }

            MovementController = movementController;
            CharacterMotor = characterMotor;
            MovementInput = movementInput;
            Character = character;

            _isInitialized = true;
            OnInitialized();
        }

        protected virtual void OnInitialized() { }
        public virtual bool CanTransitionTo() => true;
        public virtual void OnEnter(MovementStateType previousStateType) { }
        public virtual void OnExit() { }

        public abstract void UpdateLogic();
        public abstract Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime);
    }
}
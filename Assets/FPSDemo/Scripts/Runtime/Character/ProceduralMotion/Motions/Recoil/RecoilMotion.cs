using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public sealed class RecoilMotion : 
        IMotion,
        ILookInputProvider
    {
        [SerializeField]
        private RecoilProcessor _recoilProcessor = new();

        private readonly ILookController _lookController;

        public RecoilMotion(ICharacter character)
        {
            _lookController = character.GetCC<ILookController>();
        }

        public void SetRecoilSpringSettings(RecoilStateType stateType, SpringSettings springSettings)
        {
            _recoilProcessor.GetRecoilState(stateType)?.ChangeSpringSettings(springSettings);
        }

        public void AddRecoil(Vector2 recoilAmount)
        {
            // If this is the first shot, then register the input provider
            if(_recoilProcessor.IsActive == false)
            {
                _lookController.SetAdditiveInputProvider(this);
            }

            _recoilProcessor.AddRecoil(recoilAmount);
        }

        private Vector2 CalculateRecoil()
        {
            Vector2 recoil = _recoilProcessor.Update(Time.deltaTime, _lookController.LookDelta);

            if(_recoilProcessor.IsActive == false)
            {
                _lookController.SetAdditiveInputProvider(null);
            }

            return recoil;
        }

        public Vector2 GetLookInput() => CalculateRecoil();


        public float MixerBlendWeight { get; set; } = 1f;
        public void Tick(float deltaTime) { }
        public Vector3 CalculatePositionOffset(float deltaTime) => Vector3.zero;
        public Quaternion CalculateRotationOffset(float deltaTime) => Quaternion.identity;
    }

    /// <summary>
    /// Adds recoil/updates it and manages the states of Recoil and Recovery.
    /// Works on the basic idea that when we fire we add recoil to the target position,
    /// and when we stop firing we recover to the location depending on how much we controlled
    /// the recoil.
    /// </summary>
    /// <remarks>
    /// Controlling recoil means 'Moving mouse to the opposite direction of the recoil'.
    /// <br></br>
    /// Uses a primitive state machine, not so complex, so to not complicate the system as 
    /// it would be an overkill for such a system.
    /// </remarks>
    [Serializable]
    public class RecoilProcessor
    {
        private IRecoilState[] _recoilStates = Array.Empty<IRecoilState>();

        private IRecoilState _activeRecoilState;

        private readonly PhysicsSpring2D _spring;

        /// <summary>
        /// Current recoil (spring current position).
        /// </summary>
        private Vector2 _currentRecoil;
        private Vector2 _targetRecoil;
        private Vector2 _controlOffset;

        /// <summary>
        /// Target recoil (spring target position, when recoil active).
        /// </summary>
        public Vector2 TargetRecoil => _targetRecoil;

        /// <summary>
        /// Current recoil amount.
        /// </summary>
        public Vector2 CurrentRecoil => _currentRecoil;

        /// <summary>
        /// The control offset applied while recoiling (how much did you control the recoil).
        /// </summary>
        public Vector2 ControlOffset => _controlOffset;

        /// <summary>
        /// Is recoil state machine working?
        /// </summary>
        public bool IsActive => _activeRecoilState.StateType != RecoilStateType.Inactive;

        /// <summary>
        /// Is recoil movement idle?
        /// </summary>
        public bool IsIdle => _spring.IsAtRest;

        public RecoilProcessor()
        {
            _spring = new PhysicsSpring2D(SpringSettings.Default);

            _recoilStates = new IRecoilState[]
            {
                new InactiveState(this),
                new RecoilingState(this),
                new RecoveryState(this)
            };

            _activeRecoilState = GetRecoilState(RecoilStateType.Inactive);
        }

        /// <summary>
        /// Applies <paramref name="springSettings"/> to the recoil spring.
        /// </summary>
        public void ApplySpringSettings(SpringSettings springSettings) => _spring.ChangeSpringSettings(springSettings);
        public void SetTargetRecoil(Vector2 recoil) => _targetRecoil = recoil;
        public void SetControlOffset(Vector2 controlOffset) => _controlOffset = controlOffset;

        /// <summary>
        /// Sets <paramref name="targetPosition"/> as spring's target position.
        /// </summary>
        public void SetSpringTarget(Vector2 targetPosition) => _spring.SetTargetPosition(targetPosition);
        public void Reset() => TransitionTo(RecoilStateType.Inactive);

        public void ResetStateData()
        {
            _targetRecoil = Vector2.zero;
            _currentRecoil = Vector2.zero;
            _spring.Reset();
        }

        public void AddRecoil(Vector2 recoilAmount)
        {
            _activeRecoilState.AddRecoil(recoilAmount);
            _spring.SetTargetPosition(_targetRecoil);
        }

        public void TransitionTo(RecoilStateType newState)
        {
            _activeRecoilState = GetRecoilState(newState);
            _activeRecoilState.Enter();
        }

        public Vector2 Update(float deltaTime, Vector2 lookDelta)
        {
            if(_activeRecoilState.ShouldUpdate() == false)
            {
                return Vector2.zero;
            }

            _activeRecoilState.Tick(deltaTime, lookDelta);

            return _currentRecoil = _spring.Evaluate(deltaTime);
        }

        public T GetRecoilState<T>()
            where T : class, IRecoilState
        {
            foreach (var state in _recoilStates)
            {
                if (state is T recoilState)
                {
                    return recoilState;
                }
            }

            return null;
        }

        public IRecoilState GetRecoilState(RecoilStateType recoilStateType)
        {
            foreach (var state in _recoilStates)
            {
                if (state.StateType == recoilStateType)
                {
                    return state;
                }
            }

            return null;
        }

        public bool HasReachedTarget() => HasReachedTarget(_currentRecoil, _targetRecoil);

        private static bool HasReachedTarget(Vector2 current, Vector2 target)
        {
            const float Tolerance = 0.005f;
            return Mathf.Abs(current.x + Tolerance * Mathf.Sign(current.x)) > Mathf.Abs(target.x)
                && Mathf.Abs(current.y + Tolerance * Mathf.Sign(current.y)) > Mathf.Abs(target.y);
        }
    }
}
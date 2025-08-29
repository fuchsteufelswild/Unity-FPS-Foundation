using Nexora.SaveSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Class that handles moving the character given the input from the user and the effects of 
    /// the surrounding environment. Works with <see cref="CharacterController"/> and utilizes
    /// many handler classes that are specific to their functionality.
    /// <br></br>
    /// <see cref="CharacterMotorConfig"/> holds the config about the general settings of the 
    /// motor.
    /// </summary>
    /// <remarks>
    /// Main work is handled by the subclasses, check them to understand this class' inner behaviour.
    /// </remarks>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : 
        MonoBehaviour, 
        ICharacterMotor, 
        ISaveableComponent
    {
        [Tooltip("Asset holds the setting for motor's calculations.")]
        [SerializeField]
        private CharacterMotorConfig _motorConfig;

        private CharacterController _characterController;
        private Transform _cachedTransform;

        private CharacterMotorMovement _motorMovement;
        private IGroundDetector _groundDetector;
        private ISlopeSlider _slopeSlider;
        private IForceHandler _forceHandler;
        private ICollisionHandler _collisionHandler;
        private ICharacterPhysics _characterHeightHandler;
        private IRotationInfo _rotationTracker;

        private SlopeSpeedMultiplierCalculator _slopeSpeedMultiplierCalculator;

        /// <summary>
        /// Used to keep track of initialization to prevent double initializing this class
        /// in the case of loading from save.
        /// </summary>
        private bool _isInitialized;

        public Vector3 Velocity => _motorMovement.Velocity;
        public Vector3 SimulatedVelocity => _motorMovement.SimulatedVelocity;
        public bool IsGrounded => _motorMovement.IsGrounded;
        public float LastGroundedChangeTime => _motorMovement.LastGroundedChangeTime;
        public CollisionFlags CollisionFlags => _motorMovement.CollisionFlags;
        public Vector3 GroundNormal => _groundDetector.GroundNormal;
        public float GroundSurfaceAngle => _groundDetector.GroundSurfaceAngle;
        public float SlopeLimit => _characterController.slopeLimit;
        public float PushForce => _motorConfig.PushForce;
        public float Radius => _characterHeightHandler.Radius;
        public float DefaultHeight => _characterHeightHandler.DefaultHeight;
        public float Gravity => _characterHeightHandler.Gravity;
        public float Mass => _characterHeightHandler.Mass;
        public LayerMask CollisionMask => _characterHeightHandler.CollisionMask;
        public float TurnSpeed => _rotationTracker.TurnSpeed;

        public float Height
        {
            get => _characterHeightHandler.Height;
            set => _characterHeightHandler.Height = value;
        }
        public bool CanSetHeight(float height) => _characterHeightHandler.CanSetHeight(height);

        public float GetSlopeSpeedMultiplier()
        {
            return _slopeSpeedMultiplierCalculator.CalculateSpeedMultiplier(IsGrounded, GroundSurfaceAngle, GroundNormal, Velocity);
        }

        public event UnityAction Teleported
        {
            add => _motorMovement.Teleported += value;
            remove => _motorMovement.Teleported -= value;
        }

        public event UnityAction<bool> GroundedChanged
        {
            add => _motorMovement.GroundedChanged += value;
            remove => _motorMovement.GroundedChanged -= value;
        }

        public event UnityAction<float> FallImpact
        {
            add => _motorMovement.FallImpact += value;
            remove => _motorMovement.FallImpact -= value;
        }

        public event UnityAction<float> HeightChanged
        {
            add => _characterHeightHandler.HeightChanged += value;
            remove => _characterHeightHandler.HeightChanged -= value;
        }

        private void OnEnable() => _characterController.enabled = true;
        private void OnDisable() => _characterController.enabled = false;

        private void Awake()
        {
            _cachedTransform = transform;
            _characterController = GetComponent<CharacterController>();
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            if(_isInitialized)
            {
                return;
            }

            _groundDetector = new GroundDetector(_characterController, _motorConfig);
            _slopeSlider = new SlopeSlider(_motorConfig);
            _forceHandler = new ForceHandler(_motorConfig);
            _motorMovement = new CharacterMotorMovement(transform, _characterController, _motorConfig, _forceHandler, _slopeSlider, _groundDetector);
            _collisionHandler = new CollisionHandler(_motorConfig);
            _characterHeightHandler = new CharacterHeightHandler(_characterController, _motorConfig);
            _rotationTracker = new RotationTracker(transform);
            _slopeSpeedMultiplierCalculator = new SlopeSpeedMultiplierCalculator(_characterController, _motorConfig);
        }

        private void Update()
        {
            if(_motorMovement?.HasInput() == false)
            {
                return;
            }

            _motorMovement.ProcessMovement(Time.deltaTime);
            _rotationTracker.UpdateTurnSpeed();
        }

        public void SetMovementInputFunction(MovementInputDelegate motionInput) => _motorMovement.SetMovementInputFunction(motionInput);
        public bool HasInput() => _motorMovement.HasInput();
        public void AddForce(Vector3 force, ForceMode forceMode, bool snapToGround = false)
            => _forceHandler.AddForce(force, forceMode, snapToGround);
        public void ResetMotorMovement() => _motorMovement.ResetMotorMovement();
        public void Teleport(Vector3 position, Quaternion rotation) => _motorMovement.Teleport(position, rotation);

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _groundDetector.UpdateGroundNormal(hit.normal);

            _collisionHandler.HandleControllerCollision(hit);
        }

        #region Saving
        [Serializable]
        private sealed class SaveData
        {
            public float height;
            public Vector3 velocity;
            public Vector3 simulatedVelocity;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 lastPosition;
            public bool isGrounded;
        }

        void ISaveableComponent.Load(object data)
        {
            var saveData = (SaveData)data;
            InitializeSystems();
            _isInitialized = true;
            _characterController.height = 0f;
            Height = saveData.height;

            _motorMovement.LoadSaveData(saveData.velocity, saveData.simulatedVelocity, saveData.isGrounded, saveData.lastPosition);

            _characterController.enabled = false;
            _cachedTransform.SetPositionAndRotation(saveData.position, saveData.rotation);
            _characterController.enabled = true;
        }

        object ISaveableComponent.Save()
        {
            return new SaveData
            {
                height = _characterController.height,
                velocity = _motorMovement.Velocity,
                simulatedVelocity = _motorMovement.SimulatedVelocity,
                position = _cachedTransform.position,
                rotation = _cachedTransform.rotation,
                lastPosition = _motorMovement.LastPosition,
                isGrounded = _motorMovement.IsGrounded
            };
        }
        #endregion
    }


    public static class CharacterMotorExtensions
    {
        public static bool HasCollisionFlag(this CollisionFlags flags, CollisionFlags flag)
        {
            return EnumFlagsComparer.HasFlag(flags, flag);
        }

        public static bool IsMovingUp(this IMotorMovement motorMovement)
        {
            return motorMovement.Velocity.y > 0.1f;
        }

        public static bool IsOnSteepSlope(this ICharacterMotor characterMotor)
        {
            return characterMotor.GroundSurfaceAngle > characterMotor.SlopeLimit * 0.8f;
        }

        public static void SetHeight(this ICharacterPhysics characterPhysics, float height)
        {
            if(characterPhysics.CanSetHeight(height))
            {
                characterPhysics.Height = height;
            }
        }
    }
}
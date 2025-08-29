using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nexora.Motion
{
    /// <summary>
    /// Main interface for the motions that are blendable. 
    /// Provides methods to advance motion simulation, getting current position/rotation offset's and <see cref="MixerBlendWeight"/>.
    /// </summary>
    /// <remarks>
    /// Designed in a way that the motion will make use of <see cref="PhysicsSpring"/> to simulate the motion.
    /// <see cref="Tick(float)"/> should update spring target values by one frame further according to the motion. 
    /// <see cref="CalculatePositionOffset(float)"/> and <see cref="CalculateRotationOffset(float)"/> 
    /// should advance the <see cref="PhysicsSpring"/> used for position and rotation by one frame.
    /// </remarks>
    public interface IMotion
    {
        float MixerBlendWeight { get; set; }

        /// <summary>
        /// Like Unity's update function, advances the motion simulation one frame further.
        /// </summary>
        /// <param name="deltaTime"></param>
        void Tick(float deltaTime);

        /// <summary>
        /// Calculate the current position offset imposed by the motion. 
        /// </summary>
        /// <remarks>
        /// Using <paramref name="deltaTime"/> might advance subsystems one frame further as a side effect.
        /// </remarks>
        Vector3 CalculatePositionOffset(float deltaTime);

        /// <summary>
        /// Calculate the current rotation offset imposed by the motion. 
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="CalculatePositionOffset(float)" path="/remarks"/>
        /// </remarks>
        Quaternion CalculateRotationOffset(float deltaTime);
    }

    /// <summary>
    /// Abstract base blendable mixed motion class that uses <see cref="PhysicsSpring3D"/> for
    /// 3D position and rotation motion. Must be attached to a <see cref="IMotionMixer"/> class
    /// to have motion blended with other motions. Has self blend weight and may also use parent's.
    /// </summary>
    /// <remarks>
    /// Has two <see cref="PhysicsSpring3D"/>, one for position and one for rotation motion. By calling
    /// <see cref="SetTargetPosition(Vector3)"/> and <see cref="SetTargetRotation(Vector3)"/> the springs'
    /// target positions can be changed, and using <see cref="CalculatePositionOffset(float)"/> and 
    /// <see cref="CalculateRotationOffset(float)"/> spring's can be advanced a frame further. 
    /// <br></br><see cref="Tick(float)"/> should be implemented in a way that it updates the springs' 
    /// target positions.
    /// </remarks>
    /// <typeparam name="TChild">Type of the self, *child*, interface, e.g IPlayerBehaviour</typeparam>
    /// <typeparam name="TParent">
    /// Type of the parent <see cref="MonoBehaviour"/> this motion is acting on. 
    /// This component will be synced with that parent in Unity calls.
    /// It can be Player, IPlayer, SceneEffectCamera etc.
    /// </typeparam>
    [RequireComponent(typeof(IMotionMixer))]
    public abstract class Motion<TChild, TParent> :
        ChildBehaviour<TChild, TParent>,
        IMotion
        where TChild : IChildBehaviour<TChild, TParent>
        where TParent : IParentBehaviour<TParent, TChild>
    {
        [Tooltip("Self blend weight for the motion. Not confuse with the mixer or combined blend weight.")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _blendWeight = 1f;

        protected readonly PhysicsSpring3D _positionSpring = new();
        protected readonly PhysicsSpring3D _rotationSpring = new();

        // If mixer's blend weight is ignored then the multiplier is 1
        private bool _ignoreMixerBlendWeight;
        private float _mixerBlendWeightMultiplier;

        protected IMotionMixer Mixer { get; private set; }

        protected virtual SpringSettings DefaultPositionSpringSettings => SpringSettings.Default;
        protected virtual SpringSettings DefaultRotationSpringSettings => SpringSettings.Default;

        #region Blend Weight
        public float SelfBlendWeight
        {
            get => _blendWeight;
            set => _blendWeight = value;
        }

        public bool IgnoreMixerBlendWeight
        {
            get => _ignoreMixerBlendWeight;
            set
            { 
                _ignoreMixerBlendWeight = value;
                UpdateMixerBlendWeightMultiplier();
            }
        }

        /// <summary>
        /// Sets multiplier used from mixer to 1 if mixer's blend weight is ignored,
        /// else the real value.
        /// </summary>
        private void UpdateMixerBlendWeightMultiplier()
            => _mixerBlendWeightMultiplier = _ignoreMixerBlendWeight ? 1f : Mixer.BlendWeight;

        float IMotion.MixerBlendWeight
        {
            get => _mixerBlendWeightMultiplier;
            set
            {
                if (_ignoreMixerBlendWeight == false)
                {
                    _mixerBlendWeightMultiplier = value;
                }
            }
        }

        public float CombinedBlendWeight
        {
            // As this method can be called very often in every frame cycle
            // by all types of motions, it is a very good candidate for inlining.
            // Not storing it like other blend weights to allow us to modify blend
            // weight in the editor and see the results.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _blendWeight * _mixerBlendWeightMultiplier;
        }
        #endregion

        protected virtual void Awake()
        {
            InitializeSpringsWithDefaultSettings();
            Mixer = GetComponent<IMotionMixer>();
            Assert.IsTrue(Mixer != null, 
                "A blendable mixed motion must have a mixer to work with.");
            _ignoreMixerBlendWeight = false;
        }

        /// <summary>
        /// Registers self to the mixer.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            Mixer.AddMotion(this);
        }

        /// <summary>
        /// Unregisters self from the mixer.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            Mixer.RemoveMotion(this);
        }

        /// <summary>
        /// Initializes the springs with the default settings defined in <see langword="this"/> class.
        /// </summary>
        private void InitializeSpringsWithDefaultSettings()
        {
            _positionSpring.ChangeSpringSettings(DefaultPositionSpringSettings);
            _rotationSpring.ChangeSpringSettings(DefaultRotationSpringSettings);
        }

        public abstract void Tick(float deltaTime);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// Advances the position spring one frame and return the spring's new position.
        /// </remarks>
        public Vector3 CalculatePositionOffset(float deltaTime)
            => _positionSpring.Evaluate(deltaTime);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// Advances the rotation spring one frame and return the spring's new position.
        /// </remarks>
        public Quaternion CalculateRotationOffset(float deltaTime)
        {
            Vector3 eulerAngles = _rotationSpring.Evaluate(deltaTime);
            return Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// Sets position spring's target position.
        /// </summary>
        protected void SetTargetPosition(Vector3 target)
            => _positionSpring.SetTargetPosition(target * CombinedBlendWeight);

        /// <summary>
        /// Sets rotation spring's target position.
        /// </summary>
        protected void SetTargetRotation(Vector3 target)
            => _rotationSpring.SetTargetPosition(target * CombinedBlendWeight);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(Application.isPlaying == false || _positionSpring == null || _rotationSpring == null)
            {
                return;
            }

            InitializeSpringsWithDefaultSettings();
        }
#endif
    }
}

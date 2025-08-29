using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using Nexora.ObjectPooling;
using System;
using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Abstract base class for any effect to be triggered by ejection.
    /// </summary>
    [Serializable]
    public abstract class EjectionEffect
    {
        /// <summary>
        /// Initialize with the <paramref name="gun"/>.
        /// </summary>
        /// <param name="gun">Gun that triggers the ejection.</param>
        public virtual void Initialize(IGun gun) { }

        /// <summary>
        /// Plays the effect.
        /// </summary>
        public abstract void Trigger();
    }

    /// <summary>
    /// Plays the provided audio when shell ejection happens.
    /// </summary>
    [Serializable]
    public sealed class EjectionAudioEffect : EjectionEffect
    {
        [SerializeField]
        private AudioSequence _ejectionAudio;

        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = (gun as IHandheld);

        public override void Trigger() => _handheld.AudioPlayer.PlaySequence(_ejectionAudio, BodyPart.Hands);
    }

    /// <summary>
    /// Triggers animator when shell ejection happens.
    /// </summary>
    [Serializable]
    public sealed class EjectionAnimatorEffect : EjectionEffect
    {
        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = (gun as IHandheld);

        public override void Trigger() => _handheld.Animator.SetTrigger(HandheldAnimationConstants.Eject);
    }

    /// <summary>
    /// Effect for spawning and applying physics to a shell casing.
    /// </summary>
    [Serializable]
    public sealed class EjectionPhysicsShellEffect : EjectionEffect
    {
        private const float RotationRandomizeAmount = 0.1f;
        private const float VelocityRandomizeAmount = 0.55f;
        private const float ResetShellScaleDelay = 0.3f;

        /// <summary>
        /// Percentage owner speed will be used when ejecting the shell. If character moves at a speed
        /// we add this to the shell so that physics will be accurate but we reduce a bit so to avoid 
        /// visually more appealing look.
        /// </summary>
        private const float OwnerSpeedWeight = 0.2f;

        [Title("Spawning")]
        [Tooltip("Prefab of the ejected shell to be spawned.")]
        [SerializeField, NotNull]
        private Rigidbody _shellPrefab;

        [Tooltip("Where the ejected shell be spawned.")]
        [SerializeField]
        private Transform _ejectionPoint;

        [Tooltip("Delay of the spawning the ejected shell.")]
        [SerializeField, Range(0f, 5f)]
        private float _spawnDelay;

        [Title("Physics")]
        [Tooltip("Speed of the ejected shell spawned.")]
        [SerializeField, Range(0f, 100f)]
        private float _shellSpeed = 3f;

        [Tooltip("Spin of the ejected shell spawned.")]
        [SerializeField, Range(0f, 100f)]
        private float _shellSpin = 15f;

        [Tooltip("Scale of the ejected shell that is being spawned.")]
        [SerializeField, Range(0.1f, 5f)]
        private float _shellSizeScale = 1f;

        [Tooltip("Rotation of the ejected shell.")]
        [SerializeField]
        private Vector3 _shellRotation;

        private IHandheld _handheld;
        private MonoBehaviour _coroutineRunner;

        private bool HasShellSizeScale => Mathf.Abs(_shellSizeScale - 1f) > 0.01f;

        public override void Initialize(IGun gun)
        {
            _handheld = gun as IHandheld;
            _shellPrefab.maxAngularVelocity = 10000f;
            _coroutineRunner = gun as MonoBehaviour;

            if(ObjectPoolingModule.Instance.HasPool(_shellPrefab) == false)
            {
                ObjectPoolingModule.Instance.RegisterPool(_shellPrefab,
                    new ManagedSceneObjectPool<Rigidbody>(_shellPrefab, gun.gameObject.scene, FPSDemoPoolCategory.Shells, initialSize: 4, maxSize: 16));
            }
        }

        public override void Trigger()
        {
            if(_spawnDelay > 0.01f || _handheld.IsGeometryVisible == false || HasShellSizeScale)
            {
                _coroutineRunner.StartCoroutine(EjectShellDelayed());
            }
            else
            {
                EjectShell();
            }
        }

        private Transform EjectShell()
        {
            Vector3 spawnPosition = _ejectionPoint.position;
            Quaternion rotation = Quaternion.Euler(Quaternion.LookRotation(_ejectionPoint.forward) * _shellRotation);
            Quaternion randomizedRotation = Quaternion.Lerp(rotation, UnityEngine.Random.rotation, RotationRandomizeAmount);

            var shell = ObjectPoolingModule.Instance.Get(_shellPrefab, spawnPosition, randomizedRotation);

            Vector3 velocityJitter = MathUtils.CreateJitter(VelocityRandomizeAmount);
            Vector3 characterVelocity = _handheld.Character.GetCC<ICharacterMotor>().Velocity * OwnerSpeedWeight;
            Vector3 ejectVelocity = _ejectionPoint.TransformVector(Vector3.forward * _shellSpeed + velocityJitter);

            shell.linearVelocity = ejectVelocity + characterVelocity;
            shell.position = spawnPosition;

            float spinDirection = UnityEngine.Random.Range(0f, 100f) > 50f ? 1f : -1f;
            float spinSpeed = UnityEngine.Random.Range(0.5f, 1f) * _shellSpin;

            shell.angularVelocity = spinDirection * spinSpeed * Vector3.one;
            shell.transform.localScale = Vector3.one * _shellSizeScale;
            return shell.transform;
        }

        private IEnumerator EjectShellDelayed()
        {
            yield return new Delay(_spawnDelay);

            Transform shell = EjectShell();
            if (HasShellSizeScale || _handheld.IsGeometryVisible)
            {
                yield return AdjustShellScale(shell);
            }
        }

        private IEnumerator AdjustShellScale(Transform shell)
        {
            shell.localScale = _handheld.IsGeometryVisible == false ? Vector3.one : Vector3.one * _shellSizeScale;
            yield return new Delay(ResetShellScaleDelay);
            shell.localScale = Vector3.one;
        }
    }
}
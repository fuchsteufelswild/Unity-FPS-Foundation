using Nexora.Audio;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Base class to register for performing effects on specific reload stages.
    /// </summary>
    [Serializable]
    public abstract class ReloadEffector 
    {
        public virtual void Initialize(IGunMagazineBehaviour gunMagazine)
        {
            gunMagazine.AmmoCountChanged += OnAmmoCountChanged;
            gunMagazine.ReloadStarted += OnReloadStarted;
            gunMagazine.RoundLoaded += OnRoundLoaded;
            gunMagazine.ReloadCompleted += OnReloadCompleted;
            gunMagazine.ReloadCanceled += OnReloadCanceled;
            gunMagazine.ReloadEndPhaseStarted += OnReloadEndPhaseStarted;
        }

        public virtual void LateUpdate() { }

        public virtual void Enable(IGun gun) { }
        public virtual void Disable() { }

        public virtual void OnAmmoCountChanged(int previousAmmoCount, int currentAmmoCount) { }
        public virtual void OnReloadStarted(in ReloadStartEventArgs args) { }
        public virtual void OnRoundLoaded(in RoundLoadedArgs args) { }
        public virtual void OnReloadCompleted() { }
        public virtual void OnReloadCanceled() { }
        public virtual void OnReloadEndPhaseStarted(float animationSpeed) { }
    }

    /// <summary>
    /// Plays audios for different stages of the reloading process.
    /// </summary>
    [Serializable]
    public sealed class ReloadAudioEffector : ReloadEffector
    {
        [Tooltip("Audio sequence used when reloading with empty chamber.")]
        [SerializeField]
        private AudioSequence _emptyReloadStartAudio;

        [Tooltip("Audio sequence used when reloading with non-empty chamber.")]
        [SerializeField]
        private AudioSequence _reloadStartAudio;

        [Tooltip("Audio sequence played when a single round is loaded (Used when progressively loading guns).")]
        [SerializeField]
        private AudioSequence _onRoundLoadedAudio;

        [Tooltip("Audio sequence played when ending phase of reload starts.")]
        [SerializeField]
        private AudioSequence _reloadEndPhaseStartedAudio;

        [Tooltip("Audio sequence played when reload process finishes.")]
        [SerializeField]
        private AudioSequence _reloadCompletedAudio;

        private AudioSource _audioSource;

        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = (gun as IHandheld);

        public override void OnReloadStarted(in ReloadStartEventArgs args)
            => PlaySound(args.IsEmptyReload ? _emptyReloadStartAudio : _reloadStartAudio);

        public override void OnRoundLoaded(in RoundLoadedArgs args)
        {
            if (args.IsEmptyReload == false)
            {
                PlaySound(_onRoundLoadedAudio);
            }
        }

        public override void OnReloadEndPhaseStarted(float animationSpeed) => PlaySound(_reloadEndPhaseStartedAudio);

        public override void OnReloadCompleted() => PlaySound(_reloadCompletedAudio);
        private void PlaySound(AudioSequence audio) => _audioSource = _handheld.AudioPlayer.PlaySequence(audio, BodyPart.Hands);


        public override void OnReloadCanceled()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Sets animator parameters depending on the different phases of the animation.
    /// </summary>
    [Serializable]
    public sealed class ReloadAnimatorEffector : ReloadEffector
    {
        private IHandheld _handheld;

        public override void Enable(IGun gun)
        {
            _handheld = (gun as IHandheld);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, false);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, false);
        }

        public override void OnReloadStarted(in ReloadStartEventArgs args)
        {
            _handheld.Animator.SetFloat(HandheldAnimationConstants.ReloadSpeed, args.AnimationSpeed);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, args.IsEmptyReload);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, true);
        }

        public override void OnRoundLoaded(in RoundLoadedArgs args)
        {
            if (args.AnimationSpeed.HasValue)
            {
                _handheld.Animator.SetFloat(HandheldAnimationConstants.ReloadSpeed, args.AnimationSpeed.Value);
            }

            if (args.IsEmptyReload)
            {
                return;
            }

            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, false);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, true);
        }

        public override void OnReloadEndPhaseStarted(float animationSpeed)
        {
            _handheld.Animator.SetFloat(HandheldAnimationConstants.ReloadSpeed, animationSpeed);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, false);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, false);
        }

        public override void OnReloadCompleted()
        {
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, false);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, false);
        }

        public override void OnReloadCanceled()
        {
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmptyReload, false);
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsReloading, false);
        }
    }

    /// <summary>
    /// Ejects a magazine when reloading process takes place.
    /// </summary>
    [Serializable]
    public sealed class MagazineEjectEffector : ReloadEffector
    {
        private const float EjectedMagazineDestroyDelay = 30f;

        [Tooltip("Magazine prefab to spawn when ejecting the magazine.")]
        [SerializeField, NotNull]
        private Rigidbody _ejectedMagazinePrefab;

        [Tooltip("Time to wait before spawning ejected magazine.")]
        [SerializeField]
        private float _ejectionDelay = 1f;

        [Tooltip("Transform where the magazine prefab will be spawned.")]
        [SerializeField, NotNull]
        private Transform _ejectPoint;

        [Tooltip("The force magazine is ejected with.")]
        [SerializeField]
        private Vector3 _ejectionForce;

        [Tooltip("The torque magazine is ejected with.")]
        [SerializeField]
        private Vector3 _ejectionTorque;

        private IHandheld _handheld;

        private MonoBehaviour _coroutineRunner;

        public override void Enable(IGun gun)
        {
            _handheld = gun as IHandheld;
            _coroutineRunner = (gun as MonoBehaviour);
        }

        public override void OnReloadStarted(in ReloadStartEventArgs args)
        {
            if(args.IsEmptyReload)
            {
                _coroutineRunner.InvokeDelayed(EjectMagazine, _ejectionDelay);
            }
        }

        private void EjectMagazine()
        {
            var ejectedMagazine = GameObject.Instantiate(_ejectedMagazinePrefab, _ejectPoint.position, _ejectPoint.rotation);

            Vector3 ejectionForce = _handheld.Character.transform.TransformVector(_ejectionForce);
            Vector3 ejectionTorque = _handheld.Character.transform.TransformVector(_ejectionTorque);

            ejectedMagazine.linearVelocity = ejectionForce;
            ejectedMagazine.angularVelocity = ejectionTorque;

            CoroutineUtils.StartGlobalCoroutineDelayed(DestroyEjectedMagazine, EjectedMagazineDestroyDelay);
            return;

            void DestroyEjectedMagazine()
            {
                if(ejectedMagazine != null)
                {
                    GameObject.Destroy(ejectedMagazine);
                }
            }
        }
    }

    /// <summary>
    /// Controls the phase of the moveable parts of the gun.
    /// </summary>
    [Serializable]
    public sealed class MovingPartsEffector : ReloadEffector
    {
        [SerializeField]
        private HandheldPartAnimator _handheldPartAnimator;

        public override void Initialize(IGunMagazineBehaviour gunMagazine)
        {
            _handheldPartAnimator.Initialize();
            base.Initialize(gunMagazine);
        }

        public override void OnReloadStarted(in ReloadStartEventArgs args)
        {
            _handheldPartAnimator.StopMovement();
        }

        public override void OnAmmoCountChanged(int previousAmmoCount, int currentAmmoCount)
        {
            if(currentAmmoCount == 0)
            {
                _handheldPartAnimator.BeginMovement();
            }
            else
            {
                _handheldPartAnimator.StopMovement();
            }
        }

        public override void LateUpdate() => _handheldPartAnimator.UpdateMovement();
    }
}
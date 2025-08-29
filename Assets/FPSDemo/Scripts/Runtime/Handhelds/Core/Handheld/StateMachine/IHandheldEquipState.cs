using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Movement;
using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Enum to denote the equipped state of a <see cref="IHandheld"/>,
    /// if it is equipped or equipping etc.
    /// </summary>
    public enum HandheldEquipStateType
    {
        [Tooltip("Handheld is hidden.")]
        Hidden = 0,

        [Tooltip("Handheld is being equipped.")]
        Equipping = 1,

        [Tooltip("Handheld is equipped and in hands.")]
        Equipped = 2,

        [Tooltip("Handheld is being holstered.")]
        Holstering = 3
    }

    public interface IHandheldEquipState
    {
        /// <summary>
        /// This 
        /// </summary>
        HandheldEquipStateType EquipStateType { get; }

        /// <param name="targetState">Target state.</param>
        /// <returns>If this state can transition to <paramref name="targetState"/>.</returns>
        bool CanTransitionTo(HandheldEquipStateType targetState);

        /// <summary>
        /// Enters the state with coroutine.
        /// </summary>
        IEnumerator Enter(IHandheld context, HandheldEquipConfiguration configuration, float transitionSpeed = 1f);

        /// <summary>
        /// Exits the state with coroutine.
        /// </summary>
        IEnumerator Exit(IHandheld context);
    }

    public sealed class HiddenState : IHandheldEquipState
    {
        public HandheldEquipStateType EquipStateType => HandheldEquipStateType.Hidden;

        public bool CanTransitionTo(HandheldEquipStateType targetState) 
            => targetState == HandheldEquipStateType.Equipping;

        public IEnumerator Enter(IHandheld context, HandheldEquipConfiguration configuration, float transitionSpeed = 1f)
        {
            context.gameObject.SetActive(false);
            yield return null;
        }

        public IEnumerator Exit(IHandheld context)
        {
            context.gameObject.SetActive(true);
            yield return null;
        }
    }

    public sealed class EquippingState : IHandheldEquipState
    {
        public HandheldEquipStateType EquipStateType => HandheldEquipStateType.Equipping;

        public bool CanTransitionTo(HandheldEquipStateType targetState)
            => targetState is HandheldEquipStateType.Equipped or HandheldEquipStateType.Holstering;

        public IEnumerator Enter(IHandheld context, HandheldEquipConfiguration configuration, float transitionSpeed = 1f)
        {
            context.Animator.ResetTrigger(HandheldAnimationConstants.Holster);
            context.Animator.SetTrigger(HandheldAnimationConstants.Equip);

            context.AudioPlayer.PlaySequence(configuration.EquipAudio, BodyPart.Hands);

            yield return new Delay(configuration.EquipDuration);
        }

        public IEnumerator Exit(IHandheld context)
        {
            yield return null;
        }
    }

    public sealed class EquippedState : IHandheldEquipState
    {
        public HandheldEquipStateType EquipStateType => HandheldEquipStateType.Equipped;

        public bool CanTransitionTo(HandheldEquipStateType targetState) 
            => targetState == HandheldEquipStateType.Holstering;

        public IEnumerator Enter(IHandheld context, HandheldEquipConfiguration configuration, float transitionSpeed = 1f)
        {
            if(context.Character.TryGetCC(out IMovementController movementController))
            {
                movementController.SpeedModifier.AddModifier((context as IMovementSpeedAdjuster).SpeedModifier.Evaluate);
            }

            yield return null;
        }

        public IEnumerator Exit(IHandheld context)
        {
            yield return null;
        }
    }

    public sealed class HolsteringState : IHandheldEquipState
    {
        public HandheldEquipStateType EquipStateType => HandheldEquipStateType.Holstering;

        public bool CanTransitionTo(HandheldEquipStateType targetState)
            => targetState == HandheldEquipStateType.Hidden;

        public IEnumerator Enter(IHandheld context, HandheldEquipConfiguration configuration, float transitionSpeed = 1f)
        {
            // If transition speed is greater than the maximum speed, we skip the animation
            if(transitionSpeed < HandheldAnimationConstants.MaximumHolsteringSpeed)
            {
                transitionSpeed = Mathf.Max(transitionSpeed, HandheldAnimationConstants.MinimumHolsteringSpeed);
                yield return PlayTransition(context, transitionSpeed, configuration.HolsterDuration, configuration.HolsterAudio);
            }
            else
            {
                context.Animator.Play("Idle", 0, 0f);
                yield return new WaitForEndOfFrame();
            }

            if (context.Character.TryGetCC(out IMovementController movementController))
            {
                movementController.SpeedModifier.RemoveModifier((context as IMovementSpeedAdjuster).SpeedModifier.Evaluate);
            }
        }

        private IEnumerator PlayTransition(IHandheld context, float transitionSpeed, float baseHolsterDuration, AudioSequence holsterAudio)
        {
            float holsterDuration = baseHolsterDuration / transitionSpeed;

            context.Animator.SetTrigger(HandheldAnimationConstants.Holster);
            context.Animator.SetFloat(HandheldAnimationConstants.HolsterSpeed, transitionSpeed);
            context.AudioPlayer.PlaySequence(holsterAudio, BodyPart.Hands);

            yield return new Delay(holsterDuration);
        }

        public IEnumerator Exit(IHandheld context)
        {
            yield return null;
        }
    }
}
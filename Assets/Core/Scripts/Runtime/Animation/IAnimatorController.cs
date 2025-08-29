using System;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Interface that acts like a Unity's <see cref="UnityEngine.Animator"/>.
    /// It allows for easier testing, classes act like animator and not dependence on concrete Unity types.
    /// Also may be used for custom animator solutions.
    /// </summary>
    /// <remarks>
    /// Default implementations are provided through <see cref="AnimatorExtensions"/>. 
    /// But these calls are expensive, the ones that uses <see langword="string"/> names
    /// of parameters, avoid using them in context that are frequently used.
    /// </remarks>
    public interface IAnimatorController
    {
        bool IsAnimating { get; set; }

        /// <summary>
        /// The object controlled by the animator is visible?
        /// </summary>
        bool IsObjectVisible { get; set; }

        /// <param name="parameterId">Parameter's hashed id.</param>
        void SetInteger(int parameterId, int value);

        /// <param name="parameterId"><inheritdoc cref="SetInteger(int, int)" path="/param[@name='parameterId']"/></param>
        void SetFloat(int parameterId, float value);

        /// <param name="parameterId"><inheritdoc cref="SetInteger(int, int)" path="/param[@name='parameterId']"/></param>
        void SetBool(int parameterId, bool value);

        /// <param name="parameterId"><inheritdoc cref="SetInteger(int, int)" path="/param[@name='parameterId']"/></param>
        void SetTrigger(int parameterId);

        /// <param name="parameterId"><inheritdoc cref="SetInteger(int, int)" path="/param[@name='parameterId']"/></param>
        void ResetTrigger(int parameterId);

        void Play(string id, int layer, float normalizedTime) { }
    }

    /// <summary>
    /// Null object pattern. If you are in position to might have <see cref="IAnimatorController"/> but it can be 
    /// <see langword="null"/> sometimes, set it to <see cref="NullAnimator.Instance"/>. 
    /// All calls are safely to be made, all are ignored.
    /// </summary>
    public sealed class NullAnimator : IAnimatorController
    {
        public static readonly NullAnimator Instance = new();

        public bool IsAnimating { get => false; set { } }
        public bool IsObjectVisible { get => false; set { } }

        public void SetInteger(int parameterId, int value) { }
        public void SetFloat(int parameterId, float value) { }
        public void SetBool(int parameterId, bool value) { }
        public void SetTrigger(int parameterId) { }
        public void ResetTrigger(int parameterId) { }
    }

    /// <summary>
    /// Composite animator that propagates calls to many animators.
    /// All of the animators contained in this composite, will have the
    /// <b>same state</b>. All calls will be propagated to all without any difference.
    /// </summary>
    public sealed class CompositeAnimator : IAnimatorController
    {
        private readonly IAnimatorController[] _animators;

        public CompositeAnimator(IAnimatorController[] animators)
        {
            _animators = animators;
        }

        public bool IsAnimating 
        { 
            get => _animators.Length > 0 && _animators.First().IsAnimating;
            set
            {
                foreach (var animator in _animators)
                {
                    animator.IsAnimating = value;
                }
            } 
        }

        public bool IsObjectVisible 
        { 
            get => _animators.Length > 0 && _animators.First().IsObjectVisible;
            set
            { 
                foreach(var animator in _animators)
                {
                    animator.IsObjectVisible = value;
                }
            } 
        }

        public void SetInteger(int parameterId, int value)
        {
            foreach(var animator in _animators)
            {
                animator.SetInteger(parameterId, value);
            }
        }

        public void SetFloat(int parameterId, float value)
        {
            foreach (var animator in _animators)
            {
                animator.SetFloat(parameterId, value);
            }
        }

        public void SetBool(int parameterId, bool value)
        {
            foreach (var animator in _animators)
            {
                animator.SetBool(parameterId, value);
            }
        }

        public void SetTrigger(int parameterId)
        {
            foreach (var animator in _animators)
            {
                animator.SetTrigger(parameterId);
            }
        }

        public void ResetTrigger(int parameterId)
        {
            foreach (var animator in _animators)
            {
                animator.ResetTrigger(parameterId);
            }
        }

        public void Play(string id, int layer, float normalizedTime)
        {
            foreach (var animator in _animators)
            {
                animator.Play(id, layer, normalizedTime);
            }
        }
    }

    public static class AnimatorExtensions
    {
        public static void SetParameter(
            this IAnimatorController animator, 
            AnimatorControllerParameterType parameterType, 
            int parameterIdHash, 
            float value)
        {
            switch(parameterType)
            {
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameterIdHash, (int)value);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameterIdHash, value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameterIdHash, value > 0f);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(parameterIdHash);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetInteger(this IAnimatorController animator, string parameterName, int value)
            => animator.SetInteger(Animator.StringToHash(parameterName), value);

        public static void SetFloat(this IAnimatorController animator, string parameterName, float value)
            => animator.SetFloat(Animator.StringToHash(parameterName), value);

        public static void SetBool(this IAnimatorController animator, string parameterName, bool value)
            => animator.SetBool(Animator.StringToHash(parameterName), value);

        public static void SetTrigger(this IAnimatorController animator, string parameterName)
            => animator.SetTrigger(Animator.StringToHash(parameterName));

        public static void ResetTrigger(this IAnimatorController animator, string parameterName)
            => animator.ResetTrigger(Animator.StringToHash(parameterName));
    }
}

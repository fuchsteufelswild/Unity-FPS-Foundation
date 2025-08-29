using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.PostProcessing
{
    [Serializable]
    public abstract class PostFxParameterAnimation
    {
        [SerializeField]
        private bool _enabled = true;

        public bool Enabled => _enabled;

        public abstract void Animate(float t);
        public abstract void Dispose();
    }

    /// <summary>
    /// Animation for the parameters. Has a designated target value, gets <see cref="VolumeParameter{T}"/>
    /// and changes its value throughout animation. Sets <see cref="VolumeParameter{T}"/> back to its 
    /// original state after animation is finished.
    /// </summary>
    /// <typeparam name="T">The type <see cref="VolumeParameter{T}"/> works on.</typeparam>
    [Serializable]
    public class PostFxParameterAnimation<T> :
        PostFxParameterAnimation
    {
        [SerializeField]
        private T _targetValue;

        [SerializeField]
        private AnimationCurve _animation;

        // Original values of the VolumeParameter, will be set back when animation is done
        private T _originalValue;
        private bool _originalOverrideStateParameter;

        private VolumeParameter<T> _parameter;

        public PostFxParameterAnimation(float curveStartValue, float curveEndValue)
            => _animation = AnimationCurve.EaseInOut(0f, curveStartValue, 1f, curveEndValue);

        public override void Animate(float t)
        {
#if UNITY_EDITOR
            if(Enabled == false)
            {
                _parameter.value = _originalValue;
                return;
            }
#endif

            _parameter.Interp(_originalValue, _targetValue, _animation.Evaluate(t));
        }

        public override void Dispose()
        {
            _parameter.value = _originalValue;
            _parameter.overrideState = _originalOverrideStateParameter;
            _parameter = null;
        }

        public PostFxParameterAnimation<T> SetParameter(VolumeParameter<T> parameter)
        {
            if (Enabled == false)
            {
                return null;
            }

            _parameter = parameter;
            _originalValue = parameter.GetValue<T>();
            _originalOverrideStateParameter = parameter.overrideState;
            parameter.overrideState = true;
            return this;
        }
    }
}

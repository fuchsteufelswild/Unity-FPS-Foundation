using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Used for transforms that needs to be moved for some time to the target, like 
    /// revolver hammer, when triggering charge it moves back but can be made a little and stop.
    /// </summary>
    [Serializable]
    public sealed class HandheldPartAnimator
    {
        private enum MoveState
        {
            Idle = 0,
            MovingIn = 1,
            MovingOut = 2
        }

        [Title("Timing")]
        [SerializeField, Range(0f, 20f)]
        private float _startDelay = 0.1f;

        [SerializeField, Range(0f, 20f)]
        private float _easingDuration = 0.1f;

        [SerializeField, Range(0f, 20f)]
        private float _stopDuration;

        [Title("General")]
        [SerializeField]
        private Ease _easeType = Ease.SineIn;

        [ReorderableList(ListStyle.Lined)]
        [SerializeField]
        private PartTransform[] _partsToAnimate = Array.Empty<PartTransform>();

        private MoveState _currentState = MoveState.Idle;
        private float _stateStartTime;
        private float _currentLerpFactor;

        public void Initialize()
        {
            for(int i = 0; i < _partsToAnimate.Length; i++)
            {
                _partsToAnimate[i].InitialLocalPosition = _partsToAnimate[i].Part.localPosition;
                _partsToAnimate[i].InitialLocalRotation = _partsToAnimate[i].Part.localRotation;
            }
        }

        public void BeginMovement()
        {
            _stateStartTime = Time.time + _startDelay;
            _currentState = MoveState.MovingIn;
        }

        public void StopMovement()
        {
            _stateStartTime = Time.time;
            _currentState = MoveState.MovingOut;
        }

        public void UpdateMovement()
        {
            switch (_currentState)
            {
                case MoveState.MovingIn:
                    if(Time.time >= _stateStartTime)
                    {
                        float elapsedTime = Time.time - _stateStartTime;
                        float progress = Mathf.Clamp01(elapsedTime / _easingDuration);
                        _currentLerpFactor = Easing.Evaluate(_easeType, progress);
                        ApplyTransformation(_currentLerpFactor);
                    }
                    break;
                case MoveState.MovingOut:
                    if(_currentLerpFactor > 0f)
                    {
                        float elapsedTime = Time.time - _stateStartTime;
                        float progress = 1f - Mathf.Clamp01(elapsedTime / _stopDuration);
                        _currentLerpFactor *= progress;
                        ApplyTransformation(_currentLerpFactor);
                    }
                    else
                    {
                        _currentState = MoveState.Idle;
                    }
                    break;
            }
        }

        private void ApplyTransformation(float lerpFactor)
        {
            foreach(PartTransform part in _partsToAnimate)
            {
                if(part.Part == null)
                {
                    continue;
                }

                Vector3 targetPosition = part.InitialLocalPosition + part.PositionOffset;
                Quaternion targetRotation = part.InitialLocalRotation * Quaternion.Euler(part.RotationOffset);


                part.Part.localPosition = Vector3.Lerp(part.InitialLocalPosition, targetPosition, lerpFactor);
                part.Part.localRotation = Quaternion.Slerp(part.InitialLocalRotation, targetRotation, lerpFactor);
            }
        }

        [Serializable]
        private struct PartTransform
        {
            [NotNull]
            public Transform Part;
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;

            [HideInInspector]
            public Vector3 InitialLocalPosition;

            [HideInInspector]
            public Quaternion InitialLocalRotation;
        }
    }
}
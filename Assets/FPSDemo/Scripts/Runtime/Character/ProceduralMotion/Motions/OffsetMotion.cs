using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Defines a static motion on position and rotation. Allows enter/exit force when changing the data
    /// and/or <see cref="MovementStateType"/>.
    /// </summary>
    /// <remarks>
    /// Enter/exit forces are only for rotations.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(IMovementController))]
    public sealed class OffsetMotion : CharacterDataMotion<OffsetData>
    {
        [Flags]
        private enum AllowedTransitionForceType
        {
            None = 0,
            Enter = 1 << 0,
            Exit = 1 << 1,
            Both = Enter | Exit
        }

        [SerializeField]
        private TransitionProcessor _transitionProcessor;

        private MovementStateTracker _movementStateTracker;

        private IMovementController _movementController;
        private OffsetData _previousData;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _movementController = parent.GetCC<IMovementController>();
            _movementStateTracker = new(_movementController.ActiveStateType);
        }

        protected override void OnMotionDataChanged(OffsetData motionData)
        {
            MovementStateType currentMovementState = _movementController.ActiveStateType;

            _transitionProcessor.ProcessDataTransition(
                _previousData, motionData, _movementStateTracker.PreviousState, currentMovementState, Time.time);

            if(motionData != null)
            {
                _positionSpring.ChangeSpringSettings(motionData.PositionSpring);
                _rotationSpring.ChangeSpringSettings(motionData.RotationSpring);
            }

            _previousData = motionData;
            _movementStateTracker.UpdateState(currentMovementState);
        }

        public override void Tick(float deltaTime)
        {
            Vector3 transitionForces = _transitionProcessor.TransitionForceManager.EvaluateAndCleanup(Time.time);

            if(CurrentMotionData == null)
            {
                if(_rotationSpring.IsAtRest == false)
                {
                    SetTargetRotation(transitionForces);
                }

                return;
            }

            Vector3 positionOffset = CurrentMotionData.PositionOffset;
            Vector3 rotationOffset = CurrentMotionData.RotationOffset + transitionForces;

            SetTargetPosition(positionOffset);
            SetTargetRotation(rotationOffset);
        }

        private sealed class MovementStateTracker
        {
            private MovementStateType _previousState;

            public MovementStateType PreviousState => _previousState;

            public MovementStateTracker(MovementStateType currentState) => _previousState = currentState;

            /// <summary>
            /// Updates the state to be <paramref name="currentState"/>.
            /// </summary>
            /// <returns>If state has changed.</returns>
            public bool UpdateState(MovementStateType currentState)
            {
                bool stateChanged = _previousState != currentState;
                _previousState = currentState;
                return stateChanged;
            }
        }

        [Serializable]
        private sealed class TransitionProcessor
        {
            [SerializeField]
            private TransitionRuleEvaluator _transitionRuleEvaluator;

            private ForceManager _forceManager = new();

            public ForceManager TransitionForceManager => _forceManager;

            public void ProcessDataTransition(OffsetData previousData, OffsetData currentData, 
                MovementStateType previousState, MovementStateType currentState, float currentTime)
            {
                if(previousData == currentData)
                {
                    return;
                }

                AllowedTransitionForceType allowedTransitions = _transitionRuleEvaluator.EvaluateTransition(previousState, currentState);

                ProcessExitForce(previousData, allowedTransitions, currentTime);
                ProcessEnterForce(currentData, allowedTransitions, currentTime);
            }

            /// <summary>
            /// Applies exit force if allowed.
            /// </summary>
            /// <param name="previousData">The data we are transitioning from.</param>
            /// <param name="allowedTransitions">What can kind of forces allowed for this state transition.</param>
            /// <param name="currentTime">Current game time.</param>
            private void ProcessExitForce(OffsetData previousData, AllowedTransitionForceType allowedTransitions, float currentTime)
            {
                if (previousData == null)
                {
                    return;
                }

                if (EnumFlagsComparer.HasFlag(allowedTransitions, AllowedTransitionForceType.Exit))
                {
                    SpringImpulseDefinition exitForce = previousData.StateExitForce;
                    _forceManager.AddConstantForce(exitForce.Impulse, exitForce.Duration, currentTime);
                }
            }

            private void ProcessEnterForce(OffsetData currentData, AllowedTransitionForceType allowedTransitions, float currentTime)
            {
                if(currentData == null)
                {
                    return;
                }    

                if(EnumFlagsComparer.HasFlag(allowedTransitions, AllowedTransitionForceType.Enter))
                {
                    SpringImpulseDefinition enterForce = currentData.StateEnterForce;
                    _forceManager.AddConstantForce(enterForce.Impulse, enterForce.Duration, currentTime);
                }
            }
        }

        /// <summary>
        /// Evaluates available transition types given a transition from one <see cref="MovementStateType"/> to another.
        /// </summary>
        [Serializable]
        private sealed class TransitionRuleEvaluator
        {
            [ReorderableList]
            [SerializeField]
            private TransitionData[] _ignoredMovementTransitions;

            [ReorderableList(HasLabels = false)]
            [SerializeField]
            private MovementStateType[] _ignoredCustomTransitions;

            /// <summary>
            /// Evaluates what types of transitions are allowed for a transition (<paramref name="from"/>-<paramref name="to"/>).
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public AllowedTransitionForceType EvaluateTransition(MovementStateType from,  MovementStateType to)
            {
                if(from == to)
                {
                    return IsCustomTransitionIgnored(from)
                        ? AllowedTransitionForceType.Enter
                        : AllowedTransitionForceType.Both;
                }

                return IsMovementTransitionIgnored(from, to)
                    ? AllowedTransitionForceType.None
                    : AllowedTransitionForceType.Both;
            }

            private bool IsCustomTransitionIgnored(MovementStateType stateType) => _ignoredCustomTransitions.Contains(stateType);

            private bool IsMovementTransitionIgnored(MovementStateType from, MovementStateType to)
            {
                for(int i = 0; i < _ignoredMovementTransitions.Length; i++)
                {
                    if (_ignoredMovementTransitions[i].From == from && _ignoredMovementTransitions[i].To == to)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Serializable]
        private struct TransitionData
        {
            public MovementStateType From;
            public MovementStateType To;

            public TransitionData(MovementStateType from, MovementStateType to)
            {
                From = from;
                To = to;
            }
        }
    }
}
using Nexora.FPSDemo.Movement;
using System;
using UnityEngine;

using static Nexora.FPSDemo.ProceduralMotion.GeneralMotionRandomizationManager;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(IMovementController))]
    public class JumpMotion : CharacterDataMotion<CompositeMotionData>
    {
        private GeneralMotionStateController _stateController;
        private GeneralMotionRandomizationManager _randomizationManager;

        private IMovementController _movementController;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _movementController = parent.GetCC<IMovementController>();

            Transform targetTransform = Mixer.Target;

            var animationPlayer = new GeneralMotionPlayer();
            _randomizationManager = new GeneralMotionRandomizationManager(
                new AlternatingRandomizationStrategy(),
                positionPartsToApply: ApplyTransformComponentParts.X,
                rotationPartsToApply: ApplyTransformComponentParts.Y | ApplyTransformComponentParts.Z);
            var curveEvalutor = new GeneralMotionCurveEvaluator(_randomizationManager);

            _stateController = new GeneralMotionStateController(animationPlayer, curveEvalutor);
        }

        protected override void OnMotionDataChanged(CompositeMotionData motionData)
        {
            if(motionData?.Jump == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.Jump.PositionSpring);
            _positionSpring.ChangeSpringSettings(motionData.Jump.RotationSpring);
        }

        protected override void OnBehaviourEnable(ICharacter parent)
            => _movementController.AddStateTransitionListener(MovementStateType.Jump, OnJump);

        protected override void OnBehaviourDisable(ICharacter parent)
            => _movementController.RemoveStateTransitionListener(MovementStateType.Jump, OnJump);

        private void OnJump(MovementStateType state)
        {
            _stateController.StartAnimation(1f);
            _randomizationManager.GenerateNextFactor();

            Tick(Time.deltaTime);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData?.Jump == null || _stateController.IsMotionActive == false)
            {
                return;
            }

            ProcessJumpAnimation(deltaTime);
        }

        private void ProcessJumpAnimation(float deltaTime)
        {
            var (position, rotation, positionContinue, rotationContinue) = _stateController.Tick(deltaTime, Mixer.Target, CurrentMotionData.Jump);

            if(positionContinue)
            {
                SetTargetPosition(position);
            }
            
            if(rotationContinue)
            {
                SetTargetRotation(rotation);
            }

            if(positionContinue == false && rotationContinue == false)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }
    }
}
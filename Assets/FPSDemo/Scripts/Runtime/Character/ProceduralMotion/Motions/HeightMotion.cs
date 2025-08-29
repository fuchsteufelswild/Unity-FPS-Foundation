using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    [RequireComponent(typeof(IMotionMixer))]
    public sealed class HeightMotion :
        CharacterBehaviour,
        IMotion
    {
        [SerializeField]
        private SpringSettings _springSettings;

        private float _defaultHeight;
        private PhysicsSpring _spring;

        private ICharacterMotor _characterMotor;
        
        public float MixerBlendWeight { get => 1f; set { } }

        public void Tick(float deltaTime) { }
        public Quaternion CalculateRotationOffset(float deltaTime) => Quaternion.identity;
        public Vector3 CalculatePositionOffset(float deltaTime) => Vector3.up * _spring.Evaluate(deltaTime);

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();
            _spring = new PhysicsSpring(_springSettings);
            _defaultHeight = _characterMotor.DefaultHeight;
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _characterMotor.HeightChanged += SetTargetHeight;
            _characterMotor.Teleported += _spring.Reset;
            GetComponent<IMotionMixer>().AddMotion(this);
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _characterMotor.HeightChanged -= SetTargetHeight;
            _characterMotor.Teleported -= _spring.Reset;
            GetComponent<IMotionMixer>().RemoveMotion(this);
        }

        /// <summary>
        /// Moves the spring to the direction to the height change.
        /// E.g if 'defaultHeight' is '2' and <paramref name="height"/> is '0.5',
        /// then it moves to '-1.5' so to make height '0.5'.
        /// </summary>
        /// <param name="height">New height value to set.</param>
        private void SetTargetHeight(float height) => _spring.SetTargetPosition(height - _defaultHeight);
    }
}
using UnityEngine;

namespace Nexora.FPSDemo
{
    [SelectionBase]
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public class HumanCharacter : Character
    {
        [SerializeField]
        private string _characterName;

        [SerializeField]
        private Transform _headTransform;

        [SerializeField]
        private Transform _chestTransform;

        [SerializeField]
        private Transform _handsTransform;

        [SerializeField]
        private Transform _feetTransform;

        public override string Name
        {
            get => _characterName;
            set => _characterName = value;
        }

        public override Transform GetTransformOfBodyPart(BodyPart bodyPart)
        {
            return bodyPart switch
            {
                BodyPart.Head => _headTransform,
                BodyPart.Chest => _chestTransform,
                BodyPart.Hands => _handsTransform,
                BodyPart.Legs => _feetTransform,
                BodyPart.Feet => _feetTransform,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
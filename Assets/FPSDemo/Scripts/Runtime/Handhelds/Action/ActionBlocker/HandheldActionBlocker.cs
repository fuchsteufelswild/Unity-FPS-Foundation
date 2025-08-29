using Nexora.FPSDemo.Movement;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Class for blocking actions of type <typeparamref name="T"/> on <see cref="IHandheld"/>.
    /// </summary>
    public abstract class HandheldActionBlocker<T> : 
        CharacterBehaviour
        where T : IInputActionHandler
    {
        [SerializeField]
        private ActionBlocker _blocker;

        [SerializeField]
        private ActionValidator _validator;

        public bool IsBlocked => _blocker.IsBlocked;

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _validator.Initialize(parent);
            var handheldsController = parent.GetCC<IHandheldsManager>();
            handheldsController.HolsterBegin += OnHandheldHolstered;
            handheldsController.EquipBegin += OnHandheldEquipped;
            OnHandheldEquipped(handheldsController.ActiveHandheld);
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            var handheldsController = parent.GetCC<IHandheldsManager>();
            handheldsController.HolsterBegin -= OnHandheldHolstered;
            handheldsController.EquipBegin -= OnHandheldEquipped;
            OnHandheldEquipped(null);
        }

        private void OnHandheldHolstered(IHandheld handheld)
        {
            if(_blocker.HasValidBlocker == false)
            {
                return;
            }

            StopAllCoroutines();

            _blocker.RemoveBlocker(this);
            _blocker.SetBlocker(null);

            Validate();
        }

        protected virtual bool Validate() => _validator.IsValid();

        private void OnHandheldEquipped(IHandheld handheld)
        {
            if(handheld == null)
            {
                return;
            }

            _blocker.SetBlocker(GetBlocker(handheld));
        }

        protected virtual ActionBlockerCore GetBlocker(IHandheld handheld) => handheld.GetActionOfType<T>().Blocker;

        private void FixedUpdate()
        {
            if (_blocker.HasValidBlocker == false)
            {
                return;
            }

            bool allRulesValid = Validate();

            _blocker.SetBlockState(this, !allRulesValid);
        }

        [Serializable]
        private sealed class  ActionValidator
        {
            [ReorderableList]
            [ReferencePicker(typeof(IActionValidationRule))]
            [SerializeReference]
            private IActionValidationRule[] _validationRules;

            public void Initialize(ICharacter character)
            {
                foreach (var validationRule in _validationRules)
                {
                    validationRule.Initialize(character);
                }
            }

            public bool IsValid() => _validationRules.All(rule => rule.IsValid());
        }

        [Serializable]
        private sealed class ActionBlocker
        {
            [Tooltip("How much time (in seconds) should passed before action is unblocked for it to go to sleep?")]
            [SerializeField, Range(0f, 10f)]
            private float _actionBlockCooldown = 0.4f;

            private ActionBlockerCore _actionBlocker;
            private bool _isBlocked;

            public bool HasValidBlocker => _actionBlocker != null;
            public bool IsBlocked => _actionBlocker?.IsBlocked ?? false;

            public void SetBlocker(ActionBlockerCore blocker)
            {
                _actionBlocker = blocker;
                _isBlocked = false;
            }

            public void SetBlockState(UnityEngine.Object blockObject, bool shouldBlock)
            {
                if (_isBlocked == shouldBlock)
                {
                    return;
                }

                if (shouldBlock)
                {
                    _actionBlocker.AddBlocker(blockObject);
                }
                else
                {
                    _actionBlocker.BlockFor(_actionBlockCooldown);
                    _actionBlocker.RemoveBlocker(blockObject);
                }

                _isBlocked = shouldBlock;
            }

            public void RemoveBlocker(UnityEngine.Object blockObject) => _actionBlocker.RemoveBlocker(blockObject);
        }
    }
}
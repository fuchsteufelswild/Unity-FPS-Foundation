using Nexora.FPSDemo.Handhelds.RangedWeapon;
using Nexora.FPSDemo.Options;
using Nexora.InputSystem;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IFirstPersonHandheldInputHandler : ICharacterBehaviour
    {

    }

    public sealed class FirstPersonHandheldInput :
        InputHandler<ICharacterBehaviour, ICharacter>,
        IFirstPersonHandheldInputHandler
    {
        [SerializeField]
        private InputActionReference _useAction;

        [SerializeField]
        private InputActionReference _dropAction;

        [SerializeField]
        private InputActionReference _aimAction;

        [SerializeField]
        private InputActionReference _changeFireModeAction;

        [SerializeField]
        private InputActionReference _reloadAction;

        [SerializeField]
        private InputActionReference _selectHandheldAction;

        [SerializeField]
        private InputActionReference _holsterAction;

        private IHandheldsInventory _handheldsInventory;
        private IHandheldsManager _handheldsManager;
        private IAimActionHandler _aimActionHandler;
        private IUseActionHandler _useActionHandler;
        private IReloadActionHandler _reloadActionHandler;
        private IHandheld _activeHandheld;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _handheldsManager = parent.GetCC<IHandheldsManager>();
            _handheldsInventory = parent.GetCC<IHandheldsInventory>();

            _handheldsManager.EquipEnd += OnEquip;
            _handheldsManager.HolsterBegin += OnHolster;
        }

        protected override void OnBehaviourDestroy(ICharacter parent)
        {
            if(_handheldsManager == null)
            {
                return;
            }

            _handheldsManager.EquipEnd -= OnEquip;
            _handheldsManager.HolsterBegin -= OnHolster;
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _holsterAction.AddStartedListener(HandleHolsterInput);
            _selectHandheldAction.AddStartedListener(HandleSelectHandheldInput);
            _dropAction.AddStartedListener(HandleDropInput);

            _useAction.IncrementUsageCount();
            _aimAction.IncrementUsageCount();

            _changeFireModeAction.AddStartedListener(HandleFiremodeChangeInput);
            _reloadAction.AddStartedListener(HandleReloadInput);

            if(_handheldsManager.EquipmentState == ControllerState.None)
            {
                OnEquip(_handheldsManager.ActiveHandheld);
            }
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _holsterAction.RemoveStartedListener(HandleHolsterInput);
            _selectHandheldAction.RemoveStartedListener(HandleSelectHandheldInput);
            _dropAction.RemoveStartedListener(HandleDropInput);

            _useAction.DecrementUsageCount();
            _aimAction.DecrementUsageCount();

            _changeFireModeAction.RemoveStartedListener(HandleFiremodeChangeInput);
            _reloadAction.RemoveStartedListener(HandleReloadInput);

            OnHolster(_activeHandheld);
        }

        /// <summary>
        /// Set handheld and action handlers.
        /// </summary>
        private void OnEquip(IHandheld handheld)
        {
            _activeHandheld = handheld;
            _useActionHandler = handheld.GetActionOfType<IUseActionHandler>();
            _reloadActionHandler = handheld.GetActionOfType<IReloadActionHandler>();
            _aimActionHandler = handheld.GetActionOfType<IAimActionHandler>();
        }

        /// <summary>
        /// End any active action.
        /// </summary>
        private void OnHolster(IHandheld handheld)
        {
            _useActionHandler?.HandleInput(InputActionState.End);
            _useActionHandler = null;

            _aimActionHandler?.HandleInput(InputActionState.End);
            _aimActionHandler = null;

            _reloadActionHandler?.HandleInput(InputActionState.End);
            _reloadActionHandler = null;
        }

        private void HandleReloadInput(InputAction.CallbackContext context) => _reloadActionHandler?.HandleInput(InputActionState.Start);

        private void HandleFiremodeChangeInput(InputAction.CallbackContext context)
        {
            if(_activeHandheld is IGun && _activeHandheld.gameObject.TryGetComponent<IndexedAttachmentSelector>(out var attachmentSelector))
            {
                attachmentSelector.ToggleNextAttachment();
            }
        }

        private void HandleDropInput(InputAction.CallbackContext context) => _handheldsInventory.TryDropHandheld(true);

        /// <summary>
        /// Selection by keys like (1, 2, 3, 4, 5..)
        /// </summary>
        /// <param name="context"></param>
        private void HandleSelectHandheldInput(InputAction.CallbackContext context)
        {
            int index = (int)context.ReadValue<float>() - 1;
            _handheldsInventory.SelectAtIndex(index);
        }

        private void HandleHolsterInput(InputAction.CallbackContext context)
        {
            _handheldsInventory.SelectAtIndex(_handheldsInventory.SelectedIndex != IHandheldsInventory.InvalidSelectorID 
                ? IHandheldsInventory.InvalidSelectorID
                : _handheldsInventory.PreviousIndex);
        }

        private void Update()
        {
            HandleUseInput();
            HandleAimInput();
        }

        private void HandleUseInput()
        {
            if(_useActionHandler == null)
            {
                return;
            }

            if (_useAction.action.triggered)
            {
                _useActionHandler.HandleInput(InputActionState.Start);
            }
            else if (_useAction.action.ReadValue<float>() > 0.001f)
            {
                _useActionHandler.HandleInput(InputActionState.Hold);
            }
            else if (_useAction.action.WasReleasedThisFrame() || _useAction.action.enabled == false)
            {
                _useActionHandler.HandleInput(InputActionState.End);
            }
        }

        private void HandleAimInput()
        {
            if(_aimActionHandler == null)
            {
                return;
            }

            if(InputOptions.Instance.AimToggleMode)
            {

            }
            else
            {
                if(_aimAction.action.ReadValue<float>() > 0.001f)
                {
                    if(_aimActionHandler.IsPerforming == false)
                    {
                        _aimActionHandler.HandleInput(InputActionState.Start);
                    }
                }
                else if(_aimActionHandler.IsPerforming)
                {
                    _aimActionHandler.HandleInput(InputActionState.End);
                }
            }
        }
    }
}
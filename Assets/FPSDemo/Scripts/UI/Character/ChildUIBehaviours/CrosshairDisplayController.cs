using Nexora.FPSDemo.Handhelds;
using Nexora.FPSDemo.Options;
using Nexora.InputSystem;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nexora.FPSDemo.UI
{
    public interface ICrosshairDisplayController : ICharacterUIBehaviour
    { 
    
    }


    public sealed class CrosshairDisplayController : 
        CharacterUIBehaviour, 
        ICrosshairDisplayController
    {
        [Tooltip("Canvas group to fade in/out crosshair.")]
        [SerializeField]
        private CanvasGroup _crosshairsCanvas;

        [Tooltip("Maximum alpha value for the crosshair.")]
        [SerializeField, Range(0f, 1f)]
        private float _maxAlpha = 0.9f;

        [Tooltip("Alpha when the crosshair is disabled (e.g gun cannot be used).")]
        [SerializeField, Range(0f, 1f)]
        private float _disabledAlpha = 0.4f;

        [Tooltip("Fade speed for the fade in/out.")]
        [SerializeField, Range(0f, 100f)]
        private float _fadeEaseSpeed = 0.4f;

        private CrosshairDisplay[] _availableCrosshairs;

        private CrosshairDisplay _activeCrosshair;
        private ICrosshairHandler _crosshairHandler;


        private bool _isActive;
        private float _currentAlpha;
        private float _currentScale;

        protected override void Awake()
        {
            base.Awake();

            _availableCrosshairs = GetComponentsInChildren<CrosshairDisplay>(true);

            foreach(var crosshair in _availableCrosshairs)
            {
                crosshair.Hide();
            }

            enabled = false;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            base.OnCharacterAttached(character);

            var handheldsManager = character.GetCC<IHandheldsManager>();
            handheldsManager.EquipBegin += OnEquipStarted;
            handheldsManager.HolsterBegin += OnHolsterStarted;

            OnEquipStarted(handheldsManager.ActiveHandheld);

            DamageEventSystem.SubscribeToDamage(character, OnDamageDealt);

            enabled = true;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            base.OnCharacterDetached(character);

            var handheldsManager = character.GetCC<IHandheldsManager>();
            handheldsManager.EquipBegin -= OnEquipStarted;
            handheldsManager.HolsterBegin -= OnHolsterStarted;

            DamageEventSystem.UnsubscribeFromDamage(character, OnDamageDealt);

            enabled = false;
        }

        private void OnEquipStarted(IHandheld handheld)
        {
            if(handheld is ICrosshairHandler crosshairHandler)
            {
                _crosshairHandler = crosshairHandler;
                crosshairHandler.CrosshairChanged += OnCrosshairChanged;
                OnCrosshairChanged(crosshairHandler.CrosshairID);
            }
            else
            {
                OnCrosshairChanged(0);
            }
        }

        private void OnCrosshairChanged(int crosshairID)
        {
            CrosshairDisplay previousCrosshair = _activeCrosshair;

            CrosshairDisplay newCrosshair = crosshairID < 0 ? null : _availableCrosshairs[crosshairID];

            if(previousCrosshair != null && previousCrosshair != newCrosshair)
            {
                previousCrosshair.Hide();
            }

            _activeCrosshair = newCrosshair;

            if(newCrosshair != null)
            {
                _currentScale = 0f;
                _currentAlpha = 0f;
                _isActive = true;

                newCrosshair.Show();
                newCrosshair.SetSize(1f, 0f);
                newCrosshair.SetColor(CombatOptions.Instance.CrosshairColor);
            }
            else
            {
                _isActive = false;
            }
        }

        private void OnHolsterStarted(IHandheld handheld)
        {
            if(_crosshairHandler != null)
            {
                _crosshairHandler.CrosshairChanged -= OnCrosshairChanged;
            }

            _crosshairHandler = null;
            _isActive = false;

            if(_activeCrosshair != null)
            {
                _activeCrosshair.SetSize(1f, 0f);
            }
        }

       

        private void OnDamageDealt(IDamageReceiver target, DamageOutcomeType damageOutcomeType, float damageDealt, in DamageContext context)
        {
            ICharacter damageSource = target.Character;
            if (damageSource != Parent)
            {
                // Play hitmarker animation
            }
        }

        private void LateUpdate()
        {
            float accuracy = _crosshairHandler?.Accuracy ?? 1f;

            if(_isActive)
            {
                float lerpSpeed = Time.deltaTime * _fadeEaseSpeed;

                bool isActive = _isActive; // InputModule.Instance.HasCancelAction == false;

                float targetAlpha = isActive ? 1f * _maxAlpha : 0f;
                targetAlpha *= _crosshairHandler == null || _crosshairHandler.IsCrosshairActive() ? 1f : _disabledAlpha;
                _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, lerpSpeed);
                _crosshairsCanvas.alpha = _currentAlpha;

                _currentScale = Mathf.Lerp(_currentScale, isActive ? 1f : 0f, lerpSpeed);
                _activeCrosshair.SetSize(accuracy, _currentScale);
            }

            // Update hit marker

            // Update charge
        }
    }
}
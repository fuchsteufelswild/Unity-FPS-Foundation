using Nexora.Experimental.Tweening;
using Nexora.FPSDemo.Handhelds;
using Nexora.FPSDemo.Handhelds.RangedWeapon;
using Nexora.UI;
using System;
using TMPro;
using UnityEngine;

namespace Nexora.FPSDemo.UI
{
    public interface IGunAmmoDisplay : ICharacterUIBehaviour
    {

    }

    public sealed class GunAmmoDisplay :
        CharacterUIBehaviour,
        IGunAmmoDisplay
    {
        [Title("References")]
        [SerializeField]
        private TextMeshProUGUI _magazineText;

        [SerializeField]
        private TextMeshProUGUI _ammoStorageText;

        [Title("Visual Settings")]
        [SerializeField]
        private Gradient _magazineColor;

        [SerializeField]
        private UITweenToTarget _tweenToTarget;

        [SerializeField]
        private UIAnimationConfig _showAnimation;

        [SerializeField]
        private UIAnimationConfig _ammoUseAnimation;

        [SerializeField]
        private UIAnimationConfig _hideAnimation;

        private UITweener _selfTweener;
        private UITweener _ammoTweener;

        private IGunMagazineBehaviour _magazine;
        private IGunAmmoStorage _ammoStorage;
        private IGun _gun;

        private Tween<Vector3> _positionTween; 

        private bool _isVisible;

        protected override void Awake()
        {
            base.Awake();

            _selfTweener = new UITweener(transform as RectTransform, _showAnimation, _hideAnimation);
            _ammoTweener = new UITweener(_magazineText.transform as RectTransform, _ammoUseAnimation);
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _isVisible = false;
            character.GetCC<IHandheldsManager>().EquipEnd += OnHandheldEquipped;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.GetCC<IHandheldsManager>().EquipEnd -= OnHandheldEquipped;

            if(_gun != null)
            {
                _gun.RemoveComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
                _gun.RemoveComponentChangedListener(GunBehaviourType.AmmunitionSystem, OnAmmoStorageChanged);

                if(_magazine != null)
                {
                    _magazine.AmmoCountChanged -= UpdateMagazineText;
                }

                if(_ammoStorage != null)
                {
                    _ammoStorage.AmmoCountChanged -= UpdateAmmoStorageText;
                }
            }
        }

        private void OnHandheldEquipped(IHandheld handheld)
        {
            if(handheld == _gun)
            {
                return;
            }

            DetachFromCurrentGun();

            if(handheld is IGun gun)
            {
                AttachToGun(gun);
            }
            else if(_isVisible)
            {
                PlayHideAnimation();
            }
        }

        private void DetachFromCurrentGun()
        {
            if (_gun != null)
            {
                _gun.RemoveComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
                _gun.RemoveComponentChangedListener(GunBehaviourType.AmmunitionSystem, OnAmmoStorageChanged);
                _gun = null;

                OnMagazineChanged();
                OnAmmoStorageChanged();
            }
        }

        private void AttachToGun(IGun gun)
        {
            _gun = gun;

            _gun.AddComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
            _gun.AddComponentChangedListener(GunBehaviourType.AmmunitionSystem, OnAmmoStorageChanged);

            OnMagazineChanged();
            OnAmmoStorageChanged();

            PlayShowAnimation();
        }

        private void PlayShowAnimation()
        {
            if (_isVisible == false)
            {
                _tweenToTarget.Show(transform as RectTransform, _showAnimation);
                _selfTweener.Execute(_showAnimation);
                _isVisible = true;
            }
        }

        private void PlayHideAnimation()
        {
            _tweenToTarget.Show(transform as RectTransform, _hideAnimation);
            _selfTweener.Execute(_hideAnimation);
            _isVisible = false;
        }

        private void OnMagazineChanged()
        {
            if(_magazine != null)
            {
                _magazine.AmmoCountChanged -= UpdateMagazineText;
            }

            _magazine = _gun?.Magazine;

            if(_magazine != null)
            {
                _magazine.AmmoCountChanged += UpdateMagazineText;
                UpdateMagazineText(_magazine.CurrentAmmoCount, _magazine.CurrentAmmoCount);
            }
        }

        private void UpdateMagazineText(int previousAmmo, int currentAmmo)
        {
            _magazineText.text = currentAmmo.ToString();

            // Update gradient color
            float t = (_magazine.Capacity > 0) ? currentAmmo / (float)_magazine.Capacity : 0f;
            _magazineText.color = _magazineColor.Evaluate(t);

            if(previousAmmo > currentAmmo)
            {
                _ammoTweener.Execute(_ammoUseAnimation);
            }
        }

        private void OnAmmoStorageChanged()
        {
            if (_ammoStorage != null)
            {
                _ammoStorage.AmmoCountChanged -= UpdateAmmoStorageText;
            }

            _ammoStorage = _gun?.AmmoStorage;

            if (_magazine != null)
            {
                _ammoStorage.AmmoCountChanged += UpdateAmmoStorageText;
                UpdateAmmoStorageText(_ammoStorage.CurrentAmmo);
            }
        }

        private void UpdateAmmoStorageText(int currentAmmo) => _ammoStorageText.text = currentAmmo.ToString();
    }
}
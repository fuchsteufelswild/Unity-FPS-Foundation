using Nexora.Audio;
using Nexora.Experimental.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nexora.UI
{
    /// <summary>
    /// Base interface for a system like Unity's state machine for button graphics
    /// where you can change animator state, or graphic color depending on the button's state.
    /// This is more of an extension and code-based solution for that for our own button system.
    /// Derive from this class and create own feedbacks.
    /// </summary>
    public interface IUIVisualFeedback
    {
        void ExecuteFeedback(UIInteractionState state, bool instant);
        void Initialize(InteractiveUIElementBase interactiveElement);

#if UNITY_EDITOR
        void EditorValidate(InteractiveUIElementBase interactiveElement) => Initialize(interactiveElement);
#endif
    }

    [Serializable]
    public sealed class UIColorFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private Graphic _targetGraphic;

        [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField]
        private Color _highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        [SerializeField]
        private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        [SerializeField]
        private Color _selectedColor = Color.cyan;

        [SerializeField]
        private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [SerializeField, Range(0.05f, 1f)]
        private float _fadeDuration = 0.1f;

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if (_targetGraphic == null || 
                _targetGraphic.transform.IsChildOf(interactiveElement.transform) == false)
            {
                _targetGraphic = interactiveElement.GetComponentInChildren<Graphic>();
            }

            ExecuteFeedback(
                interactiveElement.IsInteractable
                ? UIInteractionState.Normal
                : UIInteractionState.Disabled, true);
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            _targetGraphic.CrossFadeColor(
                GetTargetColor(state), 
                instant ? 0f : _fadeDuration, 
                true, 
                true);
        }

        private Color GetTargetColor(UIInteractionState state)
        {
            return state switch
            {
                UIInteractionState.Highlighted => _highlightedColor,
                UIInteractionState.Pressed => _pressedColor,
                UIInteractionState.Selected => _selectedColor,
                UIInteractionState.Disabled => _disabledColor,
                _ => _normalColor
            };
        }


    }

    [Serializable]
    public sealed class UISoundFeedback : IUIVisualFeedback
    {
        private const float CooldownBetweenPlays = 0.1f;

        private static float _highlightCooldownExpireTime;
        private static float _selectCooldownExpireTime;

        [SerializeField]
        private AudioCue _highlightSound = new(null);

        [SerializeField]
        private AudioCue _selectSound = new(null);

        private InteractiveUIElementBase _targetElement;

        public void Initialize(InteractiveUIElementBase interactiveElement) => _targetElement = interactiveElement;

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            switch(state)
            {
                case UIInteractionState.Highlighted:
                    OnHighlighted(instant);
                    break;
                case UIInteractionState.Pressed:
                    OnSelected(instant);
                    break;
                case UIInteractionState.Clicked:
                    if(_targetElement.IsSelectable == false)
                    {
                        OnSelected(false);
                    }
                    break;
            }
        }

        private void OnSelected(bool instant)
        {
            if(instant == false && Time.time - _selectCooldownExpireTime > CooldownBetweenPlays)
            {
                AudioModule.Instance.PlayCueOneShot(_selectSound, 1f, AudioChannel.UI);
                _selectCooldownExpireTime = Time.time + CooldownBetweenPlays;
                _highlightCooldownExpireTime = Time.time + CooldownBetweenPlays;
            }
        }

        private void OnHighlighted(bool instant)
        {
            if (instant == false && Time.time - _highlightCooldownExpireTime > CooldownBetweenPlays)
            {
                AudioModule.Instance.PlayCueOneShot(_highlightSound, 1f, AudioChannel.UI);
                _highlightCooldownExpireTime = Time.time + CooldownBetweenPlays;
            }
        }
    }

    [Serializable]
    public sealed class UIAnimationFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private Animator _animator;

        private static readonly int _normal = Animator.StringToHash("Normal");
        private static readonly int _highlighted = Animator.StringToHash("Highlighted");
        private static readonly int _selected = Animator.StringToHash("Selected");
        private static readonly int _pressed = Animator.StringToHash("Pressed");
        private static readonly int _disabled = Animator.StringToHash("Disabled");

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if (_animator == null)
            {
                _animator = interactiveElement.GetComponent<Animator>();
            }
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            switch (state)
            {
                case UIInteractionState.Normal:
                    _animator.SetTrigger(_normal);
                    break;
                case UIInteractionState.Highlighted:
                    _animator.SetTrigger(_highlighted);
                    break;
                case UIInteractionState.Pressed:
                    _animator.SetTrigger(_pressed);
                    break;
                case UIInteractionState.Selected:
                    _animator.SetTrigger(_selected);
                    break;
                case UIInteractionState.Disabled:
                    _animator.SetTrigger(_disabled);
                    break;
            }
        }
    }

    [Serializable]
    public sealed class UIScaleFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private RectTransform _target;

        [SerializeField, Range(0.1f, 5f)]
        private float _normalScale = 1f;

        [SerializeField, Range(0.1f, 5f)]
        private float _highlightedScale = 1.05f;

        [SerializeField, Range(0.1f, 5f)]
        private float _pressedScale = 0.95f;

        [SerializeField, Range(5f, 40f)]
        private float _transitionSpeed = 10f;

        private Tween<Vector3> _scaleTween;

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if(_target == null)
            {
                interactiveElement.GetComponent<RectTransform>();
            }
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            _scaleTween?.Stop();

            _scaleTween = _target.TweenScale(GetTargetScale(state), 10f)
                .SetEase(Motion.Ease.Linear)
                .SetSpeedMultiplier(_transitionSpeed);
        }

        private Vector3 GetTargetScale(UIInteractionState state)
        {
            return state switch
            {
                UIInteractionState.Normal => _target.localScale * _normalScale,
                UIInteractionState.Highlighted => _target.localScale * _highlightedScale,
                UIInteractionState.Pressed => _target.localScale * _pressedScale,
                _ => _target.localScale
            };
        }
    }

    [Serializable]
    public sealed class UITextFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private TextMeshProUGUI _text;

        [SerializeField, Range(0f, 120f)]
        private int _selectedFontize = 16;

        [SerializeField]
        private Color _selectedTextColor = Color.white;

        [SerializeField]
        private FontStyles _selectedFontStyle = FontStyles.Bold;

        private float _originalFontSize;
        private Color _originalTextColor;
        private FontStyles _originalFontStyle;

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if (_text == null)
            {
                _text = interactiveElement.GetComponentInDirectChildren<TextMeshProUGUI>(includeSelf: true);
            }

            _originalFontSize = _text.fontSize;
            _originalFontStyle = _text.fontStyle;
            _originalTextColor = _text.color;
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            switch(state)
            {
                case UIInteractionState.Selected:
                    ChangeTextFields();
                    break;
                case UIInteractionState.Deselected:
                    RevertChanges();
                    break;
            }
        }

        private void ChangeTextFields()
        {
            _text.fontSize = _selectedFontize;
            _text.fontStyle = _selectedFontStyle;
            _text.color = _selectedTextColor;
        }

        private void RevertChanges()
        {
            _text.fontSize = _originalFontSize;
            _text.fontStyle = _originalFontStyle;
            _text.color = _originalTextColor;
        }
    }

    [Serializable]
    public sealed class UIEventFeedback : IUIVisualFeedback
    {
        [SerializeField]
        [ReorderableList, IgnoreParent]
        private FeedbackEventArgs[] _events = Array.Empty<FeedbackEventArgs>();

        public void Initialize(InteractiveUIElementBase interactiveElement) { }

        public void ExecuteFeedback(UIInteractionState state, bool instant) => InvokeEventsWith(state);

        private void InvokeEventsWith(UIInteractionState trigger)
        {
            foreach(FeedbackEventArgs eventArgs in _events)
            {
                if(eventArgs.Trigger == trigger)
                {
                    eventArgs.Event?.Invoke();
                }
            }
        }

        [Serializable]
        private struct FeedbackEventArgs
        {
            public UIInteractionState Trigger;
            public UnityEvent Event;
        }
    }

    [Serializable]
    public sealed class UISpriteSwapFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private Image _target;

        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _highlightedSprite;

        [SerializeField]
        private Sprite _selectedSprite;

        [SerializeField]
        private Sprite _pressedSprite;

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if (_target == null)
            {
                _target = interactiveElement.GetComponent<Image>();
            }
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            var (targetSprite, shouldUpdate) = GetTargetSprite(state);
            if(shouldUpdate)
            {
                _target.sprite = targetSprite;
            }
        }

        (Sprite targetSprite, bool shouldChange) GetTargetSprite(UIInteractionState state)
        {
            return state switch
            {
                UIInteractionState.Normal => (_normalSprite, true),
                UIInteractionState.Highlighted => (_highlightedSprite, true),
                UIInteractionState.Pressed => (_pressedSprite, true),
                UIInteractionState.Selected => (_selectedSprite, true),
                _ => (null, false)
            };
        }
    }

    [Serializable]
    public sealed class UIImageFeedback : IUIVisualFeedback
    {
        [SerializeField, NotNull]
        private Image _image;

        [SerializeField]
        private Color _selectedColor = Color.white;

        [SerializeField]
        private Color _highlightedColor = Color.white;

        public void Initialize(InteractiveUIElementBase interactiveElement)
        {
            if(_image == null)
            {
                _image = interactiveElement.GetComponent<Image>();
            }
        }

        public void ExecuteFeedback(UIInteractionState state, bool instant)
        {
            switch (state)
            {
                case UIInteractionState.Normal:
                    _image.enabled = false;
                    break;
                case UIInteractionState.Highlighted:
                    _image.color = _highlightedColor;
                    _image.enabled = true;
                    break;
                case UIInteractionState.Selected:
                    _image.color = _selectedColor;
                    _image.enabled = true;
                    break;
            }
        }
    }
}
using Nexora.Audio;
using System;
using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Base panel class, handles general functionalities of Activate/Deactivate
    /// and setting Visible/Invisible along with layer information of the panel through <see cref="ActivePanelStack"/>.
    /// Plays sounds when showing/hiding the panel, and has a field for the optional animation logic.
    /// Might derive <see cref="OnActiveStateChangedInternal(bool)"/> and <see cref="OnVisibilityChangedInternal(bool, bool)"/> 
    /// to extend the functionality in derived classes.
    /// </summary>
    public abstract class PanelBase : MonoBehaviour
    {
        [Header("Panel Settings")]

        [Tooltip("Layer this panel belongs to.")]
        [SerializeField, Range(-1, ActivePanelStack.MaxLayerCount - 1)]
        private int _panelLayer = ActivePanelStack.UndefinedLayer;

        [Tooltip("Shows the panel upon GameObject Start.")]
        [SerializeField]
        private bool _autoShow = false;

        [Tooltip("Does this reacts to cancel input? (e.g close upon Escape key)")]
        [SerializeField]
        private bool _handlesCancelInput = false;

        [Header("Audio")]
        [SerializeField]
        private AudioCue _showAudio = new(null);

        [SerializeField]
        private AudioCue _hideAudio = new(null);

        [SerializeReference, ReferencePicker(typeof(IPanelAnimationStrategy))]
        private IPanelAnimationStrategy _animation;

        protected bool _isDestroyed = false;
        protected bool _isVisible = false;
        protected bool _isActive = false;

        public bool IsModal => _panelLayer != ActivePanelStack.UndefinedLayer;

        /// <summary>
        /// Is the panel currently in the active panel stack.
        /// </summary>
        /// <remarks>
        /// It might not be visible at the moment, because of being hidden
        /// when another panel is activated. But it is still in the stack.
        /// </remarks>
        public bool IsActive => _isActive;

        /// <summary>
        /// Is the panel currently being visible in the scene, really rendering.
        /// It means that the panel is either at the top of the panel stack of its layer
        /// or it is not a modal panel.
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Denotes the panel layer this panel resides in.
        /// </summary>
        public int PanelLayer => _panelLayer;

        /// <summary>
        /// Does this panel react to cancel input (like Escape button closing a panel)
        /// </summary>
        public bool HandlesCancelInput => _handlesCancelInput;

        public event Action<bool> ActiveStateChanged;
        public event Action<bool> VisibilityChanged;

        protected virtual void Awake()
        {
            _animation ??= NullAnimationStrategy.Null;
            _animation.Initialize(this);
        }

        protected virtual void Start()
        {
            if (_autoShow)
            {
                Activate();
            }
        }

        public virtual void Activate() => SetActiveState(true);
        public virtual void Deactivate() => SetActiveState(false);

        private void SetActiveState(bool active)
        {
            // If the current panel is not visible we process
            // the request, because it might be still in the stack
            // and we need to move it to on top of the stack
            // or remove from stack altogether
            if(_isActive == active && _isVisible == false)
            {
                return;
            }

            bool wasActive = _isActive;
            _isActive = active;

            if(active && ActivePanelStack.AddPanel(this))
            {
                SetVisibility(true);
            }
            else if(ActivePanelStack.RemovePanel(this))
            {
                SetVisibility(false);
            }

            if(_isActive != wasActive)
            {
                OnActiveStateChangedInternal(_isActive);
                ActiveStateChanged?.Invoke(_isActive);
            }
        }

        protected virtual void OnActiveStateChangedInternal(bool active) { }  

        public virtual void SetVisibility(bool visible, bool immediate = false)
        {
            if(_isVisible == visible || _isDestroyed || UnityUtils.IsQuitting)
            {
                return;
            }

            OnVisibilityChangedInternal(visible, immediate);
            _isVisible = visible;
            VisibilityChanged?.Invoke(visible);

            PlayAnimation();
            PlayAudio();

            return;

            void PlayAnimation()
            {
                if(visible)
                {
                    _animation.ShowAnimated();
                }
                else
                {
                    _animation.HideAnimated();
                }
            }

            void PlayAudio()
            {
                AudioCue audioCue = visible ? _showAudio : _hideAudio;
                if (audioCue.Clip != null)
                {
                    AudioModule.Instance.PlayCueOneShot(audioCue, 1f, AudioChannel.UI);
                }
            }
        }

        protected virtual void OnVisibilityChangedInternal(bool visible, bool immediate) { }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;

            if(_isActive)
            {
                SetActiveState(false);
            }

            _animation?.Cleanup();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _animation?.Validate(this);
        }

        protected virtual void Reset()
        {
            _animation?.Validate(this);
        }
#endif
    }

    /// <summary>
    /// If any of the derived panels need a special functionality when activating/deactivating
    /// panels, then use this option.
    /// </summary>
    public sealed class PanelShowOptions
    {
        public bool ShowImmediate { get; }
        public bool ShowOverCurrent { get; }

        public PanelShowOptions(bool showImmediate = false, bool showOverCurrent = false)
        {
            ShowImmediate = showImmediate;
            ShowOverCurrent = showOverCurrent;
        }

        public static readonly PanelShowOptions Default = new();
        public static readonly PanelShowOptions Immediate = new(showImmediate: true);
        public static readonly PanelShowOptions FromStack = new(showImmediate: false);
    }

    /// <inheritdoc cref="PanelShowOptions"/>
    public sealed class PanelHideOptions
    {
        public bool HideImmediate { get; private set; } = false;
        public bool HideStackBelow { get; private set; } = false;

        public PanelHideOptions(bool hideImmediate = false, bool hideStackBelow = false)
        {
            HideImmediate = hideImmediate;
            HideStackBelow = hideStackBelow;
        }

        public static readonly PanelHideOptions Default = new();
        public static readonly PanelHideOptions Immediate = new(hideImmediate: true);
        public static readonly PanelHideOptions ByStack = new(hideImmediate: false);
    }
}

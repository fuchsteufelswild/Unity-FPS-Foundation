using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IAttachmentSelector
    {
        /// <summary>
        /// Current attachment attached.
        /// </summary>
        GunAttachment CurrentAttachment { get; }

        /// <summary>
        /// Called when attached attachment changes.
        /// </summary>
        event UnityAction<GunAttachment> AttachmentChanged;

        /// <summary>
        /// Toggles the next available attachment.
        /// </summary>
        void ToggleNextAttachment();

        /// <summary>
        /// Returns if the attachment selector selects attachment for <paramref name="targetPropertyID"/> (e.g 'Sight Scope').
        /// </summary>
        bool IsTarget(int targetPropertyID);
    }

    public abstract class AttachmentSelector : 
        MonoBehaviour,
        IAttachmentSelector
    {
        [SerializeField]
        private AttachmentFeedbackPlayer _feedbackPlayer;

        protected IHandheldItem _handheldItem;
        protected IAttachmentConfigurationProvider _configurationProvider;

        public abstract GunAttachment CurrentAttachment { get; }

        public event UnityAction<GunAttachment> AttachmentChanged;

        public abstract void ToggleNextAttachment();

        protected virtual void Awake()
        {
            _handheldItem = GetComponentInParent<IHandheldItem>();

            if (_handheldItem == null)
            {
                Debug.LogError("[AttachmentSelector]: Must have a handheld item for attachment handler to work.");
                return;
            }

            _feedbackPlayer.Initialize(_handheldItem.Handheld);
            InitializeConfigurationProvider();
            _configurationProvider?.Initialize(_handheldItem);
        }

        protected abstract void InitializeConfigurationProvider();

        protected virtual void OnAttachmentChanged(GunAttachment newMode)
        {
            _feedbackPlayer?.PlayModeChangeEffects();
            AttachmentChanged?.Invoke(newMode);
        }

        public abstract bool IsTarget(int targetPropertyID);
    }
}
using System.Diagnostics;
using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Panel that works with <see cref="UnityEngine.CanvasGroup"/>, controls the 
    /// fields to set the properties upon show/hide.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu(ComponentMenuPaths.BaseUI + nameof(CanvasPanel))]
    public class CanvasPanel : PanelBase
    {
        [Header("Canvas Settings")]
        [Tooltip("Controls CanvasGroup's interactable state.")]
        [SerializeField]
        private bool _controlInteractable = true;

        [Tooltip("Controls CanvasGroup's blocks raycasts state.")]
        [SerializeField]
        private bool _controlBlocksRaycasts = true;

        [Tooltip("Controls CanvasGroup's alpha.")]
        [SerializeField]
        private bool _controlAlpha = true;

        [SerializeField, HideInInspector]
        protected CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;

        protected override void Awake()
        {
            base.Awake();

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            SetCanvasGroupState(false, true);
        }

        protected override void OnVisibilityChangedInternal(bool visible, bool immediate)
            => SetCanvasGroupState(visible, immediate);

        private void SetCanvasGroupState(bool visible, bool immediate)
        {
            if (_controlInteractable)
            {
                _canvasGroup.interactable = visible;
            }

            if (_controlBlocksRaycasts)
            {
                _canvasGroup.blocksRaycasts = visible;
            }

            if (_controlAlpha)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            }
        }
#endif
    }
}
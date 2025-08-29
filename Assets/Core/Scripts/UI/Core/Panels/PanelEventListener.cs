using UnityEngine;
using UnityEngine.Events;

namespace Nexora.UI
{
    /// <summary>
    /// Unity component to add hooks to <see cref="PanelBase"/> events.
    /// </summary>
    [RequireComponent(typeof(PanelBase))]
    public class PanelEventListener : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _onPanelActivated;

        [SerializeField]
        private UnityEvent _onPanelDeactivated;

        [SerializeField]
        private UnityEvent _onPanelHidden;

        protected virtual void Start()
        {
            var panel = GetComponent<PanelBase>();
            panel.ActiveStateChanged += OnPanelActiveStateChange;
            panel.VisibilityChanged += OnPanelVisibilityChanged;
        }

        private void OnPanelActiveStateChange(bool active)
        {
            if(active)
            { 
                _onPanelActivated?.Invoke();
            }
            else
            {
                _onPanelDeactivated?.Invoke();
            }
        }

        private void OnPanelVisibilityChanged(bool visible)
        {
            if(visible == false)
            {
                _onPanelHidden?.Invoke();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.GetOrAddComponent<PanelBase>();
        }
#endif
    }
}
using UnityEngine;

namespace Nexora.UI
{
    [RequireComponent(typeof(InteractiveUIElement))]
    public abstract class DataSlot<T> : 
        MonoBehaviour,
        IDataView<T>
        where T : class
    {
        [Tooltip("The data to populate this slot.")]
        [SerializeField]
        private T _data;

        [SerializeField, HideInInspector]
        private InteractiveUIElement _interactiveComponent;

        public InteractiveUIElement InteractiveComponent => _interactiveComponent;

        public T Data => _data;

        void Start()
        {
            FindInteractiveComponent();
            if(_data != null)
            {
                UpdateView(_data);
            }
        }

        protected abstract void UpdateView(T data);

        public void SetData(T data)
        {
            if(_data == data)
            {
                return;
            }

            _data = data;
            UpdateView(_data);
        }

        public void ClearData() => SetData(null);
        public void Refresh() => UpdateView(_data);

        private void FindInteractiveComponent() => 
            _interactiveComponent = gameObject.GetComponent<InteractiveUIElement>();

#if UNITY_EDITOR
        private void OnValidate() => FindInteractiveComponent();
#endif
    }
}
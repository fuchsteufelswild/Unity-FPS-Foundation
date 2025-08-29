using UnityEngine;

namespace Nexora.Options
{
    [CreateAssetMenu(menuName = CreateAssetMenuPath + "Auto-Save Options", fileName = nameof(AutoSaveOptions))]
    public sealed partial class AutoSaveOptions : 
        Options<AutoSaveOptions>
    {
        public const float MaxAutoSaveInterval = 1000f;

        [SerializeField]
        private Option<bool> _autoSaveEnabled = new();
        [SerializeField]
        private Option<float> _autoSaveInterval = new(300f);
        
        public Option<bool> AutoSaveEnabled => _autoSaveEnabled;
        public Option<float> AutoSaveInterval => _autoSaveInterval;

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                if (Application.isPlaying == false)
                {
                    _autoSaveInterval.SetValue(Mathf.Clamp(_autoSaveInterval.Value, 1f, MaxAutoSaveInterval));
                }
            });
        }
#endif
    }
}
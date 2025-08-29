using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.PostProcessing
{
    /// <summary>
    /// Global volume use with <see cref="PostFxModule"/>. 
    /// Note that general module can only work with one <see cref="Volume"/> in any given time.
    /// Think of this volume as the main volume of your game, like a player's eyes, or in an RTS game
    /// main camera looking through the map.
    /// <br></br><b>Only put one instance in any scene so to avoid collisions.</b>
    /// </summary>
    [RequireComponent(typeof(Volume))]
    [DisallowMultipleComponent]
    public class GlobalPostProcessVolume : MonoBehaviour
    {
        private Volume _globalVolume;

        private void Awake() => GetComponent<Volume>();

        private void OnEnable() => PostFxModule.Instance.ActiveVolume = _globalVolume;

        private void OnDisable()
        {
            if(PostFxModule.Instance.ActiveVolume == _globalVolume)
            {
                PostFxModule.Instance.ActiveVolume = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, 
                () => gameObject.GetOrAddComponent<Volume>().isGlobal = true);
        }
#endif
    }
}
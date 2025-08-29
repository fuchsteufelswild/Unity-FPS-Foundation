using System;
using UnityEngine;

#if UNITY_PIPELINE_URP
using Vignette = UnityEngine.Rendering.Universal.Vignette;
#elif UNITY_PIPELINE_HDRP
using Vignette = UnityEngine.Rendering.HighDefinition.Vignette;
#else
using Vignette = UnityEngine.Rendering.Universal.Vignette;
#endif

namespace Nexora.PostProcessing
{
    [Serializable]
    public class VignetteAnimation : PostFxAnimation<Vignette>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _intensity = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, Vignette vignette)
            => list.Add(_intensity.SetParameter(vignette.intensity));
    }
}

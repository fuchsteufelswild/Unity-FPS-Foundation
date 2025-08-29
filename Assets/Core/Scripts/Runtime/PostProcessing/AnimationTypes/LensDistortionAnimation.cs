using System;
using UnityEngine;

#if UNITY_PIPELINE_URP
using LensDistortion = UnityEngine.Rendering.Universal.LensDistortion;
#elif UNITY_PIPELINE_HDRP
using LensDistortion = UnityEngine.Rendering.HighDefinition.LensDistortion;
#else
using LensDistortion = UnityEngine.Rendering.Universal.LensDistortion;
#endif

namespace Nexora.PostProcessing
{
    [Serializable]
    public class LensDistortionAnimation : PostFxAnimation<LensDistortion>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _intensity = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, LensDistortion lensDistortion)
            => list.Add(_intensity.SetParameter(lensDistortion.intensity));
    }
}

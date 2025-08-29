using System;
using UnityEngine;

#if UNITY_PIPELINE_URP
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
#elif UNITY_PIPELINE_HDRP
using ChromaticAberration = UnityEngine.Rendering.HighDefinition.ChromaticAberration;
#else
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
#endif

namespace Nexora.PostProcessing
{
    [Serializable]
    public class ChromaticAberrationAnimation : PostFxAnimation<ChromaticAberration>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _intensity = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ChromaticAberration chromaticAberration)
            => list.Add(_intensity.SetParameter(chromaticAberration.intensity));
    }
}

using System;
using UnityEngine;

#if UNITY_PIPELINE_URP
using ColorGrading = UnityEngine.Rendering.Universal.ColorAdjustments;
#elif UNITY_PIPELINE_HDRP
using ColorGrading = UnityEngine.Rendering.HighDefinition.ColorAdjustments;
#else
using ColorGrading = UnityEngine.Rendering.Universal.ColorAdjustments;
#endif

namespace Nexora.PostProcessing
{
    [Serializable]
    public class CGPostExposureAnimation : PostFxAnimation<ColorGrading>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _postExposure = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ColorGrading colorGrading)
            => list.Add(_postExposure.SetParameter(colorGrading.postExposure));
    }

    [Serializable]
    public class CGSaturationAnimation : PostFxAnimation<ColorGrading>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _saturation = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ColorGrading colorGrading)
            => list.Add(_saturation.SetParameter(colorGrading.saturation));
    }

    [Serializable]
    public class CGContrastAnimation : PostFxAnimation<ColorGrading>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _contrast = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ColorGrading colorGrading)
            => list.Add(_contrast.SetParameter(colorGrading.contrast));
    }

    [Serializable]
    public class CGHueShiftAnimation : PostFxAnimation<ColorGrading>
    {
        [SerializeField]
        private PostFxParameterAnimation<float> _hueShift = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ColorGrading colorGrading)
            => list.Add(_hueShift.SetParameter(colorGrading.hueShift));
    }

    [Serializable]
    public class CGColorFilterAnimation : PostFxAnimation<ColorGrading>
    {
        [SerializeField]
        private PostFxParameterAnimation<Color> _colorFilter = new(0f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, ColorGrading colorGrading)
            => list.Add(_colorFilter.SetParameter(colorGrading.colorFilter));
    }
}

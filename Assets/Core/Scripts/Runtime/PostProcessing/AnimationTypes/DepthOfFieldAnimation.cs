using System;
using UnityEngine;

#if UNITY_PIPELINE_URP
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;
#elif UNITY_PIPELINE_HDRP
using DepthOfField = UnityEngine.Rendering.HighDefinition.DepthOfField;
#else
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;
#endif

namespace Nexora.PostProcessing
{
    [Serializable]
    public class DepthOfFieldAnimation : PostFxAnimation<DepthOfField>
    {
        // HDRP
        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.HDRP)]
        private PostFxParameterAnimation<float> _nearFocusStart = new(0f, 0f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.HDRP)]
        private PostFxParameterAnimation<float> _nearFocusEnd = new(4f, 4f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.HDRP)]
        private PostFxParameterAnimation<float> _farFocusStart = new(10f, 10f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.HDRP)]
        private PostFxParameterAnimation<float> _farFocusEnd = new(20f, 0f);

        // URP
        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.URP)]
        private PostFxParameterAnimation<float> _gaussianStart = new(0f, 0f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.URP)]
        private PostFxParameterAnimation<float> _gaussianEnd = new(4f, 4f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.URP)]
        private PostFxParameterAnimation<float> _gaussianMaxRadius = new(10f, 10f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.URP)]
        private PostFxParameterAnimation<float> _focusDistance = new(20f, 0f);

        [SerializeField, VisibleInRenderPipeline(VisibleInRenderPipelineAttribute.RenderPipelineType.URP)]
        private PostFxParameterAnimation<float> _aperture = new(20f, 0f);

        protected override void FillParameterAnimations(PostFxParameterAnimationList list, DepthOfField dof)
        {
#if UNITY_PIPELINE_HDRP
            list.Add(_nearFocusStart.SetParameter(dof.nearFocusStart));
            list.Add(_nearFocusEnd.SetParameter(dof.nearFocusEnd));
            list.Add(_farFocusStart.SetParameter(dof.farFocusStart));
            list.Add(_farFocusEnd.SetParameter(dof.farFocusEnd));
#elif UNITY_PIPELINE_URP
            list.Add(_gaussianStart.SetParameter(dof.gaussianStart));
            list.Add(_gaussianEnd.SetParameter(dof.gaussianEnd));
            list.Add(_gaussianMaxRadius.SetParameter(dof.gaussianMaxRadius));
            list.Add(_focusDistance.SetParameter(dof.focusDistance));
            list.Add(_aperture.SetParameter(dof.aperture));
#else
            list.Add(_gaussianStart.SetParameter(dof.gaussianStart));
            list.Add(_gaussianEnd.SetParameter(dof.gaussianEnd));
            list.Add(_gaussianMaxRadius.SetParameter(dof.gaussianMaxRadius));
            list.Add(_focusDistance.SetParameter(dof.focusDistance));
            list.Add(_aperture.SetParameter(dof.aperture));
#endif
        }
    }
}

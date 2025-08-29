using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine.Rendering;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(VisibleInRenderPipelineAttribute))]
    public class VisibleInRenderPipelineDrawer : 
        ToolboxConditionDrawer<VisibleInRenderPipelineAttribute>
    {
        protected override PropertyCondition OnGuiValidateSafe(
            SerializedProperty property, 
            VisibleInRenderPipelineAttribute attribute)
        {
            var currentRenderPipelineType = GetCurrentRenderPipelineType();

            return (currentRenderPipelineType & attribute.PipelineType) == currentRenderPipelineType
                ? PropertyCondition.Valid 
                : PropertyCondition.NonValid;
        }

        private VisibleInRenderPipelineAttribute.RenderPipelineType GetCurrentRenderPipelineType()
        {
            if(GraphicsSettings.defaultRenderPipeline != null)
            {
                string currentSRPName = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
                if(currentSRPName.Contains("HD"))
                {
                    return VisibleInRenderPipelineAttribute.RenderPipelineType.HDRP;
                }
                if(currentSRPName.Contains("Universal") || currentSRPName.Contains("URP"))
                {
                    return VisibleInRenderPipelineAttribute.RenderPipelineType.URP;
                }
            }

            return VisibleInRenderPipelineAttribute.RenderPipelineType.BUILT_IN;
        }
    }
}

using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class VisibleInRenderPipelineAttribute : ToolboxConditionAttribute
    {
        [Flags]
        public enum RenderPipelineType
        {
            BUILT_IN = 1 << 0,
            URP = 1 << 1,
            HDRP = 1 << 2,
            URP_HDRP = URP | HDRP
        }

        public RenderPipelineType PipelineType { get; }

        public VisibleInRenderPipelineAttribute(RenderPipelineType pipelineType)
            => PipelineType = pipelineType;

    }
}

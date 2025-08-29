using Nexora.Audio;
using System;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public sealed class AudioSequenceDrawer : ToolboxTargetTypeDrawer
    {
        private AudioSequence _audioSequence;

        public override Type GetTargetType() => typeof(AudioSequence);

        public override void OnGui(SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
            if (property.isExpanded)
            {
                ToolboxEditorGui.DrawPropertyChildren(property);

                EditorGUILayout.Space();
                if (GUILayout.Button(AudioPreviewManager.PlayButtonContent.WithTooltip("Play Audio Sequence")))
                {
                    _audioSequence = (AudioSequence)property.boxedValue;
                    AudioPreviewManager.PlaySequencePreview(_audioSequence);
                }
            }
            AudioPreviewManager.CleanupInactivePreview();
        }

        public override bool UseForChildren() => false;
    }
}
using Codice.Client.BaseCommands;
using Nexora.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(AudioCue))]
    public sealed class AudioCueDrawer : PropertyDrawer
    {
        /// <summary>
        /// Expand amount after prefix label, to align again.
        /// </summary>
        private const float LabelExpansion = 40f;

        /// <summary>
        /// Width threshold to determine if property label should be written
        /// in long form or short form. E.g "Volume" vs "Vol".
        /// </summary>
        private const float PropertyLabelWidthThreshold = 65f;

        private const float MaxDelay = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (property.PropertyScope(position, label))
            {
                var (clipRect, volumeRect, volumeLabelRect, delayRect, delayLabelRect, playButtonRect) 
                    = CalculateElementRects(CalculateContentRect(position, label));

                var (clipProperty, volumeProperty, delayProperty) = GetPropertyReferences(property);

                EditorGUIUtils.DrawPropertyWithTooltip(clipRect, clipProperty);

                DrawVolumeField(volumeProperty, volumeRect, volumeLabelRect);
                DrawDelayField(delayProperty, delayRect, delayLabelRect);
                DrawPlayButton(playButtonRect, clipProperty, volumeProperty);
            }
        }

        /// <summary>
        /// Calculates the position where the properties will be started to be drawn.
        /// </summary>
        private Rect CalculateContentRect(Rect position, GUIContent label)
        {
            Rect contentPosition = 
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if(position != contentPosition)
            {
                contentPosition = RectUtils.AlignFieldWithHeader(contentPosition, LabelExpansion);
            }

            return contentPosition;
        }

        private (Rect clipRect, Rect volumeRect, Rect volumeLabelRect, Rect delayRect, Rect delayLabelRect, Rect playButtonRect)
            CalculateElementRects(Rect contentPosition)
        {
            const float clipWidthRatio = 0.4f;
            const float playButtonWidth = 30f;
            const float elementPadding = 3f;
            const float labelOffset = 2f;

            const int elementCount = 3;

            float totalWidth = contentPosition.width;
            float clipWidth = totalWidth * clipWidthRatio;
            float volumeAndDelayWidth = totalWidth - clipWidth - playButtonWidth - (elementCount * elementPadding);
            volumeAndDelayWidth *= 0.5f; // Half for each

            var clipRect = contentPosition.WithWidth(clipWidth);
            var volumeRect = RectUtils.GetNextColumn(clipRect, elementPadding)
                                      .WithWidth(volumeAndDelayWidth);

            var delayRect = RectUtils.GetNextColumn(volumeRect, elementPadding);

            var volumeLabelRect = volumeRect.ShiftLeft(labelOffset);
            var delayLabelRect = delayRect.ShiftLeft(labelOffset);

            var playButtonRect = new Rect(
                contentPosition.x + contentPosition.width - playButtonWidth,
                contentPosition.y,
                playButtonWidth,
                contentPosition.height
                );

            return (clipRect, volumeRect, volumeLabelRect, delayRect, delayLabelRect, playButtonRect);
        }


        private (SerializedProperty, SerializedProperty, SerializedProperty)
            GetPropertyReferences(SerializedProperty property)
        {
            return (
                property.FindPropertyRelative("Clip"),
                property.FindPropertyRelative("Volume"),
                property.FindPropertyRelative("Delay"));
        }

        private static void DrawVolumeField(SerializedProperty volumeProperty, Rect volumeRect, Rect volumeLabelRect)
        {
            EditorGUIUtils.DrawPropertyWithTooltip(volumeRect, volumeProperty);
            volumeProperty.floatValue = Mathf.Clamp01(volumeProperty.floatValue);

            string labelText = volumeLabelRect.width > PropertyLabelWidthThreshold ? "Volume" : "Vol";
            EditorGUI.LabelField(volumeLabelRect, labelText, EditorGUIStyles.MiniLabelSuffix);
        }

        private static void DrawDelayField(SerializedProperty delayProperty, Rect delayRect, Rect delayLabelRect)
        {
            EditorGUIUtils.DrawPropertyWithTooltip(delayRect, delayProperty);
            delayProperty.floatValue = Mathf.Clamp(delayProperty.floatValue, 0f, MaxDelay);

            string labelText = delayLabelRect.width > PropertyLabelWidthThreshold ? "Delay" : "Del";
            EditorGUI.LabelField(delayLabelRect, labelText, EditorGUIStyles.MiniLabelSuffix);
        }

        private static void DrawPlayButton(Rect playButtonRect, SerializedProperty clipProperty, SerializedProperty volumeProperty)
        {
            if (GUI.Button(playButtonRect, AudioPreviewManager.PlayButtonContent.WithTooltip("Play Audio Preview")))
            {
                HandlePlayButtonClick(clipProperty, volumeProperty);
            }
        }

        private static void HandlePlayButtonClick(SerializedProperty clipProperty, SerializedProperty volumeProperty)
        {
            var audioClip = clipProperty.objectReferenceValue as AudioResource;
            float volume = volumeProperty.floatValue;

            if(audioClip != null)
            {
                AudioPreviewManager.PlaySinglePreview(audioClip, volume);
            }
            else
            {
                Debug.LogWarning("No audio clip assigned to preview.");
            }
        }
    }
}

using Codice.CM.Common;
using System;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public enum EditorInfoType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>
    /// Provides methods to draw commonly used functionalities.
    /// </summary>
    public static partial class EditorGUIUtils
    {
        public const float DefaultIconSize = 16f;
        public const float DefaultIconPadding = 4f;

        /// <summary>
        /// Proportion for field name to second element. It is determining
        /// percentage of the space the title takes, and the remaining will
        /// be for the second element.
        /// </summary>
        /// <remarks>
        /// Value is between 0-1, 0 means 0% and 1 means 100%.
        /// For the value of 0.4f, 40% of the space is alloted for the name 
        /// of the field and the remaining 60% is for the second element, similar
        /// to default Unity selector.
        /// </remarks>
        public const float DefaultColumnSplitRatio = 0.38f;

        /// <summary>
        /// Default padding of the selector button, like Unity default object selector.
        /// </summary>
        public const float DefaultSelectorPadding = 10f;

        /// <summary>
        /// How much a field(sprite, int) is tabbed from the edge.
        /// </summary>
        public const float DefaultFieldTabbing = 5f;

        /// <summary>
        /// Height of the default Unity selector.
        /// </summary>
        public const float DefaultUnitySelectorHeight = 18f;

        /// <summary>
        /// Padding for the area of the box when it contains something else.
        /// </summary>
        public const float ContainerBoxPadding = 2f;

        /// <summary>
        /// Size of the default square texture render.
        /// </summary>
        public const float DefaultTextureSize = 64f;


        public static readonly float SingleLineHeight = 
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        /// <summary>
        /// Draws an overlay of color, and shows tooltip of <paramref name="message"/>
        /// when hovered.
        /// </summary>
        public static void DrawInfoOverlay(Rect position, EditorInfoType infoType, string message)
        {
            DrawBackground(position, BackgroundColor(), EditorGUIUtility.singleLineHeight);
            var iconRect = RectUtils.CalculateRectForIcon(position, DefaultIconSize, DefaultIconPadding, UIAnchor.MiddleRight);
            GUI.DrawTexture(iconRect, Icon());

            if (Event.current.type == EventType.Repaint
            && position.Contains(Event.current.mousePosition))
            {
                DrawHoverTooltip(iconRect, message);
            }

            return;

            Color BackgroundColor()
            {
                return infoType switch
                {
                    EditorInfoType.Info => EditorGUIStyles.BlueInfoColor,
                    EditorInfoType.Warning => EditorGUIStyles.YellowWarningColor,
                    EditorInfoType.Error => EditorGUIStyles.RedErrorColor,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            Texture Icon()
            {
                return infoType switch
                {
                    EditorInfoType.Info => EditorIcons.BlueInfoIcon,
                    EditorInfoType.Warning => EditorIcons.YellowWarningIcon,
                    EditorInfoType.Error => EditorIcons.RedErrorIcon,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public static void DrawBackground(Rect position, Color backgroundColor, float height)
        {
            using (ColorScope scope = new ColorScope(backgroundColor))
            {
                position.height = height;
                GUI.Label(position, GUIContent.none, EditorGUIStyles.TextField);
            }
        }

        public static void DrawBackgroundRect(Rect position, Color color, float height)
        {
            position.height = height;
            EditorGUI.DrawRect(position, color);
        }

        public static void DrawHelpBoxBackground(Rect position, float height)
        {
            position.height = height;
            EditorGUI.LabelField(position, GUIContent.none, EditorStyles.helpBox);
        }

        public static void DrawHoverTooltip(Rect position, string message)
        {
            var tipRect = new Rect(position.x - 150, position.y + 20, 150, 40);
            GUI.Label(tipRect, message, EditorGUIStyles.TipBox);
        }

        /// <summary>
        /// Draws like Unity's default objects selector, but object selector via click 
        /// on the field and shows the field object.
        /// </summary>
        /// <remarks>
        /// In the case of type mismatch <typeparamref name="T"/> or a value type passed,
        /// it just draws plain Unity field.
        /// </remarks>
        /// <typeparam name="T">Type of the object, selector will use it.</typeparam>
        /// <param name="titleHeight">Title(selector) height of the field.</param>
        public static void DrawObjectSelectorWithTitle<T>(
            Rect position, 
            SerializedProperty property, 
            float titleHeight,
            ref int controlID)
            where T : UnityEngine.Object
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference
             || property.objectReferenceValue is not T obj)
            {
                EditorGUI.PropertyField(position, property);
                return;
            }

            var icon = GetIcon();

            Rect textureRect = new Rect(
                position.x,
                position.y + titleHeight,
                position.width,
                position.height);

            DrawTextureInBox(textureRect, icon, DefaultTextureSize, ContainerBoxPadding);

            if (GUI.Button(position, GUIContent.none, EditorStyles.toolbarButton))
            {
                controlID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<T>(obj, allowSceneObjects: false, "", controlID);
            }

            DrawTitle();

            // Handle object picker event
            if (Event.current.type == EventType.ExecuteCommand &&
                EditorGUIUtility.GetObjectPickerControlID() == controlID &&
                Event.current.commandName == "ObjectSelectorUpdated")
            {
                property.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                property.serializedObject.ApplyModifiedProperties();
                Event.current.Use();
            }

            return;

            void DrawTitle()
            {
                Rect titleRect = new Rect(position.x, position.y, position.width, titleHeight);
                EditorGUI.DrawRect(titleRect, Color.black.WithAlpha(0));

                var obj = property.objectReferenceValue as T;

                string objName = obj != null ? obj.name : "None";
                Rect nameRect = new Rect(
                    titleRect.x + DefaultFieldTabbing,
                    titleRect.y,
                    titleRect.width,
                    titleRect.height
                );

                EditorGUI.LabelField(nameRect, objName, EditorStyles.miniLabel);
            }

            Texture GetIcon()
            {
                return obj switch
                {
                    Texture texture => texture,
                    Sprite sprite => sprite.texture,
                    _ => EditorGUIUtility.ObjectContent(obj, typeof(T)).image
                };
            }
        }


        /// <summary>
        /// Draws the <paramref name="texture"/> in a box while preserving its
        /// aspect ratio.
        /// </summary>
        public static void DrawTextureInBox(
            Rect position,
            Texture texture,
            float boxSize,
            float boxPadding)
        {
            Rect boxRect = new Rect(
                position.x,
                position.y,
                position.width,
                boxSize + (boxPadding * 2)
            );
            EditorGUI.DrawRect(boxRect, EditorGUIStyles.DefaultBoxBackgroundColor);

            // Draw texture (centered in box)
            if (texture != null)
            {
                Rect textureRect = new Rect(
                    boxRect.x + boxPadding,
                    boxRect.y + boxPadding,
                    boxRect.width - (boxPadding * 2),
                    boxRect.height - (boxPadding * 2)
                );

                // Maintain aspect ratio
                Rect centeredRect = RectUtils.CalculateCenteredRect(textureRect, texture.width, texture.height);
                GUI.DrawTexture(centeredRect, texture, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// Draws selector that is like default Unity's selector.
        /// Difference is, it is not drawing as property field
        /// so that you can draw freely other things you may wanted
        /// without affecting the whole field.
        /// </summary>
        /// <remarks>
        /// Needs to be <see cref="UnityEngine.Object"/> type, 
        /// it is not a directly copy of <see cref="EditorGUI.PropertyField(Rect, SerializedProperty)"/>.
        /// <b>It does not work with value types.</b>
        /// </remarks>
        /// <typeparam name="T">Type of the object</typeparam>
        public static void DrawDefaultUnitySelector<T>(
            Rect position,
            SerializedProperty property,
            float? fieldToSelectorProportion = null,
            float? selectorHeight = null)
            where T : UnityEngine.Object
        {
            fieldToSelectorProportion ??= DefaultColumnSplitRatio;
            selectorHeight ??= DefaultUnitySelectorHeight;

            float titleRectWidth = position.width * fieldToSelectorProportion.Value;
            
            Rect titleRect = new Rect(
                position.x, position.y, titleRectWidth, selectorHeight.Value);
            EditorGUI.LabelField(titleRect, property.displayName, EditorStyles.label);

            float selectorWidth = position.width - titleRectWidth;
            Rect selectorRect = new Rect(
               position.x + titleRectWidth, position.y, selectorWidth, selectorHeight.Value);

            property.objectReferenceValue = EditorGUI.ObjectField(
                selectorRect,
                label: GUIContent.none,
                obj: property.objectReferenceValue,
                typeof(T),
                allowSceneObjects: false);
        }

        /// <summary>
        /// Validates field if it is not <see langword="null"/>.
        /// <br></br>
        /// Draws an error box to inform about a missing field.
        /// </summary>
        /// <typeparam name="T">Type of the field.</typeparam>
        public static bool ValidateField<T>(Rect position, T field, float helpBoxHeightMultiplier = 1f)
            where T : class
        {
            if (field != null)
            {
                return true;
            }

            DrawHelpBox(position, $"No {typeof(T).Name} found.", UnityEditor.MessageType.Error, helpBoxHeightMultiplier);
            return false;
        }

        /// <summary>
        /// Draws help box with given <paramref name="type"/> and with optional
        /// <paramref name="heightMultiplier"/> for extra space.
        /// </summary>
        public static void DrawHelpBox(
            Rect position, 
            string message, 
            UnityEditor.MessageType type, 
            float heightMultiplier = 1f)
        {
            position.height *= heightMultiplier;
            EditorGUI.HelpBox(position, message, type);
        }

        public static void DrawPropertyWithTooltip(
            Rect position,
            SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
            EditorGUI.LabelField(position, GUIContent.none.WithTooltip(property));
        }
    }
}

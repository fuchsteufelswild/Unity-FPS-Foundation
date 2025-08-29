using Nexora.Animation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(AnimatorOverrideProfile))]
    public sealed class AnimatorOverrideProfileDrawer : PropertyDrawer
    {
        private SerializedObject _currentSerializedObject;
        private ReorderableList _clipOverrideList;

        private SerializedProperty _animatorControllerProperty;
        private SerializedProperty _defaultParametersProperty;
        private SerializedProperty _clipOverridesProperty;

        private const float ColumnSplitRatio = 0.5f;
        private const float HeaderOffset = 14f;

        private static readonly float singleParameterHeight = 
            AnimatorParameterDrawer.GetMaximumHeight() + EditorGUIUtility.standardVerticalSpacing;

        private const string OriginalClipFieldName = "Original";
        private const string OverrideClipFieldName = "Override";

        private const float BackgroundBoxPaddingMultiplier = 2.5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (property.PropertyScope(position, label))
            {
                position.height = EditorGUIUtility.singleLineHeight;

                using (new BoxContainerScope(
                    ref position,
                    GetPropertyHeight(property, label),
                    property.displayName,
                    new RectOffset(
                        (int)EditorGUIStyles.BackgroundBoxHorizontalPadding,
                        (int)EditorGUIStyles.BackgroundBoxHorizontalPadding,
                        (int)EditorGUIStyles.BackgroundBoxVerticalPadding,
                        (int)EditorGUIStyles.BackgroundBoxVerticalPadding))
                    )
                {
                    DrawAnimatorControllerField(position, out RuntimeAnimatorController controller);
                    UpdateClipList(controller);

                    RectUtils.MoveToNextLine(ref position);
                    _clipOverrideList.DoList(EditorGUI.IndentedRect(position));

                    if (Application.isPlaying == false)
                    {
                        position.y += _clipOverrideList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, _defaultParametersProperty);
                    }
                }
            }
            
        }

        /// <summary>
        /// Draws the default Unity selector for <see cref="RuntimeAnimatorController"/>.
        /// Populates the clip list if new <paramref name="controller"/> is assigned.
        /// </summary>
        /// <remarks>
        /// <b>This is for if the assigned animator changes.</b>
        /// </remarks>
        /// <param name="controller">The value in the Unity object selector.</param>
        private void DrawAnimatorControllerField(Rect position, out RuntimeAnimatorController controller)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, _animatorControllerProperty);
            controller = _animatorControllerProperty.objectReferenceValue as RuntimeAnimatorController;

            if (EditorGUI.EndChangeCheck())
            {
                if (controller == null)
                {
                    _clipOverridesProperty.arraySize = 0;
                }
                else
                {
                    PopulateClipListFrom(controller);
                }
            }
        }

        /// <summary>
        /// Updates the list of clips using the animations of <paramref name="controller"/>.
        /// </summary>
        private void PopulateClipListFrom(RuntimeAnimatorController controller)
        {
            AnimationClip[] animationClips = controller.animationClips;
            _clipOverridesProperty.arraySize = animationClips.Length;

            for (int i = 0; i < animationClips.Length; i++)
            {
                var clipPairProperty = _clipOverridesProperty.GetArrayElementAtIndex(i);
                var originalClipProperty = clipPairProperty.FindPropertyRelative(OriginalClipFieldName);
                originalClipProperty.objectReferenceValue = animationClips[i];
            }
        }

        /// <summary>
        /// <inheritdoc cref="PopulateClipListFrom(RuntimeAnimatorController)"/> 
        /// If there is a mismatch between <paramref name="controller"/> animation list
        /// and the current property animation list.
        /// </summary>
        /// <remarks>
        /// <b>This is for if an animation is added/removed to the already assigned animator.</b>
        /// </remarks>
        private void UpdateClipList(RuntimeAnimatorController controller)
        {
            if (ShouldUpdate() == false)
            {
                return;
            }

            PopulateClipListFrom(controller);

            return;

            bool ShouldUpdate()
            {
                return controller != null && controller.animationClips.Length != _clipOverridesProperty.arraySize;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnsureInitialized(property);

            return ClipListHeight() + ParametersHeight() +
                EditorGUIStyles.BackgroundBoxVerticalPadding * BackgroundBoxPaddingMultiplier +
                EditorGUIUtils.SingleLineHeight; // For the title

            float ClipListHeight()
            {
                return _clipOverrideList.GetHeight() + EditorGUIUtils.SingleLineHeight;
            }

            float ParametersHeight()
            {
                if (Application.isPlaying)
                {
                    return 0f;
                }

                int parameterCount = _defaultParametersProperty.Copy().CountInProperty();

                // Don't take self into account
                int amountOfParametersIgnore = _defaultParametersProperty.isExpanded ? 2 : 1;
                // Has elements
                if (_defaultParametersProperty.isExpanded && parameterCount > 2)
                {
                    // Parameter count value is like the following for real element count:
                    // 0 -> 2
                    // 1 -> 4
                    // 2 -> 6
                    // 3 -> 8
                    // So we use this calculation to get the real ignore amount
                    amountOfParametersIgnore = parameterCount / 2 + 1;
                }

                float parametersHeight = (parameterCount - amountOfParametersIgnore) * singleParameterHeight;

                // Determines the height of the list's main parts' height.
                float headerHeight = EditorGUIUtility.singleLineHeight;
                float addRemoveSectionHeight = EditorGUIUtility.singleLineHeight * 1.5f;
                float emptyElementHeight = _defaultParametersProperty.isExpanded && parameterCount == 2
                    ? EditorGUIUtility.singleLineHeight
                    : 0;

                float selfHeight = _defaultParametersProperty.isExpanded
                    ? headerHeight + addRemoveSectionHeight + emptyElementHeight
                    : headerHeight;

                return parametersHeight + selfHeight;
            }
        }


        private void EnsureInitialized(SerializedProperty property)
        {
            if(_currentSerializedObject != property.serializedObject)
            {
                InitializeDrawer(property);
                _currentSerializedObject = property.serializedObject;
            }
        }

        private void InitializeDrawer(SerializedProperty property)
        {
            CachePropertyReferences();
            SetupClipOverrideList();

            return;

            void CachePropertyReferences()
            {
                _animatorControllerProperty = property.FindPropertyRelative("_baseController");
                _clipOverridesProperty = property.FindPropertyRelative("_clipOverrides");
                _defaultParametersProperty = property.FindPropertyRelative("_defaultParameters");
            }

            void SetupClipOverrideList()
            {
                _clipOverrideList = new ReorderableList(property.serializedObject, _clipOverridesProperty)
                {
                    draggable = false,
                    displayAdd = false,
                    displayRemove = false,
                    drawElementCallback = DrawClipPairElement,
                    drawHeaderCallback = DrawClipPairHeader,
                    drawNoneElementCallback = DrawEmptyListMessage,
                    elementHeight = EditorGUIUtility.singleLineHeight,
                    onSelectCallback = HandleClipSelection,
                    footerHeight = 0f
                };
            }
        }

        #region AnimatorOverrideProfile.ClipOverridePair
        /// <summary>
        /// Draws the <see cref="AnimatorOverrideProfile.ClipOverridePair"/>.
        /// </summary>
        private void DrawClipPairElement(Rect position, int index, bool isSelected, bool isFocused)
        {
            var clipPairProperty = _clipOverridesProperty.GetArrayElementAtIndex(index);
            var originalClipProperty = clipPairProperty.FindPropertyRelative(OriginalClipFieldName);
            var overrideClipProperty = clipPairProperty.FindPropertyRelative(OverrideClipFieldName);

            var originalClip = originalClipProperty.objectReferenceValue as AnimationClip;
            var overrideClip = overrideClipProperty.objectReferenceValue as AnimationClip;

            DrawOriginalClipField();
            DrawOverrideClipField();

            return;

            void DrawOriginalClipField()
            {
                Rect originalClipRect = RectUtils.GetLeftColumnRect(position, ColumnSplitRatio);
                string originalClipName = originalClip != null ? originalClip.name : "None";
                EditorGUI.LabelField(originalClipRect, originalClipName, EditorStyles.label);
            }

            void DrawOverrideClipField()
            {
                Rect overrideClipRect = RectUtils.GetRightColumnRect(position, ColumnSplitRatio);

                EditorGUI.BeginChangeCheck();
                var newOverrideClip = EditorGUI.ObjectField(
                    overrideClipRect, "", overrideClip, typeof(AnimationClip), allowSceneObjects: false);

                if(EditorGUI.EndChangeCheck())
                {
                    overrideClipProperty.objectReferenceValue = newOverrideClip;
                }
            }
        }

        /// <summary>
        /// Draws the headers for the list of <see cref="AnimatorOverrideProfile.ClipOverridePair"/> elements.
        /// Original        Override
        /// clip1           clip2
        /// ....
        /// </summary>
        private static void DrawClipPairHeader(Rect position)
        {
            var originalClipHeaderRect = RectUtils.GetLeftColumnRect(position, ColumnSplitRatio);
            EditorGUI.LabelField(originalClipHeaderRect, OriginalClipFieldName, EditorStyles.label);

            var overrideClipHeaderRect = RectUtils.GetRightColumnRect(position, ColumnSplitRatio);
            overrideClipHeaderRect.x += HeaderOffset;
            EditorGUI.LabelField(overrideClipHeaderRect, OverrideClipFieldName, EditorStyles.label);
        }

        /// <summary>
        /// Draws a message when there is no element available in the <see cref="AnimatorOverrideProfile.ClipOverridePair"/> list.
        /// </summary>
        private void DrawEmptyListMessage(Rect position)
        {
            string message = _animatorControllerProperty.objectReferenceValue == null
                ? "Assign a valid animator controller."
                : "The animator controller has no clips";

            EditorGUI.LabelField(position, message, EditorStyles.label);
        }

        private void HandleClipSelection(ReorderableList list)
        {
            if(IsIndexInBounds() == false)
            {
                return;
            }

            var selectedClipPairProperty = _clipOverridesProperty.GetArrayElementAtIndex(list.index);
            var originalClip = selectedClipPairProperty.FindPropertyRelative(OriginalClipFieldName).objectReferenceValue;
            EditorGUIUtility.PingObject(originalClip);

            return;
            bool IsIndexInBounds()
            {
                return list.index >= 0 && list.index < _clipOverridesProperty.arraySize;
            }
        }
        #endregion
    }
}
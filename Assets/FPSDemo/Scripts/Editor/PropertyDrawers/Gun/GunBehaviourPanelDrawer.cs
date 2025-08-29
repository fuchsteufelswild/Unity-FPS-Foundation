using Nexora.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.FPSDemo.Editor
{
    public sealed class GunBehaviourPanelDrawer
    {
        private readonly dynamic _selector;

        private bool _isFoldoutOpen;

        public GunBehaviourPanelDrawer(dynamic selector)
        {
            _selector = selector;
            _isFoldoutOpen = _selector.AvailableBehaviours.Length > 1;
        }

        public void Draw()
        {
            if(_selector.AvailableBehaviours.Length == 0)
            {
                EditorGUILayout.HelpBox($"No {_selector.HeaderName} components found.", UnityEditor.MessageType.Warning);
                return;
            }

            string foldoutLabel = $"{_selector.HeaderName} ({_selector.AvailableBehaviours.Length})";
            _isFoldoutOpen = EditorGUILayout.Foldout(_isFoldoutOpen, foldoutLabel, true);

            if (_isFoldoutOpen == false)
            {
                return;
            }

            using(new EditorGUI.IndentLevelScope())
            {
                int newIndex = GUILayout.SelectionGrid(_selector.SelectedIndex, _selector.BehaviourNames, 1, EditorGUIStyles.RadioButton);

                if(newIndex != _selector.SelectedIndex)
                {
                    _selector.SelectedIndex = newIndex;
                }
            }
        }
    }
}
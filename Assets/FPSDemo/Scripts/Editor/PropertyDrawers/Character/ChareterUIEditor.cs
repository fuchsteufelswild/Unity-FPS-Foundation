using Nexora.FPSDemo.UI;
using System;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.FPSDemo.Editor
{
    [CustomEditor(typeof(CharacterUI), true)]
    public class CharacterUIEditor : ToolboxEditor
    {
        private CharacterUI _character;

        private string FoldoutStateKey => $"CharacterUI.BehavioursFoldout";

        private bool _behavioursFoldout;

        private ICharacterUIBehaviour[] _behaviours;

        private void OnDisable() => SessionState.SetBool(FoldoutStateKey, _behavioursFoldout);

        private void OnEnable()
        {
            _character = target as CharacterUI;
            _behaviours ??= _character.gameObject.GetComponentsInChildren<ICharacterUIBehaviour>();
            _behavioursFoldout = SessionState.GetBool(FoldoutStateKey, _behaviours.IsEmpty() == false);
        }

        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            _behavioursFoldout = EditorGUILayout.Foldout(
                _behavioursFoldout, "Behaviours", true, EditorStyles.foldoutHeader);

            if (_behavioursFoldout == false)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (var behaviour in _behaviours)
                {
                    DrawRow(behaviour.GetType(), behaviour.gameObject);
                }
            }
        }

        private void DrawRow(Type type, GameObject behaviourObject)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string friendlyName = CharacterBehaviourEditor.GetFriendlyBehaviourName(type);
                EditorGUILayout.LabelField(friendlyName);

                if (behaviourObject != null)
                {
                    if (GUILayout.Button("Show", CharacterBehaviourEditor.BehaviourPingOptions))
                    {
                        EditorGUIUtility.PingObject(behaviourObject);
                    }
                }
            }
        }
    }
}

using Codice.Client.BaseCommands;
using System;
using System.Linq;
using System.Reflection;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.FPSDemo.Editor
{
    [CustomEditor(typeof(CharacterBehaviour), true)]
    public class CharacterBehaviourEditor : ToolboxEditor
    {
        private ICharacter _parentCharacter;
        private Dependencies _dependencies;

        private bool _dependenciesFoldout;
        private string _dependenciesLabel;

        public static readonly GUILayoutOption[] BehaviourPingOptions = { GUILayout.Width(60f) };

        private string FoldoutStateKey => $"{target.GetType().FullName}.DependenciesFoldout";

        private void OnDisable() => SessionState.SetBool(FoldoutStateKey, _dependenciesFoldout);

        protected virtual void OnEnable()
        {
            InitializeCharacter();
            InitializeDependencies();

            // Default state should be open if any of the required components are missing
            bool defaultState = _dependencies.Required.Any() && GetFoundComponentsCount().required < _dependencies.Required.Length;
            _dependenciesFoldout = SessionState.GetBool(FoldoutStateKey, defaultState);
        }

        private void InitializeCharacter()
        {
            if (target is MonoBehaviour behaviour)
            {
                _parentCharacter = behaviour.GetComponentInParent<ICharacter>();
            }
        }

        private void InitializeDependencies()
        {
            _dependencies = Dependencies.Create(target.GetType());

            if(_dependencies.HasDependencies == false)
            {
                return;
            }

            var (requiredFound, optionalFound) = GetFoundComponentsCount();
            _dependenciesLabel = GetDependenciesLabel(requiredFound, optionalFound);
        }

        private (int required, int optional) GetFoundComponentsCount()
        {
            if(_parentCharacter == null || _dependencies.HasDependencies == false)
            {
                return (0, 0);
            }

            int requiredFound = _dependencies.Required.Count(type => _parentCharacter.GetCC(type) != null);
            int optionalFound = _dependencies.Optional.Count(type => _parentCharacter.GetCC(type) != null);

            return (requiredFound, optionalFound);
        }

        private string GetDependenciesLabel(int requiredFound, int optionalFound)
        {
            string requiredColor = _dependencies.Required.Length == requiredFound ? "green" : "red";
            string optionalColor = optionalFound > 0 ? "purple" : "grey";

            return $"Dependencies    " +
                   $"Required: {requiredFound}/{_dependencies.Required.Length}   " +
                   $"Optional: {optionalFound}/{_dependencies.Optional.Length}";
        }

        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            if(_parentCharacter == null)
            {
                EditorGUILayout.HelpBox($"This object must be child of a {typeof(ICharacter).Name} object.", UnityEditor.MessageType.Error);
                return;
            }

            if(_dependencies.HasDependencies == false)
            {
                return;
            }

            DrawDependenciesSection();
        }

        private void DrawDependenciesSection()
        {
            EditorGUILayout.Space();

            _dependenciesFoldout = EditorGUILayout.Foldout(
                _dependenciesFoldout, _dependenciesLabel, true, EditorStyles.foldoutHeader);

            if (_dependenciesFoldout == false)
            {
                return;
            }

            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawDependencyList("Required", _dependencies.Required, UnityEditor.MessageType.Error);
            }

            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawDependencyList("Optional", _dependencies.Optional, UnityEditor.MessageType.Warning);
            }
        }

        private void DrawDependencyList(string title, Type[] behaviourTypes, UnityEditor.MessageType messageTypeForMissing)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);

            if (behaviourTypes.IsEmpty())
            {
                return;
            }

            foreach(var type in behaviourTypes)
            {
                var behaviourObject = _parentCharacter.GetCC(type) as UnityEngine.Object;
                DrawBehaviourRow(type, behaviourObject, messageTypeForMissing);
            }
        }

        private void DrawBehaviourRow(Type type, UnityEngine.Object behaviourObject, UnityEditor.MessageType messageTypeForMissing)
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                string friendlyName = GetFriendlyBehaviourName(type);
                EditorGUILayout.LabelField(friendlyName);

                if(behaviourObject != null)
                {
                    if(GUILayout.Button("Show", BehaviourPingOptions))
                    {
                        EditorGUIUtility.PingObject(behaviourObject);
                    }
                }
            }

            if(behaviourObject == null)
            {
                string errorMessage = messageTypeForMissing == UnityEditor.MessageType.Error
                    ? "Required behaviour not found, this component will not function correctly."
                    : "Optional behaviour not found, functionality might be limited.";
                EditorGUILayout.HelpBox(errorMessage, messageTypeForMissing);
            }
        }

        public static string GetFriendlyBehaviourName(Type type)
        {
            string name = type.Name;
            if(name.StartsWith("I") && char.IsUpper(name.ElementAtOrDefault(1)))
            {
                name = name.Substring(1);
            }

            return ObjectNames.NicifyVariableName(name);
        }

        private sealed class Dependencies
        {
            public static readonly Dependencies Empty = new(Array.Empty<Type>(), Array.Empty<Type>());

            public readonly Type[] Required;
            public readonly Type[] Optional;

            public bool HasDependencies => Required.Length > 0 || Optional.Length > 0;

            private Dependencies(Type[] required, Type[] optional)
            {
                Required = required ?? Array.Empty<Type>();
                Optional = optional ?? Array.Empty<Type>();
            }

            public static Dependencies Create(Type behaviourType)
            {
                var requiredTypes = behaviourType.GetCustomAttribute<RequireCharacterBehaviourAttribute>()?.Types;
                var optionalTypes = behaviourType.GetCustomAttribute<OptionalCharacterBehaviourAttribute>()?.Types;

                if(requiredTypes == null &&  optionalTypes == null)
                {
                    return Empty;
                }

                return new Dependencies(requiredTypes, optionalTypes);
            }
        }
    }
}
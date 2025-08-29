using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nexora
{
    /// <summary>
    /// Base definition for any data, provides minimal interface for common fields.
    /// </summary>
    /// <remarks>
    /// As a data class, it can be revalidated from editor classes via <see cref="IEditorValidatable"/> interface.
    /// </remarks>
    public abstract class Definition :
        ScriptableObject,
        IEditorValidatable
    {
        /// <summary>
        /// Unique ID of the data.
        /// </summary>
        public abstract int ID { get; }

        /// <summary>
        /// Name of the data. Designed so that names will be in the form of Category_RealName.
        /// This is so to group data more efficiently and avoid name collisions between categories.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Description of the data.
        /// </summary>
        public virtual string Description => string.Empty;

        /// <summary>
        /// How it will be displayed in the UI, Tools window.
        /// E.g Weapon/Fire Sword.
        /// Derive it customize.
        /// </summary>
        /// <remarks>
        /// Might be used by tools systems to distinguish <see cref="Definition"/> better
        /// in case of name collisions.
        /// </remarks>
        public virtual string DisplayName => Name;

        /// <summary>
        /// Icon of the data. 
        /// E.g Icon of the item.
        /// </summary>
        public virtual Sprite Icon => null;


#if UNITY_EDITOR
        public void RunValidation()
        {
            var args = new EditorValidationArgs(ValidationTrigger.ManualRefresh, isInEditor: true);
            EditorValidate(args);
        }

        /// <summary>
        /// Custom validation logic to perform when call is made from editor code.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="IEditorValidatable.RunValidation" path="/remarks"/>
        /// </remarks>
        public abstract void EditorValidate(in EditorValidationArgs validationArgs);
#endif
    }

    /// <summary>
    /// Base for typed <see cref="Definition"/>. Manages assigning unique ID,
    /// caching modified name. 
    /// </summary>
    /// <remarks>
    /// IDs are unique only between same types. E.g An Item data might have ID->5
    /// and a misc data might have ID->5, but no other Item data can have ID->5 again,
    /// this is a data collision.
    /// <br></br>
    /// <see cref="DefinitionRegistry{T}"/> keeps registry for all data of the type <typeparamref name="T"/>.
    /// </remarks>
    public abstract class Definition<T> :
        Definition
        where T : Definition<T>
    {
        [Tooltip("Unique ID of the data, automatically assigned.")]
        [SerializeField]
        [Disable]
        private int _id = DataConstants.InvalidID;

        /// <summary>
        /// Cached name to not repeatedly modify the name.
        /// </summary>
        [NonSerialized]
        private string _cachedName;

        public override int ID => _id;

        /// <summary>
        /// <inheritdoc cref="Definition.Name"/>
        /// </summary>
        /// <remarks>
        /// <see langword="get"/> returns the stripped part of the data name,
        /// that is, "Category" or grouping identifier removed. Item_Sword -> Sword.
        /// <br></br>
        /// <see langword="set"/> is used to rename data objects in tools windows,
        /// e.g when creating or modifying.
        /// </remarks>
        public override string Name
        {
            get => _cachedName ??= name.RemovePrefix('_');
            set
            {
                if (name != value)
                {
                    name = value;
                    _cachedName = value.RemovePrefix('_');
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Repeatedly tries to assign a unique ID to this data.
        /// In terms of clash with already existing data types of the same type, it tries again.
        /// </summary>
        /// <remarks>
        /// <b>The IDs are generated randomly with random functions, they are not HashCode bound to the object.</b>
        /// <br></br>
        /// The case where ID couldn't be assigned is near impossible.
        /// </remarks>
        private void GenerateAndAssignID()
        {
            const int MaxRetryCount = 100;

            int retryCount = 0;
            while (retryCount < MaxRetryCount)
            {
                int generatedID;
                do
                {
                    generatedID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                } while (generatedID is DataConstants.InvalidID or DataConstants.NullID);

                if(DefinitionRegistry<T>.First(definition => definition.ID == generatedID) == null)
                {
                    _id = generatedID;
                    UnityEditor.EditorUtility.SetDirty(this);
                    return;
                }
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Trying to solve the problem that <b>Reset</b> cannot handle the cases of <b>Duplication</b>,
        /// <b>Paste</b>, <b>Undo/Redo</b>. We need these events so that we can update the parts accordingly via
        /// <see cref="EditorValidate(in EditorValidationArgs)"/>. 
        /// </summary>
        /// <remarks>
        /// Note that this method's functionality is not guaranteed, as not all <b>OnValidate</b> calls
        /// have a valid and meaningful <see cref="Event.current"/>, or they might even have from previous 
        /// actions. This is a work-around that seems to work fine.
        /// <br></br>
        /// If problem starts to occur, then use solution of <see cref="UnityEditor.AssetPostprocessor"/>,
        /// which is an overkill and more complex solution but fail-safe.
        /// </remarks>
        private void OnValidate()
        {
            Event currentEvent = Event.current;
            if(currentEvent is { type: EventType.Used })
            {
                if(currentEvent.commandName is "Duplicate" or "Paste")
                {
                    EditorValidate(new EditorValidationArgs(ValidationTrigger.Duplication, false));
                }
            }
            else
            {
                EditorValidate(new EditorValidationArgs(ValidationTrigger.ManualRefresh, false));
            }
        }

        /// <summary>
        /// When called automatically by Unity inspector in the case of Resetting the object
        /// or newly creating it, it will validates the parts.
        /// </summary>
        private void Reset() => EditorValidate(new EditorValidationArgs(ValidationTrigger.Creation, false));

        /// <summary>
        /// In the case of creating a new asset or duplicating from another one, it
        /// assigns a new ID. Because, the first one it doesn't have a valid ID
        /// and for the second one, it copies the ID of the object it was duplicated from.
        /// </summary>
        /// <inheritdoc path="/remarks"/>
        public override void EditorValidate(in EditorValidationArgs validationArgs)
        {
            if(Name != name)
            {
                Name = name;
            }
            
            if(validationArgs.ValidationTrigger is ValidationTrigger.Creation or ValidationTrigger.Duplication)
            {
                GenerateAndAssignID();
            }
        }
#endif
    }

    /// <summary>
    /// Place the query and lookup for <see cref="Definition{T}"/>.
    /// Each registry is held for each different type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// For each type of different <see cref="Definition{T}"/>, 
    /// there is a registry of all the definitions of that type available.
    /// Lazily initializes the containers. Uses a lookup dictionary for performant
    /// O(1) queries.
    /// <br></br>
    /// In editor mode, fixes corrupted IDs when building dictionary.
    /// <br></br>
    /// <b>Uses registry resource path as "Resources/Definitions/{DefinitionType}"
    /// e.g Resources/Definitions/Item</b>
    /// <br></br>
    /// Might use <see cref="EditorUtility"/> to Add/Remove data to the registry
    /// from your own editor code. As well as reloading the registry.
    /// </remarks>
    public static class DefinitionRegistry<T>
        where T : Definition<T>
    {
        /// <summary>
        /// Resources path for specific type of T. It is a generic and might need to be
        /// edited for different games and folder structures.
        /// </summary>
        public static readonly string ResourcesStoragePath =
            $"Definitions\\{typeof(T).Name.Replace("Definition", "")}";

        /// <summary>
        /// Lookup dictionary for easier and performant lookup,
        /// instead of plain array.
        /// </summary>
        private static Dictionary<int, T> _idToDefinition;

        /// <summary>
        /// All definitions of type T in the project.
        /// </summary>
        private static T[] _allDefinitions;

        public static T[] AllDefinitions => _allDefinitions ??= LoadAllDefinitions();
        public static bool Initialized => _allDefinitions != null;

        private static Dictionary<int, T> IdToDefinition => _idToDefinition ??= BuildLookupDictionary();

        /// <summary>
        /// Used to store <see cref="Definition{T}.GenerateAndAssignID"/> method as the method
        /// is <see langword="private"/> and this class needs to safely call that method in Editor mode.
        /// </summary>
        /// <remarks>
        /// Other solutions exist, but they to some degree remove safety. Like making it internal, or
        /// changing access specifier. But that method is so crucial for data to make these adjustments.
        /// </remarks>
        private static MethodInfo IDAssigner;

        #region Queries
        public static T GetByName(string definitionName)
            => First(definition => definition.Name == definitionName);

        public static bool TryGetByName(string definitionName, out T result)
            => TryGetFirst(definition => definition.Name == definitionName, out result);

        /// <summary>
        /// Returns the first element that matches the <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>Not performant, O(n). Use <see cref="GetByID(int)"/> if possible.</remarks>
        public static T First(Func<T, bool> predicate) 
            => AllDefinitions.First(predicate);

        /// <summary>
        /// Fills <paramref name="result"/> with the first element that matches the <paramref name="predicate"/>.
        /// </summary>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetFirst(Func<T, bool> predicate, out T result)
            => AllDefinitions.TryGetFirst(predicate, out result);

        public static bool Contains(T definition) => IdToDefinition.ContainsKey(definition.ID);

        public static bool TryGetByID(int ID, out T definition)
            => IdToDefinition.TryGetValue(ID, out definition);

        public static T GetByID(int ID)
            => IdToDefinition.GetValueOrDefault(ID);
        #endregion


        /// <summary>
        /// Loads all <see cref="Definition{T}"/> available in the <see cref="ResourcesStoragePath"/>
        /// and sorts them by their <see cref="Definition.DisplayName"/>.
        /// </summary>
        /// <returns>Loaded definitions.</returns>
        private static T[] LoadAllDefinitions()
        {
            var definitions = Resources.LoadAll<T>(ResourcesStoragePath);

            Array.Sort(definitions, (def1, def2)
                => string.Compare(def1.DisplayName, def2.DisplayName, StringComparison.InvariantCultureIgnoreCase));

            if (definitions.Length > 0)
            {
                return definitions;
            }

            UnityEngine.Debug.LogWarning($"No resource instance is found for the type <{typeof(T).Name}> " +
                $"in the folder {Path.Combine(nameof(Resources), ResourcesStoragePath)}.");

            return Array.Empty<T>();
        }

        /// <summary>
        /// Builds lookup dictionary of (<see langword="int"/>-><see cref="Definition"/>), 
        /// from all definitions currently had.
        /// </summary>
        /// <returns>Built dictionary.</returns>
        private static Dictionary<int, T> BuildLookupDictionary()
        {
            var idToDefiniton = new Dictionary<int, T>(AllDefinitions.Length);

            foreach (var definition in AllDefinitions)
            {
#if UNITY_EDITOR
                // May fix collisions and uninitialized definitions in Editor's edit mode.
                if (Application.isPlaying == false)
                {
                    if (definition.ID == DataConstants.InvalidID
                    || idToDefiniton.ContainsKey(definition.ID))
                    {
                        IDAssigner ??= typeof(Definition<T>).GetMethod("GenerateAndAssignID", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        IDAssigner.Invoke(definition, null);
                    }
                }
#endif
                idToDefiniton.Add(definition.ID, definition);
            }

            return idToDefiniton;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Provides Editor only logic.
        /// </summary>
        public static class EditorUtility
        {
            /// <summary>
            /// Adds new definition to the registry, 
            /// used by editor scripts when creating a new definition by code.
            /// </summary>
            public static void AddDefinition(T definition)
            {
                _allDefinitions ??= Array.Empty<T>();
                UnityEditor.ArrayUtility.Add(ref _allDefinitions, definition);
                _idToDefinition?.Add(definition.ID, definition);
            }

            /// <summary>
            /// Removes definition from the registry, 
            /// used by editor scripts when deleting a definition by code.
            /// </summary>
            public static void RemoveDefinition(T definition)
            {
                UnityEditor.ArrayUtility.Remove(ref _allDefinitions, definition);
                _idToDefinition?.Remove(definition.ID);
            }

            /// <summary>
            /// Reloads the registry.
            /// </summary>
            public static void ReloadDefinitions()
            {
                _allDefinitions = LoadAllDefinitions();
                _idToDefinition = null;
            }
        }
#endif
    }
}
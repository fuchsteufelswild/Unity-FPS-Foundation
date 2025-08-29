using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;

namespace Nexora
{
    /// <summary>
    /// Layer indices can be used for any game, for any specific usage create a new class
    /// </summary>
    public static partial class Layers
    {
        // Layer indices
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int Debris = 6;
        public const int Effect = 7;
        public const int TriggerZone = 8;
        public const int Interactable = 9;
        public const int ViewModel = 10;
        public const int PostProcessing = 11;
        public const int Hitbox = 12;
        public const int Character = 13;
        public const int StaticObject = 14;
        public const int DynamicObject = 15;
        public const int Building = 16;
        public const int InteractableNoCollision = 17;

        // Layer masks
        public const int CharacterMask = (1 << Character);
        public const int SimpleSolidObjectsMask = (1 << Default) | (1 << StaticObject) | (1 << DynamicObject);
        public const int SolidObjectsMask = SimpleSolidObjectsMask;
    }

    public static partial class Tags
    {
        public const string ModulesRoot = "ModulesRoot";
        public const string GameModule = "GameModule";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
    }

    public static partial class ExecutionOrder
    {
        public const int GameModule = -100000;
        public const int DIContainer = -90000;
        public const int Singleton = -10000;

        public const int EarlyGameLogic = -1000;
        public const int LateGameLogic = -100;

        public const int EarlyPresentation = -10;
        public const int DefaultPresentation = 0;
        public const int AfterDefaultPresentation = 10;
        public const int LatePresentation = 100;
    }

    public static partial class StringConstants
    {
        public const int NotFound = -1;
    }

    public static partial class CollectionConstants
    {
        public const int NotFound = -1;
    }

    public static partial class DataConstants
    {
        /// <summary>
        /// Represents ID of invalid <see cref="Definition"/>.
        /// ID is not set, and it should have a valid value.
        /// </summary>
        public const int InvalidID = -1;

        /// <summary>
        /// Represents ID of a null <see cref="Definition"/>.
        /// <see cref="DefinitionReference{T}"/> uses this value.
        /// </summary>
        public const int NullID = 0;
    }

    public static partial class AttributeConstants
    {
        public const string EditorOnlyWarning = "This method should only be used in editor scripts.";
    }

    public static partial class ComponentMenuPaths
    {
        public const string Base = "Nexora/";
        
        public const string SaveSystem = Base + "Save System/";
        public const string ObjectPooling = Base + "Object Pooling/";
        public const string Audio = Base + "Audio/";
        public const string Input = Base + "Input/";
        public const string PostProcessing = Base + "PostProcessing/";
        public const string ProceduralAnimation = Base + "Procedural Animation/";

        public const string BaseUI = Base + "UI/";
    }
}
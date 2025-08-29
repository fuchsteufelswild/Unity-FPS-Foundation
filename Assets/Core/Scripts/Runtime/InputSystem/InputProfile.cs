using UnityEngine;

namespace Nexora.InputSystem
{
    /// <summary>
    /// Class to define Input Profiles, such that in the profile what types of <see cref="IInputHandler"/>
    /// is available (e.g Movement Input, Leaning Input, Shooting Input, UI Input etc.) 
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Input System/Input Profile", fileName = "InputProfile_")]
    public sealed class InputProfile : ScriptableObject
    {
        [SerializeField]
        [ReorderableList]
        [ClassImplements(typeof(IInputHandler), AllowAbstract = false, TypeGrouping = TypeGrouping.ByAddComponentMenu)]
        private SerializedType[] _allowedInputs;

        public SerializedType[] AllowedInputs => _allowedInputs;

        private static InputProfile _nullInputProfile;

        public static InputProfile NullInputProfile
        {
            get
            {
                if(_nullInputProfile == null)
                {
                    _nullInputProfile = CreateInstance<InputProfile>();
                }

                return _nullInputProfile;
            }
        }
    }
}

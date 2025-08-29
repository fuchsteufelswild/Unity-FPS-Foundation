using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nexora.Animation
{
    /// <summary>
    /// Class to wrap and define more explicit animator parameters in the inspector and easier to use.
    /// Provides fields for the Name, Type(int, float etc.), and Value.
    /// </summary>
    /// <remarks>
    /// <b>Hash is created everytime the object is serialized(e.g game started, or a prefab instantiated).</b>
    /// Drawer of this class provides this class with means of easier selection of parameters via dropdown.
    /// And provides a compound vay to set <see cref="Value"/> depending on the <see cref="AnimatorControllerParameterType"/>.
    /// <br></br>
    /// E.g For bool(0 or 1), for trigger None etc.
    /// </remarks>
    [Serializable]
    public sealed class AnimatorParameter : 
        ISerializationCallbackReceiver
    {
        [HideInInspector]
        public string Name;

        public AnimatorControllerParameterType Type;

        [HideInInspector]
        public float Value;

        [NonSerialized]
        public int Hash;

        public AnimatorParameter(string name, AnimatorControllerParameterType type, float value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public void ApplyTo(Animator animator, float multiplier = 1f)
        {
            switch (Type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(Hash, Value);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(Hash, (int)Value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(Hash, Value > 0f);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(Hash);
                    break;
                default: 
                    throw new ArgumentOutOfRangeException();    
            }
        }

        public void OnAfterDeserialize() => CreateHash();
        public void OnBeforeSerialize() => CreateHash();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CreateHash()
        {
            if(Hash == 0)
            {
                Hash = Animator.StringToHash(Name);
            }
        }
    }
}

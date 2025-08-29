using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Nexora.InputSystem
{
    public enum InputEnableMode : byte
    {
        [Tooltip("Enabled state depends on context pushed, (e.g UI, Build, Weapon)")]
        Contextual,

        [Tooltip("Input is always active.")]
        AlwaysEnabled,

        [Tooltip("Input is always inactive.")]
        AlwaysDisabled,

        [Tooltip("Enabled state is controlled via class variable.")]
        ScriptControlled
    }

    /// <summary>
    /// Interface to define classes that handles some input of Unity's new input system.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// Used in case of controlling enabled state via code.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// How the handler's enabled state is controlled.
        /// </summary>
        InputEnableMode EnableMode { get; }
    }

    /// <summary>
    /// Abstract class to define Input Handler that is not bound to any other parent class.
    /// That is, working on its own.
    /// </summary>
    public abstract class InputHandlerBase :
        MonoBehaviour,
        IInputHandler
    {
        [SerializeField]
        public InputEnableMode EnableMode { get; private set; } = InputEnableMode.Contextual;

        public bool IsEnabled 
        { 
            get => enabled; 
            set
            {
                if(this != null)
                {
                    enabled = value;
                }
            }
        }

        protected virtual void Awake()
        {
            enabled = false;
            InputModule.Instance.RegisterInputHandler(this);
        }

        protected virtual void OnDestroy()
        {
            InputModule.Instance.UnregisterInputHandler(this);
        }
    }

    /// <summary>
    /// Input Handler that is bound to some parent object, that is child of some other <see cref="GameObject"/>
    /// via connection of <see cref="ChildBehaviour{U, T}"/>.
    /// </summary>
    /// <typeparam name="TChild"><inheritdoc cref="ChildBehaviour{U, T}" path="/typeparam[@name='TChild']"/></typeparam>
    /// <typeparam name="TParent"><inheritdoc cref="ChildBehaviour{U, T}" path="/typeparam[@name='TParent']"/></typeparam>
    public abstract class InputHandler<TChild, TParent> :
        ChildBehaviour<TChild, TParent>,
        IInputHandler
        where TChild : IChildBehaviour<TChild, TParent>
        where TParent : IParentBehaviour<TParent, TChild>
    {
        [field: SerializeField]
        public InputEnableMode EnableMode { get; private set; } = InputEnableMode.Contextual;

        public bool IsEnabled
        {
            get => enabled;
            set
            {
                if (this != null)
                {
                    enabled = value;
                }
            }
        }

        protected virtual void Awake()
        {
            enabled = false;
            InputModule.Instance.RegisterInputHandler(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputModule.Instance.UnregisterInputHandler(this);
        }
    }


    //public interface ICharacterBehaviour : IChildBehaviour<ICharacterBehaviour, ICharacter>
    //{

    //}

    //public interface ICharacterInputHandler : ICharacterBehaviour
    //{

    //}

    //public class CharacterInputHandler : ChildBehaviour<ICharacterBehaviour, ICharacter>, ICharacterInputHandler
    //{

    //}

    //public interface ICharacter : IParentBehaviour<ICharacter, ICharacterBehaviour>
    //{

    //}

    //public class BodyLeanInputHandler : InputHandler<ICharacterBehaviour, ICharacter>
    //{

    //}
}

using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.UI
{
    public interface ICharacterUI : IParentBehaviour<ICharacterUI, ICharacterUIBehaviour>
    {
        /// <summary>
        /// Currently attached <see cref="ICharacter"/>.
        /// </summary>
        ICharacter Character { get; }
    }

    /// <summary>
    /// UI behaviour attached to a specific <see cref="ICharacter"/>. Acts as a main control mechanism
    /// for all <see cref="ICharacterUIBehaviour"/>
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.LatePresentation)]
    public class CharacterUI : 
        ParentBehaviour<ICharacterUI, ICharacterUIBehaviour>, 
        ICharacterUI
    {
        /// <summary>
        /// How should initially this UI behaviour is attached to <see cref="ICharacter"/>.
        /// </summary>
        private enum InitialAttachmentMethod
        {
            /// <summary>
            /// Attaches to <see cref="ICharacter"/> component in the parent object.
            /// </summary>
            AttachToParent,

            /// <summary>
            /// Attaches to <see cref="ICharacter"/> component in one of the children object.
            /// </summary>
            AttachToChild,

            /// <summary>
            /// Attachment logic is done manually, without being tied to logic of this class.
            /// </summary>
            AttachManual
        }

        [SerializeField]
        private InitialAttachmentMethod _initialAttachmentMethod = InitialAttachmentMethod.AttachManual;

        [SerializeField]
        private FPSCharacter _fpsCharacter;

        private bool _isDestroyed;

        public ICharacter Character { get; private set; }

        static CharacterUI()
        {
            _componentToInterfacePairs = BuildComponentToInterfaceDictionary();
            _cachedComponents = new List<ICharacterUIBehaviour>();
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            _components.Clear();
        }

        protected override void Awake() 
        { 
            if(_fpsCharacter != null)
            {
                _fpsCharacter.Initialized += () => AttachToCharacter(_fpsCharacter);
            }
        }

        protected virtual void Start()
        {
            if(_initialAttachmentMethod == InitialAttachmentMethod.AttachManual)
            {
                return;
            }

            ICharacter character = null;

            switch(_initialAttachmentMethod)
            {
                case InitialAttachmentMethod.AttachToParent:
                    character = GetComponentInParent<ICharacter>();
                    break;
                case InitialAttachmentMethod.AttachToChild:
                    character = GetComponentInChildren<ICharacter>();
                    break;
            }

            if(character != null)
            {
                AttachToCharacter(character);
            }
        }

        /// <summary>
        /// Attaches the UI to the <paramref name="newCharacter"/> and
        /// detaches if it was already attached to a new character.
        /// Then informs the child UI behaviours about the change.
        /// </summary>
        /// <param name="newCharacter"></param>
        public void AttachToCharacter(ICharacter newCharacter)
        {
            if(newCharacter == Character)
            {
                Debug.LogWarningFormat("{0} is already attached to the {1}", name, newCharacter.gameObject.name);
                return;
            }

            // Detach
            if(Character != null)
            {
                Character.Destroyed -= OnAttachedCharacterDestroyed;
            }

            // Attach
            ICharacter previousCharacter = Character;
            Character = newCharacter;

            if(newCharacter != null)
            {
                newCharacter.Destroyed += OnAttachedCharacterDestroyed;
            }

            foreach(var (_, behaviour) in _components)
            {
                behaviour.OnCharacterChanged(previousCharacter, newCharacter);
            }
        }

        /// <summary>
        /// Called by the attached <see cref="ICharacter"/> when the attached
        /// <see cref="ICharacter"/> is destroyed.
        /// </summary>
        private void OnAttachedCharacterDestroyed(ICharacter character)
        {
            character.Destroyed -= OnAttachedCharacterDestroyed;

            ICharacter previousCharacter = Character;
            Character = null;

            foreach(var (_, behaviour) in _components)
            {
                behaviour.OnCharacterChanged(previousCharacter, null);
            }
        }

        public override void AddBehaviour(ICharacterUIBehaviour behaviour)
        {
            _components ??= new Dictionary<System.Type, ICharacterUIBehaviour>();

            var interfaceType = _componentToInterfacePairs[behaviour.GetType()];
            if (_components.ContainsKey(interfaceType))
            {
                return;
            }

            _components.Add(interfaceType, behaviour);
            if(Character != null)
            {
                behaviour.OnCharacterChanged(null, Character);
            }
        }

        public override void RemoveBehaviour(ICharacterUIBehaviour behaviour)
        {
            var interfaceType = _componentToInterfacePairs[behaviour.GetType()];
            if (_isDestroyed == false)
            {
                _components.Remove(interfaceType);
                
            }

            behaviour.OnCharacterChanged(Character, null);
        }
    }
}
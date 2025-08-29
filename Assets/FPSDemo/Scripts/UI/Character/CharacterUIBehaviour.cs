using UnityEngine;

namespace Nexora.FPSDemo.UI
{
    public interface ICharacterUIBehaviour : IChildBehaviour<ICharacterUIBehaviour, ICharacterUI>
    {
        /// <summary>
        /// Called when the attached gameplay <see cref="ICharacter"/> changes.
        /// </summary>
        /// <param name="previousCharacter">Previously attached <see cref="ICharacter"/>.</param>
        /// <param name="newCharacter">To be attached to <see cref="ICharacter"/>.</param>
        void OnCharacterChanged(ICharacter previousCharacter, ICharacter newCharacter);
    }

    /// <summary>
    /// Abstract base class for UI behaviours of the character. Can be attached/detached on <see cref="ICharacter"/>,
    /// and provides hooks for attach/detach methods. Controlled by <see cref="ICharacterUI"/>, which is the 
    /// <see cref="ParentBehaviour{TParent, TChild}"/> of all these behaviours.
    /// </summary>
    public abstract class CharacterUIBehaviour : 
        ChildBehaviour<ICharacterUIBehaviour, ICharacterUI>, 
        ICharacterUIBehaviour
    {
        protected ICharacter AttachedCharacter { get; private set; }

        protected virtual void Awake()
        {
            Parent = GetComponentInParent<ICharacterUI>();

            if (Parent == null)
            {
                return;
            }

            Parent.AddBehaviour(this);
        }

        void ICharacterUIBehaviour.OnCharacterChanged(ICharacter previousCharacter, ICharacter newCharacter)
        {
            if(previousCharacter == newCharacter)
            {
                return;
            }

            if(AttachedCharacter != null)
            {
                OnCharacterDetached(AttachedCharacter);
            }

            AttachedCharacter = newCharacter;
            if(newCharacter != null)
            {
                OnCharacterAttached(newCharacter);
            }
        }

        protected override void OnBehaviourDestroy(ICharacterUI parent)
        {
            base.OnBehaviourDestroy(parent);
            if(UnityUtils.IsValidUnityObject(parent))
            {
                Parent.RemoveBehaviour(this);
            }
        }

        /// <summary>
        /// Called when this behaviour is attached to the <paramref name="character"/>.
        /// </summary>
        /// <param name="character"></param>
        protected virtual void OnCharacterAttached(ICharacter character) { }

        /// <summary>
        /// Called when this behaviour is detached from the <paramref name="character"/>.
        /// </summary>
        /// <param name="character"></param>
        protected virtual void OnCharacterDetached(ICharacter character) { }

    }
}
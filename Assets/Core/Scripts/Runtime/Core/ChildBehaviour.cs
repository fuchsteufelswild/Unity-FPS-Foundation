using Nexora.InputSystem;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;

namespace Nexora
{
    /// <summary>
    /// Represents a child behaviour, just an indicator for now without any logic.
    /// </summary>
    /// <remarks>
    /// Implements <b>CRTP</b> technique together with <see cref="IParentBehaviour{TParent, TChild}"/>,
    /// so that there is a type-safe relation between child and parent behaviours. E.g
    /// we can't add a VehicleBehaviour to a Character
    /// </remarks>
    /// <typeparam name="TParent">Type of the parent interface (e.g IPlayer)</typeparam>
    /// <typeparam name="TChild">Type of the child behaviour interface (e.g IInventoryHandler)</typeparam>
    public interface IChildBehaviour<TChild, TParent> :
        IMonoBehaviour
        where TChild : IChildBehaviour<TChild, TParent>
        where TParent : IParentBehaviour<TParent, TChild>
    {

    }

    /// <summary>
    /// Represents a <see cref="MonoBehaviour"/> that is tied to a specific <see cref="MonoBehaviour"/> parent.
    /// Ties its Unity callbacks to its parent object.
    /// Think the scenario where you have a main object, Player. And you want to have series
    /// of component's that are tied to this main object. Works 100% with it, like calling callbacks
    /// when that callback is called on the parent. This class provides exactly that and provides
    /// a naming to differentiate them better.
    /// </summary>
    /// <remarks>
    /// You can use this class in two ways. 
    /// <br></br> - If you need more control on the naming, then create your own new class deriving from it with specific <typeparamref name="TParent"/> 
    /// and use that type in every field and further derived class.
    /// <br></br> - Keep the name as is and just use it in the field with <typeparamref name="TParent"/> and for the classes deriving from it.
    /// </remarks>
    /// <typeparam name="TParent">Type of the parent interface (e.g IPlayer)</typeparam>
    /// <typeparam name="TChild">Type of the child interface (e.g IInventoryHandler)</typeparam>
    [Serializable]
    public abstract class ChildBehaviour<TChild, TParent> :
        MonoBehaviour,
        IChildBehaviour<TChild, TParent>
        where TChild : IChildBehaviour<TChild, TParent>
        where TParent : IParentBehaviour<TParent, TChild>
    {
        public TParent Parent { get; protected set; }

        protected virtual void Start()
        {
            TParent parent = GetComponentInParent<TParent>();

            Assert.IsTrue(parent != null,
                string.Format("Component of type {0} is not found in the parent object", typeof(TParent).Name));

            Parent = parent;
            OnBehaviourStart(parent);

            if (enabled)
            {
                OnBehaviourEnable(parent);
            }
        }

        protected virtual void OnEnable()
        {
            if (Parent != null)
            {
                OnBehaviourEnable(Parent);
            }
        }

        protected virtual void OnDisable()
        {
            if (Parent != null)
            {
                OnBehaviourDisable(Parent);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Parent != null)
            {
                OnBehaviourDestroy(Parent);
            }
        }

        /// <summary>
        /// Like Unity's <b>Start</b> but synced with the <see cref="Parent"/> object.
        /// </summary>
        protected virtual void OnBehaviourStart(TParent parent) { }

        /// <summary>
        /// Like Unity's <b>OnEnable</b> but synced with the <see cref="Parent"/> object.
        /// Called only if has a valid <see cref="Parent"/>.
        /// </summary>
        protected virtual void OnBehaviourEnable(TParent parent) { }

        /// <summary>
        /// Like Unity's <b>OnDisable</b> but synced with the <see cref="Parent"/> object.
        /// Called only if has a valid <see cref="Parent"/>.
        /// </summary>
        protected virtual void OnBehaviourDisable(TParent parent) { }

        /// <summary>
        /// Like Unity's <b>Destroy</b> but synced with the <see cref="Parent"/> object.
        /// Called only if has a valid <see cref="Parent"/>.
        /// </summary>
        protected virtual void OnBehaviourDestroy(TParent parent) { }
    }

    /*Example use cases for a Character component, which might have many child behaviours
    
    public interface ICharacter : IParentBehaviour<ICharacter, ICharacterBehaviour>
    {

    }

    public interface ICharacterBehaviour : IChildBehaviour<ICharacterBehaviour, ICharacter>
    {
        
    }
    
    // Just for a easier access later, name like this and derive from it later
    public class CharacterBehaviour : ChildBehaviour<ICharacterBehaviour, ICharacter>, ICharacterBehaviour
    {
    }

    public class CharacterInventoryHandler : CharacterBehaviour
    {

    }
    // ---------------------
    // Extend the interface
    public interface ICharacterInventoryHandler : ICharacterBehaviour
    {

    }

    public class CharacterInventoryHandler : ChildBehaviour<ICharacterBehaviour, ICharacter>, ICharacterFOVHandler
    {

    }
    // --- 
    
    public class CharacterInventoryHandler : ChildBehaviour<ICharacterBehaviour, ICharacter>, ICharacterBehaviour
    {

    }

    public class Character : ParentBehaviour<ICharacter, ICharacterBehaviour>, ICharacter
    {
       
    }
    
     */
}

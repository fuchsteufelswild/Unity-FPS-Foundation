using Nexora.Audio;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Interface that defines an <see langword="object"/> on which we can perform 
    /// various <see cref="ItemAction"/>. 
    /// </summary>
    /// <remarks>
    /// <see cref="TryGetContextInterface{T}(out T)"/>, should provide the caller 
    /// with an <see langword="object"/> that implements interface of <b>T</b>.
    /// </remarks>
    public interface IItemActionContext : IMonoBehaviour
    {
        /// <summary>
        /// Returns <see langword="object"/> that implements the <see langword="interface"/>
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the subobject/component asked.</typeparam>
        /// <returns>If context interface is found.</returns>
        bool TryGetContextInterface<T>(out T result) where T : class;

        /// <summary>
        /// Gets the inventory of the context.
        /// </summary>
        IInventory Inventory { get; }

        /// <summary>
        /// Plays the <paramref name="audio"></paramref>.
        /// </summary>
        void PlayAudio(AudioCue audio);

        /// <summary>
        /// Throws the <paramref name="throwedObjectBody"/> with <paramref name="dropForce"/> and <paramref name="dropTorque"/>.
        /// </summary>
        /// <param name="throwedObjectBody">Object being throwed.</param>
        /// <param name="dropForce">Force it is dropped with.</param>
        /// <param name="dropTorque">Toque objects is dropped with.</param>
        void ThrowObject(Rigidbody throwedObjectBody, Vector3 dropForce, float dropTorque);

        /// <summary>
        /// Gets <see cref="Transform"/> for the given <paramref name="dropPoint"/>.
        /// </summary>
        Transform GetDropPointTransform(ItemDropPoint dropPoint);
    }

    /// <summary>
    /// Abstract base class for defining actions that can be performed by actions on any object
    /// that implements <see cref="IItemActionContext"/>. 
    /// </summary>
    /// <remarks>
    /// This class will be shared among all of the items. It doesn't contain any data except for parameters
    /// for filling the UI fields. The main logic doesn't use self parameters, it is basically a <b>strategy</b>
    /// pattern but with shared instances. The beauty is, it is both <see langword="abstract"/> and should not 
    /// be created newly for each item thanks to the <b>Dependency Injection</b> through <see cref="ScriptableObject"/> architecture.
    /// <br></br><br></br>
    /// <b>Note:</b> It could be a question of whether it is a good idea to have actions be implemented by strategies in a 
    /// unified manner, rather than every object implements its own functionality. The latter seems better architecture
    /// and more decoupled, but the problem is that the most of the functionality would be shared for any object type. 
    /// Also, the parent class would be bloated with every action, even if we propagate it to the child classes/components,
    /// it would still be a repetition and a code bloat.
    /// So it is better to have a unified actions with <see langword="abstract"/>/<see langword="virtual"/> <see langword="methods"/>
    /// for the utmost simplicity and extendability at the same time.
    /// </remarks>
    [Serializable]
    public abstract class ItemAction : ScriptableObject
    {
        protected const string CreateMenuPath = "Nexora/Items/Item Actions/";

        [Title("UI Parameters")]
        [Tooltip("Display name shown in the UI menus.")]
        [SerializeField]
        private string _displayName;

        [Tooltip("Verb used to define the action (Use, Equip etc.)")]
        [SerializeField]
        private string _verb;

        [Tooltip("Icon displayed in the UI for this action.")]
        [SerializeField]
        private Sprite _icon;

        /// <summary>
        /// Display name shown in the UI menus.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Verb used to define the action (Use, Equip etc.)
        /// </summary>
        public string Verb => _verb;

        /// <summary>
        /// Icon displayed in the UI for this action.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Determines the action can be performed on a specific <paramref name="actionContext"/>
        /// with <paramref name="itemStack"/> is the owner of the action.
        /// </summary>
        /// <param name="actionContext">Context on which the action will be performed on.</param>
        /// <param name="itemStack">Item that has the action.</param>
        /// <returns>If the action of <paramref name="itemStack"/> can be performed on <paramref name="actionContext"/>.</returns>
        public abstract bool CanPerform(IItemActionContext actionContext, ItemStack itemStack);

        /// <summary>
        /// Determines duration of the action that will be performed on <paramref name="actionContext"/>
        /// with <paramref name="itemStack"/> is the owner of the action.
        /// </summary>
        /// <param name="actionContext"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='actionContext']"/></param>
        /// <param name="itemStack"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='itemStack']"/></param>
        /// <returns></returns>
        public abstract float GetDuration(IItemActionContext actionContext, ItemStack itemStack);

        /// <summary>
        /// Executes the action owned by <see cref="IItem"/> of <paramref name="itemStack"/>, on the <paramref name="actionContext"/>,
        /// where action has the <paramref name="duration"/> of seconds.
        /// </summary>
        /// <param name="actionContext"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='actionContext']"/></param>
        /// <param name="slot"><see cref="Slot"/> in which the used item is currently placed in.</param>
        /// <param name="itemStack"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='itemStack']"/></param>
        /// <param name="duration">Duration of the action to be performed.</param>
        /// <returns></returns>
        protected abstract IEnumerator ExecuteAction(
            IItemActionContext actionContext, 
            Slot slot, 
            ItemStack itemStack, 
            float duration);

        /// <summary>
        /// Executes the action owned by <see cref="IItem"/> of <paramref name="itemStack"/>, on the <paramref name="actionContext"/>,
        /// where action has the <paramref name="duration"/> of seconds.
        /// </summary>
        /// <param name="actionContext"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='actionContext']"/></param>
        /// <param name="slot"><see cref="Slot"/> in which the used item is currently placed in.</param>
        /// <param name="itemStack"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='itemStack']"/></param>
        /// <param name="durationScale">Optional scale multiplier for the duration.</param>
        /// <returns>Tuple containing the started <see cref="Coroutine"/> and the duration the action will take.</returns>
        public (Coroutine coroutine, float duration) Perform(
            IItemActionContext actionContext,
            Slot slot,
            ItemStack itemStack,
            float durationScale = 1f)
        {
            EnsureContext(actionContext);

            if(CanPerform(actionContext, itemStack) == false)
            {
                return (null, 0f);
            }

            float calculatedDuration = GetDuration(actionContext, itemStack) * durationScale;
            var routine = ExecuteAction(actionContext, slot, itemStack, calculatedDuration);
            return (actionContext.StartCoroutine(routine), calculatedDuration);
        }

        /// <summary>
        /// Stops the current running action.
        /// </summary>
        /// <param name="actionCoroutine">Action that is currently running.</param>
        /// <param name="actionContext"><inheritdoc cref="ItemAction.CanPerform(IItemActionContext, ItemStack)" path="/param[@name='actionContext']"/></param>
        public void StopAction(ref Coroutine actionCoroutine, IItemActionContext actionContext)
        {
            EnsureContext(actionContext);

            actionContext.StopCoroutine(actionCoroutine);
            actionCoroutine = null;
        }

        protected void EnsureContext(IItemActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(IItemActionContext));
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void Reset() => _displayName = name;

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(_displayName))
            {
                _displayName = name;
            }
        }
    }
}
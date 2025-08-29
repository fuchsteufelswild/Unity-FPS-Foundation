using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Represents a parent behaviour, and an interface to add/remove and get components,
    /// just like interface of Unity.
    /// </summary>
    /// <remarks>
    /// Implements <b>CRTP</b> technique together with <see cref="IChildBehaviour{TChild, TParent}"/>,
    /// so that there is a type-safe relation between child and parent behaviours. E.g
    /// we can't add a VehicleBehaviour to a Character.
    /// <br></br>
    /// <b>Check <see cref="IChildBehaviour{TChild, TParent}"/> for usage examples.</b>
    /// <br></br>
    /// Note: <b>Named TryGetCC and GetCC, to not confuse with Unity's methods of GetComponent.
    /// CC stands for ChildComponent.</b>
    /// </remarks>
    /// <inheritdoc cref="IChildBehaviour{TChild, TParent}" path="/typeparam[@name='TParent']"/>
    /// <inheritdoc cref="IChildBehaviour{TChild, TParent}" path="/typeparam[@name='TChild']"/>
    public interface IParentBehaviour<TParent, TChild> :
        IMonoBehaviour
        where TParent : IParentBehaviour<TParent, TChild>
        where TChild : IChildBehaviour<TChild, TParent>
    {
        void AddBehaviour(TChild behaviour);
        void RemoveBehaviour(TChild behaviour);

        /// <summary>
        /// Gets a interface of a <see cref="TChild"/> component that implements interface <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// Works in O(1) time.
        /// </remarks>
        /// <typeparam name="T">Implemented interface.</typeparam>
        /// <returns>If component of type <typeparamref name="T"/> has found.</returns>
        bool TryGetCC<T>(out T component) where T : class, TChild;

        /// <inheritdoc cref="TryGetCC{T}(out T)"/>
        T GetCC<T>() where T : class, TChild;

        /// <summary>
        /// Gets a child component which has implemented the <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Works in O(n) -> where n is the number of child components this parent has.
        /// </remarks>
        TChild GetCC(Type type);
    }

    /// <summary>
    /// Abstract parent behaviour, provides a default implementation for 
    /// get methods and finding the children object in the hierarchy.
    /// </summary>
    /// <remarks>
    /// <see cref="AddBehaviour(TChild)"/> and <see cref="RemoveBehaviour(TChild)"/>
    /// is best to provided in children behaviours, as different systems might treat 
    /// situations differently.
    /// </remarks>
    /// <inheritdoc cref="IChildBehaviour{Child, Parent}" path="/typeparam[@name='TParent']"/>
    /// <inheritdoc cref="IChildBehaviour{Child, Parent}" path="/typeparam[@name='TChild']"/>
    public abstract class ParentBehaviour<TParent, TChild> :
        MonoBehaviour,
        IParentBehaviour<TParent, TChild>
        where TParent : IParentBehaviour<TParent, TChild>
        where TChild : IChildBehaviour<TChild, TParent>
    {
        protected Dictionary<Type, TChild> _components;

        // Used for getting components via Unity call, so to not make allocations every time we use that function
        protected static List<TChild> _cachedComponents;
        // Dictionary to map components to their interface pairs where the interface is derived from *TChild* type
        protected static Dictionary<Type, Type> _componentToInterfacePairs;

        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/> that uses concrete implementation as a key,
        /// and an <see langword="interface"/> that derives from <see cref="TChild"/> as a value.
        /// Value of the <see cref="Dictionary{TKey, TValue}"/> is the <see langword="interface"/> that 
        /// the key <see cref="Type"/> derives from, and that <see langword="interface"/> also implements <see cref="TChild"/>.
        /// </summary>
        /// <remarks>
        /// <b>Note that there must be only on <see langword="interface"/> that derives from <see cref="TChild"/>,
        /// but also a concrete <see langword="class"/> implements it.</b>
        /// <br></br>
        /// E.g 
        /// <br></br><see langword="interface"/> IInventoryBehaviour : <see cref="TChild"/>, 
        /// <br></br><see langword="interface"/> IInventoryInspector : <see cref="TChild"/>
        /// <br></br><see langword="class"/> InventoryHandler : IInventoryBehaviour, IInventoryInspector 
        /// <br></br>
        /// <b>This is not allowed!</b> 
        /// </remarks>
        protected static Dictionary<Type, Type> BuildComponentToInterfaceDictionary()
        {
            Type baseType = typeof(TChild);

            // Gather all concrete types(not interface) that derive from *baseType* (TChild) 
            var concreteImplementations = baseType.Assembly.GetTypes()
                .Where(type => type.IsInterface == false 
                    && baseType.IsAssignableFrom(type) 
                    && type != baseType)
                .ToArray();

            var componentToInterfacePairs = new Dictionary<Type, Type>(concreteImplementations.Length);

            foreach(var concreteImplementation in concreteImplementations)
            {
                // Get of all interfaces of the concrete type
                var componentInterfaces = concreteImplementation.GetInterfaces();
                foreach( var componentInterface in componentInterfaces)
                {
                    // If that interface is not *baseType* (TChild) but derives from it
                    // Then it is the interface we are looking for
                    if(componentInterface != baseType && baseType.IsAssignableFrom(componentInterface))
                    {
                        componentToInterfacePairs.Add(concreteImplementation, componentInterface);
                        break;
                    }
                }
            }

            return componentToInterfacePairs;
        }


        protected virtual void Awake() => _components = GetChildComponentsInChildren(gameObject);

        /// <summary>
        /// Can be used to manually find all of the child behaviours that are a child of <paramref name="root"/>.
        /// </summary>
        /// <remarks>
        /// <b>Overrides all of the contents, use cautiously.</b>
        /// </remarks>
        /// <param name="root">Parent object of all the child behaviours to look for.</param>
        protected static Dictionary<Type, TChild> GetChildComponentsInChildren(GameObject root)
        {
            if(_componentToInterfacePairs == null)
            {
                return new Dictionary<Type, TChild>();
            }

            root.GetComponentsInChildren<TChild>(false, _cachedComponents);

            var components = new Dictionary<Type, TChild>(_cachedComponents.Count);
            foreach (var component in _cachedComponents)
            {
                var interfaceType = _componentToInterfacePairs[component.GetType()];

#if UNITY_EDITOR
                if (components.TryAdd(interfaceType, component) == false)
                {
                    Debug.LogError($"2 child components of the same type '{component.GetType()}'" +
                        $" found under {root.name}", root);
                }
#else
                components.Add(interfaceType, component);
#endif
            }

            return components;
        }

        public abstract void AddBehaviour(TChild behaviour);

        public abstract void RemoveBehaviour(TChild behaviour);

        public bool TryGetCC<T>(out T component)
            where T : class, TChild
        {
            EnsureComponentsDictionary();

            if(_components.TryGetValue(typeof(T), out TChild characterComponent))
            {
                component = characterComponent as T;
                return true;
            }

            component = default;
            return false;
        }

        public T GetCC<T>()
            where T : class, TChild
        {
            EnsureComponentsDictionary();

            if(_components.TryGetValue(typeof(T), out TChild component))
            {
                return component as T;
            }

            return null;
        }

        public TChild GetCC(Type type)
        {
            EnsureComponentsDictionary();
            return _components.GetValueOrDefault(type);
        }

        private void EnsureComponentsDictionary()
        {
#if UNITY_EDITOR
            _components ??= GetChildComponentsInChildren(gameObject);
#endif
        }
    }
}
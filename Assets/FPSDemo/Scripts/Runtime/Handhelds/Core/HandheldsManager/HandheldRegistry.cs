using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldRegistry
    {
        /// <summary>
        /// List of currently registered <see cref="IHandheld"/>.
        /// </summary>
        IReadOnlyList<IHandheld> RegisteredHandhelds { get; }

        /// <summary>
        /// Registers <paramref name="handheld"/> to the registry. Instantiates a new 
        /// object if the passed <paramref name="handheld"/> is a prefab.
        /// </summary>
        /// <param name="handheld">Handheld to register (scene object or prefab).</param>
        /// <param name="disable">Should be disabled upon registering.</param>
        /// <returns>Handheld that is a Unity object.</returns>
        IHandheld RegisterHandheld(IHandheld handheld, bool disable = true);

        /// <summary>
        /// Unregisters <paramref name="handheld"/> from the handhelds registry.
        /// </summary>
        /// <param name="handheld">Handheld to unregister.</param>
        /// <param name="destroy">Destroy the <paramref name="handheld"/> after unregistering?</param>
        /// <returns>If unregister action was succesfully completed.</returns>
        bool UnregisterHandheld(IHandheld handheld, bool destroy = false);
        
        /// <param name="handheld">Handheld to query.</param>
        /// <returns>Is <paramref name="handheld"/> already registered.</returns>
        bool IsRegistered(IHandheld handheld);
    }

    public sealed class HandheldRegistry : IHandheldRegistry
    {
        private readonly List<IHandheld> _registeredHandhelds = new();
        private readonly Transform _spawnRoot;
        private readonly ICharacter _character;

        public IReadOnlyList<IHandheld> RegisteredHandhelds => _registeredHandhelds;

        public HandheldRegistry(Transform spawnRoot, ICharacter character)
        {
            _spawnRoot = spawnRoot ?? throw new ArgumentNullException(nameof(spawnRoot));
            _character = character ?? throw new ArgumentNullException(nameof(character));
        }

        public IHandheld RegisterHandheld(IHandheld handheld, bool disable = true)
        {
            if(handheld == null || _registeredHandhelds.Contains(handheld))
            {
                return handheld;
            }

            handheld = EnsureHandheldInstantiated(handheld);
            _registeredHandhelds.Add(handheld);
            handheld.SetCharacter(_character);

            ConfigureHandheldGameObject(handheld, !disable);

            return handheld;
        }

        /// <summary>
        /// Ensures <paramref name="handheld"/> is instantiated. If it is a prefab, creates a new object.
        /// </summary>
        /// <param name="handheld">Handheld to ensure (scene object or prefab).</param>
        /// <returns></returns>
        private IHandheld EnsureHandheldInstantiated(IHandheld handheld)
        {
            if(handheld.gameObject?.IsSceneObject() == false)
            {
                var instance = UnityEngine.Object.Instantiate(handheld.gameObject,
                    _spawnRoot.position, _spawnRoot.rotation, _spawnRoot);
                return instance.GetComponent<IHandheld>();
            }

            return handheld;
        }

        /// <summary>
        /// Configures the <see cref="GameObject"/> of <paramref name="handheld"/>.
        /// </summary>
        /// <param name="active">Should set active?</param>
        private static void ConfigureHandheldGameObject(IHandheld handheld, bool active)
        {
            if(handheld is not MonoBehaviour)
            {
                return;
            }

            if(handheld.gameObject.activeSelf != active)
            {
                handheld.gameObject.SetActive(active);
            }
        }

        public bool UnregisterHandheld(IHandheld handheld, bool destroy = false)
        {
            if(_registeredHandhelds.Remove(handheld) == false)
            {
                return false;
            }

            handheld.SetCharacter(null);

            if(destroy && handheld.gameObject != null)
            {
                UnityEngine.Object.Destroy(handheld.gameObject);
            }

            return true;
        }

        public bool IsRegistered(IHandheld handheld) => _registeredHandhelds.Contains(handheld);
    }
}
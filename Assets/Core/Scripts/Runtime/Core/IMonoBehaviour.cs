using System.Collections;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Provides an interface to access Unity's <see cref="MonoBehaviour"/> class.
    /// Prevents tight-coupling with Unity's <see cref="MonoBehaviour"/> and renders injection of classes easier.
    /// Makes testing, modding, plugin systems development easier by preventing coupling to Unity references.
    /// </summary>
    public interface IMonoBehaviour
    {
        /// <summary>
        /// <inheritdoc cref="Component.gameObject" path="/summary"/>
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// <inheritdoc cref="Component.transform" path="/summary"/>
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// <inheritdoc cref="Behaviour.enabled" path="/summary"/>
        /// </summary>
        bool enabled { get; }

        /// <summary>
        /// <inheritdoc cref="MonoBehaviour.StartCoroutine(IEnumerator)" path="/summary"/>
        /// </summary>
        Coroutine StartCoroutine(IEnumerator routine);

        /// <summary>
        /// <inheritdoc cref="MonoBehaviour.StopCoroutine(Coroutine)"/>
        /// </summary>
        void StopCoroutine(Coroutine routine);
    }
}
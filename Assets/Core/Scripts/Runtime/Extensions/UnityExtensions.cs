using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Nexora
{
    public static class UnityExtensions
    {
        /// <summary>
        /// Checks if the <see cref="UnityEngine.GameObject"/> of the component is a scene object.
        /// Returns false if it is a prefab or for a minor case when the 
        /// <see cref="UnityEngine.GameObject"/> is created but 
        /// <see cref="SceneManager.MoveGameObjectToScene(GameObject, Scene)"/> is not called.
        /// </summary>
        public static bool IsSceneObject(this Component component)
            => component != null && component.gameObject.scene.IsValid();

        /// <summary>
        /// For the case when <see cref="Component"/> might be already destroyed and <see cref="GameObject"/> is not.
        /// And for the case of <see cref="Object.DontDestroyOnLoad(Object)"/> if needed.
        /// </summary>
        public static bool IsSceneObject(this GameObject gameObject)
            => gameObject != null && gameObject.scene.IsValid();

        public static void SetLayerIncludingChildren(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerIncludingChildren(layer);
            }
        }

        public static bool IsLayerInMask(this GameObject gameObject, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << gameObject.layer));
        }

        public static bool ImplementsInterface(this GameObject gameObject, System.Type interfaceType)
        {
            Assert.IsTrue(interfaceType.IsInterface, 
                string.Format("Passed in type {0} is not an interface", interfaceType.Name));

            return gameObject.GetComponent(interfaceType) != null;
        }

        /// <summary>
        /// Returns string path of objects separated by "/", starting from source to the head in a clean upstream hierarchy.
        /// </summary>
        public static string BuildRelativePathFrom(this Transform head, Transform source, StringBuilder path)
        {
            path.Clear();

            if(head == source)
            {
                return string.Empty;
            }

            path.Append(source.name);
            Transform tail = source.parent;
            while(tail != null && tail != head)
            {
                path.Insert(0, tail.name + "/");
                tail = tail.parent;
            }

            // Path must be formed
            Assert.IsTrue(tail == head, string.Format("Path from {0} to {1} not exists", source, head));

            return path.ToString();
        }

        /// <summary>
        /// Checks if <see cref="Animator"/> is playing(not stopped) for most of the cases. 
        /// Cannot handle edge cases, because edge cases depend on other parameters.
        /// Namely the following situations may result in this code not correctly working:
        ///     - If there is an empty(zero duration) animation is placed, the animation will be at normalizedTime=1
        ///     but it would return true.
        ///     - If animator's paused state of concern, namely, if you want it to return false when 
        ///     animator is paused. 
        /// </summary>
        public static bool IsPlaying(this UnityEngine.Animator animator, int layerIndex = 0)
        {
            if(animator == null)
            {
                return false;
            }

            if(animator.enabled == false || animator.runtimeAnimatorController == false)
            {
                return false;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            bool hasStarted = stateInfo.normalizedTime > 0f;
            bool isNotFinished = stateInfo.loop || stateInfo.normalizedTime < 1f;
            bool isInTransition = animator.IsInTransition(layerIndex);

            return (hasStarted && isNotFinished) || isInTransition;
        }

        public static void PlayClip(this UnityEngine.Animation animation, AnimationClip clip)
        {
            animation.Stop();
            animation.clip = clip;
            animation.Play();
        }

        /// <summary>
        /// Eases the <see cref="AudioSource.volume"/> from <paramref name="startVolume"/> to <paramref name="targetVolume"/>
        /// in <paramref name="duration"/> seconds.
        /// </summary>
        public static IEnumerator EaseVolume(
            this AudioSource audioSource, 
            float startVolume,
            float targetVolume,
            float duration)
        {
            float passedTime = 0f;

            while (passedTime < 1f)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, passedTime);
                passedTime += Time.deltaTime / duration;
                yield return null;
            }
        }
    }

    public static class GameObjectExtensions
    {
        public static T AddComponent<T>(this Transform transform)
            where T : Component
        {
            return transform.gameObject.AddComponent<T>();
        }

        public static T GetComponentInRoot<T>(this GameObject gameObject)
            => gameObject.transform.root.GetComponentInChildren<T>();


        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            return gameObject.TryGetComponent<T>(out T component)
                ? component
                : gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Useful for situations when the type is not known beforehand, like getting it with reflection.
        /// Else use classic Unity implementation.
        /// </summary>
        public static Component GetOrAddComponent(this GameObject gameObject, System.Type type)
        {
            return gameObject.TryGetComponent(type, out var component)
                ? component
                : gameObject.AddComponent(type);
        }

        public static Base GetOrAddDerivedComponent<Base, Derived>(this GameObject gameObject)
            where Base : Component
            where Derived : Base
        {
            return gameObject.TryGetComponent<Base>(out var component)
                ? component
                : gameObject.AddComponent<Derived>();
        }

        /// <summary>
        /// Gets if the component of type is already added to the GameObject, if the type mismatch removes the old one and adds the Component
        /// </summary>
        public static Base GetOrAddDerivedComponent<Base>(this GameObject gameObject, System.Type derivedType)
            where Base : Component
        {
            Assert.IsTrue(typeof(Base).IsAssignableFrom(derivedType),
                string.Format("{0} is not a derived type of {1}", derivedType.ToString(), typeof(Base).ToString()));

            return GetOrSwapComponentOfType<Base>(gameObject, derivedType);
        }

        public static Base GetOrSwapComponentOfType<Base>(this GameObject gameObject, System.Type type)
            where Base : Component
        {
            if (gameObject.TryGetComponent<Base>(out var component))
            {
                if (component.GetType() != type)
                {
                    SafeDestroy(component);
                }
                else
                {
                    return component;
                }
            }

            return gameObject.AddComponent(type) as Base;
        }

        /// <summary>
        /// Safely destroy the passed component with respect to being in Editor(Play/Edit mode) or Runtime
        /// <see cref="Object.Destroy"/> allows Unity to handle cleanup at the end of the frame.
        /// <see cref="Object.DestroyImmediate"/> may cause crashes in Runtime and Play mode, as the cleanup can be done mid-frame.
        /// </summary>
        public static void SafeDestroy(Component component)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(component);
            }
            else
            {
                // If not in Play mode in the editor, this way we can use Undo and also avoid ghost components
                Object.DestroyImmediate(component);
            }
#else
            Object.Destroy(component);
#endif
        }

        public static bool HasComponent<T>(this GameObject gameObject)
            => gameObject.TryGetComponent<T>(out _);

        /// <summary>
        /// Searches parent/self/siblings/children for a component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the searched component.</typeparam>
        /// <param name="component">Owner of the hierarchy.</param>
        public static T GetComponentFromHierarchy<T>(this Component component)
            where T : class
        {
            var result = component.GetComponentInParent<T>();

            if (result == null && component.transform.parent != null)
            {
                result = component.transform.parent.GetComponentInDirectChildren<T>();
            }

            if (result == null)
            {
                result = component.GetComponentInChildren<T>();
            }

            return result;
        }

        /// <summary>
        /// Searches parent/self/siblings/children for components of type <typeparamref name="T"/>.
        /// </summary>
        /// <inheritdoc cref="GetComponentFromHierarchy{T}(Component)" path="/typeparam"/>
        /// <inheritdoc cref="GetComponentFromHierarchy{T}(Component)" path="/param"/>
        public static T[] GetComponentsFromHierarchy<T>(this Component component)
            where T : class
        {
            var result = new List<T>();

            AddParent();
            AddSiblings();
            AddChildren();

            return result.ToArray();

            void AddParent()
            {
                var parentComponent = component.GetComponentInParent<T>();

                if (parentComponent != null)
                {
                    result.Add(parentComponent);
                }
            }

            void AddSiblings()
            {
                if (component.transform.parent != null)
                {
                    result.AddRange(component.transform.parent.gameObject.GetComponentsInDirectChildren<T>());
                }
            }

            void AddChildren()
            {
                result.AddRange(component.GetComponentsInChildren<T>());
            }
        }

        /// <summary>
        /// Returns the first found component in direct children of the object in the hierarchial order
        /// </summary>
        public static T GetComponentInDirectChildren<T>(this GameObject gameObject, bool includeSelf = false)
            where T : class
        {
            return gameObject.transform.GetComponentInDirectChildren<T>(includeSelf);
        }

        /// <summary>
        /// <inheritdoc cref="GetComponentInDirectChildren" path="/summary"/>
        /// </summary>
        public static T GetComponentInDirectChildren<T>(this Transform transform, bool includeSelf = false)
            where T : class
        {
            if(includeSelf && transform.TryGetComponent<T>(out var selfComponent))
            {
                return selfComponent;
            }

            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a list with all Components of type T available in the direct children of the object in the hierarchial order
        /// </summary>
        public static List<T> GetComponentsInDirectChildren<T>(this GameObject gameObject, int capacity = 10, bool multipleOnSameObject = false)
        {
            var list = new List<T>(capacity);
            gameObject.transform.GetComponentsInDirectChildren<T>(list, multipleOnSameObject);
            return list;
        }

        /// <summary>
        /// Fills the list with all Components of type T available in the direct children of the object in the hierarchial order
        /// </summary>
        public static void GetComponentsInDirectChildren<T>(this Transform transform, List<T> list, bool multipleOnSameObject = false)
        {
            list.Clear();
            int childCount = transform.childCount;
            if (multipleOnSameObject)
            {
                for (int i = 0; i < childCount; i++)
                {
                    list.AddRange(transform.GetChild(i).GetComponents<T>());
                }
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    if (transform.GetChild(i).TryGetComponent<T>(out var component))
                    {
                        list.Add(component);
                    }
                }
            }
        }

        public static bool TryGetComponentInParent<T>(this IMonoBehaviour monoBehaviour, out T component)
            where T : class
        {
            return monoBehaviour.transform.TryGetComponentInParent<T>(out component);
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component)
            where T : class
        {
            return gameObject.transform.TryGetComponentInParent<T>(out component);
        }

        public static bool TryGetComponentInParent<T>(this Transform transform, out T component)
            where T : class
        {
            if(transform.parent == null)
            {
                component = null;
                return false;
            }

            return transform.parent.TryGetComponent<T>(out component);
        }
    }

    public static class IMonoBehaviourExtensions
    {
        public static T GetComponent<T>(this IMonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.GetComponent<T>();
        }

        public static T[] GetComponents<T>(this IMonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.GetComponents<T>();
        }

        public static T AddComponent<T>(this IMonoBehaviour monoBehaviour)
            where T : Component
        {
            return monoBehaviour.gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this IMonoBehaviour monoBehaviour)
            where T : Component
        {
            return monoBehaviour.gameObject.GetOrAddComponent<T>();
        }

        public static T GetComponentInChildren<T>(this IMonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.GetComponentInChildren<T>();
        }

        public static T[] GetComponentsInChildren<T>(this IMonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.GetComponentsInChildren<T>();
        }

        public static T GetComponentInDirectChildren<T>(this IMonoBehaviour monoBehaviour, bool includeSelf = false)
            where T : class
        {
            return monoBehaviour.gameObject.GetComponentInDirectChildren<T>(includeSelf);
        }

        public static T GetComponentInRoot<T>(this IMonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.GetComponentInRoot<T>();
        }
    }
}

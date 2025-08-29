using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using UnityEngine.Pool;

namespace Nexora.Experimental.Tweening
{
    /// <summary>
    /// Singleton tween system module that manages registration, unregistration, updating and tracking 
    /// target/tween relationships using registries. Uses pooling for performance via <see cref="TweenPoolRegistry"/>,
    /// and uses <see cref="TweenRegistry"/> for handling functionalities.
    /// </summary>
    internal sealed class TweenSystem : GameModule<TweenSystem>
    {
        private TweenUpdater _tweenUpdater;
        private TweenRegistry _tweenRegistry = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => CreateInstance();

        protected override void OnInitialized()
        {
            if (_tweenRegistry == null)
            {
                _tweenRegistry = new TweenRegistry();
            }

            _tweenRegistry.Initialize();
            InitializeTickHandler();
            TweenPoolRegistry.InitializePools();
        }

        private void InitializeTickHandler()
        {
            var root = CreateChildUnderModulesRoot("[TweenSystem]").gameObject;
            _tweenUpdater = root.AddComponent<TweenUpdater>();
            _tweenUpdater.OnTick += TickActiveTweens;
        }

        private void TickActiveTweens()
            => _tweenRegistry.TickActiveTweens();

        /// <summary>
        /// <inheritdoc cref="TweenRegistry.RegisterTween(ITween)" path="/summary"/>
        /// </summary>
        internal void RegisterTween(ITween tween)
            => _tweenRegistry.RegisterTween(tween);

        /// <summary>
        /// <inheritdoc cref="TweenRegistry.UnregisterTween(ITween)" path="/summary"/>
        /// </summary>
        internal bool UnregisterTween(ITween tween)
            => _tweenRegistry.UnregisterTween(tween);

        /// <summary>
        /// <inheritdoc cref="TweenRegistry.RegisterTweenTarget(ITween, UnityEngine.Object)" path="/summary"/>
        /// </summary>
        internal void RegisterTweenTarget(ITween tween, UnityEngine.Object oldTarget = null)
            => _tweenRegistry.RegisterTweenTarget(tween, oldTarget);

        /// <summary>
        /// <inheritdoc cref="TweenRegistry.StopAndClearTweensFromTarget(UnityEngine.Object, TweenResetBehaviour)" path="/summary"/>
        /// </summary>
        internal void StopAndClearTweensFromTarget(UnityEngine.Object target, TweenResetBehaviour tweenResetBehaviour)
            => _tweenRegistry.StopAndClearTweensFromTarget(target, tweenResetBehaviour);

        /// <summary>
        /// <inheritdoc cref="TweenRegistry.HasTweensAttached(UnityEngine.Object)" path="/summary"/>
        /// </summary>
        internal bool HasTweensAttached(UnityEngine.Object target)
            => _tweenRegistry.HasTweensAttached(target);

        internal static Tween<T> GetTween<T>()
            where T : struct
        {
            return TweenPoolRegistry.GetTween<T>();
        }

        /// <summary>
        /// <inheritdoc cref="TweenPoolRegistry.CreateAndRegisterTweenPool{T}(Type, Func{Tween{T}})" path="/summary"/> 
        /// </summary>
        internal static void AddTypeToTweenPoolRegistry<T>(Type valueType, Func<Tween<T>> factory)
            where T : struct
        {
            TweenPoolRegistry.CreateAndRegisterTweenPool<T>(valueType, factory);
        }

        /// <summary>
        /// To give tween system a way to tick registered tweens.
        /// </summary>
        private sealed class TweenUpdater : MonoBehaviour
        {
            public event UnityAction OnTick;

            private void Update() => OnTick?.Invoke();
        }

        /// <summary>
        /// Registry for the all tweens. Registers/Unregisters and keeps track of the all active tweens.
        /// It also controls the attached tweens to the targets, cleaning them, attaching them.
        /// </summary>
        /// <remarks>
        /// Keeps a <see cref="List{ITween}"/> cache, these lists are assigned to the targets when a tween assigned.
        /// Keeps the registry to track which tweens are attached to which objects, this way it is easy to control 
        /// the both <see cref="Tween{T}"/> and target <see cref="UnityEngine.Object"/>.
        /// </remarks>
        private sealed class TweenRegistry
        {
            private const int DefaultTweenCount = 20;
            private const int DefaultTweenListCount = 10;

            private readonly List<ITween> _activeTweens = new(DefaultTweenCount);
            private readonly Dictionary<UnityEngine.Object, List<ITween>> _attachedTweenRegistry = new(DefaultTweenCount);
            private readonly Stack<List<ITween>> _tweenListCache = new(DefaultTweenListCount);

            /// <summary>
            /// Resets all tween containers.
            /// </summary>
            public void Initialize()
            {
                _activeTweens.Clear();
                _tweenListCache.Clear();
                _attachedTweenRegistry.Clear();

                for (int i = 0; i < DefaultTweenListCount; i++)
                {
                    _tweenListCache.Push(new List<ITween>(DefaultTweenListCount));
                }
            }

            /// <summary>
            /// Moves the progress of all active tweens.
            /// </summary>
            public void TickActiveTweens()
            {
                foreach (var tween in _activeTweens.AsReverseEnumerator())
                {
                    tween.Tick();
                }
            }

            /// <summary>
            /// Registers tween as active tween.
            /// </summary>
            public void RegisterTween(ITween tween)
            {
                _activeTweens.Add(tween);
            }

            /// <summary>
            /// Unregisters tween from the active list, if target has no tweens attached removes it from the map.
            /// </summary>
            public bool UnregisterTween(ITween tween)
            {
                if (_activeTweens.Remove(tween) == false)
                {
                    return false;
                }

                RemoveTweenFromTarget(tween.Target, tween);

                return true;
            }

            /// <summary>
            /// Removes the <paramref name="tween"/> from <paramref name="target"/>'s tween list.
            /// </summary>
            private void RemoveTweenFromTarget(UnityEngine.Object target, ITween tween)
            {
                if(target != null 
                && _attachedTweenRegistry.TryGetValue(target, out var list) 
                && list.Remove(tween)
                && list.IsEmpty())
                {
                    _attachedTweenRegistry.Remove(target);
                    ReleaseTweenList(list);
                }
            }

            private void ReleaseTweenList(List<ITween> tweenList)
            {
                tweenList.Clear();
                _tweenListCache.Push(tweenList);
            }

            /// <summary>
            /// Removes the <paramref name="tween"/> from the <paramref name="oldTarget"/> and
            /// attaches it to the new target assigned to it. If the new target does not exist
            /// in the registry yet, it creates a new record for that.
            /// </summary>
            public void RegisterTweenTarget(ITween tween, UnityEngine.Object oldTarget)
            {
                RemoveTweenFromTarget(oldTarget, tween);

                var newTarget = tween.Target;
                if (newTarget == null) 
                {
                    return;
                }

                if(_attachedTweenRegistry.TryGetValue(newTarget, out var newList) == false)
                {
                    newList = GetOrCreateTweenList();
                    _attachedTweenRegistry.Add(newTarget, newList);
                }

                newList.Add(tween);
            }

            private List<ITween> GetOrCreateTweenList()
                => _tweenListCache.IsEmpty() == false ? _tweenListCache.Pop() : new List<ITween>(DefaultTweenListCount);

            /// <summary>
            /// Stops and clears all tweens registered on the <paramref name="target"/>, 
            /// resets the tween values according to <paramref name="tweenResetBehaviour"/>.
            /// </summary>
            public void StopAndClearTweensFromTarget(UnityEngine.Object target, TweenResetBehaviour tweenResetBehaviour)
            {
                if(target == null)
                {
                    return;
                }

                if(_attachedTweenRegistry.TryGetValue(target, out var list))
                {
                    _attachedTweenRegistry.Remove(target);

                    foreach(var tween in list)
                    {
                        tween.Release(tweenResetBehaviour);
                    }

                    ReleaseTweenList(list);
                }
            }

            /// <summary>
            /// Returns if any tween is attached to the <paramref name="target"/>.
            /// </summary>
            public bool HasTweensAttached(UnityEngine.Object target)
                => _attachedTweenRegistry.TryGetValue(target, out var list) && list.IsEmpty() == false;
        }

        /// <summary>
        /// Registry to keep track of the tween pools of different value types. 
        /// Manages getting/releasing tweens of various types.
        /// </summary>
        private sealed class TweenPoolRegistry
        {
            private const string PoolNotValidMessage = "Pool for the passed type {0} is not properly added.";
            private const string PoolNotExistsMessage = 
                "Passed type {0} is not compatible with the tween system. It can be added in the pools creation method";

            private const int DefaultPoolSize = 10;
            private const int MaxPoolSize = 20;

            private static Dictionary<Type, IDisposable> _tweenPools;

            public static Dictionary<Type, IDisposable> TweenPools => _tweenPools;

            public static void InitializePools()
            {
                if( _tweenPools != null)
                {
                    DisposePools();
                }

                CreateDefaultTweenPools();
            }

            /// <summary>
            /// Creates the default pools for various types. 
            /// </summary>
            /// <remarks>
            /// If you want to add a new type to support,
            /// then you need to define a class derived from <see cref="Tween{T}"/> and manually call
            /// <see cref="CreateAndRegisterTweenPool{T}(Type, Func{Tween{T}})"/> with the appropriate factory method
            /// for your type.
            /// </remarks>
            public static void CreateDefaultTweenPools()
            {
                _tweenPools = new Dictionary<Type, IDisposable>()
                {
                    [typeof(int)] = new ObjectPool<Tween<int>>(() => new IntTween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize),
                    [typeof(float)] = new ObjectPool<Tween<float>>(() => new FloatTween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize),
                    [typeof(Vector3)] = new ObjectPool<Tween<Vector3>>(() => new Vector3Tween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize),
                    [typeof(Vector2)] = new ObjectPool<Tween<Vector2>>(() => new Vector2Tween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize),
                    [typeof(Quaternion)] = new ObjectPool<Tween<Quaternion>>(() => new QuaternionTween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize),
                    [typeof(Color)] = new ObjectPool<Tween<Color>>(() => new ColorTween(), defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize)
                };
            }

            /// <summary>
            /// Gets a <see cref="Tween{T}"/> from the the tween pool of type <typeparamref name="T"/>.
            /// Logs error if the pool object is not compatible or the type does not have a valid pool.
            /// </summary>
            public static Tween<T> GetTween<T>()
                where T : struct
            {
                Type valueType = typeof(T);

                if(_tweenPools.TryGetValue(valueType, out var pool))
                {
                    if (pool is ObjectPool<Tween<T>> objectPool)
                    {
                        return objectPool.Get();
                    }

                    Debug.LogError(string.Format(PoolNotValidMessage, valueType.Name));
                }
                else
                {
                    Debug.LogError(string.Format(PoolNotExistsMessage, valueType.Name));
                }

                return null;
            }

            /// <summary>
            /// Releases <paramref name="tween"/> from the the tween pool of type <typeparamref name="T"/>.
            /// Logs error if the pool object is not compatible or the type does not have a valid pool.
            /// </summary>
            public static void ReleaseTween<T>(Tween<T> tween)
                where T : struct
            {
                Type valueType = typeof(T);

                if(_tweenPools.TryGetValue(valueType, out var pool))
                {
                    if(pool is  ObjectPool<Tween<T>> objectPool)
                    {
                        objectPool.Release(tween);
                    }

                    Debug.LogError(string.Format(PoolNotValidMessage, valueType.Name));
                }
                else
                {
                    Debug.LogError(string.Format(PoolNotExistsMessage, valueType.Name));
                }
            }

            /// <summary>
            /// Disposes all available pools and clears the cache.
            /// </summary>
            public static void DisposePools()
            {
                foreach (var pool in _tweenPools.Values)
                {
                    pool.Dispose();
                }

                _tweenPools.Clear();
            }

            /// <summary>
            /// Creates a tween pool for <paramref name="poolObjectType"/>, and adds to the <see cref="_tweenPools"/>.
            /// If a factory method is not passed for creation of the tween type then checks if it's already available in the defaults.
            /// Throws if the type is not found.
            /// </summary>
            public static void CreateAndRegisterTweenPool<T>(Type poolObjectType, Func<Tween<T>> factory = null)
                where T : struct
            {
                if(factory == null)
                {
                    factory = () => CreateDefaultTween<T>();
                }

                var newPool = new ObjectPool<Tween<T>>(factory, defaultCapacity: DefaultPoolSize, maxSize: MaxPoolSize);
                _tweenPools.Add(poolObjectType, newPool);
            }

            /// <summary>
            /// Can be used for dynamically adding factory method if Runtime type does not exist in default initialization method.
            /// </summary>
            /// <exception cref="NotSupportedException"></exception>
            private static Tween<T> CreateDefaultTween<T>()
                where T : struct
            {
                return typeof(T) switch
                {
                    Type t when t == typeof(int) => (Tween<T>)(object)new IntTween(),
                    Type t when t == typeof(float) => (Tween<T>)(object)new FloatTween(),
                    Type t when t == typeof(Quaternion) => (Tween<T>)(object)new QuaternionTween(),
                    Type t when t == typeof(Vector3) => (Tween<T>)(object)new Vector3Tween(),
                    Type t when t == typeof(Vector2) => (Tween<T>)(object)new Vector2Tween(),
                    Type t when t == typeof(Color) => (Tween<T>)(object)new ColorTween(),
                    _ => throw new NotSupportedException($"No default tween implementation for {typeof(T)}")
                };
            }
        }
    }
}
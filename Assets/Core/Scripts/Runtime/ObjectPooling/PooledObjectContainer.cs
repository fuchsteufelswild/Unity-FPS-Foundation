using Nexora.SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Scene object that contains <see cref="IPooledObject{T}"/> children that are <b>already created in editor</b>, 
    /// manages pooling, saving, loading those children.
    /// </summary>
    public class PooledObjectContainer<T> : 
        MonoBehaviour,
        IUnityPool<T>,
        ISaveableComponent
        where T : Component,
                  IPooledObject<T>
    {
        [SerializeField, DisableInPlayMode]
        private bool _resetPositionsOnRelease;

        [EditorButton(nameof(ResetFreeObjects))]
        [EditorButton(nameof(ResetAllObjects))]

        [SerializeField, DisableInPlayMode]
        [ShowIf(nameof(_resetPositionsOnRelease), true)]
        private bool _resetPhysicsOnRelease;

        private T[] _allPooledObjects;
        private readonly Stack<T> _freePooledObjects = new();
        private LocalTransformSnapshot[] _initialPooledObjectPositions;

        public int FreeObjectCount => _freePooledObjects.Count;

        public int MaximumCapacity => _allPooledObjects.Length;

        private void Awake()
        {
            _allPooledObjects = GetComponentsInChildren<T>();
            foreach(var pooledObject in _allPooledObjects)
            {
                pooledObject.SetParentPool(this, 0);
            }

            _initialPooledObjectPositions = _resetPositionsOnRelease 
                ? ExtractSnapshots() 
                : Array.Empty<LocalTransformSnapshot>();
        }

        private LocalTransformSnapshot[] ExtractSnapshots()
        {
            var snapshots = new LocalTransformSnapshot[_allPooledObjects.Length];

            for (int i = 0; i < _allPooledObjects.Length; i++)
            {
                snapshots[i] = new LocalTransformSnapshot(_allPooledObjects[i].transform);
            }

            return snapshots;
        }

        public T AcquireAndPut(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Acquire();
            if (instance == null)
            {
                return null;
            }

            var instanceTransform = instance.transform;
            instanceTransform.SetPositionAndRotation(position, rotation);
            if (parent != null)
            {
                instanceTransform.SetParent(parent);
            }

            return instance;
        }

        public T Acquire()
        {
            if(_freePooledObjects.TryPop(out var pooledObject))
            {
                AcquireObject(pooledObject);
                return pooledObject;
            }

            return null;
        }

        private void AcquireObject(T pooledObject)
        {
            pooledObject.OnPreAcquired();
            pooledObject.OnAcquired();
        }

        public void Release(T instance)
        {
            _freePooledObjects.Push(instance);
            if (_resetPositionsOnRelease)
            {
                int index = _allPooledObjects.IndexOf(instance);
                instance.transform.SetPositionAndRotation(
                    _initialPooledObjectPositions[index].Position,
                    _initialPooledObjectPositions[index].Rotation);

                if (_resetPhysicsOnRelease && instance.TryGetComponent<Rigidbody>(out var rigidbody))
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }

            instance.gameObject.SetActive(false);
        }

        public void Warmup(int count)
        {
            
        }

        private void ResetAllObjects()
        {
            while (_freePooledObjects.Count > 0)
            {
                Acquire();
            }
        }

        private void ResetFreeObjects()
        {
            _freePooledObjects.Clear();

            foreach (var pooledObject in _allPooledObjects)
            {
                AcquireObject(pooledObject);
            }
        }


        public void Dispose() => Clear();
        public void Clear() => _freePooledObjects.Clear();

        #region Saving
        private sealed class SaveData
        {
            public LocalTransformSnapshot[] Snapshots;
            public bool[] ObjectFreeStatus;
        }

        object ISaveableComponent.Save()
        {
            bool[] objectFreeStatus = new bool[_allPooledObjects.Length];

            for(int i = 0; i < _allPooledObjects.Length; i++)
            {
                objectFreeStatus[i] = _allPooledObjects[i].gameObject.activeSelf;
            }

            return new SaveData
            {
                Snapshots = ExtractSnapshots(),
                ObjectFreeStatus = objectFreeStatus
            };
        }
        
        void ISaveableComponent.Load(object data)
        {
            var saveData = (SaveData)data;
            LocalTransformSnapshot[] snapshots = saveData.Snapshots;
            bool[] objectFreeStatus = saveData.ObjectFreeStatus;

            for(int i = 0; i < _allPooledObjects.Length; i++)
            {
                var pooledObject = _allPooledObjects[i];

                if (objectFreeStatus[i] == false)
                {
                    pooledObject.Release();
                }

                LocalTransformSnapshot.ApplySnapshot(in snapshots[i], pooledObject.transform);
            }
        }
        #endregion
    }
}
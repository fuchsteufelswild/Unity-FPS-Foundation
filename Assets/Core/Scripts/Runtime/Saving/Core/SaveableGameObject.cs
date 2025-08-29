using Nexora.ObjectPooling;
using Nexora.Serialization;
using System.Text;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// A Unity object that is saveable, and can work with <see cref="IPooledObject{T}"/>. Responsible for saving all child components
    /// that implements <see cref="ISaveableComponent"/>, along with specific list of <see cref="Transform"/>
    /// and <see cref="Rigidbody"/>.
    /// <br></br>
    /// Listens to events fired by <see cref="IPooledObject{T}"/> for the case when <see cref="GameObject"/> 
    /// this component being part of is pooled.
    /// <br></br>
    /// Keeps track of prefabGuid along with object guid, so that for prefabs it can be initialized when the game is loaded.
    /// </summary>
    [AddComponentMenu(ComponentMenuPaths.SaveSystem + nameof(SaveableGameObject))]
    public class SaveableGameObject : 
        StableGuidComponent,
        IUnityObjectPoolListener
    {
        [SerializeField]
        [UnityGuidUIOptions(disableForPrefabs: false, hasNewGuidButton: false)]
        private UnityGuid _prefabGuid;

        [SerializeField]
#if UNITY_EDITOR
        [OnValueChanged(nameof(OnSaveableStatusChanged))]
#endif
        private bool _isSaveable = true;

        private bool _isRegistered;

        [SerializeField, ReorderableList(HasLabels = false)]
        private Rigidbody[] _rigidbodiesToSave;

        [SerializeField, NewLabel("Transforms To Save"), ReorderableList(HasLabels = false)]
        private Transform[] _childrenToSave;

        [SerializeField, SpaceArea(spaceBefore: 3f)]
        private LocalTransformSnapshot.Components _rootSaveComponents =
            LocalTransformSnapshot.Components.Position | LocalTransformSnapshot.Components.Rotation;

        public UnityGuid PrefabGuid
        {
            get => _prefabGuid;
            set => _prefabGuid = value;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Handles saveable status change in the editor, results in Register/Unregister of the object
        /// Check <see cref="IsSaveable"/> set for the resulting effect.
        /// </summary>
        private void OnSaveableStatusChanged() => IsSaveable = !_isSaveable;
#endif

        /// <summary>
        /// Gets the current saveable status.
        /// </summary>
        /// <remarks>Handles registration/unregistration of the object when saveable status is set.</remarks>
        public bool IsSaveable
        {
            get => _isSaveable;
            set
            {
                if(_isSaveable == value)
                {
                    return;
                }

                _isSaveable = value;

                if(_isSaveable)
                {
                    RegisterSaveableIfNeeded();
                }
                else
                {
                    UnregisterSaveable();
                }
            }
        }

        private void RegisterSaveableIfNeeded()
        {
            if(_isRegistered || Application.isPlaying == false || _isSaveable == false)
            {
                return;
            }

            _isRegistered = true;

            if(SceneSaveController.TryGetSceneSaveController(gameObject.scene, out var sceneSaveController))
            {
                sceneSaveController.RegisterSaveable(this);
            }
        }

        private void Start() => RegisterSaveableIfNeeded();
        protected override void OnDestroy()
        {
            UnregisterSaveable();
            base.OnDestroy();
        }

        public void OnAcquired() => RegisterSaveableIfNeeded();
        public void OnReleased() => UnregisterSaveable();
        public void OnPreAcquired() { }
        public void OnPreReleased() { }

        private void UnregisterSaveable()
        {
            if(_isRegistered == false)
            {
                return;
            }

            _isRegistered = false;
            if(SceneSaveController.TryGetSceneSaveController(gameObject.scene, out var sceneSaveController))
            {
                sceneSaveController.UnregisterSaveable(this);
            }
        }

        public GameObjectSaveData CaptureState(StringBuilder path)
        {
            return new GameObjectSaveData
            {
                PrefabGuid = PrefabGuid,
                InstanceGuid = StableGuid,
                LocalTransformSnapshot = new LocalTransformSnapshot(transform),
                ComponentSnapshots = UnityComponentSnapshot.ExtractSnapshots(transform, path),
                ChildrenLocalTransformSnapshots = LocalTransformSnapshot.ExtractSnaphots(_childrenToSave),
                RigidbodySnapshots = RigidbodySnapshot.ExtractSnapshots(_rigidbodiesToSave)
            };
        }

        public void RestoreState(GameObjectSaveData saveData)
        {
            StableGuid = saveData.InstanceGuid;

            UnityComponentSnapshot.ApplySnapshots(saveData.ComponentSnapshots, transform);
            LocalTransformSnapshot.ApplySnapshot(saveData.LocalTransformSnapshot, transform, _rootSaveComponents);
            LocalTransformSnapshot.ApplySnapshots(saveData.ChildrenLocalTransformSnapshots, _childrenToSave);
            RigidbodySnapshot.ApplySnapshots(saveData.RigidbodySnapshots, _rigidbodiesToSave);
        }
    }
}
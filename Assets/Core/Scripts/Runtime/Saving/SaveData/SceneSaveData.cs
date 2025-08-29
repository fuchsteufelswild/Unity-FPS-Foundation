using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Save data for a scene, contains name of the scene and all the <see cref="ISaveableComponent"/> data.
    /// </summary>
    [Serializable]
    public class SceneSaveData
    {
        [SerializeField]
        private string _sceneName;

        [SerializeField]
        private GameObjectSaveData[] _gameObjectSaveDataArray;

        public string SceneName => _sceneName;
        public GameObjectSaveData[] GameObjectSaveDataArray => _gameObjectSaveDataArray;

        public SceneSaveData(string sceneName, GameObjectSaveData[] containedObjectsSaveData)
        {
            _sceneName = sceneName;
            _gameObjectSaveDataArray = containedObjectsSaveData;
        }
    }
}
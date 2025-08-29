using Nexora.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.SaveSystem
{
    using SerializedDateTime = Nexora.Serialization.SerializedDateTime;

    /// <summary>
    /// Metadata for save game, includes id, time, thumbnail etc.
    /// </summary>
    [Serializable]
    public class GameSaveMetadata
    {
        [SerializeField]
        private UnityGuid _saveId;

        [SerializeField]
        private int _saveIndex;

        [SerializeField]
        private string _sceneName;

        [SerializeField]
        private string _levelName;

        [SerializeField]
        private SerializedDateTime _saveTime;

        [SerializeField]
        private SerializedTexture2D _thumbnail;

        [SerializeField]
        private SceneContextData _sceneContextData;

        /// <summary>
        /// Unique id for the save.
        /// </summary>
        public Guid SaveId => _saveId;
        public int SaveIndex => _saveIndex;
        public string SceneName => _sceneName;
        public string LevelName => _levelName;
        public DateTime SaveTime => _saveTime;
        public Texture2D Thumbnail => _thumbnail;

        private GameSaveMetadata() { }

        public GameSaveMetadata(
            Guid saveId, 
            int saveIndex, 
            string sceneName, 
            string levelName, 
            DateTime saveTime, 
            Texture2D thumbnail, 
            Dictionary<string, object> sceneData)
        {
            _saveId = new UnityGuid(saveId);
            _saveIndex = saveIndex;
            _sceneName = sceneName;
            _levelName = levelName;
            _saveTime = new SerializedDateTime(saveTime);
            _thumbnail = new SerializedTexture2D(thumbnail);
            _sceneContextData = new SceneContextData(sceneData);
        }

        public bool TryGetSavedContextValue<T>(string key, out T value)
            => _sceneContextData.TryGetValue(key, out value);

        [Serializable]
        private sealed class SceneContextData
        {
            [SerializeField]
            private SerializedDictionary<string, string> _sceneData;

            public SerializedDictionary<string, string> SceneData => _sceneData;

            public SceneContextData(Dictionary<string, object> sceneData)
                => Serialize(sceneData);

            private void Serialize(Dictionary<string, object> sceneData)
            {
                _sceneData = new SerializedDictionary<string, string>();
                if (sceneData == null)
                {
                    return;
                }

                foreach (var (key, value) in sceneData)
                {
                    if (value != null)
                    {
                        string json = JsonUtility.ToJson(value);
                        _sceneData.Add(key, json);
                    }
                }
            }

            public bool TryGetValue<T>(string key, out T value)
            {
                if (_sceneData.TryGetValue(key, out var jsonData))
                {
                    value = JsonUtility.FromJson<T>(jsonData);
                    return true;
                }

                value = default;
                return false;
            }
        }
    }
}
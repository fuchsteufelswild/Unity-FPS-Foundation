using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Representing data for the current's game whole save. Includes metadata <see cref="GameSaveMetadata"/>,
    /// and all available scenes' data <see cref="SceneSaveData"/>.
    /// Hierarchy is as follows: <see cref="GameSaveData"/> -> <see cref="SceneSaveData"/> -> <see cref="GameObjectSaveData"/>
    /// -> Various Snapshots
    /// </summary>
    [Serializable]
    public class GameSaveData 
    {
        [SerializeField]
        private GameSaveMetadata _saveMetadata;

        [SerializeField]
        private SceneSaveData[] _sceneSaveDataArray;

        public GameSaveMetadata SaveMetadata => _saveMetadata;
        public SceneSaveData[] SceneSaveDataArray => _sceneSaveDataArray;

        public GameSaveData(GameSaveMetadata saveMetadata, params SceneSaveData[] sceneSaveDataArray)
        {
            _saveMetadata = saveMetadata;
            _sceneSaveDataArray = sceneSaveDataArray;
        }
    }
}

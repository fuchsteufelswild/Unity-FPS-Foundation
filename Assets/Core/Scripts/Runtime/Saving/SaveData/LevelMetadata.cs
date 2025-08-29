using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Represents metadata of a level. Includes associated scene, description, thumbnail, and loading image. 
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Level Metadata", fileName = "New Level Metadata")]
    public class LevelMetaData : ScriptableObject
    {
        [SerializeField]
        private SerializedScene _scene;

        [SerializeField]
        private string _name;

        [SerializeField]
        private string _loadingScreenDescription;

        [SerializeField]
        private Sprite _thumbnail;

        [SerializeField]
        private Sprite _loadingScreenImage;

        public string LevelName => _name;
        public string SceneName => _scene.SceneName;
        public int SceneBuildIndex => _scene.BuildIndex;
        public string LoadingScreenDescription => _loadingScreenDescription;
        public Sprite Thumbnail => _thumbnail;
        public Sprite LoadingScreenImage => _loadingScreenImage;


    }
}

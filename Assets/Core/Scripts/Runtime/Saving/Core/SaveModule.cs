using System;
using UnityEngine;
using UnityEngine.SceneManagement;

using static Nexora.SaveSystem.SaveLoadHandler;


namespace Nexora.SaveSystem
{
    //[DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [CreateAssetMenu(menuName = CreateMenuPath + "Save Module", fileName = nameof(SaveModule))]
    public sealed class SaveModule : GameModule<SaveModule>
    {
        /// <summary>
        /// Defines how should save thumbnail should be get.
        /// </summary>
        public enum ThumbnailExtractMethod
        {
            /// <summary>
            /// It is already existent in the metadata object.
            /// </summary>
            Predefined,

            /// <summary>
            /// Current screenshot of the screen is saved as a save thumbnail.
            /// </summary>
            InGameScreenshot
        }

        [Tooltip("Determines how to extract thumbnail for the game save.")]
        [SerializeField]
        private ThumbnailExtractMethod _thumbnailExtractMethod;

        [Title("Scene Transition Handler")]
        [SerializeReference, ReferencePicker]
        private ISceneTransitionHandler _sceneTransitionHandler;

        [Tooltip("Fade screen prefab to use at saving operations.")]
        [MustImplementInterface(typeof(IScreenFadeTransition))]
        [SerializeField, PrefabObjectOnly]
        private GameObject _fadeTransitionPrefab;

        [Space, Title("SaveLoad Handler Parts")]
        [SerializeReference, ReferencePicker]
        private IFileSystem _fileSystem;
        [SerializeReference, ReferencePicker]
        private ISaveIndexValidator _saveIndexValidator;
        [SerializeReference, ReferencePicker]
        private ISavePathProvider _savePathProvider;
        [SerializeReference, ReferencePicker]
        private ISerializer _serializer;

        private ThumbnailGenerator _thumbnailGenerator;
        private GameSession _currentGameSession;
        private SaveLoadHandler _saveLoadHandler;
        
        public Guid CurrentGameID => _currentGameSession?.GameID ?? Guid.Empty;
        public bool IsLoadingOrSaving => _sceneTransitionHandler?.IsOperationInProgress ?? false;
        public bool IsCurrentGameValid => _currentGameSession?.IsValid ?? false;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
            InitializeComponents();
            InitializeGameSessionInEditor();

            // Fade out played before the menu screen
            _sceneTransitionHandler?.FadeScreen();
        }

        private void InitializeComponents()
        {
            var rootObject = CreateChildUnderModulesRoot("[SaveModuleRuntime]");
            _sceneTransitionHandler.Initialize(rootObject.gameObject, _fadeTransitionPrefab);

            _savePathProvider.Initialize();
            _saveLoadHandler = new SaveLoadHandler(_fileSystem, _serializer, _savePathProvider, _saveIndexValidator);

            _thumbnailGenerator = new ThumbnailGenerator(_thumbnailExtractMethod);
        }


        /// <summary>
        /// Creates a dummy <see cref="GameSession"/> every time we go into play mode in the Editor.
        /// </summary>
        /// <remarks>
        /// This is required as there is a case of loading a scene other than menu scene (without any save or create) 
        /// This way we ensure there is a valid game ID.
        /// </remarks>
        private void InitializeGameSessionInEditor()
        {
#if UNITY_EDITOR
            _sceneTransitionHandler.SceneLoader.InvokeNextFrame(TryCreateGameSession);
#endif

            return;

            void TryCreateGameSession()
            {
                var gameID = SceneSaveController.TryGetSceneSaveController(SceneManager.GetActiveScene(), out _)
                    ? Guid.NewGuid() 
                    : Guid.Empty;

                _currentGameSession = new GameSession(gameID);
            }
        }

        /// <summary>
        /// Creates a new game and loads the specified scene.
        /// </summary>
        /// <param name="sceneName">Scene to load as a first scene of the game.</param>
        public bool CreateGame(string sceneName)
        {
            if(LoadScene(sceneName) == false)
            {
                return false;
            }

            _currentGameSession = new GameSession(Guid.NewGuid());
            return true;
        }

        public bool LoadScene(string sceneName)
        {
            SceneValidator.ThrowIfSceneDoesNotExist(sceneName);

            if (IsLoadingOrSaving)
            {
                return false;
            }

            return _sceneTransitionHandler.LoadScene(sceneName);
        }

        /// <summary>
        /// Closes the current game and loads the specified scene.
        /// </summary>
        /// <param name="sceneName">Scene to load after closing the current game.</param>
        public bool CloseCurrentGame(string sceneName)
        {
            SceneValidator.ThrowIfSceneDoesNotExist(sceneName);

            if (IsLoadingOrSaving)
            {
                return false;
            }

            _currentGameSession = new GameSession(Guid.Empty);
            return _sceneTransitionHandler.LoadScene(sceneName);
        }

        public void FadeInAndQuitApplication()
        {
            if(IsLoadingOrSaving)
            {
                return;
            }

            _sceneTransitionHandler?.FadeInAndQuitApplication();
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void OnGameLoaded(Guid gameID) => _currentGameSession = new GameSession(gameID);

        public LoadResult<GameSaveData> LoadGame(int saveFileIndex)
        {
            if(IsLoadingOrSaving)
            {
                return LoadResult<GameSaveData>.Failure("Currently another save/load operation is in progress.");
            }

            return _saveLoadHandler.LoadGame(saveFileIndex);
        }

        public SaveResult SaveCurrentGame(int saveFileIndex) => SaveCurrentGame(saveFileIndex, out _);

        public SaveResult SaveCurrentGame(int saveFileIndex, out GameSaveMetadata metadata)
        {
            if(IsLoadingOrSaving || IsCurrentGameValid == false)
            {
                metadata = null;
                return SaveResult.Failure(IsLoadingOrSaving 
                    ? "Currently another save/load operation in progress."
                    : "Game session is not valid.");
            }

            GameSaveData saveData = GenerateGameSaveData(saveFileIndex);
            metadata = saveData.SaveMetadata;

            return _saveLoadHandler.SaveGame(saveData, saveFileIndex);
        }

        private GameSaveData GenerateGameSaveData(int saveFileIndex)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneSaveController currentSceneSaveController
                = SceneSaveController.GetSceneSaveController(activeScene);

            SceneSaveData sceneSaveData = currentSceneSaveController.CaptureSceneState();
            Texture2D thumbnail = _thumbnailGenerator.GetThumbnail(currentSceneSaveController);

            GameSaveMetadata gameSaveMetaData = new GameSaveMetadata(
                CurrentGameID,
                saveFileIndex,
                currentSceneSaveController.Scene.name,
                currentSceneSaveController.LevelMetaData.LevelName,
                DateTime.Now,
                thumbnail,
                currentSceneSaveController.CollectSceneContextData());

            return new GameSaveData(gameSaveMetaData, sceneSaveData);
        }

        private sealed class GameSession
        {
            public Guid GameID { get; }
            public bool IsValid => GameID != Guid.Empty;

            public GameSession(Guid gameID) => GameID = gameID;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if (_fileSystem == null)
            {
                UnityEngine.Debug.LogWarning($"FileSystem not configured in {name}");
            }

            if (_saveIndexValidator == null)
            {
                UnityEngine.Debug.LogWarning($"SaveIndexValidator not configured in {name}");
            }

            if (_savePathProvider == null)
            {
                UnityEngine.Debug.LogWarning($"SavePathProvider not configured in {name}");
            }

            if (_serializer == null)
            {
                UnityEngine.Debug.LogWarning($"Serializer not configured in {name}");
            }

            if (_sceneTransitionHandler == null)
            {
                UnityEngine.Debug.LogWarning($"Scene transition handler is not configured in {name}");
            }
        }
    }

    /// <summary>
    /// Generates thumbnail texture for a save.
    /// </summary>
    public class ThumbnailGenerator
    {
        private readonly SaveModule.ThumbnailExtractMethod _thumbnailExtractMethod;

        public ThumbnailGenerator(SaveModule.ThumbnailExtractMethod thumbnailExtractMethod) 
            => _thumbnailExtractMethod = thumbnailExtractMethod;

        public Texture2D GetThumbnail(SceneSaveController sceneSaveController)
        {
            return _thumbnailExtractMethod switch
            {
                SaveModule.ThumbnailExtractMethod.Predefined => GetSceneThumbnail(sceneSaveController),
                SaveModule.ThumbnailExtractMethod.InGameScreenshot => TakeScreenshot(),
                _ => null
            };
        }

        private Texture2D GetSceneThumbnail(SceneSaveController sceneSaveController)
        {
            return sceneSaveController.LevelMetaData?.Thumbnail.texture ?? TakeScreenshot();
        }

        private static Texture2D TakeScreenshot()
        {
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            var rect = new Rect(0, 0, Screen.width, Screen.height);

            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            return texture;
        }
    }

    public static class SceneValidator
    {
        public const int InvalidSceneIndex = -1;

        public static void ThrowIfSceneDoesNotExist(string sceneName)
        {
            if(SceneExists(sceneName) == false)
            {
                throw new ArgumentException($"Scene {sceneName} does not exist.");
            }
        }

        public static bool SceneExists(string sceneName)
            => SceneUtility.GetBuildIndexByScenePath(sceneName) != InvalidSceneIndex;
    }
}
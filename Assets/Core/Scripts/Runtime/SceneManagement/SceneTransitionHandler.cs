using Nexora.InputSystem;
using Nexora.PostProcessing;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Interface handling scene transitions, used mainly by save system.
    /// </summary>
    public interface ISceneTransitionHandler
    {
        /// <summary>
        /// Object that is used background to invoke <see cref="Coroutine"/>.
        /// May be used by objects that are not <see cref="MonoBehaviour"/>,
        /// like <see cref="ScriptableObject"/>.
        /// </summary>
        MonoBehaviour SceneLoader { get; }

        /// <summary>
        /// Any scene transition operation is currently active?
        /// </summary>
        bool IsOperationInProgress { get; }

        /// <summary>
        /// Initialization to set up the handler, with root object and a fade screen prefab.
        /// </summary>
        void Initialize(GameObject saveModuleObject, GameObject fadeTransitionPrefab);

        void FadeScreen();
        void FadeInAndQuitApplication();
        bool LoadScene(string sceneName);
    }

    [Serializable]
    public abstract class SceneTransitionHandler : ISceneTransitionHandler
    {
        private IScreenFadeTransition _fadeTransition;
        private Coroutine _currentSceneLoadCoroutine;

        public MonoBehaviour SceneLoader { get; private set; }
        public bool IsOperationInProgress => _currentSceneLoadCoroutine != null;

        public virtual void Initialize(GameObject saveModuleObject, GameObject fadeTransitionPrefab)
        {
            SceneLoader = saveModuleObject.AddComponent<SceneLoaderComponent>();
            if (fadeTransitionPrefab != null)
            {
                _fadeTransition = UnityEngine.Object.Instantiate(
                    fadeTransitionPrefab).GetComponent<IScreenFadeTransition>();

                // [Revisit] Should mark it DontDestroyOnLoad()?
            }
        }

        public void FadeScreen()
        {
            if (_fadeTransition != null)
            {
                SceneLoader.StartCoroutine(_fadeTransition.Show());
            }
        }

        /// <summary>
        /// Block input and start fade, after fade the application quits.
        /// </summary>
        public void FadeInAndQuitApplication()
        {
            InputModule.Instance.PushInputProfile(InputProfile.NullInputProfile);
            _currentSceneLoadCoroutine = SceneLoader.StartCoroutine(
                CoroutineUtils.RunActionAfterRoutine(SaveModule.Instance.QuitApplication,
                _fadeTransition.Hide(0.33f)));
        }

        public bool LoadScene(string sceneName)
        {
            if (IsOperationInProgress)
            {
                return false;
            }

            _currentSceneLoadCoroutine = SceneLoader.StartCoroutine(LoadSceneRoutine(sceneName));
            return true;
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            PreLoadScene();
            yield return LoadSceneWithLoadingScreen(sceneName);
            PostLoadScene();
            _currentSceneLoadCoroutine = null;
        }

        protected abstract void PreLoadScene();
        protected abstract void PostLoadScene();

        private IEnumerator LoadSceneWithLoadingScreen(string sceneName)
        {
            yield return FadeIn();

            PostFxModule.Instance.StopAllAnimations();

            AsyncOperation loadTargetScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (loadTargetScene.isDone == false)
            {
                yield return null;
            }

            FadeOut();
        }

        private IEnumerator FadeIn()
        {
            if (_fadeTransition != null)
            {
                yield return _fadeTransition.Hide();
            }
        }

        private void FadeOut()
        {
            if (_fadeTransition != null)
            {
                SceneLoader.StartCoroutine(_fadeTransition.Show());
            }
        }

        protected sealed class SceneLoaderComponent : MonoBehaviour { }
    }

    [Serializable]
    public sealed class DefaultSceneTransitionHandler : SceneTransitionHandler
    {
        protected override void PreLoadScene() 
            => InputModule.Instance.PushInputProfile(InputProfile.NullInputProfile);

        protected override void PostLoadScene()
            => InputModule.Instance.RemoveInputProfile(InputProfile.NullInputProfile);
    }
}
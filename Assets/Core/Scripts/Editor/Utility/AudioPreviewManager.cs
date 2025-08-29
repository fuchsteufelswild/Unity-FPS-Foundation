using Nexora.Audio;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.Editor
{
    /// <summary>
    /// Manager for playing audio in the Editor. Can play both <see cref="AudioSequence"/> using
    /// <see cref="EditorCoroutineUtility"/> and plain <see cref="AudioResource"/>.
    /// </summary>
    public static class AudioPreviewManager
    {
        private static EditorCoroutine _activePreviewRoutine;
        private static AudioSource _previewAudioSource;

        public static readonly GUIContent PlayButtonContent = EditorGUIUtility.IconContent("d_Play");

        /// <summary>
        /// Plays the <paramref name="clip"/> with optional <paramref name="volume"/> and <paramref name="pitch"/>
        /// parameters.
        /// </summary>
        public static void PlaySinglePreview(AudioResource clip, float volume = 1f, float pitch = 1f)
        {
            if(PrepareToPlay(clip) == false)
            {
                return;
            }

            _previewAudioSource.resource = clip;
            _previewAudioSource.volume = volume;
            _previewAudioSource.pitch = pitch;
            _previewAudioSource.Play();
        }

        public static void PlaySequencePreview(AudioSequence sequence)
        {
            if (PrepareToPlay(sequence) == false)
            {
                return;
            }

            _activePreviewRoutine = EditorCoroutineUtility.StartCoroutineOwnerless(
                sequence.PlaySequenceOn(_previewAudioSource));
        }

        /// <summary>
        /// If <paramref name="playResource"/> is valid then stops current preview and 
        /// creates temporary <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="playResource">
        /// Can be <see cref="AudioClip"/>, <see cref="AudioSequence"/>, <see cref="AudioResource"/>
        /// </param>
        /// <returns>True if the <paramref name="playResource"/> can be played.</returns>
        private static bool PrepareToPlay(object playResource)
        {
            if(playResource == null)
            {
                return false;
            }

            StopActivePreview();
            EnsureAudioSource();

            return true;
        }

        public static void StopActivePreview()
        {
            if(_activePreviewRoutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_activePreviewRoutine);
                _activePreviewRoutine = null;
            }

            if(_previewAudioSource != null && _previewAudioSource.isPlaying)
            {
                _previewAudioSource.Stop();
            }
        }

        /// <summary>
        /// Creates a temporary <see cref="AudioSource"/> if one is not already exists or destroyed already.
        /// </summary>
        private static void EnsureAudioSource()
        {
            if (_previewAudioSource == null)
            {
                _previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags(
                    "Preview Audio Source",
                    HideFlags.HideAndDontSave,
                    typeof(AudioSource)
                    ).GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Destroys idle preview <see cref="AudioSource"/> to avoid wasting resources.
        /// </summary>
        public static void CleanupInactivePreview()
        {
            if(_previewAudioSource != null && _previewAudioSource.isPlaying == false)
            {
                StopActivePreview();
                Object.DestroyImmediate(_previewAudioSource.gameObject);
                _previewAudioSource = null;
            }
        }
    }
}

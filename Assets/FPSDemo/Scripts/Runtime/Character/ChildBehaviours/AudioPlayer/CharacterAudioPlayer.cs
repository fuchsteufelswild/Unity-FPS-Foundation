using Nexora.Audio;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public class CharacterAudioPlayer : 
        CharacterBehaviour,
        ICharacterAudioPlayer
    {
        [ReorderableList, LabelByChild(nameof(BodyPartAudioPlaybackConfig.BodyPart))]
        [SerializeField]
        private BodyPartAudioPlaybackConfig[] _bodyPartAudioConfigs = Array.Empty<BodyPartAudioPlaybackConfig>();

        private readonly Transform[] _bodyPointTransforms = new Transform[CharacterUtility.BodyPartsCount];
        private readonly IAudioPlaybackStrategy[] _playbackStrategies = new IAudioPlaybackStrategy[CharacterUtility.BodyPartsCount];

        protected override void OnBehaviourStart(ICharacter parent)
        {
            for(int i = 0; i < _bodyPointTransforms.Length; i++)
            {
                _bodyPointTransforms[i] = parent.GetTransformOfBodyPart((BodyPart)i);

                _playbackStrategies[i] = CreatePlaybackStrategyForMode(_bodyPartAudioConfigs[i].AudioPlaybackMode);
            }
        }

        private IAudioPlaybackStrategy CreatePlaybackStrategyForMode(AudioPlaybackMode mode)
        {
            return mode switch
            {
                AudioPlaybackMode.Static2D => new PlaybackStrategy2D(),
                AudioPlaybackMode.Static3D => new PlaybackStrategy3DStatic(),
                AudioPlaybackMode.Follow3D => new PlaybackStrategy3DFollow(),
                _ => throw new NotImplementedException()
            };
        }

        public AudioSource PlayClip(AudioResource clip, BodyPart bodyPart, float volume = 1, float delay = 0)
        {
            int index = (int)bodyPart;
            return _playbackStrategies[index].PlayClip(clip, transform, volume, delay);
        }

        public AudioSource PlaySequence(AudioSequence sequence, BodyPart bodyPart, float speed = 1)
        {
            int index = (int)bodyPart;
            return _playbackStrategies[index].PlaySequence(sequence, transform, speed);
        }

        public AudioSource StartLoop(AudioResource clip, BodyPart bodyPart, float volume, float duration)
        {
            int index = (int)bodyPart;
            return _playbackStrategies[index].StartLoop(clip, transform, volume, duration);
        }

        public void StopLoop(AudioSource audioSource) => AudioModule.Instance.StopLoop(audioSource);

        [Serializable]
        private struct BodyPartAudioPlaybackConfig
        {
            [Hide]
            public BodyPart BodyPart;

            public AudioPlaybackMode AudioPlaybackMode;

            public BodyPartAudioPlaybackConfig(BodyPart bodyPart, AudioPlaybackMode audioPlaybackMode)
            {
                this.BodyPart = bodyPart;
                this.AudioPlaybackMode = audioPlaybackMode;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => ValidateBodyPartCount();
        private void Reset() => ValidateBodyPartCount();

        private void ValidateBodyPartCount()
        {
            int currentBodyPartCount = CharacterUtility.BodyPartsCount;

            var newConfigs = new List<BodyPartAudioPlaybackConfig>(currentBodyPartCount);

            for(int i = 0; i < currentBodyPartCount; i++)
            {
                newConfigs.Add(i < _bodyPartAudioConfigs.Length ? _bodyPartAudioConfigs[i] : new BodyPartAudioPlaybackConfig((BodyPart)i, AudioPlaybackMode.Static2D));
            }

            EditorUtility.SetDirty(this);
            _bodyPartAudioConfigs = newConfigs.ToArray();
        }
#endif
    }
}
using Nexora.Options;
using System.Collections;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Schedules the auto-save of game levels depending on user preferences.
    /// </summary>
    public class AutoSaveScheduler : MonoBehaviour
    {
        private const int AutoSaveIndex = 0;
        private Coroutine _autoSaveRoutine;
        private float _nextAutoSaveTime;

        /// <summary>
        /// Initializes with the current auto-save options and listens to auto-save change event.
        /// </summary>
        private void Start()
        {
            this.InvokeNextFrame(() =>
            {
                AutoSaveOptions.Instance.AutoSaveEnabled.OnValueChanged += OnAutoSaveSettingChanged;
                OnAutoSaveSettingChanged(AutoSaveOptions.Instance.AutoSaveEnabled);
            });

            Application.quitting += () =>
            {
                AutoSaveOptions.Instance.AutoSaveEnabled.OnValueChanged -= OnAutoSaveSettingChanged;
            };
        }

        private void OnAutoSaveSettingChanged(bool autoSaveEnabled)
        {
            if(autoSaveEnabled)
            {
                CoroutineUtils.StopAndReplaceCoroutine(this, ref _autoSaveRoutine, AutoSaveRoutine());
            }
            else
            {
                CoroutineUtils.StopCoroutine(this, ref _autoSaveRoutine);
            }
        }    

        private IEnumerator AutoSaveRoutine()
        {
            _nextAutoSaveTime = GetNextAutoSaveTime();
            while(true)
            {
                if(Time.time < _nextAutoSaveTime)
                {
                    yield return null;
                }

                _nextAutoSaveTime = GetNextAutoSaveTime();

                // Level manager save game

                yield return null;
            }

            float GetNextAutoSaveTime()
                => Time.time + AutoSaveOptions.Instance.AutoSaveInterval;
        }
    }
}
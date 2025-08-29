using UnityEngine;

namespace Nexora.FPSDemo
{
    public class GameMode : SingletonMonoBehaviour<GameMode>
    {
        private void OnEnable()
        {
            UnityUtils.LockCursor();
        }

        private void OnDisable()
        {
            UnityUtils.UnlockCursor();
        }
    }
}
using UnityEngine;

namespace Nexora.Experimental.Tweening
{
    public enum LoopType
    {
        None = 0,
        Restart = 1,
        Yoyo = 2,
        // Incremental = 3
    }

    public enum TweenState
    {
        Stopped = 0,
        Playing = 1,
        Paused = 2
    }

    /// <summary>
    /// Used to determine what do to a tween's value when it is reset.
    /// </summary>
    public enum TweenResetBehaviour
    {
        /// <summary>
        /// Keeps the value of the tween without changing.
        /// </summary>
        KeepCurrentValue = 0,

        /// <summary>
        /// Sets the value of the tween to the start value. (from*, to)
        /// </summary>
        ResetToStartValue = 1,

        /// <summary>
        /// Sets the value of the tween to the end value. (from, to*)
        /// </summary>
        ResetToEndValue = 2
    }
}
using Nexora.FPSDemo.ProceduralMotion;
using System.Collections.Generic;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Interface for initiating a lean motion to the <see cref="LeanMotion"/>.
    /// It is not exactly an adapter, but delegates specific calls to it, that are related to
    /// starting the lean motion.
    /// </summary>
    public interface ILeanMotionController
    {
        /// <summary>
        /// Maximum leaning percent of the motion.
        /// </summary>
        float MaxLeanPercent { get; set; }

        /// <summary>
        /// Sets motion to perform leaning to <paramref name="leanState"/>.
        /// </summary>
        void SetLeanState(LeanState leanState);
    }


    public sealed class LeanMotionController : ILeanMotionController
    {
        private readonly LeanMotion _leanMotion;

        public float MaxLeanPercent 
        {
            get => _leanMotion.MaxLeanPercent; 
            set => _leanMotion.MaxLeanPercent = value;
        }

        public LeanMotionController(LeanMotion leanMotion) => _leanMotion = leanMotion;

        public void SetLeanState(LeanState leanState) => _leanMotion.SetLeanState(leanState);
    }

    /// <summary>
    /// Keeps collection of motion controllers and applies actions to all of them.
    /// </summary>
    public sealed class LeanMotionGroup
    {
        private readonly IReadOnlyList<ILeanMotionController> _controllers;

        public LeanMotionGroup(params ILeanMotionController[] controllers) => _controllers = controllers;

        public void SetLeanState(LeanState leanState)
        {
            foreach(var controller in _controllers)
            {
                controller.SetLeanState(leanState);
            }
        }

        public void SetMaxLeanPercent(float percent)
        {
            foreach(var controller in _controllers)
            {
                controller.MaxLeanPercent = percent;
            }
        }
    }
}
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Interface that is for mixing multiple motions that will be applied on a target <see cref="Transform"/>.
    /// Provides methods for adding/removing/getting motions, and preset offsets that are set on initializing the mixer.
    /// </summary>
    public interface IMotionMixer
    {
        /// <summary>
        /// Blend weight multiplier provided by the mixer, motions may use it along with their own blend weights.
        /// </summary>
        float BlendWeight { get; set; }

        /// <summary>
        /// Target the motions are applied to.
        /// </summary>
        Transform Target { get; }

        /// <summary>
        /// Offset applied before mixing motions. This is like modifying the base (0, 0, 0).
        /// </summary>
        Vector3 BaseOffset { get; }

        /// <summary>
        /// Position offset applied when mixing motions.
        /// </summary>
        Vector3 PositionOffset { get; }

        /// <summary>
        /// Rotation offset applied when mixing motions.
        /// </summary>
        Vector3 RotationOffset { get; }

        /// <summary>
        /// Add motion to the current list of motions that are mixed.
        /// </summary>
        void AddMotion(IMotion motion);
        /// <summary>
        /// Remove motion from the current list of motions that are mixed.
        /// </summary>
        void RemoveMotion(IMotion motion);

        bool TryGetMotion<T>(out T motion) where T : class, IMotion;
        T GetMotion<T>() where T : class, IMotion;

        /// <summary>
        /// Transforms the given vector into the space of the target <see cref="Transform"/>
        /// motions are applied to.
        /// </summary>
        Vector3 InverseTransformVector(Vector3 worldVector);

        /// <summary>
        /// Configure mixer with the new set of values and a target.
        /// </summary>
        void ConfigureMixer(Transform target, Vector3 pivotOffset, Vector3 positionOffset, Vector3 rotationOffset);
    }
}

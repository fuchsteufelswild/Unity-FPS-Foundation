using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Provides interface about the current parameters about the 
    /// physics values of the characters.
    /// </summary>
    public interface ICharacterPhysics
    {
        /// <summary>
        /// Character height.
        /// </summary>
        float Height { get; set; }

        /// <summary>
        /// Character radius.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Default height of the character (without any crouching or modifications).
        /// </summary>
        float DefaultHeight { get; }

        /// <summary>
        /// Gravity affecting the player (it can be different than the Physics settings for adjustment).
        /// </summary>
        float Gravity { get; }

        /// <summary>
        /// Mass of the character.
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// With what objects can character collide?
        /// </summary>
        LayerMask CollisionMask { get; }

        /// <summary>
        /// Can character set its height to be <paramref name="height"/>?
        /// (You might try to stand but there can be a wall above).
        /// </summary>
        /// <param name="height">Target height value.</param>
        /// <returns>If height can be set to <paramref name="height"/>, <see langword="null"/> otherwise.</returns>
        bool CanSetHeight(float height);

        /// <summary>
        /// Event fired upon character's height changes.
        /// </summary>
        event UnityAction<float> HeightChanged;
    }
}
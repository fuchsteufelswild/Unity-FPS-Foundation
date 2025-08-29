using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.ProceduralMotion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Core interface for <see cref="IHandheld"/>. Provides with main interface without
    /// any controllers.
    /// </summary>
    public interface IHandheldCore
    {
        /// <summary>
        /// Owning character.
        /// </summary>
        ICharacter Character { get; }

        /// <summary>
        /// Current equip state of the handheld (is it hidden, equipped, etc.).
        /// </summary>
        HandheldEquipStateType EquipState { get; }

        /// <summary>
        /// Is renderers of the object is visible.
        /// </summary>
        bool IsGeometryVisible { get; set; }

        /// <summary>
        /// Sets the controller character of the handheld to <paramref name="character"/>.
        /// </summary>
        /// <param name="character"></param>
        void SetCharacter(ICharacter character);

        /// <summary>
        /// Called when <see cref="HandheldEquipStateType"/> of handheld is changed (e.g transition from equipped to holstering).
        /// </summary>
        event HandheldEquipStateChangedDelegate EquipStateChanged;
    }

    /// <summary>
    /// Provides interface for accessing controllers of the <see cref="IHandheld"/>.
    /// </summary>
    public interface IHandheldComponents
    {
        /// <summary>
        /// Targets that procedural motion of <see cref="IHandheld"/> has effects on.
        /// </summary>
        IHandheldMotionTargets MotionTargets { get; }

        /// <summary>
        /// Procedural motion controller of the handheld, used for first person procedural motions.
        /// </summary>
        IHandheldMotionController MotionController { get; }

        /// <summary>
        /// Animator used for animating animations such as Equip, Holster etc. which are not suitable for
        /// procedural motion.
        /// </summary>
        IAnimatorController Animator { get; }

        /// <summary>
        /// Audio player of the owning character to play audios.
        /// </summary>
        ICharacterAudioPlayer AudioPlayer { get; }
    }
}
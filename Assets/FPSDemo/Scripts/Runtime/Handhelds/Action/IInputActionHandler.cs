using Nexora.FPSDemo.Handhelds.RangedWeapon;
using Nexora.FPSDemo.Options;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public enum InputActionState
    {
        /// <summary>
        /// Input is started (key pressed).
        /// </summary>
        Start = 0,

        /// <summary>
        /// Input is held (key is held down).
        /// </summary>
        Hold = 1,

        /// <summary>
        /// Input is ended (key is released).
        /// </summary>
        End = 2
    }

    public interface IHasActionHandler
    {
        bool HasActionOfType<T>() where T : IInputActionHandler => TryGetActionOfType<T>(out _);
        
        bool TryGetActionOfType<T>(out T actionHandler) where T : IInputActionHandler
        {
            actionHandler = GetActionOfType<T>();
            return actionHandler != null;
        }

        T GetActionOfType<T>() where T : IInputActionHandler;

        bool TryGetActionOfType(Type actionType, out IInputActionHandler actionHandler)
        {
            actionHandler = GetActionOfType(actionType);
            return actionHandler != null;
        }

        IInputActionHandler GetActionOfType(Type actionType);
    }

    /// <summary>
    /// Base interface for handling input actions.
    /// </summary>
    public interface IInputActionHandler
    {
        /// <summary>
        /// Is action currently performing?
        /// </summary>
        bool IsPerforming { get; }

        /// <summary>
        /// Blocker that controls the input flow to this action.
        /// </summary>
        ActionBlockerCore Blocker { get; }

        /// <summary>
        /// Handles the input action based on input state (start/hold/end).
        /// </summary>
        /// <returns></returns>
        bool HandleInput(InputActionState inputState);
    }

    public enum InputActionType
    {
        Use = 0,
        Aim = 1,
        Reload = 2
    }

    public interface IUseActionHandler : IInputActionHandler
    {
        bool StartUse();
        bool HoldUse();
        bool EndUse();
    }

    public interface IAimActionHandler : IInputActionHandler
    {
        bool StartAim();
        bool EndAim();
    }

    public interface IReloadActionHandler : IInputActionHandler
    {
        bool StartReload();
        bool EndReload();
    }
}
using System;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Delegate used for providing look input (mouse movement).
    /// </summary>
    public delegate Vector2 LookInputDelegate();

    public interface ILookInputProvider
    {
        Vector2 GetLookInput();
    }

    public sealed class DelegateInputProvider : ILookInputProvider
    {
        private readonly LookInputDelegate _inputDelegate;

        public DelegateInputProvider(LookInputDelegate inputDelegate) 
            => _inputDelegate = inputDelegate ?? throw new ArgumentNullException(nameof(inputDelegate));

        public Vector2 GetLookInput() => _inputDelegate.Invoke();
    }

    public sealed class StaticInputProvider : ILookInputProvider
    {
        private readonly Vector2 _staticInput;

        public StaticInputProvider(Vector2 staticInput) => _staticInput = staticInput;

        public Vector2 GetLookInput() => _staticInput;
    }
}
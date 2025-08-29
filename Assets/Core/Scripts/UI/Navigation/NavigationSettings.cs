using System;
using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Navigation settings used for objects that can be navigated in the UI
    /// namely <see cref="InteractiveUIElementBase"/>.
    /// </summary>
    [Serializable]
    public struct NavigationSettings : IEquatable<NavigationSettings>
    {
        [Flags]
        public enum NavigationMode
        {
            /// <summary>
            /// Navigation is not allowed.
            /// </summary>
            None = 0,

            /// <summary>
            /// Navigation is only allowed left-right.
            /// </summary>
            Horizontal = 1 << 0,

            /// <summary>
            /// Navigation is only allowed up-down.
            /// </summary>
            Vertical = 1 << 1,

            /// <summary>
            /// Navigation allowed in all 4 directions(left-right-up-down).
            /// </summary>
            Automatic = Horizontal | Vertical, // 3

            /// <summary>
            /// Navigation is explicit, targets are chosen depending on the assignments
            /// done in the settings. (namely <see cref="_navigateOnLeft"/>, <see cref="_navigateOnRight"/>)
            /// <see cref="_navigateOnUp"/>, <see cref="_navigateOnDown"/>).
            /// </summary>
            Explicit = 1 << 2
        };

        [Tooltip("Defines how navigation behaves.")]
        [SerializeField]
        private NavigationMode _navigationMode;

        [Tooltip("If enabled, navigation wraps around the last element to the first, " +
            "within the same navigation group.")]
        [ShowIf(nameof(_navigationMode), 
            NavigationMode.Horizontal | NavigationMode.Vertical, 
            Comparison = UnityComparisonMethod.Mask)]
        [SerializeField]
        private bool _wrapAround;

        [Tooltip("Navigation element to go to when left button is pressed.")]
        [ShowIf(nameof(_navigationMode), NavigationMode.Explicit)]
        [SerializeField]
        private InteractiveUIElementBase _navigateOnLeft;

        [Tooltip("Navigation element to go to when right button is pressed.")]
        [ShowIf(nameof(_navigationMode), NavigationMode.Explicit)]
        [SerializeField]
        private InteractiveUIElementBase _navigateOnRight;

        [Tooltip("Navigation element to go to when up button is pressed.")]
        [ShowIf(nameof(_navigationMode), NavigationMode.Explicit)]
        [SerializeField]
        private InteractiveUIElementBase _navigateOnUp;

        [Tooltip("Navigation element to go to when down button is pressed.")]
        [ShowIf(nameof(_navigationMode), NavigationMode.Explicit)]
        [SerializeField]
        private InteractiveUIElementBase _navigateOnDown;

        public NavigationMode Mode
        { 
            readonly get => _navigationMode; 
            set => _navigationMode = value;
        }

        public bool WrapAround
        {
            readonly get => _wrapAround;
            set => _wrapAround = value;
        }

        public InteractiveUIElementBase NavigateOnLeft
        {
            get => _navigateOnLeft;
            set => _navigateOnLeft = value;
        }

        public InteractiveUIElementBase NavigateOnRight
        {
            get => _navigateOnRight;
            set => _navigateOnRight = value;
        }

        public InteractiveUIElementBase NavigateOnUp
        {
            get => _navigateOnUp;
            set => _navigateOnUp = value;
        }

        public InteractiveUIElementBase NavigateOnDown
        {
            get => _navigateOnDown;
            set => _navigateOnDown = value;
        }

        public static NavigationSettings DefaultNavigation
        {
            get
            {
                return new NavigationSettings
                {
                    _navigationMode = NavigationMode.Automatic,
                    _wrapAround = false,
                };
            }
        }

        public readonly bool Equals(NavigationSettings other)
        {
            return _navigationMode == other._navigationMode
                && _wrapAround == other._wrapAround
                && _navigateOnLeft == other._navigateOnLeft
                && _navigateOnRight == other._navigateOnRight
                && _navigateOnUp == other._navigateOnUp
                && _navigateOnDown == other._navigateOnDown;
        }

        public override bool Equals(object obj)
        {
            return obj is NavigationSettings other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(
                (int)_navigationMode,
                _wrapAround.GetHashCode(),
                _navigateOnLeft?.GetHashCode() ?? 0,
                _navigateOnRight?.GetHashCode() ?? 0,
                _navigateOnUp?.GetHashCode() ?? 0,
                _navigateOnDown?.GetHashCode() ?? 0);
        }

        public static bool operator ==(NavigationSettings left, NavigationSettings right)
            => left.Equals(right);

        public static bool operator !=(NavigationSettings left, NavigationSettings right)
            => left.Equals(right) == false;
    }
}
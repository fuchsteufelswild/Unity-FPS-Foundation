using UnityEngine.EventSystems;

namespace Nexora.UI
{
    /// <summary>
    /// Provides interface to access <see cref="UIBehaviour"/>, to prevent 
    /// tight-coupling with Unity's concrete classes.
    /// </summary>
    public interface IUIBehaviour : IMonoBehaviour
    {
        /// <summary>
        /// Returns true if the GameObject and the Component are active.
        /// </summary>
        bool IsActive();

        /// <summary>
        /// Returns true if the native representation of the behaviour has been destroyed.
        /// </summary>
        /// <remarks>
        /// When a parent canvas is either enabled, disabled or a nested canvas's OverrideSorting is changed this function is called. You can for example use this to modify objects below a canvas that may depend on a parent canvas - for example, if a canvas is disabled you may want to halt some processing of a UI element.
        /// <br></br><br></br>
        /// Workaround for Unity native side of the object
        /// having been destroyed but accessing via interface
        /// won't call the overloaded ==
        /// </remarks>
        bool IsDestroyed();
    }
}
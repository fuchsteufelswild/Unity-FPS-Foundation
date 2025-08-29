using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Delegate to use for informing about change of scope.
    /// </summary>
    /// <param name="scopeID">New scope's ID.</param>
    public delegate void ScopeChangedDelegate(int scopeID);

    /// <summary>
    /// Delegate to use for informing about the change of <b>enabled</b> state of scope.
    /// </summary>
    /// <param name="enabled">Is enabled?</param>
    public delegate void ScopeEnabledDelegate(bool enabled);

    public interface IScopeHandler
    {
        /// <summary>
        /// Indicating the ID of scope.
        /// </summary>
        int ScopeID { get; }

        /// <summary>
        /// Current zoom of the scope.
        /// </summary>
        int CurrentZoomLevel { get; }

        /// <summary>
        /// Maximum achievable zoom of the scope.
        /// </summary>
        int MaxZoomLevel { get; }

        /// <returns>If the scope currently active and visible.</returns>
        bool IsScopeEnabled();
    }
}
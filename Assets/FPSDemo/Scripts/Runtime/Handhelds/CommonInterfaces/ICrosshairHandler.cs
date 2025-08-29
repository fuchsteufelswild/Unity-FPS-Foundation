using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Delegate to use for informing about change of crosshair.
    /// </summary>
    /// <param name="crosshairID">New crosshair's ID.</param>
    public delegate void CrosshairChangedDelegate(int crosshairID);

    public interface ICrosshairHandler
    {
        /// <summary>
        /// Indicating the ID of crosshair.
        /// </summary>
        int CrosshairID { get; set; }

        /// <summary>
        /// Charge of the crosshair, if it can be filled with charge duration (holding mouse for some time).
        /// </summary>
        float Charge { get; }

        /// <summary>
        /// Accuracy of the crosshair, denotes how little spread will be.
        /// </summary>
        float Accuracy { get; }

        /// <summary>
        /// Invoked when assigned crosshair is changed (crosshair ID).
        /// </summary>
        event CrosshairChangedDelegate CrosshairChanged;

        /// <summary>
        /// Resets the crosshair to be default.
        /// </summary>
        void ResetCrosshair();

        /// <returns>If the crosshair currently active and visible.</returns>
        bool IsCrosshairActive();

        /// <summary>
        /// Sets the charge value.
        /// </summary>
        void SetCharge(float charge);
    }

    public abstract class CrosshairBase : 
        MonoBehaviour,
        ICrosshairHandler
    {
        [Tooltip("Default crosshair index, (-1) for no crosshair.")]
        [OnValueChanged(nameof(ValidateCrosshairChanged))]
        [SerializeField, Range(-1, 50)]
        private int _defaultCrosshairID;

        private int _currentCrosshairID;

        public int CrosshairID
        {
            get => _currentCrosshairID;
            set
            {
                if(_currentCrosshairID == value)
                {
                    return;
                }

                _currentCrosshairID = value;
                CrosshairChanged?.Invoke(_currentCrosshairID);
            }
        }

        public float Charge { get; private set; } = 0f;
        public float Accuracy { get; protected set; }

        public event CrosshairChangedDelegate CrosshairChanged;

        protected virtual void Awake() => ResetCrosshair();

        public void SetCharge(float charge) => Charge = charge;
        public virtual bool IsCrosshairActive() => true;
        public void ResetCrosshair() => CrosshairID = _defaultCrosshairID;

        [Conditional("UNITY_EDITOR")]
        protected void ValidateCrosshairChanged()
        {
            if(Application.isPlaying)
            {
                CrosshairID = _defaultCrosshairID;
            }
        }
    }

    public static partial class CrosshairConstants
    {
        public const int NoCrosshairID = -1;
    }
}
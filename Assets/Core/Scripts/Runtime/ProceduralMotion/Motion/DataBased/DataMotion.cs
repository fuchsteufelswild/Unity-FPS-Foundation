using UnityEngine;
using System;

namespace Nexora.Motion
{
    /// <summary>
    /// Abstract class that is mixes motions, like <see cref="Motion{TChild, TParent}"/> from which it derives.
    /// The main addition is that this motion uses a specific data to set new targets for the position/rotation springs.
    /// It receives new <see cref="IMotionData"/> of type <typeparamref name="TMotionData"/> from <see cref="IMotionDataBroadcaster"/> 
    /// and updates the motion depending on the data.
    /// </summary>
    /// <remarks>
    /// Check <see cref="Motion{TChild, TParent}"/> doc first to understand this class better.
    /// <br></br>This motion can be fed new <see cref="IMotionData"/> in runtime. But it should be of type
    /// <typeparamref name="TMotionData"/>, no other type would work out.
    /// </remarks>
    /// <typeparam name="TChild"><inheritdoc cref="Motion{TChild, TParent}" path="/typeparam[@name='TChild']"/></typeparam>
    /// <typeparam name="TParent"><inheritdoc cref="Motion{TChild, TParent}" path="/typeparam[@name='TParent']"/></typeparam>
    /// <typeparam name="TMotionData">Type of the motion data.</typeparam>
    public abstract class DataMotion<TChild, TParent, TMotionData> :
        Motion<TChild, TParent>,
        IMotionDataReceiver
        where TChild : IChildBehaviour<TChild, TParent>
        where TParent : IParentBehaviour<TParent, TChild>
        where TMotionData : class, IMotionData
    {
        [SerializeField]
        [DisableIf(nameof(HasMotionDataBroadcaster), true)]
        private TMotionData _currentMotionData;

        protected TMotionData CurrentMotionData => _currentMotionData;

        /// <summary>
        /// When object is enabled registers self to the broadcaster. 
        /// So that it can receive datas.
        /// </summary>
        protected sealed override void OnEnable()
        {
            base.OnEnable();

            if (TryGetComponent<IMotionDataBroadcaster>(out var dataBroadcaster))
            {
                _currentMotionData = null;
                dataBroadcaster.AddDataReceiver(this);
            }
        }

        /// <summary>
        /// When object is disabled unregisters self from the broadcaster. 
        /// So that it will preserve its current state and won't listen to any events.
        /// </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();

            if (TryGetComponent<IMotionDataBroadcaster>(out var dataBroadcaster))
            {
                _currentMotionData = null;
                dataBroadcaster.RemoveDataReceiver(this);
            }
        }

        Type IMotionDataReceiver.MotionDataType => typeof(TMotionData);

        /// <summary>
        /// Changes the current motion data with the received one and
        /// call <see cref="OnMotionDataChanged(TMotionData)"/> which will be implemented
        /// by derived classes.
        /// </summary>
        /// <remarks>
        /// If the received data is null, springs are reset.
        /// </remarks>
        void IMotionDataReceiver.SetMotionData(IMotionData motionData)
        {
            TMotionData previousData = _currentMotionData;
            _currentMotionData = motionData as TMotionData;

            if (previousData == motionData)
            {
                return;
            }

            if (_currentMotionData == null)
            {
                ResetMotionTargets();
            }

            OnMotionDataChanged(_currentMotionData);
        }

        /// <summary>
        /// Called when the motion data is changed.
        /// </summary>
        /// <param name="motionData"></param>
        protected virtual void OnMotionDataChanged(TMotionData motionData) { }

        private void ResetMotionTargets()
        {
            SetTargetPosition(Vector3.zero);
            SetTargetRotation(Vector3.zero);
        }

        protected bool HasMotionDataBroadcaster() => GetComponent<IMotionDataBroadcaster>() != null;
    }
}

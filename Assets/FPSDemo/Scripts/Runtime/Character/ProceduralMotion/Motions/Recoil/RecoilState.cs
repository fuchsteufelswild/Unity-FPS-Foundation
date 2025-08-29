using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public enum RecoilStateType
    {
        Inactive = 0,
        Recoiling = 1,
        Recovering = 2
    }

    public interface IRecoilState
    {
        /// <summary>
        /// Type of the state.
        /// </summary>
        RecoilStateType StateType { get; }

        /// <summary>
        /// Should update this state? (Is state active or allows updates).
        /// </summary>
        /// <returns></returns>
        bool ShouldUpdate();

        /// <summary>
        /// Called for entering this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Advances this state a frame further.
        /// </summary>
        /// <param name="lookDelta">How much look changed in the last frame?</param>
        void Tick(float deltaTime, Vector2 lookDelta);

        /// <summary>
        /// Adds <paramref name="recoilAmount"/>.
        /// </summary>
        void AddRecoil(Vector2 recoilAmount);

        /// <summary>
        /// Changes the <see cref="SpringSettings"/> defined for this state.
        /// </summary>
        /// <param name="springSettings"></param>
        void ChangeSpringSettings(SpringSettings springSettings);
    }

    [SerializeField]
    public abstract class RecoilState : IRecoilState
    {
        protected readonly RecoilProcessor _context;

        public abstract RecoilStateType StateType { get; }

        protected RecoilState(RecoilProcessor context) => _context = context ?? throw new ArgumentNullException(nameof(context));

        public virtual void ChangeSpringSettings(SpringSettings springSettings) { }
        public virtual bool ShouldUpdate() => true;
        public abstract void AddRecoil(Vector2 recoilAmount);
        public abstract void Enter();
        public abstract void Tick(float deltaTime, Vector2 lookDelta);
    }

    [Serializable]
    public sealed class InactiveState : RecoilState
    {
        public override RecoilStateType StateType => RecoilStateType.Inactive;

        public InactiveState(RecoilProcessor context) : base(context) { }

        public override bool ShouldUpdate() => false;

        public override void Enter() => _context.ResetStateData();

        public override void Tick(float deltaTime, Vector2 lookDelta) { }

        public override void AddRecoil(Vector2 recoilAmount)
        {
            _context.SetTargetRecoil(recoilAmount);
            _context.TransitionTo(RecoilStateType.Recoiling);
        }
    }

    [Serializable]
    public sealed class RecoilingState : RecoilState
    {
        [SerializeField]
        private SpringSettings _recoilSpringSettings = SpringSettings.Default;

        public override RecoilStateType StateType => RecoilStateType.Recoiling;

        public RecoilingState(RecoilProcessor context) : base(context) { }

        public override void Enter()
        {
            _context.ApplySpringSettings(_recoilSpringSettings);
            _context.SetControlOffset(Vector2.zero);
        }

        public override void ChangeSpringSettings(SpringSettings springSettings) => _recoilSpringSettings = springSettings;

        public override void AddRecoil(Vector2 recoilAmount) => _context.SetTargetRecoil(_context.TargetRecoil + recoilAmount);

        public override void Tick(float deltaTime, Vector2 lookDelta)
        {
            UpdateControlOffset(lookDelta);

            if (_context.HasReachedTarget())
            {
                if (HasControlledRecoil())
                {
                    _context.TransitionTo(RecoilStateType.Inactive);
                }
                else
                {
                    _context.TransitionTo(RecoilStateType.Recovering);
                }
            }
        }

        /// <summary>
        /// Increases control offset in both axes, if mouse movement is done
        /// <b>opposite</b> to the recoil (as controlling recoil is done this way).
        /// </summary>
        /// <param name="lookDelta">How much look changed by input in the last frame.</param>
        private void UpdateControlOffset(Vector2 lookDelta)
        {
            Vector2 controlOffset = _context.ControlOffset;
            Vector2 targetRecoil = _context.TargetRecoil;

            if (lookDelta.y > 0f)
            {
                controlOffset.y += lookDelta.y;
            }

            // We control recoil if we move mouse to the opposite side
            if (targetRecoil.x != 0f)
            {
                float opposingSign = -Mathf.Sign(targetRecoil.x);

                if (Mathf.Sign(lookDelta.x) == opposingSign)
                {
                    controlOffset.x += lookDelta.x;
                }
            }

            _context.SetControlOffset(controlOffset);
        }

        /// <summary>
        /// Checks if recoil fully controlled? 
        /// (Moved mouse in opposite direction of the recoil and more than the recoil).
        /// </summary>
        /// <returns></returns>
        private bool HasControlledRecoil()
        {
            Vector2 controlOffset = _context.ControlOffset;
            Vector2 targetRecoil = _context.TargetRecoil;

            bool verticalControl = controlOffset.y > Mathf.Abs(targetRecoil.y);
            bool horizontalControl = controlOffset.x > Mathf.Abs(targetRecoil.x);

            return verticalControl && horizontalControl;
        }
    }

    [Serializable]
    public sealed class RecoveryState : RecoilState
    {
        [SerializeField]
        private SpringSettings _recoverySpringSettings = SpringSettings.Default;

        public override RecoilStateType StateType => RecoilStateType.Recovering;

        public RecoveryState(RecoilProcessor context) : base(context) { }

        public override void ChangeSpringSettings(SpringSettings springSettings) => _recoverySpringSettings = springSettings;

        public override void Enter()
        {
            _context.ApplySpringSettings(_recoverySpringSettings);
            _context.SetSpringTarget(CalculateRecoveryPosition());
        }

        private Vector2 CalculateRecoveryPosition()
        {
            Vector2 controlOffset = _context.ControlOffset;
            Vector2 targetRecoil = _context.TargetRecoil;

            return new()
            {
                // "Control offset" is 'negative', because they must have different signs with "target recoil"
                // Control is increased if it's in the opposing direction of the recoil
                x = Mathf.Clamp(-controlOffset.x, targetRecoil.x, 0f),
                y = Mathf.Clamp(-controlOffset.y, -Mathf.Abs(targetRecoil.y * 0.5f), Mathf.Abs(targetRecoil.y * 0.5f))
            };
        }

        public override void AddRecoil(Vector2 recoilAmount)
        {
            _context.SetTargetRecoil(_context.CurrentRecoil + recoilAmount);
            _context.TransitionTo(RecoilStateType.Recoiling);
        }

        public override void Tick(float deltaTime, Vector2 lookDelta)
        {
            if (_context.IsIdle)
            {
                _context.Reset();
            }
        }
    }
}
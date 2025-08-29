using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldRetractionHandler : ICharacterBehaviour
    {
        /// <summary>
        /// Closest object's distance to the <see cref="IHandheld"/>.
        /// </summary>
        float ClosestObjectDistance { get; }
    }

    /// <summary>
    /// Class for blocking actions of <see cref="IHandheld"/> if it is very close to an object.
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public sealed class HandheldRetractionHandler : 
        CharacterBehaviour, 
        IHandheldRetractionHandler
    {
        [SerializeField]
        private Transform _view;

        [SerializeField]
        private ObstacleDetector _obstacleDetector;

        [Tooltip("If an object detected closer than this distance, then actions will be blocked.")]
        [SerializeField, Range(0.01f, 5f)]
        private float _blockDistance = 0.5f;

        [SpaceArea]
        [SerializeField, ReorderableList]
        [field: TypeConstraint(typeof(IInputActionHandler))]
        private SerializedType[] _blockedActions;

        private Transform _ignoredRoot;
        private IHandheld _currentHandheld;
        private bool _isBlocked;

        public float ClosestObjectDistance { get; private set; }

        protected override void OnBehaviourStart(ICharacter parent) => _ignoredRoot = parent.transform;

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            var controller = parent.GetCC<IHandheldsManager>();
            controller.EquipBegin += OnEquippingStarted;
            _currentHandheld = controller.ActiveHandheld;
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            var controller = parent.GetCC<IHandheldsManager>();
            controller.EquipBegin -= OnEquippingStarted;
        }

        private void OnEquippingStarted(IHandheld handheld)
        {
            SetBlockerStatus(false);
            _currentHandheld = handheld;
        }

        private void FixedUpdate()
        {
            if (_currentHandheld == null)
            {
                return;
            }

            var ray = new Ray(_view.position, _view.forward);
            ClosestObjectDistance = _obstacleDetector.DetectClosestDistance(ray, _ignoredRoot);

            SetBlockerStatus(ClosestObjectDistance < _blockDistance);
        }

        private void SetBlockerStatus(bool shouldBlock)
        {
            if (_isBlocked == shouldBlock || _currentHandheld == null)
            {
                return;
            }

            _isBlocked = shouldBlock;

            ApplyToBlockers(shouldBlock);
        }

        private void ApplyToBlockers(bool shouldBlock)
        {
            foreach(var action in _blockedActions)
            {
                if(_currentHandheld.TryGetActionOfType(out IInputActionHandler actionHandler))
                {
                    if (shouldBlock)
                    {
                        actionHandler.Blocker.AddBlocker(this);
                    }
                    else
                    {
                        actionHandler.Blocker.RemoveBlocker(this);
                    }
                }
            }
        }

        [Serializable]
        private sealed class ObstacleDetector
        {
            [SerializeField]
            private LayerMask _layerMask = Layers.SimpleSolidObjectsMask;

            [SerializeField, Range(0.01f, 5f)]
            private float _castDistance = 1f;

            [SerializeField, Range(0f, 1f)]
            private float _castRadius = 0.04f;

            public float DetectClosestDistance(Ray ray, Transform ignoredRoot)
            {
                bool useSphereCast = _castRadius > 0.001f;

                return useSphereCast
                    ? PhysicsUtils.GetClosestSphereCastDistance(ray, _castRadius, _castDistance, _layerMask, ignoredRoot)
                    : PhysicsUtils.GetClosestRaycastDistance(ray, _castDistance, _layerMask, ignoredRoot);
            }
        }
    }
}
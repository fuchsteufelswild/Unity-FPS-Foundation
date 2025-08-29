using Nexora.Motion;
using UnityEngine;
using UnityEngine.Animations;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldArmsController : ICharacterBehaviour
    {
        Animator Animator { get; }
        bool IsVisible { get; set; }

        void EnableArms();
        void DisableArms();
    }

    [RequireComponent(typeof(ParentConstraint))]
    public class HandheldArmsController : 
        MonoBehaviour,
        IHandheldArmsController
    {
        [SerializeField, NotNull]
        private Animator _animator;

        private ParentConstraint _parentConstraint;
        private IMotionMixer _motionMixer;

        public Animator Animator => _animator;

        public bool IsVisible { get; set; }

        private void Awake()
        {
            _parentConstraint = GetComponent<ParentConstraint>();
            _motionMixer = GetComponentInParent<IMotionMixer>();
            gameObject.SetActive(false);
        }

        public void DisableArms()
        {
            _animator.Rebind();
            gameObject.SetActive(false);
            _parentConstraint.constraintActive = false;
        }

        public void EnableArms()
        {
            gameObject.SetActive(true);

            var source = new ConstraintSource { weight = 1f, sourceTransform = _motionMixer.Target };
            _parentConstraint.constraintActive = true;

            if(_parentConstraint.sourceCount == 0)
            {
                _parentConstraint.AddSource(source);
            }
            else
            {
                _parentConstraint.SetSource(0, source);
            }
        }
    }
}
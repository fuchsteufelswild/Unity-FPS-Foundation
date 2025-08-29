using Nexora.FPSDemo.ProceduralMotion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public static class HandheldUtility
    {
        public static IAnimatorController CreateAnimator(GameObject gameObject)
        {
            var animators = gameObject.GetComponentsInChildren<IAnimatorController>(false);
            return animators.Length switch
            {
                0 => NullAnimator.Instance,
                1 => animators.First(),
                _ => new CompositeAnimator(animators)
            };
        }

        public static IHandheldMotionController CreateMotionController(GameObject gameObject)
        {
            var motionController = gameObject.GetComponentInDirectChildren<IHandheldMotionController>();
            if(motionController == null)
            {
                Debug.LogError("This handheld has no motion controller, procedural animations won't work.");
            }

            return motionController;
        }

        public static IHandheldMotionTargets CreateMotionTargets(GameObject gameObject)
        {
            var motionTargets = gameObject.GetComponentInDirectChildren<IHandheldMotionTargets>();
            if (motionTargets == null)
            {
                Debug.LogError("This handheld has no motion controller, procedural animations won't work.");
            }

            return motionTargets;
        }
    }
}
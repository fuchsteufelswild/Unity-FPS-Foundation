using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public readonly struct LaunchContext
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Velocity;
        public readonly Vector3 Torque;
        public readonly float Gravity;
        public readonly int LayerMask;
        public readonly float? Speed; 

        public LaunchContext(Vector3 origin, Vector3 velocity, Vector3 torque, float gravity, int layerMask = Layers.SolidObjectsMask, float? speed = null)
        {
            Origin = origin;
            Velocity = velocity;
            Torque = torque;
            Gravity = gravity;
            LayerMask = layerMask;
            Speed = speed;
        }
    }

    public interface IGunFiringMechanismBehaviour : IGunBehaviour
    {
        /// <summary>
        /// Amount of ammo used per shot.
        /// </summary>
        int AmmoUsedPerShot { get; }

        /// <summary>
        /// Executes the fire with <paramref name="accuracy"/> and <paramref name="impactEffect"/>.
        /// </summary>
        /// <param name="accuracy">Accuracy of the gun at the moment.</param>
        /// <param name="impactEffect">Impact effect to be played when shot is landed.</param>
        void Fire(float accuracy, IGunImpactEffectBehaviour impactEffect);

        /// <summary>
        /// Called when a shot fired.
        /// </summary>
        event UnityAction Shoot;

        /// <summary>
        /// Gets <see cref="LaunchContext"/> containing information about the shot's projectory and settings.
        /// </summary>
        /// <returns></returns>
        LaunchContext GetLaunchContext();
    }

    public sealed class NullFiringMechanism : IGunFiringMechanismBehaviour
    {
        public static readonly NullFiringMechanism Instance = new();

        public int AmmoUsedPerShot => 0;

        public event UnityAction Shoot { add { } remove { } }

        public void Attach() { }
        public void Detach() { }
        public void Fire(float accuracy, IGunImpactEffectBehaviour impactEffect) { }
        public LaunchContext GetLaunchContext() => default;
    }

    public sealed class GunFiringMechanism :
        GunBehaviour,
        IGunFiringMechanismBehaviour
    {
        [Tooltip("The strategy defines how a shot is fired, (e.g HitScan, Projectile etc.)")]
        [ReferencePicker(typeof(ShotStrategy))]
        [SerializeReference]
        private ShotStrategy _shotStrategy;

        /// <summary>
        /// List of effects that can be played as a result of the shot.
        /// </summary>
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(FireEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private FireEffect[] _effectors = Array.Empty<FireEffect>();

        public event UnityAction Shoot;

        public int AmmoUsedPerShot { get; private set; } = 0;

        protected override void Awake()
        {
            base.Awake();

            foreach (FireEffect effect in _effectors)
            {
                if (effect is ConsumeAmmoEffect consumeAmmoEffect)
                {
                    AmmoUsedPerShot = consumeAmmoEffect.AmmoPerShot;
                }

                effect.Initialize(GetComponent<IHandheldItem>());
            }
        }

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.FiringMechanism = this;
                _shotStrategy.Enable(Gun);
                foreach(FireEffect effect in _effectors)
                {
                    if(effect is ConsumeAmmoEffect consumeAmmoEffect)
                    {
                        AmmoUsedPerShot = consumeAmmoEffect.AmmoPerShot;
                    }

                    effect.Enable(Gun);
                }
            }
        }
        public void Fire(float accuracy, IGunImpactEffectBehaviour impactEffect)
        {
            if (CheckCanFire() == false)
            {
                return;
            }

            foreach (FireEffect effector in _effectors)
            {
                effector.Play(Gun, default);
            }

            Shoot?.Invoke();

            foreach (ShotResult shotResult in _shotStrategy.Execute(Gun, impactEffect, accuracy))
            {
                
            }
        }

        private bool CheckCanFire()
        {
            if (Gun.Magazine.IsReloading)
            {
                return false;
            }

            if (Gun.Magazine.CurrentAmmoCount == 0)
            {
                Gun.DryFireEffect.TriggerDryFireEffect();
                return false;
            }

            return true;
        }

        public LaunchContext GetLaunchContext() => _shotStrategy.GetLaunchContext(Gun);
    }
}
using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Movement;
using Nexora.InteractionSystem;
using Nexora.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Nexora.FPSDemo
{
    public static partial class CharacterUtility
    {
        public static readonly BodyPart[] BodyParts = EnumUtility.GetAllEnumValues<BodyPart>();
        public static readonly int BodyPartsCount = BodyParts.Length; 

        public static BodyPart GetBodyPart(ItemDropPoint dropPoint)
        {
            return dropPoint switch
            {
                ItemDropPoint.Body => BodyPart.Head,
                _ => BodyPart.Feet
            };
        }
    }

    /// <summary>
    /// Represents part of character's body (e.g Head, Chest etc.)
    /// </summary>
    public enum BodyPart
    {
        Head = 0,
        Chest = 1,
        Hands = 2,
        Legs = 3,
        Feet = 4,
    }

    public interface ICharacter : 
        IParentBehaviour<ICharacter, ICharacterBehaviour>, 
        IDamageSource, 
        IInteractorContext,
        IItemActionContext
    {
        /// <summary>
        /// Name of the character.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Animator controller responsible for animating the character.
        /// </summary>
        IAnimatorController AnimatorController { get; }

        /// <summary>
        /// Audio player responsible for playing audios on the character.
        /// </summary>
        ICharacterAudioPlayer AudioPlayer { get; }

        /// <summary>
        /// Health controller of the character.
        /// </summary>
        IHealthController HealthController { get; }

        /// <summary>
        /// Gets <see cref="Transform"/> of the <paramref name="bodyPart"/> of the character.
        /// </summary>
        /// <param name="bodyPart">Body part.</param>
        /// <returns>Body part's transform.</returns>
        Transform GetTransformOfBodyPart(BodyPart bodyPart);

        /// <summary>
        /// Called when character is destroyed.
        /// </summary>
        event UnityAction<ICharacter> Destroyed;
    }

    [SelectionBase]
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public abstract class Character : 
        ParentBehaviour<ICharacter, ICharacterBehaviour>, 
        ICharacter
    {
        public IAnimatorController AnimatorController { get; private set; }
        public ICharacterAudioPlayer AudioPlayer { get; private set; }
        public IHealthController HealthController { get; private set; }
        public IInventory Inventory { get; private set; }

        public event UnityAction<ICharacter> Destroyed;

        public string DamageSourceName => Name;

        public virtual string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        static Character()
        {
            _componentToInterfacePairs = BuildComponentToInterfaceDictionary();
            _cachedComponents = new List<ICharacterBehaviour>();
        }

        protected override void Awake()
        {
            base.Awake();

            AudioPlayer = GetComponentInChildren<ICharacterAudioPlayer>(true) ?? new DefaultCharacterAudioPlayer();

            Inventory = GetComponentInChildren<IInventory>(true) ?? new NullInventory(gameObject);

            HealthController = GetComponentInChildren<IHealthController>() ?? NullHealthController.Instance;

            var animators = GetComponentsInChildren<IAnimatorController>(false);
            AnimatorController = animators.Length switch
            {
                0 => NullAnimator.Instance,
                1 => animators.First(),
                _ => new CompositeAnimator(animators)
            };

            DamageEventSystem.SubscribeSource(this);
        }

        protected virtual void OnDestroy()
        {
            Destroyed?.Invoke(this);
            DamageEventSystem.UnsubscribeSource(this);
        }

        public abstract Transform GetTransformOfBodyPart(BodyPart bodyPart);

        public override void AddBehaviour(ICharacterBehaviour behaviour) { }
        public override void RemoveBehaviour(ICharacterBehaviour behaviour) { }

        public bool TryGetContextInterface<T>(out T result) 
            where T : class
        {
            result = GetCC(typeof(T)) as T;
            result ??= GetComponentInChildren<T>();
            return result != null;
        }

        public void PlayAudio(AudioCue audio)
        {
            AudioPlayer.PlayClip(audio, BodyPart.Chest);
        }

        public void ThrowObject(Rigidbody throwedObjectBody, Vector3 dropForce, float dropTorque)
        {
            Vector3 characterVelocity = CalculateCharacterVelocity(this);

            Vector3 totalForce = dropForce + characterVelocity;
            Vector3 totalTorque = Random.rotation.eulerAngles.normalized * dropTorque;

            throwedObjectBody.AddForce(totalForce, ForceMode.VelocityChange);
            throwedObjectBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            StartInterpolationRoutine(this, throwedObjectBody);
        }

        private static Vector3 CalculateCharacterVelocity(ICharacter character)
        {
            Vector3 velocity = character.TryGetCC(out ICharacterMotor motor) ? motor.Velocity : Vector3.zero;
            velocity.y = Mathf.Abs(velocity.y);
            return velocity;
        }

        private static void StartInterpolationRoutine(IMonoBehaviour coroutineRunner, Rigidbody rigidbody)
        {
            if(rigidbody.interpolation == RigidbodyInterpolation.None)
            {
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                coroutineRunner.StartCoroutine(ResetInterpolationAfterDelay(rigidbody, 0.75f));
            }
        }

        private static IEnumerator ResetInterpolationAfterDelay(Rigidbody rigidbody, float delay)
        {
            yield return new Delay(delay);
            if(rigidbody != null)
            {
                rigidbody.interpolation = RigidbodyInterpolation.None;
            }
        }

        public Transform GetDropPointTransform(ItemDropPoint dropPoint) => GetTransformOfBodyPart(CharacterUtility.GetBodyPart(dropPoint));
    }
}
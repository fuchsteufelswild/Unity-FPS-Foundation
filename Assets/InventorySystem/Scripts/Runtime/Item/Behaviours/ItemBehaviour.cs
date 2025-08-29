using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Class to define a behaviour to an item. E.g ConsumableBehaviour, FuelBehaviour, CookableBehaviour, 
    /// anything that an item might have and can be used for, even CraftableBehaviour.
    /// </summary>
    /// <remarks>
    /// Even can be used to mark item behaviours through type in list.
    /// </remarks>
    [Serializable]
    public abstract class ItemBehaviour
    {
    }

    // Example classes what type of behaviours can be defined
    [Serializable]
    public sealed class ConsumableBehaviour : ItemBehaviour
    {
        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _healthRegenerateRange;

        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _hungerRegenerateRange;

        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _thirstRegenerateRange;

        public float HealthRegeneration => _healthRegenerateRange.GetRandomFromRange();
        public float HungerRegeneration => _healthRegenerateRange.GetRandomFromRange();
        public float ThirstRegeneration => _healthRegenerateRange.GetRandomFromRange();
    }

    [Serializable]
    public sealed class FuelBehaviour : ItemBehaviour
    {
        [SerializeField, MinMaxSlider(1f, 500f)]
        private Vector2 _fuelCapacityRange;

        [SerializeField, MinMaxSlider(1f, 10f)]
        private Vector2 _burnDurationRange;

        [SerializeField]
        private bool _producesAsh;

        public float FuelCapacity => _fuelCapacityRange.GetRandomFromRange();
        public float BurnDuration => _burnDurationRange.GetRandomFromRange();

        public bool ProducesAsh => _producesAsh;
    }
}
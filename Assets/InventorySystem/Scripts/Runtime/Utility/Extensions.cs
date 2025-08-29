using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public static partial class InventoryExtensions
    {
        /// <returns>
        /// The first <see cref="Slot"/> in any of the <see cref="IContainer"/> of the <paramref name="inventory"/>
        /// that passes the <paramref name="filter"/>.
        /// </returns>
        public static Slot GetSlot(this IInventory inventory, Func<Slot, bool> filter)
        {
            foreach (IContainer container in inventory.Containers)
            {
                Slot slot = container.GetSlot(filter);
                if (slot.IsValid)
                {
                    return slot;
                }
            }

            return Slot.None;
        }
    }

    public static partial class ContainerExtensions
    {
        public static bool IsFull(this ISlotStorage storage)
        {
            for(int i = 0; i < storage.SlotsCount; i++)
            {
                if(storage.GetItemAtIndex(i).HasItem == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEmpty(this ISlotStorage storage) => Mathf.Approximately(storage.CurrentWeight, 0f);

        /// <returns>The first slot matching the <paramref name="filter"/>.</returns>
        public static Slot GetSlot(this ISlotStorage storage, Func<Slot, bool> filter)
        {
            for (int i = 0; i < storage.SlotsCount; i++)
            {
                var slot = storage.GetSlot(i);
                if (filter(slot))
                {
                    return slot;
                }
            }

            return default;
        }

        /// <returns><see cref="Slot"/> at <paramref name="slotIndex"/>.</returns>
        public static Slot GetSlot(this ISlotStorage storage, int slotIndex) => new(storage, slotIndex);

        /// <typeparam name="T"><see cref="Type"/> of the constraint.</typeparam>
        /// <returns>If has a constraint that is of <see cref="Type"/> <typeparamref name="T"/>.</returns>
        public static bool HasConstraint<T>(this IContainerAddConstraintService addConstraintService)
            where T : ContainerAddConstraint
        {
            return TryGetConstraint<T>(addConstraintService, out _);
        }

        /// <summary>
        /// Searches for the <see cref="ContainerAddConstraint"/> of <see cref="Type"/> <typeparamref name="T"/>
        /// in the container.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the constraint.</typeparam>
        /// <returns>If has a constraint that is of <see cref="Type"/> <typeparamref name="T"/>.</returns>
        public static bool TryGetConstraint<T>(this IContainerAddConstraintService addConstraintService, out T constraint)
            where T : ContainerAddConstraint
        {
            constraint = GetConstraint<T>(addConstraintService);
            return constraint != null;
        }

        /// <typeparam name="T"><see cref="Type"/> of the constraint.</typeparam>
        /// <returns>Constraint that is of <see cref="Type"/> <typeparamref name="T"/>, <see langword="null"/> if not found.</returns>
        public static T GetConstraint<T>(this IContainerAddConstraintService addConstraintService)
            where T : ContainerAddConstraint
        {
            return GetConstraint<T>(addConstraintService.AddConstraints);
        }

        /// <typeparam name="T"><see cref="Type"/> of the constraint.</typeparam>
        /// <returns>Constraint that is of <see cref="Type"/> <typeparamref name="T"/>, <see langword="null"/> if not found.</returns>
        public static T GetConstraint<T>(this IReadOnlyList<ContainerAddConstraint> constraints)
            where T : ContainerAddConstraint
        {
            foreach (var constraint in constraints)
            {
                if (constraint is T)
                {
                    return (T)constraint;
                }
            }

            return null;
        }

        public static bool AllowsItem(this IReadOnlyList<ContainerAddConstraint> constraints, ItemDefinition itemDefinition)
            => constraints.AllowsItem(Item.CreateDummyItem(itemDefinition));

        public static bool AllowsItem(this IReadOnlyList<ContainerAddConstraint> constraints, IItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (constraints == null)
            {
                return true;
            }

            foreach (var constraint in constraints)
            {
                if (constraint.AllowsItem(null, item) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsRequiringTag(this IContainer container, DefinitionReference<ItemTagDefinition> tag)
            => IsRestrictingTag(container, tag, FilterConstraint.FilterPolicy.Require);

        public static bool IsExcludingTag(this IContainer container, DefinitionReference<ItemTagDefinition> tag)
            => IsRestrictingTag(container, tag, FilterConstraint.FilterPolicy.Exclude);

        private static bool IsRestrictingTag(
            this IContainer container, 
            DefinitionReference<ItemTagDefinition> tag, 
            FilterConstraint.FilterPolicy filterPolicy = FilterConstraint.FilterPolicy.Require)
        {
            return container.TryGetConstraint<TagConstraint>(out var tagConstraint) && tagConstraint != null
                && tagConstraint.FilteringPolicy == filterPolicy && tagConstraint.ConstrainElements.Contains(tag);
        }
    }

    public static partial class ItemDefinitionExtensions
    {
        /// <returns>If <see cref="ItemBehaviour"/> of <paramref name="type"/> is attached to <paramref name="item"/>.</returns>
        public static bool HasBehaviourOfType(this ItemDefinition item, Type type)
        {
            foreach (ItemBehaviour itemBehaviour in item.SharedItemBehaviours)
            {
                if (itemBehaviour.GetType() == type)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if <paramref name="item"/> has a <see cref="ItemBehaviour"/>
        /// of type <typeparamref name="T"/> attached.
        /// </summary>
        /// <typeparam name="T">Type of the behaviour.</typeparam>
        /// <returns>If any <see cref="ItemBehaviour"/> of type <typeparamref name="T"/> is attached to <paramref name="item"/>.</returns>
        public static bool HasBehaviourOfType<T>(this ItemDefinition item)
            where T : ItemBehaviour
        {
            return item.TryGetBehaviourOfType<T>(out _);
        }

        /// <summary>
        /// Gets <see cref="ItemBehaviour"/>, if <paramref name="item"/> has a <see cref="ItemBehaviour"/>
        /// of type <typeparamref name="T"/> attached.
        /// </summary>
        /// <typeparam name="T">Type of the behaviour.</typeparam>
        /// <param name="behaviour">Found behaviour, if not found <see langword="null"/>.</param>
        /// <returns>If any <see cref="ItemBehaviour"/> of type <typeparamref name="T"/> is attached to <paramref name="item"/>.</returns>
        public static bool TryGetBehaviourOfType<T>(this ItemDefinition item, out T behaviour)
            where T : ItemBehaviour
        {
            behaviour = item.GetBehaviourOfType<T>();
            return behaviour != null;
        }

        /// <summary>
        /// Gets <see cref="ItemBehaviour"/> of type <typeparamref name="T"/> from <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T">Type of the behaviour.</typeparam>
        /// <returns><see cref="ItemBehaviour"/> of type <typeparamref name="T"/> if found, else <see langword="null"/>.</returns>
        public static T GetBehaviourOfType<T>(this ItemDefinition item)
            where T : ItemBehaviour
        {
            foreach (ItemBehaviour itemBehaviour in item.SharedItemBehaviours)
            {
                if (itemBehaviour is T matchingBehaviour)
                {
                    return matchingBehaviour;
                }
            }

            return null;
        }

        /// <returns>If <see cref="ItemBehaviour"/> of <paramref name="type"/> is attached to <paramref name="item"/>.</returns>
        public static bool HasActionOfType(this ItemDefinition item, Type type)
        {
            foreach (ItemAction itemBehaviour in item.ItemActions)
            {
                if (itemBehaviour.GetType() == type)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if <paramref name="item"/> has a <see cref="ItemAction"/>
        /// of type <typeparamref name="T"/> attached.
        /// </summary>
        /// <typeparam name="T">Type of the action.</typeparam>
        /// <returns>If any <see cref="ItemAction"/> of type <typeparamref name="T"/> is attached to <paramref name="item"/>.</returns>
        public static bool HasActionOfType<T>(this ItemDefinition item)
            where T : ItemAction
        {
            return item.TryGetActionOfType<T>(out _);
        }

        /// <summary>
        /// Gets <see cref="ItemAction"/>, if <paramref name="item"/> has a <see cref="ItemAction"/>
        /// of type <typeparamref name="T"/> attached.
        /// </summary>
        /// <typeparam name="T">Type of the action.</typeparam>
        /// <param name="itemAction">Found action, if not found <see langword="null"/>.</param>
        /// <returns>If any <see cref="ItemAction"/> of type <typeparamref name="T"/> is attached to <paramref name="item"/>.</returns>
        public static bool TryGetActionOfType<T>(this ItemDefinition item, out T itemAction)
            where T : ItemAction
        {
            itemAction = item.GetActionOfType<T>();
            return itemAction != null;
        }

        /// <summary>
        /// Gets <see cref="ItemAction"/> of type <typeparamref name="T"/> from <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T">Type of the action.</typeparam>
        /// <returns><see cref="ItemAction"/> of type <typeparamref name="T"/> if found, else <see langword="null"/>.</returns>
        public static T GetActionOfType<T>(this ItemDefinition item)
            where T : ItemAction
        {
            foreach(ItemAction itemAction in item.ItemActions)
            {
                if(itemAction is T matchingAction)
                {
                    return matchingAction;
                }
            }

            return null;
        }

        /// <returns>If <paramref name="item"/> has a <see cref="DynamicItemPropertyGenerator"/> with type of <paramref name="targetProperty"/>.</returns>
        public static bool HasProperty(this ItemDefinition item, DefinitionReference<DynamicItemPropertyDefinition> targetProperty)
        {
            var allPropertyGenerators = item.DynamicItemPropertyGenerators;

            for(int i = 0; i < allPropertyGenerators.Count; i++)
            {
                if (allPropertyGenerators[i].PropertyDefinition == targetProperty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates list of <see cref="DynamicItemProperty"/> using the list <see cref="DynamicItemPropertyGenerator"/>.
        /// </summary>
        public static DynamicItemProperty[] GenerateProperties(this ItemDefinition item)
        {
            var allPropertyGenerators = item.DynamicItemPropertyGenerators;
            var generatedProperties = new DynamicItemProperty[allPropertyGenerators.Count];

            for (int i = 0; i < allPropertyGenerators.Count; i++)
            {
                generatedProperties[i] = allPropertyGenerators[i].GenerateProperty();
            }

            return generatedProperties;
        }
    }
}
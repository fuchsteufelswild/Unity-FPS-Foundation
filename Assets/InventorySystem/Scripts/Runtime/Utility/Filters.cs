using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Static collection of methods that return various commonly used filters for <see cref="IItem"/>. 
    /// In the case complex queries <see cref="Builder"/> can be used as well, it is mainly the same thing
    /// but with more readability and method chaining. Use static methods for simple cases, use <see cref="Builder"/>
    /// for the complex cases or when you need to store the filter and combine it with other filters later.
    /// </summary>
    /// <remarks>
    /// <b>Caution:</b> Using these methods in a performance-sensitive loops might hurt <b>GC</b>, if that would 
    /// be the case then consider caching the frequently used filters. 
    /// E.g
    /// <code>
    ///     <![CDATA[
    ///         string swordFilter = ItemFilters.WithName("Sword");
    ///     ]]>
    /// </code>
    /// </remarks>
    public static partial class ItemFilters
    {
        /// <summary>
        /// Returns a filter, which filters items by exact name (case-insensitive).
        /// </summary>
        public static Func<IItem, bool> WithName(string name)
        {
            return item => StringComparer.OrdinalIgnoreCase.Equals(item.Name, name);
        }

        /// <summary>
        /// Returns a filter, which filters items by exact id match.
        /// </summary>
        public static Func<IItem, bool> WithID(int id)
        {
            return item => item.ID == id;
        }

        /// <summary>
        /// Returns a filter, which filter items that have the weight less than <paramref name="maxWeight"/>.
        /// </summary>
        public static Func<IItem, bool> WithWeightLessThan(float maxWeight)
        {
            return item => item.Weight < maxWeight;
        }

        /// <summary>
        /// Returns a filter, which filter items that are stackable.
        /// </summary>
        public static Func<IItem, bool> IsStackable()
        {
            return null;
        }

        /// <summary>
        /// Returns a filter, which filter items have the specified category (case-insensitive).
        /// </summary>
        public static Func<IItem, bool> WithCategory(string category)
        {
            return null;
        }

        /// <summary>
        /// Returns a filter, which filter items have the specified tag (case-insensitive).
        /// </summary>
        public static Func<IItem, bool> WithTag(string tagName)
        {
            return null;
            // return item => StringComparer.OrdinalIgnoreCase.Equals(item.Definition.Tag, tagName); [Revisit]
        }

        /// <summary>
        /// Returns a filter, which filter items whose name contains the given 
        /// <paramref name="substring"/> (case-insensitive).
        /// </summary>
        public static Func<IItem, bool> WithNameContaining(string substring)
        {
            return item => item.Name?.Contains(substring, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Returns a filter, which is the combinations of all the <paramref name="filters"/>.
        /// </summary>
        public static Func<IItem, bool> And(params Func<IItem, bool>[] filters)
        {
            return item => filters.All(filter => filter(item));
        }

        /// <summary>
        /// Returns a filter, which returns true if any of the filters in the <paramref name="filters"/> pass out.
        /// </summary>
        public static Func<IItem, bool> Or(params Func<IItem, bool>[] filters)
        {
            return item => filters.Any(filter => filter(item));
        }


        /// <summary>
        /// Starts new builder for the method chaining.
        /// </summary>
        public static Builder Start() => new Builder();

        /// <summary>
        /// Builder that can be used to build <see cref="ItemFilters"/> as a method chaining.
        /// </summary>
        /// <remarks>
        /// For simple cases combining filters consider using <see cref="ItemFilters.And(Func{IItem, bool}[])"/>.
        /// </remarks>
        public sealed partial class Builder
        {
            private readonly List<Func<IItem, bool>> _filters = new();

            internal Builder() { }

            public Builder WithName(string name)
            {
                _filters.Add(ItemFilters.WithName(name));
                return this;
            }

            public Builder WithID(int id)
            {
                _filters.Add(ItemFilters.WithID(id));
                return this;
            }

            public Builder WithWeightLessThan(float maxWeight)
            {
                _filters.Add(ItemFilters.WithWeightLessThan(maxWeight));
                return this;
            }

            public Builder IsStackable()
            {
                _filters.Add(ItemFilters.IsStackable());
                return this;
            }

            public Builder WithCategory(string category)
            {
                _filters.Add(ItemFilters.WithCategory(category));
                return this;
            }

            public Builder WithTag(string tagName)
            {
                _filters.Add(ItemFilters.WithTag(tagName));
                return this;
            }

            public Builder WithNameContaining(string substring)
            {
                _filters.Add(ItemFilters.WithNameContaining(substring));
                return this;
            }

            public Func<IItem, bool> Build()
            {
                return item => _filters.All(filter => filter(item));
            }
        }
    }

    /// <summary>
    /// Static collection of filters that are commonly used for filtering <see cref="Slot"/>.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ItemFilters" path="/remarks"/>
    /// </remarks>
    public static partial class SlotFilters
    {
        public static Func<Slot, bool> Empty = slot => slot.HasItem == false;

        public static Func<Slot, bool> FirstSlot = slot => slot.SlotIndex == 0;

        public static Func<Slot, bool> LastSlot = slot => slot.SlotIndex == slot.Storage.SlotsCount - 1;

        public static Func<Slot, bool> EmptyOrLastSlot = Or(Empty, LastSlot);

        public static Func<Slot, bool> Locked = slot => slot.IsLocked;

        public static Func<Slot, bool> And(params Func<Slot, bool>[] filters)
        {
            return slot => filters.All(filter => filter(slot));
        }

        public static Func<Slot, bool> Or(params Func<Slot, bool>[] filters)
        {
            return slot => filters.Any(filter => filter(slot));
        }

        /// <summary>
        /// Returns a filter, which filter slots that have the following underlying <paramref name="item"/>.
        /// </summary>
        public static Func<Slot, bool> WithItem(IItem item)
        {
            return slot => slot.Item == item;
        }

        /// <summary>
        /// Returns a filter, which filter slots that have an underlying item with the given <paramref name="itemID"/>.
        /// </summary>
        public static Func<Slot, bool> WithItemID(int itemID)
        {
            return slot => slot.Item?.ID == itemID;
        }

        /// <summary>
        /// Returns a filter, which filter slots that have an underlying item with the given.
        /// </summary>
        public static Func<Slot, bool> WithItemDefinition()
        {
            return null;
        }

        /// <summary>
        /// Returns a filter, which filter slots that have an underlying item that passes the given <paramref name="itemFilter"/>.
        /// </summary>
        public static Func<Slot, bool> WithItemFilter(Func<IItem, bool> itemFilter)
        {
            return slot => slot.TryGetItem(out IItem item) && itemFilter(item);
        }
    }

    /// <summary>
    /// Static collection of filters that are commonly used for filtering <see cref="IContainer"/>.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ItemFilters" path="/remarks"/>
    /// </remarks>
    public static class ContainerFilters
    {
        public static Func<IContainer, bool> WithName(string name)
        {
            return container => StringComparer.OrdinalIgnoreCase.Equals(container.Name, name);
        }

        public static Func<IContainer, bool> WithoutTag =
            container => container.TryGetConstraint(out TagConstraint tagConstraint) == false || tagConstraint.HasConstrainElements == false;

        /// <summary>
        /// Returns a filter, that would filter containers based on if they have any <see cref="TagConstraint"/>
        /// that would constrain the <paramref name="targetTag"/>.
        /// </summary>
        /// <param name="targetTag"></param>
        /// <returns></returns>
        public static Func<IContainer, bool> RequiringTag(DefinitionReference<ItemTagDefinition> targetTag)
        {
            return container => container.TryGetConstraint(out TagConstraint tagConstraint)
                ? targetTag.IsNull || tagConstraint.ConstrainElements.Contains(targetTag) && tagConstraint.FilteringPolicy == FilterConstraint.FilterPolicy.Require
                : targetTag.IsNull;
        }

        /// <summary>
        /// Returns a filter, that would filter containers based on if they have any <see cref="TagConstraint"/>
        /// that would constrain the <paramref name="targetTag"/>.
        /// </summary>
        /// <param name="targetTag"></param>
        /// <returns></returns>
        public static Func<IContainer, bool> ExcludingTag(DefinitionReference<ItemTagDefinition> targetTag)
        {
            return container => container.TryGetConstraint(out TagConstraint tagConstraint)
                ? targetTag.IsNull == false && tagConstraint.ConstrainElements.Contains(targetTag) && tagConstraint.FilteringPolicy == FilterConstraint.FilterPolicy.Exclude
                : false;
        }
    }

    public static partial class ItemCategoryFilters
    {
        public static Func<ItemCategoryDefinition, bool> NotEmpty = category => category.Members.IsEmpty();

        public static Func<ItemCategoryDefinition, bool> WithItemDefinition(Func<ItemDefinition, bool> itemFilter)
        {
            return category => category.Members.Any(itemFilter);
        }

        public static Func<ItemCategoryDefinition, bool> And(params Func<ItemCategoryDefinition, bool>[] filters)
        {
            return category => filters.All(filter => filter(category));
        }
    }
}
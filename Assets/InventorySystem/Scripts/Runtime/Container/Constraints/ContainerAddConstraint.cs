using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Defines rules for whether and how many specific <see cref="IItem"/> can be added to a <see cref="IContainer"/>.
    /// Each constraint can specify custom logic and a rejection message.
    /// </summary>
    [Serializable]
    public abstract class ContainerAddConstraint : ScriptableObject
    {
        [Tooltip("The message shown when this constraint prevents and item from being added.")]
        [SerializeField]
        private string _rejectionMessage;

        public const string DefaultRejection = "Cannot add item";
        public const string InventoryFull = "Inventory is full";
        public const string WeightLimitExceeded = "Cannot carry more weight";
        public const string InvalidItem = "Item is invalid";

        protected const string AssetMenuPath = "Nexora/Inventory/Add Constraints/";

        public string RejectionMessage 
            => string.IsNullOrEmpty(_rejectionMessage) ? DefaultRejection : _rejectionMessage;

        /// <summary>
        /// Quick check if <b>any</b> amount of <paramref name="item"/> can be added <paramref name="container"/>.
        /// </summary>
        public bool AllowsItem(IContainer container, IItem item)
            => GetAllowedCount(container, item, 1) > 0;

        /// <summary>
        /// Calculates how many of the <paramref name="requestedAmount"/> of <paramref name="item"/>
        /// can be added to the <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Container the items will be placed in.</param>
        /// <param name="item">The item that will be placed in to the container.</param>
        /// <param name="requestedAmount">How many items do we want to place?</param>
        /// <returns>How many items can be placed given this constraint rule (MAX is the <paramref name="requestedAmount"/>)</returns>
        public abstract int GetAllowedCount(IContainer container, IItem item, int requestedAmount);

        /// <inheritdoc cref="GetAllowedCount(IContainer, IItem, int)"/>
        public int GetAllowedCount(IContainer container, ItemStack itemStack)
            => GetAllowedCount(container, itemStack.Item, itemStack.Quantity);
    }

    /// <summary>
    /// Filters based on a policy of either require <b>any</b> of the proposed constraints,
    /// or exclude <b>all</b> of the proposed constraints.
    /// </summary>
    [Serializable]
    public abstract class FilterConstraint : ContainerAddConstraint
    {
        /// <summary>
        /// Filtering policy to whether <b>require</b> or <b>exclude</b> the tags defined.
        /// </summary>
        public enum FilterPolicy
        {
            Require = 0,
            Exclude = 1
        }

        [SerializeField]
        protected FilterPolicy _filterPolicy = FilterPolicy.Require;

        public FilterPolicy FilteringPolicy => _filterPolicy;
    }

    [Serializable]
    public abstract class ClassTypeFilterConstraint<T, TBaseType> :
        FilterConstraint
        where T : ClassTypeFilterConstraint<T, TBaseType>
        where TBaseType : class
    {
        /// <summary>
        /// Creates a new <typeparamref name="TBaseType"/> with specified <paramref name="filterPolicy"/> 
        /// and the <typeparamref name="TBaseType"/> <paramref name="constraintTypes"/>.
        /// </summary>
        protected static T CreateNew(FilterPolicy filterPolicy, params Type[] constraintTypes)
        {
            foreach(Type type in constraintTypes)
            {
                if(typeof(TBaseType).IsAssignableFrom(type) == false)
                {
                    throw new ArgumentException($"The type '{type.Name}' does not implement '{typeof(TBaseType).Name}'");
                }
            }

            var instance = CreateInstance<T>();
            instance._filterPolicy = filterPolicy;
            return instance;
        }
    }

    [Serializable]
    public abstract class DefinitionFilterConstraint<T, TDefinition> : 
        FilterConstraint
        where T : DefinitionFilterConstraint<T, TDefinition>
        where TDefinition : Definition<TDefinition>
    {
        [ReorderableList(HasLabels = false)]
        [DefinitionReference(NullElement = "")]
        [SerializeField]
        protected DefinitionReference<TDefinition>[] _constraintElements;

        public DefinitionReference<TDefinition>[] ConstrainElements => _constraintElements;

        public bool HasConstrainElements => _constraintElements.IsEmpty() == false;

        /// <summary>
        /// Creates a new <typeparamref name="T"/> with specified <paramref name="filterPolicy"/> 
        /// and the <typeparamref name="TDefinition"/> <paramref name="constrainElements"/>.
        /// </summary>
        protected static T CreateNew(FilterPolicy filterPolicy, params DefinitionReference<TDefinition>[] constrainElements)
        {
            var instance = CreateInstance<T>();
            instance._filterPolicy = filterPolicy;
            instance._constraintElements = constrainElements;
            return instance;
        }

        protected int HandleRequireFilter(DefinitionReference<TDefinition> target, int requestedAmount)
        {
            foreach (var constrain in _constraintElements)
            {
                if (constrain == target)
                {
                    return requestedAmount;
                }
            }

            return 0;
        }

        protected int HandleExcludeFilter(DefinitionReference<TDefinition> target, int requestedAmount)
        {
            foreach (var constrain in _constraintElements)
            {
                if (constrain == target)
                {
                    return 0;
                }
            }

            return requestedAmount;
        }
    }
}
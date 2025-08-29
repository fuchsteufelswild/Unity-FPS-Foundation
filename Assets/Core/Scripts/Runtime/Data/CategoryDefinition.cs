using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Defines a category for different types of <see cref="CategoryMemberDefinition{TMember, TCategory}"/>,
    /// it is like having an ItemCategory. E.g Category -> Backpacks, and members would be
    /// different backpack definitions.
    /// <br></br>
    /// Provides <b>Editor-Only</b> methods to provide interface for adding/removing members
    /// from <b>Editor</b> codes. Keeps member list always valid by removing broken(null or different category)
    /// members.
    /// </summary>
    /// <remarks>
    /// <para name="MutuallyRecursiveGenerics">
    /// Using powerful <b>Mutually Recursive Generics</b> technique to create
    /// a <b>strongly-typed, two-way link</b>. Without this we would need have
    /// reference to upper base class and perform casts, which is error-prone
    /// and cumbersome to work with.
    /// <br></br>
    /// Itself is also a <see cref="Definition{T}"/>.
    /// </para>
    /// It is not <see langword="abstract"/> to have junk drawer.
    /// </remarks>
    /// <typeparam name="TCategory">Category Type.</typeparam>
    /// <typeparam name="TMember">Member Type.</typeparam>
    public class CategoryDefinition<TCategory, TMember> :
        Definition<TCategory>
        where TCategory : CategoryDefinition<TCategory, TMember>
        where TMember : CategoryMemberDefinition<TMember, TCategory>
    {
        [Tooltip("Icon to define the category.")]
        [SerializeField]
        [SpriteThumbnail(height:128f)]
        private Sprite _categoryIcon;

        [SerializeField]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private TMember[] _members;

        public TMember[] Members => _members;

        public override Sprite Icon => _categoryIcon;

#if UNITY_EDITOR
        /// <summary>
        /// Sets the member array to <paramref name="members"/> and
        /// clears out the broken members.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="IEditorValidatable.RunValidation" path="/remarks"/>
        /// </remarks>
        public void SetMembersArray(TMember[] members)
        {
            members ??= Array.Empty<TMember>();
            
            if(members != _members)
            {
                _members = members;
                RemoveBrokenMembers();
                EditorUtility.SetDirty(this);
            }
        }

        /// <remarks>
        /// <inheritdoc cref="IEditorValidatable.RunValidation" path="/remarks"/>
        /// </remarks>
        public void AddMember(TMember member)
        {
            if(member == null)
            {
                return;
            }

            if(_members.Contains(member) == false)
            { 
                UnityEditor.ArrayUtility.Add(ref _members, member);
                EditorUtility.SetDirty(this);
                RemoveBrokenMembers();
            }
        }

        /// <remarks>
        /// <inheritdoc cref="IEditorValidatable.RunValidation" path="/remarks"/>
        /// </remarks>
        public void RemoveMember(TMember member)
        {
            if(_members.Contains(member))
            {
                UnityEditor.ArrayUtility.Remove(ref _members, member);
                EditorUtility.SetDirty(this);
                RemoveBrokenMembers();
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// <inheritdoc path="/summary"/>
        /// </summary>
        /// <remarks>
        /// <inheritdoc path="/remarks"/>
        /// </remarks>
        public override void EditorValidate(in EditorValidationArgs validationArgs)
        {
            base.EditorValidate(in validationArgs);

            if(validationArgs.IsInEditor)
            {
                if(validationArgs.ValidationTrigger is ValidationTrigger.ManualRefresh)
                {
                    RemoveBrokenMembers();
                }
            }
            else
            {
                if(validationArgs.ValidationTrigger is ValidationTrigger.Creation or ValidationTrigger.Duplication)
                {
                    _members = Array.Empty<TMember>();
                }
            }
        }

        /// <summary>
        /// Removes all <typeparamref name="TMember"/> from member list,
        /// that are either <see langword="null"/>
        /// or belongs to category other than <see langword="this"/>.
        /// </summary>
        private void RemoveBrokenMembers()
        {
            for(int i = _members.Length - 1; i >= 0; i--)
            {
                if (_members[i] == null || _members[i].OwningCategory != this)
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref _members, i);
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }

    /// <summary>
    /// Class to define <see cref="Definition{T}"/> that belongs to a specific category
    /// <see cref="CategoryDefinition{TCategory, TMember}"/>. Used to make grouping of <see cref="Definition{T}"/>
    /// better, easier to work with and less error-prone.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="CategoryDefinition{TCategory, TMember}" path="/remarks/para[@name='MutuallyRecursiveGenerics']"/>
    /// </remarks>
    /// <typeparam name="TMember">Member Type.</typeparam>
    /// <typeparam name="TCategory">Category Type.</typeparam>
    public abstract class CategoryMemberDefinition<TMember, TCategory> :
        Definition<TMember>
        where TMember : CategoryMemberDefinition<TMember, TCategory>
        where TCategory : CategoryDefinition<TCategory, TMember>
    {
        [Tooltip("Category this definition belongs to.")]
#if UNITY_EDITOR
        [EditorButton(nameof(RefreshCategory))]
#endif
        [SerializeField]
        private TCategory _owningCategory;

        [NonSerialized]
        private string _cachedDisplayName;

        public TCategory OwningCategory => _owningCategory;
        public bool BelongsToCategory => _owningCategory != null;

        public override string DisplayName
        {
            get
            {
                _cachedDisplayName ??= BuildDisplayName();
                return _cachedDisplayName;
            }
        }

        private string BuildDisplayName()
            => (OwningCategory?.Name ?? "Null Category") + " / " + Name;

#if UNITY_EDITOR
        public void RefreshCategory()
        {
            if(_owningCategory != null)
            {
                return;
            }

            if (DefinitionRegistry<TCategory>.TryGetFirst(predicate: null, out TCategory category))
            {
                SetCategory(category);
            }
        }

        /// <summary>
        /// Removes self from the previous <see cref="CategoryDefinition{C, M}"/>
        /// and adds to the <paramref name="category"/>.
        /// </summary>
        /// <param name="category"></param>
        public void SetCategory(TCategory category)
        {
            _owningCategory?.RemoveMember((TMember)this);
            _owningCategory = category;
            _owningCategory?.AddMember((TMember)this);

            _cachedDisplayName = BuildDisplayName();
        }
#endif

#if UNITY_EDITOR
        public override void EditorValidate(in EditorValidationArgs validationArgs)
        {
            base.EditorValidate(validationArgs);

            if(validationArgs.IsInEditor)
            {
                _owningCategory?.AddMember((TMember)this);
            }
            else
            {
                if(validationArgs.ValidationTrigger is ValidationTrigger.Creation)
                {
                    if(DefinitionRegistry<TCategory>.TryGetFirst(predicate:null, out TCategory category))
                    {
                        category.AddMember((TMember)this);
                    }
                }
                else if(validationArgs.ValidationTrigger is ValidationTrigger.Duplication)
                {
                    _owningCategory?.AddMember((TMember)this);
                }
            }
        }
#endif
    }
}

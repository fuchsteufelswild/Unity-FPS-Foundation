namespace Nexora
{

#if UNITY_EDITOR

    public enum ValidationTrigger
    {
        /// <summary>
        /// Newly created asset.
        /// </summary>
        Creation = 0,

        /// <summary>
        /// Duplicated from another asset.
        /// </summary>
        Duplication = 1,

        /// <summary>
        /// Explicit user action (e.g button click).
        /// </summary>
        ManualRefresh = 2
    }

    /// <summary>
    /// Defines why and from where validation is originated.
    /// </summary>
    public readonly struct EditorValidationArgs
    {
        public readonly ValidationTrigger ValidationTrigger;
        public readonly bool IsInEditor;

        public EditorValidationArgs(ValidationTrigger validationTrigger, bool isInEditor)
        {
            ValidationTrigger = validationTrigger;
            IsInEditor = isInEditor;
        }
    }
#endif

    /// <summary>
    /// Implement this to provide validation logic that can be used by Editor classes,
    /// provides interface for custom Editor scripts.
    /// </summary>
    public interface IEditorValidatable
    {
#if UNITY_EDITOR
        /// <summary>
        /// Validation logic to perform, similar to Unity's <b>OnValidate</b>,
        /// but it is called from user code and timings are up to the code.
        /// </summary>
        /// <remarks>
        /// <b>Only use from Editor scripts, does not work in runtime!</b>
        /// </remarks>
        void RunValidation();
#endif
    }
}

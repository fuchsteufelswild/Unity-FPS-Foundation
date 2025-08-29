using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Use this attribute to select from parameters using same parameter type <see cref="AnimatorControllerParameterType"/>
    /// from <see cref="Animator"/> that is available in parent/self/children hierarchy of the object.
    /// </summary>
    /// <remarks>
    /// It can be used to bind fields to <see cref="AnimatorControllerParameterType"/> and work together.
    /// Or just pass in <see cref="AnimatorControllerParameterType"/>.
    /// <br></br>
    /// <b>Field must be type of string.</b>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class AnimatorParameterAttribute : PropertyAttribute
    {
        public AnimatorControllerParameterType ParameterType { get; set; }
        public string ParameterTypeFieldName { get; }
        
        /// <summary>
        /// This variable in used drawer to define the selected index in the <see cref="Animator.parameters"/>,
        /// where all parameters match the type <see cref="ParameterType"/>.
        /// </summary>
        /// <remarks>
        /// It is attribute-wise, so to not buggy situations arise when using more than one attribute in the
        /// same Editor window.
        /// </remarks>
        public int SelectedParameterIndex { get; set; }

        /// <summary>
        /// Initialize with the type explicitly.
        /// </summary>
        public AnimatorParameterAttribute(AnimatorControllerParameterType parameterType)
        {
            ParameterType = parameterType;
            ParameterTypeFieldName = string.Empty;
        }

        /// <summary>
        /// Use this constructor to bind this field to <paramref name="parameterTypeFieldName"/>,
        /// namely we will be retrieving <see cref="AnimatorControllerParameterType"/> from that field.
        /// </summary>
        /// <param name="parameterTypeFieldName">
        /// The name of the field we will be retrieving the <see cref="AnimatorControllerParameterType"/> from.
        /// </param>
        public AnimatorParameterAttribute(string parameterTypeFieldName)
        {
            ParameterTypeFieldName = parameterTypeFieldName;
        }
    }
}
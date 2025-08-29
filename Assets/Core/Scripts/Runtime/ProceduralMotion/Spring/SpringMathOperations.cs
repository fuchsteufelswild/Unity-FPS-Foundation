using System;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Provides an interface for a value type to have some arithmetic operations.
    /// If we use a generic type in a class, we can't do any mathmetical operations
    /// with itself or with int, float etc.. To fix this problem, we need to have an
    /// interface which defines these operations and apply them. It is tedious and 
    /// not the most beatiful looking solution but it is necessary with current C# version.
    /// </summary>
    /// <typeparam name="T">Value type, (int, float, Vector3 etc.)</typeparam>
    public interface ISpringMathOperations<T>
        where T : struct
    {
        T Add(T left, T right);
        T Multiply(T left, float right);
        T Negate(T left);
    }

    /// <summary>
    /// Factory to create different <see cref="ISpringMathOperations{T}"/>. 
    /// Lazy creation is taking place and the result is cached so no more than one creation takes place.
    /// Can easily be extended when in need of operations for a new class.
    /// </summary>
    public static class SpringMathOperationsFactory
    {
        private static ISpringMathOperations<float> floatSpringMathOperations;
        private static ISpringMathOperations<Vector2> vector2SpringMathOperations;
        private static ISpringMathOperations<Vector3> vector3SpringMathOperations;

        public static ISpringMathOperations<T> Create<T>()
            where T : struct
        {
            return typeof(T) switch
            {
                Type t when t == typeof(float) => (ISpringMathOperations<T>)(floatSpringMathOperations ??= new FloatSpringMathOperations()),
                Type t when t == typeof(Vector2) => (ISpringMathOperations<T>)(vector2SpringMathOperations ??= new Vector2SpringMathOperations()),
                Type t when t == typeof(Vector3) => (ISpringMathOperations<T>)(vector3SpringMathOperations ??= new Vector3SpringMathOperations()),
                _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
            };
        }

        private class FloatSpringMathOperations
            : ISpringMathOperations<float>
        {
            public float Add(float left, float right) => left + right;
            public float Multiply(float left, float right) => left * right;
            public float Negate(float left) => -left;
        }

        private class Vector2SpringMathOperations
            : ISpringMathOperations<Vector2>
        {
            public Vector2 Add(Vector2 left, Vector2 right) => left + right;
            public Vector2 Multiply(Vector2 left, float right) => left * right;
            public Vector2 Negate(Vector2 left) => -left;
        }

        private class Vector3SpringMathOperations
            : ISpringMathOperations<Vector3>
        {
            public Vector3 Add(Vector3 left, Vector3 right) => left + right;
            public Vector3 Multiply(Vector3 left, float right) => left * right;
            public Vector3 Negate(Vector3 left) => -left;
        }
    }
}

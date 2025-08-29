using System;
using System.Linq;
using UnityEngine;

namespace Nexora
{
    public static class UnityUtils
    {
        private static Camera _cachedCamera;

        public static bool IsQuitting { get; private set; }

        public static Camera CachedCamera
        {
            get
            {
                if (_cachedCamera == null)
                {
                    _cachedCamera = Camera.main;
                }

                return _cachedCamera;
            }
        }

        public static void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public static void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            IsQuitting = false;
            Application.quitting += Quit;

            static void Quit()
            {
                IsQuitting = true;
                Application.quitting -= Quit;
            }
        }

        /// <summary>
        /// Safely checks if <paramref name="obj"/> points to a valid(<see langword="not null"/>)
        /// <see cref="UnityEngine.Object"/> class.
        /// </summary>
        /// <remarks>
        /// We must perform it this way, because in the case of interfaces that might be implemented
        /// by a <see cref="MonoBehaviour"/>, and we keep a reference to that interface. In this case,
        /// when a <see cref="UnityEngine.Object.Destroy(Object)"/> call is made for an <see cref="Object"/>
        /// then it is not immediately <see langword="null"/> in C#. To prevent fake null cases, this method
        /// <b>must</b> be used for cases when interface might reference to a <see cref="Object"/>.
        /// </remarks>
        /// <returns>
        /// True, if underlying Unity object is <see langword="not null"/>, or if it is a plain C# class and <see langword="not null"/>.
        /// </returns>
        public static bool IsValidUnityObject<T>(T obj)
            where T : class
        {
            // Plain C# class or Unity object, it is null
            if(obj == null)
            {
                return false;
            }

            // Does the referenced object is a Unity object?
            if(obj is UnityEngine.Object unityObject)
            {
                // Overloaded Unity's check for the object
                return unityObject != null;
            }

            // Plain C# class and not null
            return true;
        }
    }

    public static class EnumUtility
    {
        public static T[] GetAllEnumValues<T>() 
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }
}

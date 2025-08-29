using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Represents snapshot of a <see cref="Rigidbody"/>, including components of
    /// <see cref="Rigidbody.position"/>, <see cref="Rigidbody.rotation"/>, 
    /// <see cref="Rigidbody.linearVelocity"/>, <see cref="Rigidbody.angularVelocity"/>
    /// </summary>
    [Serializable]
    public readonly struct RigidbodySnapshot
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public readonly Vector3 LinearVelocity;
        public readonly Vector3 AngularVelocity;

        public RigidbodySnapshot(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity)
        {
            Position = position;
            Rotation = rotation;
            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;
        }

        public RigidbodySnapshot(Rigidbody rigidbody)
        {
            Position = rigidbody.position;
            Rotation = rigidbody.rotation;
            LinearVelocity = rigidbody.linearVelocity;
            AngularVelocity = rigidbody.angularVelocity;
        }

        public static RigidbodySnapshot[] ExtractSnapshots(ReadOnlySpan<Rigidbody> rigidbodies)
        {
            if(rigidbodies == null || rigidbodies.Length == 0)
            {
                return Array.Empty<RigidbodySnapshot>();
            }

            var snapshots = new RigidbodySnapshot[rigidbodies.Length];
            for(int i = 0; i < rigidbodies.Length; i++)
            {
                snapshots[i] = new RigidbodySnapshot(rigidbodies[i]);
            }

            return snapshots;
        }

        public static void ApplySnapshots(
            ReadOnlySpan<RigidbodySnapshot> snapshots,
            ReadOnlySpan<Rigidbody> rigidbodies)
        {
            if(snapshots == null || snapshots.IsEmpty)
            {
                return;
            }

            for (int i = 0; i < snapshots.Length; i++)
            {
                ApplySnapshot(in snapshots[i], rigidbodies[i]);
            }
        }

        public static void ApplySnapshot(in RigidbodySnapshot snapshot, Rigidbody rigidbody)
        {
            rigidbody.position = snapshot.Position;
            rigidbody.rotation = snapshot.Rotation;
            rigidbody.linearVelocity = snapshot.LinearVelocity;
            rigidbody.angularVelocity = snapshot.AngularVelocity;
        }
    }
}
using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Represents snapshot of a <see cref="Transform"/> local values, including components of position, rotation, and scale
    /// </summary>
    [Serializable]
    public readonly struct LocalTransformSnapshot : IEquatable<LocalTransformSnapshot>
    {
        [Flags]
        public enum Components
        {
            None = 0,
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
            All = Position | Rotation | Scale
        }

        // Snapshot
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;

        public static LocalTransformSnapshot Identity
            => new LocalTransformSnapshot(Vector3.zero, Quaternion.identity, Vector3.one);

        public LocalTransformSnapshot(Transform transform)
        {
            if(transform == null)
            {
                throw new ArgumentNullException("Snapshot transform cannot be null");
            }

            Position = transform.position;
            Rotation = transform.rotation;
            Scale = transform.localScale;
        }

        public LocalTransformSnapshot(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static LocalTransformSnapshot[] ExtractSnaphots(ReadOnlySpan<Transform> transforms)
        {
            if (transforms.IsEmpty)
            {
                return Array.Empty<LocalTransformSnapshot>();
            }

            var snapshots = new LocalTransformSnapshot[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                snapshots[i] = new LocalTransformSnapshot(transforms[i]);
            }

            return snapshots;
        }

        public static void ApplySnapshots(
            ReadOnlySpan<LocalTransformSnapshot> snapshots,
            ReadOnlySpan<Transform> transforms,
            Components snapshotFlags = Components.All)
        {
            if (snapshots == null || snapshots.IsEmpty)
            {
                return;
            }

            for (int i = 0; i < snapshots.Length; i++)
            {
                var snapshot = snapshots[i];
                var transform = transforms[i];

                ApplySnapshot(in snapshot, transform, snapshotFlags);
            }
        }

        public static void ApplySnapshot(
            in LocalTransformSnapshot snaphot, 
            Transform transform, 
            Components snapshotFlags = Components.All)
        {
            if(ShouldApplyComponent(Components.Position, snapshotFlags))
            {
                transform.localPosition = snaphot.Position;
            }
            if(ShouldApplyComponent(Components.Rotation, snapshotFlags))
            {
                transform.localRotation = snaphot.Rotation;
            }
            if(ShouldApplyComponent(Components.Scale, snapshotFlags))
            {
                transform.localScale = snaphot.Scale;
            }

            bool ShouldApplyComponent(Components componentFlag, Components snapshotFlags)
                => (componentFlag & snapshotFlags) == componentFlag;
        }

        public override string ToString() 
            => $"Position: {Position} | Rotation: {Rotation.eulerAngles} | Scale: {Scale}";


        public override bool Equals(object obj)
        {
            return obj is LocalTransformSnapshot other && Equals(other);
        }

        public bool Equals(LocalTransformSnapshot other)
        {
            return Position.Equals(other.Position)
                && Rotation.Equals(other.Rotation)
                && Scale.Equals(other.Scale);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation, Scale);
        }

        public static bool operator ==(LocalTransformSnapshot left, LocalTransformSnapshot right)
            => left.Equals(right);

        public static bool operator !=(LocalTransformSnapshot left, LocalTransformSnapshot right)
            => !left.Equals(right);
    }
}
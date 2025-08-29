using UnityEngine;

namespace Nexora
{
    public static class MathExtensions
    {
        public static double ClampAboveZero(this double value) => System.Math.Max(value, 0.0);
        public static float ClampAboveZero(this float value) => Mathf.Max(value, 0);
        public static int ClampAboveZero(this int value) => Mathf.Max(value, 0);
        public static decimal ClampAboveZero(this decimal value) => System.Math.Max(value, 0);

        public static float Normalize01(this float value, float min, float max)
            => Mathf.Clamp01((value - min) / (max - min));

        public static float Jitter(this float value, float jitter)
            => value + Random.Range(-jitter, jitter);

        public static float MagnitudeSum(this Vector3 vector)
            => Mathf.Abs(vector.x) + Mathf.Abs(vector.y) + Mathf.Abs(vector.z);

        public static Vector3 Jitter(this Vector3 vector, Vector3 jitter)
        {
            return new Vector3
            (
                vector.x.Jitter(jitter.x),
                vector.y.Jitter(jitter.y),
                vector.z.Jitter(jitter.z)
            );
        }

        /// <summary>
        /// Randomizes the signs of each component (x,y,z)
        /// </summary>
        /// <returns>Sign randomized resulting vector</returns>
        public static Vector3 RandomizeSigns(this Vector3 vector)
        {
            vector.x *= Mathf.Sign(Random.Range(-1000, 1000));
            vector.y *= Mathf.Sign(Random.Range(-1000, 1000));
            vector.z *= Mathf.Sign(Random.Range(-1000, 1000));

            return vector;
        }

        /// <summary>
        /// Strips Y component and returns vector on horizontal plane
        /// </summary>
        public static Vector3 Horizontal(this Vector3 vector)
            => new Vector3(vector.x, 0f, vector.z);

        public static Vector3 WithOverride(this Vector3 vector, float? x, float? y, float? z)
            => new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);

        public static Vector3 WithX(this Vector3 vector, float x)
            => new Vector3(x, vector.y, vector.z);

        public static Vector3 WithY(this Vector3 vector, float y)
            => new Vector3(vector.x, y, vector.z);

        public static Vector3 WithZ(this Vector3 vector, float z)
            => new Vector3(vector.x, vector.y, z);

        public static bool ApproximatelyEquals(this Vector3 lhs, Vector3 rhs, float epsilon = 0.0001f)
        {
            return Mathf.Abs(lhs.x - rhs.x) < epsilon
                && Mathf.Abs(lhs.y - rhs.y) < epsilon
                && Mathf.Abs(lhs.z - rhs.z) < epsilon;
        }

        /// <summary>
        /// Returns random value from the range provided(right inclusive)
        /// </summary>
        public static int GetRandomFromRange(this Vector2Int range)
            => Random.Range(range.x, range.y + 1);

        public static float GetRandomFromRange(this Vector2 range)
            => Random.Range(range.x, range.y);

        public static Vector3 GetRandomPoint(this Bounds bounds)
            => Nexora.MathUtils.GetRandomPoint(bounds.min, bounds.max);

        /// <summary>
        /// Checks if the given point is inside the bounds that has been rotated with given rotation amount and the pivot
        /// Reverse the rotation for the bounds and the point and then apply AABB check
        /// </summary>
        public static bool IsPointInsideRotatedBounds(
            this Bounds bounds, 
            Quaternion rotationAmount, 
            Vector3 rotationPivot, 
            Vector3 point)
        {
            // Move point and the bounds center so that rotation pivot can be treated like origin (0,0,0)
            // As rotation is much easier to do when the pivot is on the origin
            Vector3 movedPoint = point - rotationPivot;
            Vector3 movedBoundsCenter = bounds.center - rotationPivot;

            // Remove bound's rotation from the point's and boundCenter's perspective
            // So that it will be like rotation never happened and it's only reduced to simple AABB check
            Vector3 rotationEliminatedPoint = Quaternion.Inverse(rotationAmount) * movedPoint + rotationPivot;
            Vector3 rotationEliminatedBoundsCenter = Quaternion.Inverse(rotationAmount) * movedBoundsCenter + rotationPivot;

            Bounds rotationEliminatedBounds = new Bounds(rotationEliminatedBoundsCenter, bounds.size);

            return rotationEliminatedBounds.Contains(rotationEliminatedPoint);
        }

        /// <summary>
        /// Checks if the given Ray intersects with the bounds that is already rotated with the given amount around the pivot
        /// First eliminate rotation from bounds and ray and then make simple ray intersection check
        /// </summary>
        public static bool IntersectRotatedBounds(
            this Bounds bounds,
            Ray ray,
            Quaternion rotationAmount, 
            Vector3 rotationPivot, 
            out float hitDistance)
        {
            // Move elements so that the rotation pivot can be treated like origin (0,0,0)
            Vector3 movedRayOrigin = ray.origin - rotationPivot;
            Vector3 movedBoundsCenter = bounds.center - rotationPivot;

            // Eliminate rotation from Ray and the Bounds so that it will be like a plain Ray-Bounds Intersection
            Vector3 rotationEliminatedRayOrigin = Quaternion.Inverse(rotationAmount) * movedRayOrigin + rotationPivot;
            Vector3 rotationEliminatedRayDirection = Quaternion.Inverse(rotationAmount) * ray.direction;
            Ray rotationEliminatedRay = new Ray(rotationEliminatedRayOrigin, rotationEliminatedRayDirection);

            Vector3 rotationEliminatedBoundsCenter = Quaternion.Inverse(rotationAmount) * movedBoundsCenter + rotationPivot;
            Bounds rotationEliminatedBounds = new Bounds(rotationEliminatedBoundsCenter, bounds.size);

            if(rotationEliminatedBounds.IntersectRay(rotationEliminatedRay, out hitDistance))
            {
                return true;
            }

            hitDistance = 0;
            return false;
        }

        public static Color WithAlpha(this Color color, float alpha)
            => new Color(color.r, color.g, color.b, alpha);
    }
}

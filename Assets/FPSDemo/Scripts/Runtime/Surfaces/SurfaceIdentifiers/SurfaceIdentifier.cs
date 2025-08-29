using UnityEngine;

namespace Nexora.FPSDemo.SurfaceSystem
{
    /// <summary>
    /// Class that provides extracting <see cref="SurfaceDefinition"/> using <see cref="RaycastHit"/>
    /// and <see cref="Collision"/>. Used for surfaces when there are no <see cref="PhysicsMaterial"/>
    /// attached.
    /// </summary>
    public abstract class SurfaceIdentifier : MonoBehaviour
    {
        public abstract SurfaceDefinition GetSurfaceFromHit(in RaycastHit hit);
        public abstract SurfaceDefinition GetSurfaceFromCollision(Collision collision);
    }

    public abstract class SurfaceIdentifier<TCollider> : 
        SurfaceIdentifier
        where TCollider : Collider
    {
        public sealed override SurfaceDefinition GetSurfaceFromHit(in RaycastHit hit)
        {
            return hit.collider is TCollider collider
                ? GetSurfaceFromHit(collider, in hit)
                : null;
        }

        public sealed override SurfaceDefinition GetSurfaceFromCollision(Collision collision)
        {
            return collision.collider is TCollider collider
                ? GetSurfaceFromCollision(collider, collision)
                : null;
        }

        protected abstract SurfaceDefinition GetSurfaceFromHit(TCollider collider, in RaycastHit hit);
        protected abstract SurfaceDefinition GetSurfaceFromCollision(TCollider collider, Collision collision);
    }
}
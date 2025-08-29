using System;
using UnityEngine;

namespace Nexora.FPSDemo.SurfaceSystem
{
    /// <summary>
    /// Surface identifier defined for <see cref="MeshCollider"/>, has a single
    /// <see cref="SurfaceDefinition"/> for each of the <b>material indices</b> of the <see cref="Mesh"/>.
    /// </summary>
    public sealed class MeshSurfaceIdentifier : SurfaceIdentifier<MeshCollider>
    {
        [ReorderableList(ListStyle.Lined, elementLabel: "Material Surface")]
        [DefinitionReference(NullElement = "", HasAssetReference = true)]
        [Help("Each surface definition is linked with the material that is on the same index of the mesh renderer.")]
        [SerializeField]
        private DefinitionReference<SurfaceDefinition>[] _materialSurface = Array.Empty<DefinitionReference<SurfaceDefinition>>();

        /// <summary>
        /// Find the hit on the surface using the contact point and calls <see cref="GetSurfaceFromHit(MeshCollider, in RaycastHit)"/>.
        /// </summary>
        protected override SurfaceDefinition GetSurfaceFromCollision(MeshCollider collider, Collision collision)
        {
            ContactPoint contact = collision.contacts[0];
            var ray = new Ray(contact.point + contact.normal * 0.05f, -contact.normal);
            return contact.otherCollider.Raycast(ray, out RaycastHit hit, 0.1f)
                ? GetSurfaceFromHit(in hit)
                : null;
        }

        /// <summary>
        /// Finds the material index to return the matching <see cref="SurfaceDefinition"/>.
        /// It checks all <see cref="Mesh"/> triangle indices to find the one that matches
        /// the triangle associated with the <paramref name="hit"/>.
        /// </summary>
        /// <returns>Surface the <paramref name="hit"/> shoot upon.</returns>
        protected override SurfaceDefinition GetSurfaceFromHit(MeshCollider collider, in RaycastHit hit)
        {
            Mesh mesh = collider.sharedMesh;

            if(collider.convex || mesh.isReadable == false)
            {
                return _materialSurface.First().Definition;
            }

            int materialIndex = FindSubMeshIndex(mesh, hit.triangleIndex);
            return materialIndex != -1 
                ? _materialSurface[materialIndex].Definition
                : null;
        }

        private static int FindSubMeshIndex(Mesh mesh, int triangleIndex)
        {
            if(triangleIndex < 0 || triangleIndex * 3 >= mesh.triangles.Length)
            {
                return -1;
            }

            int[] triangleVertices = GetTriangleVertices(mesh, triangleIndex);

            for(int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                if(ContainsTriangle(mesh.GetTriangles(subMeshIndex), triangleVertices))
                {
                    return subMeshIndex;
                }
            }

            return -1;  
        }

        private static int[] GetTriangleVertices(Mesh mesh, int triangleIndex)
        {
            return new int[]
            {
                mesh.triangles[triangleIndex * 3],
                mesh.triangles[triangleIndex * 3 + 1],
                mesh.triangles[triangleIndex * 3 + 2]
            };
        }

        private static bool ContainsTriangle(int[] subMeshTriangles, int[] targetTriangles)
        {
            for(int i = 0; i < subMeshTriangles.Length; i += 3)
            {
                if (subMeshTriangles[i] == targetTriangles[0]
                 && subMeshTriangles[i + 1] == targetTriangles[1]
                 && subMeshTriangles[i + 2] == targetTriangles[2])
                {
                    return true;
                }
            }

            return false;
        }
        
    }
}
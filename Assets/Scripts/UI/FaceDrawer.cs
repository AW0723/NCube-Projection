using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FaceDrawer : MonoBehaviour
{
    private Mesh mesh;
    private readonly List<Vector3> meshVertices = new();
    private readonly List<int> meshTriangles = new();

    /// <summary>
    /// Rebuild all faces as a single mesh. The mesh and its GameObject are created once
    /// and reused, so redrawing creates no objects that need destroying.
    /// </summary>
    /// <param name="origin">Point the faces should face away from (used to orient normals)</param>
    /// <param name="faces">One unsorted vertex loop per face</param>
    public void DrawFaces(Vector3 origin, List<Vector3[]> faces)
    {
        if (mesh == null)
        {
            mesh = CreateMeshObject();
        }

        meshVertices.Clear();
        meshTriangles.Clear();
        foreach (Vector3[] face in faces)
        {
            AppendFace(origin, face);
        }

        mesh.Clear();
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(meshTriangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private Mesh CreateMeshObject()
    {
        Mesh newMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
        newMesh.MarkDynamic();

        GameObject faceObject = new GameObject("Faces");
        faceObject.transform.SetParent(transform, false);
        faceObject.AddComponent<MeshFilter>().mesh = newMesh;

        MeshRenderer meshRenderer = faceObject.AddComponent<MeshRenderer>();

        // Use the material from the FaceDrawer's renderer if it exists
        MeshRenderer parentRenderer = GetComponent<MeshRenderer>();
        if (parentRenderer != null && parentRenderer.material != null)
        {
            meshRenderer.material = parentRenderer.material;
        }
        else
        {
            // Fallback to a default material if the parent doesn't have one
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.white;
            meshRenderer.material = material;
        }

        return newMesh;
    }

    private void AppendFace(Vector3 origin, Vector3[] face)
    {
        // Need at least 3 vertices to form a face
        if (face.Length < 3)
        {
            Debug.LogWarning("DrawFaces requires at least 3 vertices to form a face");
            return;
        }

        // Sort vertices in counter-clockwise order around their centroid
        List<Vector3> vertexList = SortVerticesCounterClockwise(face);

        // Calculate the centroid of the face
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 vertex in vertexList)
        {
            centroid += vertex;
        }
        centroid /= vertexList.Count;

        // Calculate the normal from the first two edges
        Vector3 normal = Vector3.Cross(vertexList[1] - vertexList[0], vertexList[2] - vertexList[0]).normalized;

        // If the normal points toward the origin, reverse the vertex order
        if (Vector3.Dot(normal, (centroid - origin).normalized) < 0)
        {
            vertexList.Reverse();
        }

        // Fan triangulation: connect all vertices to the first vertex
        int baseIndex = meshVertices.Count;
        meshVertices.AddRange(vertexList);
        for (int i = 1; i < vertexList.Count - 1; i++)
        {
            meshTriangles.Add(baseIndex);
            meshTriangles.Add(baseIndex + i);
            meshTriangles.Add(baseIndex + i + 1);
        }
    }

    private List<Vector3> SortVerticesCounterClockwise(IReadOnlyList<Vector3> vertices)
    {
        // Calculate the centroid of all vertices
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            centroid += vertex;
        }
        centroid /= vertices.Count;

        // Calculate the normal to the plane formed by the vertices
        // Use the first two edges to get a more stable normal
        Vector3 edge1 = vertices[1] - vertices[0];
        Vector3 edge2 = vertices[2] - vertices[0];
        Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

        // If normal is zero (collinear points), try different edges
        if (normal.magnitude < 0.001f)
        {
            for (int i = 0; i < vertices.Count - 2; i++)
            {
                edge1 = vertices[i + 1] - vertices[i];
                edge2 = vertices[i + 2] - vertices[i];
                normal = Vector3.Cross(edge1, edge2).normalized;
                if (normal.magnitude > 0.001f)
                    break;
            }
        }

        // Create a local coordinate system on the plane
        Vector3 tangent = (vertices[0] - centroid);
        if (tangent.magnitude < 0.001f)
        {
            tangent = (vertices[1] - centroid);
        }
        tangent.Normalize();

        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        // Sort vertices by angle around the centroid
        List<(Vector3 vertex, float angle)> verticesWithAngles = new List<(Vector3, float)>();
        foreach (Vector3 vertex in vertices)
        {
            Vector3 direction = vertex - centroid;
            float x = Vector3.Dot(direction, tangent);
            float y = Vector3.Dot(direction, bitangent);
            float angle = Mathf.Atan2(y, x);
            verticesWithAngles.Add((vertex, angle));
        }

        // Sort by angle
        verticesWithAngles.Sort((a, b) => a.angle.CompareTo(b.angle));

        // Extract sorted vertices
        List<Vector3> sortedVertices = new List<Vector3>();
        foreach (var item in verticesWithAngles)
        {
            sortedVertices.Add(item.vertex);
        }

        return sortedVertices;
    }
}

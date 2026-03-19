using System.Collections.Generic;
using UnityEngine;

public class FaceDrawer : MonoBehaviour
{
    private List<GameObject> faceGameObjects = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawOneFace(Vector3 origin, IEnumerable<Vector3> vertices)
    {
        // Convert vertices to a list for easier manipulation
        List<Vector3> vertexList = new List<Vector3>(vertices);

        // Need at least 3 vertices to form a face
        if (vertexList.Count < 3)
        {
            Debug.LogWarning("DrawOneFace requires at least 3 vertices to form a face");
            return;
        }

        // Sort vertices in counter-clockwise order around their centroid
        vertexList = SortVerticesCounterClockwise(vertexList);

        // Calculate the centroid of the face
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 vertex in vertexList)
        {
            centroid += vertex;
        }
        centroid /= vertexList.Count;

        // Calculate the normal from the first two edges
        Vector3 edge1 = vertexList[1] - vertexList[0];
        Vector3 edge2 = vertexList[2] - vertexList[0];
        Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

        // Direction from origin to face centroid - the normal should point this way
        Vector3 outwardDirection = (centroid - origin).normalized;

        // If the normal points toward the origin, reverse the vertex order
        if (Vector3.Dot(normal, outwardDirection) < 0)
        {
            vertexList.Reverse();
        }

        // Create a new mesh
        Mesh mesh = new Mesh();

        // Set vertices
        mesh.vertices = vertexList.ToArray();

        // Create triangles using fan triangulation
        // This works by connecting all vertices to the first vertex
        int[] triangles = new int[(vertexList.Count - 2) * 3];
        for (int i = 0; i < vertexList.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.triangles = triangles;

        // Recalculate mesh properties for proper rendering
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create a new child GameObject for this face
        GameObject faceObject = new GameObject("Face");
        faceObject.transform.SetParent(transform);
        faceGameObjects.Add(faceObject);

        // Get or create a mesh filter
        MeshFilter meshFilter = faceObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Add a mesh renderer
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
    }

    public void ClearFaces()
    {
        foreach (GameObject faceObject in faceGameObjects)
        {
            Destroy(faceObject);
        }
        faceGameObjects.Clear();
    }

    private List<Vector3> SortVerticesCounterClockwise(List<Vector3> vertices)
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NCubeController : MonoBehaviour
{
    public int dimension;

    public bool debugLines;

    private VectorN Origin;
    private List<VectorN> Points = new List<VectorN>();
    private List<(int, int)> Lines = new List<(int, int)>();
    private List<List<int>> Faces = new List<List<int>>();

    private List<Vector3> IntersectionPoints = new List<Vector3>();
    private List<(Vector3, Vector3)> IntersectionLines = new List<(Vector3, Vector3)>();

    // Start is called before the first frame update
    void Start()
    {
        Origin = VectorN.zero(dimension);
        BuildNCube(dimension);
        FindIntersection();

        foreach (var point in Points)
        {
            Debug.Log(point);
        }
        foreach (var line in Lines)
        {
            Debug.Log(string.Join(" ", line.Item1, line.Item2, Points[line.Item1], Points[line.Item2]));
        }
        foreach (var face in Faces)
        {
            Debug.Log(string.Join(" ", face));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (debugLines)
        {
            foreach (var line in Lines)
            {
                VectorN point1 = Points[line.Item1];
                VectorN point2 = Points[line.Item2];

                // TODO: allow some kind of casting from VectorN to Vector3
                Vector3 pos1 = new Vector3(point1[0], point1[1], point1.dimension > 2 ? point1[2] : 0);
                Vector3 pos2 = new Vector3(point2[0], point2[1], point2.dimension > 2 ? point2[2] : 0);

                Debug.DrawLine(pos1, pos2);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 1);
        foreach (var pos in IntersectionPoints)
        {
            Gizmos.DrawSphere(pos, 0.05f);
        }

        foreach (var line in IntersectionLines)
        {
            Vector3[] points = IntersectionLines.SelectMany(line => new[] { line.Item1, line.Item2 }).ToArray();
            Gizmos.DrawLineList(points);
        }
    }

    public void Translate(int axis, float amount)
    {
        VectorN direction = VectorN.unit(dimension, axis - 1);
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] += direction * amount;
        }
        Origin += direction * amount;

        FindIntersection();
    }

    public void Rotate(int axisA, int axisB, float amount)
    {
        if (axisA == axisB)
        {
            throw new System.Exception("Axes cannot be the same");
        }
        axisA = axisA - 1;
        axisB = axisB - 1;

        MatrixNxN rotationMatrix = MatrixNxN.identity(dimension);
        rotationMatrix[axisA, axisA] = Mathf.Cos(amount);
        rotationMatrix[axisA, axisB] = -Mathf.Sin(amount);
        rotationMatrix[axisB, axisA] = Mathf.Sin(amount);
        rotationMatrix[axisB, axisB] = Mathf.Cos(amount);

        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = rotationMatrix * (Points[i] - Origin) + Origin;
        }

        FindIntersection();
    }

    private void BuildNCube(int dimension)
    {
        if (dimension < 1) { throw new System.Exception(); }

        Points.Add(VectorN.zero(dimension));
        for (int currDimension = 1; currDimension <= dimension; currDimension++)
        {
            int pointsCount = Points.Count;
            int linesCount = Lines.Count;
            int facesCount = Faces.Count;

            for (int currFace = 0; currFace < facesCount; currFace++)
            {
                List<int> face = new List<int>(Faces[currFace]);
                for (int i = 0; i < face.Count; i++)
                {
                    face[i] += linesCount;
                }
                Faces.Add(face);
            }

            for (int currLine = 0; currLine < linesCount; currLine++)
            {
                (int, int) line = Lines[currLine];
                Lines.Add((line.Item1 + pointsCount, line.Item2 + pointsCount));

                Faces.Add(new List<int>() {
                    currLine,
                    Lines.Count - 1,
                    line.Item1 + 2 * linesCount,
                    line.Item2 + 2 * linesCount });
            }

            for (int currPoint = 0; currPoint < pointsCount; currPoint++)
            {
                VectorN offset = VectorN.unit(dimension, currDimension - 1);
                Points.Add(Points[currPoint] + offset);
                Points[currPoint] -= offset;

                Lines.Add((currPoint, Points.Count - 1));
            }
        }
    }

    private void FindIntersection()
    {
        IntersectionPoints.Clear();
        foreach (var line in Lines)
        {
            Vector3? position = FindLineIntersection(Points[line.Item1], Points[line.Item2]);
            if (position != null)
            {
                IntersectionPoints.Add(position.Value);
            }
        }

        IntersectionLines.Clear();
        foreach (var face in Faces)
        {
            List<Vector3> faceIntersectionPoints = new List<Vector3>();
            foreach (var currLine in face)
            {
                (int, int) line = Lines[currLine];
                Vector3? point = FindLineIntersection(Points[line.Item1], Points[line.Item2]);
                if (point != null)
                {
                    faceIntersectionPoints.Add(point.Value);
                }
            }
            if (faceIntersectionPoints.Count >= 2)
            {
                IntersectionLines.Add((faceIntersectionPoints[0], faceIntersectionPoints[1]));
            }
        }
    }

    private Vector3? FindLineIntersection(VectorN pointA, VectorN pointB)
    {
        // TODO: generalize to higher dimensions
        if (pointA[3] == pointB[3] || pointA[3] * pointB[3] >= 0)
        {
            return null;
        }
        float k = pointA[3] / (pointA[3] - pointB[3]);
        VectorN vectorA = pointA * (1 - k);
        VectorN vectorB = pointB * k;
        VectorN result = vectorA + vectorB;
        return new Vector3(result[0], result[1], result[2]);
    }
}
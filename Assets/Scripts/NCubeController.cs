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
        Origin = VectorN.Zero(dimension);
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

    }

    private void OnDrawGizmos()
    {
        DebugDraw();

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

    private void DebugDraw()
    {
        if (debugLines)
        {
            Gizmos.color = Color.yellow;
            List<Vector3> points = new List<Vector3>();
            foreach (var line in Lines)
            {
                VectorN point1 = Points[line.Item1];
                VectorN point2 = Points[line.Item2];

                points.Add(point1.toVector3());
                points.Add(point2.toVector3());
            }
            Gizmos.DrawLineList(points.ToArray());
        }
    }

    public void Translate(int axis, float amount)
    {
        VectorN direction = VectorN.Unit(dimension, axis - 1);
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

        Points.Add(VectorN.Zero(dimension));
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
                VectorN offset = VectorN.Unit(dimension, currDimension - 1);
                Points.Add(Points[currPoint] + offset);
                Points[currPoint] -= offset;

                Lines.Add((currPoint, Points.Count - 1));
            }
        }
    }

    // TODO: generalize to higher dimension
    private void FindIntersection()
    {
        IntersectionPoints.Clear();
        foreach (var line in Lines)
        {
            Vector3? position = FindLineIntersection(Points[line.Item1], Points[line.Item2], 4);
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
                Vector3? point = FindLineIntersection(Points[line.Item1], Points[line.Item2], 4);
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

    /// <summary>
    /// Find the intersection point of a line with the plane where the nth dimension is 0
    /// </summary>
    /// <param name="pointA">Point A of the line</param>
    /// <param name="pointB">Point B of the line</param>
    /// <param name="dimension">The dimensional component that should be 0</param>
    /// <returns></returns>
    private Vector3? FindLineIntersection(VectorN pointA, VectorN pointB, int dimension)
    {
        int index = dimension - 1;

        if (pointA[index] == pointB[index] || pointA[index] * pointB[index] >= 0)
        {
            return null;
        }
        float k = pointA[index] / (pointA[index] - pointB[index]);
        VectorN vectorA = pointA * (1 - k);
        VectorN vectorB = pointB * k;
        VectorN result = vectorA + vectorB;
        return result.toVector3();
    }
}
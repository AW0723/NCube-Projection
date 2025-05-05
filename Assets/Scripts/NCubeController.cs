using System;
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
    private List<List<int[]>> AllSimplices = new List<List<int[]>>();

    private List<Vector3> IntersectionPoints = new List<Vector3>();
    private List<(Vector3, Vector3)> IntersectionLines = new List<(Vector3, Vector3)>();

    // Start is called before the first frame update
    void Start()
    {
        Origin = VectorN.Zero(dimension);

        //BuildNCube(dimension);
        //foreach (var point in Points)
        //{
        //    Debug.Log(point);
        //}
        //foreach (var line in Lines)
        //{
        //    Debug.Log(string.Join(" ", line.Item1, line.Item2, Points[line.Item1], Points[line.Item2]));
        //}
        //foreach (var face in Faces)
        //{
        //    Debug.Log(string.Join(" ", face));
        //}

        //FindIntersection();

        BuildNCube2(dimension);
        foreach (var point in Points)
        {
            Debug.Log(point);
        }
        foreach (var simplices in AllSimplices)
        {
            foreach (var simplex in simplices)
            {
                Debug.Log(string.Join(" ", simplex));
            }
        }

        FindIntersection2(dimension);
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
        FindIntersection2(dimension);
    }

    public void Rotate(int axisA, int axisB, float amount)
    {
        if (axisA == axisB)
        {
            throw new Exception("Axes cannot be the same");
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
        FindIntersection2(dimension);
    }

    private void BuildNCube(int dimension)
    {
        if (dimension < 1) { throw new Exception(); }

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

    private void BuildNCube2(int dimension)
    {
        if (dimension < 1) { throw new Exception(); }

        for (int currDimension = 0; currDimension <= dimension; currDimension++)
        {
            AllSimplices.Add(new List<int[]>());
        }

        Points.Add(VectorN.Zero(dimension));
        for (int currDimension1 = 1; currDimension1 <= dimension; currDimension1++)
        {
            int pointsCount = Points.Count;

            int[] initSimplexCount = AllSimplices.Select(simplices => simplices.Count).ToArray();
            for (int currDimension2 = currDimension1; currDimension2 > 0; currDimension2--)
            {
                int simplexCount = initSimplexCount[currDimension2];
                int lowerSimplexCount = currDimension2 == 1 ? Points.Count : initSimplexCount[currDimension2 - 1];
                for (int currSimplex = 0; currSimplex < simplexCount; currSimplex++)
                {
                    int[] simplex = AllSimplices[currDimension2][currSimplex];

                    int[] copiedSimplex = new int[simplex.Length];
                    Array.Copy(simplex, copiedSimplex, simplex.Length);
                    for (int i = 0; i < copiedSimplex.Length; i++)
                    {
                        copiedSimplex[i] += lowerSimplexCount;
                    }
                    AllSimplices[currDimension2].Add(copiedSimplex);

                    if (currDimension2 < dimension)
                    {
                        int[] higherSimplex = new int[2 * (currDimension2 + 1)];
                        higherSimplex[0] = currSimplex;
                        higherSimplex[1] = AllSimplices[currDimension2].Count - 1;

                        for (int i = 0; i < simplex.Length; i++)
                        {
                            higherSimplex[i + 2] = simplex[i] + 2 * simplexCount;
                        }
                        AllSimplices[currDimension2 + 1].Add(higherSimplex);
                    }
                }
            }

            for (int currPoint = 0; currPoint < pointsCount; currPoint++)
            {
                VectorN offset = VectorN.Unit(dimension, currDimension1 - 1);
                Points.Add(Points[currPoint] + offset);
                Points[currPoint] -= offset;

                AllSimplices[1].Add(new int[] { currPoint, Points.Count - 1 });
            }
        }
    }

    // TODO: generalize to higher dimension
    private void FindIntersection()
    {
        //IntersectionPoints.Clear();
        //foreach (var line in Lines)
        //{
        //    Vector3? position = FindLineIntersection(Points[line.Item1], Points[line.Item2], dimension)?.toVector3();
        //    if (position != null)
        //    {
        //        IntersectionPoints.Add(position.Value);
        //    }
        //}

        //IntersectionLines.Clear();
        //foreach (var face in Faces)
        //{
        //    List<Vector3> faceIntersectionPoints = new List<Vector3>();
        //    foreach (var currLine in face)
        //    {
        //        (int, int) line = Lines[currLine];
        //        Vector3? point = FindLineIntersection(Points[line.Item1], Points[line.Item2], dimension)?.toVector3();
        //        if (point != null)
        //        {
        //            faceIntersectionPoints.Add(point.Value);
        //        }
        //    }
        //    if (faceIntersectionPoints.Count >= 2)
        //    {
        //        IntersectionLines.Add((faceIntersectionPoints[0], faceIntersectionPoints[1]));
        //    }
        //}

        //Debug.Log(IntersectionLines.Count);
        //foreach (var line in IntersectionLines)
        //{
        //    Debug.Log(line);
        //}
    }

    private void FindIntersection2(int currDimension)
    {
        if (currDimension <= 3) { return; }

        int targetDimension = currDimension - 2;

        List<VectorN> intersectionPoints = new List<VectorN>();
        List<List<int[]>> allIntersectionSimplices = new List<List<int[]>>();
        Dictionary<int, int> lineToPoint = new Dictionary<int, int>();

        List<int[]> simplices = AllSimplices[targetDimension];

        for (int i = 0; i < currDimension; i++)
        {
            allIntersectionSimplices.Add(new List<int[]>());
        }

        for (int currSimplex = 0; currSimplex < simplices.Count; currSimplex++)
        {
            FindSimplexIntersection(targetDimension, currSimplex, lineToPoint, intersectionPoints, allIntersectionSimplices, Points, AllSimplices, dimension);
        }

        IntersectionLines.Clear();
        foreach (var intersectionLine in allIntersectionSimplices[1])
        {
            IntersectionLines.Add((intersectionPoints[intersectionLine[0]].toVector3(), intersectionPoints[intersectionLine[1]].toVector3()));
        }
    }

    private int? FindSimplexIntersection(int currDimension, int simplexIndex, Dictionary<int, int> lineToPoint, List<VectorN> intersectionPoints, List<List<int[]>> allIntersectionSimplices,
                                        List<VectorN> points, List<List<int[]>> allSimplices, int dimension)
    {
        int[] simplex = allSimplices[currDimension][simplexIndex];
        if (currDimension == 1)
        {
            if (lineToPoint.ContainsKey(simplexIndex))
            {
                return lineToPoint[simplexIndex];
            }
            else
            {
                VectorN intersectionPoint = FindLineIntersection(points[simplex[0]], points[simplex[1]], dimension);
                if (intersectionPoint != null)
                {
                    intersectionPoints.Add(intersectionPoint);
                    lineToPoint.Add(simplexIndex, intersectionPoints.Count - 1);
                    return intersectionPoints.Count - 1;
                }
            }
            return null;
        }

        int lowerDimension = currDimension - 1;

        List<int> intersectionSimplices = new List<int>();
        foreach (int lowerSimplex in simplex)
        {
            int? intersectionSimplex = FindSimplexIntersection(lowerDimension, lowerSimplex, lineToPoint, intersectionPoints, allIntersectionSimplices, points, allSimplices, dimension);
            if (intersectionSimplex != null)
            {
                intersectionSimplices.Add(intersectionSimplex.Value);
            }
        }
        if (intersectionSimplices.Count >= (currDimension - 1) * 2)
        {
            if (allIntersectionSimplices.Count < lowerDimension)
            {
                allIntersectionSimplices.Add(new List<int[]>());
            }
            allIntersectionSimplices[lowerDimension].Add(intersectionSimplices.ToArray());
            return allIntersectionSimplices[lowerDimension].Count - 1;
        }
        return null;
    }

    /// <summary>
    /// Find the intersection point of a line with the plane where the nth dimension is 0
    /// </summary>
    /// <param name="pointA">Point A of the line</param>
    /// <param name="pointB">Point B of the line</param>
    /// <param name="dimension">The dimensional component that should be 0</param>
    /// <returns></returns>
    private VectorN FindLineIntersection(VectorN pointA, VectorN pointB, int dimension)
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
        return result;
    }
}
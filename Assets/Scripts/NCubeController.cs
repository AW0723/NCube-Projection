using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NCubeController : MonoBehaviour
{
    public int dimension;

    public bool debugLines;
    public int debugIntersectionDimension;

    public bool draw3D;

    private VectorN Origin;
    private List<VectorN> Points = new List<VectorN>();
    private List<List<int[]>> AllSimplices = new List<List<int[]>>();

    private List<(Vector3, Vector3)> IntersectionLines = new List<(Vector3, Vector3)>();

    private Dictionary<int, List<(Vector3, Vector3)>> DebugIntersectionLines = new Dictionary<int, List<(Vector3, Vector3)>>();

    // Start is called before the first frame update
    void Start()
    {
        Origin = VectorN.Zero(dimension);

        for (int i = dimension; i > 3; i--)
        {
            DebugIntersectionLines.Add(i, new List<(Vector3, Vector3)>());
        }

        BuildNCube(dimension);
        //foreach (var point in Points)
        //{
        //    Debug.Log(point);
        //}
        //foreach (var simplices in AllSimplices)
        //{
        //    foreach (var simplex in simplices)
        //    {
        //        Debug.Log(string.Join(" ", simplex));
        //    }
        //}

        FindIntersection();
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
        Draw3D();
    }

    private void Draw3D()
    {
        if (draw3D)
        {
            Gizmos.color = new Color(1, 1, 1, 1);
            List<Vector3> points = new List<Vector3>();

            foreach (var line in IntersectionLines)
            {
                points.Add(line.Item1);
                points.Add(line.Item2);

                Gizmos.DrawSphere(line.Item1, 0.05f);
                Gizmos.DrawSphere(line.Item2, 0.05f);
            }
            Gizmos.DrawLineList(points.ToArray());
        }
    }

    private void DebugDraw()
    {
        if (debugLines)
        {
            Gizmos.color = Color.yellow;
            List<Vector3> points = new List<Vector3>();

            if (DebugIntersectionLines.TryGetValue(debugIntersectionDimension, out List<(Vector3, Vector3)> lines))
            {
                foreach (var line in lines)
                {
                    points.Add(line.Item1);
                    points.Add(line.Item2);

                    Gizmos.DrawSphere(line.Item1, 0.05f);
                    Gizmos.DrawSphere(line.Item2, 0.05f);
                }
                Gizmos.DrawLineList(points.ToArray());
            }
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
    }

    private void BuildNCube(int dimension)
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

    private void FindIntersection()
    {
        if (dimension <= 3) { return; }

        Dictionary<int, int> lineToPoint = new Dictionary<int, int>();
        List<VectorN> intersectionPoints = new List<VectorN>();
        List<List<int[]>> allIntersectionSimplices = new List<List<int[]>>();

        List<VectorN> points = Points;
        List<List<int[]>> allSimplices = AllSimplices;

        for (int currDimension = dimension; currDimension > 3; currDimension--)
        {
            DebugIntersectionLines[currDimension].Clear();
            foreach (var line in allSimplices[1])
            {
                DebugIntersectionLines[currDimension].Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
            }

            lineToPoint.Clear();
            intersectionPoints = new List<VectorN>();
            allIntersectionSimplices = new List<List<int[]>>();

            for (int i = 0; i < currDimension; i++)
            {
                allIntersectionSimplices.Add(new List<int[]>());
            }

            for (int currSimplex = 0; currSimplex < allSimplices[currDimension].Count; currSimplex++)
            {
                FindSimplexIntersection(currDimension, currSimplex, lineToPoint, intersectionPoints, allIntersectionSimplices, points, allSimplices, currDimension);
            }

            points = intersectionPoints;
            allSimplices = allIntersectionSimplices;
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
        if (intersectionSimplices.Count > 0)
        {
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
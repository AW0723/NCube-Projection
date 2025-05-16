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

    public ShaderInterface shaderInterface;

    //public List<VectorN> pointsA;
    //public List<VectorN> pointsB;

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

        //pointsA = new List<VectorN>()
        //{
        //    new VectorN(1, 1, 1),
        //    new VectorN(1, 1, 1),
        //    new VectorN(1, 1, 1),
        //};

        //pointsB = new List<VectorN>()
        //{
        //    new VectorN(1, 1, -1),
        //    new VectorN(-1, -1, -1),
        //    new VectorN(-1, 1, 1),
        //};

        //for (int i = 0; i < pointsA.Count; i++)
        //{
        //    Debug.Log(FindLineIntersection(pointsA[i], pointsB[i], 3));
        //}

        FindIntersection();
        //FindIntersection2();
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

    private void FindIntersection2()
    {
        if (dimension <= 3) { return; }

        // for performance optimization
        List<List<int[]>> simplices1 = new List<List<int[]>>();
        List<List<int[]>> simplices2 = new List<List<int[]>>();

        List<VectorN> intersectionPoints = new List<VectorN>();
        List<List<int[]>> allIntersectionSimplices = simplices1;

        List<Dictionary<int, int>> savedIntersections = new List<Dictionary<int, int>>();

        List<VectorN> points = Points;
        List<List<int[]>> allSimplices = AllSimplices;

        for (int i = 0; i < dimension; i++)
        {
            simplices1.Add(new List<int[]>());
            simplices2.Add(new List<int[]>());
            savedIntersections.Add(new Dictionary<int, int>());
        }
        savedIntersections.Add(new Dictionary<int, int>());

        for (int currDimension = dimension; currDimension > 3; currDimension--)
        {
            DebugIntersectionLines[currDimension].Clear();
            //foreach (var line in allSimplices[1])
            //{
            //    DebugIntersectionLines[currDimension].Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
            //}

            List<int[]> lines = allSimplices[1];

            (float[] componentsA, float[] componentsB) = FlattenLines(points, lines, currDimension);

            if (allIntersectionSimplices == simplices1)
            {
                allIntersectionSimplices = simplices2;
            }
            else
            {
                allIntersectionSimplices = simplices1;
            }

            foreach (var simplices in allIntersectionSimplices)
            {
                simplices.Clear();
            }

            foreach (var savedIntersection in savedIntersections)
            {
                savedIntersection.Clear();
            }

            intersectionPoints = shaderInterface.FindIntersections(componentsA, componentsB, currDimension);

            FindSimplexIntersection(currDimension, 0, intersectionPoints, allIntersectionSimplices, savedIntersections, points, allSimplices);

            // prepare for next loop
            points = intersectionPoints;
            allSimplices = allIntersectionSimplices;
            savedIntersections.RemoveAt(savedIntersections.Count - 1);
        }

        IntersectionLines.Clear();
        foreach (var intersectionLine in allIntersectionSimplices[1])
        {
            IntersectionLines.Add((intersectionPoints[intersectionLine[0]].toVector3(), intersectionPoints[intersectionLine[1]].toVector3()));
        }
    }

    private void FindIntersection()
    {
        if (dimension <= 3) { return; }

        List<VectorN> intersectionPoints = new List<VectorN>();
        List<Dictionary<int, int[]>> allIntersectionSimplices = new List<Dictionary<int, int[]>>();

        List<VectorN> points = Points;
        List<Dictionary<int, int[]>> allSimplices = new List<Dictionary<int, int[]>>();

        for (int currDimension = 0; currDimension < AllSimplices.Count; currDimension++)
        {
            allSimplices.Add(new Dictionary<int, int[]>());
            List<int[]> simplices = AllSimplices[currDimension];
            for (int currSimplices = 0; currSimplices < simplices.Count; currSimplices++)
            {
                allSimplices[currDimension][currSimplices] = AllSimplices[currDimension][currSimplices];
            }
        }

        for (int currDimension = dimension; currDimension > 3; currDimension--)
        {
            DebugIntersectionLines[currDimension].Clear();
            foreach (var pair in allSimplices[1])
            {
                int[] line = pair.Value;
                DebugIntersectionLines[currDimension].Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
            }

            allIntersectionSimplices = new List<Dictionary<int, int[]>>();
            for (int i = 0; i < currDimension; i++)
            {
                allIntersectionSimplices.Add(new Dictionary<int, int[]>());
            }

            List<int[]> lines = new List<int[]>();
            Dictionary<int, int> lineToPoint = new Dictionary<int, int>();

            foreach (var line in allSimplices[1])
            {
                lines.Add(line.Value);
                lineToPoint.Add(line.Key, lines.Count - 1);
            }

            // calculate line intersections
            (float[] componentsA, float[] componentsB) = FlattenLines(points, lines, currDimension);
            intersectionPoints = shaderInterface.FindIntersections(componentsA, componentsB, currDimension);

            // find face intersections
            foreach (var simplices in allSimplices[2])
            {
                List<int> currIntersections = new List<int>();
                foreach (var line in simplices.Value)
                {
                    if (IsValid(intersectionPoints[lineToPoint[line]]))
                    {
                        currIntersections.Add(lineToPoint[line]);
                    }
                }
                if (currIntersections.Count > 0)
                {
                    allIntersectionSimplices[1].Add(simplices.Key, currIntersections.ToArray());
                }
            }

            // find intersection of the rest
            for (int currDimension2 = 3; currDimension2 <= currDimension; currDimension2++)
            {
                foreach (var simplices in allSimplices[currDimension2])
                {
                    List<int> currIntersections = new List<int>();
                    foreach (var lowerSimplex in simplices.Value)
                    {
                        if (allIntersectionSimplices[currDimension2 - 2].ContainsKey(lowerSimplex))
                        {
                            currIntersections.Add(lowerSimplex);
                        }
                    }
                    if (currIntersections.Count > 0)
                    {
                        allIntersectionSimplices[currDimension2 - 1].Add(simplices.Key, currIntersections.ToArray());
                    }
                }
            }

            points = intersectionPoints;
            allSimplices = allIntersectionSimplices;
        }

        IntersectionLines.Clear();
        foreach (var intersectionLine in allIntersectionSimplices[1])
        {
            IntersectionLines.Add((intersectionPoints[intersectionLine.Value[0]].toVector3(), intersectionPoints[intersectionLine.Value[1]].toVector3()));
        }
    }

    private int? FindSimplexIntersection(int currDimension, int simplexIndex,
                    List<VectorN> intersectionPoints, List<List<int[]>> allIntersectionSimplices, List<Dictionary<int, int>> savedIntersections,
                    List<VectorN> points, List<List<int[]>> allSimplices)
    {
        if (currDimension == 1)
        {
            if (IsValid(intersectionPoints[simplexIndex]))
            {
                return simplexIndex;
            }
            else
            {
                return null;
            }
        }

        if (savedIntersections[currDimension].TryGetValue(simplexIndex, out int savedIntersection))
        {
            return savedIntersection;
        }

        int lowerDimension = currDimension - 1;
        int[] simplex = allSimplices[currDimension][simplexIndex];

        List<int> intersectionSimplices = new List<int>();
        foreach (int lowerSimplex in simplex)
        {
            int? intersectionSimplex = FindSimplexIntersection(lowerDimension, lowerSimplex, intersectionPoints, allIntersectionSimplices, savedIntersections, points, allSimplices);
            if (intersectionSimplex != null)
            {
                intersectionSimplices.Add(intersectionSimplex.Value);
            }
        }
        if (intersectionSimplices.Count > 0)
        {
            allIntersectionSimplices[lowerDimension].Add(intersectionSimplices.ToArray());
            int intersectionIndex = allIntersectionSimplices[lowerDimension].Count - 1;
            savedIntersections[currDimension].Add(simplexIndex, intersectionIndex);
            return intersectionIndex;
        }
        return null;
    }

    private (float[], float[]) FlattenLines(List<VectorN> points, List<int[]> lines, Dictionary<int, int> lineToPoint, int dimension)
    {
        float[] componentsA = new float[lines.Count * dimension];
        float[] componentsB = new float[lines.Count * dimension];

        for (int i = 0; i < lines.Count; i++)
        {
            int[] currLine = lines[i];
            VectorN pointA = points[lineToPoint[currLine[0]]];
            VectorN pointB = points[lineToPoint[currLine[1]]];
            Array.Copy(pointA.components, 0, componentsA, i * dimension, dimension);
            Array.Copy(pointB.components, 0, componentsB, i * dimension, dimension);
        }
        return (componentsA, componentsB);
    }

    private (float[], float[]) FlattenLines(List<VectorN> points, List<int[]> lines, int dimension)
    {
        float[] componentsA = new float[lines.Count * dimension];
        float[] componentsB = new float[lines.Count * dimension];

        for (int i = 0; i < lines.Count; i++)
        {
            int[] currLine = lines[i];
            VectorN pointA = points[currLine[0]];
            VectorN pointB = points[currLine[1]];
            Array.Copy(pointA.components, 0, componentsA, i * dimension, dimension);
            Array.Copy(pointB.components, 0, componentsB, i * dimension, dimension);
        }
        return (componentsA, componentsB);
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
        return result.Reduce(dimension - 1);
    }

    private bool IsValid(VectorN point)
    {
        float component = point.components[0];
        if (float.IsNaN(component) || float.IsInfinity(component))
        {
            return false;
        }
        return true;
    }
}
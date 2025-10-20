using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NCubeController : MonoBehaviour
{
    private int dimension;

    public bool debugLines;
    public int debugIntersectionDimension;

    public bool draw3D;

    public ShaderInterface shaderInterface;
    public LineDrawer lineDrawer;

    public VectorN Origin { private set; get; }
    private List<VectorN> Points = new List<VectorN>();
    private List<List<int[]>> AllSimplices = new List<List<int[]>>();

    private List<(Vector3, Vector3)> IntersectionLines = new List<(Vector3, Vector3)>();

    private Dictionary<int, List<(Vector3, Vector3)>> DebugIntersectionLines = new Dictionary<int, List<(Vector3, Vector3)>>();

    private bool useShader = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        Draw3D();
    }

    public void SetupWithDimension(int dimension)
    {
        this.dimension = dimension;

        Points.Clear();
        AllSimplices.Clear();
        IntersectionLines.Clear();
        DebugIntersectionLines.Clear();

        Origin = VectorN.Zero(dimension);

        for (int i = dimension; i > 3; i--)
        {
            DebugIntersectionLines.Add(i, new List<(Vector3, Vector3)>());
        }

        BuildNCube(dimension);
        FindIntersection();
    }

    private void OnDrawGizmos()
    {
        DebugDrawAllProjection();
        DebugDraw3D();
    }

    private void Draw3D()
    {
        List<Vector3> points = new();

        foreach (var line in IntersectionLines)
        {
            points.Add(line.Item1);
            points.Add(line.Item2);
        }
        lineDrawer.DrawLineList(points.ToArray());
    }

    private void DebugDraw3D()
    {
        if (draw3D)
        {
            Gizmos.color = new Color(1, 1, 1, 1);
            List<Vector3> points = new();

            foreach (var line in IntersectionLines)
            {
                points.Add(line.Item1);
                points.Add(line.Item2);

                Gizmos.DrawSphere(line.Item1, 0.05f);
                Gizmos.DrawSphere(line.Item2, 0.05f);
            }
        }
    }

    private void DebugDrawAllProjection()
    {
        if (debugLines)
        {
            Gizmos.color = Color.yellow;
            List<Vector3> points = new();

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

    public void SetTranslation(int axis, float amount)
    {
        VectorN originalPos = new(Origin);
        Origin[axis - 1] = amount;
        VectorN offset = Origin - originalPos;
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] += offset;
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
    }

    public void ResetTranslation()
    {
        for (int i = 1; i <= dimension; i++)
        {
            SetTranslation(i, 0);
        }
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
    }

    /// <summary>
    /// Randomize rotation in every axes
    /// </summary>
    /// <param name="strength">Strength of the randomization, 1 for complete randomness</param>
    public void RandomizeRotation(float strength)
    {
        float maxValue = strength * 2 * Mathf.PI;
        for (int i = 1; i < dimension; i++)
        {
            for (int j = i + 1; j <= dimension; j++)
            {
                if (i == j) { continue; }
                Rotate(i, j, Random.Range(0, maxValue));
            }
        }
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

    public void FindIntersection()
    {
        IntersectionLines.Clear();

        if (dimension <= 3)
        {
            foreach (var line in AllSimplices[1])
            {
                IntersectionLines.Add((Points[line[0]].toVector3(), Points[line[1]].toVector3()));
            }
            return;
        }

        List<VectorN> intersectionPoints = new List<VectorN>();
        List<Dictionary<int, int[]>> allIntersectionSimplices = new List<Dictionary<int, int[]>>();
        Dictionary<int, int> lineToPoint = new Dictionary<int, int>();

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
            // for debugging
            DebugIntersectionLines[currDimension].Clear();
            foreach (var pair in allSimplices[1])
            {
                int[] line = pair.Value;
                DebugIntersectionLines[currDimension].Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
            }

            // reset allIntersectionSimplices
            allIntersectionSimplices = new List<Dictionary<int, int[]>>();
            for (int i = 0; i < currDimension; i++)
            {
                allIntersectionSimplices.Add(new Dictionary<int, int[]>());
            }

            List<int[]> lines = new List<int[]>();
            lineToPoint.Clear();

            foreach (var line in allSimplices[1])
            {
                lines.Add(line.Value);
                lineToPoint.Add(line.Key, lines.Count - 1);
            }

            // calculate line intersections
            if (useShader)
            {
                (float[] componentsA, float[] componentsB) = FlattenLines(points, lines, currDimension);
                intersectionPoints = shaderInterface.FindIntersections(componentsA, componentsB, currDimension);
            }
            else
            {
                intersectionPoints = new();
                foreach (int[] line in lines)
                {
                    intersectionPoints.Add(FindLineIntersection(points[line[0]], points[line[1]], currDimension));
                }
            }

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

        foreach (var intersectionLine in allIntersectionSimplices[1])
        {
            IntersectionLines.Add((intersectionPoints[intersectionLine.Value[0]].toVector3(), intersectionPoints[intersectionLine.Value[1]].toVector3()));
        }
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
    /// <returns>The intersection between the two points</returns>
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
        if (point == null)
        {
            return false;
        }
        float component = point.components[0];
        if (float.IsNaN(component) || float.IsInfinity(component))
        {
            return false;
        }
        return true;
    }

    public List<VectorN> GetPoints() => Points;
    public List<List<int[]>> GetAllSimplices() => AllSimplices;
}
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
    public FaceDrawer faceDrawer;

    public VectorN Origin { private set; get; }
    private List<VectorN> Points = new();
    private List<List<int[]>> AllSimplices = new();

    private List<(Vector3, Vector3)> IntersectionLines = new();
    private List<Vector3[]> IntersectionPlanes = new();

    private Dictionary<int, List<(Vector3, Vector3)>> DebugIntersectionLines = new();

    private bool useShader = false;

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

        BuildNCube();
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
        Vector3 centroid = Vector3.zero;

        foreach (var (start, end) in IntersectionLines)
        {
            points.Add(start);
            points.Add(end);
            centroid += start + end;
        }
        if (points.Count > 0)
        {
            centroid /= points.Count;
        }

        lineDrawer.DrawLineList(points.ToArray());

        faceDrawer.ClearFaces();
        foreach (var plane in IntersectionPlanes)
        {
            faceDrawer.DrawOneFace(centroid, plane);
        }
    }

    private void DebugDraw3D()
    {
        if (!draw3D) { return; }

        Gizmos.color = Color.white;
        foreach (var (start, end) in IntersectionLines)
        {
            Gizmos.DrawSphere(start, 0.05f);
            Gizmos.DrawSphere(end, 0.05f);
        }
    }

    private void DebugDrawAllProjection()
    {
        if (!debugLines || !DebugIntersectionLines.TryGetValue(debugIntersectionDimension, out var lines)) { return; }

        Gizmos.color = Color.yellow;
        List<Vector3> points = new();

        foreach (var (start, end) in lines)
        {
            points.Add(start);
            points.Add(end);

            Gizmos.DrawSphere(start, 0.05f);
            Gizmos.DrawSphere(end, 0.05f);
        }
        Gizmos.DrawLineList(points.ToArray());
    }

    public void SetTranslation(int axis, float amount)
    {
        VectorN offset = VectorN.Unit(dimension, axis - 1) * (amount - Origin[axis - 1]);
        Origin[axis - 1] = amount;
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] += offset;
        }
    }

    public void Translate(int axis, float amount) => SetTranslation(axis, Origin[axis - 1] + amount);

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

        if (axisA < 0 || axisA >= dimension || axisB < 0 || axisB >= dimension)
        {
            throw new Exception("Axes must be within the dimension range");
        }

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

    public void RandomizeRotation()
    {
        for (int i = 1; i < dimension; i++)
        {
            for (int j = i + 1; j <= dimension; j++)
            {
                Rotate(i, j, Random.Range(0, 2 * Mathf.PI));
            }
        }
    }

    // Extrude the cube one axis at a time: each pass duplicates every existing simplex,
    // shifts originals and copies to opposite sides of the new axis, and connects each
    // original to its copy with a simplex one dimension higher.
    private void BuildNCube()
    {
        if (dimension < 1) { throw new Exception(); }

        for (int i = 0; i <= dimension; i++)
        {
            AllSimplices.Add(new List<int[]>());
        }

        Points.Add(VectorN.Zero(dimension));
        for (int axis = 1; axis <= dimension; axis++)
        {
            int pointsCount = Points.Count;
            int[] initSimplexCount = AllSimplices.Select(simplices => simplices.Count).ToArray();

            for (int simplexDim = axis; simplexDim > 0; simplexDim--)
            {
                int simplexCount = initSimplexCount[simplexDim];
                int lowerSimplexCount = simplexDim == 1 ? pointsCount : initSimplexCount[simplexDim - 1];
                for (int currSimplex = 0; currSimplex < simplexCount; currSimplex++)
                {
                    int[] simplex = AllSimplices[simplexDim][currSimplex];

                    int[] copiedSimplex = simplex.Select(index => index + lowerSimplexCount).ToArray();
                    AllSimplices[simplexDim].Add(copiedSimplex);

                    if (simplexDim < dimension)
                    {
                        int[] higherSimplex = new int[simplex.Length + 2];
                        higherSimplex[0] = currSimplex;
                        higherSimplex[1] = AllSimplices[simplexDim].Count - 1;

                        for (int i = 0; i < simplex.Length; i++)
                        {
                            higherSimplex[i + 2] = simplex[i] + 2 * simplexCount;
                        }
                        AllSimplices[simplexDim + 1].Add(higherSimplex);
                    }
                }
            }

            VectorN offset = VectorN.Unit(dimension, axis - 1);
            for (int currPoint = 0; currPoint < pointsCount; currPoint++)
            {
                Points.Add(Points[currPoint] + offset);
                Points[currPoint] -= offset;

                AllSimplices[1].Add(new[] { currPoint, Points.Count - 1 });
            }
        }
    }

    public void FindIntersection()
    {
        IntersectionLines.Clear();
        IntersectionPlanes.Clear();

        if (dimension <= 3)
        {
            PopulateIntersectionLines(AllSimplices[1], Points);
            if (dimension == 3)
            {
                PopulateIntersectionPlanes(AllSimplices[2], index => AllSimplices[1][index], Points);
            }
            return;
        }

        // Simplices are keyed by their original index so references stay valid as
        // simplices without intersections drop out at each projection step.
        List<VectorN> points = Points;
        List<Dictionary<int, int[]>> allSimplices = new();
        foreach (List<int[]> simplices in AllSimplices)
        {
            Dictionary<int, int[]> keyed = new();
            for (int i = 0; i < simplices.Count; i++)
            {
                keyed[i] = simplices[i];
            }
            allSimplices.Add(keyed);
        }

        // Project down one dimension at a time by intersecting with the hyperplane
        // where that dimension's component is 0.
        for (int currDimension = dimension; currDimension > 3; currDimension--)
        {
            DebugIntersectionLines[currDimension].Clear();
            foreach (int[] line in allSimplices[1].Values)
            {
                DebugIntersectionLines[currDimension].Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
            }

            List<Dictionary<int, int[]>> allIntersectionSimplices = new();
            for (int i = 0; i < currDimension; i++)
            {
                allIntersectionSimplices.Add(new Dictionary<int, int[]>());
            }

            List<int[]> lines = new();
            Dictionary<int, int> lineToPoint = new();
            foreach (var pair in allSimplices[1])
            {
                lines.Add(pair.Value);
                lineToPoint.Add(pair.Key, lines.Count - 1);
            }

            // calculate line intersections
            List<VectorN> intersectionPoints;
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

            // a face survives as a line if any of its lines intersect the hyperplane
            foreach (var face in allSimplices[2])
            {
                int[] intersections = face.Value
                    .Where(line => IsValid(intersectionPoints[lineToPoint[line]]))
                    .Select(line => lineToPoint[line])
                    .ToArray();
                if (intersections.Length > 0)
                {
                    allIntersectionSimplices[1].Add(face.Key, intersections);
                }
            }

            // a higher simplex survives if any of its lower simplices survived
            for (int simplexDim = 3; simplexDim <= currDimension; simplexDim++)
            {
                foreach (var simplex in allSimplices[simplexDim])
                {
                    int[] intersections = simplex.Value
                        .Where(allIntersectionSimplices[simplexDim - 2].ContainsKey)
                        .ToArray();
                    if (intersections.Length > 0)
                    {
                        allIntersectionSimplices[simplexDim - 1].Add(simplex.Key, intersections);
                    }
                }
            }

            points = intersectionPoints;
            allSimplices = allIntersectionSimplices;
        }

        PopulateIntersectionLines(allSimplices[1].Values, points);
        PopulateIntersectionPlanes(allSimplices[2].Values, index => allSimplices[1][index], points);
    }

    private (float[], float[]) FlattenLines(List<VectorN> points, List<int[]> lines, int dimension)
    {
        float[] componentsA = new float[lines.Count * dimension];
        float[] componentsB = new float[lines.Count * dimension];

        for (int i = 0; i < lines.Count; i++)
        {
            int[] currLine = lines[i];
            Array.Copy(points[currLine[0]].components, 0, componentsA, i * dimension, dimension);
            Array.Copy(points[currLine[1]].components, 0, componentsB, i * dimension, dimension);
        }
        return (componentsA, componentsB);
    }

    /// <summary>
    /// Find the intersection point of a line with the plane where the nth dimension is 0
    /// </summary>
    /// <param name="pointA">Point A of the line</param>
    /// <param name="pointB">Point B of the line</param>
    /// <param name="dimension">The dimensional component that should be 0</param>
    /// <returns>The intersection between the two points, or null if the line does not cross the plane</returns>
    private VectorN FindLineIntersection(VectorN pointA, VectorN pointB, int dimension)
    {
        int index = dimension - 1;

        if (pointA[index] * pointB[index] >= 0)
        {
            return null;
        }
        float k = pointA[index] / (pointA[index] - pointB[index]);
        return (pointA * (1 - k) + pointB * k).Reduce(dimension - 1);
    }

    private bool IsValid(VectorN point) =>
        point != null && !float.IsNaN(point.components[0]) && !float.IsInfinity(point.components[0]);

    private void PopulateIntersectionLines(IEnumerable<int[]> lines, List<VectorN> points)
    {
        foreach (int[] line in lines)
        {
            IntersectionLines.Add((points[line[0]].toVector3(), points[line[1]].toVector3()));
        }
    }

    private void PopulateIntersectionPlanes(IEnumerable<int[]> faces, Func<int, int[]> getLine, List<VectorN> points)
    {
        foreach (int[] face in faces)
        {
            List<Vector3> planePoints = new();
            HashSet<int> pointIndices = new();

            foreach (int lineIndex in face)
            {
                foreach (int pointIndex in getLine(lineIndex))
                {
                    if (pointIndices.Add(pointIndex))
                    {
                        planePoints.Add(points[pointIndex].toVector3());
                    }
                }
            }

            IntersectionPlanes.Add(planePoints.ToArray());
        }
    }

    public List<VectorN> GetPoints() => Points;
    public List<List<int[]>> GetAllSimplices() => AllSimplices;
}

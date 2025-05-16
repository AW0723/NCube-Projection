using System.Collections.Generic;
using UnityEngine;

public class CodeDebug : MonoBehaviour
{
    public bool debug;

    public NCubeController nCubeController;
    public ShaderInterface shaderInterface;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var point in nCubeController.GetPoints())
        {
            Debug.Log(point);
        }
        foreach (var simplices in nCubeController.GetAllSimplices())
        {
            foreach (var simplex in simplices)
            {
                Debug.Log(string.Join(" ", simplex));
            }
        }

        List<VectorN> pointsA = new List<VectorN>()
        {
            new VectorN(1, 1, 1),
            new VectorN(1, 1, 1),
            new VectorN(1, 1, 1),
        };

        List<VectorN> pointsB = new List<VectorN>()
        {
            new VectorN(1, 1, -1),
            new VectorN(-1, -1, -1),
            new VectorN(-1, 1, 1),
        };

        for (int i = 0; i < pointsA.Count; i++)
        {
            Debug.Log(FindLineIntersection(pointsA[i], pointsB[i], 3));
        }

        List<VectorN> result = shaderInterface.FindIntersections(pointsA, pointsB, 3);
        foreach (VectorN point in result)
        {
            Debug.Log(point);
        }
    }

    // Update is called once per frame
    void Update()
    {

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
}

using System.Collections.Generic;
using UnityEngine;

public class NCubeCreator : MonoBehaviour
{
    public int dimension;
    public List<float> position;

    private List<VectorN> points = new List<VectorN>();
    private List<(int, int)> lines = new List<(int, int)>();

    // Start is called before the first frame update
    void Start()
    {
        BuildNCube(dimension);
        foreach (VectorN point in points)
        {
            Debug.Log(point);
        }
        foreach (var line in lines)
        {
            Debug.Log(line);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void BuildNCube(int dimension)
    {
        if (dimension < 1) { throw new System.Exception(); }

        points.Add(VectorN.zero(dimension));
        for (int currDimension = 1; currDimension <= dimension; currDimension++)
        {
            int pointsCount = points.Count;
            int linesCount = lines.Count;

            for (int currLine = 0; currLine < linesCount; currLine++)
            {
                (int, int) line = lines[currLine];
                lines.Add((line.Item1 + pointsCount, line.Item2 + pointsCount));
            }

            for (int currPoint = 0; currPoint < pointsCount; currPoint++)
            {
                VectorN offset = VectorN.unit(dimension, currDimension - 1);
                points.Add(points[currPoint] + offset);
                points[currPoint] -= offset;

                lines.Add((currPoint, points.Count - 1));
            }
        }
    }
}
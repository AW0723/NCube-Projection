using System.Collections.Generic;
using UnityEngine;

public class NCubeController : MonoBehaviour
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

    private void FixedUpdate()
    {
        foreach (var line in lines)
        {
            VectorN point1 = points[line.Item1];
            VectorN point2 = points[line.Item2];
            Vector3 pos1 = new Vector3(point1[0], point1[1], point1[2]);
            Vector3 pos2 = new Vector3(point2[0], point2[1], point2[2]);

            Debug.DrawLine(pos1, pos2);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (var point in points)
        {
            Vector3 pos = new Vector3(point[0], point[1], point[2]);
            Gizmos.DrawSphere(pos, 0.05f);
        }
    }

    public void Translate(int axis, float amount)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += VectorN.unit(dimension, axis - 1) * amount;
        }
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
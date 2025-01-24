using System.Collections.Generic;
using UnityEngine;

public class NCube : MonoBehaviour
{
    public int dimension;
    public List<float> position;

    private List<VectorN> points = new List<VectorN>();

    // Start is called before the first frame update
    void Start()
    {
        points = BuildPoints(dimension);
        foreach (var point in points)
        {
            Debug.Log(point);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private static List<VectorN> BuildPoints(int dimension)
    {
        List<VectorN> result = new List<VectorN>() { VectorN.zero(dimension) };
        for (int i = 0; i < dimension; i++)
        {
            int count = result.Count;
            for (int j = 0; j < count; j++)
            {
                result.Add(result[j] + VectorN.unit(dimension, i));
                result[j] = result[j] - VectorN.unit(dimension, i);
            }
        }
        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public ComputeShader computeShader;

    // Start is called before the first frame update
    void Start()
    {
        List<VectorN> pointsA = new List<VectorN>()
        {
            new VectorN(100, 2, 3),
            new VectorN(4, 5, 6),
            new VectorN(7, 1, 9),
            new VectorN(10, 11, 12),
            new VectorN(13, 14, 15),
            new VectorN(16, 17, 18),
            new VectorN(19, 20, 21),
            new VectorN(22, 23, 24),
            new VectorN(25, 26, 27),
            new VectorN(28, 29, 30),
            new VectorN(31, 32, 33),
            new VectorN(34, 35, 36),
            new VectorN(37, 38, 39),
            new VectorN(40, 41, 42),
            new VectorN(43, 44, 45),
            new VectorN(46, 47, 48),
            new VectorN(49, 50, 51),
            new VectorN(52, 53, 54),
            new VectorN(55, 56, 57),
            new VectorN(58, 59, 60),
            new VectorN(61, 62, 63),
            new VectorN(64, 65, 66),
            new VectorN(67, 68, 69),
            new VectorN(70, 71, 72),
            new VectorN(73, 74, 75),
            new VectorN(76, 77, 78),
            new VectorN(79, 80, 81),
            new VectorN(82, 83, 84),
            new VectorN(85, 86, 87),
            new VectorN(88, 89, 90),
        };
        List<VectorN> pointsB = new List<VectorN>()
        {
            new VectorN(1, 2, 3),
            new VectorN(4, 5, 6),
            new VectorN(7, 8, 9),
            new VectorN(10, 1, 12),
            new VectorN(13, 14, 15),
            new VectorN(16, 17, 18),
            new VectorN(19, 20, 21),
            new VectorN(22, 23, 24),
            new VectorN(25, 26, 27),
            new VectorN(28, 29, 30),
            new VectorN(31, 32, 33),
            new VectorN(34, 35, 36),
            new VectorN(37, 38, 39),
            new VectorN(40, 41, 42),
            new VectorN(43, 44, 45),
            new VectorN(46, 47, 48),
            new VectorN(49, 50, 51),
            new VectorN(52, 53, 54),
            new VectorN(55, 56, 57),
            new VectorN(58, 59, 60),
            new VectorN(61, 62, 63),
            new VectorN(64, 65, 66),
            new VectorN(67, 68, 69),
            new VectorN(70, 71, 72),
            new VectorN(73, 74, 75),
            new VectorN(76, 77, 78),
            new VectorN(79, 80, 81),
            new VectorN(82, 83, 84),
            new VectorN(85, 86, 87),
            new VectorN(250, 1, 90),
        };
        List<VectorN> result = FindIntersections(pointsA, pointsB, 3);
        foreach (VectorN point in result)
        {
            Debug.Log(point);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<VectorN> FindIntersections(List<VectorN> pointsA, List<VectorN> pointsB, int dimension)
    {
        int kernelIndex = computeShader.FindKernel("CSMain");
        int pointsCount = pointsA.Count;

        // 4 comes from numthreads 2x2
        int groupSizeX = CeilingDivide(pointsCount, 4);
        int groupSizeY = 1;

        computeShader.SetInt("dimension", dimension);
        computeShader.SetInt("pointsCount", pointsCount);
        computeShader.SetInt("groupSizeX", groupSizeX);
        computeShader.SetInt("groupSizeY", groupSizeY);

        int resultsLength = pointsA.Count * (dimension - 1);

        // points buffer
        ComputeBuffer pointsABuffer = GetPointsBuffer(pointsA, dimension);
        ComputeBuffer pointsBBuffer = GetPointsBuffer(pointsB, dimension);

        computeShader.SetBuffer(kernelIndex, "pointsA", pointsABuffer);
        computeShader.SetBuffer(kernelIndex, "pointsB", pointsBBuffer);

        // results buffer
        ComputeBuffer resultBuffer = new ComputeBuffer(resultsLength, sizeof(float));
        computeShader.SetBuffer(kernelIndex, "result", resultBuffer);

        // debug buffer
        ComputeBuffer debugXBuffer = new ComputeBuffer(2 * groupSizeX, sizeof(uint));
        ComputeBuffer debugYBuffer = new ComputeBuffer(2 * groupSizeY, sizeof(uint));
        computeShader.SetBuffer(kernelIndex, "debugX", debugXBuffer);
        computeShader.SetBuffer(kernelIndex, "debugY", debugYBuffer);

        computeShader.Dispatch(kernelIndex, groupSizeX, groupSizeY, 1);

        float[] resultAry = new float[resultsLength];
        resultBuffer.GetData(resultAry);

        uint[] debugX = new uint[2 * groupSizeX];
        uint[] debugY = new uint[2 * groupSizeY];
        debugXBuffer.GetData(debugX);
        debugYBuffer.GetData(debugY);

        Debug.Log(string.Join(" ", debugX));
        Debug.Log(string.Join(" ", debugY));

        List<VectorN> resultList = new List<VectorN>();
        for (int i = 0; i < resultAry.Length; i += dimension - 1)
        {
            VectorN newVec = new VectorN(dimension - 1);
            Array.Copy(resultAry, i, newVec.components, 0, dimension - 1);
            resultList.Add(newVec);
        }

        pointsABuffer.Dispose();
        pointsBBuffer.Dispose();
        resultBuffer.Dispose();
        debugXBuffer.Dispose();
        debugYBuffer.Dispose();

        return resultList;
    }

    private ComputeBuffer GetPointsBuffer(List<VectorN> points, int dimension)
    {
        int componentsLength = points.Count * dimension;
        float[] array = points.SelectMany(point => point.components).ToArray();
        ComputeBuffer buffer = new ComputeBuffer(componentsLength, sizeof(float));
        buffer.SetData(array);
        return buffer;
    }

    private int CeilingDivide(int a, int b)
    {
        return (a + b - 1) / b;
    }
}

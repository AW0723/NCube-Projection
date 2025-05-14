using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShaderInterface : MonoBehaviour
{
    public NCubeController controller;
    public ComputeShader computeShader;

    // Start is called before the first frame update
    void Start()
    {
        List<VectorN> pointsA = controller.pointsA;
        List<VectorN> pointsB = controller.pointsB;
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
        float[] componentsA = pointsA.SelectMany(point => point.components).ToArray();
        float[] componentsB = pointsB.SelectMany(point => point.components).ToArray();
        return FindIntersections(componentsA, componentsB, dimension);
    }

    public List<VectorN> FindIntersections(float[] componentsA, float[] componentsB, int dimension)
    {
        if (componentsA.Length == 0 || componentsB.Length == 0)
        {
            return new List<VectorN>();
        }

        int kernelIndex = computeShader.FindKernel("CSMain");
        int pointsCount = componentsA.Length / dimension;

        // numthreads 4x4
        int groupSizeX = CeilingDivide(pointsCount, 4 * 4);
        int groupSizeY = 1;

        computeShader.SetInt("dimension", dimension);
        computeShader.SetInt("pointsCount", pointsCount);
        computeShader.SetInt("groupSizeX", groupSizeX);
        computeShader.SetInt("groupSizeY", groupSizeY);

        int resultsLength = pointsCount * (dimension - 1);

        // points buffer
        ComputeBuffer pointsABuffer = GetPointsBuffer(componentsA);
        ComputeBuffer pointsBBuffer = GetPointsBuffer(componentsB);

        computeShader.SetBuffer(kernelIndex, "pointsA", pointsABuffer);
        computeShader.SetBuffer(kernelIndex, "pointsB", pointsBBuffer);

        // results buffer
        ComputeBuffer resultBuffer = new ComputeBuffer(resultsLength, sizeof(float));
        computeShader.SetBuffer(kernelIndex, "result", resultBuffer);

        // debug buffer
        ComputeBuffer debugXBuffer = new ComputeBuffer(4 * groupSizeX, sizeof(uint));
        ComputeBuffer debugYBuffer = new ComputeBuffer(4 * groupSizeY, sizeof(uint));
        computeShader.SetBuffer(kernelIndex, "debugX", debugXBuffer);
        computeShader.SetBuffer(kernelIndex, "debugY", debugYBuffer);

        computeShader.Dispatch(kernelIndex, groupSizeX, groupSizeY, 1);

        float[] resultAry = new float[resultsLength];
        resultBuffer.GetData(resultAry);

        uint[] debugX = new uint[4 * groupSizeX];
        uint[] debugY = new uint[4 * groupSizeY];
        debugXBuffer.GetData(debugX);
        debugYBuffer.GetData(debugY);

        //Debug.Log(string.Join(" ", debugX));
        //Debug.Log(string.Join(" ", debugY));

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

    private ComputeBuffer GetPointsBuffer(float[] components)
    {
        int componentsLength = components.Length;
        ComputeBuffer buffer = new ComputeBuffer(componentsLength, sizeof(float));
        buffer.SetData(components);
        return buffer;
    }

    private ComputeBuffer GetPointsBuffer(List<VectorN> points, int dimension)
    {
        int componentsLength = points.Count * dimension;
        float[] array = new float[componentsLength];
        for (int i = 0; i < points.Count; i++)
        {
            Array.Copy(points[i].components, 0, array, i * dimension, dimension);
        }
        ComputeBuffer buffer = new ComputeBuffer(componentsLength, sizeof(float));
        buffer.SetData(array);
        return buffer;
    }

    private int CeilingDivide(int a, int b)
    {
        return (a + b - 1) / b;
    }
}

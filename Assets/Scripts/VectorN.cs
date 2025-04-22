using System;
using System.Collections.Generic;
using System.Linq;

public class VectorN
{
    public float[] components { get; private set; }

    public int dimension
    {
        get { return components.Length; }
    }

    public VectorN(int dimension)
    {
        components = new float[dimension];
    }

    public VectorN(params float[] components)
    {
        this.components = components;
    }

    public VectorN(IEnumerable<float> components)
    {
        this.components = components.ToArray();
    }

    public VectorN(VectorN copy)
    {
        components = new float[copy.components.Length];
        Array.Copy(copy.components, components, copy.components.Length);
    }

    public float this[int index]
    {
        get
        {
            if (index < 0 || index >= components.Length)
            {
                throw new IndexOutOfRangeException("Invalid VectorN index!");
            }
            return components[index];
        }
        set
        {
            if (index < 0 || index >= components.Length)
            {
                throw new IndexOutOfRangeException("Invalid VectorN index!");
            }
            components[index] = value;
        }
    }

    public override string ToString()
    {
        string result = "[";
        for (int i = 0; i < components.Length; i++)
        {
            result += components[i].ToString() + (i != components.Length - 1 ? ", " : "");
        }
        result += "]";
        return result;
    }

    public static VectorN operator +(VectorN a, VectorN b)
    {
        return new VectorN(Map(a.components, b.components, (x, y) => x + y));
    }

    public static VectorN operator -(VectorN a, VectorN b)
    {
        return new VectorN(Map(a.components, b.components, (x, y) => x - y));
    }

    public static VectorN operator -(VectorN a)
    {
        return new VectorN(a.components.Select(x => -x));
    }

    public static VectorN operator *(float num, VectorN vector)
    {
        float[] components = new float[vector.components.Length];
        Array.Copy(vector.components, components, vector.components.Length);
        for (int i = 0; i < components.Length; i++)
        {
            components[i] *= num;
        }
        return new VectorN(components);
    }

    public static VectorN operator *(VectorN vector, float num)
    {
        return num * vector;
    }

    public static float[] Map(float[] a, float[] b, Func<float, float, float> f)
    {
        if (a.Length != b.Length) throw new Exception("Dimensions do not match");
        return a.Zip(b, (x, y) => f(x, y)).ToArray();
    }

    public static VectorN zero(int dimension)
    {
        return fill(dimension, 0);
    }

    public static VectorN one(int dimension)
    {
        return fill(dimension, 1);
    }

    public static VectorN unit(int dimension, int index)
    {
        if (index >= dimension) throw new IndexOutOfRangeException("Index cannot be larger or equal to dimension.");
        VectorN result = zero(dimension);
        result.components[index] = 1;
        return result;
    }

    public static VectorN fill(int dimension, float value)
    {
        float[] array = new float[dimension];
        Array.Fill(array, value);
        return new VectorN(array);
    }

    public static float Dot(VectorN a, VectorN b)
    {
        if (a.dimension != b.dimension) throw new Exception("Dimensions must match.");

        float result = 0;
        for (int i = 0; i < a.dimension; i++)
        {
            result += a.components[i] * b.components[i];
        }
        return result;
    }
}

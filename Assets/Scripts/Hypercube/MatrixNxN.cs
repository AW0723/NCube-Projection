using System;

public class MatrixNxN
{
    public VectorN[] columns;

    public int dimension
    {
        get { return columns.Length; }
    }

    public MatrixNxN(params VectorN[] columns)
    {
        if (columns.Length == 0) { return; }

        int height = columns[0].dimension;
        foreach (VectorN column in columns)
        {
            if (column.dimension != height)
            {
                throw new Exception("Columns should have same height.");
            }
        }
        if (columns.Length != height)
        {
            throw new Exception("Only square matrix supported.");
        }
        this.columns = columns;
    }

    public float this[int row, int column]
    {
        get
        {
            return columns[column][row];
        }
        set
        {
            columns[column][row] = value;
        }
    }

    public override string ToString()
    {
        string result = "";
        for (int col = 0; col < dimension; col++)
        {
            for (int row = 0; row < dimension; row++)
            {
                result += columns[row][col];
                if (row != dimension - 1)
                {
                    result += "\t";
                }
                else if (col != dimension - 1)
                {
                    result += "\n";
                }
            }
        }
        return result;
    }

    public static MatrixNxN identity(int dimension)
    {
        VectorN[] columns = new VectorN[dimension];
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = VectorN.Zero(dimension);
            columns[i][i] = 1;
        }
        return new MatrixNxN(columns);
    }

    public static VectorN operator *(MatrixNxN matrix, VectorN vector)
    {
        if (matrix.dimension != vector.dimension)
        {
            throw new Exception("Matrix and vector dimensions must match");
        }

        VectorN result = VectorN.Zero(matrix.dimension);
        for (int i = 0; i < matrix.dimension; i++)
        {
            result += vector[i] * matrix.columns[i];
        }
        return result;
    }
}

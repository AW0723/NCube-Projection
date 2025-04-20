public class Line
{
    private VectorN pointA;
    private VectorN pointB;

    public Line(VectorN pointA, VectorN pointB)
    {
        this.pointA = pointA;
        this.pointB = pointB;
    }

    public Line(Line line)
    {
        pointA = new VectorN(line.pointA);
        pointB = new VectorN(line.pointB);
    }

    public void Translate(VectorN vec)
    {
        pointA += vec;
        pointB += vec;
    }

    public override string ToString()
    {
        return "[" + pointA + ", " + pointB + "]";
    }
}

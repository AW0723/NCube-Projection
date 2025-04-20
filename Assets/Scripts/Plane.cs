using System.Collections.Generic;

public class Plane
{
    private List<Line> lines = new List<Line>();

    public Plane(Line line1, Line line2, Line line3, Line line4)
    {
        lines.Add(line1);
        lines.Add(line2);
        lines.Add(line3);
        lines.Add(line4);
    }
}

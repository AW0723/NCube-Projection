using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject linePrefab;
    public int lineCount = 50;
    public float lineSpace = 0.5f;
    public float lineWidth = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float lineLength = lineCount * lineSpace / 2;

        Vector3 offsetX = new Vector3(0, 0, lineSpace);
        Vector3 lineLengthX = new Vector3(lineLength, 0, 0);
        GenerateLines(lineLengthX, offsetX);

        Vector3 offsetY = new Vector3(lineSpace, 0, 0);
        Vector3 lineLengthY = new Vector3(0, 0, lineLength);
        GenerateLines(lineLengthY, offsetY);
    }

    private void GenerateLines(Vector3 lineLength, Vector3 offset)
    {
        Vector3 currPos = -lineCount / 2 * offset;
        for (int i = 0; i <= lineCount; i++)
        {
            Vector3 start = new Vector3(currPos.x - lineLength.x, transform.position.y, currPos.z - lineLength.z);
            Vector3 end = new Vector3(currPos.x + lineLength.x, transform.position.y, currPos.z + lineLength.z);
            ConnectTwoPoints(start, end);
            currPos += offset;
        }
    }

    private void ConnectTwoPoints(Vector3 start, Vector3 end)
    {
        GameObject obj = Instantiate(linePrefab, transform);
        LineRenderer lineRenderer = obj.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }
}

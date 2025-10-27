using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public float lineWidth;
    public GameObject lineRendererPrefab;

    private List<GameObject> lineRenderers = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawLineList(Vector3[] points)
    {
        int lineCount = points.Length / 2;
        for (int i = 0; i < lineCount; i++)
        {
            if (i >= lineRenderers.Count)
            {
                lineRenderers.Add(Instantiate(lineRendererPrefab, transform));
            }
            LineRenderer lineRenderer = lineRenderers[i].GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, points[2 * i]);
            lineRenderer.SetPosition(1, points[2 * i + 1]);
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }

        for (int i = lineCount; i < lineRenderers.Count; i++)
        {
            LineRenderer lineRenderer = lineRenderers[i].GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0;
            lineRenderer.endWidth = 0;
        }
    }
}

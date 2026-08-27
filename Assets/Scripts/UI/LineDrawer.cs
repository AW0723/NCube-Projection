using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public float lineWidth;
    public GameObject lineRendererPrefab;

    private readonly List<LineRenderer> lineRenderers = new();
    private Material lineMaterial;
    private float appliedLineWidth;

    private void Awake()
    {
        // Use the material from the parent LineRenderer if it exists
        LineRenderer parentLineRenderer = GetComponent<LineRenderer>();
        lineMaterial = parentLineRenderer != null ? parentLineRenderer.material : null;
    }

    public void DrawLineList(Vector3[] points)
    {
        int lineCount = points.Length / 2;

        // Re-style existing lines only when the width setting changed
        if (lineWidth != appliedLineWidth)
        {
            appliedLineWidth = lineWidth;
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                ApplyStyle(lineRenderer);
            }
        }

        for (int i = 0; i < lineCount; i++)
        {
            if (i >= lineRenderers.Count)
            {
                lineRenderers.Add(CreateLineRenderer());
            }
            LineRenderer lineRenderer = lineRenderers[i];
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, points[2 * i]);
            lineRenderer.SetPosition(1, points[2 * i + 1]);
        }

        // Hide leftover renderers so they can be reused by a later, larger draw
        for (int i = lineCount; i < lineRenderers.Count; i++)
        {
            lineRenderers[i].enabled = false;
        }
    }

    private LineRenderer CreateLineRenderer()
    {
        LineRenderer lineRenderer = Instantiate(lineRendererPrefab, transform).GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        ApplyStyle(lineRenderer);
        return lineRenderer;
    }

    private void ApplyStyle(LineRenderer lineRenderer)
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // Fallback to default colors if the parent doesn't have a material
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        }
    }
}

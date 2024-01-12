using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public List<Vector3> points;

    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (points == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].x, -0.45f, points[i].z));
        }
    }

    public void UpdatePath (List<Vector3> newSetOfPoints)
    {
        points = newSetOfPoints;
        UpdatePath();
    }
}
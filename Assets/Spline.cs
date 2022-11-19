using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePoint
{
    public Vector3 anchorA;
    public Vector3 anchorB;
    public Vector3 position;
    public SplinePoint(Vector3 pos)
    {
        position = pos;
        anchorA = null;
        anchorB = pos;
    }
}


public class Spline : MonoBehaviour
{
    List<SplinePoint> splinePoints = new List<SplinePoint>();
    private SplinePoint selectedPoint;
    private float splinePointRadius = 0.05f;
    private void Start()
    {
        splinePoints.Add(new SplinePoint(Vector3.zero));
        splinePoints.Add(new SplinePoint(Vector3.up));
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < splinePoints.Count; i++)
        {
            Gizmos.DrawSphere(splinePoints[i].position, splinePointRadius);
            Gizmos.DrawSphere(splinePoints[i].anchorA, splinePointRadius);
            Gizmos.DrawSphere(splinePoints[i].anchorB, splinePointRadius);
        }
    }

    private void Update()
    {
        bool mousePressed = Input.GetMouseButtonDown(0);
        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool mouseHold = Input.GetMouseButton(0);
        if (mousePressed)
        {
            for(int i = 0; i < splinePoints.Count; i++)
            {
                Plane plane = new Plane(Camera.main.transform.forward, splinePoints[i].position);
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distanceToObject;
                float dist = Mathf.Infinity;
                if(plane.Raycast(ray, out distanceToObject))
                {
                    Vector3 mousePoint = ray.GetPoint(distanceToObject);
                    dist = Vector3.Distance(mousePoint, splinePoints[i].position);
                }
                if (dist <= splinePointRadius)
                {
                    selectedPoint = splinePoints[i];
                }
            }
            
        }
        if (mouseReleased)
        {
            selectedPoint = null;
        }
        if (mouseHold)
        {
            if(selectedPoint != null)
            {
                Plane plane = new Plane(Camera.main.transform.forward, selectedPoint.position);
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distanceToObject;
                float dist = Mathf.Infinity;
                if (plane.Raycast(ray, out distanceToObject))
                {
                    Vector3 mousePoint = ray.GetPoint(distanceToObject);
                    selectedPoint.position = mousePoint;
                }
            }
        }
    }
}

// TODO for tomorrow:
// - make it more efficient
// - make the movement controllabel
// - impliment actual spline
// - spline point creation
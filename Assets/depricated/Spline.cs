using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SplinePoint
{
    public float pointradius = 0.1f;
    public Vector3 anchorA;
    public Vector3 anchorB;
    public Vector3 position;
    public SplinePoint(Vector3 pos)
    {
        position = pos;
        anchorA = pos + Vector3.up * pointradius;
        anchorB = pos + Vector3.down * pointradius;
    }
}

public enum SplineObject
{
    ANCHOR_A,
    ANCHOR_B,
    POSITION, 
    NONE
}

public class Spline : MonoBehaviour
{
    [SerializeField]
    List<SplinePoint> splinePoints = new List<SplinePoint>();
    private SplinePoint selectedPoint;
    private float splinePointRadius = 0.05f;
    private SplineObject currentSelected = SplineObject.NONE;
    private void Start()
    {
        #region Temporary
        if (!this.TryGetComponent<LineRenderer>(out connectionLine))
            connectionLine = this.gameObject.AddComponent<LineRenderer>();
        connectionLine.SetWidth(0.01f, 0.01f);
        #endregion
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
        CheckForSplineMovement();
        setupTemporaryRendering();
    }

    #region Temporary
    private LineRenderer connectionLine;
    private void setupTemporaryRendering()
    {
        int length = splinePoints.Count;
        connectionLine.positionCount = length;
        Vector3[] connectedPositions = new Vector3[length];
        for(int i = 0; i < length; i++)
        {
            Debug.Log(i);
            connectedPositions[i] = splinePoints[i].position;
        }
        connectionLine.SetPositions(connectedPositions);
    }
    #endregion

    bool CheckForSplineMovement()
    {
        bool mousePressed = Input.GetMouseButtonDown(0);
        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool mouseHold = Input.GetMouseButton(0);
        if (mousePressed)
        {
            for (int i = 0; i < splinePoints.Count; i++)
            {
                float dist = Mathf.Infinity;

                float dist1 = GetDistToHandle(splinePoints[i].position);
                float dist2 = GetDistToHandle(splinePoints[i].anchorA);
                float dist3 = GetDistToHandle(splinePoints[i].anchorB);

                SplineObject tmp;
                if (dist1 < dist2)
                {
                    if (dist1 < dist3)
                    {
                        dist = dist1;
                        tmp = SplineObject.POSITION;
                    }
                    else
                    {
                        dist = dist3;
                        tmp = SplineObject.ANCHOR_B;
                    }
                }
                else if (dist2 < dist3)
                {
                    dist = dist2;
                    tmp = SplineObject.ANCHOR_A;
                }
                else
                {
                    dist = dist3;
                    tmp = SplineObject.ANCHOR_B;
                }

                if (dist <= splinePointRadius)
                {
                    selectedPoint = splinePoints[i];
                    currentSelected = tmp;
                    break;
                }
                else
                    currentSelected = SplineObject.NONE;
            }
        }
        if (mouseReleased)
        {
            selectedPoint = null;
            currentSelected = SplineObject.NONE;
        }
        if (mouseHold)
        {
            if (selectedPoint != null)
            {
                ref Vector3 planePos = ref selectedPoint.position;
                ref Vector3 secundary = ref selectedPoint.position;
                bool moveAnchors = false;
                switch (currentSelected)
                {
                    case SplineObject.ANCHOR_A:
                        planePos = ref selectedPoint.anchorA;
                        moveAnchors = true;
                        secundary = ref selectedPoint.anchorB;
                        break;
                    case SplineObject.ANCHOR_B:
                        planePos = ref selectedPoint.anchorB;
                        moveAnchors = true;
                        secundary = ref selectedPoint.anchorA;
                        break;
                    case SplineObject.POSITION:
                        break;
                    case SplineObject.NONE:
                        return false;
                    default:
                        break;
                }
                Plane plane = new Plane(Camera.main.transform.forward, planePos);
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distanceToObject;
                float dist = Mathf.Infinity;
                if (plane.Raycast(ray, out distanceToObject))
                {
                    Vector3 mousePoint = ray.GetPoint(distanceToObject);
                    if (moveAnchors)
                        MoveSplineAnchor(ref planePos, ref secundary, selectedPoint.position, mousePoint);
                    else
                        MoveSplineCenter(selectedPoint, mousePoint);
                }
            }
        }
        if (currentSelected == SplineObject.NONE)
            return false;
        else return true;
    }

    private float GetDistToHandle(Vector3 handlePos)
    {
        Plane plane = new Plane(Camera.main.transform.forward, handlePos);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceToObject;
        if (plane.Raycast(ray, out distanceToObject))
        {
            Vector3 mousePoint = ray.GetPoint(distanceToObject);
            float dist = Vector3.Distance(mousePoint, handlePos);
            return dist;
        }
        else return Mathf.Infinity;
    }

    private void MoveSplineCenter(SplinePoint point, Vector3 pos)
    {
        Vector3 posDelta = pos - point.position;
        point.position = pos;
        point.anchorA += posDelta;
        point.anchorB += posDelta;
    }

    private void MoveSplineAnchor(ref Vector3 primary, ref Vector3 secondary, Vector3 center, Vector3 pos)
    {
        Vector3 primOffset = Vector3.Normalize(pos - center);
        float secOffset = Vector3.Magnitude(secondary - center);
        secondary = center + secOffset * -primOffset;  
        primary = pos;
    }
}


// TODO for tomorrow:
// - make it more efficient
// - make the movement controllabel
// - impliment actual spline
// - spline point creation
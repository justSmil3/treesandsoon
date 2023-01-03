using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMesh : MonoBehaviour
{
    public SplineTree tree;
    public float maxRadius = 10;
    public float verteciemult = 100;
    public float ra = .1f;

    private Vector3[] Tvertecies;
    // private Vector2[] uvs;
    // private int[] triangles;


    private void OnDrawGizmos()
    {
        if (Tvertecies != null)
            for (int i = 0; i < Tvertecies.Length; i++)
            {
                Gizmos.DrawSphere(Tvertecies[i], 0.01f);
            }
    }

    private void FixedUpdate()
    {
        tree.CalculateVigor();
        GetComponent<MeshFilter>().mesh = new Mesh();
        GenerateMeshspline(0, 0);
        for (int i = 0; i < tree.branches.Count; i++)
        {
            SplineNode current = tree.branches[i];
            for (int j = 1; j < current.GetNumberOfConnections(); j++)
            {
                GenerateMeshspline(i, j);
            }
        }
    }

    void GenerateMeshspline(int branchIndex, int starter = 0, int index = 0)
    {
        SplineNode root = tree.branches[branchIndex].GetSplineNode(index, starter);
        if (root != null)
        {
            GenerateMeshSegment(index, branchIndex, starter);
        }
        else return;
        index += 3;
        GenerateMeshspline(branchIndex, starter, index);
    }

    void GenerateMeshSegment(int startindex, int branch, int starter = 0)
    {

        // get distane of segment points
        SplineNode pp0 = tree.branches[branch].GetSplineNode(startindex, starter);
        if (pp0.GetNumberOfConnections() <= 0) return;
        Vector3 p0 = pp0.point;
        Vector3 p1 = tree.branches[branch].GetSplineNode(startindex + 1, starter).point;
        Vector3 p2 = tree.branches[branch].GetSplineNode(startindex + 2, starter).point;
        Vector3 p3 = tree.branches[branch].GetSplineNode(startindex + 3, starter).point;


        Vector3 p0p1 = p1 - p0;
        Vector3 p1p2 = p2 - p1;
        Vector3 p2p3 = p3 - p2;
        Vector3 p0p3 = p0p1 + p1p2 + p2p3;
        float length = p0p3.magnitude;

        Debug.DrawRay(p0, p0p3, Color.green);

        // get radius 
        float radius = Mathf.Max(1, maxRadius = 5);
        radius = ra;
        float radius1 = tree.branches[branch].GetSplineNode(startindex, starter).GetVigor();
        float radius2 = tree.branches[branch].GetSplineNode(startindex + 3, starter).GetVigor();
        radius1 /= 500;
        radius2 /= 500;

        // initialte vertecie, uv and triangle arrays
        float fWidth = radius * Mathf.Sqrt(verteciemult);

        int width = (int)(fWidth * 15);
        int heigth = System.Math.Max(2, (int)(length * Mathf.Sqrt(verteciemult)));
        int numtriangles = width * (heigth - 1) * 6;
        int numvertecies = width * heigth;
        Vector3[] vertecies = new Vector3[numvertecies];
        Vector2[] uvs = new Vector2[numvertecies];
        int[] triangles;// = new int[numtriangles];

        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        vertecies = FillVertexArray(vertecies, ref uvs, width, heigth, radius1, radius2, p0, p1, p2, p3);
        Tvertecies = vertecies;
        triangles = FillTriangleArray(width, heigth, mesh.vertexCount);

        List<Vector3> newVertecies = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newTriangles = new List<int>();

        newVertecies.AddRange(mesh.vertices);
        newUVs.AddRange(mesh.uv);
        newUVs.AddRange(uvs);
        newTriangles.AddRange(mesh.triangles);
        if (startindex > 0)
        {
            for (int i = 0; i < width; i++)
            {
                int ul = newVertecies.Count + width + i;
                int ur = newVertecies.Count + width + ((i + 1) % width);
                int dl = newVertecies.Count - width + i;
                int dr = newVertecies.Count - width + ((i + 1) % width);

                newTriangles.Add(ur);
                newTriangles.Add(ul);
                newTriangles.Add(dl);

                newTriangles.Add(ur);
                newTriangles.Add(dl);
                newTriangles.Add(dr);
            }
        }
        newVertecies.AddRange(vertecies);
        newTriangles.AddRange(triangles);
        mesh.vertices = newVertecies.ToArray();
        Tvertecies = newVertecies.ToArray();
        mesh.uv = newUVs.ToArray();
        mesh.triangles = newTriangles.ToArray();
    }

    Vector3[] FillVertexArray(Vector3[] v, ref Vector2[] vuw, int width, int height, float radius1, float radius2, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        for (int i = 0; i < v.Length; i++)
        {
            float t = (int)(i / width);
            float rt = i % width;
            Vector3 centerPoint = Bezier.GetPoint(p0, p1, p2, p3, t / (height - 1));
            Vector3 dir = Bezier.GetFirstDerivative(p0, p1, p2, p3, t / (height - 1));
            Vector3 W = Vector3.Cross(Vector3.right, dir.normalized);
            float radius = Mathf.Lerp(radius1, radius2, t / (height - 1));
            W = W.normalized * radius;
            Debug.DrawRay(centerPoint, dir * radius, Color.red);
            Debug.DrawRay(centerPoint, Vector3.right * radius, Color.black);
            Debug.DrawRay(centerPoint, W, Color.blue);
            float angleDivider = rt;
            if (t != 0)
            {
                Vector3 prevDir = Bezier.GetFirstDerivative(p0, p1, p2, p3, (t - 1) / (height - 1));
                float angle = Mathf.Abs(Vector3.Angle(prevDir, dir));
                if (angle > 90)
                    angleDivider = width - 1 - rt;
            }
            float angleMult = angleDivider / ((float)width - 1);
            W = Quaternion.AngleAxis(360 * (rt / ((float)width - 1)), dir.normalized) * W;
            if(rt == width - 1)
            Debug.DrawRay(centerPoint, W, Color.cyan);
            Vector3 point = centerPoint + W;
            v[i] = point;
            vuw[i] = new Vector2(rt, t);
        }
        return v;
    }

    int[] FillTriangleArray(int width, int height, int startIdx = 0) 
    {
        List<int> res = new List<int>();
        for (int i = 0; i < height-1; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int newJ = j + width * i;
                int ul = newJ + width;
                int ur = (j + 1) % width + (width * i) + width;
                int dr = (j + 1) % width + (width * i);
                int dl = newJ;

                ul += startIdx; 
                ur += startIdx; 
                dl += startIdx; 
                dr += startIdx;

                res.Add(ur);
                res.Add(ul);
                res.Add(dl);

                res.Add(ur);
                res.Add(dl);
                res.Add(dr);
            }
        }
        return res.ToArray();
    }
}

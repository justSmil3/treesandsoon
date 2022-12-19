using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bez2Lsys
{
    // old name = ConvertSplineTreeToLSystemString

    private static string convertSingleBranch(SplineNode startNode, SplineNode parent = null, int currentbranch = 0, bool bIsRoot = false)
    {
        string result = "";
        // starting node
        if (bIsRoot)
            result += ";0";

        if (parent != null)
        {
            Vector3 connection = startNode.point - parent.point;
            Vector3 direction = connection.normalized;

            float angleX = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(0f, direction.y, direction.z)));
            if (direction.x < 0)
                angleX = 360 - angleX;

            float angleZ = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(direction.x, direction.y, 0f)));
            if (direction.x < 0)
                angleZ = 360 - angleZ;

            result += "Ax" + angleX.ToString() + "z" + angleZ.ToString();

            result += "L" + connection.magnitude.ToString();
            int branches = startNode.GetNumberOfConnections();
            if (branches == 1)
            {
                result += ":" + startNode.branchIdx.ToString(); // lazy indexer however is not always the correct one
            }
            else
            {
                while (branches > 1)
                {
                    result += ";" + startNode.branchIdx.ToString(); // lazy indexer however is not always the correct one
                    branches--;
                }
            }
        }
        SplineNode nextNode = startNode.GetNextNode(currentbranch);
        if (nextNode != null)
        {
            result += convertSingleBranch(nextNode, startNode);
        }
        else result += ":]";
        return result;
    }

    public static bool Convert(List<SplineNode> tree, out string result)
    {
        result = "";
        if (tree == null || tree.Count == 0) return false;

        result += convertSingleBranch(tree[0], null, 0, true);

        for (int i = 1; i < tree.Count; i++)
        {
            // TODO danger i dont know if the nodes can be in here multiple times but for now i am assuming they are not
            for (int j = 1; j < tree[i].GetNumberOfConnections(); j++)
            {
                result += convertSingleBranch(tree[i], null, j);
            }
        }
        return true;
    }

    public static string ConvertBranch(SplineNode root, SplineNode prev = null, int idx = 0)
    {
        string result = "";
        if (prev != null)
        {
            Vector3 connection = root.point - prev.point;
            Vector3 direction = connection.normalized;

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, direction);
            //float angleX = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(0f, direction.y, direction.z)));
            //if (direction.x < 0)
            //    angleX = 360 - angleX;

            //float angleZ = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(direction.x, direction.y, 0f)));
            //if (direction.x < 0)
            //    angleZ = 360 - angleZ;

            result += "A" + rot.x.ToString() + "q" + rot.y.ToString() + "q" + rot.z.ToString() + "q" + rot.w.ToString();

            result += "L" + connection.magnitude.ToString();
            result += ':';
        }
        else result += ";";
        int i = root.GetNumberOfConnections() - 1;  
        if(i < 0)
        {
            result += "]";
            return result;
        }
        else if (idx % 3 == 0 && idx != 0)
        {
            result += "M";
        }
        for(int j = 0; j < i; j++)
        {
            result += ";";
        }
        for(; i >= 0; i--)
        {
            result += ConvertBranch(root.GetNextNode(i), root, idx+1);
        }
        return result;
    }

}

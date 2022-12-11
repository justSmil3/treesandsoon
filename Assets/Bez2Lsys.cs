using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bez2Lsys
{
    // old name = ConvertSplineTreeToLSystemString
    public static bool Convert(List<SplineNode> tree, out string result)
    {
        result = "";
        if (tree == null) return false;

        // starting node
        result += ";0";

        // iterate over the rest of the tree
        for (int i = 1; i < 2/* tree.Count*/; i++)
        {
            // add A
            // SplineNode current = tree[i];
            // SplineNode previous = tree[i - 1];

            Vector3 connection = new Vector3(-1f, -1f, 0f);
            Vector3 direction = connection.normalized;

            float angleX = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(0f, direction.y, direction.z)));
            if (direction.x < 0)
                angleX = 360 - angleX;

            float angleZ = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, new Vector3(direction.x, direction.y, 0f)));
            if (direction.x < 0)
                angleZ = 360 - angleZ;

            result += "Ax" + angleX.ToString() + "z" + angleZ.ToString();

            result += "L" + direction.magnitude.ToString();
            if (true) // here check if the node has more nodes to check weather to save it
            {
                result += ";" + i.ToString(); // lazy indexer however is not always the correct one
            }
            else
            {
                result += ":" + i.ToString(); // lazy indexer however is not always the correct one
            }
        }

        return true;
    }

}

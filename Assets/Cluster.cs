using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    SplineNode root;
    int rootIdx;
    string Lsys;
    List<SplineNode> endNodes;
    // some sort of size constaint
    // this is the current mehtod
    Vector3 bounds = new Vector3();

    public Cluster(SplineNode _rootNode, int _rootIdx, List<SplineNode> _endNodes)
    {
        root = _rootNode;
        endNodes = _endNodes;
        rootIdx = _rootIdx;

        bounds = CreateBounds();
        // the Lsys must be created first by using a not implemented function of 
        // Bez2Lsys and then this must be run through the translator
    }

    private Vector3 CreateBounds()
    {
        float maxX, maxY, maxZ;
        Vector3 tmpBounds = root.point;
        maxX = Mathf.Abs(tmpBounds.x);
        maxY = Mathf.Abs(tmpBounds.y);
        maxZ = Mathf.Abs(tmpBounds.z);
        for (int i = 0; i < endNodes.Count; i++)
        {
            Vector3 tmpPoint = endNodes[i].point;
            float tmpMaxX, tmpMaxY, tmpMaxZ;
            tmpMaxX = Mathf.Abs(tmpPoint.x);
            tmpMaxY = Mathf.Abs(tmpPoint.y);
            tmpMaxZ = Mathf.Abs(tmpPoint.z);
            if(tmpMaxX > maxX)
            {
                maxX = tmpMaxX;
                tmpBounds.x = tmpPoint.x;
            }
            if (tmpMaxY > maxY)
            {
                maxY = tmpMaxY;
                tmpBounds.y = tmpPoint.y;
            }
            if (tmpMaxZ > maxZ)
            {
                maxZ = tmpMaxZ;
                tmpBounds.z = tmpPoint.z;
            }
        }
        return tmpBounds;
    }

    /// <summary>
    /// apply the constaints for the used area her 
    /// TODO i do not know if this is the best way of doing this but it is a start
    /// </summary>
    public void constrain()
    {
        Vector3 newBounds = CreateBounds();
        float scalerX = bounds.x / newBounds.x;
        float scalerY = bounds.y / newBounds.y;
        float scalerZ = bounds.z / newBounds.z;
        root.scalePoint(endNodes, new Vector3(scalerX, scalerY, scalerZ));
    }


    /// <summary>
    /// update the cluster
    /// 
    /// 
    /// </summary>
    /// <param name="newTree"></param>
    public void connect(List<SplineNode> newTree)
    {
        root.DeleteSplineTree(endNodes);
        root.CopyValues(newTree[0]);
        // find a way to connect the end nodes again
        // and god damn work tomorrow

        // get all new endpoints
        List<SplineNode> newEnds;

        for (int i = 1; i < newTree.Count; i++)
        {
            
        }
        for(int i = 0; i < endNodes.Count; i++)
        {

        }

    }

    private SplineNode[] ExtractEnds(SplineNode node, int extractionStartIdx = 0)
    {
        int nextCount = node.GetNumberOfConnections();
        int numberOfEnds = nextCount - extractionStartIdx;
        SplineNode[] result = new SplineNode[numberOfEnds];
        for (int i = 0; i < numberOfEnds; i++)
        {
            int extractionIdx = i + extractionStartIdx;
            result[i] = node.GetLastSplineNode(extractionIdx);
        }
        return result;
    }
}



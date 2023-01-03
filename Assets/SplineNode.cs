using System.Collections.Generic;
using UnityEngine;
public class SplineNode
{
    private List<SplineNode> next;
    public int branchIdx = 0;
    public int numberOfBranchesAhead = 0;
    public Vector3 point = Vector3.zero;
    private float vigor = 0;
    private float vigorCost = 5;

    public SplineNode()
    {
        next = new List<SplineNode>();
        point = Vector3.zero;
    }
    public SplineNode(int idx)
    {
        next = new List<SplineNode>();
        point = Vector3.zero;
        branchIdx = idx;
    }
    public SplineNode(Vector3 position)
    {
        next = new List<SplineNode>();
        point = position;
    }
    public SplineNode(Vector3 position, int bidx)
    {
        next = new List<SplineNode>();
        point = position;
        branchIdx = bidx;
    }

    public void LogAll(string prefix = "", int idx = 0)
    {
        string perfix = idx == 0 ? "__root__: " : "__" + idx.ToString() + "__: ";
        Debug.Log(prefix + perfix + point);
        for (int i = 0; i < next.Count; i++)
        {
            next[i].LogAll(prefix, idx + 1);
        }
    }
    public SplineNode GetNextNode(int idx = 0)
    {
        if (next.Count <= idx)
            return null;
        else return next[idx];
    }

    public int GetBranchSize(int idx = 0)
    {
        return next.Count <= idx ? 1 : 1 + next[idx].GetBranchSize();
    }


    public int GetNumberOfConnections()
    {
        return next.Count;
    }

    public void SetNext(SplineNode node, int idx = -1)
    {
        if (idx < 0 && idx < next.Count)
            next.Add(node);
        else
            next.Insert(idx, node);
        node.numberOfBranchesAhead = numberOfBranchesAhead;

    }

    public void SetNext(SplineNode[] stemp)
    {
        SplineNode current = this;
        for (int i = 0; i < stemp.Length; i++)
        {
            current.SetNext(stemp[i]);
            stemp[i].numberOfBranchesAhead = current.numberOfBranchesAhead;
            current = stemp[i];
        }
    }

    public void ClearNext()
    {
        next = new List<SplineNode>();
    }

    public SplineNode GetSplineNode(int idx, int branch = 0)
    {
        if (idx == 0) return this;
        idx--;
        if (next.Count <= branch) return null;
        return next[branch].GetSplineNode(idx, 0);
    }

    public SplineNode GetLastSplineNode()
    {
        if (next.Count == 0) return this;
        return next[0].GetLastSplineNode();
    }

    public void DeleteSplineTree()
    {
        for (int i = 0; i < next.Count; i++)
        {
            next[i].DeleteSplineTree();
        }
        next = new List<SplineNode>();
    }

    public bool CheckForEndNode()
    {
        return next.Count == 0;
    }

    public int GetNumbersOfBranchesAhead()
    {
        int result = next.Count > 1 ? 1 : 0;
        for (int i = 0; i < next.Count; i++)
        {
            result += next[i].GetNumbersOfBranchesAhead();
        }
        return result;
    }

    public int GetNumberOfControllPointsAhead(int n = 1)
    {
        if (n != 1) n += 1;
        int result = n % 3 == 0 ? 1 : 0;
        for (int i = 0; i < next.Count; i++)
        {
            result += next[i].GetNumberOfControllPointsAhead(n + 1);
        }
        return result;
    }

    public int GetIndexInBranch(SplineNode node, int currentindex = 0)
    {
        if (node == this) return currentindex;
        for (int i = 0; i < next.Count; i++)
        {
            currentindex = next[i].GetIndexInBranch(node, currentindex + 1);
        }
        return currentindex;
    }

    public float GetVigor()
    {
        return vigor;
    }

    public void SetVigorCost(int vigorCost)
    {
        vigor = vigorCost;
    }

    public void RecalculateVigor(float _vigor, SplineNode prevNode = null, float maxcost = 1, float maxvigor = 1)
    {
        float _maxcost = maxcost;
        float _maxvigor = maxvigor;
        if (prevNode == null)
        {
            vigor = _vigor;
            _maxcost = CalculateVigorCost();
            _maxvigor = _vigor;
        }
        else
        {
            float cost = 0;
            vigor = _vigor - (vigorCost / _maxcost * _maxvigor);
            for (int i = 1; i < prevNode.GetNumberOfConnections(); i++)
            {
                float _cost = prevNode.GetNextNode(i).CalculateVigorCost();
                prevNode.GetNextNode(i).RecalculateVigor(_cost, maxcost: _maxcost, maxvigor: _maxvigor);
                cost += _cost;
            }
            vigor = Mathf.Max(1, vigor - cost);
        }
        if (next.Count > 0)
            next[0].RecalculateVigor(vigor, this, _maxcost, _maxvigor);
    }

    public float CalculateVigorCost()
    {
        float res = vigorCost;
        for (int i = 0; i < GetNumberOfConnections(); i++)
        {
            res += next[i].CalculateVigorCost();
        }
        return res;
    }
}
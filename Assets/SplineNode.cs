using System.Collections.Generic;
using UnityEngine;
public class SplineNode
{
	private List<SplineNode> next;
	public Vector3 point = Vector3.zero;

	public SplineNode()
	{
		next = new List<SplineNode>();
		point = Vector3.zero;
	}
	public SplineNode(Vector3 position)
	{
		next = new List<SplineNode>();
		point = position;
	}

	public int GetBranchSize(int idx = 0)
	{
		return next.Count <= idx ? 1 : 1 + next[idx].GetBranchSize();
	}

	public int GetNumberOfConnections()
    {
		return next.Count;
    }

	public void SetNext(SplineNode node)
	{
		next.Add(node);
	}

	public void SetNext(SplineNode[] stemp)
	{
		SplineNode current = this;
		for(int i = 0; i < stemp.Length; i++)
        {
			current.SetNext(stemp[i]);
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
		for(int i = 0; i < next.Count; i++)
        {
			next[i].DeleteSplineTree();
        }
		next = new List<SplineNode>();
	}

	public bool CheckForEndNode()
    {
		return next.Count == 0;
    }

}
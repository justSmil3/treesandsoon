using UnityEngine;
using System;
using System.Collections.Generic;


public class SplineTree : MonoBehaviour
{
	[SerializeField]
	public List<SplineNode> branches = new List<SplineNode>();
	public List<int> branchStarter = new List<int>();
	[SerializeField]
	public List<BezierControlPointMode[]> modes = new List<BezierControlPointMode[]>();


	public Vector3 GetControlPoint(int branchIndex, int startIndex, int index)
	{
		return branches[branchIndex].GetSplineNode(index, startIndex).point;
    }

    public void SetControlPoint(int branchIndex, int index, Vector3 point)
	{
		SplineNode node = branches[branchIndex];
		int branchStartIdx = branchStarter[branchIndex];
		if (index % 3 == 0)
		{
			Vector3 delta = point - node.GetSplineNode(index, branchStartIdx).point;
			if (index > 0)
			{
				node.GetSplineNode(index - 1, branchStartIdx).point += delta;
			}
			if (index + 1 < node.GetBranchSize(branchStartIdx))
			{
				node.GetSplineNode(index + 1, branchStartIdx).point += delta;
			}
		}
		node.GetSplineNode(index, branchStartIdx).point = point;
		EnforceMode(branchIndex, index);
	}

	public void Reset()
	{
		if(branches.Count > 0)
        {
			branches[0].DeleteSplineTree();
        }
		branches = new List<SplineNode>();
		branchStarter = new List<int>();
		SplineNode n0 = new SplineNode(new Vector3(0.0f, 0.0f, 0.0f));
		SplineNode n1 = new SplineNode(new Vector3(0.0f, 0.1f, 0.0f));
		SplineNode n2 = new SplineNode(new Vector3(0.0f, 0.9f, 0.0f));
		SplineNode n3 = new SplineNode(new Vector3(0.0f, 1.0f, 0.0f));
		SplineNode[] branch = new SplineNode[] { n1, n2, n3 };
		branches.Add(n0);
		branchStarter.Add(0);
		n0.SetNext(branch);
		modes = new List<BezierControlPointMode[]>();
		modes.Add(new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		});
	}

	public BezierControlPointMode GetControlPointMode(int branchIndex, int index)
	{
		return modes[branchIndex][(index + 1) / 3];
	}

	public void SetControlPointMode(int branchIndex, int index, BezierControlPointMode mode)
	{
		int modeIndex = (index + 1) / 3;
		modes[branchIndex][modeIndex] = mode;
		EnforceMode(branchIndex, index);
	}
	public Vector3 GetPoint(int branchIndex, int idx, float t)
	{
		int i = idx / 3 * 3;
		int branchstartindex = branchStarter[branchIndex];
		t = Mathf.Clamp01(t);
		return transform.TransformPoint(Bezier.GetPoint(
			branches[branchIndex].GetSplineNode(i, branchstartindex).point, 
			branches[branchIndex].GetSplineNode(i + 1, branchstartindex).point, 
			branches[branchIndex].GetSplineNode(i + 2, branchstartindex).point,
			branches[branchIndex].GetSplineNode(i + 3, branchstartindex).point, 
			t));
	}

	public Vector3 GetVelocity(int branchIndex, int idx, float t)
	{
		int i = idx / 3 * 3;
		t = Mathf.Clamp01(t);
		int branchstartindex = branchStarter[branchIndex];
		return transform.TransformPoint(Bezier.GetFirstDerivative(
			branches[branchIndex].GetSplineNode(i, branchstartindex).point,
			branches[branchIndex].GetSplineNode(i + 1, branchstartindex).point,
			branches[branchIndex].GetSplineNode(i + 2, branchstartindex).point,
			branches[branchIndex].GetSplineNode(i + 3, branchstartindex).point,
			t)) - transform.position;
	}

	public Vector3 GetDirection(int branchIndex, int idx, float t)
	{
		return GetVelocity(branchIndex, idx, t).normalized;
	}

	public int InsertCurve(int branchIndex, int idx, float t)
	{
		int insertIdx = (idx / 3 + 1) * 3 - 1;
		float pointPosChange = .05f;
		Vector3 velo = GetDirection(branchIndex, idx, t);
		Vector3 p = GetPoint(branchIndex, idx, t);
		SplineNode[] stemp = { new SplineNode(),
							   new SplineNode(),
							   new SplineNode()};
		stemp[0].point = p - (pointPosChange * velo);
		stemp[1].point = p;
		stemp[2].point = p + (pointPosChange * velo);

		SplineNode branch = branches[branchIndex]; 
		int branchStartIdx = branchStarter[branchIndex];
		SplineNode tmp = branch.GetSplineNode(insertIdx, branchStartIdx);
		SplineNode preNode = branch.GetSplineNode(insertIdx - 1, branchStartIdx);
		preNode.ClearNext();
		preNode.SetNext(stemp);
		stemp[2].SetNext(tmp);

		BezierControlPointMode[] modeArray = modes[branchIndex];
		Array.Resize(ref modeArray, modeArray.Length + 1);

		modeArray[modeArray.Length - 1] = modeArray[modeArray.Length - 2];
		modeArray[modeArray.Length - 2] = BezierControlPointMode.Mirrored;
		modes[branchIndex] = modeArray;
		EnforceMode(branchIndex, branch.GetBranchSize(branchStartIdx) - 4);
		return insertIdx + 1;
	}

	public void AddStemp(int branchIndex, int idx, float t, Ray mouseRay) 
    {
		// maybe for that somewhen do uncontrollable points on the curve i guess
		// but definitly after the pre alpha 

		SplineNode branch = branches[branchIndex];
		int connectionPointIndex = InsertCurve(branchIndex, idx, t);
		SplineNode connectionNode = branch.GetSplineNode(connectionPointIndex, branchStarter[branchIndex]);
		Vector3 p = Vector3.zero;
		Plane plane = new Plane(UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward, connectionNode.point);
		float dist2obj;
		if (plane.Raycast(mouseRay, out dist2obj))
		{
			p = mouseRay.GetPoint(dist2obj) - connectionNode.point;
		}

		SplineNode[] stemp = new SplineNode[]
		{
			new SplineNode(connectionNode.point + p * 0.1f),
            new SplineNode(connectionNode.point + p * 0.9f),
            new SplineNode(connectionNode.point + p)
        };
        branchStarter.Add(connectionNode.GetNumberOfConnections());
        connectionNode.SetNext(stemp);
        branches.Add(connectionNode);

		modes.Add(new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		});
		EnforceMode(branches.Count - 1, 0);
	}

	public void AddCurve(int branchIndex)
	{
		SplineNode branch = branches[branchIndex];
		int branchStartIdx = branchStarter[branchIndex];
		SplineNode end = branch.GetLastSplineNode();
		Vector3 point = end.point;

		Vector3 dir = GetDirection(branchIndex, branch.GetBranchSize(branchStartIdx) - 2, 0);

		SplineNode[] next = new SplineNode[]
		{
			new SplineNode(point + dir * .05f),
			new SplineNode(point + dir * .1f),
			new SplineNode(point + dir * .15f)
		};

		end.SetNext(next);

		BezierControlPointMode[] modeArray = modes[branchIndex]; 
		Array.Resize(ref modeArray, modeArray.Length + 1);
		modeArray[modeArray.Length - 1] = modeArray[modeArray.Length - 2];
		modeArray[modeArray.Length - 2] = BezierControlPointMode.Mirrored;
		modes[branchIndex] = modeArray;
		EnforceMode(branchIndex, branch.GetBranchSize(branchStartIdx) - 4);
	}

	private void EnforceMode(int branchIndex, int index)
	{
		BezierControlPointMode[] modeArray = modes[branchIndex];
		SplineNode branch = branches[branchIndex];
		int branchStartIdx = branchStarter[branchIndex];
		int branchSize = branch.GetBranchSize(branchStartIdx);

		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modeArray[modeIndex];
		if (mode == BezierControlPointMode.Free || (modeIndex == 0 || modeIndex == modeArray.Length - 1))
		{
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex)
		{
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0)
			{
				fixedIndex = branchSize - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= branchSize)
			{
				enforcedIndex = 1;
			}
		}
		else
		{
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= branchSize)
			{
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0)
			{
				enforcedIndex = branchSize - 2;
			}
		}

		Vector3 middle = branch.GetSplineNode(middleIndex, branchStartIdx).point;
		Vector3 enforcedTangent = middle - branch.GetSplineNode(fixedIndex, branchStartIdx).point;
		if (mode == BezierControlPointMode.Aligned)
		{
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, branch.GetSplineNode(enforcedIndex, branchStartIdx).point);
		}
		branch.GetSplineNode(enforcedIndex, branchStartIdx).point = middle + enforcedTangent;
	}
}

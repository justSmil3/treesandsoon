using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class SplineTreeUnit
{
	public List<SplineNode> branches;
	public List<int> branchStarter;
	public List<BezierControlPointMode[]> modes;

    public SplineTreeUnit()
    {
		branches = new List<SplineNode>();
		branchStarter = new List<int>();
		modes = new List<BezierControlPointMode[]>();
	}
}

public static class Lsys2Bez
{
	//public static SplineTreeUnit Convert(string Lsys)
	//   {
	//	SplineTreeUnit tree = new SplineTreeUnit();
	//	SplineNode prevNode = null;
	//	Stack<SplineNode> StackedNode = new Stack<SplineNode>();
	//	Vector3 dir = Vector3.up;
	//	Vector3 point = new Vector3(-2, 0, 0);

	//	// format: ;0Ax00z00L00
	//	Debug.Log(Lsys);
	//	string[] branches = Lsys.Split(']');
	//	for(int i = 0; i < branches.Length-1; i++)
	//       {
	//		string branch = branches[i];
	//		for(int j = 0; j < branch.Length; j++)
	//           {
	//			char letter = branch[j];
	//			switch (letter)
	//			{
	//				case ';':
	//					SplineNode newNode1 = new SplineNode(point, i);
	//					tree.branches.Add(newNode1);
	//					if (i == 0 && j == 0)
	//						tree.branchStarter.Add(0);
	//					else
	//						tree.branchStarter.Add(1);
	//					// from here on it is not in the flow shart
	//					if (prevNode != null)
	//						prevNode.SetNext(newNode1);
	//					prevNode = newNode1;
	//					StackedNode.Push(newNode1);
	//					break;
	//				case ':':
	//					SplineNode newNode2 = new SplineNode(point, i);
	//					if (prevNode != null)
	//						prevNode.SetNext(newNode2);
	//					prevNode = newNode2;
	//					break;
	//				case 'A':
	//					// TODO think about putting redundancy checks here 
	//					int k;
	//					float rotX = ExtractFloat(j + 2, branch, 'z', out k);
	//					// rotate the first way
	//					dir = Quaternion.Euler(rotX, 0, 0) * dir;
	//					float rotZ = ExtractFloat(k + 1, branch, 'L', out k);
	//					// rotate the second way
	//					dir = Quaternion.Euler(0, 0, rotZ) * dir;
	//					float length = ExtractFloat(k + 1, branch, new char[] { ':', ';' }, out k);
	//					point += (dir.normalized * length);
	//					dir = Vector3.up;
	//					// TODO think about leaving the rotation as is but that has to also be 
	//					// accounted for in the bez2lsys file.
	//					j = k-1;
	//					break;
	//				default:
	//					break;
	//               }
	//		}
	//		prevNode = StackedNode.Pop();
	//	}

	//	return tree;
	//   }

	public static SplineTreeUnit Convert(string Lsys)
	{
		SplineTreeUnit tree = new SplineTreeUnit();
		SplineNode prevNode = null;
		Stack<SplineNode> StackedNode = new Stack<SplineNode>();
		Stack<int> idxStack = new Stack<int>();
		Vector3 dir = Vector3.up;
		Vector3 point = new Vector3(-2, 0, 0);
		int idx = 0;

		// format: ...;...Ax00z00L00:]
		for (int i = 0; i < Lsys.Length - 1; i++)
		{
			char letter = Lsys[i];
			switch (letter)
			{
				case ':':
					SplineNode newNode = new SplineNode(point, i);
					if (prevNode != null)
						prevNode.SetNext(newNode, 0);
					prevNode = newNode;
					break;
				case ';':
					if (prevNode == null)
					{
						prevNode = new SplineNode(point, i);
					}
					if (!tree.branches.Contains(prevNode))
					{
						tree.branches.Add(prevNode);
						// TODO this one is currently not implimented it has to do with the thickness
						// throuvh the branchidx
						
						tree.branchStarter.Add(Mathf.Min(i, 1));
						tree.modes.Add(new BezierControlPointMode[] {
							BezierControlPointMode.Free,
							BezierControlPointMode.Free
						});
					}
					// from here on it is not in the flow shart
					StackedNode.Push(prevNode);
					idxStack.Push(idx);
					idx = tree.modes.Count - 1;
					break;
				case 'A':
					// TODO think about putting redundancy checks here 
					int k;
					float rotX = ExtractFloat(i + 1, Lsys, 'q', out k);
					float rotY = ExtractFloat(k + 1, Lsys, 'q', out k);
					float rotZ = ExtractFloat(k + 1, Lsys, 'q', out k);
					float rotW = ExtractFloat(k + 1, Lsys, 'L', out k);
					// rotate the second way
					dir = new Quaternion(rotX, rotY, rotZ, rotW) * dir;
					float length = ExtractFloat(k + 1, Lsys, new char[] { ':', ';' }, out k);
					point += (dir.normalized * length);
					dir = Vector3.up;
					// TODO think about leaving the rotation as is but that has to also be 
					// accounted for in the bez2lsys file.
					i = k - 1;
					break;
				case 'M':
					BezierControlPointMode[] modeArray = tree.modes[idx];
					Array.Resize(ref modeArray, modeArray.Length + 1);

					modeArray[modeArray.Length - 1] = modeArray[modeArray.Length - 2];
					modeArray[modeArray.Length - 2] = BezierControlPointMode.Mirrored;
					tree.modes[idx] = modeArray;
					break;
				case ']':
					prevNode = StackedNode.Pop();
					idx = idxStack.Pop();
					point = prevNode.point;
					break;
				default:
					break;
			}
		}

		return tree;
	}

	private static float ExtractFloat(int start, string s, char[] breakpoint, out int k)
    {
		string result = "";
		float fres;
		int i = start;
		for(;  i < s.Length; i++)
        {
			foreach(char bp in breakpoint)
			if(s[i] == bp)
            {
					fres = float.Parse(result, CultureInfo.InvariantCulture.NumberFormat);
					k = i;
					return fres;
			}
			result += s[i];
        }
		fres = float.Parse(result, CultureInfo.InvariantCulture.NumberFormat);
		k = i;
		return fres;
	}

	private static float ExtractFloat(int start, string s, char breakpoint, out int k)
	{
		string result = "";
		float fres;
		int i = start;
		for (; i < s.Length; i++)
		{
			if (s[i] == breakpoint)
			{
				fres = float.Parse(result, CultureInfo.InvariantCulture.NumberFormat);
				k = i;
				return fres;
			}
			result += s[i];
		}

		fres = float.Parse(result, CultureInfo.InvariantCulture.NumberFormat);
		k = i;
		return fres;
	}
}

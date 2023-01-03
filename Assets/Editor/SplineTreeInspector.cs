using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineTree))]
public class SplineTreeInspector : Editor
{
	float threshhold = 10f;
	private float selectionthreshhold = 2f;
	private float selectionThreshhold = 250.0f;
	private SplineTree spline;
	private Transform handleTransform;
	private Quaternion handleRotation;

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private int selectedIndex = -1;
	private int selectedBranch = -1;

	private bool editable = false;
	private static Color[] modeColors = {
		Color.white, // Free
		Color.yellow, // Aligned
		Color.cyan // Mirrowed
	};

	struct SelectionData
    {
		public float t, dist;
		public int idx, branchIdx;
		public SelectionData(float _t, int _branchIdx, int _idx, float _dist)
        {
			t = _t;
			idx = _idx;
			dist = _dist;
			branchIdx = _branchIdx;
        }
    }

	public override void OnInspectorGUI()
	{
		spline = target as SplineTree;
		EditorGUI.BeginChangeCheck();
		//for (int i = 0; i < spline.branches.Count; i++)
		//{
		//	if (selectedIndex >= 0 && selectedIndex < spline.branches[i].GetBranchSize(spline.branchStarter[i])) // controllpointcount
		//		if (selectedBranch >= 0 && selectedBranch < spline.branches.Count) // controllpointcount
		//		{
		//			DrawSelectedPointInspector(i, spline.branchStarter[i]);
		//		}
		//}
		// TODO make an int filed to spcify where to add to
		if (GUILayout.Button("Add Curve"))
		{
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve(0);
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button("Convert"))
		{
			spline.ConvertToLSystem();
		}
		if (GUILayout.Button("Convert back"))
		{
			spline.ConvertToBezierTree();
		}
		if (GUILayout.Button("Reset"))
		{
			spline.Reset();
		}
        if (GUILayout.Button("Debug"))
        {
			spline.CalculateVigor();
        }
    }

	private void DrawSelectedPointInspector(int branchIndex, int bsi)
	{
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(branchIndex, bsi, selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(branchIndex, selectedIndex, point);
		}
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(branchIndex, selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(branchIndex, selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
	}
	private void OnSceneGUI()
	{
		spline = target as SplineTree;


		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		// TODO this is not optimized for branches 
		for (int j = 0; j < spline.branches.Count; j++)
		{
			int startIndex = spline.branchStarter[j];
			Vector3 p0 = ShowPoint(j, startIndex, 0);
			for (int i = 1; i < spline.branches[j].GetBranchSize(startIndex); i += 3)
			{
				Vector3 p1 = ShowPoint(j, startIndex, i);
				Vector3 p2 = ShowPoint(j, startIndex, i + 1);
				Vector3 p3 = ShowPoint(j, startIndex, i + 2);

				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);

				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}
		}

		// TODO make it work with all branches 
		Event e = Event.current;

		if (e.control)
		switch (e.type)
		{
			case EventType.MouseDown:
					SelectionData select = TryCreateSplineSection(e);
					if (e.button == 2)
					{
                        if (select.dist < threshhold)
                        {
                            spline.InsertCurve(select.branchIdx , select.idx, select.t);
							e.Use();
						}
					}
					else if (e.button == 0)
                    {
						if (select.idx >= 0)
						{
							spline.AddStemp(select.branchIdx, select.idx, select.t, HandleUtility.GUIPointToWorldRay(e.mousePosition));
							e.Use();
						}
					}						
				break;
			default:
				break;

			case EventType.KeyDown:
				break;
			case EventType.KeyUp:
				break;
		}
	}
	private Vector3 ShowPoint(int branchIndex, int startIndex, int index)
	{
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(branchIndex, startIndex, index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0)
		{
			size *= 2f;
		}
		Handles.color = modeColors[(int)spline.GetControlPointMode(branchIndex, index)]; 
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
		{
			selectedIndex = index;
			selectedBranch = branchIndex;
			Repaint();
		}
		if (selectedIndex == index && branchIndex == selectedBranch)
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(branchIndex, index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}

	// TODO make it work with all branches
	private SelectionData TryCreateSplineSection(Event e)
	{
		float dist = Mathf.Infinity;
		int idx = -1;
		int numSteps = 40;
		int bestBranch = -1;

		float t = 0.0f;

		for (int currentBranch = 0; currentBranch < spline.branches.Count; currentBranch++)
		{
			SplineNode branch = spline.branches[currentBranch];
			int branchStartIdx = spline.branchStarter[currentBranch];

			int cpc = branch.GetBranchSize(branchStartIdx);

			// TODO the minus one there is a really janky solution
			// this wont work in a branch
			for (int i = 0; i < cpc - 1; i+=3)
			{
				Vector3 point = branch.GetSplineNode(i, branchStartIdx).point;
				Vector2 pointPos = HandleUtility.WorldToGUIPoint(point);
				float tmpDist = Vector2.Distance(e.mousePosition, pointPos);
				if (tmpDist > selectionThreshhold) continue;
				Vector3 velocity = spline.GetDirection(currentBranch, i, 0);
				velocity = point + velocity;
				Vector2 velocity2D = HandleUtility.WorldToGUIPoint(velocity);
				float angle = Vector2.Angle(e.mousePosition - pointPos, velocity2D - pointPos);
				if (/*tmpDist < dist && */angle < 90)
				{
					//dist = tmpDist;
					//bestBranch = currentBranch;
					//idx = i;

					for (int k = 0; k < numSteps; k++)
					{
						float stepT = k / (float)numSteps;
						Vector3 stepPoint = spline.GetPoint(currentBranch, i, stepT);
						Vector2 stepPointPos = HandleUtility.WorldToGUIPoint(stepPoint);
						tmpDist = Vector2.Distance(e.mousePosition, stepPointPos);
						if (tmpDist < dist)
						{
							bestBranch = currentBranch;
							idx = i;
							dist = tmpDist;
							t = stepT;
						}
					}
				}

			}
		}
		//for (int i = 0; i < numSteps; i++)
		//{
		//	float stepT = i / (float)numSteps;
		//	Vector3 stepPoint = spline.GetPoint(bestBranch, idx, stepT);
		//	Vector2 stepPointPos = HandleUtility.WorldToGUIPoint(stepPoint);
		//	float tmpDist = Vector2.Distance(e.mousePosition, stepPointPos);
		//	if (tmpDist < dist)
		//	{
		//		dist = tmpDist;
		//		t = stepT;
		//	}
		//}

		return new SelectionData(t, bestBranch, idx, dist);
		//float threshhold = 10000f;
		//if (dist < threshhold)
		//{
		//	spline.InsertCurve(t, idx);
		//	return true;
		//}
		//return false;
	}

}
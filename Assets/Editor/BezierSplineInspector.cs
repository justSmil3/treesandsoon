using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{

	private BezierSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private int selectedIndex = -1;

	private static Color[] modeColors = {
		Color.white, // Free
		Color.yellow, // Aligned
		Color.cyan // Mirrowed
	};
	public override void OnInspectorGUI()
	{
		spline = target as BezierSpline;
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
		{
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Curve"))
		{
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
	}

	private void DrawSelectedPointInspector()
	{
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
	}
	private void OnSceneGUI()
	{
		spline = target as BezierSpline;


		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		Vector3 p0 = ShowPoint(0);
		for (int i = 1; i < spline.ControlPointCount; i += 3)
		{
			Vector3 p1 = ShowPoint(i);
			Vector3 p2 = ShowPoint(i + 1);
			Vector3 p3 = ShowPoint(i + 2);

			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);

			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}

		Event e = Event.current;
		switch (e.type)
		{
			case EventType.MouseDown:
				if (e.button == 2)
				{
					if (TryCreateSplineSection(e))
						e.Use();
				}
				break;
			default:
				break;
        }
	}
	private Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0)
		{
			size *= 2f;
		}
		Handles.color = modeColors[(int)spline.GetControlPointMode(index)]; ;
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
		{
			selectedIndex = index;
			Repaint();
		}
		if (selectedIndex == index)
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}

	private bool TryCreateSplineSection(Event e)
    {
		float dist = Mathf.Infinity;
		int idx = -1;
		int cpc = spline.ControlPointCount;
		int numSteps = 20;
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
		for (int i = 0; i < spline.ControlPointCount; i++)
		{
			Vector3 point = spline.GetPoint(i / (float)(cpc - 1));
			Vector2 pointPos = HandleUtility.WorldToGUIPoint(point);
			float tmpDist = Vector2.Distance(e.mousePosition, pointPos);
			Vector3 velocity = spline.GetVelocity(i / (float)spline.ControlPointCount);
			velocity = point + velocity;
			Vector2 velocity2D = HandleUtility.WorldToGUIPoint(velocity);
			float angle = Vector2.Angle(e.mousePosition - pointPos, velocity2D - pointPos);
			if (tmpDist < dist && angle < 90)
			{
				dist = tmpDist;
				idx = i;
			}
		}
		Vector3 point2 = spline.GetPoint((idx + 1) / (float)(cpc - 1));
		Vector2 point2Pos = HandleUtility.WorldToGUIPoint(point2);

		float t = idx / (float)(cpc - 1);
		for (int i = 0; i < numSteps; i++)
		{
			float stepT = idx + i / (float)numSteps;
			stepT = stepT / (float)(cpc - 1);
			Vector3 stepPoint = spline.GetPoint(stepT);
			Vector2 stepPointPos = HandleUtility.WorldToGUIPoint(stepPoint);
			float tmpDist = Vector2.Distance(e.mousePosition, stepPointPos);
			if (tmpDist < dist)
			{
				dist = tmpDist;
				t = stepT;
			}
		}

		float threshhold = 10f;
		if (dist < threshhold)
		{
			spline.InsertCurve(t, idx);
			return true;
		}
		return false;
	}

}
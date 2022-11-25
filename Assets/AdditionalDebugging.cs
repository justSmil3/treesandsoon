using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AdditionalDebugging
{
	public static void DrawPlane(Vector3 position, Vector3 normal)
	{
		Vector3 v3;
		if (normal.normalized != Vector3.forward)
			v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
		else
			v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;
		var corner0 = position + v3;
		var corner2 = position - v3;
		var q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		var corner1 = position + v3;
		var corner3 = position - v3;
		Debug.DrawLine(corner0, corner2, Color.green, 20);
		Debug.DrawLine(corner1, corner3, Color.green, 20);
		Debug.DrawLine(corner0, corner1, Color.green, 20);
		Debug.DrawLine(corner1, corner2, Color.green, 20);
		Debug.DrawLine(corner2, corner3, Color.green, 20);
		Debug.DrawLine(corner3, corner0, Color.green, 20);
		Debug.DrawRay(position, normal, Color.red, 20);
	}

}

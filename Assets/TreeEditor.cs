using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeEditor : MonoBehaviour
{
    private List<Vector3> points = new List<Vector3>();

    private void Awake()
    {
        points.Add(Vector3.zero);
    }
}

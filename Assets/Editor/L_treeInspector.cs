using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(L_tree))]
public class L_treeInspector : Editor
{
    // slider values for the amount of procedural influence 
    float randomnessSliderValue = 10;
    float minRandomnessSliderValue = 0;
    float maxRandomnessSliderValue = 100;

    // L_object 
    L_tree tree;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tree = target as L_tree;
        EditorGUI.BeginChangeCheck();
        randomnessSliderValue = 
            GUILayout.HorizontalSlider(randomnessSliderValue,
                                       minRandomnessSliderValue,
                                       maxRandomnessSliderValue);
    }
}

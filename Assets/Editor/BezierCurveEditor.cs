/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    private readonly int[] _indices = {2, 0, 2, 1};
    private readonly Vector3[] _points = new Vector3[3];

    private void OnSceneGUI()
    {
        var curve = target as BezierCurve;

        EditorGUI.BeginChangeCheck();

        foreach (var segment in curve.segments)
        {
            segment.startPosition = Handles.PositionHandle(segment.startPosition, Quaternion.identity);
            segment.handlePosition = Handles.PositionHandle(segment.handlePosition, Quaternion.identity);
            segment.endPosition = Handles.PositionHandle(segment.endPosition, Quaternion.identity);

            Handles.DrawDottedLine(segment.startPosition, segment.handlePosition, 5f);
            Handles.DrawDottedLine(segment.endPosition, segment.handlePosition, 5f);
        }

        if (EditorGUI.EndChangeCheck()) curve.UpdatePoints();

        return;
        EditorGUI.BeginChangeCheck();
        _points[0] = Handles.PositionHandle(curve.startPosition, Quaternion.identity);

        _points[1] = Handles.PositionHandle(curve.endPosition, Quaternion.identity);

        _points[2] = Handles.PositionHandle(curve.handlePosition, Quaternion.identity);

        Handles.DrawDottedLines(_points, _indices, 5f);

        if (EditorGUI.EndChangeCheck())
        {
            curve.startPosition = _points[0];
            curve.endPosition = _points[1];
            curve.handlePosition = _points[2];

            curve.UpdatePoints();
        }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("segments"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfPoints"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("thickness"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meshFilter"));

        serializedObject.ApplyModifiedProperties();
    }
}*/
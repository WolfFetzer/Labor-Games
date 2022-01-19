/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Graph))]
public class GraphEditor : Editor
{
    public int nodeIndexOne, nodeIndexTwo;

    private void OnSceneGUI()
    {
        return;
        var graph = target as Graph;

        foreach (var node in graph.graph.nodes)
            node.position = Handles.PositionHandle(node.position, Quaternion.identity);
    }

    public override void OnInspectorGUI()
    {
        var t = (Graph) target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("graph"));

        serializedObject.ApplyModifiedProperties();

        nodeIndexOne = EditorGUILayout.IntSlider("Node A", nodeIndexOne, 0, t.graph.nodes.Count - 1);
        nodeIndexTwo = EditorGUILayout.IntSlider("Node B", nodeIndexTwo, 0, t.graph.nodes.Count - 1);
        if (GUILayout.Button("Add Connection"))
        {
            if (nodeIndexOne == nodeIndexTwo) return;

            t.graph.ConnectNodes(t.graph.nodes[nodeIndexOne], t.graph.nodes[nodeIndexTwo]);
        }

        if (GUILayout.Button("Randomize Cost")) t.graph.RandomizeCosts();
        if (GUILayout.Button("Calculate Path"))
            t.graph.CalculateShortestWay(t.graph.nodes[nodeIndexOne], t.graph.nodes[nodeIndexTwo]);
    }
}*/
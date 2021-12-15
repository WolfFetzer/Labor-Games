using System;
using System.Collections.Generic;
using Curves.Util;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class BezierSegment
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 handlePosition;
}

public class BezierCurve : MonoBehaviour
{
    public List<BezierSegment> segments;

    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 handlePosition;


    [Range(3, 50)] public int numberOfPoints = 5;

    public float thickness = 1f;

    public MeshFilter meshFilter;

    private BezierPoint[] _points;


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_points is null) return;

        foreach (var segment in segments)
        {
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(segment.startPosition, Vector3.up, 0.5f);
            Handles.DrawSolidDisc(segment.endPosition, Vector3.up, 0.5f);

            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(segment.handlePosition, Vector3.up, 0.5f);

            Handles.color = Color.magenta;
            for (var i = 0; i < _points.Length - 1; i++)
                Handles.DrawAAPolyLine(_points[i].Position, _points[i + 1].Position);
        }

        /*
        if(_points is null) return;
        
        Handles.color = Color.magenta;
        foreach (BezierPoint bezierPoint in _points)
        {
            Handles.DrawAAPolyLine(bezierPoint.Position);
        }
        */
    }
#endif


    public void UpdatePoints()
    {
        var stepAmount = 1f / (numberOfPoints - 1);
        _points = new BezierPoint[segments.Count * numberOfPoints];

        var index = 0;

        foreach (var segment in segments)
        {
            var pos1 = segment.startPosition;
            var handle = segment.handlePosition;
            var pos2 = segment.endPosition;

            for (var i = 0; i < numberOfPoints; i++)
                _points[index++] = new BezierPoint(pos1, handle, pos2, i * stepAmount);
        }

        CreateMesh();
    }

    #region Test

    private void Start()
    {
        startPosition = transform.position;
    }

    private void AddPoints(int index, ref Vector3[] vertices)
    {
        var a = _points[index].Position;
        var normal = _points[index].Normal2D;

        var start = index * 2;

        vertices[start] = new Vector3(a.x - thickness * normal.x, a.y, a.z - thickness * normal.y);
        vertices[start + 1] = new Vector3(a.x + thickness * normal.x, a.y, a.z + thickness * normal.y);
    }

    private void CreateMesh()
    {
        var mesh = new Mesh();

        var vertices = new Vector3[numberOfPoints * 2];
        var indices = new int[(numberOfPoints - 1) * 6];

        AddPoints(0, ref vertices);

        var start = 0;
        var startInd = 0;

        for (var i = 1; i < numberOfPoints; i++)
        {
            AddPoints(i, ref vertices);

            indices[startInd] = start;
            indices[startInd + 1] = start + 1;
            indices[startInd + 2] = start + 2;
            indices[startInd + 3] = start + 2;
            indices[startInd + 4] = start + 1;
            indices[startInd + 5] = start + 3;

            start += 2;
            startInd += 6;
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    #endregion
}
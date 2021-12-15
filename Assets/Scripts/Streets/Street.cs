using System.Collections.Generic;
using Curves.Util;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Street : MonoBehaviour
{
    [SerializeField] private int pointsPerSegment = 20;

    public GameObject spawnPrefab;
    private readonly List<Vector3> _handles = new List<Vector3>();


    private bool _isHandle;
    private readonly List<Vector3> _points = new List<Vector3>();

    private readonly List<Vector3> _streetPoints = new List<Vector3>();

    private readonly List<Vector3> _wayPoints = new List<Vector3>();


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.cyan;
        for (var i = 0; i < _points.Count - 1; i++) Handles.DrawAAPolyLine(_points[i], _points[i + 1]);

        Handles.color = Color.red;
        for (var i = 0; i < _streetPoints.Count - 1; i++)
            Handles.DrawAAPolyLine(_streetPoints[i], _streetPoints[i + 1]);

        foreach (var streetPoint in _streetPoints) Handles.DrawSolidDisc(streetPoint, Vector3.up, 0.25f);

        Handles.color = Color.yellow;
        foreach (var handle in _handles) Handles.DrawSolidDisc(handle, Vector3.up, 0.5f);

        Handles.color = Color.blue;
        foreach (var point in _points) Handles.DrawSolidDisc(point, Vector3.up, 0.5f);

        Handles.color = Color.green;
        foreach (var wayPoint in _wayPoints) Handles.DrawSolidDisc(wayPoint, Vector3.up, 0.25f);
    }
#endif

    public void AddPoint(Vector3 position)
    {
        if (_isHandle)
        {
            _handles.Add(position);
            _isHandle = false;
        }
        else
        {
            _points.Add(position);
            _isHandle = true;

            if (_points.Count > 1)
            {
                var verts = new Vector3[pointsPerSegment * 2];
                var tria = new int[(pointsPerSegment - 1) * 6];

                var step = 1 / (pointsPerSegment - 1f);
                var index = _points.Count - 1;

                for (var i = 0; i < pointsPerSegment; i++)
                {
                    var streetPoint =
                        MathUtil.QuadraticLerp(_points[index - 1], _handles[index - 1], position, i * step);
                    _streetPoints.Add(streetPoint);

                    var tangent = MathUtil.GetTangent(_points[index - 1], _handles[index - 1], position, i * step);
                    var normal = Vector2.Perpendicular(new Vector2(tangent.x, tangent.z));
                    var addition = new Vector3(normal.x, 0f, normal.y);


                    _wayPoints.Add(streetPoint + 0.5f * addition);
                    _wayPoints.Add(streetPoint - 0.5f * addition);


                    var start = i * 2;
                    verts[start] = streetPoint + addition;
                    verts[start + 1] = streetPoint - addition;

                    /*
                     *    4 #           # 3
                     *     /           /
                     * 2 #           # 1
                     *
                     */
                }

                var distance = 0f;
                var ind = _streetPoints.Count - pointsPerSegment;
                for (var i = 0; i < pointsPerSegment - 1; i++)
                    distance += Vector3.Distance(_streetPoints[ind++], _streetPoints[ind]);

                var pointDist = Vector3.Distance(_points[index - 1], position);

                var acc = MathUtil.GetAcceleration(
                    _points[index - 1], _handles[index - 1], position);

                Debug.Log(
                    "Acceleration: " + acc +
                    ", Distance: " + distance +
                    ", Difference: " + acc / distance +
                    ", PointDistance: " + pointDist);


                index = 0;
                for (var i = 0; i < pointsPerSegment - 1; i++)
                {
                    var start = i * 6;
                    tria[start] = index;
                    tria[start + 1] = index + 3;
                    tria[start + 2] = index + 1;
                    tria[start + 3] = index;
                    tria[start + 4] = index + 2;
                    tria[start + 5] = index + 3;

                    index += 2;
                }

                var mesh = new Mesh();
                mesh.vertices = verts;
                mesh.triangles = tria;

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                var go = Instantiate(spawnPrefab);

                var ms = go.GetComponent<MeshFilter>();
                ms.mesh = mesh;

                go.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }
    }
}